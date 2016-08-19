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

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Represents a pedestrian
    /// </summary>
    public class Pedestrian : Vehicle
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Pedestrian()
        {
            AccessibleTags.Add("service", string.Empty);
            AccessibleTags.Add("services", string.Empty);
            AccessibleTags.Add("steps", string.Empty);
            AccessibleTags.Add("footway", string.Empty);
            AccessibleTags.Add("cycleway", string.Empty);
            AccessibleTags.Add("path", string.Empty);
            AccessibleTags.Add("road", string.Empty);
            AccessibleTags.Add("track", string.Empty);
            AccessibleTags.Add("pedestrian", string.Empty);
            AccessibleTags.Add("living_street", string.Empty);
            AccessibleTags.Add("residential", string.Empty);
            AccessibleTags.Add("unclassified", string.Empty);
            AccessibleTags.Add("secondary", string.Empty);
            AccessibleTags.Add("secondary_link", string.Empty);
            AccessibleTags.Add("primary", string.Empty);
            AccessibleTags.Add("primary_link", string.Empty);
            AccessibleTags.Add("tertiary", string.Empty);
            AccessibleTags.Add("tertiary_link", string.Empty);

            VehicleTypes.Add("foot");
        }

        /// <summary>
        /// Returns true if the vehicle is allowed on the way represented by these tags
        /// </summary>
        protected override bool IsVehicleAllowed(IAttributeCollection tags, string highwayType)
        {
            if (!tags.InterpretAccessValues(VehicleTypes, "access"))
            {
                return false;
            }

            var foot = string.Empty;
            if (tags.TryGetValue("foot", out foot))
            {
                if (foot == "designated")
                {
                    return true; // designated foot
                }
                if (foot == "yes")
                {
                    return true; // yes for foot
                }
                if (foot == "no")
                {
                    return false; // no for foot
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
                    return 5f;
                case "track":
                case "road":
                    return 4.5f;
                case "residential":
                case "unclassified":
                    return 4.4f;
                case "motorway":
                case "motorway_link":
                    return 4.3f;
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    return 4.2f;
                default:
                    return 4f;
            }
        }

        /// <summary>
        /// Returns true if the edge is one way forward, false if backward, null if bidirectional.
        /// </summary>
        public override bool? IsOneWay(IAttributeCollection tags)
        {
            return null;
        }

        /// <summary>
        /// Returns the maximum possible speed this vehicle can achieve in Km/h.
        /// </summary>
        public override float MaxSpeed()
        {
            return 5;
        }

        /// <summary>
        /// Returns the minimum speed in Km/h.
        /// </summary>
        public override float MinSpeed()
        {
            return 3;
        }

        /// <summary>
        /// Returns a unique name this vehicle type.
        /// </summary>
        public override string UniqueName
        {
            get { return "Pedestrian"; }
        }

        /// <summary>
        /// Gets all profiles for this vehicle.
        /// </summary>
        /// <returns></returns>
        public override Profile[] GetProfiles()
        {
            return new Profile[]
            {
                this.Fastest(),
                this.Shortest(),
                this.Shortcuts()
            };
        }

        /// <summary>
        /// Returns a profile specifically for pedestrians that uses all shortcuts added as much as possible.
        /// </summary>
        /// <returns></returns>
        public Profile Shortcuts()
        {
            return new Profiles.PedestrianShortcuts(this);
        }
    }
}