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

using System;
using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs.Directed;
using Itinero.Logging;
using System.Collections.Generic;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Restrictions;
using System.Linq;
using Itinero.Algorithms.Weights;
using System.Threading;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly IPriorityCalculator _priorityCalculator;
        private readonly IWitnessCalculator<T> _witnessCalculator;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;
        private const float E = 0.1f;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedDynamicGraph graph, IPriorityCalculator priorityCalculator, IWitnessCalculator<T> witnessCalculator,
            WeightHandler<T> weightHandler, Func<uint, IEnumerable<uint[]>> getRestrictions)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _priorityCalculator = priorityCalculator;
            _witnessCalculator = witnessCalculator;
            _getRestrictions = getRestrictions;
            _weightHandler = weightHandler;
        }

        private BinaryHeap<uint> _queue; // the vertex-queue.
        private BitArray32 _contractedFlags; // contains flags for contracted vertices.
        private BitArray32 _restrictionFlags; // contains flags for restricted vertices.

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _queue = new BinaryHeap<uint>((uint)_graph.VertexCount);
            _contractedFlags = new BitArray32(_graph.VertexCount);
            _restrictionFlags = new BitArray32(_graph.VertexCount);
            _missesQueue = new Queue<bool>();

            // build restrictions flags.
            for(uint i= 0; i < _graph.VertexCount; i++)
            {
                var restrictions = _getRestrictions(i);
                if (restrictions != null && restrictions.Any())
                {
                    _restrictionFlags[i] = true;
                }
            }

            // remove all edges that have witness paths, meaning longer than the shortest path
            // between the two ending vertices.
            this.RemoveWitnessedEdges();

            // build queue.
            this.CalculateQueue();

            var next = this.SelectNext();
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            while (next != null)
            {
                // contract...
                this.Contract(next.Value);

                // ... and select next.
                next = this.SelectNext();

                // calculate and log progress.
                var progress = (float)(System.Math.Floor(((double)current / (double)total) * 10000) / 100.0);
                if (progress < 99)
                {
                    progress = (float)(System.Math.Floor(((double)current / (double)total) * 100) / 1.0);
                }
                if (progress != latestProgress)
                {
                    latestProgress = progress;

                    int totaEdges = 0;
                    int totalUncontracted = 0;
                    int maxCardinality = 0;
                    var neighbourCount = new Dictionary<uint, int>();
                    for (uint v = 0; v < _graph.VertexCount; v++)
                    {
                        if (!_contractedFlags[v])
                        {
                            neighbourCount.Clear();
                            var edges = _graph.GetEdgeEnumerator(v);
                            if (edges != null)
                            {
                                var edgesCount = edges.Count();
                                totaEdges = edgesCount + totaEdges;
                                if (maxCardinality < edgesCount)
                                {
                                    maxCardinality = edgesCount;
                                }
                            }
                            totalUncontracted++;
                        }
                    }

                    var density = (double)totaEdges / (double)totalUncontracted;
                    _logger.Log(TraceEventType.Information, "Preprocessing... {0}% [{1}/{2}] {3}q #{4} max {5}",
                        progress, current, total, _queue.Count, density, maxCardinality);
                }
                current++;
            }
        }

        /// <summary>
        /// Calculates the entire queue.
        /// </summary>
        private void CalculateQueue()
        {
            _logger.Log(TraceEventType.Information, "Calculating queue...");

            _queue.Clear();
            for (uint v = 0; v < _graph.VertexCount; v++)
            {
                if (!_contractedFlags[v])
                {
                    _queue.Push(v, _priorityCalculator.Calculate(
                        _contractedFlags, _getRestrictions, v));
                }
            }
        }

        /// <summary>
        /// Remove all witnessed edges.
        /// </summary>
        private void RemoveWitnessedEdges()
        {
            //_logger.Log(TraceEventType.Information, "Removing witnessed edges...");

            //var edges = new List<DynamicEdge>();
            //var weights = new List<float>();
            //var targets = new List<uint>();
            //for (uint vertex = 0; vertex < _graph.VertexCount; vertex++)
            //{
            //    if (_restrictionFlags[vertex])
            //    { // don't remove witnessed edges when there is a potential restriction.
            //        continue;
            //    }

            //    edges.Clear();
            //    weights.Clear();
            //    targets.Clear();

            //    edges.AddRange(_graph.GetEdgeEnumerator(vertex));

            //    var forwardWitnesses = new EdgePath<float>[edges.Count];
            //    var backwardWitnesses = new EdgePath<float>[edges.Count];
            //    for (var i = 0; i < edges.Count; i++)
            //    {
            //        var edge = edges[i];

            //        float edgeWeight;
            //        bool? edgeDirection;
            //        ContractedEdgeDataSerializer.Deserialize(edge.Data[0],
            //            out edgeWeight, out edgeDirection);
            //        var edgeCanMoveForward = edgeDirection == null || edgeDirection.Value;
            //        var edgeCanMoveBackward = edgeDirection == null || !edgeDirection.Value;

            //        if (_restrictionFlags[edge.Neighbour])
            //        { // don't remove shortcuts when there is a potential restriction.
            //            forwardWitnesses[i] = new EdgePath<float>();
            //            backwardWitnesses[i] = new EdgePath<float>();
            //            weights.Add(0);
            //            targets.Add(edge.Neighbour);
            //        }
            //        else
            //        {
            //            if (!edgeCanMoveForward)
            //            {
            //                forwardWitnesses[i] = new EdgePath<float>();
            //            }
            //            if (!edgeCanMoveBackward)
            //            {
            //                backwardWitnesses[i] = new EdgePath<float>();
            //            }
            //            weights.Add(edgeWeight);
            //            targets.Add(edge.Neighbour);
            //        }
            //    }

            //    // calculate all witness paths.
            //    _witnessCalculator.Calculate(_graph, _getRestrictions, vertex, targets, weights,
            //        ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);

            //    // check witness paths.
            //    for (var i = 0; i < edges.Count; i++)
            //    {
            //        if (forwardWitnesses[i].Vertex != Constants.NO_VERTEX && forwardWitnesses[i].Weight < weights[i] && 
            //            backwardWitnesses[i].Vertex != Constants.NO_VERTEX && backwardWitnesses[i].Weight < weights[i])
            //        { // in both directions the edge does not represent the shortest path.
            //            _graph.RemoveEdge(vertex, targets[i]);
            //        }
            //        else if (forwardWitnesses[i].Vertex != Constants.NO_VERTEX && forwardWitnesses[i].Weight < weights[i])
            //        { // only in forward direction is this edge useless.
            //            _graph.RemoveEdge(vertex, targets[i]);
            //            _graph.AddEdge(vertex, targets[i], weights[i], false);
            //        }
            //        else if (backwardWitnesses[i].Vertex != Constants.NO_VERTEX && backwardWitnesses[i].Weight < weights[i])
            //        { // only in backward direction is this edge useless.
            //            _graph.RemoveEdge(vertex, targets[i]);
            //            _graph.AddEdge(vertex, targets[i], weights[i], true);
            //        }
            //    }
            //}
        }

        private int _k = 20; // The amount of queue 'misses' to recalculated.
        private int _misses; // Holds a counter of all misses.
        private Queue<bool> _missesQueue; // Holds the misses queue.

        /// <summary>
        /// Select the next vertex to contract.
        /// </summary>
        /// <returns></returns>
        private uint? SelectNext()
        {
            // first check the first of the current queue.
            while (_queue.Count > 0)
            { // get the first vertex and check.
                var first = _queue.Peek();
                if (_contractedFlags[first])
                { // already contracted, priority was updated.
                    _queue.Pop();
                    continue;
                }
                var queuedPriority = _queue.PeekWeight();

                // the lazy updating part!
                // calculate priority
                var priority = _priorityCalculator.Calculate(_contractedFlags, _getRestrictions, first);
                if (priority != queuedPriority)
                { // a succesfull update.
                    _missesQueue.Enqueue(true);
                    _misses++;
                }
                else
                { // an unsuccessfull update.
                    _missesQueue.Enqueue(false);
                }
                if (_missesQueue.Count > _k)
                { // dequeue and update the misses.
                    if (_missesQueue.Dequeue())
                    {
                        _misses--;
                    }
                }

                // if the misses are _k
                if (_misses == _k)
                { // recalculation.
                    this.CalculateQueue();

                    // clear misses.
                    _missesQueue.Clear();
                    _misses = 0;
                }
                else
                { // no recalculation.
                    if (priority != queuedPriority)
                    { // re-enqueue.
                        _queue.Pop();
                        _queue.Push(first, priority);
                    }
                    else
                    { // try to select another.
                        return _queue.Pop();
                    }
                }
            }
            return null; // all nodes have been contracted.
        }

        /// <summary>
        /// Contracts the given vertex.
        /// </summary>
        private void Contract(uint vertex)
        {
            // get and keep edges.
            var enumerator = _graph.GetEdgeEnumerator(vertex);
            var edges = new List<DynamicEdge>(enumerator);

            // check if this vertex has a potential restrictions.
            var hasRestrictions = _restrictionFlags[vertex];

            // loop over all edge-pairs once.
            for (var j = 1; j < edges.Count; j++)
            {
                var edge1 = edges[j];
                var edge1Sequence2 = edges[j].GetSequence2();
                if (edge1Sequence2.Length == 0)
                {
                    edge1Sequence2 = new uint[] { vertex };
                }
                
                bool? edge1Direction;
                var edge1Weight = _weightHandler.GetEdgeWeight(edge1, out edge1Direction);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                // figure out what witness paths to calculate.
                var forwardWitnesses = new EdgePath<T>[j];
                var backwardWitnesses = new EdgePath<T>[j];
                var targets = new List<uint>(j);
                var targetWeights = new List<T>(j);
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    bool? edge2Direction;
                    var edge2Weight = _weightHandler.GetEdgeWeight(edge2, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    // use witness flags to represent impossible routes.
                    if (!(edge1CanMoveBackward && edge2CanMoveForward))
                    {
                        forwardWitnesses[k] = new EdgePath<T>();
                    }
                    if (!(edge1CanMoveForward && edge2CanMoveBackward))
                    {
                        backwardWitnesses[k] = new EdgePath<T>();
                    }

                    targets.Add(edge2.Neighbour);
                    if (hasRestrictions)
                    {
                        targetWeights.Add(_weightHandler.Infinite);
                    }
                    else
                    {
                        targetWeights.Add(_weightHandler.Add(edge1Weight, edge2Weight));
                    }
                }

                // calculate all witness paths.
                _witnessCalculator.Calculate(_graph, _getRestrictions, edge1.Neighbour, targets, targetWeights, ref forwardWitnesses,
                    ref backwardWitnesses, Constants.NO_VERTEX);

                // get all sequences where needed.
                var s1forward = new uint[forwardWitnesses.Length][];
                var s2forward = new uint[forwardWitnesses.Length][];
                var s1backward = new uint[backwardWitnesses.Length][];
                var s2backward = new uint[backwardWitnesses.Length][];
                for (var k = 0; k < j; k++)
                {
                    var edge2Sequence2 = edges[k].GetSequence2();
                    if (edge2Sequence2.Length == 0)
                    {
                        edge2Sequence2 = new uint[] { vertex };
                    }

                    if (forwardWitnesses[k].HasVertex(vertex))
                    { // get forward sequences.
                        s1forward[k] = forwardWitnesses[k].GetSequence1(enumerator, 1);
                        s2forward[k] = forwardWitnesses[k].GetSequence2(enumerator, 1);

                        if (!s1forward[k].IsSequenceIdentical(edge1Sequence2) ||
                            !s2forward[k].IsSequenceIdentical(edge2Sequence2))
                        { // start and end sequences of shortest paths need to match.
                            s1forward[k] = null;
                            s2forward[k] = null;
                        }
                    }
                    if (backwardWitnesses[k].HasVertex(vertex))
                    { // get backward sequences.
                        s1backward[k] = backwardWitnesses[k].GetSequence1(enumerator, 1);
                        s2backward[k] = backwardWitnesses[k].GetSequence2(enumerator, 1);

                        if (!s1backward[k].IsSequenceIdentical(edge1Sequence2) ||
                            !s2backward[k].IsSequenceIdentical(edge2Sequence2))
                        { // start and end sequences of shortest paths need to match.
                            s1backward[k] = null;
                            s2backward[k] = null;
                        }
                    }
                }

                // add contracted edges if needed.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    if (edge1.Neighbour == edge2.Neighbour)
                    { // do not try to add a shortcut between identical vertices.
                        continue;
                    }

                    //if (s1forward[k] != null && s1backward[k] != null &&
                    //    System.Math.Abs(_weightHandler.GetMetric(forwardWitnesses[k].Weight) - _weightHandler.GetMetric(backwardWitnesses[k].Weight)) < E)
                    //{ // paths in both direction are possible and with the same weight, add just one edge in each direction.
                    //    s1backward[k].Reverse();
                    //    s2backward[k].Reverse();
                    //    _weightHandler.AddOrUpdateEdge(_graph, edge1.Neighbour, edge2.Neighbour, vertex, null,
                    //        forwardWitnesses[k].Weight, s1forward[k], s2forward[k]);
                    //    //_graph.AddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                    //    //    forwardWitnesses[k].Weight, null, vertex, s1forward[k], s2forward[k]);
                    //    _weightHandler.AddOrUpdateEdge(_graph, edge2.Neighbour, edge1.Neighbour, vertex, null,
                    //        backwardWitnesses[k].Weight, s2backward[k], s1backward[k]);
                    //    //_graph.AddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                    //    //    backwardWitnesses[k].Weight, null, vertex, s2backward[k], s1backward[k]);
                    //}
                    //else
                    //{ // add two edge per direction.
                        if (s1forward[k] != null)
                        { // add forward edge.
                            _weightHandler.AddOrUpdateEdge(_graph, edge1.Neighbour, edge2.Neighbour, vertex, true,
                                forwardWitnesses[k].Weight, s1forward[k], s2forward[k]);
                            //_graph.AddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            //    forwardWitnesses[k].Weight, true, vertex, s1forward[k], s2forward[k]);
                            s1forward[k].Reverse();
                            s2forward[k].Reverse();
                            _weightHandler.AddOrUpdateEdge(_graph, edge2.Neighbour, edge1.Neighbour, vertex, false,
                                forwardWitnesses[k].Weight, s2forward[k], s1forward[k]);
                            //_graph.AddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            //    forwardWitnesses[k].Weight, false, vertex, s2forward[k], s1forward[k]);
                        }
                        if (s1backward[k] != null)
                        { // add forward edge.
                            _weightHandler.AddOrUpdateEdge(_graph, edge1.Neighbour, edge2.Neighbour, vertex, false,
                                backwardWitnesses[k].Weight, s1backward[k], s2backward[k]);
                            //_graph.AddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            //    backwardWitnesses[k].Weight, false, vertex, s2backward[k], s1backward[k]);
                            s1backward[k].Reverse();
                            s2backward[k].Reverse();
                            _weightHandler.AddOrUpdateEdge(_graph, edge2.Neighbour, edge1.Neighbour, vertex, true,
                                backwardWitnesses[k].Weight, s2backward[k], s1backward[k]);
                            //_graph.AddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            //    backwardWitnesses[k].Weight, true, vertex, s1backward[k], s2backward[k]);
                        }
                    //}
                }
            }

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < edges.Count)
            {
                _graph.RemoveEdge(edges[i].Neighbour, vertex);

                if (_contractedFlags[edges[i].Neighbour])
                { // neighbour was already contracted, remove 'downward' edge and exclude it.
                    _graph.RemoveEdge(vertex, edges[i].Neighbour);
                    edges.RemoveAt(i);
                }
                else
                { // move to next edge.
                    i++;
                }
            }

            _contractedFlags[vertex] = true;
            _priorityCalculator.NotifyContracted(vertex);
        }
    }

    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public sealed class HierarchyBuilder : HierarchyBuilder<float>
    {
        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedDynamicGraph graph, IPriorityCalculator priorityCalculator, IWitnessCalculator<float> witnessCalculator,
            Func<uint, IEnumerable<uint[]>> getRestrictions)
            : base(graph, priorityCalculator, witnessCalculator, new DefaultWeightHandler(null), getRestrictions)
        {

        }
    }
}