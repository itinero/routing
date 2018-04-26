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
using Itinero.Algorithms.Contracted.Dual.Witness;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using Itinero.Logging;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder<T> : AlgorithmBase
        where T : struct
    {
        protected readonly DirectedMetaGraph _graph;
        private readonly DykstraWitnessCalculator<T> _witnessCalculator;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        protected readonly WeightHandler<T> _weightHandler;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        protected VertexInfo<T> _vertexInfo;
        protected Dictionary<uint, VertexInfo<T>> _vertexInfoCache = new Dictionary<uint, VertexInfo<T>>();
        public const float E = 0.1f;

        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedMetaGraph graph, DykstraWitnessCalculator<T> witnessCalculator,
            WeightHandler<T> weightHandler)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _witnessCalculator = witnessCalculator;
            _weightHandler = weightHandler;

            _vertexInfo = new VertexInfo<T>();
            _depth = new Dictionary<long, int>();
            _contractionCount = new Dictionary<uint, int>();

            this.DifferenceFactor = 5;
            this.DepthFactor = 5;
            this.ContractedFactor = 5;
        }

        private BinaryHeap<uint> _queue; // the vertex-queue.
        protected BitArray32 _contractedFlags; // contains flags for contracted vertices.
        protected BitArray32 _noWitnessesFlags; // contains flags for vertices without any witnesses.

        /// <summary>
        /// Gets or sets the difference factor.
        /// </summary>
        public int DifferenceFactor { get; set; }

        /// <summary>
        /// Gets or sets the depth factor.
        /// </summary>
        public int DepthFactor { get; set; }

        /// <summary>
        /// Gets or sets the contracted factor.
        /// </summary>
        public int ContractedFactor { get; set; }

        /// <summary>
        /// Updates the vertex info object with the given vertex.
        /// </summary>
        /// <returns>True if witness paths have been found.</returns>
        private bool UpdateVertexInfo(uint v)
        {
            var contracted = 0;
            var depth = 0;

            VertexInfo<T> vertexInfo;
            if (_vertexInfoCache.TryGetValue(v, out vertexInfo))
            {
                _vertexInfo = vertexInfo;

                _contractionCount.TryGetValue(v, out contracted);
                _vertexInfo.ContractedNeighbours = contracted;
                _depth.TryGetValue(v, out depth);
                _vertexInfo.Depth = depth;

                return true;
            }
            _vertexInfo = new VertexInfo<T>();

            // update vertex info.
            _vertexInfo.Clear();
            _vertexInfo.Vertex = v;
            _contractionCount.TryGetValue(v, out contracted);
            _vertexInfo.ContractedNeighbours = contracted;
            _depth.TryGetValue(v, out depth);
            _vertexInfo.Depth = depth;

            // calculate shortcuts and witnesses.
            _vertexInfo.AddRelevantEdges(_graph.GetEdgeEnumerator());
            _vertexInfo.BuildShortcuts(_weightHandler);
            //var witnessed = _vertexInfo.Shortcuts.RemoveWitnessed(v, _witnessCalculator);
            var witnessed = false;
            if (!_noWitnessesFlags[v])
            { // we're not sure there are no witnesses, so check by recalculating.
                witnessed = _vertexInfo.Shortcuts.RemoveWitnessed(v, _witnessCalculator);
                if (!witnessed)
                { // there are no witnesses.
                    _noWitnessesFlags[v] = true;
                }
                else
                { // keep cache when witnesses.
                    _vertexInfoCache[v] = _vertexInfo.Clone() as VertexInfo<T>;
                }
            }

            return witnessed;
        }

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            _queue = new BinaryHeap<uint>((uint)_graph.VertexCount);
            _contractedFlags = new BitArray32(_graph.VertexCount);
            _noWitnessesFlags = new BitArray32(_graph.VertexCount);
            uint queueSize = (uint)(_graph.VertexCount / 100 * 25);
            _missesQueue = new Queue<bool>();

            // remove all edges that have witness paths, meaning longer than the shortest path
            // between the two ending vertices.
            this.RemoveWitnessedEdges();

            // build queue.
            this.CalculateQueue(queueSize);

            this.SelectNext(queueSize);
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            var toDoCount = total;
            while (_queue.Count > 0 || 
                toDoCount > 0)
            {
                if (_queue.Count == 0)
                {
                    this.CalculateQueue(queueSize);
                }

                // contract...
                this.Contract();

                // ... and select next.
                this.SelectNext(queueSize);

                toDoCount = total - current;
                if (toDoCount > queueSize / 3 &&
                    _queue.Count < queueSize / 3)
                {
                    this.CalculateQueue(queueSize);
                }

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
        private void CalculateQueue(uint size)
        {
            _logger.Log(TraceEventType.Information, "Calculating queue...");

            long witnessed = 0;
            long total = 0;
            _queue.Clear();
            for (uint v = 0; v < _graph.VertexCount; v++)
            {
                if (!_contractedFlags[v])
                {
                    // update vertex info.
                    if (this.UpdateVertexInfo(v))
                    {
                        witnessed++;
                    }
                    total++;

                    // calculate priority.
                    var priority = _vertexInfo.Priority(_graph, _weightHandler, this.DifferenceFactor, this.ContractedFactor, this.DepthFactor);

                    // queue vertex.
                    _queue.Push(v, priority);

                    if (_queue.Count >= size)
                    {
                        break;
                    }
                }
            }
            _logger.Log(TraceEventType.Information, "Queue calculated: {0}/{1} have witnesses.",
                witnessed, total);
        }

        /// <summary>
        /// Remove all witnessed edges.
        /// </summary>
        private void RemoveWitnessedEdges()
        {
            
        }

        private int _k = 80; // The amount of queue 'misses' to recalculated.
        private int _misses; // Holds a counter of all misses.
        private Queue<bool> _missesQueue; // Holds the misses queue.

        /// <summary>
        /// Select the next vertex to contract.
        /// </summary>
        /// <returns></returns>
        protected virtual void SelectNext(uint queueSize)
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
                this.UpdateVertexInfo(first);
                var priority = _vertexInfo.Priority(_graph, _weightHandler, this.DifferenceFactor, this.ContractedFactor,
                    this.DepthFactor);
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
                    this.CalculateQueue(queueSize);

                    // clear misses.
                    _missesQueue.Clear();
                    _misses = 0;
                }
                else
                { // recalculation.
                    if (priority != queuedPriority)
                    { // re-enqueue.
                        _queue.Pop();
                        _queue.Push(first, priority);
                    }
                    else
                    { // selection succeeded.
                        _queue.Pop();
                        return;
                    }
                }
            }
            return; // all nodes have been contracted.
        }

        /// <summary>
        /// Contracts the given vertex.
        /// </summary>
        protected virtual void Contract()
        {
            var vertex = _vertexInfo.Vertex;

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < _vertexInfo.Count)
            {
                var edge = _vertexInfo[i];

                _graph.RemoveEdge(edge.Neighbour, vertex);
                i++;

                //_noWitnessesFlags[edge.Neighbour] = false; // this could lead to new witnesses.
                _vertexInfoCache.Remove(edge.Neighbour);
            }

            // add shortcuts.
            foreach (var s in _vertexInfo.Shortcuts)
            {
                var shortcut = s.Value;
                var edge = s.Key;
                var forwardMetric = _weightHandler.GetMetric(shortcut.Forward);
                var backwardMetric = _weightHandler.GetMetric(shortcut.Backward);

                if (forwardMetric > 0 && forwardMetric < float.MaxValue &&
                    backwardMetric > 0 && backwardMetric < float.MaxValue &&
                    System.Math.Abs(backwardMetric - forwardMetric) < HierarchyBuilder<float>.E)
                { // forward and backward and identical weights.
                    _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, null, shortcut.Forward);
                    _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, null, shortcut.Backward);
                    _vertexInfoCache.Remove(edge.Vertex1);
                    _vertexInfoCache.Remove(edge.Vertex2);
                    _noWitnessesFlags[edge.Vertex1] = false; // this could lead to new witnesses.
                    _noWitnessesFlags[edge.Vertex2] = false; // this could lead to new witnesses.
                }
                else
                {
                    if (forwardMetric > 0 && forwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, true, shortcut.Forward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, false, shortcut.Forward);
                        _vertexInfoCache.Remove(edge.Vertex1);
                        _vertexInfoCache.Remove(edge.Vertex2);
                        _noWitnessesFlags[edge.Vertex1] = false; // this could lead to new witnesses.
                        _noWitnessesFlags[edge.Vertex2] = false; // this could lead to new witnesses.
                    }
                    if (backwardMetric > 0 && backwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, false, shortcut.Backward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, true, shortcut.Backward);
                        _vertexInfoCache.Remove(edge.Vertex1);
                        _vertexInfoCache.Remove(edge.Vertex2);
                        _noWitnessesFlags[edge.Vertex1] = false; // this could lead to new witnesses.
                        _noWitnessesFlags[edge.Vertex2] = false; // this could lead to new witnesses.
                    }
                }
            }

            _contractedFlags[vertex] = true;
            this.NotifyContracted(vertex);
        }


        private DirectedMetaGraph.EdgeEnumerator _edgeEnumerator = null;

        /// <summary>
        /// Notifies this calculator that the given vertex was contracted.
        /// </summary>
        public void NotifyContracted(uint vertex)
        {
            // removes the contractions count.
            _contractionCount.Remove(vertex);

            // loop over all neighbours.
            if (_edgeEnumerator == null)
            {
                _edgeEnumerator = _graph.GetEdgeEnumerator();
            }
            _edgeEnumerator.MoveTo(vertex);

            int vertexDepth = 0;
            _depth.TryGetValue(vertex, out vertexDepth);
            _depth.Remove(vertex);
            vertexDepth++;

            // store the depth.
            _edgeEnumerator.Reset();
            while (_edgeEnumerator.MoveNext())
            {
                var neighbour = _edgeEnumerator.Neighbour;

                int depth = 0;
                _depth.TryGetValue(neighbour, out depth);
                if (vertexDepth >= depth)
                {
                    _depth[neighbour] = vertexDepth;
                }

                int count;
                if (!_contractionCount.TryGetValue(neighbour, out count))
                {
                    _contractionCount[neighbour] = 1;
                }
                else
                {
                    count++;
                    _contractionCount[neighbour] = count;
                }
            }

            _vertexInfoCache.Remove(vertex);
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
        public HierarchyBuilder(DirectedMetaGraph graph, DykstraWitnessCalculator witnessCalculator)
            : base(graph, witnessCalculator, new DefaultWeightHandler(null)) // the get factor function is never called.
        {

        }

        ///// <summary>
        ///// Contracts the given vertex.
        ///// </summary>
        //protected override void Contract()
        //{
        //    var vertex = _vertexInfo.Vertex;

        //    // remove 'downward' edge to vertex.
        //    var i = 0;
        //    while (i < _vertexInfo.Count)
        //    {
        //        var edge = _vertexInfo[i];

        //        _graph.RemoveEdge(edge.Neighbour, vertex);
        //        i++;

        //        _noWitnessesFlags[edge.Neighbour] = false; // this could lead to new witnesses.
        //        _vertexInfoCache.Remove(edge.Neighbour);
        //    }

        //    // add shortcuts.
        //    foreach (var s in _vertexInfo.Shortcuts)
        //    {
        //        var shortcut = s.Value;
        //        var edge = s.Key;
        //        var forwardMetric = shortcut.Forward;
        //        var backwardMetric = shortcut.Backward;

        //        if (forwardMetric > 0 && forwardMetric < float.MaxValue &&
        //            backwardMetric > 0 && backwardMetric < float.MaxValue &&
        //            System.Math.Abs(backwardMetric - forwardMetric) < HierarchyBuilder<float>.E)
        //        { // forward and backward and identical weights.
        //            _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
        //                    vertex, null, shortcut.Forward);
        //            _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
        //                    vertex, null, shortcut.Backward);
        //        }
        //        else
        //        {
        //            if (forwardMetric > 0 && forwardMetric < float.MaxValue)
        //            {
        //                _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
        //                    vertex, true, shortcut.Forward);
        //                _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
        //                    vertex, false, shortcut.Forward);
        //            }
        //            if (backwardMetric > 0 && backwardMetric < float.MaxValue)
        //            {
        //                _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
        //                    vertex, false, shortcut.Backward);
        //                _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
        //                    vertex, true, shortcut.Backward);
        //            }
        //        }
        //    }

        //    _contractedFlags[vertex] = true;
        //    this.NotifyContracted(vertex);
        //}
    }
}