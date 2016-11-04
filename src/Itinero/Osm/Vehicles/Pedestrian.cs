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
    public class Pedestrian : Vehicle
    {
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Pedestrian";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return new string[] { "foot" };
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

            var speed = 4f;
            switch (highway)
            {
                case "services":
                case "proposed":
                case "cycleway":
                case "pedestrian":
                case "steps":
                case "path":
                case "footway":
                case "living_street":
                    speed = 5;
                    break;
                case "service":
                case "track":
                case "road":
                    speed = 4.5f;
                    break;
                case "residential":
                case "unclassified":
                    speed = 4.4f;
                    break;
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    speed = 4.2f;
                    break;
                case "motorway":
                case "motorway_link":
                    return Profiles.FactorAndSpeed.NoFactor;
            }
            whiteList.Add("highway");

            // access tags.
            if (!Vehicle.InterpretAccessValues(attributes, whiteList, this.VehicleTypes, "access"))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }
            var foot = string.Empty;
            if (attributes.TryGetValue("foot", out foot))
            {
                if (foot == "no")
                {
                    return Profiles.FactorAndSpeed.NoFactor;
                }
                whiteList.Add("foot");
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