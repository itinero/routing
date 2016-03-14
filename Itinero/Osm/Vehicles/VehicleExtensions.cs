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

using Itinero.Attributes;
using System.Collections.Generic;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Contains extension methods for vehicles.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns true if any vehicle in the given array can traverse a way with the given tags.
        /// </summary>
        public static bool AnyCanTraverse(this Vehicle[] vehicles, IAttributeCollection tags)
        {
            for(var i  = 0; i < vehicles.Length; i++)
            {
                if(vehicles[i].CanTraverse(tags))
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
                if (vehicles[i].IsRelevantForMeta(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the key is relevant for profile-data.
        /// </summary>
        public static bool IsRelevantForProfile(this Vehicle[] vehicles, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return false;
            }

            for (var i = 0; i < vehicles.Length; i++)
            {
                if (vehicles[i].IsRelevantForProfile(key))
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