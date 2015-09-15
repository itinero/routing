// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Coordinates.Collections;

namespace OsmSharp.Routing.Graphs.Geometric
{
    /// <summary>
    /// Contains extensions for the geometric graph and edge.
    /// </summary>
    public static class GeometricExtensions
    {
        /// <summary>
        /// Projects a point onto an edge.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this GeometricGraph graph, GeometricEdge edge, float latitude, float longitude,
            out float projectedLatitude, out float projectedLongitude, out float projectedDistanceFromFirst)
        {
            int projectedShapeIndex;
            float distanceToProjected;
            return graph.ProjectOn(edge, latitude, longitude, out projectedLatitude, out projectedLongitude,
                out projectedDistanceFromFirst, out projectedShapeIndex, out distanceToProjected);
        }

        /// <summary>
        /// Projects a point onto an edge.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this GeometricGraph graph, GeometricEdge edge, float latitude, float longitude,
            out float projectedLatitude, out float projectedLongitude, out float projectedDistanceFromFirst,
            out int projectedShapeIndex, out float distanceToProjected)
        {
            float totalLength;
            return graph.ProjectOn(edge, latitude, longitude, out projectedLatitude, out projectedLongitude, out projectedDistanceFromFirst,
                out projectedShapeIndex, out distanceToProjected, out totalLength);
        }

        /// <summary>
        /// Projects a point onto an edge.
        /// </summary>
        /// <returns></returns>
        public static bool ProjectOn(this GeometricGraph graph, GeometricEdge edge, float latitude, float longitude,
            out float projectedLatitude, out float projectedLongitude, out float projectedDistanceFromFirst,
            out int projectedShapeIndex, out float distanceToProjected, out float totalLength)
        {
            distanceToProjected = float.MaxValue;
            projectedDistanceFromFirst = 0;
            projectedLatitude = float.MaxValue;
            projectedLongitude = float.MaxValue;
            projectedShapeIndex = -1;

            ICoordinate previous = graph.GetVertex(edge.From);
            var shape = edge.Shape;
            var previousShapeDistance = 0.0f;
            var previousShapeIndex = -1;

            var shapeEnumerator = shape.GetEnumerator();
            while (true)
            {
                // get current point.
                var isShapePoint = true;
                ICoordinate current = null;
                if (shapeEnumerator.MoveNext())
                { // one more shape point.
                    current = shapeEnumerator.Current;
                }
                else
                { // no more shape points.
                    isShapePoint = false;
                    current = graph.GetVertex(edge.From);
                }

                var line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                    new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                     new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                var coordinate = new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude);
                var projectedPoint = line.ProjectOn(coordinate);
                if (projectedPoint != null)
                { // ok, projection succeeded.
                    var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                        projectedPoint[1], projectedPoint[0],
                        latitude, longitude);
                    if (distance < distanceToProjected)
                    { // ok, new best edge yay!
                        distanceToProjected = (float)distance;
                        projectedLatitude = (float)projectedPoint[1];
                        projectedLongitude = (float)projectedPoint[0];
                        projectedDistanceFromFirst = (float)(previousShapeDistance +
                            OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                projectedLatitude, projectedLongitude,
                                previous.Latitude, previous.Longitude));
                        projectedShapeIndex = previousShapeIndex + 1;
                    }
                }

                if (!isShapePoint)
                { // if the current is not a shape point, it's time to stop.
                    var to = graph.GetVertex(edge.To);
                    totalLength = previousShapeDistance +
                        (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                            previous.Latitude, previous.Longitude,
                            to.Latitude, to.Longitude);
                    break;
                }

                // add up the current shape distance.
                previousShapeDistance += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                        previous.Latitude, previous.Longitude,
                        current.Latitude, current.Longitude);
                previousShapeIndex++;

                previous = current;
            }
            return distanceToProjected != float.MaxValue;
        }

        /// <summary>
        /// Gets the length of an edge.
        /// </summary>
        /// <returns></returns>
        public static float Length(this GeometricGraph graph, GeometricEdge edge)
        {
            var totalLength = 0.0f;

            ICoordinate previous = graph.GetVertex(edge.From);
            var shape = edge.Shape;
            var shapeEnumerator = shape.GetEnumerator();
            while (true)
            {
                // get current point.
                ICoordinate current = null;
                if (shapeEnumerator.MoveNext())
                { // one more shape point.
                    current = shapeEnumerator.Current;
                }
                else
                { // no more shape points.
                    current = graph.GetVertex(edge.From);
                    totalLength += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                            previous.Latitude, previous.Longitude,
                            current.Latitude, current.Longitude);
                    return totalLength;
                }
                totalLength += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                        previous.Latitude, previous.Longitude,
                        current.Latitude, current.Longitude);
                previous = current;
            }
        }
    }
}