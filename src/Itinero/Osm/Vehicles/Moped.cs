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
    /// Represents the default OSM moped profile.
    /// </summary>
    public class Moped : Vehicle
    {
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public override string Name
        {
            get
            {
                return "Moped";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return new string[] { "vehicle", "motor_vehicle", "moped" };
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

            var speed = 70.0f;
            var canstopon = true;
            switch (highway)
            {
                case "services":
                case "living_street":
                    speed = 5;
                    break;
                case "service":
                case "track":
                case "road":
                case "residential":
                case "unclassified":
                case "tertiary":
                case "tertiary_link":
                case "secondary":
                case "secondary_link":
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    speed = 40;
                    break;
                default:
                    return Profiles.FactorAndSpeed.NoFactor;
            }
            whiteList.Add("highway");

            // get max-speed tag if any.
            var maxSpeed = 0.0f;
            if (attributes.TryGetMaxSpeed(out maxSpeed))
            {
                whiteList.Add("maxspeed");
                if (speed > 40)
                {
                    speed = 40;
                }
                else
                {
                    speed = maxSpeed * 0.75f;
                }
            }

            // access tags.
            if (!Vehicle.InterpretAccessValues(attributes, whiteList, this.VehicleTypes, "access"))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }

            // oneway restrictions.
            short direction = 0; // 0=bidirectional, 1=forward, 2=backward
            string oneway;
            string junction;
            if (attributes.TryGetValue("junction", out junction))
            {
                if (junction == "roundabout")
                {
                    whiteList.Add("junction");
                    direction = 1;
                }
            }
            if (attributes.TryGetValue("oneway", out oneway))
            {
                if (oneway == "yes")
                {
                    direction = 1;
                }
                else if (oneway == "no")
                { // explicitly tagged as not oneway.

                }
                else
                {
                    direction = 2;
                }
                whiteList.Add("oneway");
            }

            speed = speed / 3.6f; // to m/s
            if (!canstopon)
            { // add canstop on info to direction.
                direction += 3;
            }

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
