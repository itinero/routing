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
using OsmSharp.Routing.Exceptions;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Osm.Vehicles;

namespace OsmSharp.Routing
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
        /// Searches for the closest point on the routing network that's routable for the given vehicles.
        /// </summary>
        /// <returns></returns>
        public static RouterPoint Resolve(this IRouter router, Vehicle[] vehicles, GeoCoordinate location)
        {
            return router.TryResolve(vehicles, location).Value;
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        /// <returns></returns>
        public static bool CheckConnectivity(this IRouter router, Vehicle vehicle, RouterPoint point)
        {
            return router.CheckConnectivity(vehicle, point, DefaultConnectivityRadius);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public static Route Calculate(this IRouter router, Vehicle vehicle,
            GeoCoordinate source, GeoCoordinate target)
        {
            return router.TryCalculate(vehicle, source, target).Value;
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public static Result<Route> TryCalculate(this IRouter router, Vehicle vehicle,
            GeoCoordinate source, GeoCoordinate target)
        {
            var vehicles = new Vehicle[] { vehicle };
            var sourcePoint = router.TryResolve(vehicles, source);
            var targetPoint = router.TryResolve(vehicles, target);

            return router.TryCalculate(vehicle, sourcePoint.Value, targetPoint.Value);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public static Result<Route>[] TryCalculate(this IRouter router, Vehicle vehicle, RouterPoint source, RouterPoint[] targets)
        {
            var result = router.TryCalculate(vehicle, new RouterPoint[] { source }, targets);
            var routes = new Result<Route>[result.Length];
            for (var j = 0; j < result.Length; j++)
            {
                routes[j] = result[0][j];
            }
            return routes;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public static Route[] Calculate(this IRouter router, Vehicle vehicle, RouterPoint source, RouterPoint[] targets)
        {
            var result = router.TryCalculate(vehicle, new RouterPoint[] { source }, targets);
            var routes = new Route[result.Length];
            for (var j = 0; j < result.Length; j++)
            {
                routes[j] = result[0][j].Value;
            }
            return routes;
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public static Route[][] Calculate(this IRouter router, Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets)
        {
            var result = router.TryCalculate(vehicle, sources, targets);
            var routes = new Route[result.Length][];
            for(var i = 0; i < result.Length; i++)
            {
                routes[i] = new Route[result[i].Length];
                for(var j = 0; j < result[i].Length; j++)
                {
                    routes[i][j] = result[i][j].Value;
                }
            }
            return routes;
        }
    }
}