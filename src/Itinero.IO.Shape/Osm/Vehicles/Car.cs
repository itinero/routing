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

using System;
using Itinero.Attributes;
using Itinero.IO.Shape.Vehicles;
using System.Collections.Generic;

namespace Itinero.IO.Shape.Osm.Vehicles
{
    /// <summary>
    /// A vehicle profile for OSM-based shapefiles.
    /// </summary>
    public class Car : Vehicle
    {
        /// <summary>
        /// Gets the unique name.
        /// </summary>
        public override string UniqueName
        {
            get
            {
                return "OSM.Car";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override List<string> VehicleTypes
        {
            get
            {
                return new List<string>(new[] { "motor_vehicle", "car" });
            }
        }

        /// <summary>
        /// Returns true if an attribute with the given key is relevant for the profile.
        /// </summary>
        public override bool IsRelevantForProfile(string key)
        {
            return key == "highway" || key == "oneway";
        }

        /// <summary>
        /// Returns the maximum speed.
        /// </summary>
        /// <returns></returns>
        public override float MaxSpeed()
        {
            return 130;
        }

        /// <summary>
        /// Returns the minimum speed.
        /// </summary>
        /// <returns></returns>
        public override float MinSpeed()
        {
            return 5;
        }

        /// <summary>
        /// Returns the probable speed.
        /// </summary>
        public override float ProbableSpeed(IAttributeCollection tags)
        {
            string highwayType;
            if (tags.TryGetValue("highway", out highwayType))
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
                    case "service":
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
            return 0;
        }

        /// <summary>
        /// Returns true if the edge is oneway forward, false if backward, null if bidirectional.
        /// </summary>
        protected override bool? IsOneWay(IAttributeCollection tags)
        {
            string oneway;
            if (tags.TryGetValue("oneway", out oneway))
            {
                if (oneway == "yes")
                {
                    return true;
                }
                else if (oneway == "no")
                { // explicitly tagged as not oneway.
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
    }
}