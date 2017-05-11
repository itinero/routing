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
using System.Diagnostics;

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public class DykstraWitnessCalculator<T>
        where T : struct
    {
        protected readonly BinaryHeap<SettledEdge> _heap;
        protected readonly WeightHandler<T> _weightHandler;
        protected readonly DirectedDynamicGraph _graph;
        protected readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, WeightHandler<T> weightHandler, int hopLimit)
            : this (graph, getRestrictions, weightHandler, hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, WeightHandler<T> weightHandler, int hopLimit, int maxSettles)
        {
            _hopLimit = hopLimit;
            _weightHandler = weightHandler;
            _graph = graph;
            _getRestrictions = getRestrictions;

            _heap = new BinaryHeap<SettledEdge>();
            _maxSettles = maxSettles;
        }

        protected int _hopLimit;
        protected int _maxSettles;
        protected uint[] _sequence1 = new uint[16];
        
        /// <summary>
        /// Calculates 
        /// </summary>
        public virtual void Calculate(Shortcuts<T> witnesses)
        {
            var accessor = witnesses.GetAccessor();
            while(accessor.MoveNextSource())
            {
                this.Calculate(accessor);
            }
        }

        /// <summary>
        /// Calculates a path between the two edges that have one vertex in common but may include a restriction.
        /// </summary>
        public virtual void CalculateTurn(uint vertex, DynamicEdge source, DynamicEdge target, out T weightForward, out T weightBackward)
        {
            var forwardMinWeight = new Dictionary<EdgePath<T>, T>();
            var backwardMinWeight = new Dictionary<EdgePath<T>, T>();
            var forwardFound = false;
            var backwardFound = false;
            var forwardMaxWeight = _weightHandler.Infinite;
            var backwardMaxWeight = _weightHandler.Infinite;
            weightForward = _weightHandler.Infinite;
            weightBackward = _weightHandler.Infinite;

            // extract target/source information.
            var sourceWeight = _weightHandler.GetEdgeWeight(source);
            var sourceDirection = sourceWeight.Direction;
            sourceDirection.Reverse();
            var targetWeight = _weightHandler.GetEdgeWeight(target);
            var targetDirection = targetWeight.Direction;

            // first calculate weights without extra edges.
            var restrictions = _getRestrictions(vertex);
            if (restrictions == null || restrictions.IsEmpty())
            { // no restrictions, we can immidiately return a weight.
                if (sourceDirection.F && targetDirection.F)
                { // route possible, source->target.
                    weightForward = _weightHandler.Add(sourceWeight.Weight, targetWeight.Weight);
                }
                if (sourceDirection.B && targetDirection.B)
                { // route possible, target->source.
                    weightBackward = _weightHandler.Add(sourceWeight.Weight, targetWeight.Weight);
                }
                return;
            }
            // restriction, check sequence.
            var sequence = new uint[3];
            if (source.IsOriginal())
            { // is an original edge
                sequence[0] = source.Neighbour;
                sequence[1] = vertex;
            }
            else
            { // is not an original edge, should always have a sequence.
                var s1 = source.GetSequence1();
                sequence[0] = s1[0];
                sequence[1] = vertex;
            }

            if (target.IsOriginal())
            { // is an original edge
                sequence[2] = target.Neighbour;
            }
            else
            { // is not an original edge, should always have a sequence.
                var s1 = target.GetSequence1();
                sequence[2] = s1[0];
            }

            if (sourceDirection.F && targetDirection.F)
            { // *a* forward route is possible.
                if (restrictions.IsSequenceAllowed(sequence))
                { // simple forward route is possible, restriction doesn't apply.
                    weightForward = _weightHandler.Add(sourceWeight.Weight, targetWeight.Weight);
                    forwardFound = true;
                }
            }
            else
            { // forward route is impossible.
                forwardFound = true;
            }
            sequence.Reverse();
            if (sourceDirection.F && targetDirection.F)
            { // *a* backward route is possible.
                if (restrictions.IsSequenceAllowed(sequence))
                { // backward route is possible, restriction doesn't apply.
                    weightBackward = _weightHandler.Add(sourceWeight.Weight, targetWeight.Weight);
                    backwardFound = true;
                }
            }
            else
            { // backward route is impossible.
                backwardFound = true;
            }

            var backwardSettled = new HashSet<EdgePath<T>>();
            var s = new List<uint>();
            var forwardSettled = new HashSet<EdgePath<T>>();
            _heap.Clear();
            _heap.Push(new SettledEdge(new EdgePath<T>(vertex, sourceWeight.Weight, new EdgePath<T>(sequence[2])), 0,
                sourceDirection.F, sourceDirection.B), 0);
            var edgeEnumerator = _graph.GetEdgeEnumerator();

            // keep looping until the queue is empty or the target is found!
            while (_heap.Count > 0)
            { // pop the first customer.
                var current = _heap.Pop();
                if (current.Hops + 1 >= _hopLimit)
                {
                    continue;
                }

                var forwardWasSettled = forwardSettled.Contains(current.Path);
                var backwardWasSettled = backwardSettled.Contains(current.Path);
                if (forwardWasSettled && backwardWasSettled)
                { // both are already settled.
                    continue;
                }

                if (forwardSettled.Count >= _maxSettles &&
                    backwardSettled.Count >= _maxSettles)
                { // do not continue searching.
                    break;
                }

                // check if one of the targets was matched.
                if (current.Forward && !forwardFound)
                { // this is a forward settle.
                    forwardSettled.Add(current.Path);
                    forwardMinWeight.Remove(current.Path);
                    if (target.IdDirected() == current.Path.Edge)
                    { // a better witness is found, update the path.
                        weightForward = current.Path.Weight;
                        forwardFound = true;
                        if (backwardFound)
                        {
                            return;
                        }
                    }
                }
                if (current.Backward && !backwardFound)
                { // this is a backward settle.
                    backwardSettled.Add(current.Path);
                    backwardMinWeight.Remove(current.Path);
                    if (target.IdDirected() == current.Path.Edge)
                    { // a better witness is found, update the path.
                        weightBackward = current.Path.Weight;
                        backwardFound = true;
                        if (forwardFound)
                        {
                            return;
                        }
                    }
                }

                var doForward = current.Forward && !forwardFound && !forwardWasSettled;
                var doBackward = current.Backward && !backwardFound && !backwardWasSettled;
                if (doForward || doBackward)
                { // get the neighbours.
                    // check for a restriction and if need build the original sequence.
                    restrictions = _getRestrictions(current.Path.Vertex);
                    sequence = current.Path.GetSequence2(edgeEnumerator, int.MaxValue, s);
                    sequence = sequence.Append(current.Path.Vertex);

                    // move to the current vertex.
                    edgeEnumerator.MoveTo(current.Path.Vertex);
                    //uint neighbour, data0, data1;
                    while (edgeEnumerator.MoveNext())
                    { // move next.
                        var neighbour = edgeEnumerator.Neighbour;

                        var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator);

                        var totalNeighbourWeight = _weightHandler.Add(current.Path.Weight, neighbourWeight.Weight);
                        var neighbourPath = new EdgePath<T>(neighbour, totalNeighbourWeight, edgeEnumerator.IdDirected(),
                            current.Path);

                        var doNeighbourForward = doForward && neighbourWeight.Direction.F && _weightHandler.IsSmallerThanOrEqual(totalNeighbourWeight, forwardMaxWeight) &&
                            !forwardSettled.Contains(neighbourPath);
                        var doNeighbourBackward = doBackward && neighbourWeight.Direction.B && _weightHandler.IsSmallerThanOrEqual(totalNeighbourWeight, backwardMaxWeight) &&
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

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        /// <param name="source">The witness accessor in the position of the source to calculate for.</param>
        protected virtual void Calculate(Shortcuts<T>.Accessor sourceAccessor)
        {
            var source = sourceAccessor.Source;

            // creates the settled list.
            var s = new List<uint>();
            var backwardSettled = new HashSet<EdgePath<T>>();
            var forwardSettled = new HashSet<EdgePath<T>>();
            var backwardTargets = new HashSet<uint>();
            var forwardTargets = new HashSet<uint>();
            T forwardMaxWeight = _weightHandler.Zero, backwardMaxWeight = _weightHandler.Zero;
            while (sourceAccessor.MoveNextTarget())
            {
                var witness = sourceAccessor.Current;

                if (_weightHandler.GetMetric(witness.Forward) > 0)
                {
                    forwardTargets.Add(witness.Edge.Vertex2);
                    if (_weightHandler.IsLargerThan(witness.Forward, forwardMaxWeight))
                    {
                        forwardMaxWeight = witness.Forward;
                    }
                }
                if (_weightHandler.GetMetric(witness.Backward) > 0)
                {
                    backwardTargets.Add(witness.Edge.Vertex2);
                    if (_weightHandler.IsLargerThan(witness.Backward, backwardMaxWeight))
                    {
                        backwardMaxWeight = witness.Backward;
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

            // add all neighbour edges of the source vertex that start with the source path.
            var edgeEnumerator = _graph.GetEdgeEnumerator();
            edgeEnumerator.MoveTo(source.Vertex1);
            while (edgeEnumerator.MoveNext())
            { // move next.
                var neighbour = edgeEnumerator.Neighbour;
                if (edgeEnumerator.IsOriginal())
                {
                    if (neighbour != source.Vertex2)
                    { // not an edge that matches the source.
                        continue;
                    }
                }
                else
                {
                    var s1 = edgeEnumerator.GetSequence1();
                    if (s1[0] != source.Vertex2)
                    { // not an edge that matches the source.
                        continue;
                    }
                }

                bool? neighbourDirection;
                var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator, out neighbourDirection);
                var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                var neighbourPath = new EdgePath<T>(neighbour, neighbourWeight, edgeEnumerator.IdDirected(),
                    new EdgePath<T>(source.Vertex1));

                var doNeighbourForward = neighbourCanMoveForward && _weightHandler.IsSmallerThanOrEqual(neighbourWeight, forwardMaxWeight) &&
                    !forwardSettled.Contains(neighbourPath);
                var doNeighbourBackward = neighbourCanMoveBackward && _weightHandler.IsSmallerThanOrEqual(neighbourWeight, backwardMaxWeight) &&
                    !backwardSettled.Contains(neighbourPath);

                _heap.Push(new SettledEdge(neighbourPath, 0, doNeighbourForward, doNeighbourBackward), _weightHandler.GetMetric(neighbourPath.Weight));
            }

            // keep looping until the queue is empty or the target is found!
            while (_heap.Count > 0)
            { // pop the first customer.
                var current = _heap.Pop();
                if (current.Hops + 1 < _hopLimit)
                {
                    var forwardWasSettled = forwardSettled.Contains(current.Path);
                    var backwardWasSettled = backwardSettled.Contains(current.Path);
                    if (forwardWasSettled && backwardWasSettled)
                    { // both are already settled.
                        continue;
                    }

                    // check if one of the targets was matched.
                    if (current.Forward)
                    { // this is a forward settle.
                        forwardSettled.Add(current.Path);
                        forwardMinWeight.Remove(current.Path);
                        if (forwardTargets.Contains(current.Path.Vertex))
                        {
                            var originalEdge = current.Path.ToOriginalEdge(edgeEnumerator, true);
                            sourceAccessor.ResetTarget();
                            while (sourceAccessor.MoveNextTarget())
                            {
                                var witness = sourceAccessor.Current;

                                if (witness.Edge.Equals(originalEdge) &&
                                    _weightHandler.IsSmallerThan(current.Path.Weight, witness.Forward))
                                { // a better witness is found, update the path.
                                    witness.Forward = _weightHandler.Zero;
                                    sourceAccessor.Current = witness;
                                    forwardTargets.Remove(originalEdge.Vertex2);
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
                            var originalEdge = current.Path.ToOriginalEdge(edgeEnumerator, true);
                            sourceAccessor.ResetTarget();
                            while (sourceAccessor.MoveNextTarget())
                            {
                                var witness = sourceAccessor.Current;

                                if (witness.Edge.Equals(originalEdge) &&
                                    _weightHandler.IsSmallerThan(current.Path.Weight, witness.Backward))
                                { // a better witness is found, update the path.
                                    witness.Backward = _weightHandler.Zero;
                                    sourceAccessor.Current = witness;
                                    backwardTargets.Remove(originalEdge.Vertex2);
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
                        var restrictions = _getRestrictions(current.Path.Vertex);
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
        protected class SettledEdge
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
        public DykstraWitnessCalculator(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, int hopLimit)
            : base (graph, getRestrictions, new DefaultWeightHandler(null), hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, int hopLimit, int maxSettles)
            : base (graph, getRestrictions, new DefaultWeightHandler(null), hopLimit, maxSettles)
        {

        }

        ///// <summary>
        ///// Calculates witness paths.
        ///// </summary>
        ///// <param name="graph">The graph being contracted.</param>
        ///// <param name="getRestrictions">Function to get restrictions.</param>
        ///// <param name="source">A path containing one edge as source.</param>
        ///// <param name="targets">A list of paths that represent the targets, each path contains exactly one edge.</param>
        //public override void Calculate(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, OriginalEdge source, 
        //    List<OriginalEdge> targets, List<float> weights, ref EdgePath<float>[] forwardWitness, ref EdgePath<float>[] backwardWitness, uint vertexToSkip)
        //{
        //    // creates the settled list.
        //    var s = new List<uint>();
        //    var backwardSettled = new HashSet<EdgePath<float>>();
        //    var forwardSettled = new HashSet<EdgePath<float>>();
        //    var backwardTargets = new HashSet<uint>();
        //    var forwardTargets = new HashSet<uint>();
        //    float forwardMaxWeight = 0, backwardMaxWeight = 0;
        //    for (int idx = 0; idx < weights.Count; idx++)
        //    {
        //        if (forwardWitness[idx] == null)
        //        {
        //            forwardWitness[idx] = new EdgePath<float>();
        //            forwardTargets.Add(targets[idx].Vertex2);
        //            //if (_weightHandler.IsSmallerThan(forwardMaxWeight, weights[idx]))
        //            if (forwardMaxWeight < weights[idx])
        //            {
        //                forwardMaxWeight = weights[idx];
        //            }
        //        }
        //        if (backwardWitness[idx] == null)
        //        {
        //            backwardWitness[idx] = new EdgePath<float>();
        //            backwardTargets.Add(targets[idx].Vertex2);
        //            //if (_weightHandler.IsSmallerThan(backwardMaxWeight, weights[idx]))
        //            if (backwardMaxWeight < weights[idx])
        //            {
        //                backwardMaxWeight = weights[idx];
        //            }
        //        }
        //    }

        //    if (forwardMaxWeight == 0 &&
        //        backwardMaxWeight == 0)
        //    { // no need to search!
        //        return;
        //    }

        //    // creates the priorty queue.
        //    var forwardMinWeight = new Dictionary<EdgePath<float>, float>();
        //    var backwardMinWeight = new Dictionary<EdgePath<float>, float>();
        //    _heap.Clear();

        //    // add all neighbour edges of the source vertex that start with the source path.
        //    var edgeEnumerator = graph.GetEdgeEnumerator();
        //    edgeEnumerator.MoveTo(source.Vertex1);
        //    while (edgeEnumerator.MoveNext())
        //    { // move next.
        //        var neighbour = edgeEnumerator.Neighbour;
        //        if (edgeEnumerator.IsOriginal())
        //        {
        //            if (neighbour != source.Vertex2)
        //            { // not an edge that matches the source.
        //                continue;
        //            }
        //        }
        //        else
        //        {
        //            var s1 = edgeEnumerator.GetSequence1();
        //            if (s1[0] != source.Vertex2)
        //            { // not an edge that matches the source.
        //                continue;
        //            }
        //        }
                
        //        float neighbourWeight;
        //        bool? neighbourDirection;
        //        Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
        //            out neighbourWeight, out neighbourDirection);

        //        var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
        //        var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

        //        var neighbourPath = new EdgePath<float>(neighbour, neighbourWeight, edgeEnumerator.IdDirected(),
        //            new EdgePath<float>(source.Vertex1));

        //        //var doNeighbourForward = neighbourCanMoveForward && _weightHandler.IsSmallerThanOrEqual(neighbourWeight, forwardMaxWeight) &&
        //        //    !forwardSettled.Contains(neighbourPath);
        //        var doNeighbourForward = neighbourCanMoveForward && neighbourWeight <= forwardMaxWeight &&
        //            !forwardSettled.Contains(neighbourPath);
        //        //var doNeighbourBackward = neighbourCanMoveBackward && _weightHandler.IsSmallerThanOrEqual(neighbourWeight, backwardMaxWeight) &&
        //        //    !backwardSettled.Contains(neighbourPath);
        //        var doNeighbourBackward = neighbourCanMoveBackward && neighbourWeight <= backwardMaxWeight &&
        //            !backwardSettled.Contains(neighbourPath);

        //        _heap.Push(new SettledEdge(neighbourPath, 0, doNeighbourForward, doNeighbourBackward), neighbourPath.Weight);
        //    }

        //    // keep looping until the queue is empty or the target is found!
        //    while (_heap.Count > 0)
        //    { // pop the first customer.
        //        var current = _heap.Pop();
        //        if (current.Hops + 1 < this.HopLimit)
        //        {
        //            if (current.Path.Vertex == vertexToSkip)
        //            { // this is the vertex being contracted.
        //                continue;
        //            }
        //            var forwardWasSettled = forwardSettled.Contains(current.Path);
        //            var backwardWasSettled = backwardSettled.Contains(current.Path);
        //            if (forwardWasSettled && backwardWasSettled)
        //            { // both are already settled.
        //                continue;
        //            }

        //            // check if one of the targets was matched.
        //            if (current.Forward)
        //            { // this is a forward settle.
        //                forwardSettled.Add(current.Path);
        //                forwardMinWeight.Remove(current.Path);
        //                if (forwardTargets.Contains(current.Path.Vertex))
        //                {
        //                    var originalEdge = current.Path.ToOriginalEdge(edgeEnumerator, true);
        //                    for (var i = 0; i < targets.Count; i++)
        //                    {
        //                        if (targets[i].Equals(originalEdge) &&
        //                            current.Path.Weight <= weights[i])
        //                        { // TODO: check if this is a proper stop condition.
        //                            if (forwardWitness[i] == null || forwardWitness[i].Vertex == Constants.NO_VERTEX ||
        //                                current.Path.Weight < forwardWitness[i].Weight)
        //                            {
        //                                forwardWitness[i] = current.Path;
        //                                forwardTargets.Remove(originalEdge.Vertex2);
        //                            }
        //                        }
        //                    }
        //                }
        //            }
        //            if (current.Backward)
        //            { // this is a backward settle.
        //                backwardSettled.Add(current.Path);
        //                backwardMinWeight.Remove(current.Path);
        //                if (backwardTargets.Contains(current.Path.Vertex))
        //                {
        //                    var originalEdge = current.Path.ToOriginalEdge(edgeEnumerator, true);
        //                    for (var i = 0; i < targets.Count; i++)
        //                    {
        //                        if (targets[i].Equals(originalEdge) &&
        //                            current.Path.Weight <= weights[i])
        //                        { // TODO: check if this is a proper stop condition.
        //                            if (backwardWitness[i] == null || backwardWitness[i].Vertex == Constants.NO_VERTEX ||
        //                                current.Path.Weight < backwardWitness[i].Weight)
        //                            {
        //                                backwardWitness[i] = current.Path;
        //                                backwardTargets.Remove(originalEdge.Vertex2);
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            if (forwardTargets.Count == 0 &&
        //                backwardTargets.Count == 0)
        //            { // there is nothing left to check.
        //                break;
        //            }

        //            if (forwardSettled.Count >= this.MaxSettles &&
        //                backwardSettled.Count >= this.MaxSettles)
        //            { // do not continue searching.
        //                break;
        //            }

        //            var doForward = current.Forward && forwardTargets.Count > 0 && !forwardWasSettled;
        //            var doBackward = current.Backward && backwardTargets.Count > 0 && !backwardWasSettled;
        //            if (doForward || doBackward)
        //            { // get the neighbours.

        //                // check for a restriction and if need build the original sequence.
        //                var restrictions = getRestrictions(current.Path.Vertex);
        //                var sequence = current.Path.GetSequence2(edgeEnumerator, int.MaxValue, s);
        //                sequence = sequence.Append(current.Path.Vertex);

        //                // move to the current vertex.
        //                edgeEnumerator.MoveTo(current.Path.Vertex);
        //                //uint neighbour, data0, data1;
        //                while (edgeEnumerator.MoveNext())
        //                { // move next.
        //                    var neighbour = edgeEnumerator.Neighbour;

        //                    bool? neighbourDirection;
        //                    float neighbourWeight;
        //                    Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
        //                        out neighbourWeight, out neighbourDirection);

        //                    var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
        //                    var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

        //                    var totalNeighbourWeight = current.Path.Weight + neighbourWeight;
        //                    var neighbourPath = new EdgePath<float>(neighbour, totalNeighbourWeight, edgeEnumerator.IdDirected(),
        //                        current.Path);

        //                    var doNeighbourForward = doForward && neighbourCanMoveForward && totalNeighbourWeight <= forwardMaxWeight &&
        //                        !forwardSettled.Contains(neighbourPath);
        //                    var doNeighbourBackward = doBackward && neighbourCanMoveBackward && totalNeighbourWeight <= backwardMaxWeight &&
        //                        !backwardSettled.Contains(neighbourPath);
        //                    if (doNeighbourBackward || doNeighbourForward)
        //                    {
        //                        float existingWeight;
        //                        uint[] sequenceAlongNeighbour = null;

        //                        if ((doNeighbourBackward || doNeighbourForward) && sequence.Length > 0)
        //                        {
        //                            if (edgeEnumerator.IsOriginal())
        //                            {
        //                                if (sequence.Length > 1 && sequence[sequence.Length - 2] == neighbour)
        //                                { // a t-turn!
        //                                    continue;
        //                                }
        //                                sequenceAlongNeighbour = sequence.Append(neighbour);
        //                            }
        //                            else
        //                            {
        //                                var sequence1Length = edgeEnumerator.GetSequence1(ref _sequence1);
        //                                //var neighbourSequence = edgeEnumerator.GetSequence1();
        //                                if (sequence.Length > 1 && sequence[sequence.Length - 2] == _sequence1[0])
        //                                { // a t-turn!
        //                                    continue;
        //                                }
        //                                sequenceAlongNeighbour = sequence.Append(_sequence1, sequence1Length);
        //                            }
        //                        }

        //                        if (doNeighbourForward)
        //                        {
        //                            if (sequence.Length == 0 || restrictions.IsSequenceAllowed(sequenceAlongNeighbour))
        //                            { // restrictions ok.
        //                                if (forwardMinWeight.TryGetValue(neighbourPath, out existingWeight))
        //                                {
        //                                    if (existingWeight <= totalNeighbourWeight)
        //                                    {
        //                                        doNeighbourForward = false;
        //                                    }
        //                                    else
        //                                    {
        //                                        forwardMinWeight[neighbourPath] = totalNeighbourWeight;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    forwardMinWeight[neighbourPath] = totalNeighbourWeight;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                doNeighbourForward = false;
        //                            }
        //                        }
        //                        if (doNeighbourBackward)
        //                        {
        //                            if (sequenceAlongNeighbour != null)
        //                            {
        //                                sequenceAlongNeighbour.Reverse();
        //                            }
        //                            if (sequence.Length == 0 || restrictions.IsSequenceAllowed(sequenceAlongNeighbour))
        //                            { // restrictions ok.
        //                                if (backwardMinWeight.TryGetValue(neighbourPath, out existingWeight))
        //                                {
        //                                    if (existingWeight <= totalNeighbourWeight)
        //                                    {
        //                                        doNeighbourBackward = false;
        //                                    }
        //                                    else
        //                                    {
        //                                        backwardMinWeight[neighbourPath] = totalNeighbourWeight;
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    backwardMinWeight[neighbourPath] = totalNeighbourWeight;
        //                                }
        //                            }
        //                            else
        //                            {
        //                                doNeighbourBackward = false;
        //                            }
        //                        }

        //                        if (doNeighbourBackward || doNeighbourForward)
        //                        { // add to heap.
        //                            var newSettle = new SettledEdge(neighbourPath, current.Hops + 1, doNeighbourForward, doNeighbourBackward);
        //                            _heap.Push(newSettle, neighbourPath.Weight);
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}
    }
}