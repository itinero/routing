/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Attributes;
using System.Collections.Generic;
using System.IO;
using System;

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
            _profiles[profile.Name.ToLowerInvariant()] = profile;
        }

        /// <summary>
        /// Returns the profile with the given name.
        /// </summary>
        public Profile Profile(string name)
        {
            return _profiles[name.ToLowerInvariant()];
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
        public virtual Profile Shortest()
        {
            return this.Profile("shortest");
        }

        /// <summary>
        /// Gets the profile to calculate fastest routes.
        /// </summary>
        public virtual Profile Fastest()
        {
            return this.Profile(string.Empty);
        }

        /// <summary>
        /// Registers this vehicle.
        /// </summary>
        [Obsolete]
        public virtual void Register()
        {
            Vehicle.Register(this);

            foreach(var profile in _profiles)
            {
                Itinero.Profiles.Profile.Register(profile.Value);
            }
        }

        private static Dictionary<string, Vehicle> _vehicles = new Dictionary<string, Vehicle>();

        /// <summary>
        /// Registers a vehicle.
        /// </summary>
        [Obsolete]
        public static void Register(Vehicle vehicle)
        {
            _vehicles[vehicle.Name.ToLowerInvariant()] = vehicle;
        }

        /// <summary>
        /// Gets a registered vehicle.
        /// </summary>
        [Obsolete]
        public static Vehicle Get(string name)
        {
            return _vehicles[name.ToLowerInvariant()];
        }

        /// <summary>
        /// Tries to get a registred vehicle.
        /// </summary>
        [Obsolete]
        public static bool TryGet(string name, out Vehicle value)
        {
            return _vehicles.TryGetValue(name.ToLowerInvariant(), out value);
        }

        /// <summary>
        /// Gets all registered vehicles.
        /// </summary>
        /// <returns></returns>
        [Obsolete]
        public static IEnumerable<Vehicle> GetRegistered()
        {
            return _vehicles.Values;
        }

        /// <summary>
        /// Serializes this vehicle.
        /// </summary>
        public long Serialize(Stream stream)
        {
            var size = stream.WriteWithSize(this.GetType().FullName);

            size += this.DoSerialize(stream);

            return size;
        }

        /// <summary>
        /// Serializes the content of this vehicle.
        /// </summary
        protected virtual long DoSerialize(Stream stream)
        {
            return 0;
        }

        /// <summary>
        /// Deserializes a vehicle from the given stream.
        /// </summary>
        public static Vehicle Deserialize(Stream stream)
        {
            var typeName = stream.ReadWithSizeString();
            switch (typeName)
            {
                case "Itinero.Profiles.DynamicVehicle":
                    var vehicle = new DynamicVehicle(stream.ReadWithSizeString());
                    return vehicle;
            }
            if (Vehicle.CustomDeserializer != null)
            {
                var vehicle = Vehicle.CustomDeserializer(typeName, stream);
                return vehicle;
            }
            throw new Exception(string.Format("Cannot deserialize for type with name: {0}. A custom deserializer was not found.", typeName));
        }

        /// <summary>
        /// Gets parameters 
        /// </summary>
        public virtual IReadonlyAttributeCollection Parameters
        {
            get
            {
                return new AttributeCollection();
            }
        }

        /// <summary>
        /// Gets or sets a custom vehicle deserializer.
        /// </summary>
        public static Func<string, Stream, Vehicle> CustomDeserializer
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a description of this vehicle.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Name;
        }
    }
}