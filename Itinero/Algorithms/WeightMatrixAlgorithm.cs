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

using Itinero.LocalGeo;
using Itinero.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms
{
    /// <summary>
    /// An algorithm to calculate a weight-matrix for a set of locations.
    /// </summary>
    public class WeightMatrixAlgorithm : AlgorithmBase, IWeightMatrixAlgorithm
    {
        private readonly IRouter _router;
        private readonly Profile _profile;
        private readonly Coordinate[] _locations;
        private readonly Func<RoutingEdge, int, bool> _matchEdge;

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(IRouter router, Profile profile, Coordinate[] locations)
            : this(router, profile, locations, null)
        {

        }

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(IRouter router, Profile profile, Coordinate[] locations,
            Func<RoutingEdge, int, bool> matchEdge)
        {
            _router = router;
            _profile = profile;
            _locations = locations;
            _matchEdge = matchEdge;
        }

        private Dictionary<int, LocationError> _errors; // all errors per original location idx.
        private List<int> _resolvedPointsIndices; // the original location per resolved point index.
        private List<RouterPoint> _resolvedPoints; // only the valid resolved points.
        private float[][] _weights; // the weights between all valid resolved points.

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _errors = new Dictionary<int, LocationError>(_locations.Length);
            _resolvedPoints = new List<RouterPoint>(_locations.Length);
            _resolvedPointsIndices = new List<int>(_locations.Length);

            // resolve all locations.
            var resolvedPoints = new RouterPoint[_locations.Length];
            var profiles = new Profile[] { _profile };
            for (var i = 0; i < _locations.Length; i++)
            {
                Result<RouterPoint> resolveResult = null;
                if (_matchEdge != null)
                { // use an edge-matcher.
                    resolveResult = _router.TryResolve(profiles, _locations[i], (edge) =>
                    {
                        return _matchEdge(edge, i);
                    });
                }
                else
                { // don't use an edge-matcher.
                    resolveResult = _router.TryResolve(profiles, _locations[i]);
                }

                if (!resolveResult.IsError)
                { // resolving was succesful.
                    resolvedPoints[i] = resolveResult.Value;
                }
            }

            // remove all points that could not be resolved.
            for (var i = 0; i < resolvedPoints.Length; i++)
            {
                if (resolvedPoints[i] == null)
                { // could not be resolved!
                    _errors[i] = new LocationError()
                    {
                        Code = LocationErrorCode.NotResolved,
                        Message = "Location could not be linked to the road network."
                    };
                }
                else
                { // resolve is ok.
                    resolvedPoints[i].Attributes.AddOrReplace("index", i.ToInvariantString());

                    _resolvedPointsIndices.Add(i);
                    _resolvedPoints.Add(resolvedPoints[i]);
                }
            }

            // calculate matrix.
            var nonNullResolvedArray = _resolvedPoints.ToArray();
            var nonNullInvalids = new HashSet<int>();
            _weights = _router.CalculateWeight(_profile, nonNullResolvedArray, nonNullInvalids);

            // take into account the non-null invalids now.
            if (nonNullInvalids.Count > 0)
            { // shrink lists and add errors.
                foreach (var invalid in nonNullInvalids)
                {
                    _errors[_resolvedPointsIndices[invalid]] = new LocationError()
                    {
                        Code = LocationErrorCode.NotRoutable,
                        Message = "Location could not routed to or from."
                    };
                }

                _resolvedPoints = _resolvedPoints.ShrinkAndCopyList(nonNullInvalids);
                _resolvedPointsIndices = _resolvedPointsIndices.ShrinkAndCopyList(nonNullInvalids);
                _weights = _weights.SchrinkAndCopyMatrix(nonNullInvalids);
            }
            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        public List<RouterPoint> RouterPoints
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _resolvedPoints;
            }
        }

        /// <summary>
        /// Gets the weights between all valid router points.
        /// </summary>
        public float[][] Weights
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _weights;
            }
        }

        /// <summary>
        /// Returns the index of the location in the resolved points list.
        /// </summary>
        /// <returns></returns>
        public int IndexOf(int locationIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _resolvedPointsIndices.IndexOf(locationIdx);
        }

        /// <summary>
        /// Returns the index of the router point in the original locations array.
        /// </summary>
        /// <returns></returns>
        public int LocationIndexOf(int routerPointIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _resolvedPointsIndices[routerPointIdx];
        }

        /// <summary>
        /// Returns the errors indexed per location idx.
        /// </summary>
        public Dictionary<int, LocationError> Errors
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _errors;
            }
        }
    }

    /// <summary>
    /// A represention of an error that can occur for a location.
    /// </summary>
    public class LocationError
    {
        /// <summary>
        /// Gets or sets the error-code.
        /// </summary>
        public LocationErrorCode Code { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// The types of errors that can occur for a location.
    /// </summary>
    public enum LocationErrorCode
    {
        /// <summary>
        /// What happened?
        /// </summary>
        Unknown,
        /// <summary>
        /// Cannot find a suitable road nearby.
        /// </summary>
        NotResolved,
        /// <summary>
        /// Cannot find a route to this location.
        /// </summary>
        NotRoutable
    }
}