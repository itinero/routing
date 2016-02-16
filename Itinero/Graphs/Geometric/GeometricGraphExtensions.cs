// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Geo;
using Itinero.Graphs.Geometric.Shapes;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs.Geometric
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
        public static bool ProjectOn(this GeometricGraph graph, GeometricEdge edge, float latitude, float longitude,
            out float projectedLatitude, out float projectedLongitude, out float projectedDistanceFromFirst,
            out int projectedShapeIndex, out float distanceToProjected, out float totalLength)
        {
            distanceToProjected = float.MaxValue;
            projectedDistanceFromFirst = 0;
            projectedLatitude = float.MaxValue;
            projectedLongitude = float.MaxValue;
            projectedShapeIndex = -1;

            var previous = graph.GetVertex(edge.From);
            var shape = edge.Shape;
            IEnumerator<Coordinate> shapeEnumerator = null;
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
                Coordinate? current = null;
                if (shapeEnumerator != null && shapeEnumerator.MoveNext())
                { // one more shape point.
                    current = shapeEnumerator.Current;
                }
                else
                { // no more shape points.
                    isShapePoint = false;
                    current = graph.GetVertex(edge.To);
                }

                var line = new Line(previous, current.Value);
                var coordinate = new Coordinate(latitude, longitude);
                var projectedPoint = line.ProjectOn(coordinate);
                if (projectedPoint != null)
                { // ok, projection succeeded.
                    var distance = Coordinate.DistanceEstimateInMeter(
                        projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
                        latitude, longitude);
                    if (distance < distanceToProjected)
                    { // ok, new best edge yay!
                        distanceToProjected = distance;
                        projectedLatitude = projectedPoint.Value.Latitude;
                        projectedLongitude = projectedPoint.Value.Longitude;
                        projectedDistanceFromFirst = (previousShapeDistance +
                            Coordinate.DistanceEstimateInMeter(
                                projectedLatitude, projectedLongitude,
                                previous.Latitude, previous.Longitude));
                        projectedShapeIndex = previousShapeIndex + 1;
                    }
                }

                if (!isShapePoint)
                { // if the current is not a shape point, it's time to stop.
                    var to = graph.GetVertex(edge.To);
                    totalLength = previousShapeDistance +
                        Coordinate.DistanceEstimateInMeter(
                            previous.Latitude, previous.Longitude,
                            to.Latitude, to.Longitude);
                    break;
                }

                // add up the current shape distance.
                previousShapeDistance += Coordinate.DistanceEstimateInMeter(
                        previous.Latitude, previous.Longitude,
                        current.Value.Latitude, current.Value.Longitude);
                previousShapeIndex++;

                previous = current.Value;
            }
            return distanceToProjected != float.MaxValue;
        }

        /// <summary>
        /// Gets the length of an edge.
        /// </summary>
        public static float Length(this GeometricGraph graph, GeometricEdge edge)
        {
            var totalLength = 0.0f;

            var previous = graph.GetVertex(edge.From);
            Coordinate? current = null;
            var shape = edge.Shape;
            if(shape != null)
            {
                var shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
                while (shapeEnumerator.MoveNext())
                {
                    current = shapeEnumerator.Current;
                    totalLength += Coordinate.DistanceEstimateInMeter(
                            previous.Latitude, previous.Longitude,
                            current.Value.Latitude, current.Value.Longitude);
                    previous = current.Value;
                }
            }
            current = graph.GetVertex(edge.To);
            totalLength += Coordinate.DistanceEstimateInMeter(
                    previous.Latitude, previous.Longitude,
                    current.Value.Latitude, current.Value.Longitude);
            return totalLength;
        }

        /// <summary>
        /// Gets the shape points including the two vertices.
        /// </summary>
        public static List<Coordinate> GetShape(this GeometricGraph graph, GeometricEdge geometricEdge)
        {
            var points = new List<Coordinate>();
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
        public static List<Coordinate> GetShape(this GeometricGraph graph, GeometricEdge geometricEdge, float minDistance, float maxDistance)
        {
            var points = new List<Coordinate>();
            if (geometricEdge.Shape == null)
            {
                return points;
            }
            var previous = graph.GetVertex(geometricEdge.From);
            var distance = 0.0f;
            var shapeEnumerator = geometricEdge.Shape.GetEnumerator();
            shapeEnumerator.Reset();
            while (shapeEnumerator.MoveNext())
            {
                var current = shapeEnumerator.Current;
                distance += Coordinate.DistanceEstimateInMeter(
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
        public static List<Coordinate> GetShape(this GeometricGraph graph, GeometricEdge geometricEdge, uint vertex, float maxDistance)
        {
            var points = new List<Coordinate>();
            if(geometricEdge.Shape == null)
            {
                return points;
            }
            if (geometricEdge.From == vertex)
            { // start at from.
                var previous = graph.GetVertex(vertex);
                var distance = 0.0f;
                var shapeEnumerator = geometricEdge.Shape.GetEnumerator();
                while(shapeEnumerator.MoveNext())
                {
                    var current = shapeEnumerator.Current;
                    distance += Coordinate.DistanceEstimateInMeter(
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
            else if (geometricEdge.To == vertex)
            { // start at to.
                var shape = geometricEdge.Shape.Reverse();
                var previous = graph.GetVertex(vertex);
                var distance = 0.0f;
                var shapeEnumerator = shape.GetEnumerator();
                while (shapeEnumerator.MoveNext())
                {
                    var current = shapeEnumerator.Current;
                    distance += Coordinate.DistanceEstimateInMeter(
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
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
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
        public static uint AddEdge(this GeometricGraph graph, uint vertex1, uint vertex2, uint[] data, params Coordinate[] shape)
        {
            return graph.AddEdge(vertex1, vertex2, data, new ShapeEnumerable(shape));
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public static uint AddEdge(this GeometricGraph graph, uint vertex1, uint vertex2, uint[] data, IEnumerable<Coordinate> shape)
        {
            return graph.AddEdge(vertex1, vertex2, data, new ShapeEnumerable(shape));
        }
    }
}