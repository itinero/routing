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

using Itinero.Profiles;
using System;
using Itinero.Graphs.Directed;
using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Data.Contracted;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Collections;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// A default weight handler.
    /// </summary>
    public sealed class DefaultWeightHandler : WeightHandler<float>
    {
        private Func<ushort, Factor> _getFactor;

        /// <summary>
        /// Creates a new default weight handler.
        /// </summary>
        public DefaultWeightHandler(Func<ushort, Factor> getFactor)
        {
            _getFactor = getFactor;
        }

        /// <summary>
        /// Returns the weight that represents 'zero'.
        /// </summary>
        /// <returns></returns>
        public sealed override float Zero
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Returns the weight that represents 'infinite'.
        /// </summary>
        /// <returns></returns>
        public sealed override float Infinite
        {
            get
            {
                return float.MaxValue;
            }
        }

        /// <summary>
        /// Adds the two weights.
        /// </summary>
        public sealed override float Add(float weight1, float weight2)
        {
            return weight1 + weight2;
        }

        /// <summary>
        /// Subtracts the two weights.
        /// </summary>
        public sealed override float Subtract(float weight1, float weight2)
        {
            return weight1 - weight2;
        }

        /// <summary>
        /// Calculates the weight for the given edge and distance.
        /// </summary>
        public sealed override float Calculate(ushort edgeProfile, float distance, out Factor factor)
        {
            factor = _getFactor(edgeProfile);
            return (distance * factor.Value);
        }

        /// <summary>
        /// Calculates the weight and direction for the given edge profile.
        /// </summary>
        public sealed override WeightAndDir<float> CalculateWeightAndDir(ushort edgeProfile, float distance)
        {
            bool accessible;
            return this.CalculateWeightAndDir(edgeProfile, distance, out accessible);
        }

        /// <summary>
        /// Calculates the weight and direction for the given edge profile.
        /// </summary>
        public sealed override WeightAndDir<float> CalculateWeightAndDir(ushort edgeProfile, float distance, out bool accessible)
        {
            var factor = _getFactor(edgeProfile);
            var weight = new WeightAndDir<float>();
            if (factor.Direction == 0)
            {
                weight.Direction = new Dir(true, true);
            }
            else if (factor.Direction == 1)
            {
                weight.Direction = new Dir(true, false);
            }
            else
            {
                weight.Direction = new Dir(false, true);
            }

            weight.Weight = distance * factor.Value;
            accessible = factor.Value != 0;
            return weight;
        }

        /// <summary>
        /// Adds weight to given weight based on the given distance and profile.
        /// </summary>
        public sealed override float Add(float weight, ushort edgeProfile, float distance, out Factor factor)
        {
            factor = _getFactor(edgeProfile);
            return weight + (distance * factor.Value);
        }

        /// <summary>
        /// Gets the actual metric the algorithm should be using to determine shortest paths.
        /// </summary>
        public sealed override float GetMetric(float weight)
        {
            return weight;
        }

        /// <summary>
        /// Adds a new edge with the given direction and weight.
        /// </summary>
        public sealed override void AddEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, float weight)
        {
            var data = Data.Contracted.Edges.ContractedEdgeDataSerializer.Serialize(
                weight, direction);
            graph.AddEdge(vertex1, vertex2, data, contractedId);
        }

        /// <summary>
        /// Adds or updates an edge.
        /// </summary>
        public sealed override void AddOrUpdateEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, float weight)
        {
            graph.AddOrUpdateEdge(vertex1, vertex2, weight, direction, contractedId);
        }

        /// <summary>
        /// Adds or updates an edge.
        /// </summary>
        public sealed override void AddOrUpdateEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, float weight, uint[] s1, uint[] s2)
        {
            graph.AddOrUpdateEdge(vertex1, vertex2, weight, direction, contractedId, s1, s2);
        }

        /// <summary>
        /// Adds a new edge with the given direction and weight.
        /// </summary>
        public sealed override void AddEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, bool? direction, float weight)
        {
            var data = Data.Contracted.Edges.ContractedEdgeDataSerializer.Serialize(
                weight, direction);
            graph.AddEdge(vertex1, vertex2, data);
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override float GetEdgeWeight(MetaEdge edge, out bool? direction)
        {
            float weight;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data[0],
                out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override WeightAndDir<float> GetEdgeWeight(MetaEdge edge)
        {
            return Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data[0]);
        }

        /// <summary>
        /// Adds a vertex to the path tree.
        /// </summary>
        public override uint AddPathTree(PathTree tree, uint vertex, float weight, uint previous)
        {
            return tree.Add(vertex, (uint)(weight * 10.0f), previous);
        }

        /// <summary>
        /// Gets a vertex from the path tree.
        /// </summary>
        public override void GetPathTree(PathTree tree, uint pointer, out uint vertex, out float weight, out uint previous)
        {
            uint data1;
            tree.Get(pointer, out vertex, out data1, out previous);
            weight = data1 / 10.0f;
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override float GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge, out bool? direction)
        {
            float weight;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data0,
                out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override WeightAndDir<float> GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge)
        {
            return Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data0);
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override float GetEdgeWeight(DynamicEdge edge, out bool? direction)
        {
            float weight;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data[0],
                out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override float GetEdgeWeight(DirectedDynamicGraph.EdgeEnumerator edge, out bool? direction)
        {
            float weight;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data0,
                out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Returns true if the given contracted db can be used.
        /// </summary>
        public sealed override bool CanUse(ContractedDb db)
        { // any current layout can be used with default weights.
            return true;
        }

        /// <summary>
        /// Gets the size of the fixed parth in a dynamic directed graph when using this weight.
        /// </summary>
        public sealed override int DynamicSize
        {
            get
            {
                return ContractedEdgeDataSerializer.DynamicFixedSize;
            }
        }

        /// <summary>
        /// Gets the size of the meta-data in a directed meta graph when using this weight.
        /// </summary>
        public sealed override int MetaSize
        {
            get
            {
                return ContractedEdgeDataSerializer.MetaSize;
            }
        }

        /// <summary>
        /// Returns true if the given weight is smaller than all of fields in max.-
        /// </summary>
        public sealed override bool IsSmallerThanAny(float weight, float max)
        {
            return weight < max;
        }
    }
}