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
using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;
using Itinero.Graphs;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An implementation of a directed edge-based dykstra routing algorithm.
    /// </summary>
    public class DirectedDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly Graph _graph;
        private readonly DirectedDykstraSource<T> _source;
        private readonly WeightHandler<T> _weightHandler;
        private readonly RestrictionCollection _restrictions;
        private readonly T _sourceMax;
        private readonly bool _backward;
        

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public DirectedDykstra(Graph graph, WeightHandler<T> weightHandler, RestrictionCollection restrictions,
            DirectedEdgeId source, T sourceMax, bool backward)
            : this(graph, weightHandler, restrictions, new DirectedDykstraSource<T>(source, weightHandler.Zero), sourceMax, backward)
        {

        }

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public DirectedDykstra(Graph graph, WeightHandler<T> weightHandler, RestrictionCollection restrictions,
            DirectedDykstraSource<T> source, T sourceMax, bool backward)
        {
            _graph = graph;
            _source = source;
            _weightHandler = weightHandler;
            _sourceMax = sourceMax;
            _backward = backward;
            _restrictions = restrictions;
        }

        private Graph.EdgeEnumerator _edgeEnumerator;
        private PathTree _pathTree;
        private HashSet<uint> _visits;
        private BinaryHeap<uint> _pointerHeap;

        /// <summary>
        /// Executes the algorithm.
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
            _visits = new HashSet<uint>();
            _pointerHeap = new BinaryHeap<uint>();
            _pathTree = new PathTree();

            // initialize the edge enumerator.
            _edgeEnumerator = _graph.GetEdgeEnumerator();

            // queue source.
            if (!_source.Edge1.IsNoEdge)
            {
                _pointerHeap.Push(_weightHandler.AddPathTree(_pathTree, _source.Edge1.Raw, _source.Weight1, uint.MaxValue), 0);
            }
            if (!_source.Edge2.IsNoEdge)
            {
                _pointerHeap.Push(_weightHandler.AddPathTree(_pathTree, _source.Edge2.Raw, _source.Weight2, uint.MaxValue), 0);
            }
        }

        /// <summary>
        /// Executes one step in the search.
        /// </summary>
        public bool Step()
        {
            // while the visit list is not empty.
            var cPointer = uint.MaxValue;
            while (cPointer == uint.MaxValue)
            { // choose the next edge.
                if (_pointerHeap.Count == 0)
                {
                    return false;
                }
                cPointer = _pointerHeap.Pop();
            }

            // get details.
            uint cEdge, cPreviousPointer;
            T cWeight;
            _weightHandler.GetPathTree(_pathTree, cPointer, out cEdge, out cWeight, out cPreviousPointer);
            if (_visits.Contains(cEdge))
            {
                return true;
            }
            _visits.Add(cEdge);
            var cDirectedEdge = new DirectedEdgeId()
            {
                Raw = cEdge
            };

            // signal was found.
            if (this.WasFound != null)
            {
                this.WasFound(cPointer, cDirectedEdge, cWeight);
            }

            // move to the current edge's target vertex.
            _edgeEnumerator.MoveToEdge(cDirectedEdge.EdgeId);
            var cOriginalEdge = new OriginalEdge(_edgeEnumerator.From, _edgeEnumerator.To);
            if (!cDirectedEdge.Forward)
            {
                cOriginalEdge = cOriginalEdge.Reverse();
            }
            var cEdgeWeightAndDirection = _weightHandler.GetEdgeWeight(_edgeEnumerator);

            // calculate total weight.
            var totalWeight = _weightHandler.Add(cEdgeWeightAndDirection.Weight, cWeight);

            if (!_weightHandler.IsSmallerThan(totalWeight, _sourceMax))
            { // weight is too big.
                this.MaxReached = true;
                return true;
            }

            // loop over all neighbours.
            _edgeEnumerator.MoveTo(cOriginalEdge.Vertex2);
            var turn = new Turn(cOriginalEdge, Constants.NO_VERTEX);
            _restrictions.Update(turn.Vertex2);
            while (_edgeEnumerator.MoveNext())
            {
                turn.Vertex3 = _edgeEnumerator.To;
                if (turn.IsUTurn ||
                    turn.IsRestrictedBy(_restrictions))
                { // turn is restricted.
                    continue;
                }

                var nDirectedEdgeId = _edgeEnumerator.DirectedEdgeId();
                if (_visits.Contains(nDirectedEdgeId.Raw))
                { // has already been choosen.
                    continue;
                }

                // get the speed from cache or calculate.
                var nWeightAndDirection = _weightHandler.GetEdgeWeight(_edgeEnumerator);
                var nWeightMetric = _weightHandler.GetMetric(nWeightAndDirection.Weight);
                if (nWeightMetric <= 0)
                { // edge is not valid for profile.
                    continue;
                }
                if (!_backward && !nWeightAndDirection.Direction.F)
                { // cannot do forward search on edge.
                    continue;
                }
                if (_backward && !nWeightAndDirection.Direction.B)
                { // cannot do backward on edge.
                    continue;
                }

                // update the visit list.
                _pointerHeap.Push(_weightHandler.AddPathTree(_pathTree, nDirectedEdgeId.Raw, totalWeight, cPointer), _weightHandler.GetMetric(totalWeight));
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
        /// Gets the max reached flag.
        /// </summary>
        /// <remarks>True if the source-max value was reached.</remarks>
        public bool MaxReached { get; private set; }

        /// <summary>
        /// A signature of the was found callback function.
        /// </summary>
        /// <param name="pointer">The pointer in the path three, can be used to retrieve the complete path.</param>
        /// <param name="edge">The edge found.</param>
        /// <param name="weight">The weight at the vertex.</param>
        /// <returns>>When true is returned, the listener signals it knows what it wants to know and the search stops.</returns>
        /// <remarks>Yes, we can use Func but this is less confusing and contains meaning about the parameters.</remarks>
        public delegate bool WasFoundDelegate(uint pointer, DirectedEdgeId edge, T weight);

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
        public Graph Graph
        {
            get
            {
                return _graph;
            }
        }
    }
}