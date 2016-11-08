// Itinero - Routing for .NET
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
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Geometric;
using Itinero.Data.Edges;
using Itinero.Logging;

namespace Itinero
{
    /// <summary>
    /// Contains extension methods on top of the RouterBase abstract class.
    /// </summary>
    public static class RouterBaseExtensions
    {
        /// <summary>
        /// The default connectivity radius.
        /// </summary>
        public const float DefaultConnectivityRadius = 250;

        /// <summary>
        /// Returns true if all given profiles are supported.
        /// </summary>
        public static bool SupportsAll(this RouterBase router, params Profile[] profiles)
        {
            for (var i = 0; i < profiles.Length; i++)
            {
                if (!router.Db.Supports(profiles[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the default get factor for the given profile instance but used the cached version whenever available.
        /// </summary>
        public static Func<ushort, Factor> GetDefaultGetFactor(this RouterBase router, IProfileInstance profileInstance)
        {
            if (router.ProfileFactorAndSpeedCache != null && router.ProfileFactorAndSpeedCache.ContainsAll(profileInstance))
            { // use cached version and don't consult profiles anymore.
                return router.ProfileFactorAndSpeedCache.GetGetFactor(profileInstance);
            }
            else
            { // use the regular function, and consult profiles continuously.
                Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.Name);
                return profileInstance.GetGetFactor(router.Db);
            }
        }
        
        /// <summary>
        /// Gets the default weight handler for the given profile instance.
        /// </summary>
        public static DefaultWeightHandler GetDefaultWeightHandler(this RouterBase router, IProfileInstance profileInstance)
        {
            if (router.ProfileFactorAndSpeedCache != null && router.ProfileFactorAndSpeedCache.ContainsAll(profileInstance))
            { // use cached version and don't consult profiles anymore.
                return new DefaultWeightHandler(router.ProfileFactorAndSpeedCache.GetGetFactor(profileInstance));
            }
            else
            { // use the regular function, and consult the profile continuously.
                Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.Name);
                return new DefaultWeightHandler((p) =>
                {
                    return profileInstance.Factor(router.Db.EdgeProfiles.Get(p));
                });
            }
        }

        /// <summary>
        /// Gets the default weight handler for the given profile.
        /// </summary>
        public static WeightHandler GetAugmentedWeightHandler(this RouterBase router, IProfileInstance profileInstance)
        {
            if (router.ProfileFactorAndSpeedCache != null && router.ProfileFactorAndSpeedCache.ContainsAll(profileInstance))
            { // use cached version and don't consult profiles anymore.
                return new WeightHandler(router.ProfileFactorAndSpeedCache.GetGetFactorAndSpeed(profileInstance));
            }
            else
            { // use the regular function, and consult profiles continuously.
                Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.Name);
                return new WeightHandler(profileInstance.GetGetFactorAndSpeed(router.Db));
            }
        }

        /// <summary>
        /// Gets the augmented get factor for the given profile but used the cached version whenever available.
        /// </summary>
        public static Func<ushort, FactorAndSpeed> GetAugmentedGetFactor(this RouterBase router, IProfileInstance profileInstance)
        {
            if (router.ProfileFactorAndSpeedCache != null && router.ProfileFactorAndSpeedCache.ContainsAll(profileInstance))
            { // use cached version and don't consult profiles anymore.
                return router.ProfileFactorAndSpeedCache.GetGetFactorAndSpeed(profileInstance);
            }
            else
            { // use the regular function, and consult profiles continuously.
                Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.Name);
                return profileInstance.GetGetFactorAndSpeed(router.Db);
            }
        }

        /// <summary>
        /// Returns the IsAcceptable function to use in the default resolver algorithm.
        /// </summary>
        public static Func<GeometricEdge, bool> GetIsAcceptable(this RouterBase router, params IProfileInstance[] profiles)
        {
            if (router.ProfileFactorAndSpeedCache != null && router.ProfileFactorAndSpeedCache.ContainsAll(profiles))
            { // use cached version and don't consult profiles anymore.
                return router.ProfileFactorAndSpeedCache.GetIsAcceptable(router.VerifyAllStoppable,
                    profiles);
            }
            else
            { // use the regular function, and consult profiles continuously.
                Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, 
                    "Not all profiles are cached, this could slow down routing significantly, consider building a profile cache.");
                return (edge) =>
                { // check all profiles, they all need to be traversible.
                  // get profile.
                    float distance;
                    ushort edgeProfileId;
                    EdgeDataSerializer.Deserialize(edge.Data[0],
                        out distance, out edgeProfileId);
                    var edgeProfile = router.Db.EdgeProfiles.Get(edgeProfileId);
                    for (var i = 0; i < profiles.Length; i++)
                    {
                        var factorAndSpeed = profiles[i].Profile.FactorAndSpeed(edgeProfile);
                        // get factor from profile.
                        if (factorAndSpeed.Value <= 0)
                        { // cannot be traversed by this profile.
                            return false;
                        }
                        if (router.VerifyAllStoppable)
                        { // verify stoppable.
                            if (!factorAndSpeed.CanStopOn())
                            { // this profile cannot stop on this edge.
                                return false;
                            }
                        }
                        if (profiles[i].IsConstrained(factorAndSpeed.Constraints))
                        { // this edge is constrained, this vehicle cannot travel here.
                            return false;
                        }
                    }
                    return true;
                };
            }
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, Profile profile, float latitude, float longitude, 
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(new Profile[] { profile }, latitude, longitude, searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, Profile[] profiles, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, null, 
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, Profile profile, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(new Profile[] { profile }, latitude, longitude, searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, Profile[] profiles, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, Profile[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, isBetter, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this RouterBase router, Profile profile, RouterPoint point, float radiusInMeters)
        {
            return router.TryCheckConnectivity(profile, point, radiusInMeters).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this RouterBase router, Profile profile, RouterPoint point)
        {
            return router.CheckConnectivity(profile, point, DefaultConnectivityRadius);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, Profile profile, RouterPoint[] locations)
        {
            return router.TryCalculate(profile, locations).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, Profile profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude)
        {
            return router.TryCalculate(profile, sourceLatitude, sourceLongitude, targetLatitude, targetLongitude).Value;
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, Profile profile, RouterPoint[] locations)
        {
            if (locations.Length < 2)
            {
                throw new ArgumentOutOfRangeException("Cannot calculate a routing along less than two locations.");
            }
            var route = router.TryCalculate(profile, locations[0], locations[1]);
            if (route.IsError)
            {
                return route;
            }
            for (var i = 2; i < locations.Length; i++)
            {
                var nextRoute = router.TryCalculate(profile, locations[i - 1], locations[i]);
                if (nextRoute.IsError)
                {
                    return nextRoute;
                }
                route = new Result<Route>(route.Value.Concatenate(nextRoute.Value));
            }
            return route;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, Profile profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude)
        {
            var profiles = new Profile[] { profile };
            var sourcePoint = router.TryResolve(profiles, sourceLatitude, sourceLongitude);
            var targetPoint = router.TryResolve(profiles, targetLatitude, targetLongitude);

            if(sourcePoint.IsError)
            {
                return sourcePoint.ConvertError<Route>();
            }
            if (targetPoint.IsError)
            {
                return targetPoint.ConvertError<Route>();
            }
            return router.TryCalculate(profile, sourcePoint.Value, targetPoint.Value);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Result<Route[]> TryCalculate(this RouterBase router, Profile profile, RouterPoint source, RouterPoint[] targets)
        {
            var result = router.TryCalculate(profile, new RouterPoint[] { source }, targets);
            if(result.IsError)
            {
                return result.ConvertError<Route[]>();
            }

            var routes = new Route[result.Value.Length];
            for (var j = 0; j < result.Value.Length; j++)
            {
                routes[j] = result.Value[0][j];
            }
            return new Result<Route[]>(routes);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, Profile profile, RouterPoint source, RouterPoint target)
        {
            return router.TryCalculate(profile, source, target).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[] Calculate(this RouterBase router, Profile profile, RouterPoint source, RouterPoint[] targets)
        {
            return router.TryCalculate(profile, source, targets).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public static Result<Route[][]> TryCalculate(this RouterBase router, Profile profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var paths = router.TryCalculateRaw(profile, weightHandler, sources, targets);
            if (paths.IsError)
            {
                return paths.ConvertError<Route[][]>();
            }

            var routes = new Route[paths.Value.Length][];
            for (var s = 0; s < paths.Value.Length; s++)
            {
                routes[s] = new Route[paths.Value[s].Length];
                for (var t = 0; t < paths.Value[s].Length; t++)
                {
                    var localPath = paths.Value[s][t];
                    if (localPath != null)
                    {
                        var route = router.BuildRoute(profile, weightHandler, sources[s],
                            targets[t], localPath);
                        if (route.IsError)
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
        public static Route[][] Calculate(this RouterBase router, Profile profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            return router.TryCalculate(profile, sources, targets).Value;
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude) where T : struct
        {
            var profiles = new Profile[] { profile };
            var sourcePoint = router.TryResolve(profiles, sourceLatitude, sourceLongitude);
            var targetPoint = router.TryResolve(profiles, targetLatitude, targetLongitude);

            if (sourcePoint.IsError)
            {
                return sourcePoint.ConvertError<T>();
            }
            if (targetPoint.IsError)
            {
                return targetPoint.ConvertError<T>();
            }
            return router.TryCalculateWeight(profile, weightHandler, sourcePoint.Value, targetPoint.Value);
        }

        /// <summary>
        /// Tries to calculate the weight between the given source and target.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler,
            RouterPoint source, RouterPoint target) where T : struct
        {
            var result = router.TryCalculateRaw<T>(profile, weightHandler, source, target);
            if (result.IsError)
            {
                return result.ConvertError<T>();
            }
            return new Result<T>(result.Value.Weight);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler, RouterPoint[] locations)
            where T : struct
        {
            var invalids = new HashSet<int>();
            var result = router.TryCalculateWeight(profile, weightHandler, locations, locations, invalids, invalids);
            if (invalids.Count > 0)
            {
                return new Result<T[][]>("At least one location could not be routed from/to. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            return result;
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            ISet<int> invalids) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, locations, invalids, invalids);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static T[][] CalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            ISet<int> invalids) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, invalids).Value;
        }
        
        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static float[][] CalculateWeight(this RouterBase router, Profile profile, RouterPoint[] locations,
            ISet<int> invalids)
        {
            return router.TryCalculateWeight(profile, profile.DefaultWeightHandler(router), locations, invalids).Value;
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this RouterBase router, Profile profile, Coordinate[] coordinates,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            if (coordinates == null) { throw new ArgumentNullException("coordinate"); }

            var result = new Result<RouterPoint>[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                result[i] = router.TryResolve(profile, coordinates[i], searchDistanceInMeter);
            }
            return result;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, Profile profile, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(new Profile[] { profile }, coordinate, searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this RouterBase router, Profile[] profiles, Coordinate[] coordinates,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            if (coordinates == null) { throw new ArgumentNullException("coordinate"); }

            var result = new Result<RouterPoint>[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                result[i] = router.TryResolve(profiles, coordinates[i], searchDistanceInMeter);
            }
            return result;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, Profile[] profiles, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate.Latitude, coordinate.Longitude,
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, Profile[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate.Latitude, coordinate.Longitude, isBetter,
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, Profile profile, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profile, coordinate, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, Profile[] profiles, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint[] Resolve(this RouterBase router, Profile profile, Coordinate[] coordinates,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            var results = router.TryResolve(profile, coordinates, searchDistanceInMeter);
            var routerPoints = new RouterPoint[results.Length];
            for (var i = 0; i < results.Length; i++)
            {
                routerPoints[i] = results[i].Value;
            }
            return routerPoints;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, Profile[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter,
                float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, isBetter, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, Profile profile, Coordinate source, Coordinate target)
        {
            return router.TryCalculate(profile, source, target).Value;
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, Profile profile, Coordinate[] locations)
        {
            return router.TryCalculate(profile, locations).Value;
        }

        /// <summary>
        /// Calculates a route the given locations;
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, Profile profile, RouterPoint source, RouterPoint target)
        {
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var path = router.TryCalculateRaw(profile, weightHandler, source, target);
            if (path.IsError)
            {
                return path.ConvertError<Route>();
            }
            return router.BuildRoute(profile, weightHandler, source, target, path.Value);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, Profile profile, Coordinate source,
            Coordinate target)
        {
            return router.TryCalculate(profile, source.Latitude, source.Longitude, target.Latitude, target.Longitude);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, Profile profile, Coordinate[] locations)
        {
            if (locations.Length < 2)
            {
                throw new ArgumentOutOfRangeException("Cannot calculate a routing along less than two locations.");
            }
            var resolved = router.TryResolve(profile, locations);
            var route = router.TryCalculate(profile, resolved[0].Value, resolved[1].Value);
            if (route.IsError)
            {
                return route;
            }
            for (var i = 2; i < resolved.Length; i++)
            {
                var nextRoute = router.TryCalculate(profile, resolved[i - 1].Value, resolved[i].Value);
                if (nextRoute.IsError)
                {
                    return nextRoute;
                }
                route = new Result<Route>(route.Value.Concatenate(nextRoute.Value));
            }
            return route;
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<float> TryCalculateWeight(this RouterBase router, Profile profile, Coordinate source, Coordinate target)
        {
            return router.TryCalculateWeight(profile, profile.DefaultWeightHandler(router), source, target);
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler, Coordinate source, Coordinate target)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, source.Latitude, source.Longitude, target.Latitude, target.Longitude);
        }

        /// <summary>
        /// Calculates all weights between all given locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, WeightHandler<T> weightHandler, Profile profile, Coordinate[] locations)
            where T : struct
        {
            return router.TryCalculateWeight(profile,  weightHandler, locations, locations);
        }

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, Profile profile, WeightHandler<T> weightHandler, Coordinate[] sources, Coordinate[] targets)
            where T : struct
        {
            var resolvedSources = new RouterPoint[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                var result = router.TryResolve(profile, sources[i]);
                if (result.IsError)
                {
                    return new Result<T[][]>(string.Format("Source at index {0} could not be resolved: {1}",
                        i, result.ErrorMessage), (s) =>
                        {
                            throw new Exceptions.ResolveFailedException(s);
                        });
                }
                resolvedSources[i] = result.Value;
            }
            var resolvedTargets = new RouterPoint[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                var result = router.TryResolve(profile, targets[i]);
                if (result.IsError)
                {
                    return new Result<T[][]>(string.Format("Target at index {0} could not be resolved: {1}",
                        i, result.ErrorMessage), (s) =>
                        {
                            throw new Exceptions.ResolveFailedException(s);
                        });
                }
                resolvedTargets[i] = result.Value;
            }

            var invalidSources = new HashSet<int>();
            var invalidTargets = new HashSet<int>();
            var weights = router.TryCalculateWeight(profile, weightHandler, resolvedSources, resolvedTargets, invalidSources, invalidTargets);
            if (invalidSources.Count > 0)
            {
                return new Result<T[][]>("Some sources could not be routed from. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            if (invalidTargets.Count > 0)
            {
                return new Result<T[][]>("Some targets could not be routed to. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            return weights;
        }
    }
}