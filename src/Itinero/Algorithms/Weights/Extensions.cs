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

using Itinero.Algorithms.Collections;
using Itinero.Data.Contracted;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// Contains extension methods related to weights and weight handlers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Calculates the weight for the given edge and distance.
        /// </summary>
        public static T Calculate<T>(this WeightHandler<T> handler, ushort edgeProfile, float distance)
            where T : struct
        {
            Factor factor;
            return handler.Calculate(edgeProfile, distance, out factor);
        }

        /// <summary>
        /// Returns true if weigh1 > weight2 according to the weight handler.
        /// </summary>
        public static bool IsLargerThan<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) > handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 > weight2 according to the weight handler.
        /// </summary>
        public static bool IsLargerThanOrEqual<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) >= handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 smaller than weight2 according to the weight handler.
        /// </summary>
        public static bool IsSmallerThan<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) < handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 smaller than weight2 according to the weight handler.
        /// </summary>
        public static bool IsSmallerThanOrEqual<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) <= handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 smaller than metric according to the weight handler.
        /// </summary>
        public static bool IsSmallerThan<T>(this WeightHandler<T> handler, T weight1, float metric)
            where T : struct
        {
            return handler.GetMetric(weight1) < metric;
        }

        /// <summary>
        /// Returns the default weight handler.
        /// </summary>
        public static DefaultWeightHandler DefaultWeightHandler(this IProfileInstance profile, RouterBase router)
        {
            return router.GetDefaultWeightHandler(profile);
        }

        /// <summary>
        /// Returns the default weight handler but calculates a profile cache first.
        /// </summary>
        public static DefaultWeightHandler DefaultWeightHandlerCached(this IProfileInstance profile, RouterDb routerDb)
        {            
            // prebuild profile factor cache.
            var profileCache = new ProfileFactorAndSpeedCache(routerDb);
            profileCache.CalculateFor(profile.Profile);

            return new Weights.DefaultWeightHandler(profileCache.GetGetFactor(profile));
        }

        /// <summary>
        /// Returns the augmented weight handler.
        /// </summary>
        public static WeightHandler AugmentedWeightHandler(this IProfileInstance profile, RouterBase router)
        {
            return router.GetAugmentedWeightHandler(profile);
        }

        /// <summary>
        /// Returns the augmented weight handler.
        /// </summary>
        public static WeightHandler AugmentedWeightHandlerCached(this IProfileInstance profile, RouterDb routerDb)
        {
            // prebuild profile factor cache.
            var profileCache = new ProfileFactorAndSpeedCache(routerDb);
            profileCache.CalculateFor(profile.Profile);

            return new Weights.WeightHandler(profileCache.GetGetFactorAndSpeed(profile));
        }

        /// <summary>
        /// Checks if the given graph can be used with the weight handler.
        /// </summary>
        public static void CheckCanUse<T>(this WeightHandler<T> weightHandler, ContractedDb contractedDb)
            where T : struct
        {
            if (!weightHandler.CanUse(contractedDb))
            {
                throw new ArgumentException("Cannot used the given graph and the weight handler together. The data layouts don't match.");
            }
        }
        
        /// <summary>
        /// Checks if the given graph can be used with the weight handler.
        /// </summary>
        public static void CheckCanUse<T>(this WeightHandler<T> weightHandler, DirectedMetaGraph graph)
            where T : struct
        {
            weightHandler.CheckCanUse(new ContractedDb(graph, false));
        }

        /// <summary>
        /// Checks if the given graph can be used with the weight handler.
        /// </summary>
        public static void CheckCanUse<T>(this WeightHandler<T> weightHandler, DirectedDynamicGraph graph)
            where T : struct
        {
            weightHandler.CheckCanUse(new ContractedDb(graph));
        }

        /// <summary>
        /// Returns an edge path for the path represented by the given pointer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EdgePath<T> GetPath<T>(this WeightHandler<T> weightHandler, PathTree pathTree, uint pointer)
            where T : struct
        {
            uint vertex, previous;
            T weight;
            weightHandler.GetPathTree(pathTree, pointer, out vertex, out weight, out previous);
            if (previous == uint.MaxValue)
            {
                return new EdgePath<T>(vertex);
            }
            var previousPath = weightHandler.GetPath(pathTree, previous);
            return new EdgePath<T>(vertex, weight, previousPath);
        }
    }
}