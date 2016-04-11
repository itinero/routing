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

using Itinero.Algorithms.Default;
using Itinero.Algorithms.Routes;
using Itinero.Algorithms.Search;
using Itinero.Exceptions;
using Itinero.Graphs.Geometric;
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.Data.Contracted;
using Itinero.Data.Edges;

namespace Itinero
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
            var dykstra = new Dykstra(_db.Network.GeometricGraph.Graph, getFactor, null,
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
            float pathWeight;
            ContractedDb contracted;
            if(_db.TryGetContracted(profile, out contracted))
            { // contracted calculation.
                if (!contracted.HasNodeBasedGraph)
                {
                    throw new NotSupportedException("Edge-based routing not yet supported.");
                }
                if (_db.HasComplexRestrictions(profile))
                {
                    throw new NotSupportedException("Edge-based routing not yet supported.");
                }

                path = null;
                var bidirectionalSearch = new Itinero.Algorithms.Contracted.BidirectionalDykstra(contracted.NodeBasedGraph,
                    source.ToPaths(_db, getFactor, true), target.ToPaths(_db, getFactor, false));
                bidirectionalSearch.Run();
                if (!bidirectionalSearch.HasSucceeded)
                {
                    return new Result<Route>(bidirectionalSearch.ErrorMessage, (message) =>
                    {
                        return new RouteNotFoundException(message);
                    });
                }
                path = bidirectionalSearch.GetPath(out pathWeight);
            }
            else
            { // non-contracted calculation.
                if (_db.HasComplexRestrictions(profile))
                {
                    var sourceSearch = new Algorithms.Default.Edge.Dykstra(_db.Network.GeometricGraph.Graph, getFactor, null,
                        source.ToEdgePaths(_db, getFactor, true), float.MaxValue, false);
                }
                else
                {
                    var sourceSearch = new Dykstra(_db.Network.GeometricGraph.Graph, getFactor, null,
                        source.ToPaths(_db, getFactor, true), float.MaxValue, false);
                    var targetSearch = new Dykstra(_db.Network.GeometricGraph.Graph, getFactor, null,
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
                    path = bidirectionalSearch.GetPath(out pathWeight);
                }
            }

            if(source.EdgeId == target.EdgeId)
            { // check for a shorter path on the same edge.
                var edgePath = source.PathTo(_db, profile, target);
                if(edgePath != null &&
                   edgePath.Weight < pathWeight)
                {
                    path = new List<uint>(2);
                    while(edgePath != null)
                    {
                        path.Insert(0, edgePath.Vertex);
                        edgePath = edgePath.From;
                    }
                }
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
            
            ContractedDb contracted;
            if (_db.TryGetContracted(profile, out contracted))
            {
                Logging.Logger.Log("Router", Logging.TraceEventType.Warning, 
                    "Many to many route calculations are not possible yet using contracted algorithms.");
            }

            // use non-contracted calculation.
            var algorithm = new Itinero.Algorithms.Default.ManyToMany(_db, getFactor, sources, targets, float.MaxValue);
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
            ContractedDb contracted;
            if (_db.TryGetContracted(profile, out contracted))
            { // contracted calculation.
                var algorithm = new Itinero.Algorithms.Contracted.ManyToManyBidirectionalDykstra(_db, profile,
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
                var algorithm = new Itinero.Algorithms.Default.ManyToMany(_db, getFactor, sources, targets, float.MaxValue);
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

            // check for invalids.
            var invalidTargetCounts = new int[targets.Length];
            for (var s = 0; s < weights.Length; s++)
            {
                var invalids = 0;
                for (var t = 0; t < weights[s].Length; t++)
                {
                    if (t != s)
                    {
                        if (weights[s][t] == float.MaxValue)
                        {
                            invalids++;
                            invalidTargetCounts[t]++;
                            if (invalidTargetCounts[t] > (sources.Length - 1) / 2)
                            {
                                invalidTargets.Add(t);
                            }
                        }
                    }
                }

                if (invalids > (targets.Length - 1) / 2)
                {
                    invalidSources.Add(s);
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
                    EdgeDataSerializer.Deserialize(edge.Data[0],
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