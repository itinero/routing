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
using Itinero.Profiles;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Represents a bicycle
    /// </summary>
    public class Bicycle : Vehicle
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Bicycle()
        {
            AccessibleTags.Add("steps", string.Empty); // only when there is a ramp.
            AccessibleTags.Add("service", string.Empty);
            AccessibleTags.Add("cycleway", string.Empty);
            AccessibleTags.Add("path", string.Empty);
            AccessibleTags.Add("road", string.Empty);
            AccessibleTags.Add("track", string.Empty);
            AccessibleTags.Add("living_street", string.Empty);
            AccessibleTags.Add("residential", string.Empty);
            AccessibleTags.Add("unclassified", string.Empty);
            AccessibleTags.Add("secondary", string.Empty);
            AccessibleTags.Add("secondary_link", string.Empty);
            AccessibleTags.Add("primary", string.Empty);
            AccessibleTags.Add("primary_link", string.Empty);
            AccessibleTags.Add("tertiary", string.Empty);
            AccessibleTags.Add("tertiary_link", string.Empty);

            VehicleTypes.Add("bicycle");
        }

        /// <summary>
        /// Returns true if the vehicle is allowed on the way represented by these tags
        /// </summary>
        protected override bool IsVehicleAllowed(IAttributeCollection tags, string highwayType)
        {
            // do the designated tags.
            var bicycle = string.Empty;
            if (tags.TryGetValue("bicycle", out bicycle))
            {
                if (bicycle == "designated")
                {
                    return true; // designated bicycle
                }
                if (bicycle == "yes")
                {
                    return true; // yes for bicycle
                }
                if (bicycle == "no")
                {
                    return false; //  no for bicycle
                }
            }
            if (highwayType == "steps")
            {
                if (tags.Contains("ramp", "yes"))
                {
                    return true;
                }
                return false;
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
                    return this.MaxSpeed();
                case "track":
                case "road":
                    return 30f;
                case "residential":
                case "unclassified":
                    return 50f;
                case "motorway":
                case "motorway_link":
                    return 120f;
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    return 90f;
                default:
                    return 70f;
            }
        }

        /// <summary>
        /// Returns true if the given key is relevant for this profile.
        /// </summary>
        public override bool IsRelevantForProfile(string key)
        {
            if(base.IsRelevantForProfile(key))
            {
                return true;
            }
            return key == "ramp";
        }

        /// <summary>
        /// Returns true if the edge is one way forward, false if backward, null if bidirectional.
        /// </summary>
        public override bool? IsOneWay(IAttributeCollection tags)
        {
            string oneway;
            string highway;
            if (tags.TryGetValue("oneway:bicycle", out oneway))
            {
                if (oneway == "yes")
                {
                    return true;
                }
                else if (oneway == "no")
                {
                    return null;
                }
                return false;
            }

            if (tags.TryGetValue("oneway", out oneway) &&
                (tags.TryGetValue("highway", out highway) &&
                 (highway == "cycleway")))
            {
                if (oneway == "yes")
                {
                    return true;
                }
                else if (oneway == "no")
                {
                    return null;
                }
                return false;
            }

            string junction;
            if (tags.TryGetValue("junction", out junction))
            {
                if (junction == "roundabout")
                {
                    return true;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the maximum possible speed this vehicle can achieve.
        /// </summary>
        public override float MaxSpeed()
        {
            return 15;
        }

        /// <summary>
        /// Returns the minimum speed.
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
            get { return "Bicycle"; }
        }

        /// <summary>
        /// Gets all profiles for this vehicle.
        /// </summary>
        public override Profile[] GetProfiles()
        {
            return new Profile[]
            {
                this.Fastest(),
                this.Shortest(),
                this.Balanced()
            };
        }

        /// <summary>
        /// Returns a profile specifically for bicycle that tries to balance between bicycle infrastructure and fastest route.
        /// </summary>
        public Itinero.Profiles.Profile Balanced()
        {
            return new Profiles.BicycleBalanced(this);
        }
    }
}