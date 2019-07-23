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

using Itinero.Graphs.Geometric;
using Itinero.Algorithms.Search.Hilbert;
using System;
using System.Collections.Generic;
using Itinero.LocalGeo;
using System.Threading;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// An algorithm to search for a good location on a routing network to start routing for a given location.
    /// </summary>
    public class ResolveMultipleAlgorithm : AlgorithmBase
    {
        private readonly GeometricGraph _graph;
        private readonly float _latitude;
        private readonly float _longitude;
        private readonly float _maxOffset;
        private readonly float _maxDistance;
        private readonly Func<GeometricEdge, bool> _isAcceptable;
        private readonly bool _allEdges = false;

        /// <summary>
        /// Creates a new resolve algorithm.
        /// </summary>
        public ResolveMultipleAlgorithm(GeometricGraph graph, float latitude, float longitude,
            float maxOffset, float maxDistance, Func<GeometricEdge, bool> isAcceptable)
            : this (graph, latitude, longitude, maxOffset, maxDistance, isAcceptable, false)
        {
            
        }
        
        /// <summary>
        /// Creates a new resolve algorithm.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="maxOffset">The maximum offset.</param>
        /// <param name="maxDistance">The maximum distance.</param>
        /// <param name="isAcceptable">The acceptable function.</param>
        /// <param name="allEdges">A flag to return all edges or not.</param>
        public ResolveMultipleAlgorithm(GeometricGraph graph, float latitude, float longitude,
            float maxOffset, float maxDistance, Func<GeometricEdge, bool> isAcceptable, bool allEdges)
        {
            _graph = graph;
            _latitude = latitude;
            _longitude = longitude;
            _maxDistance = maxDistance;
            _maxOffset = maxOffset;
            _isAcceptable = isAcceptable;
            _allEdges = allEdges;
        }

        private List<RouterPoint> _results = null;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _results = new List<RouterPoint>();

            // get the closest edge.
            var edges = _graph.SearchCloserThan(_latitude, _longitude,
                    _maxOffset, _maxDistance, _isAcceptable);
            if (edges.Count == 0)
            { // oeps, no edge was found, too far from road network.
                this.ErrorMessage = string.Format("Could not resolve point at [{0}, {1}]. Probably too far from closest road or outside of the loaded network.",
                    _latitude.ToInvariantString(), _longitude.ToInvariantString());
                return;
            }

            // project onto the edge.
            for (var e = 0; e < edges.Count; e++)
            {
                var edgeId = edges[e];
                var edge = _graph.GetEdge(edgeId);
                if (!_graph.ProjectOn(edge, _latitude, _longitude,
                    out _, out _, out var projectedDistanceFromFirst,
                    out _, out var distanceToProjected, out var totalLength))
                {
                    if (!_allEdges) continue;
                    
                    var points = _graph.GetShape(edge);
                    var previous = points[0];

                    var bestProjectedDistanceFromFirst = 0.0f;
                    projectedDistanceFromFirst = 0;
                    var bestDistanceToProjected = Coordinate.DistanceEstimateInMeter(previous,
                        new Coordinate(_latitude, _longitude));
                    for (var i = 1; i < points.Count; i++)
                    {
                        var current = points[i];
                        projectedDistanceFromFirst += Coordinate.DistanceEstimateInMeter(current, previous);
                        distanceToProjected = Coordinate.DistanceEstimateInMeter(current,
                            new Coordinate(_latitude, _longitude));
                        if (distanceToProjected < bestDistanceToProjected)
                        {
                            bestDistanceToProjected = distanceToProjected;
                            bestProjectedDistanceFromFirst = projectedDistanceFromFirst;
                        }
                        previous = current;
                    }

                    // set best distance.
                    projectedDistanceFromFirst = bestProjectedDistanceFromFirst;
                }
                var offset = (ushort)((projectedDistanceFromFirst / totalLength) * ushort.MaxValue);
                _results.Add(new RouterPoint(_latitude, _longitude, edgeId, offset));
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the resulting router points.
        /// </summary>
        public List<RouterPoint> Results
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _results;
            }
        }
    }
}