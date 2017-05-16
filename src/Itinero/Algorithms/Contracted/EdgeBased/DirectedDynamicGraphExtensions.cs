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

using Itinero.Algorithms.Restrictions;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains contraction related extension methods related to the directed dynamic graph.
    /// </summary>
    /// <remarks>
    /// Edge definition of an edge-based dynamic graph:
    /// - Original edge     : [0] FIXED weight.
    /// - Contracted edges   
    ///     - no sequences  : [0] FIXED weight.
    ///                       [0] DYN   contracted id.
    ///     - sequences     : [0] FIXED weight.
    ///                       [0] DYN   contracted id.
    ///                       [1] DYN   size 
    ///                       [2] DYN   seq1.0 (assuming size = 1)
    ///                       [3] DYN   seq2.0 (starts at this location assuming size = 1)
    ///                       
    ///  Sequences are always added in the corresponding edge-direction:
    ///     - source seq: {source} -> seq1.0 -> seq1.0 -> ...
    ///     - target seq: ... -> seq2.0 -> seq2.1 -> {target}
    /// </remarks>
    public static class DirectedDynamicGraphExtensions
    {
        private static uint[] EMPTY = new uint[0];

        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static long IdDirected(this DynamicEdge edge)
        {
            return (edge.Id + 1);
        }

        /// <summary>
        /// Returns a directed version of the edge-id. Smaller than 0 if inverted, as-is if not inverted.
        /// </summary>
        /// <remarks>
        /// The relationship between a regular edge id and a directed edge id:
        /// - 0 -> 1 forward, -1 backward.
        /// - all other id's are offset by 1 and postive when forward, negative when backward.
        /// </remarks>
        public static long IdDirected(this DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            return (enumerator.Id + 1);
        }

        /// <summary>
        /// Moves to the given directed edge-id.
        /// </summary>
        public static void MoveToEdge(this DirectedDynamicGraph.EdgeEnumerator enumerator, long directedEdgeId)
        {
            if (directedEdgeId == 0) { throw new ArgumentOutOfRangeException("directedEdgeId"); }

            uint edgeId;
            if (directedEdgeId > 0)
            {
                edgeId = (uint)directedEdgeId - 1;
            }
            else
            {
                edgeId = (uint)((-directedEdgeId) - 1);
            }
            enumerator.MoveToEdge(edgeId);
        }

        /// <summary>
        /// Returns true if this edge is an original edge, not a shortcut.
        /// </summary>
        public static bool IsOriginal(this DynamicEdge edge)
        {
            if (edge.DynamicData == null)
            {
                throw new ArgumentException("DynamicData array of an edge should not be null.");
            }
            return edge.DynamicData.Length == 0;
        }

        /// <summary>
        /// Returns true if this edge is an original edge, not a shortcut.
        /// </summary>
        public static bool IsOriginal(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            return !edge.HasDynamicData;
        }

        /// <summary>
        /// Gets the contracted id, returns null if this edge is not a shortcut.
        /// </summary>
        public static uint? GetContracted(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            var dynamicO = edge.DynamicData0;
            if (dynamicO == uint.MaxValue)
            {
                return null;
            }
            return dynamicO;
        }

        /// <summary>
        /// Gets the contracted id, returns null if this edge is not a shortcut.
        /// </summary>
        public static uint? GetContracted(this DynamicEdge edge)
        {
            if (edge.DynamicData == null)
            {
                throw new ArgumentException("DynamicData array of an edge should not be null.");
            }
            if (edge.DynamicData.Length == 0)
            {
                return null;
            }
            return edge.DynamicData[0];
        }

        /// <summary>
        /// Gets the sequence at the source.
        /// </summary>
        public static uint GetSequence1(this DynamicEdge edge)
        {
            if (edge.IsOriginal())
            {
                return Constants.NO_VERTEX;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is is the sequence.
                return dynamicData[0];
            }
            var size = dynamicData[1];
            if (size == 0)
            {
                return dynamicData[0];
            }
            return dynamicData[2];
        }

        /// <summary>
        /// Gets the sequence at the source.
        /// </summary>
        public static uint GetSequence1(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            if (edge.IsOriginal())
            {
                return Constants.NO_VERTEX;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is is the sequence.
                return dynamicData[0];
            }
            var size = dynamicData[1];
            if (size == 0)
            {
                return dynamicData[0];
            }
            return dynamicData[2];
        }
        
        /// <summary>
        /// Gets the sequence at the target.
        /// </summary>
        public static uint GetSequence2(this DynamicEdge edge)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return Constants.NO_VERTEX;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is is the sequence.
                return dynamicData[0];
            }
            var size = dynamicData[1];
            if (dynamicData.Length - size - 2 == 0)
            {
                return dynamicData[0];
            }
            return dynamicData[dynamicData.Length - 1];
        }

        /// <summary>
        /// Gets the sequence at the target.
        /// </summary>
        public static uint GetSequence2(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return Constants.NO_VERTEX;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is the sequence.
                return dynamicData[0];
            }
            var size = dynamicData[1];
            if (dynamicData.Length - size - 2 == 0)
            {
                return dynamicData[0];
            }
            return dynamicData[dynamicData.Length - 1];
        }

        /// <summary>
        /// Moves the enumerator to the target vertex of the given edge id.
        /// </summary>
        public static bool MoveToTargetVertex(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint edgeId)
        {
            if(!enumerator.MoveToEdge(edgeId))
            {
                return false;
            }
            return enumerator.MoveTo(enumerator.Neighbour);
        }

        /// <summary>
        /// Gets the shortest edge between two vertices.
        /// </summary>
        /// <returns></returns>
        public static DynamicEdge GetShortestEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, Func<uint[], float?> getWeight)
        {
            var minWeight = float.MaxValue;
            var edges = graph.GetEdgeEnumerator(vertex1);
            DynamicEdge edge = null;
            while (edges.MoveNext())
            {
                if (edges.Neighbour == vertex2)
                { // the correct neighbour, get the weight.
                    var weight = getWeight(edges.Data);
                    if (weight.HasValue &&
                        weight.Value < minWeight)
                    { // weight is better.
                        edge = edges.Current;
                    }
                }
            }
            return edge;
        }

        /// <summary>
        /// Expands a the shortest edge between the two given vertices.
        /// </summary>
        public static void ExpandEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, List<uint> vertices, bool inverted,
            bool forward)
        {
            // check if expansion is needed.
            var edge = graph.GetShortestEdge(vertex1, vertex2, data =>
            {
                float weight;
                bool? direction;
                ContractedEdgeDataSerializer.Deserialize(data[0], out weight, out direction);
                if (direction == null || direction.Value == forward)
                {
                    return weight;
                }
                return null;
            });
            if (edge == null)
            {
                edge = graph.GetShortestEdge(vertex2, vertex1, data =>
                {
                    float weight;
                    bool? direction;
                    ContractedEdgeDataSerializer.Deserialize(data[0], out weight, out direction);
                    if (direction == null || direction.Value == !forward)
                    {
                        return weight;
                    }
                    return null;
                });
            }
            if (edge == null)
            { // no edge found!
                return;
                //throw new Exception(string.Format("No edge found from {0} to {1}.", vertex1, vertex2));
            }
            var edgeContractedId = edge.GetContracted();
            if (edgeContractedId.HasValue)
            { // further expansion needed.
                if (inverted)
                {
                    graph.ExpandEdge(edgeContractedId.Value, vertex1, vertices, false, !forward);
                    vertices.Add(edgeContractedId.Value);
                    graph.ExpandEdge(edgeContractedId.Value, vertex2, vertices, true, forward);
                }
                else
                {
                    graph.ExpandEdge(edgeContractedId.Value, vertex2, vertices, false, forward);
                    vertices.Add(edgeContractedId.Value);
                    graph.ExpandEdge(edgeContractedId.Value, vertex1, vertices, true, !forward);
                }
            }
        }

        /// <summary>
        /// Adds an edge.
        /// </summary>
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction)
        {
            return graph.AddEdge(vertex1, vertex2, ContractedEdgeDataSerializer.Serialize(weight, direction));
        }

        /// <summary>
        /// Adds an edge.
        /// </summary>
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, float distance, float time)
        {
            return graph.AddEdge(vertex1, vertex2, ContractedEdgeDataSerializer.SerializeDynamicAugmented(weight, direction, distance, time));
        }

        /// <summary>
        /// Adds a contracted edge including sequences.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="vertex1">The start vertex.</param>
        /// <param name="vertex2">The end vertex.</param>
        /// <param name="contractedId">The vertex being shortcutted.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="sequence1">The relevant sequence starting but not including vertex1; vertex1->(0->1...).</param>
        /// <param name="sequence2">The relevant sequence starting but not including vertex2; (0->1...)->vertex2.</param>
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId, 
            uint sequence1, uint sequence2)
        {
            if (sequence1 == contractedId)
            {
                sequence1 = Constants.NO_VERTEX;
            }
            if (sequence2 == contractedId)
            {
                sequence2 = Constants.NO_VERTEX;
            }

            var dataSize = 2; // Fixed weight + contracted id.
            if (sequence1 != Constants.NO_VERTEX)
            {
                dataSize += 1; // size field.
                dataSize += 1;
            }
            if (sequence2 != Constants.NO_VERTEX)
            {
                if (sequence1 == Constants.NO_VERTEX)
                {
                    dataSize += 1; // size field if sequence 1 null.
                }
                dataSize += 1;
            }
            var data = new uint[dataSize];
            data[0] = ContractedEdgeDataSerializer.Serialize(weight, direction);
            data[1] = contractedId;
            if (sequence1 != Constants.NO_VERTEX)
            {
                data[2] = (uint)1;
                data[3] = sequence1;
            }
            if (sequence2 != Constants.NO_VERTEX)
            {
                var sequence2Start = 3;
                if (sequence1 == Constants.NO_VERTEX)
                {
                    data[2] = 0;
                }
                else
                {
                    sequence2Start += 1;
                }
                data[sequence2Start] = sequence2;
            }
            return graph.AddEdge(vertex1, vertex2, data);
        }

        /// <summary>
        /// Adds a contracted edge including sequences.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="vertex1">The start vertex.</param>
        /// <param name="vertex2">The end vertex.</param>
        /// <param name="distance">The distance.</param>
        /// <param name="time">The time.</param>
        /// <param name="contractedId">The vertex being shortcutted.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="sequence1">The relevant sequence starting but not including vertex1; vertex1->(0->1...).</param>
        /// <param name="sequence2">The relevant sequence starting but not including vertex2; (0->1...)->vertex2.</param>
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId,
            float distance, float time, uint sequence1, uint sequence2)
        {
            if (sequence1 == contractedId)
            {
                sequence1 = Constants.NO_VERTEX;
            }
            if (sequence2 == contractedId)
            {
                sequence2 = Constants.NO_VERTEX;
            }

            var dataSize = 4; // fixed weight + contracted id + distance/time.
            if (sequence1 != Constants.NO_VERTEX)
            {
                dataSize += 1; // size field.
                dataSize += 1;
            }
            if (sequence2 != Constants.NO_VERTEX)
            {
                if (sequence1 == Constants.NO_VERTEX)
                {
                    dataSize += 1; // size field if sequence 1 null.
                }
                dataSize += 1;
            }
            var data = new uint[dataSize];
            data[0] = ContractedEdgeDataSerializer.Serialize(weight, direction);
            data[1] = ContractedEdgeDataSerializer.SerializeDistance(distance);
            data[2] = ContractedEdgeDataSerializer.SerializeTime(time);
            data[3] = contractedId;
            if (sequence1 != Constants.NO_VERTEX)
            {
                data[4] = (uint)1;
                data[5] = sequence1;
            }
            if (sequence2 != Constants.NO_VERTEX)
            {
                var sequence2Start = 5;
                if (sequence1 == Constants.NO_VERTEX)
                {
                    data[4] = 0;
                }
                else
                {
                    sequence2Start += 1;
                }
                data[sequence2Start] = sequence2;
            }
            return graph.AddEdge(vertex1, vertex2, data);
        }

        /// <summary>
        /// Add or update edge.
        /// </summary>
        /// <returns></returns>
        public static void AddOrUpdateEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight,
            bool? direction, uint contractedId, uint s1, uint s2)
        {
            var forward = false;
            var forwardWeight = float.MaxValue;
            var forwardContractedId = uint.MaxValue;
            var backward = false;
            var backwardWeight = float.MaxValue;
            var backwardContractedId = uint.MaxValue;

            if (direction == null || direction.Value)
            {
                forward = true;
                forwardWeight = weight;
                forwardContractedId = contractedId;
            }
            if (direction == null || !direction.Value)
            {
                backward = true;
                backwardWeight = weight;
                backwardContractedId = contractedId;
            }

            var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2 && !edgeEnumerator.IsOriginal())
                {
                    var localS1 = edgeEnumerator.GetSequence1();
                    var localS2 = edgeEnumerator.GetSequence2();
                    var localWeight = ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0);

                    if (s1 == localS1 &&
                        s2 == localS2)
                    {
                        if (localWeight.Direction.F)
                        { // a better identical edge found here.
                            if (forward)
                            { // there is already a forward weight, only replace if better.
                                if (localWeight.Weight < forwardWeight)
                                {
                                    forwardWeight = localWeight.Weight;
                                    forwardContractedId = edgeEnumerator.GetContracted().Value;
                                }
                            }
                            else
                            { // there is no forward weight, but we need to keep it.
                                forward = true;
                                forwardWeight = localWeight.Weight;
                                forwardContractedId = edgeEnumerator.GetContracted().Value;
                            }
                        }

                        if (localWeight.Direction.B)
                        {
                            if (backward)
                            { // there is already a backward weight, only replace if better. &&
                                if (localWeight.Weight < backwardWeight)
                                {
                                    backwardWeight = localWeight.Weight;
                                    backwardContractedId = edgeEnumerator.GetContracted().Value;
                                }
                            }
                            else
                            { // there is no backward weight, but we need to keep it.
                                backward = true;
                                backwardWeight = localWeight.Weight;
                                backwardContractedId = edgeEnumerator.GetContracted().Value;
                            }
                        }
                    }
                }
            }

            edgeEnumerator.TryRemoveAllEdges(vertex1, vertex2, s1, s2);

            if (forward && backward &&
                System.Math.Abs(forwardWeight - backwardWeight) < EdgeBased.Contraction.HierarchyBuilder<float>.E &&
                forwardContractedId == backwardContractedId)
            { // add one bidirectional edge.
                graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId, s1, s2);
            }
            else
            { // add two unidirectional edges if needed.
                if (forward)
                { // there is a forward edge.
                    graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, s1, s2);
                }
                if (backward)
                { // there is a backward edge.
                    graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, s1, s2);
                }
            }
        }
        
        /// <summary>
        /// Add or update edge.
        /// </summary>
        /// <returns></returns>
        public static void AddOrUpdateEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, float distance, float time,
            bool? direction, uint contractedId, uint s1, uint s2)
        {
            var forward = false;
            var forwardWeight = float.MaxValue;
            var forwardContractedId = uint.MaxValue;
            var forwardTime = float.MaxValue;
            var forwardDistance = float.MaxValue;
            var backward = false;
            var backwardWeight = float.MaxValue;
            var backwardContractedId = uint.MaxValue;
            var backwardTime = float.MaxValue;
            var backwardDistance = float.MaxValue;

            if (direction == null || direction.Value)
            {
                forward = true;
                forwardWeight = weight;
                forwardTime = time;
                forwardDistance = distance;
                forwardContractedId = contractedId;
            }
            if (direction == null || !direction.Value)
            {
                backward = true;
                backwardWeight = weight;
                backwardTime = time;
                backwardDistance = distance;
                backwardContractedId = contractedId;
            }

            var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2 && !edgeEnumerator.IsOriginal())
                {
                    var localS1 = edgeEnumerator.GetSequence1();
                    var localS2 = edgeEnumerator.GetSequence2();
                    float localWeight, localDistance, localTime;
                    bool? localDirection;
                    ContractedEdgeDataSerializer.DeserializeDynamic(edgeEnumerator.Data, out localWeight, out localDirection, out localDistance, 
                        out localTime);

                    if (s1 == localS1 &&
                        s2 == localS2)
                    {
                        if (localDirection == null || localDirection.Value)
                        { // a better identical edge found here.
                            if (forward)
                            { // there is already a forward weight, only replace if better.
                                if (localWeight < forwardWeight)
                                {
                                    forwardWeight = localWeight;
                                    forwardDistance = localDistance;
                                    forwardTime = localTime;
                                    forwardContractedId = edgeEnumerator.GetContracted().Value;
                                }
                            }
                            else
                            { // there is no forward weight, but we need to keep it.
                                forward = true;
                                forwardWeight = localWeight;
                                forwardDistance = localDistance;
                                forwardTime = localTime;
                                forwardContractedId = edgeEnumerator.GetContracted().Value;
                            }
                        }

                        if (localDirection == null || !localDirection.Value)
                        {
                            if (backward)
                            { // there is already a backward weight, only replace if better. &&
                                if (localWeight < backwardWeight)
                                {
                                    backwardWeight = localWeight;
                                    backwardTime = localTime;
                                    backwardDistance = localDistance;
                                    backwardContractedId = edgeEnumerator.GetContracted().Value;
                                }
                            }
                            else
                            { // there is no backward weight, but we need to keep it.
                                backward = true;
                                backwardWeight = localWeight;
                                backwardTime = localTime;
                                backwardDistance = localDistance;
                                backwardContractedId = edgeEnumerator.GetContracted().Value;
                            }
                        }
                    }
                }
            }

            edgeEnumerator.TryRemoveAllEdges(vertex1, vertex2, s1, s2);

            if (forward && backward &&
                System.Math.Abs(forwardWeight - backwardWeight) < EdgeBased.Contraction.HierarchyBuilder<float>.E &&
                System.Math.Abs(forwardDistance - backwardDistance) < EdgeBased.Contraction.HierarchyBuilder<float>.E &&
                System.Math.Abs(forwardTime - backwardTime) < EdgeBased.Contraction.HierarchyBuilder<float>.E &&
                forwardContractedId == backwardContractedId)
            { // add one bidirectional edge.
                graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId, forwardDistance, forwardTime, s1, s2);
            }
            else
            { // add two unidirectional edges if needed.
                if (forward)
                { // there is a forward edge.
                    graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, s1, s2);
                }
                if (backward)
                { // there is a backward edge.
                    graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, s1, s2);
                }
            }
        }

        ///// <summary>
        ///// Add or update edge.
        ///// </summary>
        ///// <returns></returns>
        //public static void TryAddOrUpdateEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId,
        //    uint[] s1, uint[] s2, out int added)
        //{
        //    var forward = false;
        //    var forwardWeight = float.MaxValue;
        //    var forwardContractedId = uint.MaxValue;
        //    var forwardS1 = Constants.EMPTY_SEQUENCE;
        //    var forwardS2 = Constants.EMPTY_SEQUENCE;
        //    var backward = false;
        //    var backwardWeight = float.MaxValue;
        //    var backwardContractedId = uint.MaxValue;
        //    var backwardS1 = Constants.EMPTY_SEQUENCE;
        //    var backwardS2 = Constants.EMPTY_SEQUENCE;

        //    if (direction == null || direction.Value)
        //    {
        //        forward = true;
        //        forwardWeight = weight;
        //        forwardContractedId = contractedId;
        //        forwardS1 = s1;
        //        forwardS2 = s2;
        //    }
        //    if (direction == null || !direction.Value)
        //    {
        //        backward = true;
        //        backwardWeight = weight;
        //        backwardContractedId = contractedId;
        //        backwardS1 = s1;
        //        backwardS2 = s2;
        //    }

        //    added = 0;

        //    if (forward && backward &&
        //        forwardWeight == backwardWeight &&
        //        forwardContractedId == backwardContractedId &&
        //        forwardS1.IsSequenceIdentical(backwardS1) &&
        //        forwardS2.IsSequenceIdentical(backwardS2))
        //    { // add one bidirectional edge.
        //        added++;
        //    }
        //    else
        //    { // add two unidirectional edges if needed.
        //        if (forward)
        //        { // there is a forward edge.
        //            added++;
        //        }
        //        if (backward)
        //        { // there is a backward edge.
        //            added++;
        //        }
        //    }
        //}

        /// <summary>
        /// Adds a contracted edge including sequences.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="vertex1">The start vertex.</param>
        /// <param name="vertex2">The end vertex.</param>
        /// <param name="contractedId">The vertex being shortcutted.</param>
        /// <param name="direction">The direction.</param>
        /// <param name="weight">The weight.</param>
        /// <param name="sequence1">The relevant sequence starting but not including vertex1; vertex1->(0->1...).</param>
        /// <param name="sequence2">The relevant sequence starting but not including vertex2; (0->1...)->vertex2.</param>
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint? contractedId, uint sequence1, uint sequence2)
        {
            if (!contractedId.HasValue)
            {
                return graph.AddEdge(vertex1, vertex2, weight, direction);
            }
            else
            {
                return graph.AddEdge(vertex1, vertex2, weight, direction, contractedId.Value, sequence1, sequence2);
            }
        }

        /// <summary>
        /// Gets the weight for the given original sequence.
        /// </summary>
        public static float GetOriginalWeight(this DirectedDynamicGraph graph, uint[] sequence)
        {
            float weight = 0;
            for(var i = 0; i < sequence.Length - 1; i++)
            {
                weight += graph.GetOriginalWeight(sequence[i], sequence[i + 1]);
            }
            return weight;
        }

        /// <summary>
        /// Gets the best original weight between the two given vertices.
        /// </summary>
        public static float GetOriginalWeight(this DirectedDynamicGraph graph, uint vertex1, uint vertex2)
        {
            var enumerator = graph.GetEdgeEnumerator();
            if (enumerator.MoveTo(vertex1))
            {
                while(enumerator.MoveNext())
                {
                    if (enumerator.Neighbour != vertex2)
                    {
                        continue;
                    }
                    if (enumerator.IsOriginal())
                    {
                        float weight;
                        bool? direction;
                        ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                            out weight, out direction);
                        if (direction == null || direction.Value)
                        {
                            return weight;
                        }
                    }
                }
            }
            if (enumerator.MoveTo(vertex2))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Neighbour != vertex1)
                    {
                        continue;
                    }
                    if (enumerator.IsOriginal())
                    {
                        float weight;
                        bool? direction;
                        ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                            out weight, out direction);
                        if (direction == null || !direction.Value)
                        {
                            return weight;
                        }
                    }
                }
            }
            throw new ArgumentException("No original edge found.");
        }

        /// <summary>
        /// Moves the enumerator to the edge representing the original edge between the two given vertices that can form a path from vertex1 -> vertex2. When returned true, the edge is vertex1 -> vertex2 when false vertex2 -> vertex1.
        /// </summary>
        public static bool MoveToOriginal(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2)
        {
            if (enumerator.MoveTo(vertex1))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Neighbour != vertex2)
                    {
                        continue;
                    }
                    if (enumerator.IsOriginal())
                    {
                        float weight;
                        bool? direction;
                        ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                            out weight, out direction);
                        if (direction == null || direction.Value)
                        {
                            return true;
                        }
                    }
                }
            }
            if (enumerator.MoveTo(vertex2))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Neighbour != vertex1)
                    {
                        continue;
                    }
                    float weight;
                    bool? direction;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                        out weight, out direction);
                    if (direction == null || !direction.Value)
                    {
                        return false;
                    }
                }
            }
            throw new ArgumentException("No original edge found.");
        }

        /// <summary>
        /// Gets the original edge.
        /// </summary>
        public static DynamicEdge GetOriginal(this DirectedDynamicGraph graph, uint vertex1, uint vertex2)
        {
            return graph.GetEdgeEnumerator().GetOriginal(vertex1, vertex2);
        }

        /// <summary>
        /// Gets the original edge.
        /// </summary>
        public static DynamicEdge GetOriginal(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2)
        {
            if (enumerator.MoveTo(vertex1))
            {
                while (enumerator.MoveNext())
                {
                    if (enumerator.Neighbour != vertex2)
                    {
                        continue;
                    }
                    if (enumerator.IsOriginal())
                    {
                        return enumerator.Current;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        public static float Weight(this DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            float weight;
            bool? direction;
            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Gets the direction.
        /// </summary>
        public static bool? Direction(this DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            float weight;
            bool? direction;
            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                out weight, out direction);
            return direction;
        }

        /// <summary>
        /// Removes the edge (vertex1->vertex2) with identical sequences.
        /// </summary>
        public static void RemoveEdge<T>(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2, uint sequence1, uint sequence2,
            Weights.WeightHandler<T> weightHandler, bool? direction)
            where T : struct
        {
            if (!TryRemoveEdge(enumerator, vertex1, vertex2, sequence1, sequence2, weightHandler, direction))
            {
                throw new Exception("Edge {0}->{1} could not be removed because no matching edge was found!");
            }
        }

        /// <summary>
        /// Removes the edge (vertex1->vertex2) with identical sequences.
        /// </summary>
        public static bool TryRemoveEdge<T>(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2, uint sequence1, uint sequence2,
            Weights.WeightHandler<T> weightHandler, bool? direction)
            where T : struct
        {
            enumerator.MoveTo(vertex1);
            while (enumerator.MoveNext())
            {
                if (enumerator.Neighbour != vertex2)
                {
                    continue;
                }

                bool? xDirection;
                var xWeight = weightHandler.GetEdgeWeight(enumerator, out xDirection);
                if (xDirection != null &&
                    xDirection.Value != direction)
                {
                    continue;
                }

                if (direction != null &&
                    direction.Value != xDirection)
                {
                    continue;
                }

                if (sequence1 != enumerator.GetSequence1())
                {
                    continue;
                }
                if (sequence2 != enumerator.GetSequence2())
                {
                    continue;
                }

                enumerator.Graph.RemoveEdgeById(vertex1, enumerator.Id);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Removes all the edges (vertex1->vertex2) with identical sequences.
        /// </summary>
        public static bool TryRemoveAllEdges(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2, uint sequence1, uint sequence2)
        {
            var removed = false;
            enumerator.MoveTo(vertex1);
            while (enumerator.MoveNext())
            {
                if (enumerator.Neighbour != vertex2)
                {
                    continue;
                }

                if (sequence1 != enumerator.GetSequence1())
                {
                    continue;
                }
                if (sequence2 != enumerator.GetSequence2())
                {
                    continue;
                }

                enumerator.Graph.RemoveEdgeById(vertex1, enumerator.Id);
                removed = true;
                enumerator.MoveTo(vertex1); // remove others if there are more.
            }
            return removed;
        }

        /// <summary>
        /// Moves this enumerator to an edge (vertex1->vertex2) that has an end sequence that matches the given sequence.
        /// </summary>
        public static bool MoveToEdge<T>(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2, uint sequence2, 
            Weights.WeightHandler<T> weightHandler, bool direction, out T weight)
            where T : struct
        {
            enumerator.MoveTo(vertex1);
            while(enumerator.MoveNext())
            {
                if (enumerator.Neighbour != vertex2)
                {
                    continue;
                }

                bool? xDirection;
                var xWeight = weightHandler.GetEdgeWeight(enumerator, out xDirection);
                if (xDirection != null &&
                    xDirection.Value != direction)
                {
                    continue;
                }

                if (xDirection != null &&
                    xDirection.Value != direction)
                {
                    continue;
                }

                var xSequence2 = enumerator.GetSequence2();
                if (xSequence2 != sequence2)
                {
                    continue;
                }
                weight = xWeight;
                return true;
            }
            weight = default(T);
            return false;
        }

        /// <summary>
        /// Gets all edges starting at this edges.
        /// </summary>
        public static List<DynamicEdge> GetEdges(this DirectedDynamicGraph graph, uint vertex)
        {
            return new List<DynamicEdge>(graph.GetEdgeEnumerator(vertex));
        }

        /// <summary>
        /// Builds a path along the sequence of vertices that can be followed using original edges.
        /// </summary>
        /// <param name="enumerator">The enumerator.</param>
        /// <param name="originalPath">The sequence."</param>
        /// <param name="reverse">The sequence has to be used in reverse, for performance reasons to avoid creating another array.</param>
        /// <param name="pathIsBackwards">When the resulting path is a backwards path agains the direction of the direction flags.</param>
        public static EdgePath<float> BuildPath(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint[] originalPath, bool reverse = false, bool pathIsBackwards = false)
        {
            if (!pathIsBackwards)
            {
                if (!reverse)
                {
                    var path = new EdgePath<float>(originalPath[0]);
                    for (var i = 1; i < originalPath.Length; i++)
                    {
                        if (enumerator.MoveToOriginal(originalPath[i - 1], originalPath[i]))
                        {
                            float weight;
                            bool? direction;
                            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                out weight, out direction);
                            path = new EdgePath<float>(originalPath[i], weight + path.Weight, enumerator.IdDirected(), path);
                        }
                        else
                        {
                            float weight;
                            bool? direction;
                            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                out weight, out direction);
                            path = new EdgePath<float>(originalPath[i], weight + path.Weight, -enumerator.IdDirected(), path);
                        }
                    }
                    return path;
                }
                else
                {
                    var path = new EdgePath<float>(originalPath[originalPath.Length - 1]);
                    if (originalPath.Length > 1)
                    {
                        for (var i = originalPath.Length - 2; i >= 0; i--)
                        {
                            if (enumerator.MoveToOriginal(originalPath[i + 1], originalPath[i]))
                            {
                                float weight;
                                bool? direction;
                                ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                    out weight, out direction);
                                path = new EdgePath<float>(originalPath[i], weight + path.Weight, enumerator.IdDirected(), path);
                            }
                            else
                            {
                                float weight;
                                bool? direction;
                                ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                    out weight, out direction);
                                path = new EdgePath<float>(originalPath[i], weight + path.Weight, -enumerator.IdDirected(), path);
                            }
                        }
                    }
                    return path;
                }
            }
            else
            {
                if (!reverse)
                {
                    var path = new EdgePath<float>(originalPath[0]);
                    for (var i = 1; i < originalPath.Length; i++)
                    {
                        if (enumerator.MoveToOriginal(originalPath[i], originalPath[i - 1]))
                        {
                            float weight;
                            bool? direction;
                            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                out weight, out direction);
                            path = new EdgePath<float>(originalPath[i], weight + path.Weight, enumerator.IdDirected(), path);
                        }
                        else
                        {
                            float weight;
                            bool? direction;
                            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                out weight, out direction);
                            path = new EdgePath<float>(originalPath[i], weight + path.Weight, -enumerator.IdDirected(), path);
                        }
                    }
                    return path;
                }
                else
                {
                    var path = new EdgePath<float>(originalPath[originalPath.Length - 1]);
                    if (originalPath.Length > 1)
                    {
                        for (var i = originalPath.Length - 2; i >= 0; i--)
                        {
                            if (enumerator.MoveToOriginal(originalPath[i], originalPath[i + 1]))
                            {
                                float weight;
                                bool? direction;
                                ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                    out weight, out direction);
                                path = new EdgePath<float>(originalPath[i], weight + path.Weight, enumerator.IdDirected(), path);
                            }
                            else
                            {
                                float weight;
                                bool? direction;
                                ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
                                    out weight, out direction);
                                path = new EdgePath<float>(originalPath[i], weight + path.Weight, -enumerator.IdDirected(), path);
                            }
                        }
                    }
                    return path;
                }
            }
        }

        /// <summary>
        /// Builds a path along the sequence of vertices that can be followed using original edges.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="originalPath">The sequence."</param>
        /// <param name="reverse">The sequence has to be used in reverse, for performance reasons to avoid creating another array.</param>
        /// <param name="pathIsBackwards">When the resulting path is a backwards path agains the direction of the direction flags.</param>
        public static EdgePath<float> BuildPath(this DirectedDynamicGraph graph, uint[] originalPath, bool reverse = false, bool pathIsBackwards = false)
        {
            return graph.GetEdgeEnumerator().BuildPath(originalPath, reverse);
        }
    }
}