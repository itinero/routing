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
using OsmSharp.Collections.Tags;
using OsmSharp.Geo.Attributes;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graphs.Geometric.Shapes;
using System;
using System.Collections.Generic;

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
            IEnumerator<ICoordinate> shapeEnumerator = null;
            if(shape != null)
            {
                shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
            }
            var previousShapeDistance = 0.0f;
            var previousShapeIndex = -1;

            while (true)
            {
                // get current point.
                var isShapePoint = true;
                ICoordinate current = null;
                if (shapeEnumerator != null && shapeEnumerator.MoveNext())
                { // one more shape point.
                    current = shapeEnumerator.Current;
                }
                else
                { // no more shape points.
                    isShapePoint = false;
                    current = graph.GetVertex(edge.To);
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
        public static float Length(this GeometricGraph graph, GeometricEdge edge)
        {
            var totalLength = 0.0f;

            ICoordinate previous = graph.GetVertex(edge.From);
            ICoordinate current = null;
            var shape = edge.Shape;
            if(shape != null)
            {
                var shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
                while (shapeEnumerator.MoveNext())
                {
                    current = shapeEnumerator.Current;
                    totalLength += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                            previous.Latitude, previous.Longitude,
                            current.Latitude, current.Longitude);
                    previous = current;
                }
            }
            current = graph.GetVertex(edge.To);
            totalLength += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                    previous.Latitude, previous.Longitude,
                    current.Latitude, current.Longitude);
            return totalLength;
        }

        /// <summary>
        /// Gets the shape points including the two vertices.
        /// </summary>
        public static List<ICoordinate> GetShape(this GeometricGraph graph, GeometricEdge geometricEdge)
        {
            var points = new List<ICoordinate>();
            points.Add(graph.GetVertex(geometricEdge.From));
            var shape = geometricEdge.Shape;
            if(shape != null)
            {
                if(geometricEdge.DataInverted)
                {
                    shape = shape.Reverse();
                }
                var shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
                while (shapeEnumerator.MoveNext())
                {
                    points.Add(shapeEnumerator.Current);
                }
            }
            points.Add(graph.GetVertex(geometricEdge.To));
            return points;
        }

        /// <summary>
        /// Gets the shape points starting at the given vertex until the max distance.
        /// </summary>
        /// <returns></returns>
        public static List<ICoordinate> GetShape(this GeometricGraph graph, GeometricEdge geometricEdge, float minDistance, float maxDistance)
        {
            var points = new List<ICoordinate>();
            if (geometricEdge.Shape == null)
            {
                return points;
            }
            ICoordinate previous = graph.GetVertex(geometricEdge.From);
            var distance = 0.0f;
            var shapeEnumerator = geometricEdge.Shape.GetEnumerator();
            shapeEnumerator.Reset();
            while (shapeEnumerator.MoveNext())
            {
                var current = shapeEnumerator.Current;
                distance += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                    previous, current);
                if(minDistance < distance &&
                    distance < maxDistance)
                {
                    points.Add(current);
                }
                previous = current;
            }
            return points;
        }

        /// <summary>
        /// Gets the shape points starting at the given vertex until the max distance.
        /// </summary>
        /// <returns></returns>
        public static List<ICoordinate> GetShape(this GeometricGraph graph, GeometricEdge geometricEdge, uint vertex, float maxDistance)
        {
            var points = new List<ICoordinate>();
            if(geometricEdge.Shape == null)
            {
                return points;
            }
            if (geometricEdge.From == vertex)
            { // start at from.
                ICoordinate previous = graph.GetVertex(vertex);
                var distance = 0.0f;
                var shapeEnumerator = geometricEdge.Shape.GetEnumerator();
                while(shapeEnumerator.MoveNext())
                {
                    var current = shapeEnumerator.Current;
                    distance += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                        previous, current);
                    if(distance >= maxDistance)
                    { // do not include this point anymore.
                        break;
                    }
                    points.Add(current);
                    previous = current;
                }
                return points;
            }
            else if (geometricEdge.To == vertex)
            { // start at to.
                var shape = geometricEdge.Shape.Reverse();
                ICoordinate previous = graph.GetVertex(vertex);
                var distance = 0.0f;
                var shapeEnumerator = shape.GetEnumerator();
                while (shapeEnumerator.MoveNext())
                {
                    var current = shapeEnumerator.Current;
                    distance += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                        previous, current);
                    if (distance >= maxDistance)
                    { // do not include this point anymore.
                        break;
                    }
                    points.Add(current);
                    previous = current;
                }
                return points;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, geometricEdge.Id));
        }

        /// <summary>
        /// Gets all features inside the given bounding box.
        /// </summary>
        /// <returns></returns>
        public static FeatureCollection GetFeaturesIn(this GeometricGraph graph, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var features = new FeatureCollection();

            var vertices = OsmSharp.Routing.Algorithms.Search.Hilbert.Search(graph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = graph.GetEdgeEnumerator();
            foreach(var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                features.Add(new Feature(new Point(new GeoCoordinate(vertexLocation.Latitude, vertexLocation.Longitude)),
                    new SimpleGeometryAttributeCollection(new Tag[] { Tag.Create("id", vertex.ToInvariantString()) })));
                edgeEnumerator.MoveTo(vertex);
                edgeEnumerator.Reset();
                while(edgeEnumerator.MoveNext())
                {
                    if(edges.Contains(edgeEnumerator.Id))
                    {
                        continue;
                    }
                    edges.Add(edgeEnumerator.Id);

                    var shape = graph.GetShape(edgeEnumerator.Current);
                    var coordinates = new List<GeoCoordinate>();
                    foreach(var shapePoint in shape)
                    {
                        coordinates.Add(new GeoCoordinate(shapePoint.Latitude, shapePoint.Longitude));
                    }
                    var geometry = new LineString(coordinates);
                    features.Add(new Feature(geometry,
                        new SimpleGeometryAttributeCollection(new Tag[] { Tag.Create("id", edgeEnumerator.Id.ToInvariantString()) })));
                }
            }

            return features;
        }

        /// <summary>
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
        /// <returns></returns>
        public static uint GetOther(this GeometricEdge edge, uint vertex)
        {
            if (edge.From == vertex)
            {
                return edge.To;
            }
            else if (edge.To == vertex)
            {
                return edge.From;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }
        
        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this GeometricGraph graph, uint vertex1, uint vertex2, uint[] data, params ICoordinate[] shape)
        {
            return graph.AddEdge(vertex1, vertex2, data, new ShapeEnumerable(shape));
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this GeometricGraph graph, uint vertex1, uint vertex2, uint[] data, IEnumerable<ICoordinate> shape)
        {
            return graph.AddEdge(vertex1, vertex2, data, new ShapeEnumerable(shape));
        }
    }
}