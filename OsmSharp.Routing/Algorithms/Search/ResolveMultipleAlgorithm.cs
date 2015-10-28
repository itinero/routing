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

using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Search
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

        /// <summary>
        /// Creates a new resolve algorithm.
        /// </summary>
        public ResolveMultipleAlgorithm(GeometricGraph graph, float latitude, float longitude,
            float maxOffset, float maxDistance, Func<GeometricEdge, bool> isAcceptable)
        {
            _graph = graph;
            _latitude = latitude;
            _longitude = longitude;
            _maxDistance = maxDistance;
            _maxOffset = maxOffset;
            _isAcceptable = isAcceptable;
        }

        private List<RouterPoint> _results = null;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
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
                    var bestDistanceToProjected = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(previous.Latitude, previous.Longitude,
                        _latitude, _longitude);
                    projectedLatitude = previous.Latitude;
                    projectedLongitude = previous.Longitude;
                    for (var i = 1; i < points.Count; i++)
                    {
                        var current = points[i];
                        projectedDistanceFromFirst += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(current.Latitude, current.Longitude,
                            previous.Latitude, previous.Longitude);
                        distanceToProjected = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(current.Latitude, current.Longitude,
                            _latitude, _longitude);
                        if (distanceToProjected < bestDistanceToProjected)
                        { // improvement.
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

                var offset = (ushort)((projectedDistanceFromFirst / totalLength) * ushort.MaxValue);
                _results.Add(new RouterPoint(projectedLatitude, projectedLongitude, edgeId, offset));
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