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
using Itinero.Algorithms.Restrictions;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate a point-to-point route based on a contraction hierarchy.
    /// </summary>
    public class BidirectionalDykstra : AlgorithmBase
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly IEnumerable<EdgePath> _sources;
        private readonly IEnumerable<EdgePath> _targets;
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;
        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedDynamicGraph graph, IEnumerable<Path> sources, IEnumerable<Path> targets)
        {
            _graph = graph;
            _sources = sources.Select(x => x.ToEdgePath());
            _targets = targets.Select(x => x.ToEdgePath());
            _getRestrictions = (x) => null;
        }

        /// <summary>
        /// Creates a new contracted bidirectional router.
        /// </summary>
        public BidirectionalDykstra(DirectedDynamicGraph graph, IEnumerable<Path> sources, IEnumerable<Path> targets,
            Func<uint, IEnumerable<uint[]>> getRestrictions)
        {
            _graph = graph;
            _sources = sources.Select(x => x.ToEdgePath());
            _targets = targets.Select(x => x.ToEdgePath());
            _getRestrictions = getRestrictions;
        }

        private Tuple<EdgePath, EdgePath, float> _best;
        private Dictionary<uint, LinkedEdgePath> _forwardVisits;
        private Dictionary<uint, LinkedEdgePath> _backwardVisits;
        private BinaryHeap<EdgePath> _forwardQueue;
        private BinaryHeap<EdgePath> _backwardQueue;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            // keep settled vertices.
            _forwardVisits = new Dictionary<uint, LinkedEdgePath>();
            _backwardVisits = new Dictionary<uint, LinkedEdgePath>();

            // initialize the queues.
            _forwardQueue = new BinaryHeap<EdgePath>();
            _backwardQueue = new BinaryHeap<EdgePath>();

            // queue sources.
            foreach (var source in _sources)
            {
                _forwardQueue.Push(source, source.Weight);
            }

            // queue targets.
            foreach (var target in _targets)
            {
                _backwardQueue.Push(target, target.Weight);
            }

            // update best with current visits.
            _best = new Tuple<EdgePath, EdgePath, float>(new EdgePath(), new EdgePath(), float.MaxValue);

            // calculate stopping conditions.
            var queueBackwardWeight = _backwardQueue.PeekWeight();
            var queueForwardWeight = _forwardQueue.PeekWeight();
            while (true)
            { // keep looping until stopping conditions.
                if (_backwardQueue.Count == 0 && _forwardQueue.Count == 0)
                { // stop the search; both queues are empty.
                    break;
                }
                if (_best.Item3 < queueForwardWeight &&
                    _best.Item3 < queueBackwardWeight)
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
                        var restrictions = _getRestrictions(current.Vertex);

                        // check if there is a backward path that matches this vertex.
                        LinkedEdgePath backwardPath = null;
                        if (_backwardVisits.TryGetValue(current.Vertex, out backwardPath))
                        { // check for a new best.
                            if (restrictions != null)
                            {
                                throw new NotSupportedException();
                            }
                            else
                            { // no restrictions just choose the best vertex.
                                var best = backwardPath.Best();
                                if (current.Weight + best.Weight < _best.Item3)
                                { // a better path was found.
                                    _best = new Tuple<EdgePath, EdgePath, float>(current, best, current.Weight + best.Weight);
                                    this.HasSucceeded = true;
                                }
                            }
                        }

                        // continue the search.
                        this.SearchForward(current, restrictions);
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
                        var restrictions = _getRestrictions(current.Vertex);

                        // check if there is a backward path that matches this vertex.
                        LinkedEdgePath forwardPath = null;
                        if (_forwardVisits.TryGetValue(current.Vertex, out forwardPath))
                        { // check for a new best.
                            if (restrictions != null)
                            {
                                throw new NotSupportedException();
                            }
                            else
                            { // no restrictions just choose the best vertex.
                                var best = forwardPath.Best();
                                if (current.Weight + best.Weight < _best.Item3)
                                { // a better path was found.
                                    _best = new Tuple<EdgePath, EdgePath, float>(best, current, current.Weight + best.Weight);
                                    this.HasSucceeded = true;
                                }
                            }
                        }

                        // continue the search.
                        this.SearchBackward(current, restrictions);
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
        private void SearchForward(EdgePath current, IEnumerable<uint[]> restrictions)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var edgeEnumerator = _graph.GetEdgeEnumerator();
                var currentSequence = current.GetSequence2(edgeEnumerator);
                currentSequence = currentSequence.Append(current.Vertex);

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
                        var neighbourSequence = Constants.EMPTY_SEQUENCE;
                        if (edgeEnumerator.IsOriginal())
                        { // original edge.
                            if (currentSequence.Length > 1 && currentSequence[currentSequence.Length - 2] == neighbourNeighbour)
                            { // this is a u-turn.
                                continue;
                            }
                            if (restrictions != null)
                            {
                                neighbourSequence = currentSequence.Append(neighbourNeighbour);
                            }
                        }
                        else
                        { // not an original edge, use the sequence.
                            neighbourSequence = edgeEnumerator.GetSequence1();
                            if (currentSequence.Length > 1 && currentSequence[currentSequence.Length - 2] == neighbourSequence[0])
                            { // this is a u-turn.
                                continue;
                            }
                            if (restrictions != null)
                            {
                                neighbourSequence = currentSequence.Append(neighbourSequence);
                            }
                        }

                        if (restrictions != null)
                        { // check restrictions.
                            if (!restrictions.IsSequenceAllowed(neighbourSequence))
                            {
                                continue;
                            }
                        }

                        // build route to neighbour and check if it has been visited already.
                        var routeToNeighbour = new EdgePath(
                            neighbourNeighbour, current.Weight + neighbourWeight, edgeEnumerator.IdDirected(), current);
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
        private void SearchBackward(EdgePath current, IEnumerable<uint[]> restrictions)
        {
            if (current != null)
            { // there is a next vertex found.
                // get the edge enumerator.
                var edgeEnumerator = _graph.GetEdgeEnumerator();
                var currentSequence = current.GetSequence2(edgeEnumerator);
                currentSequence = currentSequence.Append(current.Vertex);

                // get neighbours.
                edgeEnumerator.MoveTo(current.Vertex);

                // add the neighbours to the queue.
                while (edgeEnumerator.MoveNext())
                {
                    float neighbourWeight;
                    bool? neighbourDirection;
                    ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out neighbourWeight, out neighbourDirection);
                    if (neighbourDirection == null || !neighbourDirection.Value)
                    { // the edge is forward, and is to higher or was not contracted at all.
                        var neighbourNeighbour = edgeEnumerator.Neighbour;
                        var neighbourSequence = Constants.EMPTY_SEQUENCE;
                        if (edgeEnumerator.IsOriginal())
                        { // original edge.
                            if (currentSequence.Length > 1 && currentSequence[currentSequence.Length - 2] == neighbourNeighbour)
                            { // this is a u-turn.
                                continue;
                            }
                            if (restrictions != null)
                            {
                                neighbourSequence = currentSequence.Append(neighbourNeighbour);
                            }
                        }
                        else
                        { // not an original edge, use the sequence.
                            neighbourSequence = edgeEnumerator.GetSequence1();
                            if (currentSequence.Length > 1 && currentSequence[currentSequence.Length - 2] == neighbourSequence[0])
                            { // this is a u-turn.
                                continue;
                            }
                            if (restrictions != null)
                            {
                                neighbourSequence = currentSequence.Append(neighbourSequence);
                            }
                        }

                        if (restrictions != null)
                        { // check restrictions.
                            neighbourSequence.Reverse();
                            if (!restrictions.IsSequenceAllowed(neighbourSequence))
                            {
                                continue;
                            }
                        }
                        
                        // build route to neighbour and check if it has been visited already.
                        var routeToNeighbour = new EdgePath(
                            neighbourNeighbour, current.Weight + neighbourWeight, edgeEnumerator.IdDirected(), current);
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
        public bool TryGetForwardVisit(uint vertex, out EdgePath visit)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePath path;
            if (_forwardVisits.TryGetValue(vertex, out path))
            {
                visit = path.Best();
                return true;
            }
            visit = null;
            return false;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetBackwardVisit(uint vertex, out EdgePath visit)
        {
            this.CheckHasRunAndHasSucceeded();

            LinkedEdgePath path;
            if (_backwardVisits.TryGetValue(vertex, out path))
            {
                visit = path.Best();
                return true;
            }
            visit = null;
            return false;
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        public List<uint> GetPath(out float weight)
        {
            this.CheckHasRunAndHasSucceeded();

            var vertices = new List<uint>();
            var fromSource = _best.Item1.Expand(_graph).ToPath();
            var toTarget = _best.Item2.Expand(_graph).ToPath();
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

        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath()
        {
            float weight;
            return this.GetPath(out weight);
        }
        
        private class LinkedEdgePath
        {
            public EdgePath Path { get; set; }
            public LinkedEdgePath Next { get; set; }

            public EdgePath Best()
            {
                var best = this.Path;
                var current = this.Next;
                while (current != null)
                {
                    if (current.Path.Weight < best.Weight)
                    {
                        best = current.Path;
                    }
                    current = current.Next;
                }
                return best;
            }

            public bool HasPath(EdgePath path)
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
}