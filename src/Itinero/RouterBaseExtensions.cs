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
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Geometric;
using Itinero.Data.Edges;
using Itinero.Logging;
using System.Text;
using Itinero.Algorithms;
using Itinero.Data.Contracted;
using System.Threading;
using Itinero.Algorithms.Default.EdgeBased;
using Itinero.Algorithms.Search;

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
        public static bool SupportsAll(this RouterBase router, params IProfileInstance[] profiles)
        {
            for (var i = 0; i < profiles.Length; i++)
            {
                if (!router.Db.Supports(profiles[i].Profile))
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
                if (router.ProfileFactorAndSpeedCache != null)
                { // when there is a cache, built it on demand.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Information, "Profile {0} is not cached, building cache.",
                        profileInstance.Profile.FullName);
                    router.ProfileFactorAndSpeedCache.CalculateFor(profileInstance.Profile);
                    return router.ProfileFactorAndSpeedCache.GetGetFactor(profileInstance);
                }
                else
                { // no cache, caching disabled.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                        profileInstance.Profile.FullName);
                    return profileInstance.GetGetFactor(router.Db);
                }
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
                if (router.ProfileFactorAndSpeedCache != null)
                { // when there is a cache, built it on demand.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Information, "Profile {0} is not cached, building cache.",
                        profileInstance.Profile.FullName);
                    router.ProfileFactorAndSpeedCache.CalculateFor(profileInstance.Profile);
                    return new DefaultWeightHandler(router.ProfileFactorAndSpeedCache.GetGetFactor(profileInstance));
                }
                else
                { // no cache, caching disabled.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.FullName);
                    return new DefaultWeightHandler((p) =>
                    {
                        return profileInstance.Factor(router.Db.EdgeProfiles.Get(p));
                    });
                }
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
                if (router.ProfileFactorAndSpeedCache != null)
                { // when there is a cache, built it on demand.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Information, "Profile {0} is not cached, building cache.",
                        profileInstance.Profile.FullName);
                    router.ProfileFactorAndSpeedCache.CalculateFor(profileInstance.Profile);
                    return new WeightHandler(router.ProfileFactorAndSpeedCache.GetGetFactorAndSpeed(profileInstance));
                }
                else
                { // no cache, caching disabled.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.FullName);
                    return new WeightHandler(profileInstance.GetGetFactorAndSpeed(router.Db));
                }
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
                if (router.ProfileFactorAndSpeedCache != null)
                { // when there is a cache, built it on demand.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Information, "Profile {0} is not cached, building cache.",
                        profileInstance.Profile.FullName);
                    router.ProfileFactorAndSpeedCache.CalculateFor(profileInstance.Profile);
                    return router.ProfileFactorAndSpeedCache.GetGetFactorAndSpeed(profileInstance);
                }
                else
                { // no cache, caching disabled.
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Warning, "Profile {0} is not cached, this could slow down routing significantly, consider building a profile cache.",
                    profileInstance.Profile.FullName);
                    return profileInstance.GetGetFactorAndSpeed(router.Db);
                }
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
                if (router.ProfileFactorAndSpeedCache != null)
                { // when there is a cache, built it on demand.
                    var profileNames = new StringBuilder();
                    var profileArray = new Profile[profiles.Length];
                    for(var i = 0; i < profiles.Length; i++)
                    {
                        if (i > 0)
                        {
                            profileNames.Append(',');
                        }
                        profileNames.Append(profiles[i].Profile.FullName);
                        profileArray[i] = profiles[i].Profile;
                    }
                    Itinero.Logging.Logger.Log("RouterBaseExtensions", TraceEventType.Information, "Profile(s) {0} not cached, building cache.",
                        profileNames.ToInvariantString());
                    router.ProfileFactorAndSpeedCache.CalculateFor(profileArray);
                    return router.ProfileFactorAndSpeedCache.GetIsAcceptable(router.VerifyAllStoppable,
                        profiles);
                }
                else
                {
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
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance profile, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profile, latitude, longitude, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance profile, float latitude, float longitude, 
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(new IProfileInstance[] { profile }, latitude, longitude, searchDistanceInMeter, cancellationToken);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance[] profiles, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance[] profiles, float latitude, float longitude,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, latitude, longitude, null,
                searchDistanceInMeter, null, cancellationToken);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance profile, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profile, latitude, longitude, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance profile, float latitude, float longitude,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.Resolve(new IProfileInstance[] { profile }, latitude, longitude, searchDistanceInMeter, cancellationToken);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profiles, latitude, longitude, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, float latitude, float longitude,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, latitude, longitude, searchDistanceInMeter, cancellationToken).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profiles, latitude, longitude, isBetter, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, latitude, longitude, isBetter, searchDistanceInMeter, null, cancellationToken).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this RouterBase router, IProfileInstance profile, RouterPoint point, float radiusInMeters)
        {
            return router.CheckConnectivity(profile, point, radiusInMeters, CancellationToken.None);
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this RouterBase router, IProfileInstance profile, RouterPoint point, float radiusInMeters, CancellationToken cancellationToken)
        {
            return router.TryCheckConnectivity(profile, point, radiusInMeters, null, cancellationToken).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this RouterBase router, IProfileInstance profile, RouterPoint point)
        {
            return router.CheckConnectivity(profile, point, CancellationToken.None);
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this RouterBase router, IProfileInstance profile, RouterPoint point, CancellationToken cancellationToken)
        {
            return router.CheckConnectivity(profile, point, DefaultConnectivityRadius, cancellationToken);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, RouterPoint[] locations)
        {
            return router.Calculate(profile, locations, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, RouterPoint[] locations, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, locations, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude)
        {
            return router.Calculate(profile, sourceLatitude, sourceLongitude, targetLatitude, targetLongitude, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude,
            CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, sourceLatitude, sourceLongitude, targetLatitude, targetLongitude, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint[] locations)
        {
            return router.TryCalculate(profile, locations, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint[] locations, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, locations, 0, (Tuple<bool?, bool?>[])null, cancellationToken);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude)
        {
            return router.TryCalculate(profile, sourceLatitude, sourceLongitude, targetLatitude, targetLongitude, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude, CancellationToken cancellationToken)
        {
            var profiles = new IProfileInstance[] { profile };
            var sourcePoint = router.TryResolve(profiles, sourceLatitude, sourceLongitude, 50, cancellationToken);
            var targetPoint = router.TryResolve(profiles, targetLatitude, targetLongitude, 50, cancellationToken);

            if(sourcePoint.IsError)
            {
                return sourcePoint.ConvertError<Route>();
            }
            if (targetPoint.IsError)
            {
                return targetPoint.ConvertError<Route>();
            }
            return router.TryCalculate(profile, sourcePoint.Value, targetPoint.Value, cancellationToken);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Result<Route[]> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint[] targets)
        {
            return router.TryCalculate(profile, source, targets, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Result<Route[]> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint[] targets, CancellationToken cancellationToken)
        {
            var result = router.TryCalculate(profile, new RouterPoint[] { source }, targets, cancellationToken);
            if(result.IsError)
            {
                return result.ConvertError<Route[]>();
            }

            if (result.Value.Length == 0)
            {
                return new Result<Route[]>("No routes found.");
            }
            
            var routes = new Route[result.Value[0].Length];
            for (var j = 0; j < result.Value[0].Length; j++)
            {
                routes[j] = result.Value[0][j];
            }
            return new Result<Route[]>(routes);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint target)
        {
            return router.Calculate(profile, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint target, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, source, target, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[] Calculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint[] targets)
        {
            return router.Calculate(profile, source, targets, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[] Calculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint[] targets, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, source, targets, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public static Result<Route[][]> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            return router.TryCalculate(profile, sources, targets, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public static Result<Route[][]> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint[] sources, RouterPoint[] targets, CancellationToken cancellationToken)
        {
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var paths = router.TryCalculateRaw(profile, weightHandler, sources, targets, null, cancellationToken);
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
        public static Route[][] Calculate(this RouterBase router, IProfileInstance profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            return router.Calculate(profile, sources, targets, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[][] Calculate(this RouterBase router, IProfileInstance profile, RouterPoint[] sources, RouterPoint[] targets, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, sources, targets, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, sourceLatitude, sourceLongitude, targetLatitude, targetLongitude, CancellationToken.None);
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude, CancellationToken cancellationToken) where T : struct
        {
            var profiles = new IProfileInstance[] { profile };
            var sourcePoint = router.TryResolve(profiles, sourceLatitude, sourceLongitude, 50, cancellationToken);
            var targetPoint = router.TryResolve(profiles, targetLatitude, targetLongitude, 50, cancellationToken);

            if (sourcePoint.IsError)
            {
                return sourcePoint.ConvertError<T>();
            }
            if (targetPoint.IsError)
            {
                return targetPoint.ConvertError<T>();
            }
            return router.TryCalculateWeight(profile, weightHandler, sourcePoint.Value, targetPoint.Value, cancellationToken);
        }

        /// <summary>
        /// Tries to calculate the weight between the given source and target.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler,
            RouterPoint source, RouterPoint target) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Tries to calculate the weight between the given source and target.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler,
            RouterPoint source, RouterPoint target, CancellationToken cancellationToken) where T : struct
        {
            var result = router.TryCalculateRaw<T>(profile, weightHandler, source, target, null, cancellationToken);
            if (result.IsError)
            {
                return result.ConvertError<T>();
            }
            return new Result<T>(result.Value.Weight);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] locations)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            CancellationToken cancellationToken)
            where T : struct
        {
            var invalids = new HashSet<int>();
            var result = router.TryCalculateWeight(profile, weightHandler, locations, locations, invalids, invalids, null, cancellationToken);
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
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            ISet<int> invalids) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, invalids, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            ISet<int> invalids, CancellationToken cancellationToken) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, locations, invalids, invalids, null, cancellationToken);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static T[][] CalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            ISet<int> invalids) where T : struct
        {
            return router.CalculateWeight(profile, weightHandler, locations, invalids, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static T[][] CalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] locations,
            ISet<int> invalids, CancellationToken cancellationToken) where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, invalids, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static float[][] CalculateWeight(this RouterBase router, IProfileInstance profile, RouterPoint[] locations,
            ISet<int> invalids)
        {
            return router.CalculateWeight(profile, locations, invalids, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static float[][] CalculateWeight(this RouterBase router, IProfileInstance profile, RouterPoint[] locations,
            ISet<int> invalids, CancellationToken cancellationToken)
        {
            return router.TryCalculateWeight(profile, profile.DefaultWeightHandler(router), locations, invalids, cancellationToken).Value;
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this RouterBase router, IProfileInstance profile, Coordinate[] coordinates,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profile, coordinates, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this RouterBase router, IProfileInstance profile, Coordinate[] coordinates,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            if (coordinates == null) { throw new ArgumentNullException("coordinate"); }

            var result = new Result<RouterPoint>[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                result[i] = router.TryResolve(profile, coordinates[i], searchDistanceInMeter, cancellationToken);
            }
            return result;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance profile, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profile, coordinate, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance profile, Coordinate coordinate,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(new IProfileInstance[] { profile }, coordinate, searchDistanceInMeter, cancellationToken);
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this RouterBase router, IProfileInstance[] profiles, Coordinate[] coordinates,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinates, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this RouterBase router, IProfileInstance[] profiles, Coordinate[] coordinates,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            if (coordinates == null) { throw new ArgumentNullException("coordinate"); }

            var result = new Result<RouterPoint>[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                result[i] = router.TryResolve(profiles, coordinates[i], searchDistanceInMeter, cancellationToken);
            }
            return result;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, coordinate.Latitude, coordinate.Longitude,
                searchDistanceInMeter, cancellationToken);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, isBetter, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, coordinate.Latitude, coordinate.Longitude, isBetter,
                searchDistanceInMeter, null, cancellationToken);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance profile, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profile, coordinate, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance profile, Coordinate coordinate,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profile, coordinate, searchDistanceInMeter, cancellationToken).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profiles, coordinate, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, coordinate, searchDistanceInMeter, cancellationToken).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint[] Resolve(this RouterBase router, IProfileInstance profile, Coordinate[] coordinates,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profile, coordinates, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint[] Resolve(this RouterBase router, IProfileInstance profile, Coordinate[] coordinates,
            float searchDistanceInMeter, CancellationToken cancellationToken)
        {
            var results = router.TryResolve(profile, coordinates, searchDistanceInMeter, cancellationToken);
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
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(profiles, coordinate, isBetter, searchDistanceInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this RouterBase router, IProfileInstance[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter,
            CancellationToken cancellationToken)
        {
            return router.TryResolve(profiles, coordinate, isBetter, searchDistanceInMeter, cancellationToken).Value;
        }

        /// <summary>
        /// Resolves a location but also checks if it's connected to the rest of the network.
        /// </summary>
        public static Result<RouterPoint> TryResolveConnected(this RouterBase router, IProfileInstance profileInstance, Coordinate location,
            float radiusInMeter = 2000, float maxSearchDistance = Constants.SearchDistanceInMeter, bool? forward = null)
        {
            return router.TryResolveConnected(profileInstance, location, radiusInMeter, maxSearchDistance, forward, CancellationToken.None);
        }

        /// <summary>
        /// Resolves a location but also checks if it's connected to the rest of the network.
        /// </summary>
        public static Result<RouterPoint> TryResolveConnected(this RouterBase router, IProfileInstance profileInstance, Coordinate location,
            float radiusInMeter, float maxSearchDistance, bool? forward,
            CancellationToken cancellationToken)
        {
            return router.TryResolveConnected(profileInstance, location.Latitude, location.Longitude, radiusInMeter, maxSearchDistance,
                forward, cancellationToken);
        }

        /// <summary>
        /// Resolves a location but also checks if it's connected to the rest of the network.
        /// </summary>
        public static Result<RouterPoint> TryResolveConnected(this RouterBase router, IProfileInstance profileInstance, float latitude, float longitude,
            float radiusInMeter = 2000, float maxSearchDistance = Constants.SearchDistanceInMeter, bool? forward = null)
        {
            return router.TryResolveConnected(profileInstance, latitude, longitude, radiusInMeter, maxSearchDistance, forward, CancellationToken.None);
        }

        /// <summary>
        /// Resolves a location but also checks if it's connected to the rest of the network.
        /// </summary>
        public static Result<RouterPoint> TryResolveConnected(this RouterBase router, IProfileInstance profileInstance, float latitude, float longitude, 
            float radiusInMeter, float maxSearchDistance, bool? forward,
            CancellationToken cancellationToken)
        {
            var isAcceptable = router.GetIsAcceptable(profileInstance);
            var resolver = new Algorithms.Search.ResolveAlgorithm(router.Db.Network.GeometricGraph, latitude, longitude, radiusInMeter, maxSearchDistance, (edge) =>
            {
                // check if the edge is acceptable for the profile.
                if (!isAcceptable(edge))
                {
                    return false;
                }

                // create a temp resolved point in the middle of this edge.
                var tempRouterPoint = new RouterPoint(0, 0, edge.Id, ushort.MaxValue / 2);
                var connectivityResult = router.TryCheckConnectivity(profileInstance, tempRouterPoint, radiusInMeter, forward, cancellationToken);
                if (connectivityResult.IsError)
                { // if there is an error checking connectivity, choose not report it, just don't choose this point.
                    return false;
                }
                return connectivityResult.Value;
            });
            resolver.Run(cancellationToken);

            if (!resolver.HasSucceeded)
            { // something went wrong.
                return new Result<RouterPoint>(resolver.ErrorMessage, (message) =>
                {
                    return new Itinero.Exceptions.ResolveFailedException(message);
                });
            }
            return new Result<RouterPoint>(resolver.Result);
        }

        /// <summary>
        /// Resolves a location but also checks if it's connected to the rest of the network.
        /// </summary>
        public static RouterPoint ResolveConnected(this RouterBase router, IProfileInstance profileInstance, float latitude, float longitude, float radiusInMeter = 1000,
            float maxSearchDistance = Constants.SearchDistanceInMeter)
        {
            return router.ResolveConnected(profileInstance, latitude, longitude, radiusInMeter, maxSearchDistance, CancellationToken.None);
        }

        /// <summary>
        /// Resolves a location but also checks if it's connected to the rest of the network.
        /// </summary>
        public static RouterPoint ResolveConnected(this RouterBase router, IProfileInstance profileInstance, float latitude, float longitude, float radiusInMeter,
            float maxSearchDistance, CancellationToken cancellationToken)
        {
            return router.TryResolveConnected(profileInstance, latitude, longitude, radiusInMeter, maxSearchDistance, null, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, Coordinate source, Coordinate target)
        {
            return router.Calculate(profile, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, Coordinate source, Coordinate target, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, source, target, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, Coordinate[] locations)
        {
            return router.Calculate(profile, locations, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, Coordinate[] locations, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, locations, cancellationToken).Value;
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="resolvingAlgorithm">The resolving algorithm containing the location to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredTurns">The perferred turns. First value for each point is the arrival direction, second value the departure direction.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, IMassResolvingAlgorithm resolvingAlgorithm, float turnPenalty = 0,
            Tuple<bool?, bool?>[] preferredTurns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, resolvingAlgorithm, turnPenalty, preferredTurns, cancellationToken).Value;
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="resolvingAlgorithm">The resolving algorithm containing the location to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredDirections">The perferred directions in degrees.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Route Calculate(this RouterBase router, IProfileInstance profile, IMassResolvingAlgorithm resolvingAlgorithm, float turnPenalty = 0,
            float?[] preferredDirections = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, resolvingAlgorithm, turnPenalty, preferredDirections, cancellationToken).Value;
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredTurns">The perferred turns. First value for each point is the arrival direction, second value the departure direction.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Route Calculate(this RouterBase router, IProfileInstance profile,
            RouterPoint[] locations, float turnPenalty = 0, Tuple<bool?, bool?>[] preferredTurns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, locations, turnPenalty, preferredTurns, cancellationToken).Value;
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredDirections">The perferred directions in degrees.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Route Calculate(this RouterBase router, IProfileInstance profile,
            RouterPoint[] locations, float turnPenalty = 0, float?[] preferredDirections = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, locations, turnPenalty, preferredDirections, cancellationToken).Value;
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredTurns">The perferred turns. First value for each point is the arrival direction, second value the departure direction.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Route Calculate(this RouterBase router, IProfileInstance profile,
            Coordinate[] locations, float turnPenalty = 0, Tuple<bool?, bool?>[] preferredTurns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, locations, turnPenalty, preferredTurns, cancellationToken).Value;
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredDirections">The perferred directions in degrees.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Route Calculate(this RouterBase router, IProfileInstance profile,
            Coordinate[] locations, float turnPenalty = 0, float?[] preferredDirections = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, locations, turnPenalty, preferredDirections, cancellationToken).Value;
        }

        /// <summary>
        /// Calculates a route the given locations;
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint target)
        {
            return router.TryCalculate(profile, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route the given locations;
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, RouterPoint source, RouterPoint target, CancellationToken cancellationToken)
        {
            var weightHandler = router.GetDefaultWeightHandler(profile);
            return router.TryCalculate<float>(profile, weightHandler, source, target, null, cancellationToken);
        }

        /// <summary>
        /// Calculates a route the given locations;
        /// </summary>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler,
            RouterPoint source, RouterPoint target, RoutingSettings<T> settings = null)
            where T : struct
        {
            return router.TryCalculate(profile, weightHandler, source, target, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route the given locations;
        /// </summary>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, 
            RouterPoint source, RouterPoint target, RoutingSettings<T> settings, CancellationToken cancellationToken) 
            where T : struct
        {
            var path = router.TryCalculateRaw(profile, weightHandler, source, target, settings, cancellationToken);
            if (path.IsError)
            {
                return path.ConvertError<Route>();
            }
            return router.BuildRoute(profile, weightHandler, source, target, path.Value);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, Coordinate source,
            Coordinate target)
        {
            return router.TryCalculate(profile, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, Coordinate source,
            Coordinate target, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, source.Latitude, source.Longitude, target.Latitude, target.Longitude, cancellationToken);
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredTurns">The perferred turns. First value for each point is the arrival direction, second value the departure direction.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile,
            RouterPoint[] locations, float turnPenalty, Tuple<bool?, bool?>[] preferredTurns = null, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (locations.Length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(locations), 
                    "Cannot calculate a routing along less than two locations.");
            }
            
            var massResolvingAlgorith = new MassResolvingAlgorithm(router, new[] {profile}, locations);
            return router.TryCalculate(profile, massResolvingAlgorith, turnPenalty, preferredTurns, cancellationToken);
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredDirections">The perferred directions in degrees.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile,
            RouterPoint[] locations, float turnPenalty, float?[] preferredDirections, 
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (locations.Length < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(locations), 
                    "Cannot calculate a routing along less than two locations.");
            }
            
            var massResolvingAlgorith = new MassResolvingAlgorithm(router, new[] {profile}, locations);
            return router.TryCalculate(profile, massResolvingAlgorith, turnPenalty, preferredDirections, cancellationToken);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, Coordinate[] locations, CancellationToken cancellationToken)
        {
            return router.TryCalculate(profile, locations, 0, (Tuple<bool?, bool?>[])null, cancellationToken);
        }
        
        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredTurns">The perferred turns. First value for each point is the arrival direction, second value the departure direction.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, Coordinate[] locations, float turnPenalty = 0, 
            Tuple<bool?, bool?>[] preferredTurns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, new MassResolvingAlgorithm(router, new[] { profile }, locations), turnPenalty, preferredTurns, cancellationToken);
        }
        
        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="locations">The locations to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredDirections">The perferred directions in degrees.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile, Coordinate[] locations, float turnPenalty = 0, 
            float?[] preferredDirections = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return router.TryCalculate(profile, new MassResolvingAlgorithm(router, new[] { profile }, locations), turnPenalty, preferredDirections, cancellationToken);
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="resolvingAlgorithm">The resolving algorithm containing the location to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredTurns">The perferred turns. First value for each point is the arrival direction, second value the departure direction.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile,
            IMassResolvingAlgorithm resolvingAlgorithm, float turnPenalty = 0, Tuple<bool?, bool?>[] preferredTurns = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!resolvingAlgorithm.HasRun)
            { // run resolver if needed.
                resolvingAlgorithm.Run(cancellationToken);
                if (!resolvingAlgorithm.HasSucceeded)
                {
                    return new Result<Route>($"Failed to resolve: {resolvingAlgorithm.ErrorMessage}");
                }
            }

            if (turnPenalty <= 0 && preferredTurns == null)
            { // no turn penalty defined, just route along the locations and concatenate the results.
                var locations = resolvingAlgorithm.RouterPoints;
                var route = router.TryCalculate(profile, locations[0], locations[1], cancellationToken);
                if (route.IsError)
                {
                    return route;
                }

                for (var i = 2; i < locations.Count; i++)
                {
                    var nextRoute = router.TryCalculate(profile, locations[i - 1], locations[i],
                        cancellationToken);
                    if (nextRoute.IsError)
                    {
                        return nextRoute;
                    }

                    route = new Result<Route>(route.Value.Concatenate(nextRoute.Value));
                }

                return route;
            }
            else
            { // a more advanced approach, route along all locations but minimize the u-turns taken.
                var directedSequenceRouter = new DirectedSequenceRouter(resolvingAlgorithm, turnPenalty, preferredTurns);
                directedSequenceRouter.Run(cancellationToken);
                if (!directedSequenceRouter.HasSucceeded)
                {
                    return new Result<Route>($"Failed to run {nameof(DirectedSequenceRouter)}: {directedSequenceRouter.ErrorMessage}");
                }

                var routes = directedSequenceRouter.Routes;

                return routes.Concatenate();
            }
        }

        /// <summary> 
        /// Calculates a route along the given locations.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="resolvingAlgorithm">The resolving algorithm containing the location to route along.</param>
        /// <param name="cancellationToken">The cancellation token, if any.</param>
        /// <param name="turnPenalty">The turn penalty, if any.</param>
        /// <param name="preferredDirections">The perferred directions in degrees.</param>
        /// <returns>A route along all the given locations.</returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profile,
            IMassResolvingAlgorithm resolvingAlgorithm, float turnPenalty = 0, float?[] preferredDirections = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (preferredDirections == null)
            {
                return router.TryCalculate(profile, resolvingAlgorithm, turnPenalty, (Tuple<bool?, bool?>[]) null,
                    cancellationToken);
            }

            if (!resolvingAlgorithm.HasRun)
            {
                resolvingAlgorithm.Run(cancellationToken);
            }
            var preferredTurns = new Tuple<bool?, bool?>[preferredDirections.Length];
            for (var t = 0; t < preferredTurns.Length && t < resolvingAlgorithm.RouterPoints.Count; t++)
            {
                var direction = resolvingAlgorithm.RouterPoints[t]
                    .DirectionFromAngle(router.Db, preferredDirections[t]);
                preferredTurns[t] = new Tuple<bool?, bool?>(direction, direction);
            }
            return router.TryCalculate(profile, resolvingAlgorithm, turnPenalty, preferredTurns,
                cancellationToken);
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<float> TryCalculateWeight(this RouterBase router, IProfileInstance profile, Coordinate source, Coordinate target)
        {
            return router.TryCalculateWeight(profile, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<float> TryCalculateWeight(this RouterBase router, IProfileInstance profile, Coordinate source, Coordinate target, CancellationToken cancellationToken)
        {
            return router.TryCalculateWeight(profile, profile.DefaultWeightHandler(router), source, target, cancellationToken);
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate source, Coordinate target)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, source, target, CancellationToken.None);
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<T> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate source, Coordinate target,
            CancellationToken cancellationToken)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, source.Latitude, source.Longitude, target.Latitude, target.Longitude, cancellationToken);
        }

        /// <summary>
        /// Calculates all weights between all given locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, WeightHandler<T> weightHandler, IProfileInstance profile, Coordinate[] locations)
            where T : struct
        {
            return router.TryCalculateWeight(weightHandler, profile, locations, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all given locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, WeightHandler<T> weightHandler, IProfileInstance profile, Coordinate[] locations,
            CancellationToken cancellationToken)
            where T : struct
        {
            return router.TryCalculateWeight(profile,  weightHandler, locations, locations, cancellationToken);
        }

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate[] sources, Coordinate[] targets)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, sources, targets, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate[] sources, Coordinate[] targets,
            CancellationToken cancellationToken)
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
                var result = router.TryResolve(profile, targets[i], 50, cancellationToken);
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
            var weights = router.TryCalculateWeight(profile, weightHandler, resolvedSources, resolvedTargets, invalidSources, invalidTargets, null, cancellationToken);
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

        /// <summary>
        /// Calculates a weight matrix between directed edges, returning weight exclusing the first and last edge.
        /// </summary>
        public static Result<float[][]> TryCalculateWeight(this RouterBase router, IProfileInstance profileInstance, DirectedEdgeId[] sources,
            DirectedEdgeId[] targets, RoutingSettings<float> settings = null)
        {
            return router.TryCalculateWeight(profileInstance, sources, targets, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a weight matrix between directed edges, returning weight exclusing the first and last edge.
        /// </summary>
        public static Result<float[][]> TryCalculateWeight(this RouterBase router, IProfileInstance profileInstance, DirectedEdgeId[] sources,
            DirectedEdgeId[] targets, RoutingSettings<float> settings, CancellationToken cancellationToken)
        {
            return router.TryCalculateWeight(profileInstance, router.GetDefaultWeightHandler(profileInstance), sources, targets, settings, cancellationToken);
        }

        /// <summary>
        /// Calculates a weight matrix between directed edges, returning weight exclusing the first and last edge.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, DirectedEdgeId[] sources,
            DirectedEdgeId[] targets, RoutingSettings<T> settings = null)
            where T : struct
        {
            return router.TryCalculateWeight(profileInstance, weightHandler, sources, targets, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a weight matrix between directed edges, returning weight exclusing the first and last edge.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, DirectedEdgeId[] sources, 
            DirectedEdgeId[] targets, RoutingSettings<T> settings, CancellationToken cancellationToken)
            where T : struct
        {
            try
            {
                if (!router.Db.Supports(profileInstance.Profile))
                {
                    return new Result<T[][]>("Routing profile is not supported.", (message) =>
                    {
                        return new Exception(message);
                    });
                }

                var maxSearch = weightHandler.Infinite;
                if (settings != null)
                {
                    if (!settings.TryGetMaxSearch(profileInstance.Profile.FullName, out maxSearch))
                    {
                        maxSearch = weightHandler.Infinite;
                    }
                }

                ContractedDb contracted;
                if (router.Db.TryGetContracted(profileInstance.Profile, out contracted))
                { // contracted calculation.
                    if (router.Db.HasComplexRestrictions(profileInstance.Profile))
                    {
                        if (!(contracted.HasNodeBasedGraph && contracted.NodeBasedIsEdgedBased))
                        {
                            Logging.Logger.Log("Router", TraceEventType.Warning, 
                                "There is a contracted graph in the routerdb but it cannot be used for directional queries. Rebuild the routerDb, falling back to uncontracted routing.");
                            contracted = null;
                        }
                    }

                    if (!weightHandler.CanUse(contracted))
                    { // there is a contracted graph but it is not equipped to handle this weight-type.
                        Logging.Logger.Log("Router", Logging.TraceEventType.Warning,
                            "There is a contracted graph but it's not built for the given weight calculations, falling back to uncontracted routing.");
                        contracted = null;
                    }
                }

                if (contracted != null)
                { // do dual edge-based routing.
                    var graph = contracted.NodeBasedGraph;

                    var dykstraSources = Itinero.Algorithms.Contracted.Dual.DykstraSourceExtensions.ToDykstraSources<T>(sources);
                    var dykstraTargets = Itinero.Algorithms.Contracted.Dual.DykstraSourceExtensions.ToDykstraSources<T>(targets);
                    var algorithm = new Itinero.Algorithms.Contracted.Dual.ManyToMany.VertexToVertexWeightAlgorithm<T>(graph, weightHandler,
                        dykstraSources, dykstraTargets, maxSearch);
                    algorithm.Run(cancellationToken);
                    if (!algorithm.HasSucceeded)
                    {
                        return new Result<T[][]>(algorithm.ErrorMessage, (message) =>
                        {
                            return new Exceptions.RouteNotFoundException(message);
                        });
                    }

                    // subtract the weight of the first edge from each weight.
                    var edgeEnumerator = router.Db.Network.GeometricGraph.Graph.GetEdgeEnumerator();
                    var weights = algorithm.Weights;
                    for (var s = 0; s < sources.Length; s++)
                    {
                        var id = new DirectedEdgeId()
                        {
                            Raw = dykstraSources[s].Vertex1
                        };
                        edgeEnumerator.MoveToEdge(id.EdgeId);
                        var weight = weightHandler.GetEdgeWeight(edgeEnumerator);
                        for (var t = 0; t < dykstraTargets.Length; t++)
                        {
                            if (weightHandler.IsSmallerThan(weights[s][t], weightHandler.Infinite) &&
                                sources[s].Raw != targets[t].Raw)
                            {
                                weights[s][t] = weightHandler.Subtract(weights[s][t], weight.Weight);
                            }
                        }
                    }

                    return new Result<T[][]>(weights);
                }
                else
                { // use uncontracted routing.
                    var graph = router.Db.Network.GeometricGraph.Graph;

                    var dykstraSources = Itinero.Algorithms.Default.EdgeBased.DirectedDykstraSourceExtensions.ToDykstraSources<T>(sources);
                    var dykstraTargets = Itinero.Algorithms.Default.EdgeBased.DirectedDykstraSourceExtensions.ToDykstraSources<T>(targets);
                    var algorithm = new Itinero.Algorithms.Default.EdgeBased.DirectedManyToManyWeights<T>(graph, weightHandler, router.Db.GetRestrictions(profileInstance.Profile),
                        dykstraSources, dykstraTargets, maxSearch);
                    algorithm.Run(cancellationToken);
                    if (!algorithm.HasSucceeded)
                    {
                        return new Result<T[][]>(algorithm.ErrorMessage, (message) =>
                        {
                            return new Exceptions.RouteNotFoundException(message);
                        });
                    }

                    // subtract the weight of the first edge from each weight.
                    var edgeEnumerator = router.Db.Network.GeometricGraph.Graph.GetEdgeEnumerator();
                    var weights = algorithm.Weights;
                    for (var s = 0; s < sources.Length; s++)
                    {
                        var id = dykstraSources[s].Edge1;
                        edgeEnumerator.MoveToEdge(id.EdgeId);
                        var weight = weightHandler.GetEdgeWeight(edgeEnumerator);
                        for (var t = 0; t < dykstraTargets.Length; t++)
                        {
                            if (weightHandler.IsSmallerThan(weights[s][t], weightHandler.Infinite) &&
                                sources[s].Raw != targets[t].Raw)
                            {
                                weights[s][t] = weightHandler.Subtract(weights[s][t], weight.Weight);
                            }
                        }
                    }

                    return new Result<T[][]>(weights);
                }
            }
            catch(Exception ex)
            {
                return new Result<T[][]>(ex.Message);
            }
        }

        /// <summary>
        /// Calculates a route between the two given directed edges.
        /// </summary>
        public static Result<EdgePath<T>> TryCalculateRaw<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, DirectedEdgeId source, DirectedEdgeId target,
            RoutingSettings<T> settings = null)
            where T : struct
        {
            return router.TryCalculateRaw(profileInstance, weightHandler, source, target, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two given directed edges.
        /// </summary>
        public static Result<EdgePath<T>> TryCalculateRaw<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, DirectedEdgeId source, DirectedEdgeId target,
            RoutingSettings<T> settings, CancellationToken cancellationToken)
            where T : struct
        {
            return router.TryCalculateRaw(profileInstance, weightHandler, source.SignedDirectedId, target.SignedDirectedId, settings, cancellationToken);
        }

        /// <summary>
        /// Calculates a route between the two given router points but in a fixed direction.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profileInstance, RouterPoint source, bool sourceForward, RouterPoint target, bool targetForward,
            RoutingSettings<float> settings = null)
        {
            return router.TryCalculate(profileInstance, router.GetDefaultWeightHandler(profileInstance), source, sourceForward, target, targetForward, settings);
        }

        /// <summary>
        /// Calculates a route between the two given router points but in a fixed direction.
        /// </summary>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, bool sourceForward, RouterPoint target, bool targetForward,
            RoutingSettings<T> settings = null)
            where T : struct
        {
            return router.TryCalculate(profileInstance, weightHandler, source, (bool?)sourceForward, target, targetForward, settings);
        }

        /// <summary>
        /// Calculates a route between the two given router points but in a fixed direction.
        /// </summary>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profileInstance, RouterPoint source, bool? sourceForward, RouterPoint target, bool? targetForward,
            RoutingSettings<float> settings = null)
        {
            return router.TryCalculate(profileInstance, router.GetDefaultWeightHandler(profileInstance), source, sourceForward, target, targetForward, settings);
        }

        /// <summary>
        /// Calculates a route between the two given router points but in a fixed direction.
        /// </summary>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, bool? sourceForward, RouterPoint target, bool? targetForward,
            RoutingSettings<T> settings = null)
            where T : struct
        {
            return router.TryCalculate(profileInstance, weightHandler, source, sourceForward, target, targetForward, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two given router points but in a fixed direction.
        /// </summary>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, bool? sourceForward, RouterPoint target, bool? targetForward,
            RoutingSettings<T> settings, CancellationToken cancellationToken)
            where T : struct
        {
            try
            {
                EdgePath<T> path = null;
                if (source.EdgeId == target.EdgeId)
                { // check for a path on the same edge, in the requested direction(s).
                    if (sourceForward.HasValue &&
                        targetForward.HasValue)
                    { // both direction are set.
                        if (sourceForward == targetForward)
                        { // try to get a path inside the same edge.
                            path = source.EdgePathTo(router.Db, weightHandler, target, !sourceForward.Value);
                        }
                    }
                    else if(sourceForward.HasValue)
                    { // only source is set.
                        path = source.EdgePathTo(router.Db, weightHandler, target, !sourceForward.Value);
                    }
                    else if(targetForward.HasValue)
                    { // only target is set.
                        path = source.EdgePathTo(router.Db, weightHandler, target, !targetForward.Value);
                    }
                    else
                    { // both are don't care.
                        path = source.EdgePathTo(router.Db, weightHandler, target);
                    }
                    
                    if (path != null)
                    { // update settings objects to prevent uneeded searches.
                        if (settings == null)
                        {
                            settings = new RoutingSettings<T>();
                        }
                        else
                        {
                            settings = settings.Clone();
                        }
                        settings.SetMaxSearch(profileInstance.Profile.FullName, path.Weight);
                    }
                }

                // try calculating a path.
                var result = router.TryCalculateRaw<T>(profileInstance, weightHandler, source, sourceForward, target, targetForward, settings, cancellationToken);
                if (result != null &&
                    !result.IsError)
                { 
                    if (path == null || 
                        weightHandler.IsSmallerThan(result.Value.Weight, path.Weight))
                    { // the found path is better.
                        path = result.Value;
                    }
                }

                if (path != null)
                { // a route was found, return it.
                    // make sure the path represents the route between the two routerpoints not between the two edges.
                    if (path.From == null)
                    { // path has only one vertex, this represents a path of length '0'.
                        return router.BuildRoute(profileInstance.Profile, weightHandler, source, target, path, cancellationToken);
                    }

                    // path has at least two vertices, strip first and last vertex.
                    // TODO: optimize this, this can be done without converting to a vertex-list.
                    var vertices = new List<uint>();
                    while (path != null)
                    {
                        vertices.Add(path.Vertex);
                        path = path.From;
                    }
                    vertices.Reverse();
                    vertices[0] = Constants.NO_VERTEX;
                    vertices[vertices.Count - 1] = Constants.NO_VERTEX;
                    path = router.Db.BuildEdgePath(weightHandler, source, target, vertices);
                    return router.BuildRoute(profileInstance.Profile, weightHandler, source, target, path, cancellationToken);
                }
                else if (settings != null && 
                    !settings.DirectionAbsolute)
                { // no route was found but maybe because the requested directions aren't available.
                    if (sourceForward.HasValue)
                    { // the source direction was set, try again without it.
                        return router.TryCalculate(profileInstance, weightHandler, source, null, target, targetForward, settings, cancellationToken);
                    }
                    else if (targetForward.HasValue)
                    { // the target direction was set, try again without it.
                        return router.TryCalculate(profileInstance, weightHandler, source, target, settings, cancellationToken);
                    }
                    else
                    { // route wasn't found but there was no directional info either.
                        return new Result<Route>("Route not found.");
                    }
                }
                else
                { // route wasn't found and directions are strict.
                    return new Result<Route>("Route not found.");
                }
            }
            catch(Exception ex)
            {
                return new Result<Route>(ex.Message);
            }
        }

        /// <summary>
        /// Tries to calculate a route using the given directions as guidance.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="source">The source location.</param>
        /// <param name="sourceDirection">The direction to go in at the source location, an angle in degrees relative to north, null if don't care.</param>
        /// <param name="target">The target location.</param>
        /// <param name="targetDirection">The direction to arrive on at the target location, an angle in degrees relative to north, null if don't care.</param>
        /// <param name="diffLimit">The diff limit when the angle is smaller than this we consider it the same direction.</param>
        /// <returns></returns>
        public static Result<Route> TryCalculate(this RouterBase router, IProfileInstance profileInstance, RouterPoint source, float? sourceDirection, 
            RouterPoint target, float? targetDirection, float diffLimit = 45, RoutingSettings<float> settings = null)
        {
            return router.TryCalculate(profileInstance, router.GetDefaultWeightHandler(profileInstance), source, sourceDirection, 
                target, targetDirection, diffLimit, settings);
        }

        /// <summary>
        /// Tries to calculate a route using the given directions as guidance.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="source">The source location.</param>
        /// <param name="sourceDirection">The direction to go in at the source location, an angle in degrees relative to north, null if don't care.</param>
        /// <param name="target">The target location.</param>
        /// <param name="targetDirection">The direction to arrive on at the target location, an angle in degrees relative to north, null if don't care.</param>
        /// <param name="diffLimit">The diff limit when the angle is smaller than this we consider it the same direction.</param>
        /// <returns></returns>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, float? sourceDirection,
            RouterPoint target, float? targetDirection, float diffLimit = 45, RoutingSettings<T> settings = null)
            where T : struct
        {
            return router.TryCalculate(profileInstance, weightHandler, source, sourceDirection, target, targetDirection, diffLimit, settings, CancellationToken.None);
        }

        /// <summary>
        /// Tries to calculate a route using the given directions as guidance.
        /// </summary>
        /// <param name="router">The router.</param>
        /// <param name="source">The source location.</param>
        /// <param name="sourceDirection">The direction to go in at the source location, an angle in degrees relative to north, null if don't care.</param>
        /// <param name="target">The target location.</param>
        /// <param name="targetDirection">The direction to arrive on at the target location, an angle in degrees relative to north, null if don't care.</param>
        /// <param name="diffLimit">The diff limit when the angle is smaller than this we consider it the same direction.</param>
        /// <returns></returns>
        public static Result<Route> TryCalculate<T>(this RouterBase router, IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, float? sourceDirection,
            RouterPoint target, float? targetDirection, float diffLimit, RoutingSettings<T> settings, CancellationToken cancellationToken)
            where T : struct
        {
            if (diffLimit <= 0 || diffLimit > 90) { throw new ArgumentOutOfRangeException(nameof(diffLimit), "Expected to be in range ]0, 90]."); }

            if (sourceDirection.HasValue)
            { // make sure angles are normalized.
                sourceDirection = (float)Itinero.LocalGeo.Tools.NormalizeDegrees(sourceDirection.Value);
            }
            if (targetDirection.HasValue)
            { // make sure angles are normalized.
                targetDirection = (float)Itinero.LocalGeo.Tools.NormalizeDegrees(targetDirection.Value);
            }

            var sourceForward = source.DirectionFromAngle(router.Db, sourceDirection, diffLimit);
            var targetForward = target.DirectionFromAngle(router.Db, targetDirection, diffLimit);

            return router.TryCalculate(profileInstance, weightHandler, source, sourceForward, target, targetForward, settings, cancellationToken);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>        
        public static Route Calculate(this RouterBase router, IProfileInstance profile, RouterPoint source, float? sourceDirection,
            RouterPoint target, float? targetDirection)
        {
            return router.Calculate(profile, source, sourceDirection, target, targetDirection, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>        
        public static Route Calculate(this RouterBase router, IProfileInstance profile, RouterPoint source, float? sourceDirection, 
            RouterPoint target, float? targetDirection, CancellationToken cancellationToken)
        {
            return router.TryCalculate<float>(profile, router.GetDefaultWeightHandler(profile), source, sourceDirection, target, targetDirection, 45, null, cancellationToken).Value;
        }
    }
}