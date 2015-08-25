// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.Routers;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Contains common IRouter extensions.
    /// </summary>
    public static class RouterExtensions
    {
        /// <summary>
        /// Checks connectivity of all given points and returns only those that are valid.
        /// </summary>
        /// <param name="router"></param>
        /// <param name="vehicle"></param>
        /// <param name="resolvedPoints"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public static RouterPoint[] CheckConnectivityAndRemoveInvalid(
            this Router router, Vehicle vehicle, RouterPoint[] resolvedPoints, float weight)
        {
            var connectedPoints = new List<RouterPoint>();
            for (int idx = 0; idx < resolvedPoints.Length; idx++)
            {
                RouterPoint resolvedPoint = resolvedPoints[idx];
                if (resolvedPoint != null &&
                    router.CheckConnectivity(vehicle, resolvedPoint, weight))
                { // the point is connected.
                    connectedPoints.Add(resolvedPoint);
                }
            }
            return connectedPoints.ToArray();
        }
    }
}
