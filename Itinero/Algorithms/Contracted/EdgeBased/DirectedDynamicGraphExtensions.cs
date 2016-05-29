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

using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;

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
            if (edge.DynamicData == null)
            {
                throw new ArgumentException("DynamicData array of an edge should not be null.");
            }
            return edge.DynamicData.Length == 0;
        }

        /// <summary>
        /// Gets the contracted id, returns null if this edge is not a shortcut.
        /// </summary>
        public static uint? GetContracted(this DirectedDynamicGraph.EdgeEnumerator edge)
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
            { // only a contracted id, no sequences.
                return EMPTY;
            }
            var size = dynamicData[1];
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
            { // only a contracted id, no sequences.
                return EMPTY;
            }
            var size = dynamicData[1];
            var sequence = new uint[size];
            for (var i = 0; i < size; i++)
            {
                sequence[i] = dynamicData[i + 2];
            }
            return sequence;
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
            { // only a contracted id, no sequences.
                return EMPTY;
            }
            var size = dynamicData[1];
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
            { // only a contracted id, no sequences.
                return EMPTY;
            }
            var size = dynamicData[1];
            var sequence = new uint[dynamicData.Length - 2 - size];
            for (var i = 0; i < sequence.Length; i++)
            {
                sequence[i] = dynamicData[size + 2];
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
        /// Adds an edge.
        /// </summary>
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction)
        {
            return graph.AddEdge(vertex1, vertex2, ContractedEdgeDataSerializer.Serialize(weight, direction));
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
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId, uint[] sequence1, uint[] sequence2)
        {
            var dataSize = 2; // Fixed weight + contracted id.
            if (sequence1 != null && sequence1.Length != 0)
            {
                dataSize += 1; // size field.
                dataSize += sequence1.Length;
            }
            if (sequence2 != null && sequence2.Length != 0)
            {
                if (sequence1 == null && sequence1.Length == 0)
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
                if (sequence1 == null && sequence1.Length != 0)
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
        public static void MoveToEdge(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2, uint[] sequence2)
        {
            enumerator.MoveTo(vertex1);
            enumerator.MoveNextUntil(x => x.Neighbour == vertex2);
        }

        /// <summary>
        /// Gets all edges starting at this edges.
        /// </summary>
        public static List<DynamicEdge> GetEdges(this DirectedDynamicGraph graph, uint vertex)
        {
            return new List<DynamicEdge>(graph.GetEdgeEnumerator(vertex));
        }
    }
}