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

namespace Itinero.Algorithms.Default
{
    /// <summary>
    /// An abstract implementation of the dykstra routing algorithm.
    /// </summary>
    public abstract class Dykstra<T> : AlgorithmBase
    {
        private readonly Graph _graph;
        private readonly IEnumerable<EdgePath<T>> _sources;
        private readonly Func<uint, uint> _getRestriction;
        private readonly float _sourceMax;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public Dykstra(Graph graph, Func<uint, uint> getRestriction,
            IEnumerable<EdgePath<T>> sources, float sourceMax, bool backward)
        {
            _graph = graph;
            _sources = sources;
            _sourceMax = sourceMax;
            _backward = backward;
            _getRestriction = getRestriction;
        }

        private Graph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<uint, EdgePath<T>> _visits;
        private EdgePath<T> _current;
        private BinaryHeap<EdgePath<T>> _heap;

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

            // intialize dykstra data structures.
            _visits = new Dictionary<uint, EdgePath<T>>();
            _heap = new BinaryHeap<EdgePath<T>>(1000);

            // queue all sources.
            foreach (var source in _sources)
            {
                _heap.Push(source, this.GetWeight(source.Weight));
            }

            // gets the edge enumerator.
            _edgeEnumerator = _graph.GetEdgeEnumerator();
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
                while (_current != null && _visits.ContainsKey(_current.Vertex))
                { // keep dequeuing.
                    if (_heap.Count == 0)
                    { // nothing more to pop.
                        break;
                    }
                    _current = _heap.Pop();
                }
            }

            if (_current != null &&
                !_visits.ContainsKey(_current.Vertex))
            { // we visit this one, set visit.
                _visits[_current.Vertex] = _current;
            }
            else
            { // route is not found, there are no vertices left
                // or the search went outside of the max bounds.
                return false;
            }

            if (this.WasFound != null && 
                this.WasFound(_current.Vertex, _current.Weight))
            { // vertex was found and true was returned, this search should stop.
                return false;
            }

            // check for restrictions.
            var restriction = Constants.NO_VERTEX;
            if (_getRestriction != null)
            {
                restriction = _getRestriction(_current.Vertex);
            }
            if (restriction != Constants.NO_VERTEX)
            { // this vertex is restricted, step is a success but just move 
                // to the next one because this vertex's neighbours are not allowed.
                return true;
            }

