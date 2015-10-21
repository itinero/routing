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
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Contracted
{
    /// <summary>
    /// An implementation of the dykstra routing algorithm.
    /// </summary>
    public class Dykstra : AlgorithmBase
    {
        private readonly DirectedMetaGraph _graph;
        private readonly IEnumerable<Path> _sources;
        private readonly bool _backward;

        /// <summary>
        /// Creates a new routing algorithm instance.
        /// </summary>
        public Dykstra(DirectedMetaGraph graph, IEnumerable<Path> sources, bool backward)
        {
            _graph = graph;
            _sources = sources;
            _backward = backward;
        }

        private DirectedGraph.EdgeEnumerator _edgeEnumerator;
        private Dictionary<uint, Path> _visits;
        private Path _current;
        private BinaryHeap<Path> _heap;

        /// <summary>
        /// Executes the actual run of the algorithm.
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
            _visits = new Dictionary<uint, Path>();
            _heap = new BinaryHeap<Path>();

            // queue all sources.
            foreach (var source in _sources)
            {
                _heap.Push(source, source.Weight);
            }

            // gets the edge enumerator.
            _edgeEnumerator = _graph.Graph.GetEdgeEnumerator();
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
                this.WasFound(_current.Vertex, _current.Weight);
            }

            _edgeEnumerator.MoveTo(_current.Vertex);
            while (_edgeEnumerator.MoveNext())
            {
                float neighbourWeight;
                bool? neighbourDirection;
                ContractedEdgeDataSerializer.Deserialize(_edgeEnumerator.Data0, out neighbourWeight, out neighbourDirection);
                if (neighbourDirection == null || neighbourDirection.Value == !_backward)
                { // the edge is forward, and is to higher or was not contracted at all.
                    var neighbourNeighbour = _edgeEnumerator.Neighbour;
                    if (!_visits.ContainsKey(neighbourNeighbour))
                    { // if not yet settled.
                        var routeToNeighbour = new Path(
                            neighbourNeighbour, _current.Weight + neighbourWeight, _current);
                        _heap.Push(routeToNeighbour, routeToNeighbour.Weight);
                    }
                }
            }
            return true;
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
        /// Gets or sets the wasfound function to be called when a new vertex is found.
        /// </summary>
        public Func<uint, float, bool> WasFound
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
        public Path Current
        {
            get
            {
                return _current;
            }
        }
    }
}