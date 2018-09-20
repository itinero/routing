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

using Itinero.LocalGeo;
using Itinero.Profiles;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// A dummy mass-resolve implementation which returns presolved results
    /// </summary>
    public class PresolvedMassResolvingAlgorithm : AlgorithmBase, IMassResolvingAlgorithm
    {
        private readonly RouterBase _router;
        private readonly IProfileInstance[] _profiles;
        private readonly Coordinate[] _locations;
        private readonly List<RouterPoint> _routerPoints;

        private readonly Dictionary<int, LocationError> _errors = new Dictionary<int, LocationError>();

        /// <summary>
        /// Creates a new presolved mass resolve algorithm
        /// </summary>
        public PresolvedMassResolvingAlgorithm(RouterBase router, IProfileInstance[] profiles,
            List<RouterPoint> routerPoints)
        {
            _router = router;
            _profiles = profiles;
            _routerPoints = routerPoints;
            
            _locations = new Coordinate[_routerPoints.Count];
            for (var l = 0; l < _locations.Length; l++)
            {
                _locations[l] = _routerPoints[l].Location();
            }
        }

        /// <summary>
        /// Returns the errors indexed per location idx.
        /// </summary>
        public Dictionary<int, LocationError> Errors => _errors;

        /// <summary>
        /// Gets the original locations.
        /// </summary>
        public Coordinate[] Locations => _locations;

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        public List<RouterPoint> RouterPoints => _routerPoints;

        /// <summary>
        /// Gets the router.
        /// </summary>
        public RouterBase Router => _router;

        /// <summary>
        /// Gets the profiles.
        /// </summary>
        public IProfileInstance[] Profiles => _profiles;

        /// <summary>
        /// Returns the index of the resolved point, given the original index of in the locations array.
        /// </summary>
        public int ResolvedIndexOf(int locationIdx)
        {
            return locationIdx;
        }

        /// <summary>
        /// Returns the index of the location in the original locations array, given the resolved point index..
        /// </summary>
        public int LocationIndexOf(int resolvedIdx)
        {
            return resolvedIdx;
        }

        /// <summary>
        /// Nothing to be done
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            
        }
    }
}
