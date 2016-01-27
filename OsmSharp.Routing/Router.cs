// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using OsmSharp.Routing.Algorithms.Default;
using OsmSharp.Routing.Algorithms.Routes;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Exceptions;
using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Network;
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

        /// <summary>
        /// Creates a new router.
        /// </summary>
        public Router(RouterDb db)
        {
            _db = db;

            this.VerifyAllStoppable = false;
        }

        /// <summary>
        /// Gets or sets the profile factor cache.
        /// </summary>
        public ProfileFactorCache ProfileFactorCache { get; set; }

        /// <summary>
        /// Gets or sets the delegate to create a custom resolver.
        /// </summary>
        public IResolveExtensions.CreateResolver CreateCustomResolver { get; set; }

        /// <summary>
        /// Gets or sets the delegate to use a custom route builder.
        /// </summary>
        public RouteBuilderExtensions.BuildRoute CustomRouteBuilder { get; set; }

        /// <summary>
        /// Flag to check all resolved points if stopping at the resolved location is possible.
        /// </summary>
        public bool VerifyAllStoppable { get; set; }

        /// <summary>
        /// Gets the db.
        /// </summary>
        public RouterDb Db
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Returns true if all profiles are supported.
        /// </summary>
        /// <returns></returns>
        public bool SupportsAll(params Profile[] profiles)
        {
            return _db.SupportsAll(profiles);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        /// <returns></returns>
        public Result<RouterPoint> TryResolve(Profile[] profiles, float latitude, float longitude, 
            Func<RoutingEdge, bool> isBetter, float maxSearchDistance = Constants.SearchDistanceInMeter)
        {
            if(!_db.SupportsAll(profiles))
            {
                return new Result<RouterPoint>("Not all routing profiles are supported.", (message) =>
                {
                    return new Exceptions.ResolveFailedException(message);
                });
            }

            IResolver resolver = null;
            if (this.CreateCustomResolver == null)
            { // just use the default resolver algorithm.
                Func<GeometricEdge, bool> isBetterGeometric = null;
                if(isBetter != null)
                { // take into account isBetter function.
                    isBetterGeometric = (edge) =>
                        {
                            return isBetter(_db.Network.GetEdge(edge.Id));
                        };
                }

                // get is acceptable.
                var isAcceptable = this.GetIsAcceptable(profiles);

                // create resolver.
                resolver = new ResolveAlgorithm(_db.Network.GeometricGraph, latitude, longitude, 
                    _db.Network.MaxEdgeDistance / 2,
                        maxSearchDistance, isAcceptable, isBetterGeometric);
            }
            else
            { // create the custom resolver algorithm.
                resolver = this.CreateCustomResolver(latitude, longitude, isBetter);
            }
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

            // get the get factor function.
            var getFactor = this.GetGetFactor(profile);
     
            // build and run dykstra search.
            var dykstra = new Dykstra(_db.Network.GeometricGraph.Graph, getFactor, 
                point.ToPaths(_db, getFactor, true), radiusInMeters, false);
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

            // get the get factor function.
            var getFactor = this.GetGetFactor(profile);

            List<uint> path;
            OsmSharp.Routing.Graphs.Directed.DirectedMetaGraph contracted;
            if(_db.TryGetContracted(profile, out contracted))
            { // contracted calculation.
                path = null;
                var bidirectionalSearch = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(contracted,
                    source.ToPaths(_db, getFactor, true), target.ToPaths(_db, getFactor, false));
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
                var sourceSearch = new Dykstra(_db.Network.GeometricGraph.Graph, getFactor, 
                    source.ToPaths(_db, getFactor, true), float.MaxValue, false);
                var targetSearch = new Dykstra(_db.Network.GeometricGraph.Graph, getFactor, 
                    target.ToPaths(_db, profile, false), float.MaxValue, true);

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

            return this.BuildRoute(profile, source, target, path);
        }

        /// <summary>
        /// Calculates the weight of the route between the two locations.
        /// </summary>
        /// <returns></returns>
        public Result<float> TryCalculateWeight(Profile profile, RouterPoint source, RouterPoint target)
        {
            if (!_db.Supports(profile))
            {
                return new Result<float>("Routing profile is not supported.", (message) =>
                {
                    return new Exception(message);
                });
            }

            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public Result<Route[][]> TryCalculate(Profile profile, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            if (!_db.Supports(profile))
            {
                return new Result<Route[][]>("Routing profile is not supported.", (message) =>
                {
                    return new Exception(message);
                });
            }

            // get the get factor function.
            var getFactor = this.GetGetFactor(profile);
            
            OsmSharp.Routing.Graphs.Directed.DirectedMetaGraph contracted;
            if (_db.TryGetContracted(profile, out contracted))
            {
                OsmSharp.Logging.Log.TraceEvent("Router", Logging.TraceEventType.Warning, 
                    "Many to many route calculations are not possible yet using contracted algorithms.");
            }

            // use non-contracted calculation.
            var algorithm = new OsmSharp.Routing.Algorithms.Default.ManyToMany(_db, getFactor, sources, targets, float.MaxValue);
            algorithm.Run();
            if (!algorithm.HasSucceeded)
            {
                return new Result<Route[][]>(algorithm.ErrorMessage, (message) =>
                {
                    return new RouteNotFoundException(message);
                });
            }

            // build all routes.
            var routes = new Route[sources.Length][];
            var path = new List<uint>();
            for(var s = 0; s < sources.Length; s++)
            {
                routes[s] = new Route[targets.Length];
                for (var t = 0; t < targets.Length; t++)
                {
                    var localPath = algorithm.GetPath(s, t);
                    if(localPath != null)
                    {
                        path.Clear();
                        localPath.AddToList(path);
                        var route = this.BuildRoute(profile, sources[s],
                            targets[t], path);
                        if(route.IsError)
                        {
                            return route.ConvertError<Route[][]>();
                        }
                        routes[s][t] = route.Value;
                    }
                }
            }
            return new Result<Route[][]>(routes);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public Result<float[][]> TryCalculateWeight(Profile profile, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            if (!_db.Supports(profile))
            {
                return new Result<float[][]>("Routing profile is not supported.", (message) =>
                {
                    return new Exception(message);
                });
            }

            // get the get factor function.
            var getFactor = this.GetGetFactor(profile);

            float[][] weights = null;
            OsmSharp.Routing.Graphs.Directed.DirectedMetaGraph contracted;
            if (_db.TryGetContracted(profile, out contracted))
            { // contracted calculation.
                var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(_db, profile,
                    sources, targets);
                algorithm.Run();
                if (!algorithm.HasSucceeded)
                {
                    return new Result<float[][]>(algorithm.ErrorMessage, (message) =>
                    {
                        return new RouteNotFoundException(message);
                    });
                }
                weights = algorithm.Weights;
            }
            else
            { // non-contracted calculation.
                var algorithm = new OsmSharp.Routing.Algorithms.Default.ManyToMany(_db, getFactor, sources, targets, float.MaxValue);
                algorithm.Run();
                if (!algorithm.HasSucceeded)
                {
                    return new Result<float[][]>(algorithm.ErrorMessage, (message) =>
                    {
                        return new RouteNotFoundException(message);
                    });
                }
                weights = algorithm.Weights;
            }

            // extract invalid targets.
            for(var s = 0; s < weights.Length; s++)
            {
                var invalid = true;
                for(var t = 0; t < weights[s].Length; t++)
                {
                    if(t != s)
                    {
                        if(weights[s][t] < float.MaxValue)
                        {
                            invalid = false;
                            break;
                        }
                    }
                }
                if (invalid)
                {
                    invalidSources.Add(s);
                }
            }

            // extract invalid targets.
            for (var t = 0; t < weights[0].Length; t++)
            {
                var invalid = true;
                for (var s = 0; s < weights.Length; s++)
                {
                    if (t != s)
                    {
                        if (weights[s][t] < float.MaxValue)
                        {
                            invalid = false;
                            break;
                        }
                    }
                }
                if (invalid)
                {
                    invalidTargets.Add(t);
                }
            }
            return new Result<float[][]>(weights);
        }

        /// <summary>
        /// Returns the IsAcceptable function to use in the default resolver algorithm.
        /// </summary>
        /// <param name="profiles"></param>
        /// <returns></returns>
        protected Func<GeometricEdge, bool> GetIsAcceptable(Profile[] profiles)
        {
            if (this.ProfileFactorCache != null && this.ProfileFactorCache.ContainsAll(profiles))
            { // use cached version and don't consult profiles anymore.
                return this.ProfileFactorCache.GetIsAcceptable(this.VerifyAllStoppable,
                    profiles);
            }
            else
            { // use the regular function, and consult profiles continuously.
                return (edge) =>
                { // check all profiles, they all need to be traversible.
                  // get profile.
                    float distance;
                    ushort edgeProfileId;
                    OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(edge.Data[0],
                        out distance, out edgeProfileId);
                    var edgeProfile = _db.EdgeProfiles.Get(edgeProfileId);
                    for (var i = 0; i < profiles.Length; i++)
                    {
                        // get factor from profile.
                        if (profiles[i].Factor(edgeProfile).Value <= 0)
                        { // cannot be traversed by this profile.
                            return false;
                        }
                        if (this.VerifyAllStoppable)
                        { // verify stoppable.
                            if (!profiles[i].CanStopOn(edgeProfile))
                            { // this profile cannot stop on this edge.
                                return false;
                            }
                        }
                    }
                    return true;
                };
            }
        }

        /// <summary>
        /// Gets the get factor function for the given profile.
        /// </summary>
        protected Func<ushort, Factor> GetGetFactor(Profile profile)
        {
            if (this.ProfileFactorCache != null && this.ProfileFactorCache.ContainsAll(profile))
            { // use cached version and don't consult profiles anymore.
                return this.ProfileFactorCache.GetGetFactor(profile);
            }
            else
            { // use the regular function, and consult profiles continuously.
                return (p) =>
                {
                    return profile.Factor(Db.EdgeProfiles.Get(p));
                };
            }
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        protected Result<Route> BuildRoute(Profile profile, RouterPoint source, RouterPoint target, List<uint> path)
        {
            if(this.CustomRouteBuilder != null)
            { // there is a custom route builder.
                return this.CustomRouteBuilder(_db, profile, this.GetGetFactor(profile), 
                    source, target, path);
            }

            // use the default.
            return CompleteRouteBuilder.TryBuild(_db, profile, source, target, path);
        }
    }
}