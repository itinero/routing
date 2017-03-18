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

using System.Collections.Generic;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs.Directed;
using System;
using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.EdgeBased.Witness
{
    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public class DykstraWitnessCalculator<T> : IWitnessCalculator<T>
        where T : struct
    {
        private readonly BinaryHeap<SettledEdge> _heap;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(WeightHandler<T> weightHandler, int hopLimit)
            : this (weightHandler, hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(WeightHandler<T> weightHandler, int hopLimit, int maxSettles)
        {
            _hopLimit = hopLimit;
            _weightHandler = weightHandler;

            _heap = new BinaryHeap<SettledEdge>();
            _maxSettles = maxSettles;
        }

        private int _hopLimit;
        private int _maxSettles;
        private uint[] _sequence1 = new uint[16];

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public void Calculate(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, uint source, List<uint> targets, List<T> weights,
            ref EdgePath<T>[] forwardWitness, ref EdgePath<T>[] backwardWitness, uint vertexToSkip)
        {
            if (_hopLimit == 1)
            {
                this.ExistsOneHop(graph, source, targets, weights, ref forwardWitness, ref backwardWitness);
                return;
            }

            // creates the settled list.
            var s = new List<uint>();
            var backwardSettled = new HashSet<EdgePath<T>>();
            var forwardSettled = new HashSet<EdgePath<T>>();
            var backwardTargets = new HashSet<uint>();
            var forwardTargets = new HashSet<uint>();
            T forwardMaxWeight = _weightHandler.Zero, backwardMaxWeight = _weightHandler.Zero;
            for (int idx = 0; idx < weights.Count; idx++)
            {
                if (forwardWitness[idx] == null)
                {
                    forwardWitness[idx] = new EdgePath<T>();
                    forwardTargets.Add(targets[idx]);
                    if (_weightHandler.IsSmallerThan(forwardMaxWeight, weights[idx]))
                    {
                        forwardMaxWeight = weights[idx];
                    }
                }
                if (backwardWitness[idx] == null)
                {
                    backwardWitness[idx] = new EdgePath<T>();
                    backwardTargets.Add(targets[idx]);
                    if (_weightHandler.IsSmallerThan(backwardMaxWeight, weights[idx]))
                    {
                        backwardMaxWeight = weights[idx];
                    }
                }
            }
            
            if (_weightHandler.GetMetric(forwardMaxWeight) == 0 && 
                _weightHandler.GetMetric(backwardMaxWeight) == 0)
            { // no need to search!
                return;
            }

            // creates the priorty queue.
            var forwardMinWeight = new Dictionary<EdgePath<T>, T>();
            var backwardMinWeight = new Dictionary<EdgePath<T>, T>();
            _heap.Clear();
            _heap.Push(new SettledEdge(new EdgePath<T>(source), 0, _weightHandler.GetMetric(forwardMaxWeight) > 0, _weightHandler.GetMetric(backwardMaxWeight) > 0), 0);

            // keep looping until the queue is empty or the target is found!
            var edgeEnumerator = graph.GetEdgeEnumerator();
            while (_heap.Count > 0)
            { // pop the first customer.
                var current = _heap.Pop();
                if (current.Hops + 1 < _hopLimit)
                {
                    if (current.Path.Vertex == vertexToSkip)
                    { // this is the vertex being contracted.
                        continue;
                    }
                    var forwardWasSettled = forwardSettled.Contains(current.Path);
                    var backwardWasSettled = backwardSettled.Contains(current.Path);
                    if (forwardWasSettled && backwardWasSettled)
                    { // both are already settled.
                        continue;
                    }

                    if (current.Forward)
                    { // this is a forward settle.
                        forwardSettled.Add(current.Path);
                        forwardMinWeight.Remove(current.Path);
                        if (forwardTargets.Contains(current.Path.Vertex))
                        {
                            for (var i = 0; i < targets.Count; i++)
                            {
                                if (targets[i] == current.Path.Vertex &&
                                    _weightHandler.IsSmallerThanOrEqual(current.Path.Weight, weights[i]))
                                { // TODO: check if this is a proper stop condition.
                                    if (forwardWitness[i] == null || forwardWitness[i].Vertex == Constants.NO_VERTEX ||
                                        _weightHandler.IsSmallerThan(current.Path.Weight, forwardWitness[i].Weight))
                                    {
                                        forwardWitness[i] = current.Path;
                                        forwardTargets.Remove(current.Path.Vertex);
                                    }
                                }
                            }
                        }
                    }
                    if (current.Backward)
                    { // this is a backward settle.
                        backwardSettled.Add(current.Path);
                        backwardMinWeight.Remove(current.Path);
                        if (backwardTargets.Contains(current.Path.Vertex))
                        {
                            for (var i = 0; i < targets.Count; i++)
                            {
                                if (targets[i] == current.Path.Vertex &&
                                    _weightHandler.IsSmallerThanOrEqual(current.Path.Weight, weights[i]))
                                { // TODO: check if this is a proper stop condition.
                                    if (backwardWitness[i] == null || backwardWitness[i].Vertex == Constants.NO_VERTEX || 
                                        _weightHandler.IsSmallerThan(current.Path.Weight, backwardWitness[i].Weight))
                                    {
                                        backwardWitness[i] = current.Path;
                                        backwardTargets.Remove(current.Path.Vertex);
                                    }
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

                        // check for a restriction and if need build the original sequence.
                        var restrictions = getRestrictions(current.Path.Vertex);
                        var sequence = current.Path.GetSequence2(edgeEnumerator, int.MaxValue, s);
                        sequence = sequence.Append(current.Path.Vertex);

                        // move to the current vertex.
                        edgeEnumerator.MoveTo(current.Path.Vertex);
                        //uint neighbour, data0, data1;
                        while (edgeEnumerator.MoveNext())
                        { // move next.
                            var neighbour = edgeEnumerator.Neighbour;
                            //edgeEnumerator.GetData(out neighbour, out data0, out data1);
                            
                            bool? neighbourDirection;
                            var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator, out neighbourDirection);
                            var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                            var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                            var totalNeighbourWeight = _weightHandler.Add(current.Path.Weight, neighbourWeight);
                            var neighbourPath = new EdgePath<T>(neighbour, totalNeighbourWeight, edgeEnumerator.IdDirected(), 
                                current.Path);
                            
                            var doNeighbourForward = doForward && neighbourCanMoveForward && _weightHandler.IsSmallerThanOrEqual(totalNeighbourWeight, forwardMaxWeight) &&
                                !forwardSettled.Contains(neighbourPath);
                            var doNeighbourBackward = doBackward && neighbourCanMoveBackward && _weightHandler.IsSmallerThanOrEqual(totalNeighbourWeight, backwardMaxWeight) &&
                                !backwardSettled.Contains(neighbourPath);
                            if (doNeighbourBackward || doNeighbourForward)
                            {
                                T existingWeight;
                                uint[] sequenceAlongNeighbour = null;

                                if ((doNeighbourBackward || doNeighbourForward) && sequence.Length > 0)
                                {
                                    if (edgeEnumerator.IsOriginal())
                                    {
                                        if (sequence.Length > 1 && sequence[sequence.Length - 2] == neighbour)
                                        { // a t-turn!
                                            continue;
                                        }
                                        sequenceAlongNeighbour = sequence.Append(neighbour);
                                    }
                                    else
                                    {
                                        var sequence1Length = edgeEnumerator.GetSequence1(ref _sequence1);
                                        //var neighbourSequence = edgeEnumerator.GetSequence1();
                                        if (sequence.Length > 1 && sequence[sequence.Length - 2] == _sequence1[0])
                                        { // a t-turn!
                                            continue;
                                        }
                                        sequenceAlongNeighbour = sequence.Append(_sequence1, sequence1Length);
                                    }
                                }

                                if (doNeighbourForward)
                                {
                                    if (sequence.Length == 0 || restrictions.IsSequenceAllowed(sequenceAlongNeighbour))
                                    { // restrictions ok.
                                        if (forwardMinWeight.TryGetValue(neighbourPath, out existingWeight))
                                        {
                                            if (_weightHandler.IsSmallerThanOrEqual(existingWeight, totalNeighbourWeight))
                                            {
                                                doNeighbourForward = false;
                                            }
                                            else
                                            {
                                                forwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                            }
                                        }
                                        else
                                        {
                                            forwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        doNeighbourForward = false;
                                    }
                                }
                                if (doNeighbourBackward)
                                {
                                    if (sequenceAlongNeighbour != null)
                                    {
                                        sequenceAlongNeighbour.Reverse();
                                    }
                                    if (sequence.Length == 0 || restrictions.IsSequenceAllowed(sequenceAlongNeighbour))
                                    { // restrictions ok.
                                        if (backwardMinWeight.TryGetValue(neighbourPath, out existingWeight))
                                        {
                                            if (_weightHandler.IsSmallerThanOrEqual(existingWeight, totalNeighbourWeight))
                                            {
                                                doNeighbourBackward = false;
                                            }
                                            else
                                            {
                                                backwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                            }
                                        }
                                        else
                                        {
                                            backwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        doNeighbourBackward = false;
                                    }
                                }

                                if (doNeighbourBackward || doNeighbourForward)
                                { // add to heap.
                                    var newSettle = new SettledEdge(neighbourPath, current.Hops + 1, doNeighbourForward, doNeighbourBackward);
                                    _heap.Push(newSettle, _weightHandler.GetMetric(neighbourPath.Weight));
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
        public void ExistsOneHop(DirectedDynamicGraph graph, uint source, List<uint> targets, List<T> weights,
            ref EdgePath<T>[] forwardExists, ref EdgePath<T>[] backwardExists)
        {
            var targetsToCalculate = new HashSet<uint>();
            var maxWeight = _weightHandler.Zero;
            for (int idx = 0; idx < weights.Count; idx++)
            {
                if (forwardExists[idx] == null || backwardExists[idx] == null)
                {
                    targetsToCalculate.Add(targets[idx]);
                    if (_weightHandler.IsSmallerThan(maxWeight, weights[idx]))
                    {
                        maxWeight = weights[idx];
                    }
                }
                if (forwardExists[idx] == null)
                {
                    forwardExists[idx] = new EdgePath<T>();
                }
                if (backwardExists[idx] == null)
                {
                    backwardExists[idx] = new EdgePath<T>();
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
                        
                        bool? neighbourDirection;
                        var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current, out neighbourDirection);
                        var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                        var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                        if (neighbourCanMoveForward &&
                            _weightHandler.IsSmallerThan(neighbourWeight, weights[index]))
                        {
                            forwardExists[index] = new EdgePath<T>(neighbour, neighbourWeight, edgeEnumerator.IdDirected(), new EdgePath<T>(source));
                        }
                        if (neighbourCanMoveBackward &&
                            _weightHandler.IsSmallerThan(neighbourWeight, weights[index]))
                        {
                            backwardExists[index] = new EdgePath<T>(neighbour, neighbourWeight, edgeEnumerator.IdDirected(), new EdgePath<T>(source)); ;
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
        /// Represents a settled edge.
        /// </summary>
        private class SettledEdge
        {
            /// <summary>
            /// Creates a new settled edge.
            /// </summary>
            public SettledEdge(EdgePath<T> edge, uint hops, bool forward, bool backward)
            {
                this.Path = edge;
                this.Hops = hops;
                this.Forward = forward;
                this.Backward = backward;
            }

            /// <summary>
            /// The edge that was settled.
            /// </summary>
            public EdgePath<T> Path { get; set; }

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

    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public sealed class DykstraWitnessCalculator : DykstraWitnessCalculator<float>
    {
        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit)
            : base (new DefaultWeightHandler(null), hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit, int maxSettles)
            : base (new DefaultWeightHandler(null), hopLimit, maxSettles)
        {

        }
    }
}