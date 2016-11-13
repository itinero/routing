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

namespace Itinero.IO.Osm.Normalizer
{
    /// <summary>
    /// A default tag normalizer implementation.
    /// </summary>
    public class DefaultTagNormalizer : ITagNormalizer
    {
        public static ITagNormalizer Default = new DefaultTagNormalizer();
        
        /// <summary>
        /// Splits the given tags into a normalized version, profile tags, and the rest in metatags.
        /// </summary>
        public virtual bool Normalize(AttributeCollection tags, AttributeCollection profileTags, AttributeCollection metaTags, IEnumerable<Vehicle> vehicles, Whitelist whitelist)
        {
            string highway;
            if (!tags.TryGetValue("highway", out highway))
            { // there is no highway tag, don't continue the search.
                return false;
            }

            // normalize maxspeed tags.
            tags.NormalizeMaxspeed(profileTags, metaTags);

            // normalize oneway tags.
            tags.NormalizeOneway(profileTags, metaTags);
            tags.NormalizeOnewayBicycle(profileTags, metaTags);

            // normalize cyclceway.
            tags.NormalizeCycleway(profileTags, metaTags);

            // normalize junction=roundabout tag.
            tags.NormalizeJunction(profileTags, metaTags);

            switch (highway)
            {
                case "motorway":
                case "motorway_link":
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    profileTags.AddOrReplace("highway", highway);
                    break;
                case "secondary":
                case "secondary_link":
                case "tertiary":
                case "tertiary_link":
                case "unclassified":
                case "residential":
                case "road":
                case "service":
                case "services":
                case "living_street":
                case "track":
                    profileTags.AddOrReplace("highway", highway);
                    break;
                case "cycleway":
                    profileTags.AddOrReplace("highway", highway);
                    break;
                case "path":
                    profileTags.AddOrReplace("highway", highway);
                    break;
                case "pedestrian":
                case "footway":
                case "steps":
                    tags.NormalizeRamp(profileTags, metaTags, false);
                    profileTags.AddOrReplace("highway", highway);
                    break;
            }

            // normalize access tags.
            foreach (var vehicle in vehicles)
            {
                tags.NormalizeAccess(vehicle, highway, profileTags);
            }

            // add whitelisted tags.
            foreach (var key in whitelist)
            {
                var value = string.Empty;
                if (tags.TryGetValue(key, out value))
                {
                    profileTags.AddOrReplace(key, value);
                }
            }
            foreach(var vehicle in vehicles)
            {
                foreach (var key in vehicle.ProfileWhiteList)
                {
                    var value = string.Empty;
                    if (tags.TryGetValue(key, out value))
                    {
                        profileTags.AddOrReplace(key, value);
                    }
                }
            }

            return true;
        }
    }
}