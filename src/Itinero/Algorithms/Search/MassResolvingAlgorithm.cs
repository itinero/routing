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

using Itinero.Data.Network;
using Itinero.LocalGeo;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// A mass-resolving algorithm.
    /// </summary>
    public sealed class MassResolvingAlgorithm : AlgorithmBase, IMassResolvingAlgorithm
    {
        private readonly IProfileInstance[] _profiles;
        private readonly RouterBase _router;
        private readonly Coordinate[] _locations;
        private readonly Func<RoutingEdge, int, bool> _matchEdge;
        private readonly float _maxSearchDistance;
        
        /// <summary>
        /// Creates a new mass-resolving algorithm.
        /// </summary>
        public MassResolvingAlgorithm(RouterBase router, IProfileInstance[] profiles, Coordinate[] locations,
            Func<RoutingEdge, int, bool> matchEdge = null, float maxSearchDistance = Constants.SearchDistanceInMeter)
        {
            _router = router;
            _profiles = profiles;
            _locations = locations;
            _matchEdge = matchEdge;
            _maxSearchDistance = maxSearchDistance;
        }
        
        /// <summary>
        /// Creates a new mass-resolving algorithm with pre-resolved locations.
        /// </summary>
        public MassResolvingAlgorithm(RouterBase router, IProfileInstance[] profiles, 
            IEnumerable<RouterPoint> routerPoints)
        {
            _router = router;
            _profiles = profiles;
            _resolvedPoints = new List<RouterPoint>(routerPoints);
            
            _locations = _resolvedPoints.Select(x => x.Location()).ToArray();
            _matchEdge = null;
        }

        private Dictionary<int, LocationError> _errors; // all errors per original location idx.
        private List<int> _originalLocationIndexes; // the original location per resolved point index.
        private List<RouterPoint> _resolvedPoints; // only the valid resolved points.

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            if (_resolvedPoints != null)
            { // points are already available.
                _errors = new Dictionary<int, LocationError>();
                this.HasSucceeded = true;
                return;
            }
            
            _errors = new Dictionary<int, LocationError>(_locations.Length);
            _resolvedPoints = new List<RouterPoint>(_locations.Length);
            _originalLocationIndexes = new List<int>(_locations.Length);

            // resolve all locations.
            var resolvedPoints = new RouterPoint[_locations.Length];
            for (var i = 0; i < _locations.Length; i++)
            {
                Result<RouterPoint> resolveResult = null;
                if (_matchEdge != null)
                { // use an edge-matcher.
                    resolveResult = _router.TryResolve(_profiles, _locations[i], (edge) =>
                    {
                        return _matchEdge(edge, i);
                    }, _maxSearchDistance);
                }
                else
                { // don't use an edge-matcher.
                    resolveResult = _router.TryResolve(_profiles, _locations[i], _maxSearchDistance);
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

                    _originalLocationIndexes.Add(i);
                    _resolvedPoints.Add(resolvedPoints[i]);
                }
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
        /// Returns the index of the resolved point, given the original index of in the locations array.
        /// </summary>
        public int ResolvedIndexOf(int locationIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            if (_originalLocationIndexes == null)
            {
                return locationIdx;
            }
            return _originalLocationIndexes.IndexOf(locationIdx);
        }

        /// <summary>
        /// Returns the index of the location in the original locations array, given the resolved point index..
        /// </summary>
        public int LocationIndexOf(int routerPointIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            if (_originalLocationIndexes == null)
            {
                return routerPointIdx;
            }
            return _originalLocationIndexes[routerPointIdx];
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

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        public IProfileInstance[] Profiles
        {
            get
            {
                return _profiles;
            }
        }

        /// <summary>
        /// Gets the router.
        /// </summary>
        public RouterBase Router
        {
            get
            {
                return _router;
            }
        }

        /// <summary>
        /// Gets the original locations.
        /// </summary>
        public Coordinate[] Locations
        {
            get
            {
                return _locations;
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
        NotResolved
    }
}