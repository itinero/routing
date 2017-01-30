// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.Profiles;
using Itinero.IO.Osm.Streams;

namespace Itinero.IO.Osm.Normalizer
{
    /// <summary>
    /// A default tag normalizer implementation.
    /// </summary>
    public static class DefaultTagNormalizer
    {
        /// <summary>
        /// Splits the given tags into a normalized version, profile tags, and the rest in metatags.
        /// </summary>
        public static bool Normalize(IAttributeCollection tags, IAttributeCollection profileTags, VehicleCache vehicleCache)
        {
            if (tags == null || profileTags == null || vehicleCache == null)
            {
                return false;
            }

            var normalizedTags = new HashSet<string>(new string[] { "highway", "maxspeed", "oneway", "oneway:bicycle",
                "cycleway", "junction", "access" });
            foreach(var vehicle in vehicleCache.Vehicles)
            {
                foreach(var vehicleType in vehicle.VehicleTypes)
                {
                    normalizedTags.Add(vehicleType);
                }
            }

            string highway;
            if (!tags.TryGetValue("highway", out highway))
            { // there is no highway tag, don't continue the search.
                return false;
            }

            // add the highway tag.
            profileTags.AddOrReplace("highway", highway);

            // normalize maxspeed tags.
            tags.NormalizeMaxspeed(profileTags);

            // normalize oneway tags.
            tags.NormalizeOneway(profileTags);
            tags.NormalizeOnewayBicycle(profileTags);

            // normalize cyclceway.
            tags.NormalizeCycleway(profileTags);

            // normalize junction=roundabout tag.
            tags.NormalizeJunction(profileTags);

            // normalize access tags.
            foreach (var vehicle in vehicleCache.Vehicles)
            {
                tags.NormalizeAccess(vehicleCache, vehicle, highway, profileTags);
            }

            // add whitelisted tags but only when they haven't been considered for normalization.
            foreach (var vehicle in vehicleCache.Vehicles)
            {
                foreach (var key in vehicle.ProfileWhiteList)
                {
                    var value = string.Empty;
                    if (tags.TryGetValue(key, out value))
                    {
                        if (!normalizedTags.Contains(key))
                        {
                            profileTags.AddOrReplace(key, value);
                        }
                    }
                }
            }

            return true;
        }
    }
}