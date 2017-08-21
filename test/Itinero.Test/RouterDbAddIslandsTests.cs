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

using Itinero.Algorithms.Search.Hilbert;
using Itinero.Data;
using NUnit.Framework;

namespace Itinero.Test
{
    /// <summary>
    /// Tests the add islands feature.
    /// </summary>
    [TestFixture]
    public class RouterDbAddIslandsTests
    {
        /// <summary>
        /// Tests on a perfect network with no islands.
        /// </summary>
        [Test]
        public void TestAddIslandsNoIslands()
        {
            // build the routerdb.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network14.geojson"));
            routerDb.Sort();

            // add island data.
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddIslandData(profile);

            // verify result.
            MetaCollection<ushort> islands;
            Assert.IsTrue(routerDb.VertexData.TryGet("islands_" + profile.FullName, out islands));
            Assert.AreEqual(routerDb.Network.VertexCount, islands.Count);
            for (uint i = 0; i < islands.Count; i++)
            {
                Assert.AreEqual(ushort.MaxValue, islands[i]);
            }
        }

        /// <summary>
        /// Tests on an imperfect network with 2 islands.
        /// </summary>
        [Test]
        public void TestAddIslandsWithIslands()
        {
            // build the routerdb.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network16.geojson"));
            routerDb.Sort();

            // add island data.
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddIslandData(profile);

            // verify result.
            MetaCollection<ushort> islands;
            Assert.IsTrue(routerDb.VertexData.TryGet("islands_" + profile.FullName, out islands));
            Assert.AreEqual(routerDb.Network.VertexCount, islands.Count);
            for (uint i = 0; i < islands.Count; i++)
            {
                if (i <= 5)
                {
                    Assert.AreEqual(ushort.MaxValue, islands[i]);
                }
                else if (i == 6)
                {
                    Assert.AreEqual(0, islands[i]);
                }
                else
                {
                    Assert.AreEqual(3, islands[i]);
                }
            }
        }
    }
}