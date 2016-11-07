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
using System.Collections.Generic;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Vehicle class contains routing info
    /// </summary>
    public abstract class Vehicle : Profiles.Vehicle
    {
        /// <summary>
        /// Default Car
        /// </summary>
        public static readonly Car Car = new Car();

        /// <summary>
        /// Default Pedestrian
        /// </summary>
        public static readonly Pedestrian Pedestrian = new Pedestrian();

        /// <summary>
        /// Default Bicycle
        /// </summary>
        public static readonly Bicycle Bicycle = new Bicycle();

        /// <summary>
        /// Default Moped
        /// </summary>
        public static readonly Moped Moped = new Moped();

        /// <summary>
        /// Default MotorCycle
        /// </summary>
        public static readonly MotorCycle MotorCycle = new MotorCycle();

        /// <summary>
        /// Default SmallTruck
        /// </summary>
        public static readonly SmallTruck SmallTruck = new SmallTruck();

        /// <summary>
        /// Default BigTruck
        /// </summary>
        public static readonly BigTruck BigTruck = new BigTruck();

        /// <summary>
        /// Default BigTruck
        /// </summary>
        public static readonly Bus Bus = new Bus();

        /// <summary>
        /// Registers all default vehicles.
        /// </summary>
        public static void RegisterVehicles()
        {
            Car.Register();
            Pedestrian.Register();
            Bicycle.Register();
            Moped.Register();
            MotorCycle.Register();
            SmallTruck.Register();
            BigTruck.Register();
            Bus.Register();
        }

        /// <summary>
        /// Registers all profiles in this vehicle.
        /// </summary>
        public void Register()
        {
            foreach (var profile in this.GetProfiles())
            {
                Profiles.Profile.Register(profile);
            }
        }

        private static Dictionary<string, bool?> _accessValues = null;

        protected static Dictionary<string, bool?> GetAccessValues()
        {
            if (_accessValues == null)
            {
                _accessValues = new Dictionary<string, bool?>();
                lock (_accessValues)
                {
                    _accessValues.Add("private", false);
                    _accessValues.Add("yes", true);
                    _accessValues.Add("no", false);
                    _accessValues.Add("permissive", true);
                    _accessValues.Add("destination", true);
                    _accessValues.Add("customers", false);
                    _accessValues.Add("agricultural", null);
                    _accessValues.Add("forestry", null);
                    _accessValues.Add("designated", true);
                    _accessValues.Add("public", true);
                    _accessValues.Add("discouraged", null);
                    _accessValues.Add("delivery", true);
                    _accessValues.Add("use_sidepath", false);
                }
            }
            return _accessValues;
        }

        /// <summary>
        /// Interprets a given access tag value.
        /// </summary>
        public static bool? InterpretAccessValue(IAttributeCollection attributes, string key)
        {
            string accessValue;
            if (attributes.TryGetValue(key, out accessValue))
            {
                bool? value;
                if (Vehicle.GetAccessValues().TryGetValue(accessValue, out value))
                {
                    return value;
                }
            }
            return null;
        }

        /// <summary>
        /// Interpreters a series of access tags for a series of vehicle types.
        /// </summary>
        public static bool InterpretAccessValues(IAttributeCollection attributes, Whitelist whiteList, IEnumerable<string> keys, params string[] rootKeys)
        {
            bool? value = null;
            var usedKey = string.Empty;
            for (var i = 0; i < rootKeys.Length; i++)
            {
                var currentAccess = Vehicle.InterpretAccessValue(attributes, rootKeys[i]);
                if (currentAccess != null)
                {
                    value = currentAccess;
                    usedKey = rootKeys[i];
                }
            }
            foreach (var key in keys)
            {
                var currentAccess = Vehicle.InterpretAccessValue(attributes, key);
                if (currentAccess != null)
                {
                    value = currentAccess;
                    usedKey = key;
                }
            }
            if (!string.IsNullOrWhiteSpace(usedKey))
            {
                whiteList.Add(usedKey);
            }

            return !value.HasValue || value.Value;
        }
    }
}