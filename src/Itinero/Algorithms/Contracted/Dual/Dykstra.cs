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
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Threading;
using Itinero.Algorithms.Contracted.Dual.Cache;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly DykstraSource<T> _source;
        private readonly bool _backward;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;
        private readonly SearchSpaceCache<T> _cache = null;

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, uint source, bool backward, T max, 
            SearchSpaceCache<T> cache = null)
            : this(graph, weightHandler, new DykstraSource<T>(source), backward, max)
        {

        }

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, DykstraSource<T> source, bool backward, T max,
            SearchSpaceCache<T> cache = null)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _source = source;
            _backward = backward;
            _weightHandler = weightHandler;
            _max = max;
            _cache = cache;
        }

        private BinaryHeap<uint> _pointerHeap;
        private PathTree _pathTree;
        private Dictionary<uint, Tuple<uint, T>> _visited;
        private DirectedMetaGraph.EdgeEnumerator _edgeEnumerator;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // check cache.
            if (_cache != null &&
                _cache.TryGet(_source, false, out var space))
            { // use cached search space.
                foreach (var visit in space.Visits)
                {
                    this.WasFound?.Invoke(uint.MaxValue,visit.Key, visit.Value.Item2);
                }
            }
            else
            { // no cache.
                // initialize stuff.
                this.Initialize();

                // start the search.
                while (this.Step()) { }

                if (_cache == null) return; 
                
                // cache but no entry, add.
                space = this.GetSearchSpace();
                _cache.Add(_source, _backward, space);
            }
        }
        
        /// <summary>
        /// Initializes and resets.
        /// </summary>
        public void Initialize()
        {
            // algorithm always succeeds, it may be dealing with an empty network and there are no targets.
            this.HasSucceeded = true;

            // initialize dykstra data structures.
            _pointerHeap = new BinaryHeap<uint>();
            _pathTree = new PathTree();
            _visited = new Dictionary<uint, Tuple<uint, T>>();

            // queue source.
            if (_source.Vertex1 != Constants.NO_VERTEX)
            {
                _pointerHeap.Push(_weightHandler.AddPathTree(_pathTree, _source.Vertex1, _source.Weight1, uint.MaxValue), 0);
            }
            if (_source.Vertex2 != Constants.NO_VERTEX)
            {
                _pointerHeap.Push(_weightHandler.AddPathTree(_pathTree, _source.Vertex2, _source.Weight2, uint.MaxValue), 0);
            }

            // gets the edge enumerator.
            _edgeEnumerator = _graph.GetEdgeEnumerator();
        }

        /// <summary>
        /// Executes one step in the search.
        /// </summary>
        public bool Step()
        {
            if(_pointerHeap.Count == 0)
            {
                return false;
            }

            var cPointer = _pointerHeap.Pop();
            _weightHandler.GetPathTree(_pathTree, cPointer, out var cVertex, out var cWeight, out _);

            if (_visited.ContainsKey(cVertex))
            {
                return true;
            }
            _visited.Add(cVertex, new Tuple<uint, T>(cPointer, cWeight));

            if (this.WasFound != null)
            {
                if (this.WasFound(cPointer, cVertex, cWeight))
                { // when true is returned, the listener signals it knows what it wants to know.
                    return false;
                }
            }

            _edgeEnumerator.MoveTo(cVertex);
            while (_edgeEnumerator.MoveNext())
            {
                var nWeight = _weightHandler.GetEdgeWeight(_edgeEnumerator);
                
                if ((!_backward && nWeight.Direction.F) ||
                    (_backward && nWeight.Direction.B))
                { // the edge is forward, and is to higher or was not contracted at all.
                    var nVertex = _edgeEnumerator.Neighbour;
                    var totalWeight = _weightHandler.Add(nWeight.Weight, cWeight);
                    if (!_weightHandler.IsSmallerThan(totalWeight, _max))
                    {
                        continue;
                    }
                    var nPointer =_weightHandler.AddPathTree(_pathTree, nVertex, totalWeight, cPointer);
                    _pointerHeap.Push(nPointer, _weightHandler.GetMetric(totalWeight));
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the path for the vertex at the given pointer.
        /// </summary>
        public EdgePath<T> GetPath(uint pointer)
        {
            return _weightHandler.GetPath(_pathTree, pointer);
        }

        /// <summary>
        /// Gets the weight for the vertex at the given pointer.
        /// </summary>
        public T GetWeight(uint pointer)
        {
            _weightHandler.GetPathTree(_pathTree, pointer, out _, out var weight, out _);
            return weight;
        }
        
        /// <summary>
        /// Gets the search space.
        /// </summary>
        /// <returns></returns>
        public SearchSpace<T> GetSearchSpace()
        {
            return new SearchSpace<T>()
            {
                Tree = _pathTree,
                Visits = _visited,
                VisitSet =  new HashSet<uint>(_visited.Keys)
            };
        }

        /// <summary>
        /// A signature of the was found callback function.
        /// </summary>
        /// <param name="pointer">The pointer in the path three, can be used to retrieve the complete path.</param>
        /// <param name="vertex">The vertex found.</param>
        /// <param name="weight">The weight at the vertex.</param>
        /// <returns> when true is returned, the listener signals it knows what it wants to know and the search stops.</returns>
        /// <remarks>Yes, we can use Func but this is less confusing and contains meaning about the parameters.</remarks>
        public delegate bool WasFoundDelegate(uint pointer, uint vertex, T weight);

        /// <summary>
        /// Gets or sets the was found function to be called when a new vertex is found.
        /// </summary>
        public WasFoundDelegate WasFound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the backward flag.
        /// </summary>
        public bool Backward => _backward;

        /// <summary>
        /// Gets the graph.
        /// </summary>
        public DirectedMetaGraph Graph => _graph;
    }
}