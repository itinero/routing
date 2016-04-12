// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.PriorityQueues;
using Itinero.Data.Edges;
using Itinero.Graphs;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default.Edge
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra : AlgorithmBase
    {
        private readonly Graph _graph;
        private readonly IEnumerable<EdgePath> _sources;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly Func<uint, uint[]> _getRestriction;
        private readonly float _sourceMax;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public Dykstra(Graph graph, Func<ushort, Factor> getFactor, Func<uint, uint[]> getRestriction,
            IEnumerable<EdgePath> sources, float sourceMax, bool backward)
        {
            _graph = graph;
            _sources = sources;
            _getFactor = getFactor;
            _sourceMax = sourceMax;
            _backward = backward;
            _getRestriction = getRestriction;
        }

        private Graph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<long, EdgePath> _visits;
        private EdgePath _current;
        private BinaryHeap<EdgePath> _heap;
        private Dictionary<uint, Factor> _factors;
        private Dictionary<long, LinkedRestriction> _edgeRestrictions;
        private HashSet<uint> _vertexRestrictions;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
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
            _visits = new Dictionary<long, EdgePath>();
            _heap = new BinaryHeap<EdgePath>(1000);
            _edgeRestrictions = new Dictionary<long, LinkedRestriction>();
            _vertexRestrictions = new HashSet<uint>();
            
            // gets the edge enumerator.
            _edgeEnumerator = _graph.GetEdgeEnumerator();

            // queue all sources.
            foreach (var source in _sources)
            {
                if (_getRestriction != null)
                {
                    var sourceVertex = _edgeEnumerator.GetSourceVertex(source.DirectedEdge);
                    var restriction = _getRestriction(sourceVertex);
                    if (restriction != null &&
                        restriction.Length > 1)
                    {
                        var targetVertex = _edgeEnumerator.GetTargetVertex(source.DirectedEdge);
                        if (restriction.Length == 2)
                        { // a restriction of two, an edge is forbidden.
                            if (restriction[1] == targetVertex)
                            { // don't queue this edge, it's forbidden.
                                continue;
                            }
                        }
                        else
                        { // a restriction bigger than two, check if this edge is the first one.
                            if (restriction[1] == targetVertex)
                            { // this edge is the first, queue the restriction too.
                                _edgeRestrictions[source.DirectedEdge] = new LinkedRestriction()
                                {
                                    Restriction = restriction,
                                    Next = null
                                };
                            }
                        }
                    }
                }
                _heap.Push(source, source.Weight);
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
                while (_current != null && _visits.ContainsKey(_current.DirectedEdge))
                { // keep dequeuing.
                    if (_heap.Count == 0)
                    { // nothing more to pop.
                        break;
                    }
                    _current = _heap.Pop();
                }
            }

            if (_current != null &&
                !_visits.ContainsKey(_current.DirectedEdge))
            { // we visit this one, set visit.
                _visits[_current.DirectedEdge] = _current;
            }
            else
            { // route is not found, there are no vertices left
                // or the search went outside of the max bounds.
                return false;
            }

            // report on visit.
            if (this.WasEdgeFound != null &&
                this.WasEdgeFound(_current.DirectedEdge, _current.Weight))
            {
                return false;
            }

            // move to the current edge's target vertex.
            _edgeEnumerator.MoveToTargetVertex(_current.DirectedEdge);

            // check for restrictions.
            var targetVertex = _edgeEnumerator.From;
            LinkedRestriction restrictions = null;
            if (_edgeRestrictions.TryGetValue(_current.DirectedEdge, out restrictions))
            {
                _edgeRestrictions.Remove(_current.DirectedEdge);
            }
            if (_vertexRestrictions.Contains(targetVertex))
            { // restricted vertex, no need to check outgoing edges.
                return true;
            }
            if (_getRestriction != null)
            {
                var restriction = _getRestriction(targetVertex);
                if (restriction != null &&
                    restriction.Length > 0)
                {
                    if (restriction.Length == 1)
                    { // a simple restriction, restricted vertex, no need to check outgoing edges.
                        _vertexRestrictions.Add(restriction[0]);
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
            while (_edgeEnumerator.MoveNext())
            {
                var edge = _edgeEnumerator;
                var directedEdgeId = _edgeEnumerator.IdDirected();

                if (directedEdgeId == -_current.DirectedEdge)
                { // don't go back.
                    continue;
                }

                if (_visits.ContainsKey(directedEdgeId))
                { // has already been choosen.
                    continue;
                }

                // verify restriction(s).
                LinkedRestriction newRestrictions = null;
                if(_edgeRestrictions.TryGetValue(directedEdgeId, out newRestrictions))
                { // restriction for this edge, check if the entire edge is forbidden.
                    if (newRestrictions.ContainsAnyLength(2))
                    { // move to the next edge, this one is forbidden.
                        continue;
                    }
                }
                var currentRestriction = restrictions;
                var forbidden = false;
                while (currentRestriction != null)
                { // check if some restriction prohibits this move or if we need add a new restriction
                    // for the current edge.
                    if (currentRestriction.Restriction[2] == _edgeEnumerator.To)
                    { // ok restrictions applies to this edge and the previous one.
                        if (currentRestriction.Restriction.Length == 3)
                        { // ok this is the last edge in this restriction, prohibit this move.
                            forbidden = true;
                            break;
                        }
                        else
                        { // append this restriction to the restrictions in for the current edge.
                            var newRestriction = new uint[currentRestriction.Restriction.Length - 1];
                            currentRestriction.Restriction.CopyTo(newRestriction, 0, 1, newRestriction.Length);
                            newRestrictions = new LinkedRestriction()
                            {
                                Restriction = newRestriction,
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
                if (newRestrictions != null)
                {
                    _edgeRestrictions[directedEdgeId] = newRestrictions;
                }

                // get the speed from cache or calculate.
                float distance;
                ushort edgeProfile;
                EdgeDataSerializer.Deserialize(edge.Data0, out distance, out edgeProfile);
                var factor = Factor.NoFactor;
                if (!_factors.TryGetValue(edgeProfile, out factor))
                { // speed not there, calculate speed.
                    factor = _getFactor(edgeProfile);
                    _factors.Add(edgeProfile, factor);
                }

                // check the tags against the interpreter.
                if (factor.Value > 0 && (factor.Direction == 0 ||
                    (!_backward && (factor.Direction == 1) != edge.DataInverted) ||
                    (_backward && (factor.Direction == 1) == edge.DataInverted)))
                { // it's ok; the edge can be traversed by the given vehicle.
                    // calculate neighbors weight.
                    var totalWeight = _current.Weight + (distance * factor.Value);
                    if (totalWeight < _sourceMax)
                    { // update the visit list.
                        _heap.Push(new EdgePath(directedEdgeId, totalWeight, _current),
                            totalWeight);
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
        public bool SetVisit(EdgePath visit)
        {
            if (!_visits.ContainsKey(visit.DirectedEdge))
            {
                _heap.Push(visit, visit.Weight);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given edge was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        public bool TryGetVisit(long edge, out EdgePath visit)
        {
            return _visits.TryGetValue(edge, out visit);
        }

        /// <summary>
        /// Gets the max reached flag.
        /// </summary>
        /// <remarks>True if the source-max value was reached.</remarks>
        public bool MaxReached { get; private set; }

        /// <summary>
        /// The was edge found delegate.
        /// </summary>
        public delegate bool WasEdgeFoundDelegate(long directedEdgeId, float weight);

        /// <summary>
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public WasEdgeFoundDelegate WasEdgeFound
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
        public EdgePath Current
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
}
