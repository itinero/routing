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
            var vehicle = DynamicVehicle.LoadFromEmbeddedResource(typeof(DynamicVehicleTests).Assembly, "Itinero.Test.test_data.profiles.osm.car.lua").Fastest();

            // invalid highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, null, 05, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, null, 30, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, null, 30, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, null, 70, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, null, 70, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 70, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, null, 70, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 90, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, null, 90, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 90, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, null, 90, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, null, 120, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, null, 120, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "foot", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "foot", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "foot", "no");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "bicycle", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "bicycle", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "bicycle", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "primary", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 20 * 1.60934f * .75f, "highway", "primary", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests getting factor and speed.
        /// </summary>
        protected void TestFactorAndSpeed(Itinero.Profiles.Profile profile, short? direction, float? factor, float? speed, params string[] tags)
        {
            var attributesCollection = new AttributeCollection();
            for (int idx = 0; idx < tags.Length; idx = idx + 2)
            {
                attributesCollection.AddOrReplace(tags[idx], tags[idx + 1]);
            }

            var factorAndSpeed = profile.FactorAndSpeed(attributesCollection);
            if (direction != null)
            {
                Assert.AreEqual(direction.Value, factorAndSpeed.Direction);
            }
            if (factor != null)
            {
                Assert.AreEqual(factor.Value, factorAndSpeed.Value);
            }
            if (speed != null)
            {
                if (speed == 0)
                {
                    Assert.AreEqual(0, factorAndSpeed.SpeedFactor, 0.0001);
                }
                else
                {
                    Assert.AreEqual(1.0f / (speed.Value / 3.6), factorAndSpeed.SpeedFactor, 0.0001);
                }
            }
        }
    }
}