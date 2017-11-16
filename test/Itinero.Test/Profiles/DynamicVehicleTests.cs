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
            this.TestFactorAndSpeed(vehicle, 0, null, 50, "highway", "unclassified", "motorcar", "yes");
            this.TestFactorAndSpeed(vehicle, 0, 0, 0, "highway", "unclassified", "motorcar", "no");

            // test maxspeed tags.
            this.TestFactorAndSpeed(vehicle, 0, null, 30 * .75f, "highway", "primary", "maxspeed", "30");
            this.TestFactorAndSpeed(vehicle, 0, null, 20 * 1.60934f * .75f, "highway", "primary", "maxspeed", "20 mph");
        }

        /// <summary>
        /// Tests loading a running a mock shapefile car profile.
        /// </summary>
        [Test]
        public void TestShapeCar()
        {
            var vehicle = DynamicVehicle.LoadFromEmbeddedResource(typeof(DynamicVehicleTests).Assembly, "Itinero.Test.test_data.profiles.shape.car.lua");
            var profile = vehicle.Fastest();
            
            // default types.
            this.TestFactorAndSpeed(profile, 0, null, 50, "BST_CODE", "BVD");
            this.TestFactorAndSpeed(profile, 0, null, 70, "BST_CODE", "AF");
            this.TestFactorAndSpeed(profile, 0, null, 70, "BST_CODE", "OP");
            this.TestFactorAndSpeed(profile, 0, null, 120, "BST_CODE", "HR");
            this.TestFactorAndSpeed(profile, 1, null, 30, "BST_CODE", "MRB");
            this.TestFactorAndSpeed(profile, 1, null, 30, "BST_CODE", "NRB");

            // test parameters.
            var value = string.Empty;
            Assert.IsTrue(vehicle.Parameters.TryGetValue("source_vertex", out value));
            Assert.AreEqual("JTE_ID_BEG", value);
            Assert.IsTrue(vehicle.Parameters.TryGetValue("target_vertex", out value));
            Assert.AreEqual("JTE_ID_END", value);
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