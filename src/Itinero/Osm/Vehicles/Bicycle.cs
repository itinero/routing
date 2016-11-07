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
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Represents the default OSM pedestrian profile.
    /// </summary>
    public class Bicycle : Vehicle
    {
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Bicycle";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return new string[] { "vehicle", "bicycle" };
            }
        }

        /// <summary>
        /// Gets a whitelist of attributes to keep as meta-data.
        /// </summary>
        public override HashSet<string> MetaWhiteList
        {
            get
            {
                return new HashSet<string>(new[] { "name" });
            }
        }

        /// <summary>
        /// Gets a whitelist of attributes to keep as part of the profile.
        /// </summary>
        public override HashSet<string> ProfileWhiteList
        {
            get
            {
                return new HashSet<string>();
            }
        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
        public sealed override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whiteList)
        {
            string highway = string.Empty;
            if (attributes == null ||
                !attributes.TryGetValue("highway", out highway))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }

            var speed = 15f;
            var access = true;
            switch (highway)
            {
                case "services":
                case "proposed":
                case "cycleway":
                case "path":
                case "living_street":
                case "service":
                case "track":
                case "road":
                case "residential":
                case "unclassified":
                case "tertiary":
                case "tertiary_link":
                case "secondary":
                case "secondary_link":
                case "primary":
                case "primary_link":
                case "trunk":
                case "trunk_link":
                    speed = 15f;
                    break;
                default:
                    access = false;
                    break;
            }
            whiteList.Add("highway");

            // access tags.
            if (!Vehicle.InterpretAccessValues(attributes, whiteList, this.VehicleTypes, "access"))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }            
            // do the designated tags.
            var bicycle = string.Empty;
            if (attributes.TryGetValue("bicycle", out bicycle))
            {
                if (bicycle == "no")
                {
                    whiteList.Add("bicycle");
                    return Profiles.FactorAndSpeed.NoFactor;
                }
                else if (bicycle == "yes" || 
                    bicycle == "designated")
                {
                    whiteList.Add("bicycle");
                    access = true;
                }
            }
            if (highway == "steps")
            {
                if (!attributes.Contains("ramp", "yes"))
                {
                    return Profiles.FactorAndSpeed.NoFactor;
                }
                access = true;
                whiteList.Add("ramp");
            }
            if (!access)
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }

            short direction = 0;

            speed = speed / 3.6f; // to m/s

            return new Profiles.FactorAndSpeed()
            {
                Constraints = null,
                Direction = direction,
                SpeedFactor = 1.0f / speed,
                Value = 1.0f / speed
            };
        }
    }
}