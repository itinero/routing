// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Profiles;
using System;
using Itinero.Graphs.Directed;
using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Data.Contracted;
using Itinero.Data.Contracted.Edges;

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
        public sealed override float GetEdgeWeight(DynamicEdge edge, out bool? direction)
        {
            float weight;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data[0],
                out weight, out direction);
            return weight;
        }

        /// <summary>
        /// Returns true if the given contracted db can be used.
        /// </summary>
        public sealed override bool CanUse(ContractedDb db)
        {
            if (db.HasEdgeBasedGraph)
            {
                return db.EdgeBasedGraph.FixedEdgeDataSize == ContractedEdgeDataSerializer.DynamicFixedSize;
            }
            return db.NodeBasedGraph.EdgeDataSize == ContractedEdgeDataSerializer.MetaSize;
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
    }
}