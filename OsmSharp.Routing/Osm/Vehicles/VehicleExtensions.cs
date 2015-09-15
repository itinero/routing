// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
namespace OsmSharp.Routing.Osm.Vehicles
{
    /// <summary>
    /// Contains extension methods for vehicles.
    /// </summary>
    public static class VehicleExtensions
    {
        /// <summary>
        /// Returns true if any vehicle in the given array can traverse a way with the given tags.
        /// </summary>
        /// <returns></returns>
        public static bool AnyCanTraverse(this Vehicle[] vehicles, TagsCollectionBase tags)
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
        /// <returns></returns>
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
        /// <returns></returns>
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
    }
}