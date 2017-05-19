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
using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public class BidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly IEnumerable<EdgePath<T>> _sources;
        private readonly IEnumerable<EdgePath<T>> _targets;
        protected readonly RestrictionCollection _restrictions;
        protected readonly WeightHandler<T> _weightHandler;
 
        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedDynamicGraph graph, WeightHandler<T> weightHandler, IEnumerable<EdgePath<T>> sources, IEnumerable<EdgePath<T>> targets,
            RestrictionCollection restrictions)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _weightHandler = weightHandler;
            _sources = sources.Select(x => {
                x.StripEdges();
                return x;
            });
            _targets = targets.Select(x => {
                x.StripEdges();
                return x;
            });
            _restrictions = restrictions;
        }

        private Tuple<EdgePath<T>, EdgePath<T>, T> _best;
        protected Dictionary<uint, LinkedEdgePath> _forwardVisits;
        protected Dictionary<uint, LinkedEdgePath> _backwardVisits;
        protected BinaryHeap<EdgePath<T>> _forwardQueue;
        protected BinaryHeap<EdgePath<T>> _backwardQueue;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            var edgeEnumerator = _graph.GetEdgeEnumerator();

            // keep settled vertices.
            _forwardVisits = new Dictionary<uint, LinkedEdgePath>();
            _backwardVisits = new Dictionary<uint, LinkedEdgePath>();

            // initialize the queues.
            _forwardQueue = new BinaryHeap<EdgePath<T>>();
            _backwardQueue = new BinaryHeap<EdgePath<T>>();

            // queue sources.
            foreach (var source in _sources)
            {
                _forwardQueue.Push(source, _weightHandler.GetMetric(source.Weight));
            }

            // queue targets.
            foreach (var target in _targets)
            {
                _backwardQueue.Push(target, _weightHandler.GetMetric(target.Weight));
            }

            // update best with current visits.
            _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(new EdgePath<T>(), new EdgePath<T>(), _weightHandler.Infinite);

            // calculate stopping conditions.
            var queueBackwardWeight = _backwardQueue.PeekWeight();
            var queueForwardWeight = _forwardQueue.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (_backwardQueue.Count == 0 && _forwardQueue.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (_weightHandler.GetMetric(_best.Item3) < queueForwardWeight &&
                    _weightHandler.GetMetric(_best.Item3) < queueBackwardWeight)
                { // stop the search: it now became impossible to find a better route by further searching.
                    break;
                }

                // do a forward search.
                if (_forwardQueue.Count > 0)
                { // first check for better path.
                    // get the current queued with the smallest weight that hasn't been visited yet.
                    var current = _forwardQueue.Pop();
                    while (current != null)
                    { // keep trying.
                        LinkedEdgePath edgePath = null;
                        if (!_forwardVisits.TryGetValue(current.Vertex, out edgePath))
                        { // this vertex has not been visited before.
                            _forwardVisits.Add(current.Vertex, new LinkedEdgePath()
                            {
                                Path = current
                            });
                            break;
                        }
                        else
                        { // vertex has been visited before, check if edge has.
                            if (!edgePath.HasPath(current))
                            { // current edge has not been used to get to this vertex.
                                _forwardVisits[current.Vertex] = new LinkedEdgePath()
                                {
                                    Path = current,
                                    Next = edgePath
                                };
                                break;
                            }
                        }
                        current = _forwardQueue.Pop();
                    }
                    
                    if (current != null)
                    {
                        // get relevant restrictions.
                        _restrictions.Update(current.Vertex);

                        // check if there is a backward path that matches this vertex.
                        LinkedEdgePath backwardPath = null;
                        if (_backwardVisits.TryGetValue(current.Vertex, out backwardPath))
                        { // check for a new best.
                            while (backwardPath != null)
                            {
                                var totalCurrentWeight = _weightHandler.Add(current.Weight, backwardPath.Path.Weight);
                                if (_weightHandler.IsSmallerThan(totalCurrentWeight, _best.Item3))
                                { // potentially a weight improvement.
                                    // check u-turn.
                                    var sequence2Forward = backwardPath.Path.GetSequence2(edgeEnumerator);
                                    var sequence2Current = current.GetSequence2(edgeEnumerator);
                                    var finalTurn = new Turn()
                                    {
                                        Vertex1 = sequence2Forward,
                                        Vertex2 = current.Vertex,
                                        Vertex3 = sequence2Current
                                    };
                                    if (finalTurn.IsUTurn ||
                                        finalTurn.IsRestrictedBy(_restrictions))
                                    {
                                        backwardPath = backwardPath.Next;
                                        continue;
                                    }

                                    _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(current, backwardPath.Path,
                                        _weightHandler.Add(current.Weight, backwardPath.Path.Weight));
                                    this.HasSucceeded = true;
                                }
                                backwardPath = backwardPath.Next;
                            }
                        }

                        // continue the search.
                        this.SearchForward(edgeEnumerator, current);
                    }
                }

                // do a backward search.
                if (_backwardQueue.Count > 0)
                {// first check for better path.
                    // get the current vertex with the smallest weight.
                    var current = _backwardQueue.Pop();
                    while (current != null)
                    { // keep trying.
                        LinkedEdgePath edgePath = null;
                        if (!_backwardVisits.TryGetValue(current.Vertex, out edgePath))
                        { // this vertex has not been visited before.
                            _backwardVisits.Add(current.Vertex, new LinkedEdgePath()
                            {
                                Path = current
                            });
                            break;
                        }
                        else
                        { // vertex has been visited before, check if edge has.
                            if (!edgePath.HasPath(current))
                            { // current edge has not been used to get to this vertex.
                                _backwardVisits[current.Vertex] = new LinkedEdgePath()
                                {
                                    Path = current,
                                    Next = edgePath
                                };
                                break;
                            }
                        }
                        current = _backwardQueue.Pop();
                    }

                    if (current != null)
                    {
                        // get relevant restrictions.
                        _restrictions.Update(current.Vertex);

                        // check if there is a backward path that matches this vertex.
                        LinkedEdgePath forwardPath = null;
                        if (_forwardVisits.TryGetValue(current.Vertex, out forwardPath))
                        { // check for a new best.
                            while (forwardPath != null)
                            {
                                var total = _weightHandler.Add(current.Weight, forwardPath.Path.Weight);
                                if (_weightHandler.IsSmallerThan(total, _best.Item3))
                                { // potentially a weight improvement.
                                    // check u-turn.
                                    var sequence2Forward = forwardPath.Path.GetSequence2(edgeEnumerator);
                                    var sequence2Current = current.GetSequence2(edgeEnumerator);
                                    var finalTurn = new Turn()
                                    {
                                        Vertex1 = sequence2Forward,
                                        Vertex2 = current.Vertex,
                                        Vertex3 = sequence2Current
                                    };
                                    if (!finalTurn.IsUTurn &&
                                        !finalTurn.IsRestrictedBy(_restrictions))
                                    {
                                        _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(forwardPath.Path, current,
                                            _weightHandler.Add(current.Weight, forwardPath.Path.Weight));
                                        this.HasSucceeded = true;
                                    }
                                }
                                forwardPath = forwardPath.Next;
                            }
                        }

                        // continue the search.
                        this.SearchBackward(edgeEnumerator, current);
                    }
                }

                // calculate stopping conditions.
                if (_forwardQueue.Count > 0)
                {
                    queueForwardWeight = _forwardQueue.PeekWeight();
                }
                if (_backwardQueue.Count > 0)
                {
                    queueBackwardWeight = _backwardQueue.PeekWeight();
                }
            }

            _forwardQueue = null;
            _backwardQueue = null;
        }

        /// <summary>
        /// Search forward from one vertex.
        /// </summary>
        /// <returns></returns>
        protected virtual void SearchForward(DirectedDynamicGraph.EdgeEnumerator edgeEnumerator, EdgePath<T> current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var currentS2 = current.GetSequence2(edgeEnumerator);
                var currentOriginal = new OriginalEdge(currentS2, current.Vertex);

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);

                    if (neighbourWeight.Direction.F)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        var neighbourTurn = new Turn(currentOriginal, neighbourNeighbour);
                        if (!edgeEnumerator.IsOriginal())
                        { // not an original edge, use the sequence.
                            neighbourTurn.Vertex3 = edgeEnumerator.GetSequence1();
                        }

                        // check u-turns and restrictions.
                        if (neighbourTurn.IsUTurn ||
                            neighbourTurn.IsRestrictedBy(_restrictions))
                        {
                            continue;
                        }

                        // build route to neighbour and check if it has been visited already.
                        var routeToNeighbour = new EdgePath<T>(
                            neighbourNeighbour, _weightHandler.Add(current.Weight, neighbourWeight.Weight), edgeEnumerator.IdDirected(), current);
                        LinkedEdgePath edgePath = null;
                        if (!_forwardVisits.TryGetValue(current.Vertex, out edgePath) ||
                            !edgePath.HasPath(routeToNeighbour))
                        { // this vertex has not been visited in this way before.
                            _forwardQueue.Push(routeToNeighbour, _weightHandler.GetMetric(routeToNeighbour.Weight));
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        /// <returns></returns>
        protected virtual void SearchBackward(DirectedDynamicGraph.EdgeEnumerator edgeEnumerator, EdgePath<T> current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var currentS2 = current.GetSequence2(edgeEnumerator);
                var currentOriginal = new OriginalEdge(currentS2, current.Vertex);

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);
                    if (neighbourWeight.Direction.B)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        var neighbourTurn = new Turn(currentOriginal, neighbourNeighbour);
                        if (!edgeEnumerator.IsOriginal())
                        { // not an original edge, use the sequence.
                            neighbourTurn.Vertex3 = edgeEnumerator.GetSequence1();
                        }

                        neighbourTurn.Reverse(); // this is a backward search!
                        if (neighbourTurn.IsUTurn ||
                            neighbourTurn.IsRestrictedBy(_restrictions))
                        { // don't do u-turns!
                            continue;
                        }
                        
                        // build route to neighbour and check if it has been visited already.
                        var routeToNeighbour = new EdgePath<T>(
                            neighbourNeighbour, _weightHandler.Add(current.Weight, neighbourWeight.Weight), edgeEnumerator.IdDirected(), current);
                        LinkedEdgePath edgePath = null;
                        if (!_backwardVisits.TryGetValue(current.Vertex, out edgePath) ||
                            !edgePath.HasPath(routeToNeighbour))
                        { // this vertex has not been visited in this way before.
                            _backwardQueue.Push(routeToNeighbour, _weightHandler.GetMetric(routeToNeighbour.Weight));
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

                return _best.Item1.Vertex;
            }
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetForwardVisit(uint vertex, out EdgePath<T> visit)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePath path;
            if (_forwardVisits.TryGetValue(vertex, out path))
            {
                visit = path.Best(_weightHandler);
                return true;
            }
            visit = null;
            return false;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetBackwardVisit(uint vertex, out EdgePath<T> visit)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePath path;
            if (_backwardVisits.TryGetValue(vertex, out path))
            {
                visit = path.Best(_weightHandler);
                return true;
            }
            visit = null;
            return false;
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        public List<uint> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();

            var vertices = new List<uint>();
            var fromSource = _best.Item1.Expand(_graph, _weightHandler, true);
            var toTarget = _best.Item2.Expand(_graph, _weightHandler, false);

            // add vertices from source.
            vertices.Add(fromSource.Vertex);
            while (fromSource.From != null)
            {
                //if (fromSource.From.Vertex != Constants.NO_VERTEX)
                //{ // this should be the end of the path.
                //    _graph.ExpandEdge(fromSource.From.Vertex, fromSource.Vertex, vertices, false, true);
                //}
                vertices.Add(fromSource.From.Vertex);
                fromSource = fromSource.From;
            }
            vertices.Reverse();

            // and add vertices to target.
            while (toTarget.From != null)
            {
                //if (toTarget.From.Vertex != Constants.NO_VERTEX)
                //{ // this should be the end of the path.
                //    _graph.ExpandEdge(toTarget.From.Vertex, toTarget.Vertex, vertices, false, false);
                //}
                vertices.Add(toTarget.From.Vertex);
                toTarget = toTarget.From;
            }
            return vertices;
        }
        
        protected class LinkedEdgePath
        {
            public EdgePath<T> Path { get; set; }
            public LinkedEdgePath Next { get; set; }

            public EdgePath<T> Best(WeightHandler<T> weightHandler)
            {
                var best = this.Path;
                var current = this.Next;
                while (current != null)
                {
                    if (weightHandler.IsSmallerThan(current.Path.Weight, best.Weight))
                    {
                        best = current.Path;
                    }
                    current = current.Next;
                }
                return best;
            }

            public bool HasPath(EdgePath<T> path)
            {
                var current = this;
                while (current != null)
                {
                    if (current.Path.Edge == path.Edge)
                    {
                        return true;
                    }
                    current = current.Next;
                }
                return false;
            }
        }
    }

    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public class BidirectionalDykstra : BidirectionalDykstra<float>
    {
        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedDynamicGraph graph, IEnumerable<EdgePath<float>> sources, IEnumerable<EdgePath<float>> targets,
            RestrictionCollection restrictions)
            : base(graph, new DefaultWeightHandler(null), sources, targets, restrictions)
        {

        }

        /// <summary>
        /// Search forward from one vertex.
        /// </summary>
        /// <returns></returns>
        protected override void SearchForward(DirectedDynamicGraph.EdgeEnumerator edgeEnumerator, EdgePath<float> current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var currentS2 = current.GetSequence2(edgeEnumerator);
                var currentOriginal = new OriginalEdge(currentS2, current.Vertex);

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);

                    if (neighbourWeight.Direction.F)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        var neighbourTurn = new Turn(currentOriginal, neighbourNeighbour);
                        if (!edgeEnumerator.IsOriginal())
                        { // not an original edge, use the sequence.
                            neighbourTurn.Vertex3 = edgeEnumerator.GetSequence1();
                        }

                        // check u-turns and restrictions.
                        if (neighbourTurn.IsUTurn ||
                            neighbourTurn.IsRestrictedBy(_restrictions))
                        {
                            continue;
                        }

                        // build route to neighbour and check if it has been visited already.
                        var routeToNeighbour = new EdgePath<float>(
                            neighbourNeighbour, current.Weight + neighbourWeight.Weight, edgeEnumerator.IdDirected(), current);
                        LinkedEdgePath edgePath = null;
                        if (!_forwardVisits.TryGetValue(current.Vertex, out edgePath) ||
                            !edgePath.HasPath(routeToNeighbour))
                        { // this vertex has not been visited in this way before.
                            _forwardQueue.Push(routeToNeighbour, routeToNeighbour.Weight);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        /// <returns></returns>
        protected override void SearchBackward(DirectedDynamicGraph.EdgeEnumerator edgeEnumerator, EdgePath<float> current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var currentS2 = current.GetSequence2(edgeEnumerator);
                var currentOriginal = new OriginalEdge(currentS2, current.Vertex);

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);
                    if (neighbourWeight.Direction.B)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        var neighbourTurn = new Turn(currentOriginal, neighbourNeighbour);
                        if (!edgeEnumerator.IsOriginal())
                        { // not an original edge, use the sequence.
                            neighbourTurn.Vertex3 = edgeEnumerator.GetSequence1();
                        }

                        neighbourTurn.Reverse(); // this is a backward search!
                        if (neighbourTurn.IsUTurn ||
                            neighbourTurn.IsRestrictedBy(_restrictions))
                        { // don't do u-turns!
                            continue;
                        }

                        // build route to neighbour and check if it has been visited already.
                        var routeToNeighbour = new EdgePath<float>(
                            neighbourNeighbour, current.Weight + neighbourWeight.Weight, edgeEnumerator.IdDirected(), current);
                        LinkedEdgePath edgePath = null;
                        if (!_backwardVisits.TryGetValue(current.Vertex, out edgePath) ||
                            !edgePath.HasPath(routeToNeighbour))
                        { // this vertex has not been visited in this way before.
                            _backwardQueue.Push(routeToNeighbour, routeToNeighbour.Weight);
                        }
                    }
                }
            }
        }
    }
}