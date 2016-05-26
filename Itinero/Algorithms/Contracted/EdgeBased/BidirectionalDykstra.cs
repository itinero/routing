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
        private readonly DirectedDynamicGraph _graph;
        private readonly IEnumerable<Path> _sources;
        private readonly IEnumerable<Path> _targets;

        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedDynamicGraph graph, IEnumerable<Path> sources, IEnumerable<Path> targets)
        {
            _graph = graph;
            _sources = sources;
            _targets = targets;
        }

        private Tuple<uint, float> _best;
        private Dictionary<uint, LinkedEdgePathList> _forwardVisits; // holds the edges used to reach vertices per vertex.
        private Dictionary<uint, LinkedEdgePathList> _backwardVisits; // holds the edges used to reach vertices per vertex.
        
        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            var edgeEnumerator = _graph.GetEdgeEnumerator();

            // keep settled vertices.
            _forwardVisits = new Dictionary<uint, LinkedEdgePathList>();
            _backwardVisits = new Dictionary<uint, LinkedEdgePathList>();

            // initialize the queues.
            var forwardQueue = new BinaryHeap<EdgePath>();
            var backwardQueue = new BinaryHeap<EdgePath>();

            // queue sources.
            foreach (var source in _sources)
            {
                var sourceEdgePath = source.ToEdgePath();
                forwardQueue.Push(sourceEdgePath, sourceEdgePath.Weight);
            }

            // queue targets.
            foreach (var target in _targets)
            {
                var targetEdgePath = target.ToEdgePath();
                backwardQueue.Push(targetEdgePath, targetEdgePath.Weight);
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

                    if (current != null)
                    {
                        LinkedEdgePathList toBest;
                        if (_backwardVisits.TryGetValue(current.Vertex, out toBest))
                        {
                            // check for a new best.
                            if (current.Weight + toBest.BestWeight() < _best.Item2)
                            { // a better path was found.
                                _best = new Tuple<uint, float>(current.Vertex, current.Weight + toBest.BestWeight());
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

                    if (current != null)
                    {
                        LinkedEdgePathList toBest;
                        if (_forwardVisits.TryGetValue(current.Vertex, out toBest))
                        { // check for a new best.
                            if (current.Weight + toBest.BestWeight() < _best.Item2)
                            { // a better path was found.
                                _best = new Tuple<uint, float>(current.Vertex, current.Weight + toBest.BestWeight());
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
        private void SearchForward(BinaryHeap<EdgePath> queue, EdgePath current)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var currentVertex = current.Vertex;
                var edgeEnumerator = _graph.GetEdgeEnumerator();

                // add to the settled vertices.
                LinkedEdgePathList previousLinkedRoute = null;
                _forwardVisits.TryGetValue(currentVertex, out previousLinkedRoute);
                _forwardVisits[currentVertex] = new LinkedEdgePathList()
                {
                    Next = previousLinkedRoute,
                    Path = current
                };

                // get neighbours.
                edgeEnumerator.MoveTo(currentVertex);

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
                            var routeToNeighbour = new EdgePath(neighbourNeighbour, current.Weight + neighbourWeight,
                                edgeEnumerator.IdDirected(), current);
                            queue.Push(routeToNeighbour, routeToNeighbour.Weight);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        private void SearchBackward(BinaryHeap<EdgePath> queue, EdgePath current)
        {
            if (current != null)
            {
                // get the edge enumerator.
                var currentVertex = current.Vertex;
                var edgeEnumerator = _graph.GetEdgeEnumerator();

                // add to the settled vertices.
                LinkedEdgePathList previousLinkedRoute = null;
                _backwardVisits.TryGetValue(currentVertex, out previousLinkedRoute);
                _backwardVisits[currentVertex] = new LinkedEdgePathList()
                {
                    Next = previousLinkedRoute,
                    Path = current
                };

                // get neighbours.
                edgeEnumerator.MoveTo(currentVertex);

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
                            var routeToNeighbour = new EdgePath(neighbourNeighbour, current.Weight + neighbourWeight,
                                edgeEnumerator.IdDirected(), current);
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
        public bool TryGetForwardVisit(uint vertex, out List<EdgePath> visits)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePathList visitList;
            if (_forwardVisits.TryGetValue(vertex, out visitList))
            {
                visits =  visitList.ToList();
                return true;
            }
            visits = null;
            return false;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetBackwardVisit(uint vertex, out List<EdgePath> visits)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePathList visitList;
            if (_backwardVisits.TryGetValue(vertex, out visitList))
            {
                visits = visitList.ToList();
                return true;
            }
            visits = null;
            return false;
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath(out float weight)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePathList fromSource;
            LinkedEdgePathList toTarget;
            if (_forwardVisits.TryGetValue(_best.Item1, out fromSource) &&
                _backwardVisits.TryGetValue(_best.Item1, out toTarget))
            {
                var bestFromSource = fromSource.Best();
                var bestToTarget = toTarget.Best();

                var vertices = new List<uint>();
                weight = bestFromSource.Weight + bestToTarget.Weight;

                bestFromSource = bestFromSource.Expand(_graph);
                bestToTarget = bestToTarget.Expand(_graph);

                var pathFromSource = bestFromSource.ToPath();
                var pathToTarget = bestToTarget.ToPath();

                pathFromSource.AddToList(vertices);
                vertices.RemoveAt(vertices.Count - 1);
                pathToTarget.AddToListReverse(vertices);

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

        private class LinkedEdgePathList
        {
            public EdgePath Path { get; set; }
            public LinkedEdgePathList Next { get; set; }

            internal float BestWeight()
            {
                if (this.Next == null)
                {
                    return this.Path.Weight;
                }
                var nextBest = this.Next.BestWeight();
                if (nextBest < this.Path.Weight)
                {
                    return nextBest;
                }
                return this.Path.Weight;
            }

            internal EdgePath Best()
            {
                var best = this.Path;
                var c = this.Next;
                while (c != null)
                {
                    if (c.Path.Weight < best.Weight)
                    {
                        best = c.Path;
                    }
                    c = c.Next;
                }
                return best;
            }

            internal List<EdgePath> ToList()
            {
                var list = new List<EdgePath>();
                var c = this;
                while (c != null)
                {
                    list.Add(c.Path);
                    c = c.Next;
                }
                return list;
            }
        }
    }
}