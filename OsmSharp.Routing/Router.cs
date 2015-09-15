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

using OsmSharp.Math.Geo;
using OsmSharp.Routing.Profiles;
using OsmSharp.Routing.Algorithms.Search;
using System;
using OsmSharp.Routing.Algorithms.Routing;

namespace OsmSharp.Routing
{
    /// <summary>
    /// A router implementation encapsulating basic routing functionalities.
    /// </summary>
    public class Router : IRouter
    {
        private readonly RouterDb _db;
        private readonly float _defaultSearchOffset = .01f;
        private readonly float _defaultSearchMaxDistance = 50;

        /// <summary>
        /// Creates a new router.
        /// </summary>
        public Router(RouterDb db)
        {
            _db = db;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        /// <returns></returns>
        public Result<RouterPoint> TryResolve(Profile[] profiles, float latitude, float longitude)
        {
            var resolver = new ResolveAlgorithm(_db.Network.GeometricGraph, latitude, longitude, _defaultSearchOffset,
                _defaultSearchMaxDistance, (edge) =>
                { // check all profiles, they all need to be traversible.
                    // get profile.
                    float distance;
                    ushort profile;
                    OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(edge.Data[0],
                        out distance, out profile);
                    for(var i = 0; i < profiles.Length; i++)
                    {
                        // get speed from profile.
                        if (profiles[i].Speed(_db.Profiles.Get(profile)) <= 0)
                        { // cannot be travelled by this profile.
                            return false;
                        }
                    }
                    return true;
                });
            if(!resolver.HasSucceeded)
            { // something went wrong.
                return new Result<RouterPoint>(resolver.ErrorMessage, (message) =>
                {
                    return new Exceptions.ResolveFailedException(message);
                });
            }
            return new Result<RouterPoint>(resolver.Result);
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        /// <returns></returns>
        public bool CheckConnectivity(Profile profile, RouterPoint point, float radiusInMeters)
        {
            var dykstra = new OneToAllDykstra(_db.Network.GeometricGraph.Graph, (p) =>
            {
                var speed = profile.Speed(_db.Profiles.Get(p));
                if (speed > 0)
                {
                    return new Speed() { 
                        Direction = null,
                        MeterPerSecond = 1
                    };
                }
                return Speed.NoSpeed;
            }, point.ToPaths(_db, profile), radiusInMeters, false);
            dykstra.Run();
            if (!dykstra.HasSucceeded)
            { // something went wrong.
                return false;
            }
            return dykstra.MaxReached;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public Result<Route> TryCalculate(Profile profile, RouterPoint source, RouterPoint target)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public Result<Route>[][] TryCalculate(Profile profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            throw new NotImplementedException();
        }
    }
}