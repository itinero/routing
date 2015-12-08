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

using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Profiles
{
    /// <summary>
    /// A profile mock.
    /// </summary>
    public class MockProfile : Profile
    {
        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        private MockProfile(string name, Func<TagsCollectionBase, Speed> getSpeed, 
            HashSet<string> vehicleTypes)
            : base(name, getSpeed, () => new Speed() { Value = 5, Direction = 0 }, x => true, (e1, e2) => true, vehicleTypes, ProfileMetric.Custom)
        {

        }

        /// <summary>
        /// Creates a mock car profile.
        /// </summary>
        /// <returns></returns>
        public static MockProfile CarMock()
        {
            return MockProfile.Mock("CarMock", x => new Speed() 
            {
                Value = 50f / 3.6f,
                Direction = 0
            }, VehicleTypes.MotorVehicle, VehicleTypes.Vehicle);
        }

        /// <summary>
        /// Creates a mock car profile.
        /// </summary>
        /// <returns></returns>
        public static MockProfile CarMock(Func<TagsCollectionBase, Speed> getSpeed)
        {
            return MockProfile.Mock("CarMock", getSpeed, VehicleTypes.MotorVehicle, VehicleTypes.Vehicle);
        }

        /// <summary>
        /// Creates a mock car profile.
        /// </summary>
        /// <returns></returns>
        public static MockProfile Mock(string name, Func<TagsCollectionBase, Speed> getSpeed,
            params string[] vehicleTypes)
        {
            var vehicleTypesSet = new HashSet<string>();
            foreach(var vehicleType in vehicleTypes)
            {
                vehicleTypesSet.Add(vehicleType);
            }

            return new MockProfile(name, getSpeed, vehicleTypesSet);
        }
    }
}
