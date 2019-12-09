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

using Itinero.Attributes;
using Itinero.Profiles;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Profiles
{
    /// <summary>
    /// An LRU cache for vehicle profiles.
    /// </summary>
    public class VehicleCache
    {
        private readonly Dictionary<uint, WhitelistAndFlags> _cache;
        private readonly Vehicle[] _vehicles;
        private readonly AttributesIndex _edgeProfiles;

        /// <summary>
        /// Creates a new vehicle cache.
        /// </summary>
        public VehicleCache(Vehicle[] vehicles)
        {
            _cache = new Dictionary<uint, WhitelistAndFlags>();
            _vehicles = vehicles;
            _edgeProfiles = new AttributesIndex(AttributesIndexMode.IncreaseOne
                 | AttributesIndexMode.ReverseAll);
        }

        /// <summary>
        /// Gets the vehicles.
        /// </summary>
        public Vehicle[] Vehicles
        {
            get
            {
                return _vehicles;
            }
        }

        /// <summary>
        /// Adds a new collection to cache if appropriate.
        /// </summary>
        public bool Add(IAttributeCollection attributes, bool filter = true)
        {
            Whitelist whitelist;
            bool[] canTraverse;
            return this.Add(attributes, out whitelist, out canTraverse, filter);
        }

        /// <summary>
        /// Adds a new collection to cache if appropriate.
        /// </summary>
        public bool Add(IAttributeCollection attributes, out Whitelist whitelist, out bool[] canTraverse, bool filter = true)
        {
            IAttributeCollection filtered = attributes;
            if (filter)
            {
                filtered = new AttributeCollection();

                foreach (var attribute in attributes)
                {
                    if (_vehicles.IsOnProfileWhiteList(attribute.Key))
                    {
                        filtered.AddOrReplace(attribute);
                    }
                }
            }

            if (filtered.Count == 0)
            {
                whitelist = null;
                canTraverse = null;
                return false;
            }
            var id = _edgeProfiles.Add(filtered);
            WhitelistAndFlags whitelistAndFlags;
            if (!_cache.TryGetValue(id, out whitelistAndFlags))
            {
                whitelist = new Whitelist();
                canTraverse = new bool[_vehicles.Length];
                _vehicles.AddToWhiteList(filtered, whitelist, canTraverse);
                _cache[id] = new WhitelistAndFlags()
                {
                    CanTraverse = canTraverse,
                    Whitelist = whitelist
                };
                return true;
            }
            whitelist = whitelistAndFlags.Whitelist;
            canTraverse = whitelistAndFlags.CanTraverse;
            return true;
        }

        /// <summary>
        /// Tries to get cached whitelist.
        /// </summary>
        public bool TryGetCached(IAttributeCollection attributes, out Whitelist whitelist, bool filter = true)
        {
            bool[] canTraverse;
            return this.TryGetCached(attributes, out whitelist, out canTraverse, filter);
        }

        /// <summary>
        /// Tries to get cached whitelist.
        /// </summary>
        public bool TryGetCached(IAttributeCollection attributes, out Whitelist whitelist, out bool[] canTraverse, bool filter = true)
        {
            IAttributeCollection filtered = attributes;
            if (filter)
            {
                filtered = new AttributeCollection();

                foreach (var attribute in attributes)
                {
                    if (_vehicles.IsOnProfileWhiteList(attribute.Key))
                    {
                        filtered.AddOrReplace(attribute);
                    }
                }
            }

            if (filtered.Count == 0)
            {
                whitelist = null;
                canTraverse = null;
                return false;
            }

            var id = _edgeProfiles.Add(filtered);
            WhitelistAndFlags whitelistAndFlags;
            if (!_cache.TryGetValue(id, out whitelistAndFlags))
            {
                whitelist = null;
                canTraverse = null;
                return false;
            }
            whitelist = whitelistAndFlags.Whitelist;
            canTraverse = whitelistAndFlags.CanTraverse;
            return true;
        }

        /// <summary>
        /// Returns true if any of the vehicle can traverse the given way.
        /// </summary>
        public bool AnyCanTraverse(IAttributeCollection attributes, bool filter = true)
        {
            IAttributeCollection filtered = attributes;
            if (filter)
            {
                filtered = new AttributeCollection();

                foreach (var attribute in attributes)
                {
                    if (_vehicles.IsOnProfileWhiteList(attribute.Key))
                    {
                        filtered.AddOrReplace(attribute);
                    }
                }
            }

            if (this.TryGetCached(filtered, out var whitelist, out var canTraverse, false))
            {
                if (whitelist.Count > 0)
                {
                    return canTraverse.Contains(true);
                }
            }
            if (this.Add(filtered, out whitelist, out canTraverse, false))
            {
                if (whitelist.Count > 0)
                {
                    return canTraverse.Contains(true);
                }
            }

            return false;
        }

        /// <summary>
        /// Returns true if the given vehicle can traverse the given edge.
        /// </summary>
        public bool CanTraverse(IAttributeCollection attributes1, Vehicle vehicle, bool filter = true)
        {
            IAttributeCollection filtered = attributes1;
            if (filter)
            {
                filtered = new AttributeCollection();

                foreach (var attribute in attributes1)
                {
                    if (_vehicles.IsOnProfileWhiteList(attribute.Key))
                    {
                        filtered.AddOrReplace(attribute);
                    }
                }
            }

            for (var i = 0; i < _vehicles.Length; i++)
            {
                if (_vehicles[i].Name == vehicle.Name)
                {
                    bool[] canTraverse;
                    Whitelist whitelist;
                    if (!this.TryGetCached(filtered, out whitelist, out canTraverse, false))
                    {
                        if (!this.Add(filtered, out whitelist, out canTraverse, false))
                        {
                            return false;
                        }
                    }
                    return canTraverse[i];
                }
            }
            throw new System.Exception("Cannot request data for an uncached vehicle.");
        }

        /// <summary>
        /// Adds to the whitelist.
        /// </summary>
        public bool AddToWhiteList(IAttributeCollection attributes, Whitelist whitelist, bool filter = true)
        {
            IAttributeCollection filtered = attributes;
            if (filter)
            {
                filtered = new AttributeCollection();

                foreach (var attribute in attributes)
                {
                    if (_vehicles.IsOnProfileWhiteList(attribute.Key))
                    {
                        filtered.AddOrReplace(attribute);
                    }
                }
            }

            Whitelist cachedWhitelist;
            if (this.TryGetCached(filtered, out cachedWhitelist, false))
            {
                foreach (var key in cachedWhitelist)
                {
                    whitelist.Add(key);
                }
                return cachedWhitelist.Count > 0;
            }
            bool[] canTraverse;
            if (this.Add(filtered, out cachedWhitelist, out canTraverse, false))
            {
                foreach (var key in cachedWhitelist)
                {
                    whitelist.Add(key);
                }
                return cachedWhitelist.Count > 0;
            }

            return false;
        }

        private struct WhitelistAndFlags
        {
            /// <summary>
            /// Gets or sets the whitelist.
            /// </summary>
            public Whitelist Whitelist { get; set; }

            /// <summary>
            /// Gets or sets the cantraverse array.
            /// </summary>
            public bool[] CanTraverse { get; set; }
        }
    }
}
