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
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Networks
{
    /// <summary>
    /// An algorithm that embeds routerpoints permanently as vertices in the network.
    /// </summary>
    public class RouterPointEmbedder : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly Profile[] _profiles;
        private readonly Coordinate[] _locations;
        private readonly Func<Coordinate, RouterPoint> _resolver;
        private readonly float _searchDistanceInMeter;

        /// <summary>
        /// Creates a new router point embedder.
        /// </summary>
        public RouterPointEmbedder(RouterDb routerDb, Profile[] profiles, Coordinate[] locations, float searchDistanceInMeter = 200)
        {
            _routerDb = routerDb;
            _profiles = profiles;
            _locations = locations;
            _searchDistanceInMeter = searchDistanceInMeter;

            var router = new Router(_routerDb);
            router.VerifyAllStoppable = true;
            _resolver = (loc) =>
            {
                return router.Resolve(_profiles, loc, _searchDistanceInMeter);
            };

            if (routerDb.HasContracted)
            {
                throw new InvalidOperationException("Cannot embed router points in a databases that has contracted networks, vertex id's have to change.");
            }
            if (routerDb.HasRestrictions)
            {
                throw new InvalidOperationException("Cannot embed router points in a databases that has restrictions, vertex id's have to change.");
            }
            if (routerDb.HasShortcuts)
            {
                throw new InvalidOperationException("Cannot embed router points in a databases that has shortcut db's, vertex id's have to change.");
            }
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var allSuccess = false;

            var newVertices = new uint[_locations.Length];
            for(var i = 0; i < newVertices.Length; i++)
            {
                newVertices[i] = Constants.NO_VERTEX;
            }

            while (!allSuccess)
            {
                // pick the undone vertices.
                var resolved = new List<RouterPoint>();
                var resolvedIndexes = new List<int>();
                for (var i = 0; i < _locations.Length; i++)
                {
                    if (newVertices[i] == Constants.NO_VERTEX)
                    {
                        resolvedIndexes.Add(i);
                        resolved.Add(_resolver(_locations[i]));
                    }
                }

                // add as vertices and merge with the local new vertices array.
                allSuccess = true;
                var newVerticesBatch = _routerDb.AddAsVertices(resolved.ToArray());
                for(var i = 0; i < newVerticesBatch.Length; i++)
                {
                    if (newVerticesBatch[i] != Constants.NO_VERTEX)
                    {
                        newVertices[resolvedIndexes[i]] = newVerticesBatch[i];
                    }
                    else
                    {
                        allSuccess = false;
                    }
                }

                // sort the vertices again.
                _routerDb.Network.Sort((v1, v2) =>
                {
                    for (var i = 0; i < newVertices.Length; i++)
                    {
                        if (newVertices[i] == v1)
                        {
                            newVertices[i] = (uint)v2;
                        }
                        else if (newVertices[i] == v2)
                        {
                            newVertices[i] = (uint)v1;
                        }
                    }
                });
            }
        }
    }
}