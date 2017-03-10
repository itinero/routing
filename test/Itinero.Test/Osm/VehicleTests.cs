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

using NUnit.Framework;
using Itinero.Profiles;
using Itinero.Attributes;
using System.IO;

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
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();

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
        /// Tests the pedestrian.
        /// </summary>
        [Test]
        public void TestPedestrianSerialize()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian;
            using (var stream = new MemoryStream())
            {
                vehicle.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);

                vehicle = Vehicle.Deserialize(stream);
            }
            var profile = vehicle.Fastest();

            // invalid highway types.
            this.TestFactorAndSpeed(profile, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "footway");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "pedestrian");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "path");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "cycleway");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "road");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "track");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "living_street");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "residential");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "tertiary");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "tertiary_link");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "secondary");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "secondary_link");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "primary");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "primary_link");
            this.TestFactorAndSpeed(profile, 0, 0, 0, "highway", "trunk");
            this.TestFactorAndSpeed(profile, 0, 0, 0, "highway", "trunk_link");
            this.TestFactorAndSpeed(profile, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(profile, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "foot", "designated");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "foot", "yes");
            this.TestFactorAndSpeed(profile, 0, 0, 0, "highway", "unclassified", "foot", "no");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "bicycle", "designated");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "bicycle", "yes");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "bicycle", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "maxspeed", "30");
            this.TestFactorAndSpeed(profile, 0, null, 4, "highway", "unclassified", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests the bicycle.
        /// </summary>
        [Test]
        public void TestBicycle()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest() as Itinero.Profiles.IProfileInstance;

            // invalid highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
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
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk_link");
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

            // check oneway tags.
            this.TestFactorAndSpeed(vehicle, 1, null, 15, "highway", "unclassified", "oneway", "yes");
            this.TestFactorAndSpeed(vehicle, 2, null, 15, "highway", "unclassified", "oneway", "-1");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "unclassified", "oneway", "yes", "oneway:bicycle", "no");
            this.TestFactorAndSpeed(vehicle, 0, null, 15, "highway", "unclassified", "oneway", "-1", "oneway:bicycle", "no");

            vehicle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Profile("balanced");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.2f, 15, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.2f, 15, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.9f, 15, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.9f, 15, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.1f, 15, "highway", "footway", "bicycle", "yes");

            vehicle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Profile("networks");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.2f, 15, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.2f) / 5f, 15, "highway", "path", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.2f, 15, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.2f) / 5f, 15, "highway", "cycleway", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.0f) / 5f, 15, "highway", "road", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.0f) / 5f, 15, "highway", "track", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.0f) / 5f, 15, "highway", "living_street", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.0f) / 5f, 15, "highway", "residential", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 1.0f, 15, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.0f) / 5f, 15, "highway", "unclassified", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.9f, 15, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 0.9f) / 5f, 15, "highway", "tertiary", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.9f, 15, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 0.9f) / 5f, 15, "highway", "tertiary_link", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 0.8f) / 5f, 15, "highway", "secondary", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 0.8f) / 5f, 15, "highway", "secondary_link", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 0.8f) / 5f, 15, "highway", "primary", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (15 / 3.6f)) / 0.8f, 15, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 0.8f) / 5f, 15, "highway", "primary_link", "cyclenetwork", "yes");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, ((1.0f / (15 / 3.6f)) / 1.1f) / 5f, 15, "highway", "footway", "bicycle", "yes", "cyclenetwork", "yes");
        }

        /// <summary>
        /// Tests the moped.
        /// </summary>
        [Test]
        public void TestMoped()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Moped.Fastest();

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
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "motorway_link");

            // designated roads.
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "moped", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 40, "highway", "unclassified", "moped", "yes");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "unclassified", "moped", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "primary", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 20 * 1.60934f * .75f, "highway", "primary", "maxspeed", "20 mph");

            // check oneway tags.
            this.TestFactorAndSpeed(vehicle, 1, null, 40, "highway", "unclassified", "oneway", "yes");
            this.TestFactorAndSpeed(vehicle, 2, null, 40, "highway", "unclassified", "oneway", "-1");
        }

        /// <summary>
        /// Tests the motor cycle.
        /// </summary>
        [Test]
        public void TestMotorcycle()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.MotorCycle.Fastest();

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

            // test the classifications profile.
            vehicle = Itinero.Osm.Vehicles.Vehicle.MotorCycle.Profile("classifications");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (05 / 3.6f)) / 4, 05, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway_link");
        }

        /// <summary>
        /// Tests the car.
        /// </summary>
        [Test]
        public void TestCar()
        {
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                System.Diagnostics.Debug.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                System.Diagnostics.Debug.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };

            var vehicle = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();

            // invalid highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highwey", "road");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, null, 05 * .75f, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, null, 70 * .75f, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, null, 70 * .75f, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 70 * .75f, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, null, 70 * .75f, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 90 * .75f, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, null, 90 * .75f, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, null, 90 * .75f, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, null, 90 * .75f, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, null, 120 * .75f, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, null, 120 * .75f, "highway", "motorway_link");

            // access tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified", "foot", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified", "foot", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified", "foot", "no");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified", "bicycle", "designated");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified", "bicycle", "yes");
            this.TestFactorAndSpeed(vehicle, 0, null, 50 * .75f, "highway", "unclassified", "bicycle", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "primary", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 20 * 1.60934f * .75f, "highway", "primary", "maxspeed", "20 mph");

            // check oneway tags.
            this.TestFactorAndSpeed(vehicle, 1, null, 50 * .75f, "highway", "unclassified", "oneway", "yes");
            this.TestFactorAndSpeed(vehicle, 2, null, 50 * .75f, "highway", "unclassified", "oneway", "-1");

            // test the classifications profile.
            vehicle = Itinero.Osm.Vehicles.Vehicle.Car.Profile("classifications");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (05 * .75f / 3.6f)) / 4, 05 * .75f, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 * .75f / 3.6f)) / 4, 30 * .75f, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 * .75f / 3.6f)) / 4, 30 * .75f, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 * .75f / 3.6f)) / 5, 50 * .75f, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 * .75f / 3.6f)) / 5, 50 * .75f, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 * .75f / 3.6f)) / 6, 70 * .75f, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 * .75f / 3.6f)) / 6, 70 * .75f, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 * .75f / 3.6f)) / 7, 70 * .75f, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 * .75f / 3.6f)) / 7, 70 * .75f, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 * .75f / 3.6f)) / 8, 90 * .75f, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 * .75f / 3.6f)) / 8, 90 * .75f, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 * .75f / 3.6f)) / 9, 90 * .75f, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 * .75f / 3.6f)) / 9, 90 * .75f, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 * .75f / 3.6f)) / 10, 120 * .75f, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 * .75f / 3.6f)) / 10, 120 * .75f, "highway", "motorway_link");
        }
        
        /// <summary>
        /// Tests the bus.
        /// </summary>
        [Test]
        public void TestBus()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Bus.Fastest();

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

            // test the classifications profile.
            vehicle = Itinero.Osm.Vehicles.Vehicle.Bus.Profile("classifications");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (05 / 3.6f)) / 4, 05, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway_link");
        }

        /// <summary>
        /// Tests the small truck.
        /// </summary>
        [Test]
        public void TestSmallTruck()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.SmallTruck.Fastest();

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

            // test the classifications profile.
            vehicle = Itinero.Osm.Vehicles.Vehicle.SmallTruck.Profile("classifications");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (05 / 3.6f)) / 4, 05, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway_link");
        }

        /// <summary>
        /// Tests the big truck.
        /// </summary>
        [Test]
        public void TestBigTruck()
        {
            var vehicle = Itinero.Osm.Vehicles.Vehicle.SmallTruck.Fastest();

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

            // test the classifications profile.
            vehicle = Itinero.Osm.Vehicles.Vehicle.BigTruck.Profile("classifications");

            // default highway types.
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "footway");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "pedestrian");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "path");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "cycleway");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (05 / 3.6f)) / 4, 05, "highway", "living_street");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "road");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (30 / 3.6f)) / 4, 30, "highway", "track");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "unclassified");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (50 / 3.6f)) / 5, 50, "highway", "residential");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 6, 70, "highway", "tertiary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (70 / 3.6f)) / 7, 70, "highway", "secondary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 8, 90, "highway", "primary_link");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk");
            this.TestFactorAndSpeed(vehicle, 0, (1.0f / (90 / 3.6f)) / 9, 90, "highway", "trunk_link");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway");
            this.TestFactorAndSpeed(vehicle, 3, (1.0f / (120 / 3.6f)) / 10, 120, "highway", "motorway_link");
        }

        /// <summary>
        /// Tests getting factor and speed.
        /// </summary>
        protected void TestFactorAndSpeed(Itinero.Profiles.IProfileInstance profile, short? direction, float? factor, float? speed, params string[] tags)
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
                Assert.AreEqual(factor.Value, factorAndSpeed.Value, 0.0001);
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