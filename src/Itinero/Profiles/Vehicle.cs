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
        /// Creates a new vehicle.
        /// </summary>
        public Vehicle()
        {
            if (!string.IsNullOrEmpty(this.Name))
            {
                this.Register(new Profile(this.Name + ".Shortest", ProfileMetric.DistanceInMeters, this.VehicleTypes, this));
                this.Register(new Profile(this.Name, ProfileMetric.TimeInSeconds, this.VehicleTypes, this));
            }
        }

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
        public virtual string[] VehicleTypes
        {
            get
            {
                return new string[] { };
            }
        }

        /// <summary>
        /// Gets a whitelist of attributes to keep as meta-data.
        /// </summary>
        public virtual HashSet<string> MetaWhiteList
        {
            get
            {
                return new HashSet<string>();
            }
        }

        /// <summary>
        /// Gets a whitelist of attributes to keep as part of the profile.
        /// </summary>
        public virtual HashSet<string> ProfileWhiteList
        {
            get
            {
                return new HashSet<string>();
            }
        }

        /// <summary>
        /// Adds a number of keys to the given whitelist when they are relevant for this vehicle.
        /// </summary>
        /// <returns>True if the edge with the given attributes is usefull for this vehicle.</returns>
        public virtual bool AddToWhiteList(IAttributeCollection attributes, Whitelist whitelist)
        {
            return this.FactorAndSpeed(attributes, whitelist).Value > 0;
        }

        /// <summary>
        /// Calculates a factor and speed and adds a keys to the given whitelist that are relevant.
        /// </summary>
        /// <returns>True if the edge with the given attributes is usefull for this vehicle.</returns>
        public abstract FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whitelist);

        /// <summary>
        /// Returns true if the two given edges are equals as far as this vehicle is concerned.
        /// </summary>
        public virtual bool Equals(IAttributeCollection attributes1, IAttributeCollection attributes2)
        {
            return attributes1.ContainsSame(attributes2);
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
            return this.Profile(this.Name);
        }


        private static Dictionary<string, Vehicle> _vehicles = new Dictionary<string, Vehicle>();

        /// <summary>
        /// Registers a vehicle.
        /// </summary>
        public static void Register(Vehicle vehicle)
        {
            _vehicles[vehicle.Name] = vehicle;
        }

        /// <summary>
        /// Gets a registered vehicle.
        /// </summary>
        public static Vehicle GetRegistered(string name)
        {
            return _vehicles[name];
        }

        /// <summary>
        /// Tries to get a registred vehicle.
        /// </summary>
        public static bool TryGet(string name, out Vehicle value)
        {
            return _vehicles.TryGetValue(name, out value);
        }

        /// <summary>
        /// Gets all registered vehicles.
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<Vehicle> GetRegistered()
        {
            return _vehicles.Values;
        }
    }
}