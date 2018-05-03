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
using Itinero.Algorithms.Contracted.Witness;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using Itinero.Logging;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly IPriorityCalculator _priorityCalculator;
        private readonly IWitnessCalculator _witnessCalculator;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        private readonly WeightHandler<T> _weightHandler;
        private readonly RestrictionCollection _restrictions;

        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedMetaGraph graph, RestrictionCollection restrictions, IPriorityCalculator priorityCalculator, IWitnessCalculator witnessCalculator,
            WeightHandler<T> weightHandler)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _priorityCalculator = priorityCalculator;
            _witnessCalculator = witnessCalculator;
            _weightHandler = weightHandler;
            _restrictions = restrictions;
        }

        private BinaryHeap<uint> _queue; // the vertex-queue.
        private BitArray32 _contractedFlags; // contains flags for contracted vertices.

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _queue = new BinaryHeap<uint>((uint)_graph.VertexCount);
            _contractedFlags = new BitArray32(_graph.VertexCount);
            _missesQueue = new Queue<bool>();

            // remove all edges that have witness paths, meaning longer than the shortest path
            // between the two ending vertices.
            this.RemoveWitnessedEdges();

            // build queue.
            this.CalculateQueue();

            var next = this.SelectNext();
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            while(next != null)
            {
                // contract...
                this.Contract(next.Value);

                // ... and select next.
                next = this.SelectNext();

                // calculate and log progress.
                var progress = (float)(System.Math.Floor(((double)current / (double)total) * 10000) / 100.0);
                if(progress < 99)
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
                                var edgesCount = edges.Count;
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
                if(!_contractedFlags[v])
                {
                    _queue.Push(v, _priorityCalculator.Calculate(
                        _contractedFlags, v));
                }
            }
        }

        /// <summary>
        /// Remove all witnessed edges.
        /// </summary>
        private void RemoveWitnessedEdges()
        {
            _logger.Log(TraceEventType.Information, "Removing witnessed edges...");

            var edges = new List<MetaEdge>();
            var weights = new List<T>();
            var metrics = new List<float>();
            var targets = new List<uint>();
            for (uint vertex = 0; vertex < _graph.VertexCount; vertex++)
            {
                edges.Clear();
                weights.Clear();
                metrics.Clear();
                targets.Clear();

                edges.AddRange(_graph.GetEdgeEnumerator(vertex));

                var forwardWitnesses = new bool[edges.Count];
                var backwardWitnesses = new bool[edges.Count];
                for (var i = 0; i < edges.Count; i++)
                {
                    var edge = edges[i];

                    bool? edgeDirection;
                    var edgeWeight = _weightHandler.GetEdgeWeight(edge, out edgeDirection);
                    
                    var edgeCanMoveForward = edgeDirection == null || edgeDirection.Value;
                    var edgeCanMoveBackward = edgeDirection == null || !edgeDirection.Value;

                    forwardWitnesses[i] = !edgeCanMoveForward;
                    backwardWitnesses[i] = !edgeCanMoveBackward;
                    weights.Add(edgeWeight);
                    metrics.Add(_weightHandler.GetMetric(edgeWeight));
                    targets.Add(edge.Neighbour);
                }

                // calculate all witness paths.
                _witnessCalculator.Calculate(_graph.Graph, vertex, targets, metrics, 
                    ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);

                // check witness paths.
                for (var i = 0; i < edges.Count; i++)
                {
                    if(forwardWitnesses[i] && backwardWitnesses[i])
                    { // in both directions the edge does not represent the shortest path.
                        _graph.RemoveEdge(vertex, targets[i]);
                    }
                    else if(forwardWitnesses[i])
                    { // only in forward direction is this edge useless.
                        _graph.RemoveEdge(vertex, targets[i]);
                        _weightHandler.AddEdge(_graph, vertex, targets[i], Constants.NO_VERTEX, false, weights[i]);
                        //_graph.AddEdge(vertex, targets[i], weights[i], false, Constants.NO_VERTEX);
                    }
                    else if (backwardWitnesses[i])
                    { // only in backward direction is this edge useless.
                        _graph.RemoveEdge(vertex, targets[i]);
                        _weightHandler.AddEdge(_graph, vertex, targets[i], Constants.NO_VERTEX, true, weights[i]);
                    }
                }
            }
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
                var priority = _priorityCalculator.Calculate(_contractedFlags, first);
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
            var edges = new List<MetaEdge>(_graph.GetEdgeEnumerator(vertex));

            // remove 'downward' edge to vertex.
            var i = 0;
            while(i < edges.Count)
            {
                _graph.RemoveEdge(edges[i].Neighbour, vertex);

                if(_contractedFlags[edges[i].Neighbour])
                { // neighbour was already contracted, remove 'downward' edge and exclude it.
                    _graph.RemoveEdge(vertex, edges[i].Neighbour);
                    edges.RemoveAt(i);
                }
                else
                { // move to next edge.
                    i++;
                }
            }

            // check for a restriction, if vertex is restricted don't add shortcuts.
            if (_restrictions != null &&
                _restrictions.Update(vertex))
            {
                if (_restrictions.Restricts(vertex))
                {
                    return;
                }
            }

            // loop over all edge-pairs once.
            for(var j = 1; j < edges.Count; j++)
            {
                var edge1 = edges[j];

                bool? edge1Direction;
                var edge1Weight = _weightHandler.GetEdgeWeight(edge1, out edge1Direction);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                // figure out what witness paths to calculate.
                var forwardWitnesses = new bool[j];
                var backwardWitnesses = new bool[j];
                var targets = new List<uint>(j);
                var targetWeights = new List<T>(j);
                var targetMetrics = new List<float>(j);
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];
                    
                    bool? edge2Direction;
                    var edge2Weight = _weightHandler.GetEdgeWeight(edge2, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    // use witness flags to represent impossible routes.
                    forwardWitnesses[k] = !(edge1CanMoveBackward && edge2CanMoveForward); 
                    backwardWitnesses[k] = !(edge1CanMoveForward && edge2CanMoveBackward);

                    targets.Add(edge2.Neighbour);
                    var totalWeight = _weightHandler.Add(edge1Weight, edge2Weight);
                    targetWeights.Add(totalWeight);
                    targetMetrics.Add(_weightHandler.GetMetric(totalWeight));
                }

                // calculate all witness paths.
                _witnessCalculator.Calculate(_graph.Graph, edge1.Neighbour, targets, targetMetrics, ref forwardWitnesses, 
                    ref backwardWitnesses, vertex);

                // add contracted edges if needed.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    if (edge1.Neighbour == edge2.Neighbour)
                    { // do not try to add a shortcut between identical vertices.
                        continue;
                    }

                    if(!forwardWitnesses[k] && !backwardWitnesses[k])
                    { // add bidirectional edge.
                        _weightHandler.AddOrUpdateEdge(_graph, edge1.Neighbour, edge2.Neighbour, 
                            vertex, null, targetWeights[k]);
                        //_graph.AddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                        //    targetWeights[k], null, vertex);
                        _weightHandler.AddOrUpdateEdge(_graph, edge2.Neighbour, edge1.Neighbour,
                            vertex, null, targetWeights[k]);
                        //_graph.AddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                        //    targetWeights[k], null, vertex);
                    }
                    else if(!forwardWitnesses[k])
                    { // add forward edge.
                        _weightHandler.AddOrUpdateEdge(_graph, edge1.Neighbour, edge2.Neighbour,
                            vertex, true, targetWeights[k]);
                        //_graph.AddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                        //    targetWeights[k], true, vertex);
                        _weightHandler.AddOrUpdateEdge(_graph, edge2.Neighbour, edge1.Neighbour,
                            vertex, false, targetWeights[k]);
                        //_graph.AddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                        //    targetWeights[k], false, vertex);
                    }
                    else if (!backwardWitnesses[k])
                    { // add forward edge.
                        _weightHandler.AddOrUpdateEdge(_graph, edge1.Neighbour, edge2.Neighbour,
                            vertex, false, targetWeights[k]);
                        //_graph.AddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                        //    targetWeights[k], false, vertex);
                        _weightHandler.AddOrUpdateEdge(_graph, edge2.Neighbour, edge1.Neighbour,
                            vertex, true, targetWeights[k]);
                        //_graph.AddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour, 
                        //    targetWeights[k], true, vertex);
                    }
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
        public HierarchyBuilder(DirectedMetaGraph graph, RestrictionCollection restrictions, IPriorityCalculator priorityCalculator, IWitnessCalculator witnessCalculator)
            : base(graph, restrictions, priorityCalculator, witnessCalculator, new DefaultWeightHandler(null)) // the get factor function is never called.
        {

        }
    }
}