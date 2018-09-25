using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Contracted.Dual.Witness;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using Itinero.Logging;

namespace Itinero.Algorithms.Contracted.Dual
{
    public class FastHierarchyBuilder<T> : AlgorithmBase
    where T : struct
    {
        protected readonly DirectedMetaGraph _graph;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        protected readonly WeightHandler<T> _weightHandler;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        protected VertexInfo<T> _vertexInfo;
        public const float E = 0.1f;

#if PCL
        NeighbourWitnessCalculator WitnessCalculators = new NeighbourWitnessCalculator();
#else
        ThreadLocal<NeighbourWitnessCalculator> WitnessCalculators = new ThreadLocal<NeighbourWitnessCalculator>(() =>
        {
            return new NeighbourWitnessCalculator();
        });
#endif

        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public FastHierarchyBuilder(DirectedMetaGraph graph,
            WeightHandler<T> weightHandler)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _weightHandler = weightHandler;

            _vertexInfo = new VertexInfo<T>();
            _depth = new Dictionary<long, int>();
            _contractionCount = new Dictionary<uint, int>();

            this.DifferenceFactor = 4;
            this.DepthFactor = 14;
            this.ContractedFactor = 1;
        }

        private BinaryHeap<uint> _queue; // the vertex-queue.
        private DirectedGraph _witnessGraph; // the graph with all the witnesses.
        protected BitArray32 _contractedFlags; // contains flags for contracted vertices.
        private int _k = 200; // The amount of queue 'misses' before recalculation of queue.
        private int _misses; // Holds a counter of all misses.
        private Queue<bool> _missesQueue; // Holds the misses queue.

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

        //private Itinero.Algorithms.Contracted.Dual.Witness.NeighbourWitnessCalculator _witnessCalculator = null;

        private void InitializeWitnessGraph()
        {
            _logger.Log(TraceEventType.Information, "Initializing witness graph...");

            _witnessGraph = new DirectedGraph(2, _graph.VertexCount);

#if NETSTANDARD2_0
            System.Threading.Tasks.Parallel.For(0, _graph.VertexCount, (v) =>
            {
                WitnessCalculators.Value.Run(_graph.Graph, _witnessGraph, (uint) v, null);
            });
#elif PCL
            for (uint v = 0; v < _graph.VertexCount; v++)
            {
                WitnessCalculators.Run(_graph.Graph, _witnessGraph, (uint)v, null);
            }
#else
            for (uint v = 0; v < _graph.VertexCount; v++)
            {
                WitnessCalculators.Value.Run(_graph.Graph, _witnessGraph, (uint) v, null);
            }
#endif
        }

        /// <summary>
        /// Updates the vertex info object with the given vertex.
        /// </summary>
        /// <returns>True if succeeded, false if a witness calculation is required.</returns>
        private bool UpdateVertexInfo(uint v)
        {
            var contracted = 0;
            var depth = 0;

            // update vertex info.
            _vertexInfo.Clear();
            _vertexInfo.Vertex = v;
            _contractionCount.TryGetValue(v, out contracted);
            _vertexInfo.ContractedNeighbours = contracted;
            _depth.TryGetValue(v, out depth);
            _vertexInfo.Depth = depth;

            // add neighbours.
            _vertexInfo.AddRelevantEdges(_graph.GetEdgeEnumerator());

            // check if any of neighbours are in witness queue.
            if (_witnessQueue.Count > 0)
            {
                for (var i = 0; i < _vertexInfo.Count; i++)
                {
                    var m = _vertexInfo[i];
                    if (_witnessQueue.Contains(m.Neighbour))
                    {
                        //this.DoWitnessQueue();
                        //break;
                        return false;
                    }
                }
            }

            // build shortcuts.
            _vertexInfo.BuildShortcuts(_weightHandler);

            // remove witnessed shortcuts.
            _vertexInfo.RemoveShortcuts(_witnessGraph, _weightHandler);
            return true;
        }

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _queue = new BinaryHeap<uint>((uint) _graph.VertexCount);
            _contractedFlags = new BitArray32(_graph.VertexCount); // is this strictly needed?
            _missesQueue = new Queue<bool>();

            this.InitializeWitnessGraph();

            this.CalculateQueue((uint) _graph.VertexCount);

