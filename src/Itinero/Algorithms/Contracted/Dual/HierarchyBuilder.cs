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
using System.Threading;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly DykstraWitnessCalculator<T> _witnessCalculator;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        private readonly WeightHandler<T> _weightHandler;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        private readonly VertexInfo<T> _vertexInfo;
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
        private BitArray32 _contractedFlags; // contains flags for contracted vertices.

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
        private void UpdateVertexInfo(uint v)
        {
            // update vertex info.
            _vertexInfo.Clear();
            _vertexInfo.Vertex = v;
            var contracted = 0;
            _contractionCount.TryGetValue(v, out contracted);
            _vertexInfo.ContractedNeighbours = contracted;
            var depth = 0;
            _depth.TryGetValue(v, out depth);
            _vertexInfo.Depth = depth;

            // calculate shortcuts and witnesses.
            _vertexInfo.AddRelevantEdges(_graph.GetEdgeEnumerator());
            _vertexInfo.BuildShortcuts(_weightHandler);
            _vertexInfo.Shortcuts.RemoveWitnessed(v, _witnessCalculator);
        }

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

            this.SelectNext();
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            while (_queue.Count > 0)
            {
                // contract...
                this.Contract();

                // ... and select next.
                this.SelectNext();

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
        private void CalculateQueue()
        {
            _logger.Log(TraceEventType.Information, "Calculating queue...");

            _queue.Clear();
            for (uint v = 0; v < _graph.VertexCount; v++)
            {
                if (!_contractedFlags[v])
                {
                    // update vertex info.
                    this.UpdateVertexInfo(v);

                    // calculate priority.
                    var priority = _vertexInfo.Priority(_graph, _weightHandler, this.DifferenceFactor, this.ContractedFactor, this.DepthFactor);

                    // queue vertex.
                    _queue.Push(v, priority);
                }
            }
        }

        /// <summary>
        /// Remove all witnessed edges.
        /// </summary>
        private void RemoveWitnessedEdges()
        {
            //_logger.Log(TraceEventType.Information, "Removing witnessed edges...");

            //var enumerator = _graph.GetEdgeEnumerator();
            //for (uint v = 0; v < _graph.VertexCount; v++)
            //{
            //    // update vertex info.
            //    _vertexInfo.Clear();
            //    _vertexInfo.Vertex = v;

            //    // add edges.
            //    _vertexInfo.AddRelevantEdges(enumerator);

            //    // add 'shortcuts' from this vertex to it's neighbours.
            //    for (var e = 0; e < _vertexInfo.Count; e++)
            //    {
            //        var edge = _vertexInfo[e];
            //        var edgeWeight = _weightHandler.GetEdgeWeight(edge);
            //        var originalEdge = new OriginalEdge(v, edge.Neighbour);

            //        var shortcut = new Shortcut<T>()
            //        {
            //            Forward = edgeWeight.Weight,
            //            Backward = edgeWeight.Weight
            //        };
            //        if (!edgeWeight.Direction.F)
            //        {
            //            shortcut.Forward = _weightHandler.Zero;
            //        }
            //        if (!edgeWeight.Direction.B)
            //        {
            //            shortcut.Backward = _weightHandler.Zero;
            //        }

            //        _vertexInfo.Shortcuts.AddOrUpdate(originalEdge, shortcut, _weightHandler);
            //    }

            //    // remove unneeded shortcuts, or in other words keep shortest paths to all neighbours in this case.
            //    _vertexInfo.Shortcuts.RemoveWitnessed(v, _witnessCalculator);

            //    // remove witnessed neighbours.
            //    for (var e = 0; e < _vertexInfo.Count; e++)
            //    {
            //        var edge = _vertexInfo[e];
            //        var edgeWeight = _weightHandler.GetEdgeWeight(edge);
            //        var originalEdge = new OriginalEdge(v, edge.Neighbour);

            //        var shortcut = new Shortcut<float>()
            //        {
            //            Forward = _weightHandler.GetMetric(edgeWeight.Weight),
            //            Backward = _weightHandler.GetMetric(edgeWeight.Weight)
            //        };
            //        if (!edgeWeight.Direction.F)
            //        {
            //            shortcut.Forward = 0;
            //        }
            //        if (!edgeWeight.Direction.B)
            //        {
            //            shortcut.Backward = 0;
            //        }

            //        var witnessedShortcut = _vertexInfo.Shortcuts[originalEdge];
            //        var witnessedShortcutForward = _weightHandler.GetMetric(witnessedShortcut.Forward);
            //        var witnessedShortcutBackward = _weightHandler.GetMetric(witnessedShortcut.Backward);
            //        if ((shortcut.Forward == 0 || witnessedShortcutForward < shortcut.Forward) &&
            //            (shortcut.Backward == 0 || witnessedShortcutBackward < shortcut.Backward))
            //        { // edge is useless.
            //            _graph.RemoveEdge(v, originalEdge.Vertex2);
            //        }
            //        else
            //        {
            //            if (witnessedShortcutForward < shortcut.Forward)
            //            { // edge is useless in the forward direction
            //                _graph.RemoveEdge(v, originalEdge.Vertex2);
            //                _weightHandler.AddEdge(_graph, v, originalEdge.Vertex2, Constants.NO_VERTEX,
            //                    false, edgeWeight.Weight);
            //            }
            //            if (witnessedShortcutBackward < shortcut.Backward)
            //            { // edge is useless in backward direction.
            //                _graph.RemoveEdge(v, originalEdge.Vertex2);
            //                _weightHandler.AddEdge(_graph, v, originalEdge.Vertex2, Constants.NO_VERTEX,
            //                    true, edgeWeight.Weight);
            //            }
            //        }
            //    }
            //}
        }

        private int _k = 80; // The amount of queue 'misses' to recalculated.
        private int _misses; // Holds a counter of all misses.
        private Queue<bool> _missesQueue; // Holds the misses queue.

        /// <summary>
        /// Select the next vertex to contract.
        /// </summary>
        /// <returns></returns>
        private void SelectNext()
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
        private void Contract()
        {
            var vertex = _vertexInfo.Vertex;
            var enumerator = _graph.GetEdgeEnumerator();

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < _vertexInfo.Count)
            {
                var edge = _vertexInfo[i];

                _graph.RemoveEdge(edge.Neighbour, vertex);
                i++;
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
                }
                else
                {
                    if (forwardMetric > 0 && forwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, true, shortcut.Forward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, false, shortcut.Forward);
                    }
                    if (backwardMetric > 0 && backwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, false, shortcut.Backward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, true, shortcut.Backward);
                    }
                }
            }

            _contractedFlags[vertex] = true;
            this.NotifyContracted(vertex);
        }

        /// <summary>
        /// Notifies this calculator that the given vertex was contracted.
        /// </summary>
        public void NotifyContracted(uint vertex)
        {
            // removes the contractions count.
            _contractionCount.Remove(vertex);

            // loop over all neighbours.
            var edgeEnumerator = _graph.GetEdgeEnumerator(vertex);

            int vertexDepth = 0;
            _depth.TryGetValue(vertex, out vertexDepth);
            _depth.Remove(vertex);
            vertexDepth++;

            // store the depth.
            edgeEnumerator.Reset();
            while (edgeEnumerator.MoveNext())
            {
                var neighbour = edgeEnumerator.Neighbour;

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
    }
}