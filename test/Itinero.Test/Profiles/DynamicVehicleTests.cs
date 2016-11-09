// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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
using NUnit.Framework;

namespace Itinero.Test.Profiles
{
    /// <summary>
    /// Contains tests for extensions.
    /// </summary>
    [TestFixture]
    public class DynamicVehicleTests
    {
        /// <summary>
        /// Tests loading and running the OSM car profile.
        /// </summary>
        [Test]
        public void TestOSMCar()
        {
            var car = DynamicVehicle.LoadFromEmbeddedResource(typeof(DynamicVehicleTests).Assembly, "Itinero.Test.test_data.profiles.osm.car.lua");

            var test = car.Fastest().FactorAndSpeed(new AttributeCollection(new Attribute("highway", "residential")));
        }
    }
}