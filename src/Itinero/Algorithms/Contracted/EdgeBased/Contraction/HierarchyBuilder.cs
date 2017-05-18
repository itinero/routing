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
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs.Directed;
using Itinero.Logging;
using System.Collections.Generic;
using Itinero.Algorithms.Restrictions;
using System.Linq;
using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly DykstraWitnessCalculator<T> _witnessCalculator;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        private readonly RestrictionCollection _restrictions;
        public const float E = 0.1f;
        private readonly WeightHandler<T> _weightHandler;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        public static int MaxSettles = 65536;
        public static int MaxHops = 8;
        
        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedDynamicGraph graph,
            WeightHandler<T> weightHandler, RestrictionCollection restrictions)
            : this(graph, weightHandler, restrictions, new DykstraWitnessCalculator<T>(graph, restrictions, weightHandler, MaxHops, MaxSettles))
        {

        }
        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedDynamicGraph graph,
            WeightHandler<T> weightHandler, RestrictionCollection restrictions, DykstraWitnessCalculator<T> witnessCalculator)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _witnessCalculator = witnessCalculator;
            _restrictions = restrictions;
            _weightHandler = weightHandler;

            this.DifferenceFactor = 1;
            this.DepthFactor = 2;
            this.ContractedFactor = 1;
            
            _contractionCount = new Dictionary<uint, int>();
            _depth = new Dictionary<long, int>();
            _vertexInfo = new VertexInfo<T>();
        }

        private VertexInfo<T> _vertexInfo; // structure to efficiëntly store (potential) shortcuts and vertex info.
        private BinaryHeap<uint> _queue; // the vertex-queue.
        private BitArray32 _contractedFlags; // contains flags for contracted vertices.
        private BitArray32 _restrictionFlags; // contains flags for restricted vertices.

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            _queue = new BinaryHeap<uint>((uint)_graph.VertexCount);
            _contractedFlags = new BitArray32(_graph.VertexCount);
            _restrictionFlags = new BitArray32(_graph.VertexCount);
            _missesQueue = new Queue<bool>();

            // build restrictions flags.
            for(uint i= 0; i < _graph.VertexCount; i++)
            {
                if (_restrictions.Update(i))
                {
                    _restrictionFlags[i] = true;
                }
            }

            // build queue.
            this.CalculateQueue();

            var next = this.SelectNext();
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            while (next)
            {
                // contract...
                this.Contract();

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
                    // update vertex info.
                    this.UpdateVertexInfo(v);

                    // calculate priority.
                    var priority = _vertexInfo.Priority(_weightHandler, this.DifferenceFactor, this.ContractedFactor, this.DepthFactor);

                    // queue vertex.
                    _queue.Push(v, priority);
                }
            }
        }

        /// <summary>
        /// Requeue the given vertex.
        /// </summary>
        /// <param name="v"></param>
        private void Requeue(uint v)
        {
            // update vertex info.
            this.UpdateVertexInfo(v);

            // calculate priority.
            var priority = _vertexInfo.Priority(_weightHandler, this.DifferenceFactor, this.ContractedFactor, this.DepthFactor);

            // queue vertex.
            _queue.Push(v, priority);
        }

        /// <summary>
        /// Updates the vertex info object with the given vertex.
        /// </summary>
        private void UpdateVertexInfo(uint v)
        {
            // update vertex info.
            _vertexInfo.Clear();
            _vertexInfo.Vertex = v;
            _vertexInfo.HasRestrictions = _restrictionFlags[v];
            var contracted = 0;
            _contractionCount.TryGetValue(v, out contracted);
            _vertexInfo.ContractedNeighbours = contracted;
            var depth = 0;
            _depth.TryGetValue(v, out depth);
            _vertexInfo.Depth = depth;

            // calculate shortcuts and witnesses.
            _vertexInfo.AddRelevantEdges(_graph.GetEdgeEnumerator());
            _vertexInfo.BuildShortcuts(_weightHandler, _witnessCalculator);
            _vertexInfo.Shortcuts.RemoveWitnessed(v, _witnessCalculator);
        }

        private int _k = 20; // The amount of queue 'misses' to recalculated.
        private int _misses; // Holds a counter of all misses.
        private Queue<bool> _missesQueue; // Holds the misses queue.

        /// <summary>
        /// Select the next vertex to contract.
        /// </summary>
        /// <returns></returns>
        private bool SelectNext()
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
                // update vertex info.
                this.UpdateVertexInfo(first);
                // calculate priority.
                var priority = _vertexInfo.Priority(_weightHandler, this.DifferenceFactor, this.ContractedFactor, this.DepthFactor);
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
                    {
                        return true;
                    }
                }
            }
            return false; // all nodes have been contracted.
        }
        
        /// <summary>
        /// Contracts the given vertex.
        /// </summary>
        protected virtual void Contract()
        {
            var vertex = _vertexInfo.Vertex;
            var enumerator = _graph.GetEdgeEnumerator();

#if DEBUG
            Itinero.Logging.Logger.Log("HierarchyBuilder.Contract", TraceEventType.Information, "Contracting {0}...", vertex);
#endif

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < _vertexInfo.Count)
            {
                var edge = _vertexInfo[i];

                var s1 = edge.GetSequence1();
                var s2 = edge.GetSequence2();
                bool? direction;
                _weightHandler.GetEdgeWeight(edge, out direction);
                if (direction != null)
                {
                    direction = !direction.Value;
                }
                enumerator.RemoveEdge(edge.Neighbour, vertex, s2, s1, _weightHandler, direction);
                
                i++;
            }

            // add shortcuts.
            var accessor = _vertexInfo.Shortcuts.GetAccessor();
            while(accessor.MoveNextSource())
            {
                var source = accessor.Source;
                while(accessor.MoveNextTarget())
                {
                    var shortcut = accessor.Target;

                    var forwardMetric = _weightHandler.GetMetric(shortcut.Forward);
                    var backwardMetric = _weightHandler.GetMetric(shortcut.Backward);
                    
                    // TODO: come up with an allocation-free version of addorupdateedge.
                    if (forwardMetric > 0 && backwardMetric > 0 &&
                        System.Math.Abs(backwardMetric - forwardMetric) < HierarchyBuilder<float>.E)
                    { // forward and backward and identical weights.
                        _weightHandler.AddOrUpdateEdge(_graph, source.Vertex1, shortcut.Edge.Vertex2, vertex, null, shortcut.Forward,
                            source.Vertex2, shortcut.Edge.Vertex1);
                        _weightHandler.AddOrUpdateEdge(_graph, shortcut.Edge.Vertex2, source.Vertex1, vertex, null, shortcut.Forward,
                            shortcut.Edge.Vertex1, source.Vertex2);
                    }
                    else
                    {
                        if (forwardMetric > 0)
                        {
                            _weightHandler.AddOrUpdateEdge(_graph, source.Vertex1, shortcut.Edge.Vertex2, vertex, true, shortcut.Forward,
                                source.Vertex2, shortcut.Edge.Vertex1);
                            _weightHandler.AddOrUpdateEdge(_graph, shortcut.Edge.Vertex2, source.Vertex1, vertex, false, shortcut.Forward,
                                shortcut.Edge.Vertex1, source.Vertex2);
                        }
                        if (backwardMetric > 0)
                        {
                            _weightHandler.AddOrUpdateEdge(_graph, source.Vertex1, shortcut.Edge.Vertex2, vertex, false, shortcut.Backward,
                                source.Vertex2, shortcut.Edge.Vertex1);
                            _weightHandler.AddOrUpdateEdge(_graph, shortcut.Edge.Vertex2, source.Vertex1, vertex, true, shortcut.Backward,
                                shortcut.Edge.Vertex1, source.Vertex2);
                        }
                    }
                }
            }

            _contractedFlags[vertex] = true;
            this.NotifyContracted(vertex);
        }
        
        /// <summary>
        /// Gets the contraction count dictionary.
        /// </summary>
        public Dictionary<uint, int> ContractionCount
        {
            get
            {
                return _contractionCount;
            }
        }

        /// <summary>
        /// Gets the depth per vertex.
        /// </summary>
        public Dictionary<long, int> Depth
        {
            get
            {
                return _depth;
            }
        }

        /// <summary>
        /// Gets the contracted flags.
        /// </summary>
        public BitArray32 ContractedFlags
        {
            get
            {
                return _contractedFlags;
            }
        }

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
        /// Notifies this calculator that the given vertex was contracted.
        /// </summary>
        public void NotifyContracted(uint vertex)
        {
            // removes the contractions count.
            _contractionCount.Remove(vertex);

            // loop over all neighbours.
            var edgeEnumerator = _graph.GetEdgeEnumerator(vertex);
            edgeEnumerator.Reset();
            while (edgeEnumerator.MoveNext())
            {
                var neighbour = edgeEnumerator.Neighbour;
                int count;
                if (!_contractionCount.TryGetValue(neighbour, out count))
                {
                    _contractionCount[neighbour] = 1;
                }
                else
                {
                    _contractionCount[neighbour] = count++;
                }

                //this.Requeue(neighbour);
            }

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
        public HierarchyBuilder(DirectedDynamicGraph graph, RestrictionCollection restrictions)
            : base(graph, new DefaultWeightHandler(null), restrictions, new DykstraWitnessCalculator(graph, restrictions, MaxHops, MaxSettles))
        {

        }
    }
}