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
        protected readonly DirectedDynamicGraph _graph;
        protected readonly IEnumerable<EdgePath<T>> _sources;
        protected readonly IEnumerable<EdgePath<T>> _targets;
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

            _forwardTree = new PathTree();
            _backwardTree = new PathTree();
            _forwardVisits = new Dictionary<OriginalEdge, uint>();
            _backwardVisits = new Dictionary<OriginalEdge, uint>();
        }

        protected Tuple<uint, uint, float> _best;
        protected readonly PathTree _forwardTree;
        protected readonly PathTree _backwardTree;

        protected Dictionary<OriginalEdge, uint> _forwardVisits;
        protected Dictionary<OriginalEdge, uint> _backwardVisits;
        protected BinaryHeap<uint> _forwardQueue;
        protected BinaryHeap<uint> _backwardQueue;

        protected override void DoRun()
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Returns the path.
        /// </summary>
        public virtual List<uint> GetPath()
        {
            throw new NotImplementedException();
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
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            var edgeEnumerator = _graph.GetEdgeEnumerator();

            // initialize the queues.
            _forwardQueue = new BinaryHeap<uint>();
            _backwardQueue = new BinaryHeap<uint>();

            // queue sources.
            foreach (var source in _sources)
            {
                var originalSource = new OriginalEdge(source.From.Vertex, source.Vertex);
                var sourceWeight = _weightHandler.GetMetric(source.Weight);
                var sourcePointer = _forwardTree.AddSettledEdge(originalSource, originalSource, sourceWeight, Constants.NO_EDGE,
                    uint.MaxValue);

                _forwardQueue.Push(sourcePointer, sourceWeight);
            }

            // queue targets.
            foreach (var target in _targets)
            {
                var original = new OriginalEdge(target.From.Vertex, target.Vertex);
                var weight = _weightHandler.GetMetric(target.Weight);
                var pointer = _backwardTree.AddSettledEdge(original, original, weight, Constants.NO_EDGE,
                    uint.MaxValue);

                _backwardQueue.Push(pointer, weight);
            }

            // update best with current visits.
            _best = new Tuple<uint, uint, float>(uint.MaxValue, uint.MaxValue, float.MaxValue);

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
                    uint current = uint.MaxValue, previous = uint.MaxValue, edge;
                    OriginalEdge edge1 = new OriginalEdge(), edge2 = new OriginalEdge();
                    var weight = float.MaxValue;
                    while (_forwardQueue.Count > 0)
                    { // keep trying.
                        current = _forwardQueue.Pop();
                        _forwardTree.GetSettledEdge(current, out edge1, out edge2, out weight, out edge, out previous);

                        if (!_forwardVisits.ContainsKey(edge2))
                        { // has not been visited before!
                            _forwardVisits.Add(edge2, current);
                            break;
                        }
                        current = uint.MaxValue;
                    }

                    if (current != uint.MaxValue)
                    {
                        // do the forward search.
                        this.SearchForward(edgeEnumerator, current, edge2, weight);
                    }
                }

                // do a backward search.
                if (_backwardQueue.Count > 0)
                { // first check for better path.
                    // get the current queued with the smallest weight that hasn't been visited yet.
                    uint current = uint.MaxValue, previous = uint.MaxValue, edge;
                    OriginalEdge edge1 = new OriginalEdge(), edge2 = new OriginalEdge();
                    var weight = float.MaxValue;
                    while (_backwardQueue.Count > 0)
                    { // keep trying.
                        current = _backwardQueue.Pop();
                        _backwardTree.GetSettledEdge(current, out edge1, out edge2, out weight, out edge, out previous);

                        if (!_backwardVisits.ContainsKey(edge2))
                        { // has not been visited before!
                            _backwardVisits.Add(edge2, current);
                            break;
                        }
                        current = uint.MaxValue;
                    }

                    if (current != uint.MaxValue)
                    {
                        // do the backward search.
                        this.SearchBackward(edgeEnumerator, current, edge2, weight);
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
        protected virtual void SearchForward(DirectedDynamicGraph.EdgeEnumerator edgeEnumerator, uint current,
            OriginalEdge edge2, float weight)
        {
            // update restrictions.
            _restrictions.Update(edge2.Vertex2);

            // get neighbours.
            edgeEnumerator.MoveTo(edge2.Vertex2);

            // add the neighbours to the queue.
            while (edgeEnumerator.MoveNext())
            {
                var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);
                if (neighbourWeight.Direction.F)
                { // the edge is forward, and is to higher or was not contracted at all.
                    var neighbourNeighbour = edgeEnumerator.Neighbour;
                    var neighbourTurn = new Turn(edge2, neighbourNeighbour);
                    var neighbourEdge2 = new OriginalEdge(edge2.Vertex2, neighbourNeighbour);
                    if (!edgeEnumerator.IsOriginal())
                    { // not an original edge, use the sequence.
                        neighbourTurn.Vertex3 = edgeEnumerator.GetSequence1();
                        neighbourEdge2.Vertex1 = edgeEnumerator.GetSequence2();
                    }

                    // check u-turns and restrictions.
                    if (neighbourTurn.IsUTurn ||
                        neighbourTurn.IsRestrictedBy(_restrictions))
                    {
                        continue;
                    }

                    // check the backward settles for a match.
                    var neighbourEdge1 = new OriginalEdge(edge2.Vertex2, neighbourTurn.Vertex3);
                    uint backwardPointer;
                    if (_backwardVisits.TryGetValue(neighbourEdge1.Reverse(), out backwardPointer))
                    { // there is a backward match for the outgoing edge.
                        float backwardWeight;
                        OriginalEdge backwardEdge1, backwardEdge2;
                        _backwardTree.GetSettledEdge(backwardPointer, out backwardEdge1, out backwardEdge2, out backwardWeight);
                        var totalWeight = weight + backwardWeight;
                        if (totalWeight < _weightHandler.GetMetric(_best.Item3))
                        { // all ok here, turn was already checked.
                            _best = new Tuple<uint, uint, float>(current, backwardPointer, totalWeight);
                            this.HasSucceeded = true;
                        }
                    }

                    // queue forward neighbour.
                    var newWeight = weight + neighbourWeight.Weight;
                    var pointer = _forwardTree.AddSettledEdge(neighbourEdge1, neighbourEdge2, newWeight, edgeEnumerator.Id,
                        current);
                    _forwardQueue.Push(pointer, newWeight);
                }
            }
        }

        /// <summary>
        /// Search backward from one vertex.
        /// </summary>
        /// <returns></returns>
        protected virtual void SearchBackward(DirectedDynamicGraph.EdgeEnumerator edgeEnumerator, uint current,
            OriginalEdge edge2, float weight)
        {
            // update restrictions.
            _restrictions.Update(edge2.Vertex2);

            // get neighbours.
            edgeEnumerator.MoveTo(edge2.Vertex2);

            // add the neighbours to the queue.
            while (edgeEnumerator.MoveNext())
            {
                var neighbourWeight = _weightHandler.GetEdgeWeight(edgeEnumerator.Current);
                if (neighbourWeight.Direction.B)
                { // the edge is forward, and is to higher or was not contracted at all.
                    var neighbourNeighbour = edgeEnumerator.Neighbour;
                    var s1 = neighbourNeighbour;
                    var s2 = edge2.Vertex2;
                    if (!edgeEnumerator.IsOriginal())
                    { // not an original edge, use the sequence.
                        s1 = edgeEnumerator.GetSequence1();
                        s2 = edgeEnumerator.GetSequence2();
                    }
                    var neighbourTurn = new Turn(edge2, s1);
                    neighbourTurn.Reverse(); // this is a backward search.
                    var neighbourEdge1 = new OriginalEdge(edge2.Vertex2, s1);
                    var neighbourEdge2 = new OriginalEdge(s2, neighbourNeighbour);

                    // check u-turns and restrictions.
                    if (neighbourTurn.IsUTurn ||
                        neighbourTurn.IsRestrictedBy(_restrictions))
                    {
                        continue;
                    }

                    // check the forward settles for a match.
                    uint forwardPointer;
                    if (_forwardVisits.TryGetValue(neighbourEdge1.Reverse(), out forwardPointer))
                    { // there is a backward match for the outgoing edge.
                        float forwardWeight;
                        OriginalEdge forwardEdge1, forwardEdge2;
                        _forwardTree.GetSettledEdge(forwardPointer, out forwardEdge1, out forwardEdge2, out forwardWeight);
                        var totalWeight = weight + forwardWeight;
                        if (totalWeight < _best.Item3)
                        { // all ok here, turn was already checked.
                            _best = new Tuple<uint, uint, float>(forwardPointer, current, totalWeight);
                            this.HasSucceeded = true;
                        }
                    }

                    // queue forward neighbour.
                    var newWeight = weight + neighbourWeight.Weight;
                    var pointer = _backwardTree.AddSettledEdge(neighbourEdge1, neighbourEdge2, newWeight, edgeEnumerator.Id,
                        current);
                    _backwardQueue.Push(pointer, newWeight);
                }
            }
        }
        
        /// <summary>
        /// Returns the path.
        /// </summary>
        public override List<uint> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();

            var vertices = new List<uint>();
            var bestItem1 = _forwardTree.GetEdgePath(_best.Item1);
            var bestItem2 = _backwardTree.GetEdgePath(_best.Item2);
            var fromSource = bestItem1.Expand(_graph, _weightHandler, true);
            var toTarget = bestItem2.Expand(_graph, _weightHandler, false);

            // add vertices from source.
            vertices.Add(fromSource.Vertex);
            while (fromSource.From != null)
            {
                vertices.Add(fromSource.From.Vertex);
                fromSource = fromSource.From;
            }
            vertices.Reverse();

            // and add vertices to target.
            while (toTarget.From != null)
            {
                vertices.Add(toTarget.From.Vertex);
                toTarget = toTarget.From;
            }
            return vertices;
        }
    }
}