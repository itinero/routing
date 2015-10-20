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

using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Exceptions;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

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
            if(!_db.SupportsAll(profiles))
            {
                return new Result<RouterPoint>("Not all routing profiles are supported.", (message) =>
                {
                    return new Exceptions.ResolveFailedException(message);
                });
            }

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
                        if (profiles[i].Factor(_db.Profiles.Get(profile)).Value <= 0)
                        { // cannot be travelled by this profile.
                            return false;
                        }
                    }
                    return true;
                });
            resolver.Run();
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
        public Result<bool> TryCheckConnectivity(Profile profile, RouterPoint point, float radiusInMeters)
        {
            if (!_db.Supports(profile))
            {
                return new Result<bool>("Routing profile is not supported.", (message) =>
                {
                    return new Exception(message);
                });
            }

            var dykstra = new Dykstra(_db.Network.GeometricGraph.Graph, (p) =>
            {
                return profile.Factor(_db.Profiles.Get(p));
            }, point.ToPaths(_db, profile), radiusInMeters, false);
            dykstra.Run();
            if (!dykstra.HasSucceeded)
            { // something went wrong.
                return new Result<bool>(false);
            }
            return new Result<bool>(dykstra.MaxReached);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public Result<Route> TryCalculate(Profile profile, RouterPoint source, RouterPoint target)
        {
            if (!_db.Supports(profile))
            {
                return new Result<Route>("Routing profile is not supported.", (message) =>
                {
                    return new Exception(message);
                });
            }

            List<uint> path;
            OsmSharp.Routing.Graphs.Directed.DirectedGraph contracted;
            if(_db.TryGetContracted(profile, out contracted))
            { // contracted calculation.
                path = null;
                var bidirectionalSearch = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(contracted,
                    source.ToPaths(_db, profile), target.ToPaths(_db, profile));
                bidirectionalSearch.Run();
                if (!bidirectionalSearch.HasSucceeded)
                {
                    return new Result<Route>(bidirectionalSearch.ErrorMessage, (message) =>
                    {
                        return new RouteNotFoundException(message);
                    });
                }
                path = bidirectionalSearch.GetPath();
            }
            else
            { // non-contracted calculation.
                var sourceSearch = new Dykstra(_db.Network.GeometricGraph.Graph, (p) =>
                {
                    return profile.Factor(_db.Profiles.Get(p));
                }, source.ToPaths(_db, profile), float.MaxValue, false);
                var targetSearch = new Dykstra(_db.Network.GeometricGraph.Graph, (p) =>
                {
                    return profile.Factor(_db.Profiles.Get(p));
                }, target.ToPaths(_db, profile), float.MaxValue, true);

                var bidirectionalSearch = new BidirectionalDykstra(sourceSearch, targetSearch);
                bidirectionalSearch.Run();
                if (!bidirectionalSearch.HasSucceeded)
                {
                    return new Result<Route>(bidirectionalSearch.ErrorMessage, (message) =>
                    {
                        return new RouteNotFoundException(message);
                    });
                }
                path = bidirectionalSearch.GetPath();
            }
            
            // generate route.
            var routeBuilder = new RouteBuilder(_db, profile, source, target, path);
            routeBuilder.Run();
            if(!routeBuilder.HasSucceeded)
            {
                return new Result<Route>(routeBuilder.ErrorMessage, (message) =>
                {
                    return new RouteBuildFailedException(message);
                });
            }
            return new Result<Route>(routeBuilder.Route);
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