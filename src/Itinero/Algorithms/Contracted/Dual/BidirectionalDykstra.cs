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

using Itinero.Algorithms.Collections;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public class BidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly DykstraSource<T> _source;
        private readonly DykstraSource<T> _target;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, uint source, uint target)
            : this(graph, weightHandler, new DykstraSource<T>(source), new DykstraSource<T>(target))
        {

        }

        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, DykstraSource<T> source, DykstraSource<T> target)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _source = source;
            _target = target;
            _weightHandler = weightHandler;
        }

        private Tuple<uint, uint, T> _best;
        private PathTree _pathTree;
        private Dictionary<uint, uint> _forwardVisits = new Dictionary<uint, uint>();
        private Dictionary<uint, uint> _backwardVisits = new Dictionary<uint, uint>();

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // keep settled vertices.
            _pathTree = new PathTree();

            // initialize the queues.
            var forwardQueue = new BinaryHeap<uint>();
            var backwardQueue = new BinaryHeap<uint>();

            // queue sources.
            if (_source.Vertex1 != Constants.NO_VERTEX)
            {
                forwardQueue.Push(_weightHandler.AddPathTree(_pathTree, _source.Vertex1, _source.Weight1, uint.MaxValue), 0);
            }
            if (_source.Vertex2 != Constants.NO_VERTEX)
            {
                forwardQueue.Push(_weightHandler.AddPathTree(_pathTree, _source.Vertex2, _source.Weight2, uint.MaxValue), 0);
            }

            // queue targets.
            if (_target.Vertex1 != Constants.NO_VERTEX)
            {
                backwardQueue.Push(_weightHandler.AddPathTree(_pathTree, _target.Vertex1, _target.Weight1, uint.MaxValue), 0);
            }
            if (_target.Vertex2 != Constants.NO_VERTEX)
            {
                backwardQueue.Push(_weightHandler.AddPathTree(_pathTree, _target.Vertex2, _target.Weight2, uint.MaxValue), 0);
            }

            // update best with current visits.
            _best = new Tuple<uint, uint, T>(uint.MaxValue, uint.MaxValue, _weightHandler.Infinite);

            // calculate stopping conditions.
            var queueBackwardWeight = backwardQueue.PeekWeight();
            var queueForwardWeight = forwardQueue.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (backwardQueue.Count == 0 && forwardQueue.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                var bestItem2 = _weightHandler.GetMetric(_best.Item3);
                if (bestItem2 < queueForwardWeight && bestItem2 < queueBackwardWeight)
                { // stop the search: it now became impossible to find a shorter route by further searching.
                    break;
                }

                // do a forward search.
                if (forwardQueue.Count > 0)
                { // first check for better path.
                    // get the current queued with the smallest weight that hasn't been visited yet.
                    var cPointer = forwardQueue.Pop();
                    uint cVertex, cPreviousPointer;
                    T cWeight;
                    _weightHandler.GetPathTree(_pathTree, cPointer, out cVertex, out cWeight, out cPreviousPointer);
                    while (_forwardVisits.ContainsKey(cVertex))
                    { // keep trying.
                        if (forwardQueue.Count == 0)
                        {
                            cPointer = uint.MaxValue;
                        }
                        else
                        {
                            cPointer = forwardQueue.Pop();
                            _weightHandler.GetPathTree(_pathTree, cPointer, out cVertex, out cWeight, out cPreviousPointer);
                        }
                    }

                    if (cPointer != uint.MaxValue)
                    {
                        uint bPointer;
                        if (_backwardVisits.TryGetValue(cVertex, out bPointer))
                        { // check for a new best.
                            uint bVertex, bPreviousPointer;
                            T bWeight;
                            _weightHandler.GetPathTree(_pathTree, bPointer, out bVertex, out bWeight, out bPreviousPointer);
                            var total = _weightHandler.Add(cWeight, bWeight);
                            if (_weightHandler.IsSmallerThan(total, _best.Item2))
                            { // a better path was found.
                                _best = new Tuple<uint, uint, T>(cPointer, bPointer, total);
                                this.HasSucceeded = true;
                            }
                        }

                        this.SearchForward(forwardQueue, cPointer, cVertex, cWeight);
                    }
                }

                // do a backward search.
                if (backwardQueue.Count > 0)
                {// first check for better path.
                    // get the current queued with the smallest weight that hasn't been visited yet.
                    var cPointer = backwardQueue.Pop();
                    uint cVertex, cPreviousPointer;
                    T cWeight;
                    _weightHandler.GetPathTree(_pathTree, cPointer, out cVertex, out cWeight, out cPreviousPointer);
                    while (_backwardVisits.ContainsKey(cVertex))
                    { // keep trying.
                        if (backwardQueue.Count == 0)
                        {
                            cPointer = uint.MaxValue;
                        }
                        else
                        {
                            cPointer = backwardQueue.Pop();
                            _weightHandler.GetPathTree(_pathTree, cPointer, out cVertex, out cWeight, out cPreviousPointer);
                        }
                    }

                    if (cPointer != uint.MaxValue)
                    {
                        uint bPointer; // best pointer.
                        if (_forwardVisits.TryGetValue(cVertex, out bPointer))
                        { // check for a new best.
                            uint bVertex, bPreviousPointer;
                            T bWeight;
                            _weightHandler.GetPathTree(_pathTree, bPointer, out bVertex, out bWeight, out bPreviousPointer);
                            var total = _weightHandler.Add(cWeight, bWeight);
                            if (_weightHandler.IsSmallerThan(total, _best.Item2))
                            { // a better path was found.
                                _best = new Tuple<uint, uint, T>(bPointer, cPointer, total);
                                this.HasSucceeded = true;
                            }
                        }

                        this.SearchBackward(backwardQueue, cPointer, cVertex, cWeight);
                    }
                }

                // calculate stopping conditions.
                if (forwardQueue.Count > 0)
                {
                    queueForwardWeight = forwardQueue.PeekWeight();
                }
                if (backwardQueue.Count > 0)
                {
                    queueBackwardWeight = backwardQueue.PeekWeight();
                }
            }
        }

        /// <summary>
        /// Search forward from one vertex.
        /// </summary>
        /// <returns></returns>
        private void SearchForward(BinaryHeap<uint> queue, uint cPointer, uint cVertex, T cWeight)
        {
            if (cPointer != uint.MaxValue)
            { // there is a next vertex found.

                // add to the settled vertices.
                uint ePointer; // the existing pointer.
                if (_forwardVisits.TryGetValue(cVertex, out ePointer))
                {
                    uint eVertex, ePreviousPointer;
                    T eWeight;
                    _weightHandler.GetPathTree(_pathTree, ePointer, out eVertex, out eWeight, out ePreviousPointer);
                    if (_weightHandler.IsLargerThan(eWeight, cWeight))
                    { // settle the vertex again if it has a better weight.
                        _forwardVisits[cVertex] = cPointer;
                    }
                    else
                    { // this is a worse settled, don't continue the search.
                        return;
                    }
                }
                else
                { // settled the vertex.
                    _forwardVisits.Add(cVertex, cPointer);
                }

                // get neighbours.
                var edgeEnumerator = _graph.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(cVertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var nWeightAndDirection = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);

                    if (nWeightAndDirection.Direction.F)
                    {
                        var nVertex = edgeEnumerator.Neighbour;
                        if (!_forwardVisits.ContainsKey(nVertex))
                        { // if not yet settled.
                            var nWeight = _weightHandler.Add(cWeight, nWeightAndDirection.Weight);
                            var nPointer = _weightHandler.AddPathTree(_pathTree, nVertex, nWeight,
                                cPointer);
                            queue.Push(nPointer, _weightHandler.GetMetric(nWeight));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        private void SearchBackward(BinaryHeap<uint> queue, uint cPointer, uint cVertex, T cWeight)
        {
            if (cPointer != uint.MaxValue)
            {
                // add to the settled vertices.
                uint ePointer; // the existing pointer.
                if (_backwardVisits.TryGetValue(cVertex, out ePointer))
                {
                    uint eVertex, ePreviousPointer;
                    T eWeight;
                    _weightHandler.GetPathTree(_pathTree, ePointer, out eVertex, out eWeight, out ePreviousPointer);
                    if (_weightHandler.IsLargerThan(eWeight, cWeight))
                    { // settle the vertex again if it has a better weight.
                        _backwardVisits[cVertex] = cPointer;
                    }
                    else
                    { // this is a worse settled, don't continue the search.
                        return;
                    }
                }
                else
                { // settled the vertex.
                    _backwardVisits.Add(cVertex, cPointer);
                }

                // get neighbours.
                var edgeEnumerator = _graph.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(cVertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var nWeightAndDirection = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);

                    if (nWeightAndDirection.Direction.B)
                    {
                        var nVertex = edgeEnumerator.Neighbour;
                        if (!_backwardVisits.ContainsKey(nVertex))
                        { // if not yet settled.
                            var nWeight = _weightHandler.Add(cWeight, nWeightAndDirection.Weight);
                            var nPointer = _weightHandler.AddPathTree(_pathTree, nVertex, nWeight,
                                cPointer);
                            queue.Push(nPointer, _weightHandler.GetMetric(nWeight));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        public DirectedMetaGraph Graph
        {
            get
            {
                return _graph;
            }
        }

        /// <summary>
        /// Returns the vertex on the best path.
        /// </summary>
        public uint Best
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _best.Item1;
            }
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetForwardVisit(uint vertex, out EdgePath<T> visit)
        {
            this.CheckHasRunAndHasSucceeded();

            uint vertexPointer;
            if (!_forwardVisits.TryGetValue(vertex, out vertexPointer))
            {
                visit = null;
                return false;
            }
            visit = _weightHandler.GetPath<T>(_pathTree, vertexPointer);
            return true;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetBackwardVisit(uint vertex, out EdgePath<T> visit)
        {
            this.CheckHasRunAndHasSucceeded();

            this.CheckHasRunAndHasSucceeded();

            uint vertexPointer;
            if (!_backwardVisits.TryGetValue(vertex, out vertexPointer))
            {
                visit = null;
                return false;
            }
            visit = _weightHandler.GetPath<T>(_pathTree, vertexPointer);
            return true;
        }
    }
}
