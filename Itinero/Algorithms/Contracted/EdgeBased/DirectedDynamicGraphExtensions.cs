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

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains contraction related extension methods related to the directed dynamic graph.
    /// </summary>
    public static class DirectedDynamicGraphExtensions
    {
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
            return edge.DynamicData[1];
        }

        /// <summary>
        /// Gets the sequence at the source.
        /// </summary>
        public static uint[] GetSequence(this DynamicEdge edge)
        {
            if (edge.IsOriginal())
            {
                return new uint[] { edge.Neighbour };
            }

            if (edge.DynamicData == null || edge.DynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (edge.DynamicData.Length < 2)
            {
                return new uint[0];
            }
            var size = edge.DynamicData[2];
            var sequence = new uint[size];
            for (var i = 0; i < size; i++)
            {
                sequence[i] = edge.DynamicData[i + 2];
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
                return new uint[] { edge.Neighbour };
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            {
                return new uint[0];
            }
            var size = dynamicData[2];
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
        public static uint[] GetSequence2(this DirectedDynamicGraph.EdgeEnumerator edge)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return null;
            }

            if (edge.DynamicData == null || edge.DynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (edge.DynamicData.Length < 2)
            {
                return new uint[0];
            }
            var size = edge.DynamicData[2];
            var sequence = new uint[edge.DynamicData.Length - 2 - size];
            for (var i = 0; i < sequence.Length; i++)
            {
                sequence[i] = edge.DynamicData[size + 2];
            }
            return sequence;
        }

        /// <summary>
        /// Gets the sequence at the source including the source vertex.
        /// </summary>
        public static uint[] GetSequence1WithSource(this DirectedDynamicGraph.EdgeEnumerator edge, uint sourceVertex)
        {
            if (edge.IsOriginal())
            {
                return new uint[] { sourceVertex, edge.Neighbour };
            }

            var dynamicData = edge.DynamicData;
            if (dynamicData == null || dynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (dynamicData.Length < 2)
            {
                return new uint[] { sourceVertex };
            } 
            var size = dynamicData[2];
            var sequence = new uint[size + 1];
            sequence[0] = sourceVertex;
            for (var i = 0; i < size; i++)
            {
                sequence[i + 1] = dynamicData[i + 2];
            }
            return sequence;
        }

        /// <summary>
        /// Gets the sequence at the target.
        /// </summary>
        public static uint[] GetSequence2ReverseWithSource(this DynamicEdge edge, uint sourceVertex)
        {
            if (edge.IsOriginal())
            { // sequence is the source 
                return new uint[] { sourceVertex };
            }

            if (edge.DynamicData == null || edge.DynamicData.Length < 1)
            {
                throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
            }
            if (edge.DynamicData.Length < 2)
            {
                return new uint[0];
            }
            var size = edge.DynamicData[2];
            var sequence = new uint[edge.DynamicData.Length - 2 - size];
            for (var i = 0; i < sequence.Length; i++)
            {
                sequence[i] = edge.DynamicData[size + 2];
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
        public static uint AddEdge(this DirectedDynamicGraph graph, uint vertex1, uint vertex2, float weight, bool? direction, uint contractedId, uint[] sequence1, uint[] sequence2)
        {
            var dataSize = 2;
            if (sequence1 != null)
            {
                dataSize += 1;
                dataSize += sequence1.Length;
            }
            if (sequence2 != null)
            {
                if (sequence1 == null)
                {
                    dataSize += 1;
                }
                dataSize += sequence2.Length;
            }
            var data = new uint[dataSize];
            data[0] = ContractedEdgeDataSerializer.Serialize(weight, direction);
            data[1] = contractedId;
            if (sequence1 != null)
            {
                data[2] = (uint)sequence1.Length;
                sequence1.CopyTo(data, 3);
            }
            if (sequence2 != null)
            {
                var sequence2Start = 3;
                if (sequence1 == null)
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
    }
}