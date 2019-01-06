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

using System;
using Itinero.Data.Network;
using Itinero.Profiles;

namespace Itinero.Algorithms.Search.Cache
{
    /// <summary>
    /// Abstract representation of a resolver cache.
    /// </summary>
    public interface IResolverCache
    {
        /// <summary>
        /// Tries to get from this cache, the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        /// <param name="profileInstances">The profile instances.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="isBetter">The is better function.</param>
        /// <param name="maxSearchDistance">The maximum search distance.</param>
        /// <param name="settings">The settings, if any.</param>
        /// <returns>The resulting router point in cache.</returns>
        Result<RouterPoint> TryGet(IProfileInstance[] profileInstances, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float maxSearchDistance, ResolveSettings settings);

        /// <summary>
        /// Adds to this cache, the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        /// <param name="profileInstances">The profile instances.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        /// <param name="isBetter">The is better function.</param>
        /// <param name="maxSearchDistance">The maximum search distance.</param>
        /// <param name="settings">The settings, if any.</param>
        /// <param name="routerPointResult">The result to keep.</param>
        /// <returns>The resulting router point in cache.</returns>
        void Add(IProfileInstance[] profileInstances, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float maxSearchDistance, ResolveSettings settings,
            Result<RouterPoint> routerPointResult);
    }
}