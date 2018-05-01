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
using Itinero.Profiles;
using System;
using System.Threading;

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
            return router.CalculateHeatmap(profile, origin, limitInSeconds, zoom, CancellationToken.None);
        }

        /// <summary>
        /// Calculates heatmap for the given profile.
        /// </summary>
        public static HeatmapResult CalculateHeatmap(this RouterBase router, Profile profile, Coordinate origin, int limitInSeconds, int zoom, CancellationToken cancellationToken)
        {
            if (!router.SupportsAll(profile))
            {
                throw new ArgumentException(string.Format("Profile {0} not supported.",
                    profile.FullName));
            }

            var routerOrigin = router.Resolve(profile, origin);
            return router.CalculateHeatmap(profile, routerOrigin, limitInSeconds, zoom, cancellationToken);
        }

        /// <summary>
        /// Calculates heatmap for the given profile.
        /// </summary>
        public static HeatmapResult CalculateHeatmap(this RouterBase router, Profile profile, RouterPoint origin, int limitInSeconds, int zoom = 16)
        {
            return router.CalculateHeatmap(profile, origin, limitInSeconds, zoom, CancellationToken.None);
        }

        /// <summary>
        /// Calculates heatmap for the given profile.
        /// </summary>
        public static HeatmapResult CalculateHeatmap(this RouterBase router, Profile profile, RouterPoint origin, int limitInSeconds, int zoom, CancellationToken cancellationToken)
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
            isochrone.Run(cancellationToken);

            var result = isochrone.Result;
            result.MaxMetric = profile.Metric.ToInvariantString();
            return result;
        }
    }
}