            _logger.Log(TraceEventType.Information, "Started contraction...");
            this.SelectNext((uint) _graph.VertexCount);
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            var toDoCount = total;
            while (_queue.Count > 0)
            {
                // contract...
                this.Contract();

                // ... and select next.
                this.SelectNext((uint) _graph.VertexCount);

                // calculate and log progress.
                var progress = (float) (System.Math.Floor(((double) current / (double) total) * 10000) / 100.0);
                if (progress < 99)
                {
                    progress = (float) (System.Math.Floor(((double) current / (double) total) * 100) / 1.0);
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

                    var density = (double) totaEdges / (double) totalUncontracted;
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

            this.DoWitnessQueue();

            localQueue.Clear();

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
            // _logger.Log(TraceEventType.Information, "Queue calculated: {0}/{1} have witnesses.",
            //     witnessed, total);
        }

        private HashSet<uint> localQueue = new HashSet<uint>();
        private int MaxLocalQueueSize = 1;

        private void FlushLocalQueue()
        {
            this.DoWitnessQueue();
            foreach (var queued in localQueue)
            {
                this.UpdateVertexInfo(queued);
                var localPriority = _vertexInfo.Priority(_graph, _weightHandler, this.DifferenceFactor, this.ContractedFactor,
                    this.DepthFactor);
                _queue.Push(queued, localPriority);
            }
            localQueue.Clear();
        }

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

                //this.UpdateVertexInfo(first);
                //keep trying until local queue is full.
                while (true)
                {
                    if (this.UpdateVertexInfo(first))
                    { // update succeed.
                        break;
                    }

                    // move queues.
                    _queue.Pop();
                    localQueue.Add(first);

                    if (localQueue.Count > MaxLocalQueueSize ||
                        _queue.Count == 0)
                    { // if over max or queue empty do local queue.
                        this.FlushLocalQueue();
                        this.SelectNext(queueSize);
                        return;
                    }

                    // checkout the next one.
                    first = _queue.Peek();
                    while (_contractedFlags[first])
                    { // already contracted, priority was updated.
                        _queue.Pop();

                        if (_queue.Count == 0)
                        {
                            this.FlushLocalQueue();
                            this.SelectNext(queueSize);
                            return;
                        }
                        first = _queue.Peek();
                    }
                }

                // the lazy updating part!
                var queuedPriority = _queue.PeekWeight();
                // calculate priority
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

        protected HashSet<uint> _witnessQueue = new HashSet<uint>();

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

                // TOOD: what to do when stuff is only removed, is nothing ok?
                //_witnessQueue.Add(edge.Neighbour);
            }

