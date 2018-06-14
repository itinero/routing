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

using System;
using Itinero.Attributes;
using Itinero.Profiles;
using Itinero.Profiles.Lua;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    /// Vehicle class contains routing info
    /// </summary>
    public static class Vehicle
    {
        /// <summary>
        /// Default Car
        /// </summary>
        public static readonly Profiles.Vehicle Car = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.car.lua"));

        /// <summary>
        /// Default Pedestrian
        /// </summary>
        public static readonly Profiles.Vehicle Pedestrian = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.pedestrian.lua"));

        /// <summary>
        /// Default Bicycle
        /// </summary>
        public static readonly Profiles.Vehicle Bicycle = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.bicycle.lua"));

        /// <summary>
        /// Default Moped
        /// </summary>
        public static readonly Profiles.Vehicle Moped = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.moped.lua"));

        /// <summary>
        /// Default MotorCycle
        /// </summary>
        public static readonly Profiles.Vehicle MotorCycle = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.motorcycle.lua"));

        /// <summary>
        /// Default SmallTruck
        /// </summary>
        public static readonly Profiles.Vehicle SmallTruck = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.smalltruck.lua"));

        /// <summary>
        /// Default BigTruck
        /// </summary>
        public static readonly Profiles.Vehicle BigTruck = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.bigtruck.lua"));

        /// <summary>
        /// Default BigTruck
        /// </summary>
        public static readonly Profiles.Vehicle Bus = new DynamicVehicle(VehicleExtensions.LoadEmbeddedResource("Itinero.Osm.Vehicles.bus.lua"));

        /// <summary>
        /// Registers all default vehicles.
        /// </summary>
        [Obsolete]
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

        private static Dictionary<string, bool?> _accessValues = null;

        private static Dictionary<string, bool?> GetAccessValues()
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