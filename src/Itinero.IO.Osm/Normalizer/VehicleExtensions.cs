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
using Itinero.IO.Osm.Profiles;
using OsmSharp.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itinero.IO.Osm.Normalizer
{
    /// <summary>
    /// Contains extension methods for vehicles.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns true if the given vehicle can traverse a way with the given tags.
        /// </summary>
        public static bool CanTraverse(this Vehicle vehicle, IAttributeCollection tags)
        {
            foreach(var profile in vehicle.GetProfiles())
            {
                var factorAndSpeed = profile.FactorAndSpeed(tags);
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
        public static bool AddToProfileWhiteList(this Vehicle[] vehicles, HashSet<string> whiteList, TagsCollectionBase tags)
        {
            var traversable = false;
            for (var i = 0; i < vehicles.Length; i++)
            {
                traversable = traversable || vehicles[i].AddToProfileWhiteList(whiteList, tags);
            }
            return traversable;
        }

        /// <summary>
        /// Adds all the keys to the whitelist if they are relevante for the profiles.
        /// </summary>
        public static bool AddToProfileWhiteList(this Vehicle[] vehicles, HashSet<string> whiteList, IAttributeCollection tags)
        {
            var traversable = false;
            for (var i = 0; i < vehicles.Length; i++)
            {
                traversable = traversable || vehicles[i].AddToProfileWhiteList(whiteList, tags);
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

        /// <summary>
        /// Returns true if the key is relevant for meta-data.
        /// </summary>
        public static bool IsRelevantForMeta(this Vehicle[] vehicles, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].MetaWhiteList.Contains(key))
                {
                    return true;
                }
            }
            return false;
        }

        private static Dictionary<string, bool?> _accessValues = null;

        private static Dictionary<string, bool?> GetAccessValues()
        {
            if (_accessValues == null)
            {
                _accessValues = new Dictionary<string, bool?>();
                lock (_accessValues)
                {
                    _accessValues.Add("private", false);
                    _accessValues.Add("yes", true);
                    _accessValues.Add("no", false);
                    _accessValues.Add("permissive", true);
                    _accessValues.Add("destination", true);
                    _accessValues.Add("customers", false);
                    _accessValues.Add("agricultural", null);
                    _accessValues.Add("forestry", null);
                    _accessValues.Add("designated", true);
                    _accessValues.Add("public", true);
                    _accessValues.Add("discouraged", null);
                    _accessValues.Add("delivery", true);
                    _accessValues.Add("use_sidepath", false);
                }
            }
            return _accessValues;
        }

        /// <summary>
        /// Interprets a given access tag value.
        /// </summary>
        public static bool? InterpretAccessValue(this IAttributeCollection tags, string key)
        {
            string accessValue;
            if (tags.TryGetValue(key, out accessValue))
            {
                bool? value;
                if (VehicleExtensions.GetAccessValues().TryGetValue(accessValue, out value))
                {
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Interpreters a series of access tags for a series of vehicle types.
        /// </summary>
        public static bool InterpretAccessValues(this IAttributeCollection tags, IEnumerable<string> keys, params string[] rootKeys)
        {
            bool? value = null;
            for (var i = 0; i < rootKeys.Length; i++)
            {
                var currentAccess = tags.InterpretAccessValue(rootKeys[i]);
                if (currentAccess != null)
                {
                    value = currentAccess;
                }
            }
            foreach (var key in keys)
            {
                var currentAccess = tags.InterpretAccessValue(key);
                if (currentAccess != null)
                {
                    value = currentAccess;
                }
            }
            return !value.HasValue || value.Value;
        }
    }
}