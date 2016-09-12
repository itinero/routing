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

using Itinero;
using Itinero.LocalGeo;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

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
        public static List<Tuple<float, float, List<Coordinate>>> CalculateTree(this RouterBase router, Profile profile, Coordinate origin, float max)
        {
            return router.TryCalculateTree(profile, router.Resolve(profile, origin), max).Value;
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static List<Tuple<float, float, List<Coordinate>>> CalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max)
        {
            return router.TryCalculateTree(profile, origin, max).Value;
        }

        /// <summary>
        /// Tries to calculate a tree starting at the given location.
        /// </summary>
        public static Result<List<Tuple<float, float, List<Coordinate>>>> TryCalculateTree(this RouterBase router, Profile profile, RouterPoint origin, float max)
        {
            if (!router.SupportsAll(profile))
            {
                return new Result<List<Tuple<float, float, List<Coordinate>>>>(string.Format("Profile {0} not supported.",
                    profile.Name));
            }

            if (profile.Metric != ProfileMetric.TimeInSeconds)
            {
                return new Result<List<Tuple<float, float, List<Coordinate>>>>(string.Format("Profile {0} not supported, only profiles with metric TimeInSeconds are supported.",
                    profile.Name));
            }

            // get the weight handler.
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var getFactor = router.GetDefaultGetFactor(profile);

            // calculate isochrones.
            var treeBuilder = new TreeBuilder(
                new DykstraEdgeVisitor(router.Db.Network.GeometricGraph,
                    getFactor, origin.ToEdgePaths<float>(router.Db, weightHandler, true), max));
            treeBuilder.Run();
            
            return new Result<List<Tuple<float, float, List<Coordinate>>>>(treeBuilder.Tree.Values.ToList());
        }
    }
}