            // get neighbours and queue them.
            _edgeEnumerator.MoveTo(_current.Vertex);
            while (_edgeEnumerator.MoveNext())
            {
                var edge = _edgeEnumerator;
                var neighbour = edge.To;

                if (this.WasEdgeFound == null)
                {
                    if (_current.From != null &&
                        _current.From.Vertex == neighbour)
                    { // don't go back
                        continue;
                    }

                    if (_visits.ContainsKey(neighbour))
                    { // has already been choosen
                        continue;
                    }
                }

                // get the speed from cache or calculate.
                float distance;
                ushort edgeProfile;
                EdgeDataSerializer.Deserialize(edge.Data0, out distance, out edgeProfile);
                var factor = Factor.NoFactor;
                var totalWeight = this.AddWeight(_current.Weight, edgeProfile, distance, out factor);

                // check the tags against the interpreter.
                if (factor.Value > 0 && (factor.Direction == 0 ||
                    (!_backward && (factor.Direction == 1) != edge.DataInverted) ||
                    (_backward && (factor.Direction == 1) == edge.DataInverted)))
                { // it's ok; the edge can be traversed by the given vehicle.
                    // calculate neighbors weight.
                    var edgeWeight = (distance * factor.Value);

                    if (this.WasEdgeFound != null)
                    {
                        if (this.WasEdgeFound(_current.Vertex, edge.To, _current.Weight, totalWeight, edge.IdDirected(), distance))
                        { // edge was found and true was returned, this search should stop.
                            return false;
                        }

                        if (_current.From != null &&
                            _current.From.Vertex == neighbour)
                        { // don't go back
                            continue;
                        }

                        if (_visits.ContainsKey(neighbour))
                        { // has already been choosen
                            continue;
                        }
                    }

                    if (this.GetWeight(totalWeight) < _sourceMax)
                    { // update the visit list.
                        _heap.Push(new EdgePath<T>(neighbour, totalWeight, _current),
                            this.GetWeight(totalWeight));
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Adds weight to the given weight using the edge profile and the distance.
        /// </summary>
        protected abstract T AddWeight(T weight, ushort edgeProfile, float distance, out Factor factor);

        /// <summary>
        /// Gets the weight value from the weight.
        /// </summary>
        protected abstract float GetWeight(T weight);

        /// <summary>
        /// Sets a visit on a vertex from an external source (like a transit-algorithm).
        /// </summary>
        /// <remarks>The algorithm will pick up these visits as if it was one it's own.</remarks>
        /// <returns>True if the visit was set successfully.</returns>
        public bool SetVisit(EdgePath<T> visit)
        {
            if (!_visits.ContainsKey(visit.Vertex))
            {
                _heap.Push(visit, this.GetWeight(visit.Weight));
                return true;
            }
            return false;
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
        /// Gets the max reached flag.
        /// </summary>
        /// <remarks>True if the source-max value was reached.</remarks>
        public bool MaxReached { get; private set; }

        /// <summary>
        /// The was found delegate.
        /// </summary>
        public delegate bool WasFoundDelegate(uint vertex, T weight);

        /// <summary>
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public WasFoundDelegate WasFound
        {
            get;
            set;
        }

        /// <summary>
        /// The was edge found delegate.
        /// </summary>
        /// <param name="vertex1">The vertex where the search is coming from.</param>
        /// <param name="vertex2">The vertex where the search is going to.</param>
        /// <param name="weight1">The weight at vertex1.</param>
        /// <param name="weight2">The weight at vertex2.</param>
        /// <param name="edge">The id of the current edge.</param>
        /// <param name="length">The length of the current edge.</param>
        /// <returns></returns>
        public delegate bool WasEdgeFoundDelegate(uint vertex1, uint vertex2, T weight1, T weight2, long edge, float length);

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
        public EdgePath<T> Current
        {
            get
            {
                return _current;
            }
        }
    }

    /// <summary>
    /// An implementation of the dykstra algorithm.
    /// </summary>
    public sealed class Dykstra : Dykstra<float>
    {
        private readonly Func<ushort, Factor> _getFactor;

        /// <summary>
        /// Creates a new instance of the dykstra algorithm.
        /// </summary>
        public Dykstra(Graph graph, Func<ushort, Factor> getFactor, Func<uint, uint> getRestriction,
            IEnumerable<EdgePath<float>> sources, float sourceMax, bool backward)
            : base(graph, getRestriction, sources, sourceMax, backward)
        {
            _getFactor = getFactor;

            _factors = new Dictionary<uint, Factor>();
        }

        private Dictionary<uint, Factor> _factors;
        
        /// <summary>
        /// Adds weight to the given weight using the edge profile and the distance.
        /// </summary>
        protected sealed override float AddWeight(float weight, ushort edgeProfile, float distance, out Factor factor)
        {
            if (!_factors.TryGetValue(edgeProfile, out factor))
            { // speed not there, calculate speed.
                factor = _getFactor(edgeProfile);
                _factors.Add(edgeProfile, factor);
            }

            return weight + distance * factor.Value;
        }

        /// <summary>
        /// Gets weight value from the weight.
        /// </summary>
        protected sealed override float GetWeight(float weight)
        {
            return weight;
        }
    }

    /// <summary>
    /// An implementation of the dykstra algorithm with augmented weight.
    /// </summary>
    public sealed class DykstraAugmented : Dykstra<Weight>
    {
        private readonly Func<ushort, FactorAndSpeed> _getFactorAndSpeed;

        /// <summary>
        /// Creates a new instance of the dykstra algorithm.
        /// </summary>
        public DykstraAugmented(Graph graph, Func<ushort, FactorAndSpeed> getFactorAndSpeed, Func<uint, uint> getRestriction,
            IEnumerable<EdgePath<Weight>> sources, float sourceMax, bool backward)
            : base(graph, getRestriction, sources, sourceMax, backward)
        {
            _getFactorAndSpeed = getFactorAndSpeed;

            _factorsAndSpeed = new Dictionary<uint, FactorAndSpeed>();
        }

        private Dictionary<uint, FactorAndSpeed> _factorsAndSpeed;

        /// <summary>
        /// Adds weight to the given weight using the edge profile and the distance.
        /// </summary>
        protected sealed override Weight AddWeight(Weight weight, ushort edgeProfile, float distance, out Factor factor)
        {
            FactorAndSpeed factorAndSpeed;
            if (!_factorsAndSpeed.TryGetValue(edgeProfile, out factorAndSpeed))
            { // speed not there, calculate speed.
                factorAndSpeed = _getFactorAndSpeed(edgeProfile);
                _factorsAndSpeed.Add(edgeProfile, factorAndSpeed);
            }

            factor = new Factor()
            {
                Direction = factorAndSpeed.Direction,
                Value = factorAndSpeed.Value
            };
            return new Weight()
            {
                Distance = weight.Distance + distance,
                Time = weight.Time + (distance * factorAndSpeed.Speed),
                Value = weight.Value + (distance * factorAndSpeed.Value)
            };
        }

        /// <summary>
        /// Gets weight value from the weight.
        /// </summary>
        protected sealed override float GetWeight(Weight weight)
        {
            return weight.Value;
        }
    }
}