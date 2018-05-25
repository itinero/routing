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

using Itinero.Algorithms.Search.Hilbert;
using Itinero.Graphs.Geometric;
using Itinero.LocalGeo;
using System;
using System.Threading;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// An algorithm to search for a good location on a routing network to start routing for a given location. This algorithm only returns vertices.
    /// </summary>
    public class ResolveVertexAlgorithm : AlgorithmBase, IResolver
    {
        private readonly GeometricGraph _graph;
        private readonly float _latitude;
        private readonly float _longitude;
        private readonly float _maxOffsetInMeter;
        private readonly float _maxDistance;
        private readonly Func<GeometricEdge, bool> _isAcceptable;

        /// <summary>
        /// Threshold below which to always accept the better edges.
        /// </summary>
        public static int BetterEdgeThreshold = 50;

        /// <summary>
        /// Factor to compare better edge distance to acceptable edge distance and decide which edge to take.
        /// </summary>
        public static float BetterEdgeFactor = 2;

        /// <summary>
        /// Creates a new resolve algorithm.
        /// </summary>
        public ResolveVertexAlgorithm(GeometricGraph graph, float latitude, float longitude,
            float maxOffsetInMeter, float maxDistance, Func<GeometricEdge, bool> isAcceptable)
        {
            _graph = graph;
            _latitude = latitude;
            _longitude = longitude;
            _maxDistance = maxDistance;
            _maxOffsetInMeter = maxOffsetInMeter;
            _isAcceptable = isAcceptable;
        }

        private RouterPoint _result = null;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // calculate maxOffset in degrees.
            var offsettedLocation = (new Coordinate(_latitude, _longitude)).OffsetWithDistances(_maxOffsetInMeter);
            var latitudeOffset = System.Math.Abs(_latitude - offsettedLocation.Latitude);
            var longitudeOffset = System.Math.Abs(_longitude - offsettedLocation.Longitude);

            // get the closest edge.
            uint vertexId = _graph.SearchClosest(_latitude, _longitude,
                    latitudeOffset, longitudeOffset, _isAcceptable);

            if (vertexId == Constants.NO_EDGE)
            { // oeps, no edge was found, too far from road network.
                this.ErrorMessage = string.Format("Could not resolve point at [{0}, {1}]. Probably too far from closest road or outside of the loaded network.",
                    _latitude.ToInvariantString(), _longitude.ToInvariantString());
                return;
            }

            _result = _graph.CreateRouterPointForVertex(vertexId, _isAcceptable);

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the resulting router point.
        /// </summary>
        public RouterPoint Result
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _result;
            }
        }
    }
}