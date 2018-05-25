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
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            return router.CalculateIsochrones(profile, origin, limits, zoom, CancellationToken.None);
        }

        /// <summary>
        /// Calculates isochrones for the given profile based on the given limits.
        /// </summary>
        public static List<LocalGeo.Polygon> CalculateIsochrones(this RouterBase router, Profile profile, Coordinate origin, List<float> limits, int zoom, CancellationToken cancellationToken)
        {
            var routerOrigin = router.Resolve(profile, origin);
            return router.CalculateIsochrones(profile, routerOrigin, limits, zoom, cancellationToken);
        }

        /// <summary>
        /// Calculates isochrones for the given profile based on the given limits.
        /// </summary>
        public static List<LocalGeo.Polygon> CalculateIsochrones(this RouterBase router, Profile profile, RouterPoint origin, List<float> limits, int zoom = 16)
        {
            return router.CalculateIsochrones(profile, origin, limits, zoom, CancellationToken.None);
        }

        /// <summary>
        /// Calculates isochrones for the given profile based on the given limits.
        /// </summary>
        public static List<LocalGeo.Polygon> CalculateIsochrones(this RouterBase router, Profile profile, RouterPoint origin, List<float> limits, int zoom, CancellationToken cancellationToken)
        {
            if (profile.Metric != ProfileMetric.TimeInSeconds)
            {
                throw new ArgumentException(string.Format("Profile {0} not supported, only profiles with metric TimeInSeconds are supported.",
                    profile.FullName));
            }

            // get the weight handler.
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var getFactor = router.GetDefaultGetFactor(profile);

            // calculate isochrones.
            var isochrone = new TileBasedIsochroneBuilder(router.Db.Network.GeometricGraph,
                new Algorithms.Default.Dykstra(router.Db.Network.GeometricGraph.Graph,
                    weightHandler, null, origin.ToEdgePaths<float>(router.Db, weightHandler, true), limits.Max() * 1.1f, false), 
                limits, zoom);
            isochrone.Run(cancellationToken);

            return isochrone.Polygons;
        }
    }
}