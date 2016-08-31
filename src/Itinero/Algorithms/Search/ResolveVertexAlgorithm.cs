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

using Itinero.Algorithms.Search.Hilbert;
using Itinero.Graphs.Geometric;
using Itinero.LocalGeo;
using System;

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
        protected override void DoRun()
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