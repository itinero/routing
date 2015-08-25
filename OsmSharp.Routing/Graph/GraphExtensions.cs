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

using OsmSharp.Collections.Arrays;
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Sorting;
using OsmSharp.Math.Algorithms;
using OsmSharp.Math.Geo.Simple;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Holds extensions for graphs.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Holds the default hilbert steps.
        /// </summary>
        public static int DefaultHilbertSteps = (int)System.Math.Pow(2, 15);
        
        /// <summary>
        /// Gets the location of the given vertex.
        /// </summary>
        /// <returns></returns>
        public static GeoCoordinateSimple GetLocation<TEdgeData>(this IGraphReadOnly<TEdgeData> graph, uint vertex)
            where TEdgeData : struct, IGraphEdgeData
        {
            float latitude, longitude;
            if(!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new Exception("Vertex not found.");
            }
            return new GeoCoordinateSimple()
            {
                Latitude = latitude,
                Longitude = longitude
            };
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void CopyFrom<TEdgeData>(this GraphBase<TEdgeData> copyTo, GraphBase<TEdgeData> copyFrom)
            where TEdgeData : struct, IGraphEdgeData
        {
            float latitude, longitude;
            for (uint vertex = 1; vertex <= copyFrom.VertexCount; vertex++)
            {
                copyFrom.GetVertex(vertex, out latitude, out longitude);
                uint newVertex = copyTo.AddVertex(latitude, longitude);
                if (newVertex != vertex)
                {
                    throw new Exception("Graph should be empty when copy new data to it.");
                }
            }

            for (uint vertex = 1; vertex <= copyFrom.VertexCount; vertex++)
            {
                var edges = new List<Edge<TEdgeData>>(copyFrom.GetEdges(vertex));
                foreach (var edge in edges)
                {
                    copyTo.AddEdge(vertex, edge.Neighbour, edge.EdgeData, edge.Intermediates);
                }
            }
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void SortHilbert<TEdgeData>(this GraphBase<TEdgeData> graph)
            where TEdgeData : struct, IGraphEdgeData
        {
            graph.SortHilbert(GraphExtensions.DefaultHilbertSteps);
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void SortHilbert<TEdgeData>(this GraphBase<TEdgeData> graph, int n)
            where TEdgeData : struct, IGraphEdgeData
        {
            graph.SortHilbert(GraphExtensions.DefaultHilbertSteps, null);
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void SortHilbert<TEdgeData>(this GraphBase<TEdgeData> graph, int n, Action<uint, uint> transform)
            where TEdgeData : struct, IGraphEdgeData
        {
            // build ranks.
            var ranks = graph.BuildHilbertRank(n);

            // invert ranks.
            var transformations = new HugeArray<uint>(ranks.Length);
            for (uint i = 0; i < ranks.Length; i++)
            {
                if(transform != null)
                {
                    transform(ranks[i], i);
                }
                transformations[ranks[i]] = i;
            }

            // copy from the given graph but with sorted vertices.
            graph.Sort(transformations);
        }

        /// <summary>
        /// Builds the reverse index for a directed graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <returns></returns>
        public static IDictionary<uint, List<uint>> BuildReverse<TEdgeData>(this GraphBase<TEdgeData> graph)
            where TEdgeData : struct, IGraphEdgeData
        {
            if (!graph.IsDirected) { throw new ArgumentException("Building a reverse index for a non-directed graph is not supported."); }

            var reverse = new Dictionary<uint, List<uint>>();
            for(uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                foreach (var edge in graph.GetEdges(vertex))
                {
                    var to = edge.Neighbour;
                    List<uint> neighbours;
                    if(!reverse.TryGetValue(to, out neighbours))
                    { // create an entry here.
                        neighbours = new List<uint>();
                    }
                    neighbours.Add(vertex);
                    reverse[to] = neighbours; // explicitly set again because this may be another kind of dictionary soon.
                }
            }
            return reverse;
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static List<uint> SearchHilbert<TEdgeData>(this GraphBase<TEdgeData> graph, float latitude, float longitude,
            float offset)
            where TEdgeData : struct, IGraphEdgeData
        {
            return GraphExtensions.SearchHilbert(graph, GraphExtensions.DefaultHilbertSteps, latitude, longitude, offset);
        }

        /// <summary>
        /// Searches the graph for nearby vertices assuming it has been sorted.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static List<uint> SearchHilbert<TEdgeData>(this GraphBase<TEdgeData> graph, int n, float latitude, float longitude,
            float offset)
            where TEdgeData : struct, IGraphEdgeData
        {
            var targets = HilbertCurve.HilbertDistances(
                System.Math.Max(latitude - offset, -90), 
                System.Math.Max(longitude - offset, -180),
                System.Math.Min(latitude + offset, 90), 
                System.Math.Min(longitude + offset, 180), n);
            targets.Sort();
            var vertices = new List<uint>();

            var targetIdx = 0;
            var vertex1 = (uint)1;
            var vertex2 = (uint)graph.VertexCount;
            float vertexLat, vertexLon;
            while(targetIdx < targets.Count)
            {
                uint vertex;
                int count;
                if(GraphExtensions.SearchHilbert(graph, targets[targetIdx], n, vertex1, vertex2, out vertex, out count))
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
        /// <typeparam name="TEdgeData"></typeparam>
        public static bool SearchHilbert<TEdgeData>(this GraphBase<TEdgeData> graph, long hilbert, int n,
            uint vertex1, uint vertex2, out uint vertex, out int count)
            where TEdgeData : struct, IGraphEdgeData
        {
            var hilbert1 = GraphExtensions.HilbertDistance(graph, n, vertex1);
            var hilbert2 = GraphExtensions.HilbertDistance(graph, n, vertex2);
            while (vertex1 <= vertex2)
            {
                // check the current hilbert distances.
                if(hilbert1 > hilbert2)
                { // situation is impossible and probably the graph is not sorted.
                    throw new Exception("Graph not sorted: Binary search using hilbert distance not possible.");
                }
                if (hilbert1 == hilbert)
                { // found at hilbert1.
                    var lower = vertex1;
                    while (hilbert1 == hilbert)
                    {
                        lower--;
                        if(lower == 0)
                        { // stop here, not more vertices lower.
                            break;
                        }
                        hilbert1 = GraphExtensions.HilbertDistance(graph, n, lower);
                    }
                    lower++;
                    var upper = vertex1;
                    hilbert1 = GraphExtensions.HilbertDistance(graph, n, upper);
                    while (hilbert1 == hilbert)
                    {
                        upper++;
                        if (upper > graph.VertexCount)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        hilbert1 = GraphExtensions.HilbertDistance(graph, n, upper);
                    }
                    upper--;
                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }
                if (hilbert2 == hilbert)
                { // found at hilbert2.
                    var lower = vertex2;
                    while (hilbert2 == hilbert)
                    {
                        lower--;
                        if (lower == 0)
                        { // stop here, not more vertices lower.
                            break;
                        }
                        hilbert2 = GraphExtensions.HilbertDistance(graph, n, lower);
                    }
                    lower++;
                    var upper = vertex2;
                    hilbert2 = GraphExtensions.HilbertDistance(graph, n, upper);
                    while (hilbert2 == hilbert)
                    {
                        upper++;
                        if (upper > graph.VertexCount)
                        { // stop here, no more vertices higher.
                            break;
                        }
                        hilbert2 = GraphExtensions.HilbertDistance(graph, n, upper);
                    }
                    upper--;
                    vertex = lower;
                    count = (int)(upper - lower) + 1;
                    return true;
                }
                if(hilbert1 == hilbert2 ||
                    vertex1 == vertex2 ||
                    vertex1 == vertex2 - 1)
                { // search is finished.
                    vertex = vertex1;
                    count = 0;
                    return true;
                }

                // Binary search: calculate hilbert distance of the middle.
                var vertexMiddle = vertex1 + (uint)((vertex2 - vertex1) / 2);
                var hilbertMiddle = GraphExtensions.HilbertDistance(graph, n, vertexMiddle);
                if(hilbert <= hilbertMiddle)
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
        /// Sorts the vertices in the given graph based on a hilbert curve using the default step count.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static HugeArrayBase<uint> BuildHilbertRank<TEdgeData>(this GraphBase<TEdgeData> graph)
            where TEdgeData : struct, IGraphEdgeData
        {
            var ranks = new HugeArray<uint>(graph.VertexCount + 1);
            graph.BuildHilbertRank(GraphExtensions.DefaultHilbertSteps, ranks);
            return ranks;
        }

        /// <summary>
        /// Sorts the vertices in the given graph based on a hilbert curve using the default step count.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static HugeArrayBase<uint> BuildHilbertRank<TEdgeData>(this GraphBase<TEdgeData> graph, int n)
            where TEdgeData : struct, IGraphEdgeData
        {
            var ranks = new HugeArray<uint>(graph.VertexCount + 1);
            graph.BuildHilbertRank(n, ranks);
            return ranks;
        }

        /// <summary>
        /// Sorts the vertices in the given graph based on a hilbert curve using the default step count.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void BuildHilbertRank<TEdgeData>(this GraphBase<TEdgeData> graph,
            HugeArrayBase<uint> ranks)
            where TEdgeData : struct, IGraphEdgeData
        {
            graph.BuildHilbertRank(GraphExtensions.DefaultHilbertSteps, ranks);
        }

        /// <summary>
        /// Sorts the vertices in the given graph based on a hilbert curve.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void BuildHilbertRank<TEdgeData>(this GraphBase<TEdgeData> graph, int n, 
            HugeArrayBase<uint> ranks)
            where TEdgeData : struct, IGraphEdgeData
        {
            if (graph == null) { throw new ArgumentNullException("graph"); }
            if (graph.VertexCount != ranks.Length - 1) { throw new ArgumentException("Graph and ranks array must have equal sizes."); }
            
            for (uint i = 0; i <= graph.VertexCount; i++)
            { // fill with default data.
                ranks[i] = i;
            } 
            
            if (graph.VertexCount == 1)
            { // just return the rank for the one vertex.
                ranks[0] = 0;
                ranks[1] = 1;
            }

            // sort the complete graph and keep the transformations.
            QuickSort.Sort((i) => graph.HilbertDistance(n, ranks[i]), (i, j) =>
            {
                var temp = ranks[i];
                ranks[i] = ranks[j];
                ranks[j] = temp;
            }, 1, graph.VertexCount);
        }

        /// <summary>
        /// Returns the hibert distance for n and the given vertex.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <param name="graph"></param>
        /// <param name="n"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public static long HilbertDistance<TEdgeData>(this GraphBase<TEdgeData> graph, int n, uint vertex)
            where TEdgeData : struct, IGraphEdgeData
        {
            float latitude, longitude;
            if(!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new Exception(string.Format("Vertex {0} does not exist in graph.", vertex));
            }
            return HilbertCurve.HilbertDistance(latitude, longitude, n);
        }

        /// <summary>
        /// Returns all hibert distances for n.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <returns></returns>
        public static long[] HilbertDistances<TEdgeData>(this GraphBase<TEdgeData> graph, int n)
            where TEdgeData : struct, IGraphEdgeData
        {
            var distances = new long[graph.VertexCount];
            for(uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                float latitude, longitude;
                graph.GetVertex(vertex, out latitude, out longitude);
                distances[vertex - 1] = HilbertCurve.HilbertDistance(latitude, longitude, n);
            }
            return distances;
        }

        /// <summary>
        /// Switches the locations around for the two given vertices.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void Switch<TEdgeData>(this GraphBase<TEdgeData> graph, uint vertex1, uint vertex2)
            where TEdgeData : struct, IGraphEdgeData
        {
            if (graph.IsDirected) { throw new ArgumentException("Cannot switch two vertices on a directed graph without it's reverse index."); }

            graph.Switch(vertex1, vertex2, null);
        }

        /// <summary>
        /// Switches the locations around for the two given vertices.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void Switch<TEdgeData>(this GraphBase<TEdgeData> graph, uint vertex1, uint vertex2,
            IDictionary<uint, List<uint>> reverse)
            where TEdgeData : struct, IGraphEdgeData
        {
            if (graph.IsDirected && reverse == null) { throw new ArgumentException("Cannot switch two vertices on a directed graph without it's reverse index."); }

            if (vertex1 == vertex2)
            { // switching identical vertices (?).
                return;
            }

            float vertex1Latitude, vertex1Longitude;
            graph.GetVertex(vertex1, out vertex1Latitude, out vertex1Longitude);
            float vertex2Latitude, vertex2Longitude;
            graph.GetVertex(vertex2, out vertex2Latitude, out vertex2Longitude);

            // update location.
            graph.SetVertex(vertex1, vertex2Latitude, vertex2Longitude);
            graph.SetVertex(vertex2, vertex1Latitude, vertex1Longitude);

            // switch and update edges, this is easy!
            var edges1 = new List<Edge<TEdgeData>>();
            foreach (var edge in graph.GetEdges(vertex1))
            {
                edges1.Add(new Edge<TEdgeData>()
                    {
                        EdgeData = edge.EdgeData,
                        Neighbour = edge.Neighbour,
                        Intermediates = edge.Intermediates != null ? new CoordinateArrayCollection<GeoCoordinateSimple>(
                            edge.Intermediates.ToSimpleArray()) : null
                    });
            }
            var edges2 = new List<Edge<TEdgeData>>();
            foreach (var edge in graph.GetEdges(vertex2))
            {
                edges2.Add(new Edge<TEdgeData>()
                    {
                        EdgeData = edge.EdgeData,
                        Neighbour = edge.Neighbour,
                        Intermediates = edge.Intermediates != null ? new CoordinateArrayCollection<GeoCoordinateSimple>(
                            edge.Intermediates.ToSimpleArray()) : null
                    });
            }

            graph.RemoveEdges(vertex1);
            graph.RemoveEdges(vertex2);

            foreach (var edge in edges1)
            { // update existing data.
                var neighbour = edge.Neighbour;
                var newNeighbour = edge.Neighbour;
                if (newNeighbour == vertex2)
                {
                    newNeighbour = vertex1;
                }
                graph.AddEdge(vertex2, newNeighbour, edge.EdgeData, edge.Intermediates);

                if (reverse != null)
                { // update reverse set if present.
                    List<uint> neighbours = null;
                    if (reverse.TryGetValue(neighbour, out neighbours))
                    { // remove.
                        neighbours.Remove(vertex1);
                        reverse[neighbour] = neighbours;
                    }
                    if (reverse.TryGetValue(newNeighbour, out neighbours))
                    { // add.
                        neighbours.Add(vertex2);
                    }
                    else
                    { // add new.
                        neighbours = new List<uint>();
                        neighbours.Add(vertex2);
                    }
                    reverse[newNeighbour] = neighbours;
                }
            }
            foreach (var edge in edges2)
            { // update existing data.
                var neighbour = edge.Neighbour;
                var newNeighbour = edge.Neighbour;
                if (newNeighbour == vertex1)
                {
                    newNeighbour = vertex2;
                }
                graph.AddEdge(vertex1, newNeighbour, edge.EdgeData, edge.Intermediates);

                if (reverse != null)
                { // update reverse set if present.
                    List<uint> neighbours = null;
                    if (reverse.TryGetValue(neighbour, out neighbours))
                    { // remove.
                        neighbours.Remove(vertex2);
                        reverse[neighbour] = neighbours;
                    }
                    if (reverse.TryGetValue(newNeighbour, out neighbours))
                    { // add.
                        neighbours.Add(vertex1);
                    }
                    else
                    { // add new.
                        neighbours = new List<uint>();
                        neighbours.Add(vertex1);
                    }
                    reverse[newNeighbour] = neighbours;
                }
            }

            if (graph.IsDirected)
            { // in a directed graph, there is more work to be done.
                var toUpdateSet = new HashSet<uint>();
                List<uint> neighbours1 = null;
                if (reverse.TryGetValue(vertex1, out neighbours1))
                {
                    for (var i = 0; i < neighbours1.Count; i++)
                    {
                        if (neighbours1[i] == vertex2)
                        {
                            neighbours1[i] = vertex1;
                        }
                        else
                        {
                            toUpdateSet.Add(neighbours1[i]);
                        }
                    }
                    reverse[vertex1] = neighbours1;
                }
                List<uint> neighbours2 = null;
                if (reverse.TryGetValue(vertex2, out neighbours2))
                {
                    for (var i = 0; i < neighbours2.Count; i++)
                    {
                        if (neighbours2[i] == vertex1)
                        {
                            neighbours2[i] = vertex2;
                        }
                        else
                        {
                            toUpdateSet.Add(neighbours2[i]);
                        }
                    }
                    reverse[vertex2] = neighbours2;
                }

                // switch reverses.
                if(neighbours2 == null)
                {
                    reverse.Remove(vertex1);
                }
                else
                {
                    reverse[vertex1] = neighbours2;
                }
                if (neighbours1 == null)
                {
                    reverse.Remove(vertex2);
                }
                else
                {
                    reverse[vertex2] = neighbours1;
                }

                // update all edges in these vertices.
                var updatedEdges = new List<Edge<TEdgeData>>();
                foreach(var toUpdate in toUpdateSet)
                {
                    updatedEdges.Clear();

                    foreach(var edge in graph.GetEdges(toUpdate))
                    {
                        if(edge.Neighbour == vertex1)
                        {
                            updatedEdges.Add(new Edge<TEdgeData>()
                            {
                                EdgeData = edge.EdgeData,
                                Neighbour = vertex2,
                                Intermediates = edge.Intermediates != null ? new CoordinateArrayCollection<GeoCoordinateSimple>(
                                    edge.Intermediates.ToSimpleArray()) : null
                            });
                        } else if(edge.Neighbour == vertex2) {
                            updatedEdges.Add(new Edge<TEdgeData>()
                            {
                                EdgeData = edge.EdgeData,
                                Neighbour = vertex1,
                                Intermediates = edge.Intermediates != null ? new CoordinateArrayCollection<GeoCoordinateSimple>(
                                    edge.Intermediates.ToSimpleArray()) : null
                            });
                        }
                    }

                    graph.RemoveEdge(toUpdate, vertex1);
                    graph.RemoveEdge(toUpdate, vertex2);

                    foreach(var updatedEdge in updatedEdges)
                    {
                        graph.AddEdge(toUpdate, updatedEdge.Neighbour, updatedEdge.EdgeData, updatedEdge.Intermediates);
                    }
                }
            }
        }
    }
}