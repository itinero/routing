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

using Itinero.Attributes;
using System.Collections.Generic;

namespace Itinero.Profiles
{
    /// <summary>
    /// Contains extensions methods related to the dynamic vehicle class.
    /// </summary>
    public static class DynamicVehicleExtensions
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
        public static bool AddToProfileWhiteList(this DynamicVehicle[] vehicles, HashSet<string> whiteList, IAttributeCollection attributes)
        {
            var traversable = false;
            for (var i = 0; i < vehicles.Length; i++)
            {
                traversable = traversable || vehicles[i].AddToProfileWhiteList(whiteList, attributes);
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
        public static bool IsOnProfileWhiteList(this DynamicVehicle[] vehicles, string key)
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
        public static bool IsOnMetaWhiteList(this DynamicVehicle[] vehicles, string key)
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
