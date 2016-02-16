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

using Itinero.Geo;
using Itinero.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero
{
    /// <summary>
    /// Contains extension methods on top of the basic IRouter interface.
    /// </summary>
    public static class IRouterExtensions
    {
        /// <summary>
        /// The default connectivity radius.
        /// </summary>
        public const float DefaultConnectivityRadius = 250;

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this IRouter router, Profile profile, float latitude, float longitude, 
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(new Profile[] { profile }, latitude, longitude, searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this IRouter router, Profile[] profiles, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, null, 
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this IRouter router, Profile profile, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.Resolve(new Profile[] { profile }, latitude, longitude, searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this IRouter router, Profile[] profiles, float latitude, float longitude,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this IRouter router, Profile[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, latitude, longitude, isBetter, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this IRouter router, Profile profile, RouterPoint point, float radiusInMeters)
        {
            return router.TryCheckConnectivity(profile, point, radiusInMeters).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        public static bool CheckConnectivity(this IRouter router, Profile profile, RouterPoint point)
        {
            return router.CheckConnectivity(profile, point, DefaultConnectivityRadius);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this IRouter router, Profile profile, RouterPoint[] locations)
        {
            return router.TryCalculate(profile, locations).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this IRouter router, Profile profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude)
        {
            return router.TryCalculate(profile, sourceLatitude, sourceLongitude, targetLatitude, targetLongitude).Value;
        }
        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this IRouter router, Profile profile, RouterPoint[] locations)
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
        public static Result<Route> TryCalculate(this IRouter router, Profile profile,
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
        public static Result<Route[]> TryCalculate(this IRouter router, Profile profile, RouterPoint source, RouterPoint[] targets)
        {
            var invalidSources = new HashSet<int>();
            var invalidTargets = new HashSet<int>();
            var result = router.TryCalculate(profile, new RouterPoint[] { source }, targets, invalidSources, invalidTargets);
            if (invalidSources.Count > 0)
            {
                return new Result<Route[]>("Some sources could not be routed from. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            if (invalidTargets.Count > 0)
            {
                return new Result<Route[]>("Some targets could not be routed to. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
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
        public static Route Calculate(this IRouter router, Profile profile, RouterPoint source, RouterPoint target)
        {
            return router.TryCalculate(profile, source, target).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[] Calculate(this IRouter router, Profile profile, RouterPoint source, RouterPoint[] targets)
        {
            return router.TryCalculate(profile, source, targets).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[][] Calculate(this IRouter router, Profile profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            return router.TryCalculate(profile, sources, targets).Value;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Result<Route[][]> TryCalculate(this IRouter router, Profile profile, RouterPoint[] sources, RouterPoint[] targets)
        {
            var invalidSources = new HashSet<int>();
            var invalidTargets = new HashSet<int>();
            var result = router.TryCalculate(profile, sources, targets, invalidSources, invalidTargets).Value;
            if (invalidSources.Count > 0)
            {
                return new Result<Route[][]>("Some sources could not be routed from. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            if (invalidTargets.Count > 0)
            {
                return new Result<Route[][]>("Some targets could not be routed to. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }

            var routes = new Route[result.Length][];
            for (var i = 0; i < result.Length; i++)
            {
                routes[i] = new Route[result[i].Length];
                for (var j = 0; j < result[i].Length; j++)
                {
                    routes[i][j] = result[i][j];
                }
            }
            return new Result<Route[][]>(routes);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        public static Route[][] Calculate(this IRouter router, Profile profile, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            return router.TryCalculate(profile, sources, targets, invalidSources, invalidTargets).Value;
        }

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        public static Result<float> TryCalculateWeight(this IRouter router, Profile profile,
            float sourceLatitude, float sourceLongitude, float targetLatitude, float targetLongitude)
        {
            var profiles = new Profile[] { profile };
            var sourcePoint = router.TryResolve(profiles, sourceLatitude, sourceLongitude);
            var targetPoint = router.TryResolve(profiles, targetLatitude, targetLongitude);

            if (sourcePoint.IsError)
            {
                return sourcePoint.ConvertError<float>();
            }
            if (targetPoint.IsError)
            {
                return targetPoint.ConvertError<float>();
            }
            return router.TryCalculateWeight(profile, sourcePoint.Value, targetPoint.Value);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<float[][]> TryCalculateWeight(this IRouter router, Profile profile, RouterPoint[] locations)
        {
            var invalids = new HashSet<int>();
            var result = router.TryCalculateWeight(profile, locations, locations, invalids, invalids);
            if (invalids.Count > 0)
            {
                return new Result<float[][]>("At least one location could not be routed from/to. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            return result;
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static Result<float[][]> TryCalculateWeight(this IRouter router, Profile profile, RouterPoint[] locations,
            ISet<int> invalids)
        {
            return router.TryCalculateWeight(profile, locations, locations, invalids, invalids);
        }

        /// <summary>
        /// Calculates all weights between all locations.
        /// </summary>
        public static float[][] CalculateWeight(this IRouter router, Profile profile, RouterPoint[] locations,
            ISet<int> invalids)
        {
            return router.TryCalculateWeight(profile, locations, invalids).Value;
        }


        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this IRouter router, Profile profile, Coordinate[] coordinates,
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
        public static Result<RouterPoint> TryResolve(this IRouter router, Profile profile, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(new Profile[] { profile }, coordinate, searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest points on the routing network that's routable for the given profile(s).
        /// </summary>
        public static Result<RouterPoint>[] TryResolve(this IRouter router, Profile[] profiles, Coordinate[] coordinates,
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
        public static Result<RouterPoint> TryResolve(this IRouter router, Profile[] profiles, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate.Latitude, coordinate.Longitude,
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static Result<RouterPoint> TryResolve(this IRouter router, Profile[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate.Latitude, coordinate.Longitude, isBetter,
                searchDistanceInMeter);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this IRouter router, Profile profile, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profile, coordinate, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint Resolve(this IRouter router, Profile[] profiles, Coordinate coordinate,
            float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        public static RouterPoint[] Resolve(this IRouter router, Profile profile, Coordinate[] coordinates,
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
        public static RouterPoint Resolve(this IRouter router, Profile[] profiles, Coordinate coordinate,
            Func<RoutingEdge, bool> isBetter,
                float searchDistanceInMeter = Constants.SearchDistanceInMeter)
        {
            return router.TryResolve(profiles, coordinate, isBetter, searchDistanceInMeter).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Route Calculate(this IRouter router, Profile profile, Coordinate source, Coordinate target)
        {
            return router.TryCalculate(profile, source, target).Value;
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Route Calculate(this IRouter router, Profile profile, Coordinate[] locations)
        {
            return router.TryCalculate(profile, locations).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        public static Result<Route> TryCalculate(this IRouter router, Profile profile, Coordinate source,
            Coordinate target)
        {
            return router.TryCalculate(profile, source.Latitude, source.Longitude, target.Latitude, target.Longitude);
        }

        /// <summary>
        /// Calculates a route along the given locations.
        /// </summary>
        public static Result<Route> TryCalculate(this IRouter router, Profile profile, Coordinate[] locations)
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
        public static Result<float> TryCalculateWeight(this IRouter router, Profile profile, Coordinate source, Coordinate target)
        {
            return router.TryCalculateWeight(profile, source.Latitude, source.Longitude, target.Latitude, target.Longitude);
        }

        /// <summary>
        /// Calculates all weights between all given locations.
        /// </summary>
        public static Result<float[][]> TryCalculateWeight(this IRouter router, Profile profile, Coordinate[] locations)
        {
            return router.TryCalculateWeight(profile, locations, locations);
        }

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        public static Result<float[][]> TryCalculateWeight(this IRouter router, Profile profile, Coordinate[] sources, Coordinate[] targets)
        {
            var resolvedSources = new RouterPoint[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                var result = router.TryResolve(profile, sources[i]);
                if (result.IsError)
                {
                    return new Result<float[][]>(string.Format("Source at index {0} could not be resolved: {1}",
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
                    return new Result<float[][]>(string.Format("Target at index {0} could not be resolved: {1}",
                        i, result.ErrorMessage), (s) =>
                        {
                            throw new Exceptions.ResolveFailedException(s);
                        });
                }
                resolvedTargets[i] = result.Value;
            }

            var invalidSources = new HashSet<int>();
            var invalidTargets = new HashSet<int>();
            var weights = router.TryCalculateWeight(profile, resolvedSources, resolvedTargets, invalidSources, invalidTargets);
            if (invalidSources.Count > 0)
            {
                return new Result<float[][]>("Some sources could not be routed from. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            if (invalidTargets.Count > 0)
            {
                return new Result<float[][]>("Some targets could not be routed to. Most likely there are islands in the loaded network.", (s) =>
                {
                    throw new Exceptions.RouteNotFoundException(s);
                });
            }
            return weights;
        }
    }
}