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

using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly IEnumerable<EdgePath<T>> _sources;
        private readonly bool _backward;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, WeightHandler<T> weightHandler, IEnumerable<EdgePath<T>> sources, bool backward, T max)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _sources = sources;
            _backward = backward;
            _weightHandler = weightHandler;
            _max = max;
        }

        private DirectedMetaGraph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<uint, EdgePath<T>> _visits;
        private EdgePath<T> _current;
        private BinaryHeap<EdgePath<T>> _heap;

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
            _visits = new Dictionary<uint, EdgePath<T>>();
            _heap = new BinaryHeap<EdgePath<T>>();

            // queue all sources.
            foreach (var source in _sources)
            {
                _heap.Push(source, _weightHandler.GetMetric(source.Weight));
            }

            // gets the edge enumerator.
            _edgeEnumerator = _graph.GetEdgeEnumerator();
        }

        /// <summary>
        /// Executes one step in the search.
        /// </summary>
        public bool Step()
        {
            if(_heap.Count == 0)
            {
                return false;
            }
            _current = _heap.Pop();
            if (_current != null)
            {
                while(_visits.ContainsKey(_current.Vertex))
                {
                    _current = _heap.Pop();
                    if(_current == null)
                    {
                        return false;
                    }
                }
            }
            else
            {
                return false;
            }
            _visits.Add(_current.Vertex, _current);

            if(this.WasFound != null)
            {
                this.WasFound(_current);
            }

            _edgeEnumerator.MoveTo(_current.Vertex);
            while (_edgeEnumerator.MoveNext())
            {
                bool? neighbourDirection;
                var neighbourWeight = _weightHandler.GetEdgeWeight(_edgeEnumerator.Current, out neighbourDirection);
                
                if (neighbourDirection == null || neighbourDirection.Value == !_backward)
                { // the edge is forward, and is to higher or was not contracted at all.
                    var neighbourNeighbour = _edgeEnumerator.Neighbour;
                    if (!_visits.ContainsKey(neighbourNeighbour))
                    { // if not yet settled.
                        var routeToNeighbour = new EdgePath<T>(
                            neighbourNeighbour, _weightHandler.Add(_current.Weight, neighbourWeight), _current);
                        if (_weightHandler.IsLargerThan(routeToNeighbour.Weight, _max))
                        {
                            continue;
                        }
                        _heap.Push(routeToNeighbour, _weightHandler.GetMetric(routeToNeighbour.Weight));
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetVisit(uint vertex, out EdgePath<T> visit)
        {
            return _visits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public Func<EdgePath<T>, bool> WasFound
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

        /// <summary>
        /// Gets the current.
        /// </summary>
        public EdgePath<T> Current
        {
            get
            {
                return _current;
            }
        }
    }

    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public sealed class Dykstra : Dykstra<float>
    {
        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, IEnumerable<EdgePath<float>> sources, bool backward, float max = float.MaxValue)
            : base(graph, new DefaultWeightHandler(null), sources, backward, max)
        {

        }
    }
}