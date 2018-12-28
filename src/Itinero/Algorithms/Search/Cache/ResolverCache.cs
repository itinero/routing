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
using System.Collections.Concurrent;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using Itinero.Profiles;

namespace Itinero.Algorithms.Search.Cache
{
    public class ResolverCache : IResolverCache
    {
        public Result<RouterPoint> TryGet(IProfileInstance[] profileInstances, float latitude, float longitude, Func<RoutingEdge, bool> isBetter,
            float maxSearchDistance, ResolveSettings settings)
        {
            throw new NotImplementedException();
        }

        public void Add(IProfileInstance[] profileInstances, float latitude, float longitude, Func<RoutingEdge, bool> isBetter, float maxSearchDistance,
            ResolveSettings settings, RouterPoint routerPoint)
        {
            throw new NotImplementedException();
        }
    }
}