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

using System.Collections.Generic;

namespace Itinero.Profiles
{
    /// <summary>
    /// Represents a vehicle.
    /// </summary>
    public abstract class Vehicle
    {
        private readonly Dictionary<string, Profile> _profiles = new Dictionary<string, Profiles.Profile>();
        
        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public abstract string Name
        {
            get;
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public abstract string[] VehicleTypes
        {
            get;
        }

        /// <summary>
        /// Registers a profile.
        /// </summary>
        public void Register(Profile profile)
        {
            _profiles[profile.Name] = profile;
        }

        /// <summary>
        /// Returns the profile with the given name.
        /// </summary>
        public Profile Profile(string name)
        {
            return _profiles[name];
        }

        /// <summary>
        /// Returns the profiles for this vehicle.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Profile> GetProfiles()
        {
            return _profiles.Values;
        }

        /// <summary>
        /// Gets the profile to calculate shortest routes.
        /// </summary>
        /// <returns></returns>
        public Profile Shortest()
        {
            return this.Profile(this.Name + ".shortest");
        }

        /// <summary>
        /// Gets the profile to calculate fastest routes.
        /// </summary>
        /// <returns></returns>
        public Profile Fastest()
        {
            return this.Profile(this.Name + ".fastest");
        }
    }
}