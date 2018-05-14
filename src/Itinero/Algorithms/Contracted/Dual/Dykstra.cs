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

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, uint source, bool backward, T max)
            : this(graph, weightHandler, new DykstraSource<T>(source), backward, max)
        {

        }

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, DykstraSource<T> source, bool backward, T max)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _source = source;
            _backward = backward;
            _weightHandler = weightHandler;
            _max = max;
        }

        private BinaryHeap<uint> _pointerHeap;
        private PathTree _pathTree;
        private HashSet<uint> _visited;
        private DirectedMetaGraph.EdgeEnumerator _edgeEnumerator;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // initialize stuff.
            this.Initialize();

            // start the search.
            while (this.Step()) { }
        }
        
        /// <summary>
        /// Initializes and resets.
        /// </summary>
        public void Initialize()
        {
            // algorithm always succeeds, it may be dealing with an empty network and there are no targets.
            this.HasSucceeded = true;

            // intialize dykstra data structures.
            _pointerHeap = new BinaryHeap<uint>();
            _pathTree = new PathTree();
            _visited = new HashSet<uint>();

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
            uint cVertex, cPrevious;
            T cWeight;
            _weightHandler.GetPathTree(_pathTree, cPointer, out cVertex, out cWeight, out cPrevious);

            if (_visited.Contains(cVertex))
            {
                return true;
            }
            _visited.Add(cVertex);

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
            T weight;
            uint vertex, previous;
            _weightHandler.GetPathTree(_pathTree, pointer, out vertex, out weight, out previous);
            return weight;
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
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public WasFoundDelegate WasFound
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the backward flag.
        /// </summary>
        public bool Backward
        {
            get
            {
                return _backward;
            }
        }

        /// <summary>
        /// Gets the graph.
        /// </summary>
        public DirectedMetaGraph Graph
        {
            get
            {
                return _graph;
            }
        }
    }

    // TODO: build and non-generic non-weighthandler version, also use graph not meta graph in the simple weighed version.
}