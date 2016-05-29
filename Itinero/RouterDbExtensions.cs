// Itinero - OpenStreetMap (OSM) SDK
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

using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.Witness;
using Itinero.Attributes;
using Itinero.Graphs.Directed;
using Itinero.Data.Network.Restrictions;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Contracted;
using System.Collections.Generic;
using System;
using Itinero.Profiles;

namespace Itinero
{
    /// <summary>
    /// Contains extension methods for the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted(this RouterDb db, Profiles.Profile profile, bool forceEdgeBased = false)
        {
            // create the raw directed graph.
            ContractedDb contractedDb = null;

            // prebuild profile factor cache.
            var profileCache = new ProfileFactorCache(db);
            profileCache.CalculateFor(profile);
            var getFactor = profileCache.GetGetFactor(profile);
            lock (db)
            {
                //if (db.HasComplexRestrictions(profile) || forceEdgeBased)
                //{ // edge-based is needed when complex restrictions found.
                    var contracted = new DirectedDynamicGraph(1);
                    var directedGraphBuilder = new Algorithms.Contracted.EdgeBased.DirectedGraphBuilder(db.Network.GeometricGraph.Graph,
                        contracted, getFactor);
                    directedGraphBuilder.Run();

                    // contract the graph.
                    var getRestrictions = db.GetGetRestrictions(profile, true);
                    var witnessCalculator = new Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator(db.Network.GeometricGraph.Graph,
                        getFactor, getRestrictions);
                    var hierarchyBuilder = new Algorithms.Contracted.EdgeBased.HierarchyBuilder(contracted, new Algorithms.Contracted.EdgeBased.EdgeDifferencePriorityCalculator(contracted,
                        witnessCalculator), witnessCalculator, getRestrictions);
                    hierarchyBuilder.Run();

                    contractedDb = new ContractedDb(contracted);
                //}
                //else
                //{ // vertex-based is ok when no complex restrictions found.
                //    var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size, ContractedEdgeDataSerializer.MetaSize);
                //    var directedGraphBuilder = new DirectedGraphBuilder(db.Network.GeometricGraph.Graph, contracted, (p) =>
                //    {
                //        var tags = db.EdgeProfiles.Get(p);
                //        return profile.Factor(tags);
                //    });
                //    directedGraphBuilder.Run();
                    
                //    // contract the graph.
                //    var priorityCalculator = new EdgeDifferencePriorityCalculator(contracted,
                //        new DykstraWitnessCalculator(int.MaxValue));
                //    priorityCalculator.DifferenceFactor = 5;
                //    priorityCalculator.DepthFactor = 5;
                //    priorityCalculator.ContractedFactor = 8;
                //    var hierarchyBuilder = new HierarchyBuilder(contracted, priorityCalculator,
                //            new DykstraWitnessCalculator(int.MaxValue));
                //    hierarchyBuilder.Run();

                //    contractedDb = new ContractedDb(contracted);
                //}
            }

            // add the graph.
            lock(db)
            {
                db.AddContracted(profile, contractedDb);
            }
        }

        /// <summary>
        /// Returns true if all of the given profiles are supported.
        /// </summary>
        /// <returns></returns>
        public static bool SupportsAll(this RouterDb db, params Profiles.Profile[] profiles)
        {
            for (var i = 0; i < profiles.Length; i++)
            {
                if (!db.Supports(profiles[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns one attribute collection containing both the profile and meta tags.
        /// </summary>
        public static IAttributeCollection GetProfileAndMeta(this RouterDb db, uint profileId, uint meta)
        {
            var tags = new AttributeCollection();

            var metaTags = db.EdgeMeta.Get(meta);
            if (metaTags != null)
            {
                tags.AddOrReplace(metaTags);
            }

            var profileTags = db.EdgeProfiles.Get(profileId);
            if (profileTags != null)
            {
                tags.AddOrReplace(profileTags);
            }

            return tags;
        }

        /// <summary>
        /// Returns true if this db contains restrictions for the given vehicle type.
        /// </summary>
        public static bool HasRestrictions(this RouterDb db, string vehicleType)
        {
            RestrictionsDb restrictions;
            return db.TryGetRestrictions(vehicleType, out restrictions);
        }

        /// <summary>
        /// Returns true if this db contains complex restrictions for the given vehicle type.
        /// </summary>
        public static bool HasComplexRestrictions(this RouterDb db, string vehicleType)
        {
            RestrictionsDb restrictions;
            if (db.TryGetRestrictions(vehicleType, out restrictions))
            {
                return restrictions.HasComplexRestrictions;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this db contains complex restrictions for the given vehicle types.
        /// </summary>
        public static bool HasComplexRestrictions(this RouterDb db, IEnumerable<string> vehicleTypes)
        {
            if (db.HasComplexRestrictions(string.Empty))
            {
                return true;
            }
            foreach(var vehicleType in vehicleTypes)
            {
                if (db.HasComplexRestrictions(vehicleType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this db contains complex restrictions for the given profile.
        /// </summary>
        public static bool HasComplexRestrictions(this RouterDb db, Profiles.Profile profile)
        {
            return db.HasComplexRestrictions(profile.VehicleType);
        }

        /// <summary>
        /// Gets the get restriction function for the given profile.
        /// </summary>
        public static Func<uint, IEnumerable<uint[]>> GetGetRestrictions(this RouterDb db, Profiles.Profile profile, bool first)
        {
            var vehicleTypes = new List<string>(profile.VehicleType);
            vehicleTypes.Insert(0, string.Empty);
            return (vertex) =>
            {
                var restrictionList = new List<uint[]>();
                for (var i = 0; i < vehicleTypes.Count; i++)
                {
                    RestrictionsDb restrictionsDb;
                    if (db.TryGetRestrictions(vehicleTypes[i], out restrictionsDb))
                    {
                        var enumerator = restrictionsDb.GetEnumerator();
                        if (enumerator.MoveTo(vertex, first))
                        {
                            while (enumerator.MoveNext())
                            {
                                restrictionList.Add(enumerator.ToArray(!first));
                            }
                        }
                    }
                }
                return restrictionList;
            };
        }
    }
}