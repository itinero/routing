// Itinero - Routing for .NET
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

using Itinero.Algorithms.Networks.Analytics.Trees.Models;
using Itinero.LocalGeo;
using Itinero.Profiles;

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
            return router.TryCalculateTree(profile, router.Resolve(profile, origin, 500), max).Value;
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Tree CalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max)
        {
            return router.TryCalculateTree(profile, origin, max).Value;
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Result<Tree> TryCalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max)
        {
            if (!router.SupportsAll(profile))
            {
                return new Result<Tree>(string.Format("Profile {0} not supported.",
                    profile.Name));
            }

            if (profile.Metric != ProfileMetric.TimeInSeconds)
            {
                return new Result<Tree>(string.Format("Profile {0} not supported, only profiles with metric TimeInSeconds are supported.",
                    profile.Name));
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
            treeBuilder.Run();

            return new Result<Tree>(treeBuilder.Tree);
        }
    }
}