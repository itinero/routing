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
using Itinero.Algorithms.Collections;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using Itinero.Profiles;

namespace Itinero.Algorithms.Search.Cache
{
    /// <summary>
    /// A resolver cache, caches resolved router points.
    /// </summary>
    public class ResolverCache : IResolverCache
    {
        private readonly ConcurrentDictionary<Key, LRUCache<Coordinate, Result<RouterPoint>>> _data = new ConcurrentDictionary<Key, LRUCache<Coordinate, Result<RouterPoint>>>();
        private readonly int _size;

        /// <summary>
        /// Creates a new resolver cache.
        /// </summary>
        /// <param name="size">The size of the cache.</param>
        public ResolverCache(int size = 1000)
        {
            _size = size;
        }

        //private int _hits = 0;
        
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
        public Result<RouterPoint> TryGet(IProfileInstance[] profileInstances, float latitude, float longitude, Func<RoutingEdge, bool> isBetter,
            float maxSearchDistance, ResolveSettings settings)
        {
            var key = new Key(profileInstances, isBetter, maxSearchDistance, settings);

            if (!_data.TryGetValue(key, out var lruCache))
            {
                return null;
            }

            if (!lruCache.TryGet(new Coordinate(latitude, longitude), out var cachedResult))
            {
                return null;
            }

            //_hits++;
            //Console.WriteLine($"Cache hit: {_hits}");

            return cachedResult;
        }

        //private int _added = 0;
        
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
        public void Add(IProfileInstance[] profileInstances, float latitude, float longitude, Func<RoutingEdge, bool> isBetter, float maxSearchDistance,
            ResolveSettings settings, Result<RouterPoint> routerPointResult)
        {
            var key = new Key(profileInstances, isBetter, maxSearchDistance, settings);

            if (!_data.TryGetValue(key, out var lruCache))
            {
                _data.TryAdd(key, new LRUCache<Coordinate, Result<RouterPoint>>(_size));
                _data.TryGetValue(key, out lruCache);
                if (lruCache == null)
                { // can this happen, it shouldn't, how does concurrent dictionary work exactly?
                    return;
                }
            }

            var location = new Coordinate(latitude, longitude);
            if (lruCache.TryGet(location, out var cachedResult))
            { // already in cache.
                return;
            }

            //_added++;
            //Console.WriteLine($"Added: {_added}");
            
            lruCache.Add(location, cachedResult);
        }

        private class Key
        {
            public Key(IProfileInstance[] profileInstances, Func<RoutingEdge, bool> isBetter,
                float maxSearchDistance, ResolveSettings settings)
            {
                this.ProfileInstanceNames = new string[profileInstances.Length];
                for (var i = 0; i < this.ProfileInstanceNames.Length; i++)
                {
                    this.ProfileInstanceNames[i] = profileInstances[i].Profile.FullName;
                }
                this.IsBetterFunc = isBetter;
                this.MaxSearchDistance = maxSearchDistance;
                this.ResolveSettings = settings?.Clone();
            }
            
            public string[] ProfileInstanceNames { get; }
            
            public Func<RoutingEdge, bool> IsBetterFunc { get; }
            
            public float MaxSearchDistance { get; }
            
            public ResolveSettings ResolveSettings { get; }

            public override bool Equals(object obj)
            {
                if (!(obj is Key other)) return false;

                if (this.MaxSearchDistance != other.MaxSearchDistance) return false;
                if (this.IsBetterFunc != other.IsBetterFunc) return false;
                if (!this.ResolveSettings.Equals(other.ResolveSettings)) return false;
                if (this.ProfileInstanceNames.Length != other.ProfileInstanceNames.Length) return false;

                for (var i = 0; i < this.ProfileInstanceNames.Length; i++)
                {
                    if (this.ProfileInstanceNames[i] != other.ProfileInstanceNames[i]) return false;
                }

                return true;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = 17;
                    hash = hash * 23 + this.ProfileInstanceNames.Length.GetHashCode();
                    for (var i = 0; i < this.ProfileInstanceNames.Length; i++)
                    {
                        hash = hash * 23 + this.ProfileInstanceNames[i].GetHashCode();
                    }
                    if (this.IsBetterFunc != null)
                    {
                        hash = hash * 23 + this.IsBetterFunc.GetHashCode();
                    }
                    hash = hash * 23 + this.MaxSearchDistance.GetHashCode();
                    hash = hash * 23 + this.ResolveSettings.GetHashCode();
                    return hash;
                }
            }
        }
    }
}