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

using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Data.Network;
using Itinero.Geo;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Geo
{
    /// <summary>
    /// Contains extension methods for the IRouter.
    /// </summary>
    public static class RouterExtensions
    {
        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this Router router, Profile profile, GeoAPI.Geometries.Coordinate[] coordinates,
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
        public static Result<RouterPoint> TryResolve(this Router router, Profile profile, GeoAPI.Geometries.Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(new Profile[] { profile }, coordinate, searchDistanceInMeter);
        }
        
        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this Router router, Profile[] profiles, GeoAPI.Geometries.Coordinate[] coordinates,
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
        public static Result<RouterPoint> TryResolve(this Router router, Profile[] profiles, GeoAPI.Geometries.Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, (float)coordinate.Y, (float)coordinate.X,
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this Router router, Profile[] profiles, GeoAPI.Geometries.Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, (float)coordinate.Y, (float)coordinate.X, isBetter,
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this Router router, Profile profile, GeoAPI.Geometries.Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profile, coordinate, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this Router router, Profile[] profiles, GeoAPI.Geometries.Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint[] Resolve(this Router router, Profile profile, GeoAPI.Geometries.Coordinate[] coordinates,
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
        public static RouterPoint Resolve(this Router router, Profile[] profiles, GeoAPI.Geometries.Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter,
                float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, isBetter, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this Router router, Profile profile, GeoAPI.Geometries.Coordinate source, GeoAPI.Geometries.Coordinate target)
        {
            return router.TryCalculate(profile, source, target).Value;
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this Router router, Profile profile, GeoAPI.Geometries.Coordinate[] locations)
        {
            return router.TryCalculate(profile, locations).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this Router router, Profile profile, GeoAPI.Geometries.Coordinate source,
            GeoAPI.Geometries.Coordinate target)
        {
            return router.TryCalculate(profile, (float)source.Y, (float)source.X, (float)target.Y, (float)target.X);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this Router router, Profile profile, GeoAPI.Geometries.Coordinate[] locations)
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
        public static Result<T> TryCalculateWeight<T>(this Router router, Profile profile, WeightHandler<T> weightHandler, GeoAPI.Geometries.Coordinate source, GeoAPI.Geometries.Coordinate target)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, (float)source.Y, (float)source.X, (float)target.Y, (float)target.X);
        }

        /// <summary>
        /// Calculates all weights between all given locations.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this Router router, Profile profile, WeightHandler<T> weightHandler, GeoAPI.Geometries.Coordinate[] locations)
            where T : struct
        {
            return router.TryCalculateWeight(profile, weightHandler, locations, locations);
        }

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        public static Result<T[][]> TryCalculateWeight<T>(this Router router, Profile profile, WeightHandler<T> weightHandler, GeoAPI.Geometries.Coordinate[] sources, GeoAPI.Geometries.Coordinate[] targets)
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
            var weights = router.TryCalculateWeight(profile, weightHandler, resolvedSources, resolvedTargets, invalidSources, invalidTargets, null);
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