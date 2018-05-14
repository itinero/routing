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
using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly IEnumerable<EdgePath<T>> _sources;
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;
        private readonly bool _backward;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedDynamicGraph graph, WeightHandler<T> weightHandler, IEnumerable<EdgePath<T>> sources,
            Func<uint, IEnumerable<uint[]>> getRestrictions, bool backward, T max)
        {
            weightHandler.CheckCanUse(graph);

            _graph = graph;
            _getRestrictions = getRestrictions;
            _sources = sources.Select(x => {
                x.StripEdges();
                return x;
            });
            _backward = backward;
            _weightHandler = weightHandler;
            _max = max;
        }

        private DirectedDynamicGraph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<uint, LinkedEdgePath<T>> _visits;
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
            _visits = new Dictionary<uint, LinkedEdgePath<T>>();
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
            while (_current != null)
            { // keep trying.
                LinkedEdgePath<T> edgePath = null;
                if (!_visits.TryGetValue(_current.Vertex, out edgePath))
                { // this vertex has not been visited before.
                    _visits.Add(_current.Vertex, new LinkedEdgePath<T>()
                    {
                        Path = _current,
                        MinWeight = _current.Weight
                    });
                    break;
                }
                else
                { // vertex has been visited before, check if edge has.
                    if (!edgePath.HasPath(_current))
                    { // current edge has not been used to get to this vertex.
                        _visits[_current.Vertex] = new LinkedEdgePath<T>()
                        {
                            Path = _current,
                            MinWeight = edgePath.MinWeight,
                            Next = edgePath
                        };
                        break;
                    }
                }
                _current = _heap.Pop();
            }

            if (_current == null)
            {
                return false;
            }

            if(this.WasFound != null)
            {
                this.WasFound(_current);
            }
            
            // get relevant restrictions.
            var restrictions = _getRestrictions(_current.Vertex);

            // get the edge enumerator.
            var currentSequence = _current.GetSequence2(_edgeEnumerator);
            currentSequence = currentSequence.Append(_current.Vertex);

            // get neighbours.
            _edgeEnumerator.MoveTo(_current.Vertex);

            // add the neighbours to the queue.
            while (_edgeEnumerator.MoveNext())
            {
                bool? neighbourDirection;
                var neighbourWeight = _weightHandler.GetEdgeWeight(_edgeEnumerator, out neighbourDirection);

                if (neighbourDirection == null || (neighbourDirection.Value != _backward))
                { // the edge is forward, and is to higher or was not contracted at all.
                    var neighbourNeighbour = _edgeEnumerator.Neighbour;
                    var neighbourSequence = Constants.EMPTY_SEQUENCE;
                    if (_edgeEnumerator.IsOriginal())
                    { // original edge.
                        if (currentSequence.Length > 1 && currentSequence[currentSequence.Length - 2] == neighbourNeighbour)
                        { // this is a u-turn.
                            continue;
                        }
                        if (restrictions != null)
                        {
                            neighbourSequence = currentSequence.Append(neighbourNeighbour);
                        }
                    }
                    else
                    { // not an original edge, use the sequence.
                        neighbourSequence = _edgeEnumerator.GetSequence1();
                        if (currentSequence.Length > 1 && currentSequence[currentSequence.Length - 2] == neighbourSequence[0])
                        { // this is a u-turn.
                            continue;
                        }
                        if (restrictions != null)
                        {
                            neighbourSequence = currentSequence.Append(neighbourSequence);
                        }
                    }

                    if (restrictions != null)
                    { // check restrictions.
                        if (!restrictions.IsSequenceAllowed(neighbourSequence))
                        {
                            continue;
                        }
                    }

                    // build route to neighbour and check if it has been visited already.
                    var routeToNeighbour = new EdgePath<T>(
                        neighbourNeighbour, _weightHandler.Add(_current.Weight, neighbourWeight), _edgeEnumerator.IdDirected(), _current);
                    LinkedEdgePath<T> edgePath = null;
                    if (!_visits.TryGetValue(_current.Vertex, out edgePath) ||
                        !edgePath.HasPath(routeToNeighbour))
                    { // this vertex has not been visited in this way before.
                        if (!_weightHandler.IsSmallerThanAny(routeToNeighbour.Weight, _max))
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
        /// Returns true if the given vertex was visited and sets the visits output parameter with the actual visits data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetVisits(uint vertex, out LinkedEdgePath<T> visits)
        {
            return _visits.TryGetValue(vertex, out visits);
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visits output parameter with the best visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetVisit(uint vertex, out EdgePath<T> visits)
        {
            LinkedEdgePath<T> linkedVisits;
            if (!_visits.TryGetValue(vertex, out linkedVisits))
            {
                visits = null;
                return false;
            }
            visits = linkedVisits.Best(_weightHandler);
            return true;
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
        public DirectedDynamicGraph Graph
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
        public Dykstra(DirectedDynamicGraph graph, IEnumerable<EdgePath<float>> sources,
            Func<uint, IEnumerable<uint[]>> getRestrictions, bool backward, float max = float.MaxValue)
            : base(graph, new DefaultWeightHandler(null), sources, getRestrictions, backward, max)
        {

        }
    }
}