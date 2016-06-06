// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.PriorityQueues;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public class BidirectionalDykstra : AlgorithmBase
    {
        private readonly DirectedMetaGraph _graph;
        private readonly IEnumerable<Path> _sources;
        private readonly IEnumerable<Path> _targets;

        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedMetaGraph graph, IEnumerable<Path> sources, IEnumerable<Path> targets)
        {
            _graph = graph;
            _sources = sources;
            _targets = targets;
        }

        private Tuple<uint, float> _best;
        private Dictionary<uint, Path> _forwardVisits;
        private Dictionary<uint, Path> _backwardVisits;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            // keep settled vertices.
            _forwardVisits = new Dictionary<uint, Path>();
            _backwardVisits = new Dictionary<uint, Path>();

            // initialize the queues.
            var forwardQueue = new BinaryHeap<Path>();
            var backwardQueue = new BinaryHeap<Path>();

            // queue sources.
            foreach (var source in _sources)
            {
                forwardQueue.Push(source, source.Weight);
            }

            // queue targets.
            foreach (var target in _targets)
            {
                backwardQueue.Push(target, target.Weight);
            }

            // update best with current visits.
            _best = new Tuple<uint, float>(Constants.NO_VERTEX, float.MaxValue);

            // calculate stopping conditions.
            var queueBackwardWeight = backwardQueue.PeekWeight();
            var queueForwardWeight = forwardQueue.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (backwardQueue.Count == 0 && forwardQueue.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (_best.Item2 < queueForwardWeight &&
                    _best.Item2 < queueBackwardWeight)
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
                        Path toBest;
                        if (_backwardVisits.TryGetValue(current.Vertex, out toBest))
                        { // check for a new best.
                            if (current.Weight + toBest.Weight < _best.Item2)
                            { // a better path was found.
                                _best = new Tuple<uint, float>(current.Vertex, current.Weight + toBest.Weight);
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
                        Path toBest;
                        if (_forwardVisits.TryGetValue(current.Vertex, out toBest))
                        { // check for a new best.
                            if (current.Weight + toBest.Weight < _best.Item2)
                            { // a better path was found.
                                _best = new Tuple<uint, float>(current.Vertex, current.Weight + toBest.Weight);
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
        private void SearchForward(BinaryHeap<Path> queue, Path current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var edgeEnumerator = _graph.Graph.GetEdgeEnumerator();

                // add to the settled vertices.
                Path previousLinkedRoute;
                if (_forwardVisits.TryGetValue(current.Vertex, out previousLinkedRoute))
                {
                    if (previousLinkedRoute.Weight > current.Weight)
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
                    float neighbourWeight;
                    bool? neighbourDirection;
                    ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out neighbourWeight, out neighbourDirection);
                    if (neighbourDirection == null || neighbourDirection.Value)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        if (!_forwardVisits.ContainsKey(neighbourNeighbour))
                        { // if not yet settled.
                            var routeToNeighbour = new Path(
                                neighbourNeighbour, current.Weight + neighbourWeight, current);
                            queue.Push(routeToNeighbour, routeToNeighbour.Weight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        private void SearchBackward(BinaryHeap<Path> queue, Path current)
        {
            if (current != null)
            {
                // get the edge enumerator.
                var edgeEnumerator = _graph.Graph.GetEdgeEnumerator();

                // add to the settled vertices.
                Path previousLinkedRoute;
                if (_backwardVisits.TryGetValue(current.Vertex, out previousLinkedRoute))
                {
                    if (previousLinkedRoute.Weight > current.Weight)
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
                    float neighbourWeight;
                    bool? neighbourDirection;
                    ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out neighbourWeight, out neighbourDirection);

                    if (neighbourDirection == null || !neighbourDirection.Value)
                    { // the edge is backward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        if (!_backwardVisits.ContainsKey(neighbourNeighbour))
                        { // if not yet settled.
                            var routeToNeighbour = new Path(
                                neighbourNeighbour, current.Weight + neighbourWeight, current);
                            queue.Push(routeToNeighbour, routeToNeighbour.Weight);
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
        public bool TryGetForwardVisit(uint vertex, out Path visit)
        {
            this.CheckHasRunAndHasSucceeded();

            return _forwardVisits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetBackwardVisit(uint vertex, out Path visit)
        {
            this.CheckHasRunAndHasSucceeded();

            return _backwardVisits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath(out float weight)
        {
            this.CheckHasRunAndHasSucceeded();

            Path fromSource;
            Path toTarget;
            if (_forwardVisits.TryGetValue(_best.Item1, out fromSource) &&
                _backwardVisits.TryGetValue(_best.Item1, out toTarget))
            {
                var vertices = new List<uint>();
                weight = fromSource.Weight + toTarget.Weight;

                // add vertices from source.
                vertices.Add(fromSource.Vertex);
                while (fromSource.From != null)
                {
                    if (fromSource.From.Vertex != Constants.NO_VERTEX)
                    { // this should be the end of the path.
                        _graph.ExpandEdge(fromSource.From.Vertex, fromSource.Vertex, vertices, false, true);
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
                        _graph.ExpandEdge(toTarget.From.Vertex, toTarget.Vertex, vertices, false, false);
                    }
                    vertices.Add(toTarget.From.Vertex);
                    toTarget = toTarget.From;
                }
                return vertices;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath()
        {
            float weight;
            return this.GetPath(out weight);
        }
    }
}