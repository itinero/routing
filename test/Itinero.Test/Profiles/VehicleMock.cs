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
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Test.Profiles
{
    /// <summary>
    /// A vehicle mock.
    /// </summary>
    public class VehicleMock : Vehicle
    {
        private readonly Func<IAttributeCollection, FactorAndSpeed> _getFactorAndSpeed;
        private readonly string _name;
        private readonly string[] _vehicleTypes;

        /// <summary>
        /// Creates a new mock vehicle.
        /// </summary>
        public VehicleMock(string name, string[] vehicleTypes, Func<IAttributeCollection, FactorAndSpeed> getFactorAndSpeed)
        {
            _name = name;
            _vehicleTypes = vehicleTypes;
            _getFactorAndSpeed = getFactorAndSpeed;


            this.Register(new Profile("shortest", ProfileMetric.DistanceInMeters, this.VehicleTypes, null, this));
            this.Register(new Profile(string.Empty, ProfileMetric.TimeInSeconds, this.VehicleTypes, null, this));
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return _vehicleTypes;
            }
        }

        /// <summary>
        /// Gets the name of this vehicle.
        /// </summary>
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Calculates a factor and speed.
        /// </summary>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whitelist)
        {
            return _getFactorAndSpeed(attributes);
        }

        /// <summary>
        /// Creates a mock car.
        /// </summary>
        /// <returns></returns>
        public static VehicleMock Car()
        {
            return new VehicleMock("Car", new string[] { "motor_vehicle", "vehicle" }, (a) =>
            {
                return new FactorAndSpeed()
                {
                    SpeedFactor = 1 / (50f / 3.6f),
                    Value = 1 / (50f / 3.6f),
                    Direction = 0
                };
            });
        }

        /// <summary>
        /// Creates a mock car.
        /// </summary>
        /// <returns></returns>
        public static VehicleMock Car(Func<IAttributeCollection, FactorAndSpeed> getFactorAndSpeed)
        {
            return new VehicleMock("Car", new string[] { "motorcar", "motor_vehicle", "vehicle" }, getFactorAndSpeed);
        }

        /// <summary>
        /// Creates a mock car.
        /// </summary>
        /// <returns></returns>
        public static VehicleMock Mock(string name, Func<IAttributeCollection, FactorAndSpeed> getFactorAndSpeed,
            params string[] vehicleTypes)
        {
            return new VehicleMock(name, vehicleTypes, getFactorAndSpeed);
        }
    }
}