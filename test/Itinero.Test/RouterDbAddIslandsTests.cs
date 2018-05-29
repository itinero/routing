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
using Itinero.LocalGeo;
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
                    Assert.AreEqual(1, islands[i]);
                }
                else
                {
                    Assert.AreEqual(13, islands[i]);
                }
            }
        }

        /// <summary>
        /// Tests on an imperfect network with 2 islands and including edge data.
        /// </summary>
        [Test]
        public void TestAddIslandsWithIslandsAndEdgeData()
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
            routerDb.AddIslandData(profile, true);

            // verify result.
            var edgeEnumerator = routerDb.Network.GetEdgeEnumerator();
            Assert.IsTrue(routerDb.VertexData.TryGet("islands_" + profile.FullName, 
                out MetaCollection<ushort> islands));
            Assert.IsTrue(routerDb.EdgeData.TryGet("islands_" + profile.FullName, 
                out MetaCollection<ushort> edgeIslands));
            for (uint v = 0; v < routerDb.Network.VertexCount; v++)
            {
                if (!edgeEnumerator.MoveTo(v))
                {
                    continue;
                }

                while (edgeEnumerator.MoveNext())
                {
                    var to = edgeEnumerator.To;

                    var edgeIslandCount = edgeIslands[edgeEnumerator.Id];
                    if (edgeIslandCount != 0)
                    {
                        if (v <= 5 || to <= 5)
                        {
                            Assert.AreEqual(ushort.MaxValue, edgeIslandCount);
                        }
                        else if (v >= 6 || to >= 6)
                        {
                            Assert.AreEqual(13, edgeIslandCount);
                        }
                        else
                        {
                            Assert.AreEqual(1, edgeIslandCount);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tests oneway islands and resolving.
        /// </summary>
        [Test]
        public void TestAddIslandsOneway()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network17.geojson"));
            routerDb.Sort();

            // add island data.
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddIslandData(profile);

            // resolve at the location of the oneway island.
            var router = new Router(routerDb);

            var location = new Coordinate(51.22620094060593f, 4.424091875553131f);
            var point = router.Resolve(profile, location, 1000);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(new Coordinate(51.22338f, 4.426911f),
                point.LocationOnNetwork(routerDb)) < 10);

            location = new Coordinate(51.22468580253045f, 4.421868324279785f);
            point = router.Resolve(profile, location, 1000);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(new Coordinate(51.22338f, 4.426911f),
                point.LocationOnNetwork(routerDb)) < 10);

            location = new Coordinate(51.22427593399205f, 4.425215721130371f);
            point = router.Resolve(profile, location, 1000);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(new Coordinate(51.22338f, 4.426911f),
                point.LocationOnNetwork(routerDb)) < 10);
        }

        /// <summary>
        /// Tests single vertex restriction islands and resolving.
        /// </summary>
        [Test]
        public void TestAddIslandsSingleVertexRestriction()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network18.geojson"));
            routerDb.Sort();

            // add island data.
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddIslandData(profile);

            // resolve at the location of the oneway island.
            var router = new Router(routerDb);

            var location = new Coordinate(51.22620094060593f, 4.424091875553131f);
            var point = router.Resolve(profile, location, 1000);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(new Coordinate(51.22338f, 4.426911f),
                point.LocationOnNetwork(routerDb)) < 10);

            location = new Coordinate(51.22468580253045f, 4.421868324279785f);
            point = router.Resolve(profile, location, 1000);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(new Coordinate(51.22338f, 4.426911f),
                point.LocationOnNetwork(routerDb)) < 10);

            location = new Coordinate(51.22427593399205f, 4.425215721130371f);
            point = router.Resolve(profile, location, 1000);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(new Coordinate(51.22338f, 4.426911f),
                point.LocationOnNetwork(routerDb)) < 10);
        }
        
        /// <summary>
        /// Tests has islands.
        /// </summary>
        [Test]
        public void TestHasIslands()
        {
            // build the routerdb.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network14.geojson"));
            routerDb.Sort();
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            
            // has to be false here.
            Assert.IsFalse(routerDb.HasIslandData(profile));

            // add island data.
            routerDb.AddIslandData(profile);
            
            // has to be true here.
            Assert.IsTrue(routerDb.HasIslandData(profile));
        }
    }
}