/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

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
        public static EdgePath<T> Expand<T>(this EdgePath<T> edgePath, DirectedDynamicGraph graph, WeightHandler<T> weightHandler, bool direction)
            where T : struct
        {
            return edgePath.Expand(graph.GetEdgeEnumerator(), weightHandler, direction);
        }

        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath<T> Expand<T>(this EdgePath<T> edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator, WeightHandler<T> weightHandler, bool direction)
            where T : struct
        {
            if (edgePath.From == null)
            {
                return edgePath;
            }

            // expand everything before.
            edgePath = new EdgePath<T>(edgePath.Vertex, edgePath.Weight, edgePath.Edge, edgePath.From.Expand(enumerator, weightHandler, direction));

            // expand list.
            return edgePath.ExpandLast(enumerator, weightHandler, direction);
        }

        /// <summary>
        /// Expands the last edge in the given edge path.
        /// </summary>
        private static EdgePath<T> ExpandLast<T>(this EdgePath<T> edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator, WeightHandler<T> weightHandler, bool direction)
            where T : struct
        {
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

                    if (enumerator.Neighbour != edgePath.Vertex)
                    {
                        sequence1.Reverse();
                        sequence2.Reverse();

                        var t = sequence2;
                        sequence2 = sequence1;
                        sequence1 = t;
                    }

                    // move to the first edge (contracted -> from vertex) and keep details.
                    T weight1;
                    if (!enumerator.MoveToEdge(contractedId.Value, edgePath.From.Vertex, sequence1, weightHandler, !direction, out weight1))
                    {
                        throw new Exception(string.Format("Edge between {0} -> {1} with sequence {2} could not be found.",
                            contractedId.Value, edgePath.From.Vertex, sequence1.ToStringSafe()));
                    }
                    var edge1 = enumerator.IdDirected();

                    // move to the second edge (contracted -> to vertex) and keep details.
                    T weight2;
                    if (!enumerator.MoveToEdge(contractedId.Value, edgePath.Vertex, sequence2, weightHandler, direction, out weight2))
                    {
                        throw new Exception(string.Format("Edge between {0} -> {1} with sequence {2} could not be found.",
                            contractedId.Value, edgePath.Vertex, sequence2.ToStringSafe()));
                    }
                    var edge2 = enumerator.IdDirected();

                    var contractedPath = new EdgePath<T>(contractedId.Value, weightHandler.Add(edgePath.From.Weight, weight1), edge1, edgePath.From);
                    contractedPath = contractedPath.ExpandLast(enumerator, weightHandler, direction);
                    return (new EdgePath<T>(edgePath.Vertex, edgePath.Weight, edge2, contractedPath)).ExpandLast(enumerator, weightHandler, direction);
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
                        { // TODO: this can be way more efficient by creating only one array.
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

            return path.GetSequence2<T>(enumerator, n, new List<uint>());
        }

        /// <summary>
        /// Gets sequence 2, the last vertices right before the end vertex with a maximum of n.
        /// </summary>
        public static uint[] GetSequence2<T>(this EdgePath<T> path, DirectedDynamicGraph.EdgeEnumerator enumerator, int n,
            List<uint> s)
            where T : struct
        {
            if (path.From == null)
            {
                return Constants.EMPTY_SEQUENCE;
            }

            s.Clear();
            while (true)
            {
                if (path.IsOriginal(enumerator))
                { // current segment is original.
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
                    if (s.Count > 0)
                    {
                        var s2 = enumerator.GetSequence2();
                        var result = new uint[s.Count + s2.Length];
                        for (var i = 0; i < s.Count; i++)
                        {
                            result[result.Length - 1 - i] = s[i];
                        }
                        for (var i = 0; i < s2.Length; i++)
                        {
                            result[i] = s2[i];
                        }
                        return result;
                    }
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