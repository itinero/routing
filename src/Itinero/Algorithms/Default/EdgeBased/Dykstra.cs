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

using Itinero.Algorithms.Networks.Analytics;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Data.Edges;
using Itinero.Graphs;
using Itinero.Profiles;
using System;
using System.Threading;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra<T> : AlgorithmBase, IEdgeVisitor<T>
        where T : struct
    {
        private readonly Graph _graph;
        private readonly IEnumerable<EdgePath<T>> _sources;
        private readonly WeightHandler<T> _weightHandler;
        private readonly Func<uint, IEnumerable<uint[]>> _getRestriction;
        private readonly T _sourceMax;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public Dykstra(Graph graph, WeightHandler<T> weightHandler, Func<uint, IEnumerable<uint[]>> getRestriction,
            IEnumerable<EdgePath<T>> sources, T sourceMax, bool backward)
        {
            _graph = graph;
            _sources = sources;
            _weightHandler = weightHandler;
            _sourceMax = sourceMax;
            _backward = backward;
            _getRestriction = getRestriction;
        }

        private Graph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<long, EdgePath<T>> _visits;
        private EdgePath<T> _current;
        private BinaryHeap<EdgePath<T>> _heap;
        private Dictionary<uint, Factor> _factors;
        private Dictionary<EdgePath<T>, LinkedRestriction> _edgeRestrictions;

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

            // initialize a dictionary of speeds per edge profile.
            _factors = new Dictionary<uint, Factor>();

            // intialize dykstra data structures.
            _visits = new Dictionary<long, EdgePath<T>>();
            _heap = new BinaryHeap<EdgePath<T>>(1000);
            _edgeRestrictions = new Dictionary<EdgePath<T>, LinkedRestriction>();

            // initialize the edge enumerator.
            _edgeEnumerator = _graph.GetEdgeEnumerator();

            // queue all sources.
            foreach (var source in _sources)
            {
                var queue = true;
                if (_getRestriction != null && source.Edge != Constants.NO_EDGE)
                {
                    var sourceVertex = _edgeEnumerator.GetSourceVertex(source.Edge);
                    var sourceVertexRestrictions = _getRestriction(sourceVertex);
                    LinkedRestriction linkedRestriction = null;
                    if (sourceVertexRestrictions != null)
                    {
                        foreach (var restriction in sourceVertexRestrictions)
                        {
                            if (restriction != null &&
                                restriction.Length > 1)
                            {
                                var targetVertex = _edgeEnumerator.GetTargetVertex(source.Edge);
                                if (restriction.Length == 2)
                                { // a restriction of two, an edge is forbidden.
                                    if (restriction[1] == targetVertex)
                                    { // don't queue this edge, it's forbidden.
                                        queue = false;
                                        break;
                                    }
                                }
                                else
                                { // a restriction bigger than two, check if this edge is the first one.
                                    if (restriction[1] == targetVertex)
                                    { // this edge is the first, queue the restriction too.
                                        linkedRestriction = new LinkedRestriction()
                                        {
                                            Restriction = restriction.SubArray(1, restriction.Length - 1),
                                            Next = linkedRestriction
                                        };
                                        _edgeRestrictions[source] = linkedRestriction;
                                    }
                                }
                            }
                        }
                    }
                }
                if (queue)
                {
                    _heap.Push(source, _weightHandler.GetMetric(source.Weight));
                }
            }
        }

        /// <summary>
        /// Executes one step in the search.
        /// </summary>
        public bool Step()
        {
            // while the visit list is not empty.
            _current = null;
            if (_heap.Count > 0)
            { // choose the next vertex.
                _current = _heap.Pop();
                while (_current != null && _visits.ContainsKey(_current.Edge))
                { // keep dequeuing.
                    if (_heap.Count == 0)
                    { // nothing more to pop.
                        break;
                    }
                    _current = _heap.Pop();
                }
            }

            if (_current != null)
            { // we visit this one, set visit.
                if (_current.Edge != Constants.NO_EDGE)
                {
                    _visits[_current.Edge] = _current;

                    // report on visit.
                    if (this.Visit != null)
                    {
                        if (this.Visit(_current))
                        {
                            return true;
                        }
                    }
                }
            }
            else
            { // route is not found, there are no vertices left
                // or the search went outside of the max bounds.
                return false;
            }

            // move to the current edge's target vertex.
            _edgeEnumerator.MoveTo(_current.Vertex);

            // get new restrictions at the current vertex.
            LinkedRestriction restrictions = null;
            if (_edgeRestrictions.TryGetValue(_current, out restrictions))
            {
                _edgeRestrictions.Remove(_current);
            }
            if (_getRestriction != null)
            {
                var targetVertexRestriction = _getRestriction(_current.Vertex);
                if (targetVertexRestriction != null)
                {
                    foreach (var restriction in targetVertexRestriction)
                    {
                        if (restriction != null &&
                            restriction.Length > 0)
                        {
                            if (restriction.Length == 1)
                            { // a simple restriction, restricted vertex, no need to check outgoing edges.
                                return true;
                            }
                            else
                            { // a complex restriction.
                                restrictions = new LinkedRestriction()
                                {
                                    Restriction = restriction,
                                    Next = restrictions
                                };
                            }
                        }
                    }
                }
            }
            while (_edgeEnumerator.MoveNext())
            {
                var edge = _edgeEnumerator;
                var directedEdgeId = _edgeEnumerator.IdDirected();
                var neighbour = edge.To;

                if (directedEdgeId == -_current.Edge)
                { // don't go back.
                    continue;
                }

                if (_visits.ContainsKey(directedEdgeId))
                { // has already been choosen.
                    continue;
                }

                // get the speed from cache or calculate.
                float distance;
                ushort edgeProfile;
                EdgeDataSerializer.Deserialize(edge.Data0, out distance, out edgeProfile);
                var factor = Factor.NoFactor;
                var edgeWeight = _weightHandler.Calculate(edgeProfile, distance, out factor);

                // check the tags against the interpreter.
                if (factor.Value > 0 && (factor.Direction == 0 ||
                    (!_backward && (factor.Direction == 1) != edge.DataInverted) ||
                    (_backward && (factor.Direction == 1) == edge.DataInverted)))
                { // it's ok; the edge can be traversed by the given vehicle.

                    // verify restriction(s).
                    var currentRestriction = restrictions;
                    var forbidden = false;
                    LinkedRestriction newRestrictions = null;
                    while (currentRestriction != null)
                    { // check if some restriction prohibits this move or if we need add a new restriction
                        // for the current edge.
                        if (currentRestriction.Restriction[1] == _edgeEnumerator.To)
                        { // ok restrictions applies to this edge and the previous one.
                            if (currentRestriction.Restriction.Length == 2)
                            { // ok this is the last edge in this restriction, prohibit this move.
                                forbidden = true;
                                break;
                            }
                            else
                            { // append this restriction to the restrictions in for the current edge.
                                newRestrictions = new LinkedRestriction()
                                {
                                    Restriction = currentRestriction.Restriction.SubArray(1, currentRestriction.Restriction.Length - 1),
                                    Next = newRestrictions
                                };
                            }
                        }
                        currentRestriction = currentRestriction.Next;
                    }
                    if (forbidden)
                    { // move to next neighbour.
                        continue;
                    }

                    // calculate neighbors weight.
                    var totalWeight = _weightHandler.Add(_current.Weight, edgeWeight);
                    if (_weightHandler.IsSmallerThan(totalWeight, _sourceMax))
                    { // update the visit list.

                        var path = new EdgePath<T>(neighbour, totalWeight, directedEdgeId, _current);
                        if (newRestrictions != null)
                        {
                            _edgeRestrictions[path] = newRestrictions;
                        }
                        _heap.Push(path, _weightHandler.GetMetric(totalWeight));
                    }
                    else
                    { // the maxium was reached.
                        this.MaxReached = true;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Sets a visit on a vertex from an external source (like a transit-algorithm).
        /// </summary>
        /// <remarks>The algorithm will pick up these visits as if it was one it's own.</remarks>
        /// <returns>True if the visit was set successfully.</returns>
        public bool SetVisit(EdgePath<T> visit)
        {
            if (!_visits.ContainsKey(visit.Edge))
            {
                _heap.Push(visit, _weightHandler.GetMetric(visit.Weight));
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given edge was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        public bool TryGetVisit(long edge, out EdgePath<T> visit)
        {
            return _visits.TryGetValue(edge, out visit);
        }

        /// <summary>
        /// Gets the max reached flag.
        /// </summary>
        /// <remarks>True if the source-max value was reached.</remarks>
        public bool MaxReached { get; private set; }

        /// <summary>
        /// Gets or sets the visit function to be called when a new path is found.
        /// </summary>
        public VisitDelegate<T> Visit
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

        /// <summary>
        /// A linked restriction.
        /// </summary>
        private class LinkedRestriction
        {
            /// <summary>
            /// Gets the restriction.
            /// </summary>
            public uint[] Restriction { get; set; }

            /// <summary>
            /// Gets the next linked restriction.
            /// </summary>
            public LinkedRestriction Next { get; set; }

            /// <summary>
            /// Returns true if any restriction exists with a given length.
            /// </summary>
            public bool ContainsAnyLength(int length)
            {
                var cur = this;
                while(cur != null)
                {
                    if (cur.Restriction != null &&
                        cur.Restriction.Length == length)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }

    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public sealed class Dykstra : Dykstra<float>
    {
        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public Dykstra(Graph graph, Func<ushort, Factor> getFactor, Func<uint, IEnumerable<uint[]>> getRestriction,
            IEnumerable<EdgePath<float>> sources, float sourceMax, bool backward)
            : base(graph, new DefaultWeightHandler(getFactor), getRestriction, sources, sourceMax, backward)
        {

        }

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public Dykstra(Graph graph, DefaultWeightHandler weightHandler, Func<uint, IEnumerable<uint[]>> getRestriction,
            IEnumerable<EdgePath<float>> sources, float sourceMax, bool backward)
            : base(graph, weightHandler, getRestriction, sources, sourceMax, backward)
        {

        }
    }
}