// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using NUnit.Framework;
using Itinero;
using Itinero.Osm.Vehicles;
using Itinero.Attributes;

namespace Itinero.Test.Osm
{
    /// <summary>
    /// Contains test for the default OSM vehicle classes.
    /// </summary>
    [TestFixture]
    public class VehicleTests
    {
        /// <summary>
        /// Tests the pedestrian.
        /// </summary>
        [Test]
        public void TestPedestrian()
        {
            var vehicle = Vehicle.Pedestrian.Fastest();

            // invalid highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "foot", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "foot", "yes");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "unclassified", "foot", "no");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "bicycle", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "bicycle", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "bicycle", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 4, "highway", "unclassified", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests the bicycle.
        /// </summary>
        [Test]
        public void TestBicycle()
        {
            var vehicle = Vehicle.Bicycle.Fastest();

            // invalid highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "unclassified", "foot", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "unclassified", "foot", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "unclassified", "foot", "no");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "footway", "bicycle", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "footway", "bicycle", "yes");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "unclassified", "bicycle", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "primary", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "primary", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests the moped.
        /// </summary>
        [Test]
        public void TestMoped()
        {
            var vehicle = Vehicle.Moped.Fastest();

            // invalid highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, null, 05, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "foot", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "foot", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "foot", "no");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "bicycle", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "bicycle", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "bicycle", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "primary", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 20 * 1.60934f * .75f, "highway", "primary", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests the motor cycle.
        /// </summary>
        [Test]
        public void TestMotorcycle()
        {
            var vehicle = Vehicle.MotorCycle.Fastest();

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
        /// Tests the car.
        /// </summary>
        [Test]
        public void TestCar()
        {
            var vehicle = Vehicle.Car.Fastest();

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
        /// Tests the bus.
        /// </summary>
        [Test]
        public void TestBus()
        {
            var vehicle = Vehicle.Bus.Fastest();

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
        /// Tests the small truck.
        /// </summary>
        [Test]
        public void TestSmallTruck()
        {
            var vehicle = Vehicle.SmallTruck.Fastest();

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
        /// Tests the big truck.
        /// </summary>
        [Test]
        public void TestBigTruck()
        {
            var vehicle = Vehicle.SmallTruck.Fastest();

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