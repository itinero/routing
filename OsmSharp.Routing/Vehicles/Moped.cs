// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Units.Speed;

namespace OsmSharp.Routing.Vehicles
{
    /// <summary>
    /// Represents a moped
    /// </summary>
    public class Moped : MotorVehicle
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Moped()
        {
            AccessibleTags.Remove("motorway");
            AccessibleTags.Remove("motorway_link");

            VehicleTypes.Add("moped");
        }

        /// <summary>
        /// Returns the maximum possible speed this vehicle can achieve.
        /// </summary>
        /// <returns></returns>
        public override KilometerPerHour MaxSpeed()
        {
            return 40;
        }

        /// <summary>
        /// Returns a unique name this vehicle type.
        /// </summary>
        public override string UniqueName
        {
            get { return "Moped"; }
        }
    }
}
