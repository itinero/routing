// Itinero - Routing for .NET
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Networks.Analytics.Isochrones
{
    /// <summary>
    /// Contains router extensions for the isochrone algorithm.
    /// </summary>
    public static class RouterExtensions
    {
        /// <summary>
        /// Calculates isochrones for the given profile based on the given limits.
        /// </summary>
        public static List<LocalGeo.Polygon> CalculateIsochrones(this RouterBase router, Profile profile, Coordinate origin, List<float> limits, int zoom = 16)
        {
            var routerOrigin = router.Resolve(profile, origin);
            return router.CalculateIsochrones(profile, routerOrigin, limits, zoom);
        }

        /// <summary>
        /// Calculates isochrones for the given profile based on the given limits.
        /// </summary>
        public static List<LocalGeo.Polygon> CalculateIsochrones(this RouterBase router, Profile profile, RouterPoint origin, List<float> limits, int zoom = 16)
        {
            if (profile.Definition.Metric != ProfileMetric.TimeInSeconds)
            {
                throw new ArgumentException(string.Format("Profile {0} not supported, only profiles with metric TimeInSeconds are supported.",
                    profile.Definition.Name));
            }

            // get the weight handler.
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var getFactor = router.GetDefaultGetFactor(profile);

            // calculate isochrones.
            var isochrone = new TileBasedIsochroneBuilder(router.Db.Network.GeometricGraph,
                new Algorithms.Default.Dykstra(router.Db.Network.GeometricGraph.Graph,
                    weightHandler, null, origin.ToEdgePaths<float>(router.Db, weightHandler, true), limits.Max() * 1.1f, false), 
                limits, zoom);
            isochrone.Run();

            return isochrone.Polygons;
        }
    }
}