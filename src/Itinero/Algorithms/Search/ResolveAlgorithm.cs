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
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using System;
using System.Threading;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// An algorithm to search for a good location on a routing network to start routing for a given location.
    /// </summary>
    public class ResolveAlgorithm : AlgorithmBase, IResolver
    {
        private readonly GeometricGraph _graph;
        private readonly float _latitude;
        private readonly float _longitude;
        private readonly float _maxOffsetInMeter;
        private readonly float _maxDistance;
        private readonly Func<GeometricEdge, bool> _isAcceptable;
        private readonly Func<GeometricEdge, bool> _isBetter;

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
        public ResolveAlgorithm(GeometricGraph graph, float latitude, float longitude,
            float maxOffset, float maxDistance, Func<GeometricEdge, bool> isAcceptable)
            : this(graph, latitude, longitude, maxOffset, maxDistance, isAcceptable, null)
        {

        }

        /// <summary>
        /// Creates a new resolve algorithm.
        /// </summary>
        public ResolveAlgorithm(GeometricGraph graph, float latitude, float longitude,
            float maxOffsetInMeter, float maxDistance, Func<GeometricEdge, bool> isAcceptable, Func<GeometricEdge, bool> isBetter)
        {
            _graph = graph;
            _latitude = latitude;
            _longitude = longitude;
            _maxDistance = maxDistance;
            _maxOffsetInMeter = maxOffsetInMeter;
            _isAcceptable = isAcceptable;
            _isBetter = isBetter;

            if (_maxDistance > _maxOffsetInMeter)
            {
                _maxOffsetInMeter = _maxDistance;
            }
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
            uint[] edgeIds = null;
            if(_isBetter == null)
            { // do not evaluate both, just isOk.
                edgeIds = new uint[2];
                edgeIds[0] = _graph.SearchClosestEdge(_latitude, _longitude,
                    latitudeOffset, longitudeOffset, _maxDistance, _isAcceptable, cancellationToken);
            }
            else
            { // evaluate both.
                edgeIds = _graph.SearchClosestEdges(_latitude, _longitude,
                    latitudeOffset, longitudeOffset, _maxDistance, new Func<GeometricEdge, bool>[] { _isAcceptable, (potentialEdge) => 
                        { // at least also make sure the edge is acceptable.
                            if (_isAcceptable(potentialEdge))
                            {
                                return _isBetter(potentialEdge);
                            }
                            return false;
                        }});
            }
            
            if (edgeIds[0] == Constants.NO_EDGE)
            { // oeps, no edge was found, too far from road network.
                this.ErrorMessage = string.Format("Could not resolve point at [{0}, {1}]. Probably too far from closest road or outside of the loaded network.",
                    _latitude.ToInvariantString(), _longitude.ToInvariantString());
                return;
            }

            // project onto the edge.
            var edge = _graph.GetEdge(edgeIds[0]);
            var edgeId = edgeIds[0];
            float projectedLatitude, projectedLongitude, projectedDistanceFromFirst, totalLength, distanceToProjected;
            int projectedShapeIndex;
            if (!_graph.ProjectOn(edge, _latitude, _longitude,
                out projectedLatitude, out projectedLongitude, out projectedDistanceFromFirst,
                out projectedShapeIndex, out distanceToProjected, out totalLength))
            { // oeps, could not project onto edge.              
                var points = _graph.GetShape(edge);
                var previous = points[0];

                var bestProjectedDistanceFromFirst = 0.0f;
                projectedDistanceFromFirst = 0;
                var bestDistanceToProjected = Coordinate.DistanceEstimateInMeter(previous,
                    new Coordinate(_latitude, _longitude));
                projectedLatitude = previous.Latitude;
                projectedLongitude = previous.Longitude;
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
                        projectedLatitude = current.Latitude;
                        projectedLongitude = current.Longitude;
                    }
                    previous = current;
                }

                // set best distance.
                projectedDistanceFromFirst = bestProjectedDistanceFromFirst;
            }

            if(_isBetter != null)
            { // there was a request to search for better edges.
                if(edgeIds[0] != edgeIds[1] &&
                   edgeIds[1] != Constants.NO_EDGE)
                { // edges are not equal, check if the better edge is acceptable.
                    // project onto the better edge.
                    var edge1 = _graph.GetEdge(edgeIds[1]);
                    float projectedLatitude1, projectedLongitude1, projectedDistanceFromFirst1, totalLength1, 
                        distanceToProjected1;
                    int projectedShapeIndex1;
                    if (!_graph.ProjectOn(edge1, _latitude, _longitude,
                        out projectedLatitude1, out projectedLongitude1, out projectedDistanceFromFirst1,
                        out projectedShapeIndex1, out distanceToProjected1, out totalLength1))
                    { // oeps, could not project onto edge.              
                        var points = _graph.GetShape(edge1);
                        var previous = points[0];

                        var bestProjectedDistanceFromFirst = 0.0f;
                        projectedDistanceFromFirst1 = 0;
                        var bestDistanceToProjected = Coordinate.DistanceEstimateInMeter(previous,
                            new Coordinate(_latitude, _longitude));
                        projectedLatitude1 = previous.Latitude;
                        projectedLongitude1 = previous.Longitude;
                        for (var i = 1; i < points.Count; i++)
                        {
                            var current = points[i];
                            projectedDistanceFromFirst1 += Coordinate.DistanceEstimateInMeter(current, previous);
                            distanceToProjected1 = Coordinate.DistanceEstimateInMeter(current,
                                new Coordinate(_latitude, _longitude));
                            if (distanceToProjected1 < bestDistanceToProjected)
                            {
                                bestDistanceToProjected = distanceToProjected1;
                                bestProjectedDistanceFromFirst = projectedDistanceFromFirst1;
                                projectedLatitude1 = current.Latitude;
                                projectedLongitude1 = current.Longitude;
                            }
                            previous = current;
                        }

                        // set best distance.
                        projectedDistanceFromFirst1 = bestProjectedDistanceFromFirst;
                    }

                    if(distanceToProjected1 <= BetterEdgeThreshold ||
                       distanceToProjected1 <= distanceToProjected * BetterEdgeFactor)
                    { // ok, take the better edge.
                        totalLength = totalLength1;
                        edgeId = edgeIds[1];
                        projectedLatitude = projectedLatitude1;
                        projectedLongitude = projectedLongitude1;
                        projectedDistanceFromFirst = projectedDistanceFromFirst1;
                    }
                }
            }

            var offset = (ushort)((projectedDistanceFromFirst / totalLength) * ushort.MaxValue);
            _result = new RouterPoint(_latitude, _longitude, edgeId, offset);

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