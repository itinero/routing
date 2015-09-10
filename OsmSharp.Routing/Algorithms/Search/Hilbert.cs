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
using OsmSharp.Collections.Sorting;
using OsmSharp.Math.Algorithms;
using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Graphs.Geometric;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Search
{
    /// <summary>
    /// Hilbert sorting.
    /// </summary>
    public static class Hilbert
    {
        /// <summary>
        /// Holds the default hilbert steps.
        /// </summary>
        public static int DefaultHilbertSteps = (int)System.Math.Pow(2, 15);

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        public static void Sort(this GeometricGraph graph)
        {
            graph.Sort(Hilbert.DefaultHilbertSteps);
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        public static void Sort(this GeometricGraph graph, int n)
        {
            QuickSort.Sort((vertex) =>
            {
                return graph.Distance(n, (uint)vertex);
            },
            (vertex1, vertex2) =>
            {
                graph.Switch((uint)vertex1, (uint)vertex2);
            }, 0, graph.VertexCount - 1);
        }

        /// <summary>
        /// Returns the hibert distance for n and the given vertex.
        /// </summary>
        /// <returns></returns>
        public static long Distance(this GeometricGraph graph, int n, uint vertex)
        {
            float latitude, longitude;
            if (!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new Exception(string.Format("Cannot calculate hilbert distance, vertex {0} does not exist.",
                    vertex));
            }
            return HilbertCurve.HilbertDistance(latitude, longitude, n);
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this GeometricGraph graph, float latitude, float longitude,
            float offset)
        {
            return Hilbert.Search(graph, Hilbert.DefaultHilbertSteps, latitude, longitude, offset);
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this GeometricGraph graph, int n, float latitude, float longitude,
            float offset)
        {
            var targets = HilbertCurve.HilbertDistances(
                System.Math.Max(latitude - offset, -90),
                System.Math.Max(longitude - offset, -180),
                System.Math.Min(latitude + offset, 90),
                System.Math.Min(longitude + offset, 180), n);
            targets.Sort();
            var vertices = new HashSet<uint>();

            var targetIdx = 0;
            var vertex1 = (uint)0;
            var vertex2 = (uint)graph.VertexCount - 1;
            float vertexLat, vertexLon;
            while (targetIdx < targets.Count)
            {
                uint vertex;
                int count;
                if (Hilbert.Search(graph, targets[targetIdx], n, vertex1, vertex2, out vertex, out count))
                { // the search was successful.
                    while (count > 0)
                    { // there have been vertices found.
                        if (graph.GetVertex((uint)vertex + (uint)(count - 1), out vertexLat, out vertexLon))
                        { // the vertex was found.
                            if (System.Math.Abs(latitude - vertexLat) < offset &&
                               System.Math.Abs(longitude - vertexLon) < offset)
                            { // within offset.
                                vertices.Add((uint)vertex + (uint)(count - 1));
                            }
                        }
                        count--;
                    }

                    // update vertex1.
                    vertex1 = vertex;
                }

                // move to next target.
                targetIdx++;
            }
            return vertices;
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        public static bool Search(this GeometricGraph graph, long hilbert, int n,
            uint vertex1, uint vertex2, out uint vertex, out int count)
        {
            var hilbert1 = Hilbert.Distance(graph, n, vertex1);
            var hilbert2 = Hilbert.Distance(graph, n, vertex2);
            while (vertex1 <= vertex2)
            {
                // check the current hilbert distances.
                if (hilbert1 > hilbert2)
                { // situation is impossible and probably the graph is not sorted.
                    throw new Exception("Graph not sorted: Binary search using hilbert distance not possible.");
                }
                if (hilbert1 == hilbert)
                { // found at hilbert1.
                    var lower = vertex1;
                    while (hilbert1 == hilbert)
                    {
                        if (lower == 0)
                        { // stop here, not more vertices lower.
                            break;
                        }
                        lower--;
                        hilbert1 = Hilbert.Distance(graph, n, lower);
                    }
                    var upper = vertex1;
                    hilbert1 = Hilbert.Distance(graph, n, upper);
                    while (hilbert1 == hilbert)
                    {
                        if (upper >= graph.VertexCount - 1)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        upper++;
                        hilbert1 = Hilbert.Distance(graph, n, upper);
                    }
                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }
                if (hilbert2 == hilbert)
                { // found at hilbert2.
                    var lower = vertex2;
                    while (hilbert2 == hilbert)
                    {
                        if (lower == 0)
                        { // stop here, not more vertices lower.
                            break;
                        }
                        lower--;
                        hilbert2 = Hilbert.Distance(graph, n, lower);
                    }
                    var upper = vertex2;
                    hilbert2 = Hilbert.Distance(graph, n, upper);
                    while (hilbert2 == hilbert)
                    {
                        if (upper >= graph.VertexCount - 1)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        upper++;
                        hilbert2 = Hilbert.Distance(graph, n, upper);
                    }
                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }
                if (hilbert1 == hilbert2 ||
                    vertex1 == vertex2 ||
                    vertex1 == vertex2 - 1)
                { // search is finished.
                    vertex = vertex1;
                    count = 0;
                    return true;
                }

                // Binary search: calculate hilbert distance of the middle.
                var vertexMiddle = vertex1 + (uint)((vertex2 - vertex1) / 2);
                var hilbertMiddle = Hilbert.Distance(graph, n, vertexMiddle);
                if (hilbert <= hilbertMiddle)
                { // target is in first part.
                    vertex2 = vertexMiddle;
                    hilbert2 = hilbertMiddle;
                }
                else
                { // target is in the second part.
                    vertex1 = vertexMiddle;
                    hilbert1 = hilbertMiddle;
                }
            }
            vertex = vertex1;
            count = 0;
            return false;
        }

        /// <summary>
        /// Searches for the closest vertex.
        /// </summary>
        /// <returns></returns>
        public static uint SearchClosest(this GeometricGraph graph, float latitude, float longitude,
            float offset)
        {
            // search for all nearby vertices.
            var vertices = Hilbert.Search(graph, latitude, longitude, offset);

            var bestDistance = double.MaxValue;
            var bestVertex = Constants.NO_VERTEX;
            float lat, lon;
            foreach (var vertex in vertices)
            {
                if (graph.GetVertex(vertex, out lat, out lon))
                {
                    var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(latitude, longitude, lat, lon);
                    if (distance < bestDistance)
                    { // a new closest vertex found.
                        bestDistance = distance;
                        bestVertex = vertex;
                    }
                }
            }
            return bestVertex;
        }

        /// <summary>
        /// Searches for the closest edge.
        /// </summary>
        /// <returns></returns>
        public static uint SearchClosestEdge(this GeometricGraph graph, float latitude, float longitude,
            float offset, float maxDistanceMeter, Func<GeometricEdge, bool> isOk)
        {
            // find vertices within bounding box delta.
            var vertices = graph.Search(longitude, longitude, offset);

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.

            // go over all edges and check max distance box.
            var maxDistanceBox = new OsmSharp.Math.Geo.GeoCoordinateBox(
                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    Math.Geo.Meta.DirectionEnum.NorhtWest),
                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    Math.Geo.Meta.DirectionEnum.SouthEast));

            // build result-structure.
            var bestEdge = Constants.NO_EDGE;
            var bestDistance = (double)maxDistanceMeter;

            var checkedEdges = new HashSet<uint>();
            var edgeEnumerator = graph.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                var sourceLocation = graph.GetVertex(vertex);
                if (!edgeEnumerator.MoveTo(vertex) ||
                   !edgeEnumerator.HasData)
                { // no edges, or vertex not found.
                    continue;
                }

                while (edgeEnumerator.MoveNext())
                { // loop over all edges and all shape-points.
                    if (checkedEdges.Contains(edgeEnumerator.Id))
                    { // edge was checked already.
                        continue;
                    }
                    checkedEdges.Add(edgeEnumerator.Id);

                    // check edge.
                    var edgeIsOk = isOk == null;
                    ICoordinate previous = sourceLocation;
                    var shape = edgeEnumerator.Shape;
                    if (shape != null)
                    { // loop over shape points.
                        foreach (var shapePoint in shape)
                        {
                            if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                                shapePoint.Longitude, shapePoint.Latitude))
                            { // ok, it's possible there is an intersection here, project this point.
                                var line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                                    new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                                     new OsmSharp.Math.Geo.GeoCoordinate(shapePoint.Latitude, shapePoint.Longitude), true, true);
                                var coordinate = new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude);
                                var projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                        projectedPoint[1], projectedPoint[0],
                                        latitude, longitude);
                                    if (distance < bestDistance)
                                    { // ok, new best edge yay!
                                        if (!edgeIsOk && isOk(edgeEnumerator.Current))
                                        { // ok, edge is found to be ok.
                                            edgeIsOk = true;
                                        }
                                        if (edgeIsOk)
                                        { // edge is ok, or all edges are ok by default.
                                            bestDistance = distance;
                                            bestEdge = edgeEnumerator.Id;

                                            // decrease max distance box.
                                            maxDistanceBox = new OsmSharp.Math.Geo.GeoCoordinateBox(
                                                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                                    Math.Geo.Meta.DirectionEnum.NorhtWest),
                                                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                                    Math.Geo.Meta.DirectionEnum.SouthEast));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bestEdge;
        }
    }
}
