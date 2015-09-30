// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Routing.Data;
using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Routing
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra : AlgorithmBase
    {
        private readonly Graph _graph;
        private readonly IEnumerable<Path> _sources;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly float _sourceMax;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new one-to-all dykstra algorithm instance.
        /// </summary>
        public Dykstra(Graph graph, Func<ushort, Factor> getFactor,
            IEnumerable<Path> sources, float sourceMax, bool backward)
        {
            _graph = graph;
            _sources = sources;
            _getFactor = getFactor;
            _sourceMax = sourceMax;
            _backward = backward;
        }

        private Graph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<uint, Path> _visits;
        private Path _current;
        private BinaryHeap<Path> _heap;
        private Dictionary<uint, Factor> _factors;

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

            // initialize a dictionary of speeds per profile.
            _factors = new Dictionary<uint, Factor>();

            // intialize dykstra data structures.
            _visits = new Dictionary<uint, Path>();
            _heap = new BinaryHeap<Path>(1000);

            // queue all sources.
            foreach (var source in _sources)
            {
                _heap.Push(source, source.Weight);
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

            // get neighbours.
            _edgeEnumerator.MoveTo(_current.Vertex);
            while (_edgeEnumerator.MoveNext())
            {
                var edge = _edgeEnumerator;
                var neighbour = edge.To;

                if (_current.From != null && 
                    _current.From.Vertex == neighbour)
                { // don't go back!
                    continue;
                }

                if (_visits.ContainsKey(neighbour))
                { // has already been choosen.
                    continue;
                }

                // get the speed from cache or calculate.
                float distance;
                ushort profile;
                EdgeDataSerializer.Deserialize(edge.Data0, out distance, out profile);
                var factor = Factor.NoFactor;
                if (!_factors.TryGetValue(profile, out factor))
                { // speed not there, calculate speed.
                    factor = _getFactor(profile);
                    _factors.Add(profile, factor);
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
                        _heap.Push(new Path(neighbour, totalWeight, _current),
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
        public bool SetVisit(Path visit)
        {
            if (!_visits.ContainsKey(visit.Vertex))
            {
                _heap.Push(visit, visit.Weight);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given vertex was visited and sets the visit output parameters with the actual visit data.
        /// </summary>
        /// <returns></returns>
        public bool TryGetVisit(uint vertex, out Path visit)
        {
            return _visits.TryGetValue(vertex, out visit);
        }

        /// <summary>
        /// Gets the max reached flag.
        /// </summary>
        /// <remarks>True if the source-max value was reached.</remarks>
        public bool MaxReached { get; private set; }

        /// <summary>
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public Func<long, float, bool> WasFound
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
        public Path Current
        {
            get
            {
                return _current;
            }
        }
    }
}