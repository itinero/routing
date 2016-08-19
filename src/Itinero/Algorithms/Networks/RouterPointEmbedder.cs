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
using Itinero.Profiles;
using System;

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
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            for (var i = 0; i < _locations.Length; i++)
            {
                var p = _resolver(_locations[i]);

                p.AddAsVertex(_routerDb);
            }
        }
    }
}