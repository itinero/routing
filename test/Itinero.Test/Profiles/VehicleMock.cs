// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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
                    SpeedFactor = 1 / 50f / 3.6f,
                    Value = 1 / 50f / 3.6f,
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