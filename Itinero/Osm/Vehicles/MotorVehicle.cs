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

namespace Itinero.Osm.Vehicles
{

    /// <summary>
    /// Represents a MotorVehicle
    /// </summary>
    public abstract class MotorVehicle : Vehicle
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        protected MotorVehicle()
        {
            AccessibleTags.Add("road", string.Empty);
            AccessibleTags.Add("living_street", string.Empty);
            AccessibleTags.Add("residential", string.Empty);
            AccessibleTags.Add("unclassified", string.Empty);
            AccessibleTags.Add("secondary", string.Empty);
            AccessibleTags.Add("secondary_link", string.Empty);
            AccessibleTags.Add("primary", string.Empty);
            AccessibleTags.Add("primary_link", string.Empty);
            AccessibleTags.Add("tertiary", string.Empty);
            AccessibleTags.Add("tertiary_link", string.Empty);
            AccessibleTags.Add("trunk", string.Empty);
            AccessibleTags.Add("trunk_link", string.Empty);
            AccessibleTags.Add("motorway", string.Empty);
            AccessibleTags.Add("motorway_link", string.Empty);

            VehicleTypes.Add("vehicle"); // a motor vehicle is a generic vehicle.
            VehicleTypes.Add("motor_vehicle"); // ... and also a generic motor vehicle.
        }

        /// <summary>
        /// Returns true if the vehicle is allowed on the way represented by these tags
        /// </summary>
        protected override bool IsVehicleAllowed(IAttributeCollection tags, string highwayType)
        {
            string motorVehicle = string.Empty;
            if (tags.TryGetValue("motor_vehicle", out motorVehicle))
            {
                if (motorVehicle == "no")
                {
                    return false;
                }
            }
            return AccessibleTags.ContainsKey(highwayType);
        }

        /// <summary>
        /// Returns the Max Speed for the highwaytype in Km/h.
        /// 
        /// This does not take into account how fast this vehicle can go just the max possible speed.
        /// </summary>
        public override float MaxSpeedAllowed(string highwayType)
        {
            switch (highwayType)
            {
                case "services":
                case "proposed":
                case "cycleway":
                case "pedestrian":
                case "steps":
                case "path":
                case "footway":
                case "living_street":
                    return 5;
                case "track":
                case "road":
                    return 30;
                case "residential":
                case "unclassified":
                    return 50;
                case "motorway":
                case "motorway_link":
                    return 120;
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    return 90;
                default:
                    return 70;
            }
        }

        /// <summary>
        /// Returns true if the vehicle represented by this profile can stop on the edge with the given attributes.
        /// </summary>
        public override bool CanStopOn(IAttributeCollection tags)
        {
            var highwayType = string.Empty;
            if (this.TryGetHighwayType(tags, out highwayType))
            {
                if (!string.IsNullOrWhiteSpace(highwayType))
                {
                    if(highwayType.ToLowerInvariant().Equals("motorway") ||
                       highwayType.ToLowerInvariant().Equals("motorway_link"))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the maximum possible speed this vehicle can achieve.
        /// </summary>
        public override float MaxSpeed()
        {
            return 200;
        }

        /// <summary>
        /// Returns the minimum possible speed.
        /// </summary>
        public override float MinSpeed()
        {
            return 30;
        }
    }
}