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
        public abstract void AddOrUpdateEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, T weight, uint s1, uint s2);

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public T GetEdgeWeight(MetaEdge edge, out bool? direction)
        {
            var weightAndDir = this.GetEdgeWeight(edge);
            direction = weightAndDir.Direction.AsNullableBool();
            return weightAndDir.Weight;
        }

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public abstract WeightAndDir<T> GetEdgeWeight(MetaEdge edge);

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public T GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge, out bool? direction)
        {
            var weightAndDir = this.GetEdgeWeight(edge);
            direction = weightAndDir.Direction.AsNullableBool();
            return weightAndDir.Weight;
        }

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public abstract WeightAndDir<T> GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge);

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public T GetEdgeWeight(DynamicEdge edge, out bool? direction)
        {
            var weightAndDir = this.GetEdgeWeight(edge);
            direction = weightAndDir.Direction.AsNullableBool();
            return weightAndDir.Weight;
        }

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public abstract WeightAndDir<T> GetEdgeWeight(DynamicEdge edge);

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public T GetEdgeWeight(DirectedDynamicGraph.EdgeEnumerator edge, out bool? direction)
        {
            var weightAndDir = this.GetEdgeWeight(edge);
            direction = weightAndDir.Direction.AsNullableBool();
            return weightAndDir.Weight;
        }

        /// <summary>
        /// Gets the weight from a meta-edge.
        /// </summary>
        public abstract WeightAndDir<T> GetEdgeWeight(DirectedDynamicGraph.EdgeEnumerator edge);

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

        /// <summary>
        /// Returns true if the given weight is smaller than all of fields in max.-
        /// </summary>
        public abstract bool IsSmallerThanAny(T weight, T max);
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
            factor = factorAndSpeed.ToFactor();
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
            factor = factorAndSpeed.ToFactor();
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
        public override void AddOrUpdateEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, Weight weight, uint s1, uint s2)
        {
            graph.AddOrUpdateEdge(vertex1, vertex2, weight.Value, weight.Distance, weight.Time, direction, contractedId, s1, s2);
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
        public sealed override WeightAndDir<Weight> GetEdgeWeight(MetaEdge edge)
        {
            float time;
            float distance;
            uint contractedId;
            var baseWeight = Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data[0]);
            Data.Contracted.Edges.ContractedEdgeDataSerializer.DeserializeMetaAgumented(edge.MetaData,
                out contractedId, out distance, out time);
            return new WeightAndDir<Weight>()
            {
                Weight = new Weight()
                {
                    Distance = distance,
                    Time = time,
                    Value = baseWeight.Weight
                },
                Direction = baseWeight.Direction
            };
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override WeightAndDir<Weight> GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge)
        {
            float time;
            float distance;
            uint contractedId;
            var baseWeight = Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge.Data0);
            Data.Contracted.Edges.ContractedEdgeDataSerializer.DeserializeMetaAgumented(edge.MetaData,
                out contractedId, out distance, out time);
            return new WeightAndDir<Weight>()
            {
                Weight = new Weight()
                {
                    Distance = distance,
                    Time = time,
                    Value = baseWeight.Weight
                },
                Direction = baseWeight.Direction
            };
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override WeightAndDir<Weight> GetEdgeWeight(DynamicEdge edge)
        {
            bool? direction;
            float weight;
            float time;
            float distance;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.DeserializeDynamic(edge.Data,
                out weight, out direction, out distance, out time);
            return new WeightAndDir<Weight>()
            {
                Weight = new Weight()
                {
                    Distance = distance,
                    Time = time,
                    Value = weight
                },
                Direction = new Dir(direction == null || direction.Value,
                    direction == null || !direction.Value)
            };
        }

        /// <summary>
        /// Gets the weight from the given edge and sets the direction.
        /// </summary>
        public sealed override WeightAndDir<Weight> GetEdgeWeight(DirectedDynamicGraph.EdgeEnumerator edge)
        {
            bool? direction;
            float weight;
            float time;
            float distance;
            Data.Contracted.Edges.ContractedEdgeDataSerializer.DeserializeDynamic(edge.Data,
                out weight, out direction, out distance, out time);
            return new WeightAndDir<Weight>()
            {
                Weight = new Weight()
                {
                    Distance = distance,
                    Time = time,
                    Value = weight
                },
                Direction = new Dir(direction == null || direction.Value,
                    direction == null || !direction.Value)
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

        /// <summary>
        /// Returns true if the given weight is smaller than all of fields in max.-
        /// </summary>
        public sealed override bool IsSmallerThanAny(Weight weight, Weight max)
        {
            return weight.Value < max.Value &&
                weight.Time < max.Time &&
                weight.Distance < max.Distance;
        }
    }
}