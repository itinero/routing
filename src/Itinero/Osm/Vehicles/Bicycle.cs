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

using Itinero.Profiles;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Represents the default OSM bicycle profile.
    /// </summary>
    public class Bicycle : DynamicVehicle
    {
        /// <summary>
        /// Creates a new bicycle.
        /// </summary>
        public Bicycle()
            : base(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.Lua.bicycle.lua"))
        {

        }

        /// <summary>
        /// Gets the balanced profile.
        /// </summary>
        /// <returns></returns>
        public IProfileInstance Balanced()
        {
            return this.Profile(this.Name + ".balanced");
        }

        /// <summary>
        /// Gets the cycle network profile.
        /// </summary>
        /// <returns></returns>
        public IProfileInstance Networks()
        {
            return this.Profile(this.Name + ".networks");
        }
    }
}