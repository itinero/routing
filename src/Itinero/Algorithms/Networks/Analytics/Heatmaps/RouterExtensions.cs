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

namespace Itinero.Algorithms.Networks.Analytics.Heatmaps
{
    /// <summary>
    /// Contains router extensions for the heatmap algorithm.
    /// </summary>
    public static class RouterExtensions
    {
        /// <summary>
        /// Calculates heatmap for the given profile.
        /// </summary>
        public static HeatmapResult CalculateHeatmap(this RouterBase router, Profile profile, Coordinate origin, int limitInSeconds, int zoom = 16)
        {
            if (!router.SupportsAll(profile))
            {
                throw new ArgumentException(string.Format("Profile {0} not supported.",
                    profile.FullName));
            }

            var routerOrigin = router.Resolve(profile, origin);
            return router.CalculateHeatmap(profile, routerOrigin, limitInSeconds, zoom);
        }

        /// <summary>
        /// Calculates heatmap for the given profile.
        /// </summary>
        public static HeatmapResult CalculateHeatmap(this RouterBase router, Profile profile, RouterPoint origin, int limitInSeconds, int zoom = 16)
        {
            if (!router.SupportsAll(profile))
            {
                throw new ArgumentException(string.Format("Profile {0} not supported.",
                    profile.FullName));
            }

            if (profile.Metric != ProfileMetric.TimeInSeconds)
            {
                throw new ArgumentException(string.Format("Profile {0} not supported, only profiles with metric TimeInSeconds are supported.",
                    profile.FullName));
            }

            // get the weight handler.
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var getFactor = router.GetDefaultGetFactor(profile);

            // calculate isochrones.
            var isochrone = new TileBasedHeatmapBuilder(router.Db.Network.GeometricGraph,
                new Algorithms.Default.Dykstra(router.Db.Network.GeometricGraph.Graph,
                    weightHandler, null, origin.ToEdgePaths<float>(router.Db, weightHandler, true), limitInSeconds, false), zoom);
            isochrone.Run();

            var result = isochrone.Result;
            result.MaxMetric = profile.Metric.ToInvariantString();
            return result;
        }
    }
}