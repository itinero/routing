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