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

namespace Itinero.Profiles
{
    /// <summary>
    /// Contains extension methods related to the vehicle class.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns true if the given vehicle can traverse a way with the given attributes.
        /// </summary>
        public static bool CanTraverse(this Vehicle vehicle, IAttributeCollection attributes)
        {
            foreach (var profile in vehicle.GetProfiles())
            {
                var factorAndSpeed = profile.FactorAndSpeed(attributes);
                if (factorAndSpeed.Value != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds all the keys to the whitelist if they are relevante for the profiles.
        /// </summary>
        public static bool AddToWhiteList(this Vehicle[] vehicles, IAttributeCollection attributes, Whitelist whiteList)
        {
            var traversable = false;
            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].AddToWhiteList(attributes, whiteList))
                {
                    traversable = true;
                }
            }
            return traversable;
        }

        /// <summary>
        /// Adds all the keys to the whitelist if they are relevante for the profiles.
        /// </summary>
        public static bool AddToWhiteList(this Vehicle[] vehicles, IAttributeCollection attributes, Whitelist whiteList, bool[] canTraverse)
        {
            var traversable = false;
            for (var i = 0; i < vehicles.Length; i++)
            {
                canTraverse[i] = false;
                if (vehicles[i].AddToWhiteList(attributes, whiteList))
                {
                    traversable = true;
                    canTraverse[i] = true;
                }
            }
            return traversable;
        }

        /// <summary>
        /// Returns true if any vehicle in the given array can traverse a way with the given tags.
        /// </summary>
        public static bool AnyCanTraverse(this Vehicle[] vehicles, IAttributeCollection tags)
        {
            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].CanTraverse(tags))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given key is on any of the vehicles profile whitelist.
        /// </summary>
        public static bool IsOnProfileWhiteList(this Vehicle[] vehicles, string key)
        {
            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].ProfileWhiteList.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given key is on any of the vehicles meta whitelist.
        /// </summary>
        public static bool IsOnMetaWhiteList(this Vehicle[] vehicles, string key)
        {
            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].MetaWhiteList.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }
    }
}