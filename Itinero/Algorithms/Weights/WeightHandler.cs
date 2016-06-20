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

using System;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Data.Contracted;
using Itinero.Data.Contracted.Edges;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// An abstract weight handler class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WeightHandler<T>
        where T : struct
    {
        /// <summary>
        /// Adds the weight to the given weight based on the given distance and edge profile.
        /// </summary>
        public abstract T Add(T weight, ushort edgeProfile, float distance, out Factor factor);

        /// <summary>
        /// Calculates the weight for the given edge and returns the factor.
        /// </summary>
        public abstract T Calculate(ushort edgeProfile, float distance, out Factor factor);

        /// <summary>
        /// Adds the two weights.
        /// </summary>
        public abstract T Add(T weight1, T weight2);

        /// <summary>
        /// Subtracts the two weights.
        /// </summary>
        public abstract T Subtract(T weight1, T weight2);

        /// <summary>
        /// Gets the actual metric the algorithm should be using to determine shortest paths.
        /// </summary>
        public abstract float GetMetric(T weight);

        /// <summary>
        /// Adds a new edge to a graph with the given direction and weight.
        /// </summary>
        public abstract void AddEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, 
            bool? direction, T weight);

        /// <summary>
        /// Adds or updates an edge.
        /// </summary>
        public abstract void AddOrUpdateEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, T weight);

        /// <summary>
        /// Adds a new edge to a graph with the given direction and weight.
        /// </summary>
        public abstract void AddEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, bool? direction, T weight);

        /// <summary>
        /// Adds or updates an edge.
        /// </summary>
        public abstract void AddOrUpdateEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, T weight, uint[] s1, uint[] s2);
        
        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public abstract T GetEdgeWeight(MetaEdge edge, out bool? direction);

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public abstract T GetEdgeWeight(DynamicEdge edge, out bool? direction);

        /// <summary>
        /// Returns the weight that represents 'zero'.
        /// </summary>
        /// <returns></returns>
        public abstract T Zero
        {
            get;
        }

        /// <summary>
        /// Returns the weight that represents 'infinite'.
        /// </summary>
        /// <returns></returns>
        public abstract T Infinite
        {
            get;
        }

        /// <summary>
        /// Returns true if the given contracted db can be used.
        /// </summary>
        public abstract bool CanUse(ContractedDb db);

        /// <summary>
        /// Gets the size of the meta-data in a directed meta graph when using this weight.
        /// </summary>
        public abstract int MetaSize
        {
            get;
        }

        /// <summary>
        /// Gets the size of the fixed parth in a dynamic directed graph when using this weight.
        /// </summary>
        public abstract int DynamicSize
        {
            get;
        }
    }

    /// <summary>
    /// A weight handler.
    /// </summary>
    public sealed class WeightHandler : WeightHandler<Weight>
    {
        private readonly Weight _infinite = new Weight()
        {
            Distance = float.MaxValue,
            Time = float.MaxValue,
            Value = float.MaxValue
        };
        private readonly Weight _zero = new Weight()
        {
            Distance = 0,
            Time = 0,
            Value = 0
        };
        private readonly Func<ushort, FactorAndSpeed> _getFactorAndSpeed;

        /// <summary>
        /// Creates a new weight handler.
        /// </summary>
        public WeightHandler(Func<ushort, FactorAndSpeed> getFactorAndSpeed)
        {
            _getFactorAndSpeed = getFactorAndSpeed;
        }

        /// <summary>
        /// Returns the weight that represents 'infinite'.
        /// </summary>
        /// <returns></returns>
        public sealed override Weight Infinite
        {
            get
            {
                return _infinite;
            }
        }

        /// <summary>
        /// Returns the weight that represents 'zero'.
        /// </summary>
        /// <returns></returns>
        public sealed override Weight Zero
        {
            get
            {
                return _zero;
            }
        }

        /// <summary>
        /// Adds the two weights.
        /// </summary>
        public sealed override Weight Add(Weight weight1, Weight weight2)
        {
            return new Weight()
            {
                Distance = weight1.Distance + weight2.Distance,
                Time = weight1.Time + weight2.Time,
                Value = weight1.Value + weight2.Value,
            };
        }

        /// <summary>
        /// Adds to the given weight based on the given edge profile and distance.
        /// </summary>
        public sealed override Weight Add(Weight weight, ushort edgeProfile, float distance, out Factor factor)
        {
            var factorAndSpeed = _getFactorAndSpeed(edgeProfile);
            factor = new Factor()
            {
                Direction = factorAndSpeed.Direction,
                Value = factorAndSpeed.Value
            };
            return new Weight()
            {
                Distance = weight.Distance + distance,
                Time = weight.Time + (distance * factorAndSpeed.SpeedFactor),
                Value = weight.Value + (distance * factorAndSpeed.Value)
            };
        }

        /// <summary>
        /// Calculates the weight for the given edge.
        /// </summary>
        public sealed override Weight Calculate(ushort edgeProfile, float distance, out Factor factor)
        {
            var factorAndSpeed = _getFactorAndSpeed(edgeProfile);
            factor = new Factor()
            {
                Direction = factorAndSpeed.Direction,
                Value = factorAndSpeed.Value
            };
            return new Weight()
            {
                Distance = distance,
                Time = (distance * factorAndSpeed.SpeedFactor),
                Value = (distance * factorAndSpeed.Value)
            };
        }

        /// <summary>
        /// Gets the metric for the given weight.
        /// </summary>
        public sealed override float GetMetric(Weight weight)
        {
            return weight.Value;
        }

        /// <summary>
        /// Subtracts the given weights.
        /// </summary>
        public sealed override Weight Subtract(Weight weight1, Weight weight2)
        {
            return new Weight()
            {
                Distance = weight1.Distance - weight2.Distance,
                Time = weight1.Time - weight2.Time,
                Value = weight1.Value - weight2.Value,
            };
        }

        /// <summary>
        /// Adds a new edge to a graph with the given direction and weight.
        /// </summary>
        public sealed override void AddEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, bool? direction, Weight weight)
        {
            if (graph.FixedEdgeDataSize != 3)
            {
                throw new InvalidOperationException("The given dynamic graph cannot handle augmented weights. Initialize the graph with a fixed edge data size of 3.");
            }

            var data = Data.Contracted.Edges.ContractedEdgeDataSerializer.SerializeDynamicAugmented(
                weight.Value, direction, weight.Distance, weight.Time);
            graph.AddEdge(vertex1, vertex2, data);
        }

        /// <summary>
        /// Adds or updates and edge.
        /// </summary>
        public override void AddOrUpdateEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, Weight weight, uint[] s1, uint[] s2)
        {
            graph.AddOrUpdateEdge(vertex1, vertex2, weight.Value, direction, contractedId, weight.Distance, weight.Time, s1, s2);
        }

        /// <summary>
        /// Adds or updates and edge.
        /// </summary>
        public override void AddOrUpdateEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, Weight weight)
        {
            graph.AddOrUpdateEdge(vertex1, vertex2, weight.Value, direction, contractedId, weight.Distance, weight.Time);
        }

        /// <summary>
        /// Adds a new edge to a graph with the given direction and weight.
        /// </summary>
        public sealed override void AddEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, Weight weight)
        {
            if (graph.EdgeDataSize != 3)
            {
                throw new InvalidOperationException("The given graph cannot handle augmented weights. Initialize the graph with a edge meta data size of 3.");
            }
            graph.AddEdge(vertex1, vertex2, new uint[] { Data.Contracted.Edges.ContractedEdgeDataSerializer.Serialize(weight.Value, direction) },
                Data.Contracted.Edges.ContractedEdgeDataSerializer.SerializeMetaAugmented(contractedId, weight.Distance, weight.Time));
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override Weight GetEdgeWeight(MetaEdge edge, out bool? direction)
        {
            float weight;
            float time;
            float distance;
            uint contractedId;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data[0],
                out weight, out direction);
            Data.Contracted.Edges.ContractedEdgeDataSerializer.DeserializeMetaAgumented(edge.MetaData,
                out contractedId, out distance, out time);
            return new Weight()
            {
                Distance = distance,
                Time = time,
                Value = weight
            };
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override Weight GetEdgeWeight(DynamicEdge edge, out bool? direction)
        {
            float weight;
            float time;
            float distance;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.DeserializeDynamic(edge.Data,
                out weight, out direction, out distance, out time);
            return new Weight()
            {
                Distance = distance,
                Time = time,
                Value = weight
            };
        }

        /// <summary>
        /// Returns true if the given contracted db can be used.
        /// </summary>
        public sealed override bool CanUse(ContractedDb db)
        {
            if (db.HasEdgeBasedGraph)
            {
                return db.EdgeBasedGraph.FixedEdgeDataSize == ContractedEdgeDataSerializer.DynamicAugmentedFixedSize;
            }
            return db.NodeBasedGraph.EdgeDataSize == ContractedEdgeDataSerializer.MetaAugmentedSize;
        }

        /// <summary>
        /// Gets the size of the fixed parth in a dynamic directed graph when using this weight.
        /// </summary>
        public sealed override int DynamicSize
        {
            get
            {
                return ContractedEdgeDataSerializer.DynamicAugmentedFixedSize;
            }
        }

        /// <summary>
        /// Gets the size of the meta-data in a directed meta graph when using this weight.
        /// </summary>
        public sealed override int MetaSize
        {
            get
            {
                return ContractedEdgeDataSerializer.MetaAugmentedSize;
            }
        }
    }
}