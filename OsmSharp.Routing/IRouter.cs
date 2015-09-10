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
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Abstract representation of a router.
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given vehicles.
        /// </summary>
        /// <returns></returns>
        Result<RouterPoint> TryResolve(Vehicle[] vehicles, GeoCoordinate location);

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        /// <returns></returns>
        bool CheckConnectivity(Vehicle vehicle, RouterPoint point, Meter radius);

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        Result<Route> TryCalculate(Vehicle vehicle, RouterPoint source, RouterPoint target);

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        Result<Route>[][] TryCalculate(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets);
    }
}