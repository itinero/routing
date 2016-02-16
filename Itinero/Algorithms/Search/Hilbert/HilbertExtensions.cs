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

using Itinero.Algorithms.Sorting;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Graphs.Geometric.Shapes;
using Itinero.Navigation.Directions;
using Itinero.Network;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Search.Hilbert
{
    /// <summary>
    /// Hilbert sorting.
    /// </summary>
    public static class HilbertExtensions
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
            graph.Sort(HilbertExtensions.DefaultHilbertSteps);
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        public static void Sort(this RoutingNetwork graph)
        {
            graph.Sort(HilbertExtensions.DefaultHilbertSteps);
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
                    if (vertex1 != vertex2)
                    {
                        graph.Switch((uint)vertex1, (uint)vertex2);
                    }
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
                    if (vertex1 != vertex2)
                    {
                        graph.Switch((uint)vertex1, (uint)vertex2);
                    }
                }, 0, graph.VertexCount - 1);
            }
        }

        /// <summary>
        /// Returns the hibert distance for n and the given vertex.
        /// </summary>
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
            return HilbertExtensions.Search(graph, HilbertExtensions.DefaultHilbertSteps, latitude - offset, longitude - offset, 
                latitude + offset, longitude + offset);
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        public static HashSet<uint> Search(this GeometricGraph graph, float minLatitude, float minLongitude, 
            float maxLatitude, float maxLongitude)
        {
            return HilbertExtensions.Search(graph, HilbertExtensions.DefaultHilbertSteps, minLatitude, minLongitude,
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
            while (targetIdx < targets.Count &&
                vertex1 < graph.VertexCount)
            {
                // check if there are consequitive distances.
                var distance = targets[targetIdx];
                var upper = distance;
                while (targetIdx < targets.Count - 1 &&
                    targets[targetIdx + 1] <= upper + 1)
                {
                    upper = targets[targetIdx + 1];
                    targetIdx++;
                }

                uint vertex;
                int count;
                if (distance == upper)
                {
                    if (HilbertExtensions.Search(graph, distance, n, vertex1, vertex2, out vertex, out count))
                    { // the search was successful.
                        var foundCount = count;
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
                        //vertex1 = vertex;
                        vertex1 = vertex + (uint)foundCount;
                    }
                }
                else
                {
                    if (HilbertExtensions.SearchRange(graph, distance, upper, n, vertex1, vertex2, out vertex, out count))
                    { // the search was successful.
                        var foundCount = count;
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
                        //vertex1 = vertex;
                        vertex1 = vertex + (uint)foundCount;
                    }
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
            var hilbert1 = HilbertExtensions.Distance(graph, n, vertex1);
            var hilbert2 = HilbertExtensions.Distance(graph, n, vertex2);
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
                        hilbert1 = HilbertExtensions.Distance(graph, n, lower);
                    }
                    var upper = vertex1;
                    hilbert1 = HilbertExtensions.Distance(graph, n, upper);
                    while (hilbert1 == hilbert)
                    {
                        if (upper >= graph.VertexCount - 1)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        upper++;
                        hilbert1 = HilbertExtensions.Distance(graph, n, upper);
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
                        hilbert2 = HilbertExtensions.Distance(graph, n, lower);
                    }
                    var upper = vertex2;
                    hilbert2 = HilbertExtensions.Distance(graph, n, upper);
                    while (hilbert2 == hilbert)
                    {
                        if (upper >= graph.VertexCount - 1)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        upper++;
                        hilbert2 = HilbertExtensions.Distance(graph, n, upper);
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
                var hilbertMiddle = HilbertExtensions.Distance(graph, n, vertexMiddle);
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
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        public static bool SearchRange(this GeometricGraph graph, long minHilbert, long maxHilbert, int n,
            uint vertex1, uint vertex2, out uint vertex, out int count)
        {
            var hilbert1 = HilbertExtensions.Distance(graph, n, vertex1);
            var hilbert2 = HilbertExtensions.Distance(graph, n, vertex2);
            while (vertex1 <= vertex2)
            {
                // check if both min and max are above or below half.
                var vertexMiddle = vertex1 + (uint)((vertex2 - vertex1) / 2);
                var hilbertMiddle = HilbertExtensions.Distance(graph, n, vertexMiddle);
                if (maxHilbert <= hilbertMiddle)
                { // both targets in the first part.
                    vertex2 = vertexMiddle;
                    hilbert2 = hilbertMiddle;
                }
                else if(minHilbert >= hilbertMiddle)
                { // both targets in the second part.
                    vertex1 = vertexMiddle;
                    hilbert1 = hilbertMiddle;
                }
                else
                { // middle is right between min and max hilbert.
                    // new binary search for minHilbert between vertex1 and middleVertex
                    //  => it's somewhere there!
                    var minVertex1 = vertex1;
                    var maxVertex1 = vertexMiddle;
                    var minHilbertVertex = uint.MaxValue;
                    while (minHilbertVertex == uint.MaxValue)
                    {
                        if (HilbertExtensions.Distance(graph, n, minVertex1) == minHilbert)
                        {
                            minHilbertVertex = minVertex1;
                        }
                        else if (HilbertExtensions.Distance(graph, n, maxVertex1) == minHilbert)
                        {
                            minHilbertVertex = maxVertex1;
                        }
                        else
                        {
                            while (minVertex1 <= maxVertex1)
                            {
                                var middleVertex1 = minVertex1 + (uint)((maxVertex1 - minVertex1) / 2);
                                var middleVertex1Hilbert = HilbertExtensions.Distance(graph, n, middleVertex1);

                                if (middleVertex1Hilbert > minHilbert)
                                { // minHilbert is in the first part.
                                    maxVertex1 = middleVertex1;
                                }
                                else if (middleVertex1Hilbert < minHilbert)
                                { // minHilbert is in the second part.
                                    minVertex1 = middleVertex1;
                                }
                                else
                                { // min hilbert was found.
                                    minHilbertVertex = middleVertex1;
                                    break;
                                }

                                if (minVertex1 == maxVertex1 ||
                                    minVertex1 == maxVertex1 - 1)
                                { // search is finished.
                                  // nothing was found.
                                    break;
                                }
                            }

                            if (minHilbertVertex == uint.MaxValue)
                            { // min hilbert doesn't exist.
                                minHilbert += 1;
                                if (minHilbert == maxHilbert)
                                { // both don't exist.
                                    vertex = vertex1;
                                    count = 0;
                                    return true;
                                }
                            }
                        }
                    }

                    // new binary search for maxHilbert between middleVertex and vertex2.
                    //  => it's somewhere there!
                    var minVertex2 = vertexMiddle;
                    var maxVertex2 = vertex2;
                    var maxHilbertVertex = uint.MaxValue;
                    while (maxHilbertVertex == uint.MaxValue)
                    {
                        if (HilbertExtensions.Distance(graph, n, minVertex2) == maxHilbert)
                        {
                            maxHilbertVertex = minVertex2;
                        }
                        else if (HilbertExtensions.Distance(graph, n, maxVertex2) == maxHilbert)
                        {
                            maxHilbertVertex = maxVertex2;
                        }
                        else
                        {
                            while (minVertex2 <= maxVertex2)
                            {
                                var middleVertex2 = minVertex2 + (uint)((maxVertex2 - minVertex2) / 2);
                                var middleVertex2Hilbert = HilbertExtensions.Distance(graph, n, middleVertex2);

                                if (middleVertex2Hilbert > maxHilbert)
                                { // minHilbert is in the first part.
                                    maxVertex2 = middleVertex2;
                                }
                                else if (middleVertex2Hilbert < maxHilbert)
                                { // minHilbert is in the second part.
                                    minVertex2 = middleVertex2;
                                }
                                else
                                { // min hilbert was found.
                                    maxHilbertVertex = middleVertex2;
                                    break;
                                }

                                if (minVertex2 == maxVertex2 ||
                                    minVertex2 == maxVertex2 - 1)
                                { // search is finished.
                                  // nothing was found.
                                    break;
                                }
                            }

                            if (maxHilbertVertex == uint.MaxValue)
                            { // max hilbert doesn't exist.
                                maxHilbert -= 1;
                                if (minHilbert == maxHilbert)
                                { // max hilbert doesn't exist, but min hilbert was found.
                                    maxHilbertVertex = minHilbertVertex;
                                    break;
                                }
                            }
                        }
                    }

                    // move the lowest down to check if there are more with the same hilbert distance.
                    var lower = minHilbertVertex;
                    while(true)
                    {
                        if(lower == 0)
                        { // going lower is impossible.
                            break;
                        }
                        var newHilbert = HilbertExtensions.Distance(graph, n, lower - 1);
                        if(newHilbert != minHilbert)
                        { // oeps, stop here!
                            break;
                        }
                        lower--;
                    }

                    // move the highest up to check if there are more with the same hilbert distance.
                    var upper = maxHilbertVertex;
                    while(true)
                    {
                        if(upper == graph.VertexCount - 1)
                        { // going higher is impossible.
                            break;
                        }
                        var newHilbert = HilbertExtensions.Distance(graph, n, upper + 1);
                        if(newHilbert != maxHilbert)
                        { // oeps, stop here.
                            break;
                        }
                        upper++;
                    }

                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }

                // check the current hilbert distances.
                if (hilbert1 > hilbert2)
                { // situation is impossible and probably the graph is not sorted.
                    throw new Exception("Graph not sorted: Binary search using hilbert distance not possible.");
                }

                if (hilbert1 == hilbert2 ||
                    vertex1 == vertex2 ||
                    vertex1 == vertex2 - 1)
                { // search is finished.
                    vertex = vertex1;
                    count = 0;
                    return true;
                }
            }
            vertex = vertex1;
            count = 0;
            return false;
        }

        /// <summary>
        /// Searches for the closest vertex.
        /// </summary>
        public static uint SearchClosest(this GeometricGraph graph, float latitude, float longitude,
            float latitudeOffset, float longitudeOffset)
        {
            // search for all nearby vertices.
            var vertices = HilbertExtensions.Search(graph, latitude - latitudeOffset, longitude - longitudeOffset, 
                latitude + latitudeOffset, longitude + longitudeOffset);

            var bestDistance = double.MaxValue;
            var bestVertex = Constants.NO_VERTEX;
            float lat, lon;
            foreach (var vertex in vertices)
            {
                if (graph.GetVertex(vertex, out lat, out lon))
                {
                    var distance = Coordinate.DistanceEstimateInMeter(latitude, longitude, lat, lon);
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
            float latitudeOffset, float longitudeOffset, float maxDistanceMeter, Func<GeometricEdge, bool> isOk)
        {
            var coordinate = new Coordinate(latitude, longitude);

            // find vertices within bounding box delta.
            var vertices = graph.Search(latitude - latitudeOffset, longitude - longitudeOffset, 
                latitude + latitudeOffset, longitude + longitudeOffset);

            // build result-structure.
            var bestEdge = Constants.NO_EDGE;
            var bestDistance = maxDistanceMeter;

            // get edge enumerator.
            var edgeEnumerator = graph.GetEdgeEnumerator();

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.
            foreach(var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                var distance = Coordinate.DistanceEstimateInMeter(
                    vertexLocation, new Coordinate(latitude, longitude));

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
            var maxDistanceBox = new Box(
                (new Coordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter, 
                    DirectionEnum.NorthWest),
                (new Coordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter, 
                    DirectionEnum.SouthEast));

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
                    Coordinate previous = sourceLocation;
                    Coordinate? current = null;
                    Line line;
                    Coordinate? projectedPoint;
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
                            var distance = Coordinate.DistanceEstimateInMeter(
                                        current.Value, new Coordinate(latitude, longitude));
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
                                    maxDistanceBox = new Box(
                                        (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                            DirectionEnum.NorthWest),
                                        (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                            DirectionEnum.SouthEast));
                                }
                            }

                            if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                                    current.Value.Longitude, current.Value.Latitude))
                            {
                                line = new Line(
                                    new Coordinate(previous.Latitude, previous.Longitude),
                                     new Coordinate(current.Value.Latitude, current.Value.Longitude));
                                projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    distance = Coordinate.DistanceEstimateInMeter(
                                        projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
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
                                            maxDistanceBox = new Box(
                                                (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                                    DirectionEnum.NorthWest),
                                                (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                                    DirectionEnum.SouthEast));
                                        }
                                    }
                                }
                            }
                            previous = current.Value;
                        }
                    }
                    current = graph.GetVertex(edgeEnumerator.To);
                    if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                            current.Value.Longitude, current.Value.Latitude))
                    { // ok, it's possible there is an intersection here, project this point.
                        line = new Line(
                            new Coordinate(previous.Latitude, previous.Longitude),
                             new Coordinate(current.Value.Latitude, current.Value.Longitude));
                        projectedPoint = line.ProjectOn(coordinate);
                        if (projectedPoint != null)
                        { // ok, projection succeeded.
                            var distance = Coordinate.DistanceEstimateInMeter(
                                projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
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
                                    maxDistanceBox = new Box(
                                        (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                            DirectionEnum.NorthWest),
                                        (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistance,
                                            DirectionEnum.SouthEast));
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
            float latitudeOffset, float longitudeOffset, float maxDistanceMeter, Func<GeometricEdge, bool>[] isOks)
        {
            var coordinate = new Coordinate(latitude, longitude);

            // find vertices within bounding box delta.
            var vertices = graph.Search(latitude - latitudeOffset, longitude - longitudeOffset,
                latitude + latitudeOffset, longitude + longitudeOffset);

            // build result-structure.
            var bestEdges = new uint[isOks.Length];
            var bestDistances = new float[isOks.Length];
            for (var i = 0; i < bestEdges.Length; i++)
            {
                bestEdges[i] = Constants.NO_EDGE;
                bestDistances[i] = maxDistanceMeter;
            }

            // get edge enumerator.
            var edgeEnumerator = graph.GetEdgeEnumerator();

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.
            foreach (var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                var distance = Coordinate.DistanceEstimateInMeter(latitude, longitude,
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
            var maxDistanceBoxes = new Box[isOks.Length];
            for(var i = 0; i < maxDistanceBoxes.Length; i++)
            {
                maxDistanceBoxes[i] = new Box(
                    (new Coordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                        DirectionEnum.NorthWest),
                    (new Coordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                        DirectionEnum.SouthEast));
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
                    Coordinate previous = sourceLocation;
                    Coordinate? current = null;
                    Line line;
                    Coordinate? projectedPoint;
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
                            var distance = Coordinate.DistanceEstimateInMeter(
                                        current.Value.Latitude, current.Value.Longitude, latitude, longitude);
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
                                        maxDistanceBoxes[i] = new Box(
                                            (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                DirectionEnum.NorthWest),
                                            (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                DirectionEnum.SouthEast));
                                    }
                                }
                            }

                            if (maxDistanceBoxes.AnyIntersectsPotentially(previous.Longitude, previous.Latitude,
                                    current.Value.Longitude, current.Value.Latitude))
                            { // ok, it's possible there is an intersection here, project this point.
                                line = new Line(
                                    new Coordinate(previous.Latitude, previous.Longitude),
                                     new Coordinate(current.Value.Latitude, current.Value.Longitude));
                                projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    distance = Coordinate.DistanceEstimateInMeter(
                                        projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
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
                                                maxDistanceBoxes[i] = new Box(
                                                    (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                        DirectionEnum.NorthWest),
                                                    (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                        DirectionEnum.SouthEast));
                                            }
                                        }
                                    }
                                }
                            }
                            previous = current.Value;
                        }
                    }
                    current = graph.GetVertex(edgeEnumerator.To);
                    if (maxDistanceBoxes.AnyIntersectsPotentially(previous.Longitude, previous.Latitude,
                            current.Value.Longitude, current.Value.Latitude))
                    { // ok, it's possible there is an intersection here, project this point.
                        line = new Line(
                            new Coordinate(previous.Latitude, previous.Longitude),
                            new Coordinate(current.Value.Latitude, current.Value.Longitude));
                        projectedPoint = line.ProjectOn(coordinate);
                        if (projectedPoint != null)
                        { // ok, projection succeeded.
                            var distance = Coordinate.DistanceEstimateInMeter(
                                projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
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
                                        maxDistanceBoxes[i] = new Box(
                                            (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                DirectionEnum.NorthWest),
                                            (new Coordinate(latitude, longitude)).OffsetWithDirection(bestDistances[i],
                                                DirectionEnum.SouthEast));
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
            var result = new HashSet<uint>();

            var coordinate = new Coordinate(latitude, longitude);

            // find vertices within bounding box delta.
            var vertices = graph.Search(latitude, longitude, offset);

            // get edge enumerator.
            var edgeEnumerator = graph.GetEdgeEnumerator();

            // TODO: get rid of creating all these object just to do some calculations!
            // TODO: sort vertices with the first vertices first.
            foreach (var vertex in vertices)
            {
                var vertexLocation = graph.GetVertex(vertex);
                var distance = Coordinate.DistanceEstimateInMeter(latitude, longitude,
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
            var maxDistanceBox = new Box(
                (new Coordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    DirectionEnum.NorthWest),
                (new Coordinate(latitude, longitude)).OffsetWithDirection(maxDistanceMeter,
                    DirectionEnum.SouthEast));

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
                    Coordinate previous = sourceLocation;
                    Coordinate? current = null;
                    Line line;
                    Coordinate? projectedPoint;
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
                            var distance = Coordinate.DistanceEstimateInMeter(
                                        current.Value.Latitude, current.Value.Longitude, latitude, longitude);
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
                                    current.Value.Longitude, current.Value.Latitude))
                            { // ok, it's possible there is an intersection here, project this point.
                                line = new Line(previous, current.Value);
                                projectedPoint = line.ProjectOn(coordinate);
                                if (projectedPoint != null)
                                { // ok, projection succeeded.
                                    distance = Coordinate.DistanceEstimateInMeter(
                                        projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
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
                            previous = current.Value;
                        }
                    }
                    current = graph.GetVertex(edgeEnumerator.To);
                    if (maxDistanceBox.IntersectsPotentially(previous.Longitude, previous.Latitude,
                            current.Value.Longitude, current.Value.Latitude))
                    { // ok, it's possible there is an intersection here, project this point.
                        line = new Line(
                            new Coordinate(previous.Latitude, previous.Longitude),
                             new Coordinate(current.Value.Latitude, current.Value.Longitude));
                        projectedPoint = line.ProjectOn(coordinate);
                        if (projectedPoint != null)
                        { // ok, projection succeeded.
                            var distance = Coordinate.DistanceEstimateInMeter(
                                projectedPoint.Value.Latitude, projectedPoint.Value.Longitude,
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
            return new List<uint>(result);
        }

        /// <summary>
        /// Returns true if any of the boxes in the given array intersects potentially.
        /// </summary>
        private static bool AnyIntersectsPotentially(this Box[] boxes,
            float x1, float y1, float x2, float y2)
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