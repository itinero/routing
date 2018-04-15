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

using Itinero.Algorithms.Networks.Analytics.Trees.Models;
using Itinero.LocalGeo;
using Itinero.Profiles;
using System.Threading;

namespace Itinero.Algorithms.Networks.Analytics.Trees
{
    /// <summary>
    /// Contains router extensions for the tree builder algorithm.
    /// </summary>
    public static class RouterExtension
    {
        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Tree CalculateTree(this RouterBase router, Profile profile, Coordinate origin, float max)
        {
            return router.CalculateTree(profile, origin, max, CancellationToken.None);
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Tree CalculateTree(this RouterBase router, Profile profile, Coordinate origin, float max, CancellationToken cancellationToken)
        {
            return router.TryCalculateTree(profile, router.Resolve(profile, origin, 500), max, cancellationToken).Value;
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Tree CalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max)
        {
            return router.CalculateTree(profile, origin, max, CancellationToken.None);
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Tree CalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max, CancellationToken cancellationToken)
        {
            return router.TryCalculateTree(profile, origin, max, cancellationToken).Value;
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Result<Tree> TryCalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max)
        {
            return router.TryCalculateTree(profile, origin, max, CancellationToken.None);
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Result<Tree> TryCalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max, CancellationToken cancellationToken)
        {
            if (!router.SupportsAll(profile))
            {
                return new Result<Tree>(string.Format("Profile {0} not supported.",
                    profile.FullName));
            }

            if (profile.Metric != ProfileMetric.TimeInSeconds)
            {
                return new Result<Tree>(string.Format("Profile {0} not supported, only profiles with metric TimeInSeconds are supported.",
                    profile.FullName));
            }

            // get the weight handler.
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var getFactor = router.GetDefaultGetFactor(profile);

            // calculate isochrones.
            TreeBuilder treeBuilder = null;

            if (!router.Db.HasComplexRestrictions(profile))
            {
                treeBuilder = new TreeBuilder(router.Db.Network.GeometricGraph,
                    new Algorithms.Default.Dykstra(router.Db.Network.GeometricGraph.Graph,
                        weightHandler, null, origin.ToEdgePaths<float>(router.Db, weightHandler, true), max, false));
            }
            else
            {
                treeBuilder = new TreeBuilder(router.Db.Network.GeometricGraph,
                    new Algorithms.Default.EdgeBased.Dykstra(router.Db.Network.GeometricGraph.Graph,
                        weightHandler, router.Db.GetGetRestrictions(profile, true), origin.ToEdgePaths<float>(router.Db, weightHandler, true), max, false));
            }
            treeBuilder.Run(cancellationToken);

            return new Result<Tree>(treeBuilder.Tree);
        }
    }
}