// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

namespace Itinero.Osm.Vehicles
{

    /// <summary>
    /// Represents a bus that is not acting as a public service.
    /// </summary>
    /// <remarks>http://wiki.openstreetmap.org/wiki/Key:tourist_bus</remarks>
    public class Bus : MotorVehicle
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public Bus()
        {
            // http://wiki.openstreetmap.org/wiki/Key:tourist_bus
            VehicleTypes.Add("tourist_bus");
        }

        /// <summary>
        /// Returns a unique name this vehicle type.
        /// </summary>
        public override string UniqueName
        {
            get { return "Bus"; }
        }
    }
}