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
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public class BidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly IEnumerable<EdgePath<T>> _sources;
        private readonly IEnumerable<EdgePath<T>> _targets;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, IEnumerable<EdgePath<T>> sources, IEnumerable<EdgePath<T>> targets)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _sources = sources;
            _targets = targets;
            _weightHandler = weightHandler;
        }

        private Tuple<uint, T> _best;
        private Dictionary<uint, EdgePath<T>> _forwardVisits;
        private Dictionary<uint, EdgePath<T>> _backwardVisits;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            // keep settled vertices.
            _forwardVisits = new Dictionary<uint, EdgePath<T>>();
            _backwardVisits = new Dictionary<uint, EdgePath<T>>();

            // initialize the queues.
            var forwardQueue = new BinaryHeap<EdgePath<T>>();
            var backwardQueue = new BinaryHeap<EdgePath<T>>();

            // queue sources.
            foreach (var source in _sources)
            {
                forwardQueue.Push(source, _weightHandler.GetMetric(source.Weight));
            }

            // queue targets.
            foreach (var target in _targets)
            {
                backwardQueue.Push(target, _weightHandler.GetMetric(target.Weight));
            }

            // update best with current visits.
            _best = new Tuple<uint, T>(Constants.NO_VERTEX, _weightHandler.Infinite);

            // calculate stopping conditions.
            var queueBackwardWeight = backwardQueue.PeekWeight();
            var queueForwardWeight = forwardQueue.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (backwardQueue.Count == 0 && forwardQueue.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (_weightHandler.GetMetric(_best.Item2) < queueForwardWeight &&
                    _weightHandler.GetMetric(_best.Item2) < queueBackwardWeight)
                { // stop the search: it now became impossible to find a shorter route by further searching.
                    break;
                }

                // do a forward search.
                if (forwardQueue.Count > 0)
                { // first check for better path.
                    // get the current queued with the smallest weight that hasn't been visited yet.
                    var current = forwardQueue.Pop();
                    while (current != null && _forwardVisits.ContainsKey(current.Vertex))
                    { // keep trying.
                        current = forwardQueue.Pop();
                    }

                    if (current != null)
                    {
                        EdgePath<T> toBest;
                        if (_backwardVisits.TryGetValue(current.Vertex, out toBest))
                        { // check for a new best.
                            var total = _weightHandler.Add(current.Weight, toBest.Weight);
                            if (_weightHandler.IsSmallerThan(total, _best.Item2))
                            { // a better path was found.
                                _best = new Tuple<uint, T>(current.Vertex, total);
                                this.HasSucceeded = true;
                            }
                        }

                        this.SearchForward(forwardQueue, current);
                    }
                }

                // do a backward search.
                if (backwardQueue.Count > 0)
                {// first check for better path.
                    // get the current vertex with the smallest weight.
                    var current = backwardQueue.Pop();
                    while (current != null && _backwardVisits.ContainsKey(current.Vertex))
                    { // keep trying.
                        current = backwardQueue.Pop();
                    }

                    if (current != null)
                    {
                        EdgePath<T> toBest;
                        if (_forwardVisits.TryGetValue(current.Vertex, out toBest))
                        { // check for a new best.
                            var total = _weightHandler.Add(current.Weight, toBest.Weight);
                            if (_weightHandler.IsSmallerThan(total, _best.Item2))
                            { // a better path was found.
                                _best = new Tuple<uint, T>(current.Vertex, total);
                                this.HasSucceeded = true;
                            }
                        }

                        this.SearchBackward(backwardQueue, current);
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
        private void SearchForward(BinaryHeap<EdgePath<T>> queue, EdgePath<T> current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var edgeEnumerator = _graph.GetEdgeEnumerator();

                // add to the settled vertices.
                EdgePath<T> previousLinkedRoute;
                if (_forwardVisits.TryGetValue(current.Vertex, out previousLinkedRoute))
                {
                    if (_weightHandler.IsLargerThan(previousLinkedRoute.Weight, current.Weight))
                    { // settle the vertex again if it has a better weight.
                        _forwardVisits[current.Vertex] = current;
                    }
                }
                else
                { // settled the vertex.
                    _forwardVisits.Add(current.Vertex, current);
                }

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    bool? neighbourDirection;
                    var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current, out neighbourDirection);

                    if (neighbourDirection == null || neighbourDirection.Value)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        if (!_forwardVisits.ContainsKey(neighbourNeighbour))
                        { // if not yet settled.
                            var routeToNeighbour = new EdgePath<T>(
                                neighbourNeighbour, _weightHandler.Add(current.Weight, neighbourWeight), current);
                            queue.Push(routeToNeighbour, _weightHandler.GetMetric(routeToNeighbour.Weight));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        private void SearchBackward(BinaryHeap<EdgePath<T>> queue, EdgePath<T> current)
        {
            if (current != null)
            {
                // get the edge enumerator.
                var edgeEnumerator = _graph.GetEdgeEnumerator();

                // add to the settled vertices.
                EdgePath<T> previousLinkedRoute;
                if (_backwardVisits.TryGetValue(current.Vertex, out previousLinkedRoute))
                {
                    if (_weightHandler.IsLargerThan(previousLinkedRoute.Weight, current.Weight))
                    { // settle the vertex again if it has a better weight.
                        _backwardVisits[current.Vertex] = current;
                    }
                }
                else
                { // settled the vertex.
                    _backwardVisits.Add(current.Vertex, current);
                }

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    bool? neighbourDirection;
                    var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current, out neighbourDirection);

                    if (neighbourDirection == null || !neighbourDirection.Value)
                    { // the edge is backward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        if (!_backwardVisits.ContainsKey(neighbourNeighbour))
                        { // if not yet settled.
                            var routeToNeighbour = new EdgePath<T>(
                                neighbourNeighbour, _weightHandler.Add(current.Weight, neighbourWeight), current);
                            queue.Push(routeToNeighbour, _weightHandler.GetMetric(routeToNeighbour.Weight));
                        }
                    }
                }
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

            return _forwardVisits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetBackwardVisit(uint vertex, out EdgePath<T> visit)
        {
            this.CheckHasRunAndHasSucceeded();

            return _backwardVisits.TryGetValue(vertex, out visit);
        }
        
        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();

            EdgePath<T> fromSource;
            EdgePath<T> toTarget;
            if (_forwardVisits.TryGetValue(_best.Item1, out fromSource) &&
                _backwardVisits.TryGetValue(_best.Item1, out toTarget))
            {
                var vertices = new List<uint>();

                // add vertices from source.
                vertices.Add(fromSource.Vertex);
                while (fromSource.From != null)
                {
                    if (fromSource.From.Vertex != Constants.NO_VERTEX)
                    { // this should be the end of the path.
                        if (fromSource.Edge == DirectedEdgeId.NO_EDGE)
                        { // only expand when there is no edge id.
                            _graph.ExpandEdge(fromSource.From.Vertex, fromSource.Vertex, vertices, false, true);
                        }
                    }
                    vertices.Add(fromSource.From.Vertex);
                    fromSource = fromSource.From;
                }
                vertices.Reverse();

                // and add vertices to target.
                while (toTarget.From != null)
                {
                    if (toTarget.From.Vertex != Constants.NO_VERTEX)
                    { // this should be the end of the path.
                        if (toTarget.Edge == DirectedEdgeId.NO_EDGE)
                        { // only expand when there is no edge id.
                            _graph.ExpandEdge(toTarget.From.Vertex, toTarget.Vertex, vertices, false, false);
                        }
                    }
                    vertices.Add(toTarget.From.Vertex);
                    toTarget = toTarget.From;
                }

                return vertices;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }

    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public sealed class BidirectionalDykstra : BidirectionalDykstra<float>
    {
        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedMetaGraph graph, IEnumerable<EdgePath<float>> sources, IEnumerable<EdgePath<float>> targets)
            : base(graph, new DefaultWeightHandler(null), sources, targets)
        {

        }
    }        
}