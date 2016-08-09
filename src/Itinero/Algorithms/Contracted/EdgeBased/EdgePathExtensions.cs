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

using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Extension method for the edge path.
    /// </summary>
    public static class EdgePathExtensions
    {
        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath<T> Expand<T>(this EdgePath<T> edgePath, DirectedDynamicGraph graph, WeightHandler<T> weightHandler)
            where T : struct
        {
            return edgePath.Expand(graph.GetEdgeEnumerator(), weightHandler);
        }

        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath<T> Expand<T>(this EdgePath<T> edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator, WeightHandler<T> weightHandler)
            where T : struct
        {
            if (edgePath.From == null)
            {
                return edgePath;
            }

            // expand everything before.
            edgePath = new EdgePath<T>(edgePath.Vertex, edgePath.Weight, edgePath.Edge, edgePath.From.Expand(enumerator, weightHandler));

            // expand list.
            return edgePath.ExpandLast(enumerator, weightHandler);
        }

        /// <summary>
        /// Expands the last edge in the given edge path.
        /// </summary>
        private static EdgePath<T> ExpandLast<T>(this EdgePath<T> edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator, WeightHandler<T> weightHandler)
            where T : struct
        {
            bool? direction;
            if (edgePath.Edge != Constants.NO_EDGE &&
                edgePath.From != null &&
                edgePath.From.Vertex != Constants.NO_VERTEX)
            {
                enumerator.MoveToEdge(edgePath.Edge);
                var contractedId = enumerator.GetContracted();
                if (contractedId.HasValue)
                { // there is a contracted vertex here!
                    // get source/target sequences.
                    var sequence1 = enumerator.GetSequence1();
                    sequence1.Reverse();
                    var sequence2 = enumerator.GetSequence2();

                    // move to the first edge (contracted -> from vertex) and keep details.
                    enumerator.MoveToEdge(contractedId.Value, edgePath.From.Vertex, sequence1);
                    var edge1 = enumerator.IdDirected();
                    var weight1 = weightHandler.GetEdgeWeight(enumerator.Current, out direction);

                    // move to the second edge (contracted -> to vertex) and keep details.
                    enumerator.MoveToEdge(contractedId.Value, edgePath.Vertex, sequence2);
                    var weight2 = weightHandler.GetEdgeWeight(enumerator.Current, out direction);
                    var edge2 = enumerator.IdDirected();

                    if (edgePath.Edge > 0)
                    {
                        var contractedPath = new EdgePath<T>(contractedId.Value, weightHandler.Add(edgePath.From.Weight, weight1), -edge1, edgePath.From);
                        contractedPath = contractedPath.ExpandLast(enumerator, weightHandler);
                        return (new EdgePath<T>(edgePath.Vertex, edgePath.Weight, edge2, contractedPath)).ExpandLast(enumerator, weightHandler);
                    }
                    else
                    {
                        var contractedPath = new EdgePath<T>(contractedId.Value, weightHandler.Add(edgePath.From.Weight, weight1), edge1, edgePath.From);
                        contractedPath = contractedPath.ExpandLast(enumerator, weightHandler);
                        return (new EdgePath<T>(edgePath.Vertex, edgePath.Weight, -edge2, contractedPath)).ExpandLast(enumerator, weightHandler);
                    }
                }
            }
            return edgePath;
        }

        /// <summary>
        /// Gets sequence 1, the first vertices right after the start vertex.
        /// </summary>
        public static uint[] GetSequence1<T>(this EdgePath<T> path, DirectedDynamicGraph.EdgeEnumerator enumerator)
            where T : struct
        {
            return path.GetSequence1(enumerator, int.MaxValue);
        }

        /// <summary>
        /// Gets sequence 1, the first vertices right after the start vertex with a maximum of n.
        /// </summary>
        public static uint[] GetSequence1<T>(this EdgePath<T> path, DirectedDynamicGraph.EdgeEnumerator enumerator, int n)
            where T : struct
        {
            if (path.From == null)
            {
                return Constants.EMPTY_SEQUENCE;
            }

            var s = new List<uint>();
            s.Add(path.Vertex);
            while (true)
            {
                if (path.IsOriginal(enumerator))
                { // current segment is original.
                    if (s == null)
                    {
                        s = new List<uint>();
                    }
                    if (path.From.From != null)
                    { // we need more vertices and there are some more available.
                        s.Add(path.From.Vertex);
                        path = path.From;
                    }
                    else
                    { // we have enough.
                        var result = s.ToArray();
                        if (n < result.Length)
                        { // TODO: this can be way more efficient by creating only one array.
                            result = result.SubArray(result.Length - n, n);
                        }
                        result.Reverse();
                        return result;
                    }
                }
                else
                { // not an original edge, just return the start sequence.
                    var sequence = enumerator.GetSequence1();
                    if (path.From.From == null)
                    {
                        if (sequence.Length > n)
                        {
                            sequence = sequence.SubArray(sequence.Length - n, n);
                        }
                        return sequence;
                    }
                    s.Clear();
                    sequence.Reverse();
                    s.AddRange(sequence);
                    s.Add(path.From.Vertex);
                    path = path.From;
                }
            }
        }

        /// <summary>
        /// Gets sequence 2, the last vertices right before the end vertex.
        /// </summary>
        public static uint[] GetSequence2<T>(this EdgePath<T> path, DirectedDynamicGraph.EdgeEnumerator enumerator)
            where T : struct
        {
            return path.GetSequence2(enumerator, int.MaxValue);
        }

        /// <summary>
        /// Gets sequence 2, the last vertices right before the end vertex with a maximum of n.
        /// </summary>
        public static uint[] GetSequence2<T>(this EdgePath<T> path, DirectedDynamicGraph.EdgeEnumerator enumerator, int n)
            where T : struct
        {
            if (path.From == null)
            {
                return Constants.EMPTY_SEQUENCE;
            }

            List<uint> s = null;
            while (true)
            {
                if (path.IsOriginal(enumerator))
                { // current segment is original.
                    if (s == null)
                    {
                        s = new List<uint>();
                    }
                    s.Add(path.From.Vertex);
                    if (s.Count < n && path.From.From != null)
                    { // we need more vertices and there are some more available.
                        path = path.From;
                    }
                    else
                    { // we have enough.
                        var result = s.ToArray();
                        result.Reverse();
                        return result;
                    }
                }
                else
                { // not an original edge, just return the start sequence.
                    return enumerator.GetSequence2();
                }
            }
        }

        /// <summary>
        /// Returns true if the last edge in this path is an original edge.
        /// </summary>
        public static bool IsOriginal<T>(this EdgePath<T> path, DirectedDynamicGraph.EdgeEnumerator enumerator)
            where T : struct
        {
            if (path.From == null)
            { // when there is no previous vertex this is not an edge.
                throw new ArgumentException("The path is not an edge, cannot decide about originality.");
            }
            if (path.Edge == Constants.NO_EDGE)
            { // when there is no edge info, edge has to be original otherwise the info can never be recovered.
                return true;
            }
            enumerator.MoveToEdge(path.Edge);
            if (enumerator.IsOriginal())
            { // ok, edge is original.
                return true;
            }
            return false;
        }
    }
}