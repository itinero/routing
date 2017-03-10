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
        public static uint[] GetSequence1(this DynamicEdge edge)
        {
            if (edge.IsOriginal())
            {
                return EMPTY;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is is the sequence.
                return new uint[] { dynamicData[0] };
            }
            var size = dynamicData[1];
            if (size == 0)
            {
                return new uint[] { dynamicData[0] };
            }
            var sequence = new uint[size];
            for (var i = 0; i < size; i++)
            {
                sequence[i] = dynamicData[i + 2];
            }
            return sequence;
        }

        /// <summary>
        /// Gets the sequence at the source.
        /// </summary>
        public static uint[] GetSequence1(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            if (edge.IsOriginal())
            {
                return EMPTY;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is is the sequence.
                return new uint[] { dynamicData[0] };
            }
            var size = dynamicData[1];
            if (size == 0)
            {
                return new uint[] { dynamicData[0] };
            }
            var sequence = new uint[size];
            for (var i = 0; i < size; i++)
            {
                sequence[i] = dynamicData[i + 2];
            }
            return sequence;
        }

        /// <summary>
        /// Gets the sequence at the source.
        /// </summary>
        /// <returns>The number of elements in the array that represent the sequence.</returns>
        public static int GetSequence1(this DirectedDynamicGraph.EdgeEnumerator edge, ref uint[] sequence1)
        {
            if (edge.IsOriginal())
            {
                return 0;
            }

            var dynamicDataLength = edge.FillWithDynamicData(ref sequence1);
            //var dynamicData = edge.DynamicData;
            var dynamicData = sequence1;
            if (dynamicDataLength < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicDataLength < 2)
            { // only a contracted id, the contracted id the sequence.
                return 1;
            }
            var size = dynamicData[1];
            if (size == 0)
            {
                return 1;
            }
            for (var i = 0; i < size; i++)
            {
                sequence1[i] = sequence1[i + 2];
            }
            return (int)size;
        }

        /// <summary>
        /// Gets the sequence at the target.
        /// </summary>
        public static uint[] GetSequence2(this DynamicEdge edge)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return EMPTY;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is is the sequence.
                return new uint[] { dynamicData[0] };
            }
            var size = dynamicData[1];
            if (dynamicData.Length - size - 2 == 0)
            {
                return new uint[] { dynamicData[0] };
            }
            var sequence = new uint[dynamicData.Length - 2 - size];
            for (var i = 0; i < sequence.Length; i++)
            {
                sequence[i] = dynamicData[size + 2];
            }
            return sequence;
        }

        /// <summary>
        /// Gets the sequence at the target.
        /// </summary>
        public static uint[] GetSequence2(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return EMPTY;
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            { // not even a contraced id but also not an original, something is wrong here!
                throw new ArgumentException("The given edge is not a shortcut part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            { // only a contracted id, the contracted is the sequence.
                return new uint[] { dynamicData[0] };
            }
            var size = dynamicData[1];
            if (dynamicData.Length - size - 2 == 0)
            {
                return new uint[] { dynamicData[0] };
            }
            var sequence = new uint[dynamicData.Length - 2 - size];
            for (var i = 0; i < sequence.Length; i++)
            {
                sequence[i] = dynamicData[size + 2 + i];
            }
            return sequence;
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
            uint[] sequence1, uint[] sequence2)
        {
            if (sequence1 != null && sequence1.Length == 1 &&
                sequence1[0] == contractedId)
            {
                sequence1 = null;
            }
            if (sequence2 != null && sequence2.Length == 1 &&
                sequence2[0] == contractedId)
            {
                sequence2 = null;
            }

            var dataSize = 2; // Fixed weight + contracted id.
            if (sequence1 != null && sequence1.Length != 0)
            {
                dataSize += 1; // size field.
                dataSize += sequence1.Length;
            }
            if (sequence2 != null && sequence2.Length != 0)
            {
                if (sequence1 == null || sequence1.Length == 0)
                {
                    dataSize += 1; // size field if sequence 1 null.
                }
                dataSize += sequence2.Length;
            }
            var data = new uint[dataSize];
            data[0] = ContractedEdgeDataSerializer.Serialize(weight, direction);
            data[1] = contractedId;
            if (sequence1 != null && sequence1.Length != 0)
            {
                data[2] = (uint)sequence1.Length;
                sequence1.CopyTo(data, 3);
            }
            if (sequence2 != null && sequence2.Length != 0)
            {
                var sequence2Start = 3;
                if (sequence1 == null || sequence1.Length == 0)
                {
                    data[2] = 0;
                }
                else
                {
                    sequence2Start += sequence1.Length;
                }
                sequence2.CopyTo(data, sequence2Start);
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
            float distance, float time, uint[] sequence1, uint[] sequence2)
        {
            if (sequence1 != null && sequence1.Length == 1 &&
                sequence1[0] == contractedId)
            {
                sequence1 = null;
            }
            if (sequence2 != null && sequence2.Length == 1 &&
                sequence2[0] == contractedId)
            {
                sequence2 = null;
            }

            var dataSize = 4; // fixed weight + contracted id + distance/time.
            if (sequence1 != null && sequence1.Length != 0)
            {
                dataSize += 1; // size field.
                dataSize += sequence1.Length;
            }
            if (sequence2 != null && sequence2.Length != 0)
            {
                if (sequence1 == null || sequence1.Length == 0)
                {
                    dataSize += 1; // size field if sequence 1 null.
                }
                dataSize += sequence2.Length;
            }
            var data = new uint[dataSize];
            data[0] = ContractedEdgeDataSerializer.Serialize(weight, direction);
            data[1] = ContractedEdgeDataSerializer.SerializeDistance(distance);
            data[2] = ContractedEdgeDataSerializer.SerializeTime(time);
            data[3] = contractedId;
            if (sequence1 != null && sequence1.Length != 0)
            {
                data[4] = (uint)sequence1.Length;
                sequence1.CopyTo(data, 5);
            }
            if (sequence2 != null && sequence2.Length != 0)
            {
                var sequence2Start = 5;
                if (sequence1 == null || sequence1.Length == 0)
                {
                    data[4] = 0;
                }
                else
                {
                    sequence2Start += sequence1.Length;
                }
                sequence2.CopyTo(data, sequence2Start);
            }
            return graph.AddEdge(vertex1, vertex2, data);
        }

        /// <summary>
        /// Add or update edge.
        /// </summary>
        /// <returns></returns>
        public static void AddOrUpdateEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight,
            bool? direction, uint contractedId, uint[] s1, uint[] s2)
        {
            if ((vertex1 == 2692 && vertex2 == 2730)  ||
                (vertex1 == 2730 && vertex2 == 2692))
            {
                Itinero.Logging.Logger.Log("", Logging.TraceEventType.Information, "");
            }

            var forward = false;
            var forwardWeight = float.MaxValue;
            var forwardContractedId = uint.MaxValue;
            var forwardS1 = Constants.EMPTY_SEQUENCE;
            var forwardS2 = Constants.EMPTY_SEQUENCE;
            var backward = false;
            var backwardWeight = float.MaxValue;
            var backwardContractedId = uint.MaxValue;
            var backwardS1 = Constants.EMPTY_SEQUENCE;
            var backwardS2 = Constants.EMPTY_SEQUENCE;

            if (direction == null || direction.Value)
            {
                forward = true;
                forwardWeight = weight;
                forwardContractedId = contractedId;
                forwardS1 = s1;
                forwardS2 = s2;
            }
            if (direction == null || !direction.Value)
            {
                backward = true;
                backwardWeight = weight;
                backwardContractedId = contractedId;
                backwardS1 = s1;
                backwardS2 = s2;
            }

            var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2)
                {
                    float localWeight;
                    bool? localDirection;
                    uint localContractedId = Constants.NO_VERTEX;
                    var localS1 = Constants.EMPTY_SEQUENCE;
                    var localS2 = Constants.EMPTY_SEQUENCE;
                    ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                        out localWeight, out localDirection);
                    if (!edgeEnumerator.IsOriginal())
                    {
                        localContractedId = edgeEnumerator.GetContracted().Value;
                        localS1 = edgeEnumerator.GetSequence1();
                        localS2 = edgeEnumerator.GetSequence2();
                    }
                    if (localDirection == null || localDirection.Value)
                    {
                        if (localWeight < forwardWeight)
                        {
                            forwardWeight = localWeight;
                            forward = true;
                            forwardContractedId = localContractedId;
                            forwardS1 = localS1;
                            forwardS2 = localS2;
                        }
                    }
                    if (localDirection == null || !localDirection.Value)
                    {
                        if (localWeight < backwardWeight)
                        {
                            backwardWeight = localWeight;
                            backward = true;
                            backwardContractedId = localContractedId;
                            backwardS1 = localS1;
                            backwardS2 = localS2;
                        }
                    }
                }
            }

            graph.RemoveEdge(vertex1, vertex2);

            if (forward && backward &&
                forwardWeight == backwardWeight &&
                forwardContractedId == backwardContractedId && 
                forwardS1.IsSequenceIdentical(backwardS1) &&
                forwardS2.IsSequenceIdentical(backwardS2))
            { // add one bidirectional edge.
                if (forwardContractedId == Constants.NO_VERTEX)
                {
                    graph.AddEdge(vertex1, vertex2, forwardWeight, null);
                }
                else
                {
                    graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId, forwardS1, forwardS2);
                }
            }
            else
            { // add two unidirectional edges if needed.
                if (forward)
                { // there is a forward edge.
                    if (forwardContractedId == Constants.NO_VERTEX)
                    {
                        graph.AddEdge(vertex1, vertex2, forwardWeight, true);
                    }
                    else
                    {
                        graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, forwardS1, forwardS2);
                    }
                }
                if (backward)
                { // there is a backward edge.
                    if (backwardContractedId == Constants.NO_VERTEX)
                    {
                        graph.AddEdge(vertex1, vertex2, backwardWeight, false);
                    }
                    else
                    {
                        graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, backwardS1, backwardS2);
                    }
                }
            }
        }
        
        /// <summary>
        /// Add or update edge.
        /// </summary>
        /// <returns></returns>
        public static void AddOrUpdateEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight,
            bool? direction, uint contractedId, float distance, float time, uint[] s1, uint[] s2)
        {
            var forward = false;
            var forwardWeight = float.MaxValue;
            var forwardContractedId = uint.MaxValue;
            var forwardDistance = float.MaxValue;
            var forwardTime = float.MaxValue;
            var forwardS1 = Constants.EMPTY_SEQUENCE;
            var forwardS2 = Constants.EMPTY_SEQUENCE;
            var backward = false;
            var backwardWeight = float.MaxValue;
            var backwardContractedId = uint.MaxValue;
            var backwardDistance = float.MaxValue;
            var backwardTime = float.MaxValue;
            var backwardS1 = Constants.EMPTY_SEQUENCE;
            var backwardS2 = Constants.EMPTY_SEQUENCE;

            if (direction == null || direction.Value)
            {
                forward = true;
                forwardWeight = weight;
                forwardContractedId = contractedId;
                forwardDistance = distance;
                forwardTime = time;
                forwardS1 = s1;
                forwardS2 = s2;
            }
            if (direction == null || !direction.Value)
            {
                backward = true;
                backwardWeight = weight;
                backwardContractedId = contractedId;
                backwardDistance = distance;
                backwardTime = time;
                backwardS1 = s1;
                backwardS2 = s2;
            }

            var edgeEnumerator = graph.GetEdgeEnumerator(vertex1);
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2)
                {
                    float localWeight;
                    bool? localDirection;
                    uint localContractedId = Constants.NO_VERTEX;
                    float localDistance;
                    float localTime;
                    var localS1 = Constants.EMPTY_SEQUENCE;
                    var localS2 = Constants.EMPTY_SEQUENCE;
                    ContractedEdgeDataSerializer.DeserializeDynamic(edgeEnumerator.Data,
                        out localWeight, out localDirection, out localDistance, out localTime);
                    if (!edgeEnumerator.IsOriginal())
                    {
                        localContractedId = edgeEnumerator.GetContracted().Value;
                        localS1 = edgeEnumerator.GetSequence1();
                        localS2 = edgeEnumerator.GetSequence2();
                    }
                    if (localDirection == null || localDirection.Value)
                    {
                        if (localWeight < forwardWeight)
                        {
                            forwardWeight = localWeight;
                            forward = true;
                            forwardContractedId = localContractedId;
                            forwardDistance = localDistance;
                            forwardTime = localTime;
                            forwardS1 = localS1;
                            forwardS2 = localS2;
                        }
                    }
                    if (localDirection == null || !localDirection.Value)
                    {
                        if (localWeight < backwardWeight)
                        {
                            backwardWeight = localWeight;
                            backward = true;
                            backwardContractedId = localContractedId;
                            backwardDistance = localDistance;
                            backwardTime = localTime;
                            backwardS1 = localS1;
                            backwardS2 = localS2;
                        }
                    }
                }
            }

            graph.RemoveEdge(vertex1, vertex2);

            if (forward && backward &&
                forwardWeight == backwardWeight &&
                forwardContractedId == backwardContractedId &&
                forwardS1.IsSequenceIdentical(backwardS1) &&
                forwardS2.IsSequenceIdentical(backwardS2))
            { // add one bidirectional edge.
                if (forwardContractedId == Constants.NO_VERTEX)
                {
                    graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardDistance, forwardTime);
                }
                else
                {
                    graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId, forwardDistance, forwardTime, forwardS1, forwardS2);
                }
            }
            else
            { // add two unidirectional edges if needed.
                if (forward)
                { // there is a forward edge.
                    if (forwardContractedId == Constants.NO_VERTEX)
                    {
                        graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardDistance, forwardTime);
                    }
                    else
                    {
                        graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, forwardDistance, forwardTime, forwardS1, forwardS2);
                    }
                }
                if (backward)
                { // there is a backward edge.
                    if (backwardContractedId == Constants.NO_VERTEX)
                    {
                        graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardDistance, backwardTime);
                    }
                    else
                    {
                        graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, backwardDistance, backwardTime, backwardS1, backwardS2);
                    }
                }
            }
        }

        /// <summary>
        /// Tries adding or updating an edge and returns #added and #removed edges.
        /// </summary>
        /// <returns></returns>
        public static void TryAddOrUpdateEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId,
            out int added, out int removed)
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
            var edgeCount = 0;
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.Neighbour == vertex2)
                {
                    edgeCount++;
                    float localWeight;
                    bool? localDirection;
                    uint localContractedId = Constants.NO_VERTEX;
                    ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                        out localWeight, out localDirection);
                    if (!edgeEnumerator.IsOriginal())
                    {
                        localContractedId = edgeEnumerator.GetContracted().Value;
                    }
                    if (localDirection == null || localDirection.Value)
                    {
                        if (localWeight < forwardWeight)
                        {
                            forwardWeight = localWeight;
                            forward = true;
                            forwardContractedId = localContractedId;
                        }
                    }
                    if (localDirection == null || !localDirection.Value)
                    {
                        if (localWeight < backwardWeight)
                        {
                            backwardWeight = localWeight;
                            backward = true;
                            backwardContractedId = localContractedId;
                        }
                    }
                }
            }

            removed = edgeCount;
            added = 0;

            if (forward && backward &&
                forwardWeight == backwardWeight &&
                forwardContractedId == backwardContractedId)
            { // add one bidirectional edge.
                added++;
            }
            else
            { // add two unidirectional edges if needed.
                if (forward)
                { // there is a forward edge.
                    added++;
                }
                if (backward)
                { // there is a backward edge.
                    added++;
                }
            }
        }

        ///// <summary>
        ///// Adds or updates a contracted edge including sequences.
        ///// </summary>
        ///// <param name="graph">The graph.</param>
        ///// <param name="vertex1">The start vertex.</param>
        ///// <param name="vertex2">The end vertex.</param>
        ///// <param name="contractedId">The vertex being shortcutted.</param>
        ///// <param name="direction">The direction.</param>
        ///// <param name="weight">The weight.</param>
        ///// <param name="sequence1">The relevant sequence starting but not including vertex1; vertex1->(0->1...).</param>
        ///// <param name="sequence2">The relevant sequence starting but not including vertex2; (0->1...)->vertex2.</param>
        //public static void AddEdgeOrUpdate(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId,
        //    uint[] sequence1, uint[] sequence2)
        //{
        //    var ignoreSequences = true;

        //    if (sequence1 == null || sequence1.Length < 0)
        //    {
        //        throw new ArgumentOutOfRangeException("sequence1");
        //    }
        //    if (sequence2 == null || sequence2.Length < 0)
        //    {
        //        throw new ArgumentOutOfRangeException("sequence2");
        //    }

        //    Func<uint[], uint[], bool> sequenceEquals = (s1, s2) =>
        //    {
        //        if (s1 == null || s1.Length == 0)
        //        {
        //            return s2 == null || s2.Length == 0;
        //        }
        //        if (s2 == null)
        //        {
        //            return false;
        //        }
        //        if (s1.Length == s2.Length)
        //        {
        //            for (var i = 0; i < s1.Length; i++)
        //            {
        //                if (s1[i] != s2[i])
        //                {
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }
        //        return false;
        //    };

        //    float forwardWeight = float.MaxValue;
        //    float backwardWeight = float.MaxValue;
        //    uint? forwardContractedId = null;
        //    uint? backwardContractedId = null;
        //    var forwardSequence1 = sequence1;
        //    var forwardSequence2 = sequence2;
        //    var backwardSequence1 = sequence1;
        //    var backwardSequence2 = sequence2;
        //    if (direction != null)
        //    {
        //        if (direction.Value)
        //        {
        //            forwardWeight = weight;
        //            forwardContractedId = contractedId;
        //        }
        //        else
        //        {
        //            backwardWeight = weight;
        //            backwardContractedId = contractedId;
        //        }
        //    }
        //    else
        //    {
        //        forwardWeight = weight;
        //        forwardContractedId = contractedId;
        //        backwardWeight = weight;
        //        backwardContractedId = contractedId;
        //    }

        //    var success = true;
        //    var enumerator = graph.GetEdgeEnumerator();
        //    while (success)
        //    {
        //        success = false;
        //        enumerator.MoveTo(vertex1);
        //        while (enumerator.MoveNext())
        //        {
        //            if (enumerator.Neighbour != vertex2)
        //            {
        //                continue;
        //            }

        //            if (enumerator.IsOriginal())
        //            {
        //                continue;
        //            }

        //            var s1 = enumerator.GetSequence1();
        //            var s2 = enumerator.GetSequence2();

        //            if (ignoreSequences ||
        //              (sequenceEquals(s1, sequence1) &&
        //               sequenceEquals(s2, sequence2)))
        //            {
        //                float edgeWeight = float.MaxValue;
        //                bool? edgeDirection = null;
        //                var edgeContractedId = enumerator.GetContracted();
        //                ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
        //                    out edgeWeight, out edgeDirection);
        //                if (edgeDirection == null || edgeDirection.Value)
        //                {
        //                    if (forwardWeight > edgeWeight)
        //                    {
        //                        forwardWeight = edgeWeight;
        //                        forwardContractedId = edgeContractedId;
        //                        if (ignoreSequences)
        //                        {
        //                            forwardSequence1 = s1;
        //                            forwardSequence2 = s2;
        //                        }
        //                    }
        //                }
        //                if (edgeDirection == null || !edgeDirection.Value)
        //                {
        //                    if (backwardWeight > edgeWeight)
        //                    {
        //                        backwardWeight = edgeWeight;
        //                        backwardContractedId = edgeContractedId;
        //                        if (ignoreSequences)
        //                        {
        //                            backwardSequence1 = s1;
        //                            backwardSequence2 = s2;
        //                        }
        //                    }
        //                }

        //                graph.RemoveEdgeById(vertex1, enumerator.Id);
        //                success = true;
        //                break;
        //            }
        //        }
        //    }

        //    if (forwardWeight == backwardWeight && forwardWeight != float.MaxValue && forwardContractedId == backwardContractedId &&
        //        forwardSequence1.IsSequenceIdentical(backwardSequence1) && forwardSequence2.IsSequenceIdentical(backwardSequence2))
        //    { // forward and backward are identical, add one bidirectional edge.
        //        graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId.Value, forwardSequence1, forwardSequence2);
        //    }
        //    else
        //    { // forward and backwards are different, add two edges if needed.
        //        if (forwardWeight != float.MaxValue)
        //        {
        //            graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId.Value, forwardSequence1, forwardSequence2);
        //        }
        //        if (backwardWeight != float.MaxValue)
        //        {
        //            graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId.Value, backwardSequence1, backwardSequence2);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Adds or updates a contracted edge including sequences.
        ///// </summary>
        ///// <param name="graph">The graph.</param>
        ///// <param name="vertex1">The start vertex.</param>
        ///// <param name="vertex2">The end vertex.</param>
        ///// <param name="contractedId">The vertex being shortcutted.</param>
        ///// <param name="direction">The direction.</param>
        ///// <param name="weight">The weight.</param>
        ///// <param name="sequence1">The relevant sequence starting but not including vertex1; vertex1->(0->1...).</param>
        ///// <param name="sequence2">The relevant sequence starting but not including vertex2; (0->1...)->vertex2.</param>
        //public static int TryAddEdgeOrUpdate(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId,
        //    uint[] sequence1, uint[] sequence2)
        //{
        //    Func<uint[], uint[], bool> sequenceEquals = (s1, s2) =>
        //    {
        //        if (s1 == null || s1.Length == 0)
        //        {
        //            return s2 == null || s2.Length == 0;
        //        }
        //        if (s2 == null)
        //        {
        //            return false;
        //        }
        //        if (s1.Length == s2.Length)
        //        {
        //            for (var i = 0; i < s1.Length; i++)
        //            {
        //                if (s1[i] != s2[i])
        //                {
        //                    return false;
        //                }
        //            }
        //            return true;
        //        }
        //        return false;
        //    };

        //    var removed = 0;
        //    var enumerator = graph.GetEdgeEnumerator();
        //    enumerator.MoveTo(vertex1);
        //    float forwardWeight = float.MaxValue;
        //    float backwardWeight = float.MaxValue;
        //    uint? forwardContractedId = null;
        //    uint? backwardContractedId = null;
        //    if (direction != null)
        //    {
        //        if (direction.Value)
        //        {
        //            forwardWeight = weight;
        //            forwardContractedId = contractedId;
        //        }
        //        else
        //        {
        //            backwardWeight = weight;
        //            backwardContractedId = contractedId;
        //        }
        //    }
        //    else
        //    {
        //        forwardWeight = weight;
        //        forwardContractedId = contractedId;
        //        backwardWeight = weight;
        //        backwardContractedId = contractedId;
        //    }

        //    while (enumerator.MoveNext())
        //    {
        //        if (enumerator.Neighbour != vertex2)
        //        {
        //            continue;
        //        }

        //        if (enumerator.IsOriginal())
        //        {
        //            continue;
        //        }

        //        var s1 = enumerator.GetSequence1();
        //        var s2 = enumerator.GetSequence2();

        //        //if (sequenceEquals(s1, sequence1) &&
        //        //    sequenceEquals(s2, sequence2))
        //        //{
        //            float edgeWeight = float.MaxValue;
        //            bool? edgeDirection = null;
        //            var edgeContractedId = enumerator.GetContracted();
        //            ContractedEdgeDataSerializer.Deserialize(enumerator.Data[0],
        //                out edgeWeight, out edgeDirection);
        //            if (edgeDirection == null || edgeDirection.Value)
        //            {
        //                if (forwardWeight > edgeWeight)
        //                {
        //                    forwardWeight = edgeWeight;
        //                    forwardContractedId = edgeContractedId;
        //                }
        //            }
        //            if (edgeDirection == null || !edgeDirection.Value)
        //            {
        //                if (backwardWeight > edgeWeight)
        //                {
        //                    backwardWeight = edgeWeight;
        //                    backwardContractedId = edgeContractedId;
        //                }
        //            }

        //            // graph.RemoveEdge(vertex1, enumerator.Id);
        //            removed++;
        //        //}
        //    }

        //    int added = 0;
        //    if (forwardWeight == backwardWeight && forwardWeight != float.MaxValue)
        //    {
        //        if (forwardContractedId == backwardContractedId)
        //        {
        //            //graph.AddEdge(vertex1, vertex2, forwardWeight, null, forwardContractedId, sequence1, sequence2);
        //            added++;
        //        }
        //        else
        //        {
        //            //graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, sequence1, sequence2);
        //            added++;
        //            //graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, sequence1, sequence2);
        //            added++;
        //        }
        //        return added - removed;
        //    }
        //    if (forwardWeight != float.MaxValue)
        //    {
        //        //graph.AddEdge(vertex1, vertex2, forwardWeight, true, forwardContractedId, sequence1, sequence2);
        //        added++;
        //    }
        //    if (backwardWeight != float.MaxValue)
        //    {
        //        //graph.AddEdge(vertex1, vertex2, backwardWeight, false, backwardContractedId, sequence1, sequence2);
        //        added++;
        //    }
        //    return added - removed;
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
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint? contractedId, uint[] sequence1, uint[] sequence2)
        {
            if (!contractedId.HasValue)
            {
                if ((sequence1 != null && sequence1.Length > 0) ||
                    (sequence2 != null && sequence2.Length > 0))
                {
                    throw new ArgumentException("Cannot add an edge without a contracted id but with start or end sequence.");
                }
                return graph.AddEdge(vertex1, vertex2, weight, direction);
            }
            else
            {
                if (sequence1 != null && sequence1.Length == 1 &&
                    sequence1[0] == contractedId.Value)
                {
                    sequence1 = null;
                }
                if (sequence2 != null && sequence2.Length == 1 &&
                    sequence2[0] == contractedId.Value)
                {
                    sequence2 = null;
                }
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
        /// Moves this enumerator to an edge (vertex1->vertex2) that has an end sequence that matches the given sequence.
        /// </summary>
        public static bool MoveToEdge<T>(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2, uint[] sequence2, Weights.WeightHandler<T> weightHandler, 
            bool direction, out T weight)
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
                if (xSequence2 == null || xSequence2.Length == 0)
                {
                    weight = xWeight;
                    return sequence2 == null || sequence2.Length == 0 ||
                        sequence2[0] == vertex1;
                }
                if (sequence2 == null || sequence2.Length == 0)
                {
                    continue;
                }
                for (var i = 0; i < System.Math.Min(sequence2.Length, xSequence2.Length); i++)
                {
                    if (sequence2[i] != xSequence2[i])
                    {
                        continue;
                    }
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