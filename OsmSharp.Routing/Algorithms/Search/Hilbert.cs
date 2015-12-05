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

using OsmSharp.Collections.Sorting;
using OsmSharp.Geo;
using OsmSharp.Math.Algorithms;
using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Network;
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
        public static void Sort(this RoutingNetwork graph)
        {
            graph.Sort(Hilbert.DefaultHilbertSteps);
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        public static void Sort(this GeometricGraph graph, int n)
        {
            if (graph.VertexCount > 0)
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
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        public static void Sort(this RoutingNetwork graph, int n)
        {
            if (graph.VertexCount > 0)
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
        /// Returns the hibert distance for n and the given vertex.
        /// </summary>
        /// <returns></returns>
        public static long Distance(this RoutingNetwork graph, int n, uint vertex)
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
            return Hilbert.Search(graph, Hilbert.DefaultHilbertSteps, latitude - offset, longitude - offset, 
                latitude + offset, longitude + offset);
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        /// <returns></returns>
        public static HashSet<uint> Search(this GeometricGraph graph, float minLatitude, float minLongitude, 
            float maxLatitude, float maxLongitude)
        {
            return Hilbert.Search(graph, Hilbert.DefaultHilbertSteps, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this GeometricGraph graph, int n, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var targets = HilbertCurve.HilbertDistances(
                System.Math.Max(minLatitude, -90),
                System.Math.Max(minLongitude, -180),
                System.Math.Min(maxLatitude, 90),
                System.Math.Min(maxLongitude, 180), n);
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
                            if (minLatitude < vertexLat &&
                                minLongitude < vertexLon &&
                                maxLatitude > vertexLat &&
                                maxLongitude > vertexLon)
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
            var coordinate = new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude);

            // find vertices within bounding box delta.
            var vertices = graph.Search(latitude, longitude, offset);

            // build result-structure.
            var bestEdge = Constants.NO_EDGE;
            var bestDistance = (double)maxDistanceMeter;

            // get edge enumerator.
            var edgeEnumerator = graph.GetEdgeEnumerator();

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.
            foreach(var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(latitude, longitude,
                    vertexLocation.Latitude, vertexLocation.Longitude);

                if (distance < bestDistance)
                { // ok, new best vertex yay!
                    edgeEnumerator.MoveTo(vertex);
                    while (edgeEnumerator.MoveNext())
                    {
                        if (isOk(edgeEnumerator.Current))
                        { // ok, edge is found to be ok.
                            bestDistance = distance;
                            bestEdge = edgeEnumerator.Id;
                            break;
                        }
                    }
                }
            }

            // go over all edges and check max distance box.
            var maxDistanceBox = new OsmSharp.Math.Geo.GeoCoordinateBox(
                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    Math.Geo.Meta.DirectionEnum.NorhtWest),
                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    Math.Geo.Meta.DirectionEnum.SouthEast));

            var checkedEdges = new HashSet<uint>();
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
                    ICoordinate current = null;
                    OsmSharp.Math.Geo.GeoCoordinateLine line;
                    OsmSharp.Math.Primitives.PointF2D projectedPoint;
                    var shape = edgeEnumerator.Shape;
                    if (shape != null)
                    { // loop over shape points.
                        if (edgeEnumerator.DataInverted)
                        { // invert shape.
                            shape = shape.Reverse();
                        }
                        var shapeEnumerator = shape.GetEnumerator();
                        shapeEnumerator.Reset();
                        while (shapeEnumerator.MoveNext())
                        {
                            current = shapeEnumerator.Current;
                            var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                        current.Latitude, current.Longitude, latitude, longitude);
                            if (distance < bestDistance)
                            { // ok this shape-point is clooose.
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

                            if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                                    current.Longitude, current.Latitude))
                            { // ok, it's possible there is an intersection here, project this point.
                                line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                                    new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                                     new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                                projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
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
                            previous = current;
                        }
                    }
                    current = graph.GetVertex(edgeEnumerator.To);
                    if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                            current.Longitude, current.Latitude))
                    { // ok, it's possible there is an intersection here, project this point.
                        line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                            new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                             new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                        projectedPoint = line.ProjectOn(coordinate);
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
            return bestEdge;
        }

        /// <summary>
        /// Searches for the closest edges under given conditions.
        /// </summary>
        /// <returns></returns>
        public static uint[] SearchClosestEdges(this GeometricGraph graph, float latitude, float longitude,
            float offset, float maxDistanceMeter, Func<GeometricEdge, bool>[] isOks)
        {
            var coordinate = new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude);

            // find vertices within bounding box delta.
            var vertices = graph.Search(latitude, longitude, offset);

            // build result-structure.
            var bestEdges = new uint[isOks.Length];
            var bestDistances = new double[isOks.Length];
            for (var i = 0; i < bestEdges.Length; i++)
            {
                bestEdges[i] = Constants.NO_EDGE;
                bestDistances[i] = (double)maxDistanceMeter;
            }

            // get edge enumerator.
            var edgeEnumerator = graph.GetEdgeEnumerator();

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.
            foreach (var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(latitude, longitude,
                    vertexLocation.Latitude, vertexLocation.Longitude);
                for (var i = 0; i < isOks.Length; i++)
                {
                    if (distance < bestDistances[i])
                    { // ok, new best vertex yay!
                        edgeEnumerator.MoveTo(vertex);
                        while (edgeEnumerator.MoveNext())
                        {
                            if (isOks[i](edgeEnumerator.Current))
                            { // ok, edge is found to be ok.
                                bestDistances[i] = distance;
                                bestEdges[i] = edgeEnumerator.Id;
                                break;
                            }
                        }
                    }
                }
            }

            // go over all edges and check max distance box.
            var maxDistanceBoxes = new OsmSharp.Math.Geo.GeoCoordinateBox[isOks.Length];
            for(var i =0; i < maxDistanceBoxes.Length; i++)
            {
                maxDistanceBoxes[i] = new OsmSharp.Math.Geo.GeoCoordinateBox(
                    (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                        Math.Geo.Meta.DirectionEnum.NorhtWest),
                    (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                        Math.Geo.Meta.DirectionEnum.SouthEast));
            }

            var checkedEdges = new HashSet<uint>();
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
                    var edgeIsOks = new bool[isOks.Length];
                    for(var i = 0; i < isOks.Length; i++)
                    {
                        edgeIsOks[i] = isOks[i] == null;
                    }
                    ICoordinate previous = sourceLocation;
                    ICoordinate current = null;
                    OsmSharp.Math.Geo.GeoCoordinateLine line;
                    OsmSharp.Math.Primitives.PointF2D projectedPoint;
                    var shape = edgeEnumerator.Shape;
                    if (shape != null)
                    { // loop over shape points.
                        if(edgeEnumerator.DataInverted)
                        { // invert shape.
                            shape = shape.Reverse();
                        }
                        var shapeEnumerator = shape.GetEnumerator();
                        shapeEnumerator.Reset();
                        while (shapeEnumerator.MoveNext())
                        {
                            current = shapeEnumerator.Current;
                            var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                        current.Latitude, current.Longitude, latitude, longitude);
                            for (var i = 0; i < bestEdges.Length; i++)
                            {
                                if (distance < bestDistances[i])
                                { // ok this shape-point is clooose.
                                    if (!edgeIsOks[i] && isOks[i](edgeEnumerator.Current))
                                    { // ok, edge is found to be ok.
                                        edgeIsOks[i] = true;
                                    }
                                    if (edgeIsOks[i])
                                    { // edge is ok, or all edges are ok by default.
                                        bestDistances[i] = distance;
                                        bestEdges[i] = edgeEnumerator.Id;

                                        // decrease max distance box.
                                        maxDistanceBoxes[i] = new OsmSharp.Math.Geo.GeoCoordinateBox(
                                            (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                Math.Geo.Meta.DirectionEnum.NorhtWest),
                                            (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                Math.Geo.Meta.DirectionEnum.SouthEast));
                                    }
                                }
                            }

                            if (maxDistanceBoxes.AnyIntersectsPotentially(previous.Longitude, previous.Latitude,
                                    current.Longitude, current.Latitude))
                            { // ok, it's possible there is an intersection here, project this point.
                                line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                                    new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                                     new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                                projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                        projectedPoint[1], projectedPoint[0],
                                        latitude, longitude);
                                    for (var i = 0; i < bestEdges.Length; i++)
                                    {
                                        if (distance < bestDistances[i])
                                        { // ok, new best edge yay!
                                            if (!edgeIsOks[i] && isOks[i](edgeEnumerator.Current))
                                            { // ok, edge is found to be ok.
                                                edgeIsOks[i] = true;
                                            }
                                            if (edgeIsOks[i])
                                            { // edge is ok, or all edges are ok by default.
                                                bestDistances[i] = distance;
                                                bestEdges[i] = edgeEnumerator.Id;

                                                // decrease max distance box.
                                                maxDistanceBoxes[i] = new OsmSharp.Math.Geo.GeoCoordinateBox(
                                                    (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                        Math.Geo.Meta.DirectionEnum.NorhtWest),
                                                    (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                        Math.Geo.Meta.DirectionEnum.SouthEast));
                                            }
                                        }
                                    }
                                }
                            }
                            previous = current;
                        }
                    }
                    current = graph.GetVertex(edgeEnumerator.To);
                    if (maxDistanceBoxes.AnyIntersectsPotentially(previous.Longitude, previous.Latitude,
                            current.Longitude, current.Latitude))
                    { // ok, it's possible there is an intersection here, project this point.
                        line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                            new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                             new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                        projectedPoint = line.ProjectOn(coordinate);
                        if (projectedPoint != null)
                        { // ok, projection succeeded.
                            var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                projectedPoint[1], projectedPoint[0],
                                latitude, longitude);
                            for (var i = 0; i < isOks.Length; i++)
                            {
                                if (distance < bestDistances[i])
                                { // ok, new best edge yay!
                                    if (!edgeIsOks[i] && isOks[i](edgeEnumerator.Current))
                                    { // ok, edge is found to be ok.
                                        edgeIsOks[i] = true;
                                    }
                                    if (edgeIsOks[i])
                                    { // edge is ok, or all edges are ok by default.
                                        bestDistances[i] = distance;
                                        bestEdges[i] = edgeEnumerator.Id;

                                        // decrease max distance box.
                                        maxDistanceBoxes[i] = new OsmSharp.Math.Geo.GeoCoordinateBox(
                                            (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                Math.Geo.Meta.DirectionEnum.NorhtWest),
                                            (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                Math.Geo.Meta.DirectionEnum.SouthEast));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return bestEdges;
        }

        /// <summary>
        /// Searches for all edges closer than max distance.
        /// </summary>
        public static List<uint> SearchCloserThan(this GeometricGraph graph, float latitude, float longitude,
            float offset, float maxDistanceMeter, Func<GeometricEdge, bool> isOk)
        {
            var result = new List<uint>();

            var coordinate = new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude);

            // find vertices within bounding box delta.
            var vertices = graph.Search(latitude, longitude, offset);

            // get edge enumerator.
            var edgeEnumerator = graph.GetEdgeEnumerator();

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.
            foreach (var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(latitude, longitude,
                    vertexLocation.Latitude, vertexLocation.Longitude);

                if (distance < maxDistanceMeter)
                { // ok, new best vertex yay!
                    edgeEnumerator.MoveTo(vertex);
                    while (edgeEnumerator.MoveNext())
                    {
                        if (isOk(edgeEnumerator.Current))
                        { // ok, edge is found to be ok.
                            result.Add(edgeEnumerator.Id);
                            break;
                        }
                    }
                }
            }

            // go over all edges and check max distance box.
            var maxDistanceBox = new OsmSharp.Math.Geo.GeoCoordinateBox(
                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    Math.Geo.Meta.DirectionEnum.NorhtWest),
                (new OsmSharp.Math.Geo.GeoCoordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    Math.Geo.Meta.DirectionEnum.SouthEast));

            var checkedEdges = new HashSet<uint>();
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
                    ICoordinate current = null;
                    OsmSharp.Math.Geo.GeoCoordinateLine line;
                    OsmSharp.Math.Primitives.PointF2D projectedPoint;
                    var shape = edgeEnumerator.Shape;
                    if (shape != null)
                    { // loop over shape points.
                        if (edgeEnumerator.DataInverted)
                        { // invert shape.
                            shape = shape.Reverse();
                        }
                        var shapeEnumerator = shape.GetEnumerator();
                        shapeEnumerator.Reset();
                        while (shapeEnumerator.MoveNext())
                        {
                            current = shapeEnumerator.Current;
                            var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                        current.Latitude, current.Longitude, latitude, longitude);
                            if (distance < maxDistanceMeter)
                            { // ok this shape-point is clooose.
                                if (!edgeIsOk && isOk(edgeEnumerator.Current))
                                { // ok, edge is found to be ok.
                                    edgeIsOk = true;
                                }
                                if (edgeIsOk)
                                { // edge is ok, or all edges are ok by default.
                                    result.Add(edgeEnumerator.Id);
                                }
                            }

                            if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                                    current.Longitude, current.Latitude))
                            { // ok, it's possible there is an intersection here, project this point.
                                line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                                    new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                                     new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                                projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                        projectedPoint[1], projectedPoint[0],
                                        latitude, longitude);
                                    if (distance < maxDistanceMeter)
                                    { // ok, new best edge yay!
                                        if (!edgeIsOk && isOk(edgeEnumerator.Current))
                                        { // ok, edge is found to be ok.
                                            edgeIsOk = true;
                                        }
                                        if (edgeIsOk)
                                        { // edge is ok, or all edges are ok by default.
                                            result.Add(edgeEnumerator.Id);
                                        }
                                    }
                                }
                            }
                            previous = current;
                        }
                    }
                    current = graph.GetVertex(edgeEnumerator.To);
                    if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                            current.Longitude, current.Latitude))
                    { // ok, it's possible there is an intersection here, project this point.
                        line = new OsmSharp.Math.Geo.GeoCoordinateLine(
                            new OsmSharp.Math.Geo.GeoCoordinate(previous.Latitude, previous.Longitude),
                             new OsmSharp.Math.Geo.GeoCoordinate(current.Latitude, current.Longitude), true, true);
                        projectedPoint = line.ProjectOn(coordinate);
                        if (projectedPoint != null)
                        { // ok, projection succeeded.
                            var distance = OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                projectedPoint[1], projectedPoint[0],
                                latitude, longitude);
                            if (distance < maxDistanceMeter)
                            { // ok, new best edge yay!
                                if (!edgeIsOk && isOk(edgeEnumerator.Current))
                                { // ok, edge is found to be ok.
                                    edgeIsOk = true;
                                }
                                if (edgeIsOk)
                                { // edge is ok, or all edges are ok by default.
                                    result.Add(edgeEnumerator.Id);
                                }
                            }
                        }
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns true if any of the boxes in the given array intersects potentially.
        /// </summary>
        /// <returns></returns>
        private static bool AnyIntersectsPotentially(this OsmSharp.Math.Geo.GeoCoordinateBox[] boxes,
            double x1, double y1, double x2, double y2)
        {
            for(var i = 0; i < boxes.Length; i++)
            {
                if(boxes[i].IntersectsPotentially(x1, y1, x2, y2))
                {
                    return true;
                }
            }
            return false;
        }
    }
}