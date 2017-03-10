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

using Itinero.Algorithms.PriorityQueues;
using Itinero.Data.Contracted;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.Witness
{
    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public class DykstraWitnessCalculator : IWitnessCalculator
    {
        private readonly BinaryHeap<SettledVertex> _heap;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit)
            : this(hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit, int maxSettles)
        {
            _hopLimit = hopLimit;

            _heap = new BinaryHeap<SettledVertex>();
            _maxSettles = maxSettles;
        }
        
        private int _hopLimit;
        private int _maxSettles;

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public void Calculate(DirectedGraph graph, uint source, List<uint> targets, List<float> weights,
            ref bool[] forwardWitness, ref bool[] backwardWitness, uint vertexToSkip)
        {
            if(_hopLimit == 1)
            {
                this.ExistsOneHop(graph, source, targets, weights, ref forwardWitness, ref backwardWitness);
                return;
            }

            // creates the settled list.
            var backwardSettled = new HashSet<uint>();
            var forwardSettled = new HashSet<uint>();
            var backwardTargets = new HashSet<uint>();
            var forwardTargets = new HashSet<uint>();
            float forwardMaxWeight = 0, backwardMaxWeight = 0;
            for (int idx = 0; idx < weights.Count; idx++)
            {
                if (!forwardWitness[idx])
                {
                    forwardTargets.Add(targets[idx]);
                    if (forwardMaxWeight < weights[idx])
                    {
                        forwardMaxWeight = weights[idx];
                    }
                }
                if (!backwardWitness[idx])
                {
                    backwardTargets.Add(targets[idx]);
                    if (backwardMaxWeight < weights[idx])
                    {
                        backwardMaxWeight = weights[idx];
                    }
                }
            }
            if (forwardMaxWeight == 0 && backwardMaxWeight == 0)
            { // no need to search!
                return;
            }

            // creates the priorty queue.
            var forwardMinWeight = new Dictionary<uint, float>();
            var backwardMinWeight = new Dictionary<uint, float>();
            _heap.Clear();
            _heap.Push(new SettledVertex(source, 0, 0, forwardMaxWeight > 0, backwardMaxWeight > 0), 0);

            // keep looping until the queue is empty or the target is found!
            var edgeEnumerator = graph.GetEdgeEnumerator();
            while (_heap.Count > 0)
            { // pop the first customer.
                var current = _heap.Pop();
                if (current.Hops + 1 < _hopLimit)
                {
                    if (current.VertexId == vertexToSkip)
                    { // this is the vertex being contracted.
                        continue;
                    }
                    var forwardWasSettled = forwardSettled.Contains(current.VertexId);
                    var backwardWasSettled = backwardSettled.Contains(current.VertexId);
                    if (forwardWasSettled && backwardWasSettled)
                    { // both are already settled.
                        continue;
                    }

                    if (current.Forward)
                    { // this is a forward settle.
                        forwardSettled.Add(current.VertexId);
                        forwardMinWeight.Remove(current.VertexId);
                        if (forwardTargets.Contains(current.VertexId))
                        {
                            for(var i = 0; i < targets.Count; i++)
                            {
                                if (targets[i] == current.VertexId)
                                {
                                    forwardWitness[i] = current.Weight < weights[i];
                                    forwardTargets.Remove(current.VertexId);
                                }
                            }
                        }
                    }
                    if (current.Backward)
                    { // this is a backward settle.
                        backwardSettled.Add(current.VertexId);
                        backwardMinWeight.Remove(current.VertexId);
                        if (backwardTargets.Contains(current.VertexId))
                        {
                            for(var i = 0; i < targets.Count; i++)
                            {
                                if(targets[i] == current.VertexId)
                                {
                                    backwardWitness[i] = current.Weight < weights[i];
                                    backwardTargets.Remove(current.VertexId);
                                }
                            }
                        }
                    }

                    if (forwardTargets.Count == 0 &&
                        backwardTargets.Count == 0)
                    { // there is nothing left to check.
                        break;
                    }

                    if (forwardSettled.Count >= _maxSettles &&
                        backwardSettled.Count >= _maxSettles)
                    { // do not continue searching.
                        break;
                    }

                    var doForward = current.Forward && forwardTargets.Count > 0 && !forwardWasSettled;
                    var doBackward = current.Backward && backwardTargets.Count > 0 && !backwardWasSettled;
                    if (doForward || doBackward)
                    { // get the neighbours.
                        edgeEnumerator.MoveTo(current.VertexId);
                        while (edgeEnumerator.MoveNext())
                        { // move next.
                            var neighbour = edgeEnumerator.Neighbour;

                            float neighbourWeight;
                            bool? neighbourDirection;
                            ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                                out neighbourWeight, out neighbourDirection);
                            var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                            var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                            var totalNeighbourWeight = current.Weight + neighbourWeight;
                            var doNeighbourForward = doForward && neighbourCanMoveForward && totalNeighbourWeight < forwardMaxWeight &&
                                !forwardSettled.Contains(neighbour);
                            var doNeighbourBackward = doBackward && neighbourCanMoveBackward && totalNeighbourWeight < backwardMaxWeight &&
                                !backwardSettled.Contains(neighbour);
                            if (doNeighbourBackward || doNeighbourForward)
                            {
                                float existingWeight;
                                if (doNeighbourForward)
                                {
                                    if (forwardMinWeight.TryGetValue(neighbour, out existingWeight))
                                    {
                                        if (existingWeight <= totalNeighbourWeight)
                                        {
                                            doNeighbourForward = false;
                                        }
                                        else
                                        {
                                            forwardMinWeight[neighbour] = totalNeighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        forwardMinWeight[neighbour] = totalNeighbourWeight;
                                    }
                                }
                                if (doNeighbourBackward)
                                {
                                    if (backwardMinWeight.TryGetValue(neighbour, out existingWeight))
                                    {
                                        if (existingWeight <= totalNeighbourWeight)
                                        {
                                            doNeighbourBackward = false;
                                        }
                                        else
                                        {
                                            backwardMinWeight[neighbour] = totalNeighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        backwardMinWeight[neighbour] = totalNeighbourWeight;
                                    }
                                }

                                if (doNeighbourBackward || doNeighbourForward)
                                { // add to heap.
                                    var newSettle = new SettledVertex(neighbour,
                                        totalNeighbourWeight, current.Hops + 1, doNeighbourForward, doNeighbourBackward);
                                    _heap.Push(newSettle, newSettle.Weight);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates witness paths with just one hop.
        /// </summary>
        public void ExistsOneHop(DirectedGraph graph, uint source, List<uint> targets, List<float> weights,
            ref bool[] forwardExists, ref bool[] backwardExists)
        {
            var targetsToCalculate = new HashSet<uint>();
            var maxWeight = 0.0f;
            for (int idx = 0; idx < weights.Count; idx++)
            {
                if (!forwardExists[idx] || !backwardExists[idx])
                {
                    targetsToCalculate.Add(targets[idx]);
                    if (maxWeight < weights[idx])
                    {
                        maxWeight = weights[idx];
                    }
                }
            }

            if (targetsToCalculate.Count > 0)
            {
                var edgeEnumerator = graph.GetEdgeEnumerator(source);
                while (edgeEnumerator.MoveNext())
                {
                    var neighbour = edgeEnumerator.Neighbour;
                    if (targetsToCalculate.Contains(neighbour))
                    { // ok, this is a to-edge.
                        var index = targets.IndexOf(neighbour);
                        targetsToCalculate.Remove(neighbour);

                        float neighbourWeight;
                        bool? neighbourDirection;
                        uint neighbourContractedId;
                        ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, edgeEnumerator.Data1,
                            out neighbourWeight, out neighbourDirection, out neighbourContractedId);
                        var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                        var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                        if (neighbourCanMoveForward &&
                            neighbourWeight < weights[index])
                        {
                            forwardExists[index] = true;
                        }
                        if (neighbourCanMoveBackward &&
                            neighbourWeight < weights[index])
                        {
                            backwardExists[index] = true;
                        }

                        if (targetsToCalculate.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the hop limit.
        /// </summary>
        public int HopLimit
        {
            get
            {
                return _hopLimit;
            }
            set
            {
                _hopLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the max settles.
        /// </summary>
        public int MaxSettles
        {
            get
            {
                return _maxSettles;
            }
            set
            {
                _maxSettles = value;
            }
        }

        /// <summary>
        /// Represents a settled vertex.
        /// </summary>
        private class SettledVertex
        {
            /// <summary>
            /// Creates a new settled vertex.
            /// </summary>
            public SettledVertex(uint vertex, float weight, uint hops, bool forward, bool backward)
            {
                this.VertexId = vertex;
                this.Weight = weight;
                this.Hops = hops;
                this.Forward = forward;
                this.Backward = backward;
            }

            /// <summary>
            /// The vertex that was settled.
            /// </summary>
            public uint VertexId { get; set; }

            /// <summary>
            /// The weight this vertex was settled at.
            /// </summary>
            public float Weight { get; set; }

            /// <summary>
            /// The hop-count of this vertex.
            /// </summary>
            public uint Hops { get; set; }

            /// <summary>
            /// Holds the forward flag.
            /// </summary>
            public bool Forward { get; set; }

            /// <summary>
            /// Holds the backward flag.
            /// </summary>
            public bool Backward { get; set; }
        }
    }
}