            // add shortcuts.
            foreach (var s in _vertexInfo.Shortcuts)
            {
                var shortcut = s.Value;
                var edge = s.Key;

                if (edge.Vertex1 == edge.Vertex2)
                { // TODO: figure out how this is possible, it shouldn't!
                    continue;
                }

                var forwardMetric = _weightHandler.GetMetric(shortcut.Forward);
                var backwardMetric = _weightHandler.GetMetric(shortcut.Backward);

                if (forwardMetric > 0 && forwardMetric < float.MaxValue &&
                    backwardMetric > 0 && backwardMetric < float.MaxValue &&
                    System.Math.Abs(backwardMetric - forwardMetric) < FastHierarchyBuilder<float>.E)
                { // forward and backward and identical weights.
                    _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                        vertex, null, shortcut.Forward);
                    _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                        vertex, null, shortcut.Backward);
                    _witnessQueue.Add(edge.Vertex1);
                    _witnessQueue.Add(edge.Vertex2);
                }
                else
                {
                    if (forwardMetric > 0 && forwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, true, shortcut.Forward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, false, shortcut.Forward);
                        _witnessQueue.Add(edge.Vertex1);
                        _witnessQueue.Add(edge.Vertex2);
                    }
                    if (backwardMetric > 0 && backwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, false, shortcut.Backward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, true, shortcut.Backward);
                        _witnessQueue.Add(edge.Vertex1);
                        _witnessQueue.Add(edge.Vertex2);
                    }
                }
            }

            _contractedFlags[vertex] = true;
            this.NotifyContracted(vertex);
        }

        private void DoWitnessQueue()
        {
            if (_witnessQueue.Count > 0)
            {

#if NETSTANDARD2_0
                System.Threading.Tasks.Parallel.ForEach(_witnessQueue, (v) =>
                {
                    WitnessCalculators.Value.Run(_graph.Graph, _witnessGraph, (uint) v, _witnessQueue);
                });
#elif PCL
                for (uint v = 0; v < _graph.VertexCount; v++)
                {
                    WitnessCalculators.Run(_graph.Graph, _witnessGraph, (uint)v, null);
                }
#else
                foreach (var v in _witnessQueue)
                {
                   WitnessCalculators.Value.Run(_graph.Graph, _witnessGraph, (uint)v, _witnessQueue);
                }
#endif
                _witnessQueue.Clear();
                if (_witnessGraph.EdgeSpaceCount > _witnessGraph.EdgeCount * 8)
                {
                    _witnessGraph.Compress();
                    _logger.Log(TraceEventType.Information, "Witnessgraph size: {0}", _witnessGraph.EdgeCount);
                }
            }
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

                if (_witnessGraph.VertexCount > vertex &&
                    _witnessGraph.VertexCount > neighbour)
                {
                    _witnessGraph.RemoveEdge(neighbour, vertex);
                }
            }

            if (_witnessGraph.VertexCount > vertex)
            {
                _witnessGraph.RemoveEdges(vertex);
            }
        }
    }

    public sealed class FastHierarchyBuilder : FastHierarchyBuilder<float>
    {
        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public FastHierarchyBuilder(DirectedMetaGraph graph,
            WeightHandler<float> weightHandler) : base(graph, weightHandler)
        {

        }

        /// <summary>
        /// Contracts the given vertex.
        /// </summary>
        protected override void Contract()
        {
            var vertex = _vertexInfo.Vertex;

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < _vertexInfo.Count)
            {
                var edge = _vertexInfo[i];

                _graph.RemoveEdge(edge.Neighbour, vertex);
                i++;

                // TOOD: what to do when stuff is only removed, is nothing ok?
                //_witnessQueue.Add(edge.Neighbour);
            }

            // add shortcuts.
            foreach (var s in _vertexInfo.Shortcuts)
            {
                var shortcut = s.Value;
                var edge = s.Key;

                if (edge.Vertex1 == edge.Vertex2)
                { // TODO: figure out how this is possible, it shouldn't!
                    continue;
                }

                var forwardMetric = shortcut.Forward;
                var backwardMetric = shortcut.Backward;

                if (forwardMetric > 0 && forwardMetric < float.MaxValue &&
                    backwardMetric > 0 && backwardMetric < float.MaxValue &&
                    System.Math.Abs(backwardMetric - forwardMetric) < HierarchyBuilder<float>.E)
                { // forward and backward and identical weights.
                    _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                        vertex, null, shortcut.Forward);
                    _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                        vertex, null, shortcut.Backward);
                    _witnessQueue.Add(edge.Vertex1);
                    _witnessQueue.Add(edge.Vertex2);
                }
                else
                {
                    if (forwardMetric > 0 && forwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, true, shortcut.Forward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, false, shortcut.Forward);
                        _witnessQueue.Add(edge.Vertex1);
                        _witnessQueue.Add(edge.Vertex2);
                    }
                    if (backwardMetric > 0 && backwardMetric < float.MaxValue)
                    {
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex1, edge.Vertex2,
                            vertex, false, shortcut.Backward);
                        _weightHandler.AddOrUpdateEdge(_graph, edge.Vertex2, edge.Vertex1,
                            vertex, true, shortcut.Backward);
                        _witnessQueue.Add(edge.Vertex1);
                        _witnessQueue.Add(edge.Vertex2);
                    }
                }
            }

            _contractedFlags[vertex] = true;
            this.NotifyContracted(vertex);
        }
    }

    public static class DirectedGraphExtensions
    {
        public static void AddOrUpdateEdge(this DirectedGraph graph, uint vertex1, uint vertex2, float forward, float backward)
        {

            if (vertex1 > vertex2)
            {
                var t = vertex2;
                vertex2 = vertex1;
                vertex1 = t;

                var tw = backward;
                backward = forward;
                forward = tw;
            }

            var dataForward = ToData(forward);
            var dataBackward = ToData(backward);
            var data = new uint[] { dataForward, dataBackward };
            if (graph.UpdateEdgeIfBetter(vertex1, vertex2, (d) =>
                {
                    var existingForward = FromData(d[0]);
                    var existingBackward = FromData(d[1]);
                    var update = false;
                    if (existingForward > forward)
                    { // update, what we have here is better.
                        update = true;
                    }
                    else
                    { // take what's there, it's better.
                        data[0] = d[0];
                    }
                    if (existingBackward > backward)
                    { // update, what we have here is better.
                        update = true;
                    }
                    else
                    { // take what's there, it's better.
                        data[1] = d[1];
                    }
                    return update;
                }, data) == Constants.NO_EDGE)
            { // was not updated.
                graph.AddEdge(vertex1, vertex2, data);
            }
        }

        public static uint ToData(float weight)
        {
            if (weight == float.MaxValue)
            {
                return uint.MaxValue;
            }
            return (uint) (weight * 1000);
        }

        public static float FromData(uint data)
        {
            if (data == uint.MaxValue)
            {
                return float.MaxValue;
            }
            return data / 1000.0f;
        }
    }
}