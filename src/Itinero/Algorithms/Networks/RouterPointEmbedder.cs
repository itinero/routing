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
using Itinero.LocalGeo;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

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
        protected override void DoRun()
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