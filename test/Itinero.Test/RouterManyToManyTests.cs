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
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Test.Algorithms.Search;
using Itinero.Test.Profiles;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using Itinero.Algorithms.Contracted;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Contracted;
using Itinero.Algorithms;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the router.
    /// </summary>
    [TestFixture]
    public class RouterManyToManyTests
    {
        /// <summary>
        /// Tests non-contracted many-to-many routing.
        /// </summary>
        [Test]
        public void TestUncontractedManyToMany()
        {
            var eMeter = 20f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);
            var resolved = new RouterPoint[]
            {
                resolved1,
                resolved2,
                resolved3,
                resolved5
            };

            // route and verify (a few select routes).
            var routes = router.TryCalculate(vehicle, resolved, resolved);

            // matrix of distance results:
            //      1    2    3    5
            // 1 [  0, 210, 400, 370]
            // 2 [210,   0, 191, 140]
            // 3 [400, 191,   0,  50]
            // 5 [370, 140,  50,   0]

            Assert.AreEqual(0, routes.Value[0][0].TotalDistance, eMeter);
            Assert.AreEqual(210, routes.Value[0][1].TotalDistance, eMeter);
            Assert.AreEqual(400, routes.Value[0][2].TotalDistance, eMeter);
            Assert.AreEqual(370, routes.Value[0][3].TotalDistance, eMeter);

            Assert.AreEqual(210, routes.Value[1][0].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[1][1].TotalDistance, eMeter);
            Assert.AreEqual(191, routes.Value[1][2].TotalDistance, eMeter);
            Assert.AreEqual(140, routes.Value[1][3].TotalDistance, eMeter);

            Assert.AreEqual(400, routes.Value[2][0].TotalDistance, eMeter);
            Assert.AreEqual(191, routes.Value[2][1].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[2][2].TotalDistance, eMeter);
            Assert.AreEqual(50, routes.Value[2][3].TotalDistance, eMeter);

            Assert.AreEqual(370, routes.Value[3][0].TotalDistance, eMeter);
            Assert.AreEqual(140, routes.Value[3][1].TotalDistance, eMeter);
            Assert.AreEqual(50, routes.Value[3][2].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[3][3].TotalDistance, eMeter);
        }

        /// <summary>
        /// Tests contracted many-to-many routing.
        /// </summary>
        [Test]
        public void TestContractedManyToMany()
        {
            var eMeter = 20f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, false);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);
            var resolved = new RouterPoint[]
            {
                resolved1,
                resolved2,
                resolved3,
                resolved5
            };

            // route and verify (a few select routes).
            var routes = router.TryCalculate(vehicle, resolved, resolved);

            // matrix of distance results:
            //      1    2    3    5
            // 1 [  0, 210, 400, 370]
            // 2 [210,   0, 191, 140]
            // 3 [400, 191,   0,  50]
            // 5 [370, 140,  50,   0]

            Assert.AreEqual(0, routes.Value[0][0].TotalDistance, eMeter);
            Assert.AreEqual(210, routes.Value[0][1].TotalDistance, eMeter);
            Assert.AreEqual(400, routes.Value[0][2].TotalDistance, eMeter);
            Assert.AreEqual(370, routes.Value[0][3].TotalDistance, eMeter);

            Assert.AreEqual(210, routes.Value[1][0].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[1][1].TotalDistance, eMeter);
            Assert.AreEqual(191, routes.Value[1][2].TotalDistance, eMeter);
            Assert.AreEqual(140, routes.Value[1][3].TotalDistance, eMeter);

            Assert.AreEqual(400, routes.Value[2][0].TotalDistance, eMeter);
            Assert.AreEqual(191, routes.Value[2][1].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[2][2].TotalDistance, eMeter);
            Assert.AreEqual(50, routes.Value[2][3].TotalDistance, eMeter);

            Assert.AreEqual(370, routes.Value[3][0].TotalDistance, eMeter);
            Assert.AreEqual(140, routes.Value[3][1].TotalDistance, eMeter);
            Assert.AreEqual(50, routes.Value[3][2].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[3][3].TotalDistance, eMeter);
        }

        /// <summary>
        /// Tests contracted edge based many-to-many routing.
        /// </summary>
        [Test]
        public void TestContractedEdgeBasedManyToMany()
        {
            var eMeter = 20f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);
            var resolved = new RouterPoint[]
            {
                resolved1,
                resolved2,
                resolved3,
                resolved5
            };

            // route and verify (a few select routes).
            var routes = router.TryCalculate(vehicle, resolved, resolved);

            // matrix of distance results:
            //      1    2    3    5
            // 1 [  0, 210, 400, 370]
            // 2 [210,   0, 191, 140]
            // 3 [400, 191,   0,  50]
            // 5 [370, 140,  50,   0]

            Assert.AreEqual(0, routes.Value[0][0].TotalDistance, eMeter);
            Assert.AreEqual(210, routes.Value[0][1].TotalDistance, eMeter);
            Assert.AreEqual(400, routes.Value[0][2].TotalDistance, eMeter);
            Assert.AreEqual(370, routes.Value[0][3].TotalDistance, eMeter);

            Assert.AreEqual(210, routes.Value[1][0].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[1][1].TotalDistance, eMeter);
            Assert.AreEqual(191, routes.Value[1][2].TotalDistance, eMeter);
            Assert.AreEqual(140, routes.Value[1][3].TotalDistance, eMeter);

            Assert.AreEqual(400, routes.Value[2][0].TotalDistance, eMeter);
            Assert.AreEqual(191, routes.Value[2][1].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[2][2].TotalDistance, eMeter);
            Assert.AreEqual(50, routes.Value[2][3].TotalDistance, eMeter);

            Assert.AreEqual(370, routes.Value[3][0].TotalDistance, eMeter);
            Assert.AreEqual(140, routes.Value[3][1].TotalDistance, eMeter);
            Assert.AreEqual(50, routes.Value[3][2].TotalDistance, eMeter);
            Assert.AreEqual(0, routes.Value[3][3].TotalDistance, eMeter);
        }
        
        /// <summary>
        /// Tests directed edge based many-to-many weight calculations. 
        /// </summary>
        [Test]
        public void TestUncontractedDirectedEdgeBasedManyToManyWeights()
        {
            var eMeter = 20f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Shortest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var directedIds = new DirectedEdgeId[]
            {
                new DirectedEdgeId(resolved1.EdgeId, true),
                new DirectedEdgeId(resolved1.EdgeId, false),
                new DirectedEdgeId(resolved2.EdgeId, true),
                new DirectedEdgeId(resolved2.EdgeId, false),
                new DirectedEdgeId(resolved3.EdgeId, true),
                new DirectedEdgeId(resolved3.EdgeId, false)
            };

            // route and verify (a few select weights).
            var weightsResult = router.TryCalculateWeight(vehicle, directedIds, directedIds);
            Assert.IsFalse(weightsResult.IsError);
            var weights = weightsResult.Value;

            // matrix of distance results:
            //       1F   1B   2F   2B   3F   3B  
            // 1F [   0,1048, 470, 400, 340, 580]
            // 1B [ 780,   0, 600,   0, 470, 180]
            // 2F [   0, 400,   0, 550, 466, 740]
            // 2B [ 600, 470,1220,   0,1090,   0]
            // 3F [ 180, 580,   0, 730,   0, 920]
            // 3B [ 470, 340,1090, 466, 960,   0]

            // 1F [   0,1048, 470, 400, 340, 580]
            Assert.AreEqual(0, weights[0][0], eMeter); 
            Assert.AreEqual(1048, weights[0][1], eMeter);
            Assert.AreEqual(470, weights[0][2], eMeter);
            Assert.AreEqual(400, weights[0][3], eMeter);
            Assert.AreEqual(340, weights[0][4], eMeter);
            Assert.AreEqual(580, weights[0][5], eMeter);

            // 1B [ 780,   0, 600,   0, 470, 180]
            Assert.AreEqual(780, weights[1][0], eMeter);
            Assert.AreEqual(0, weights[1][1], eMeter);
            Assert.AreEqual(600, weights[1][2], eMeter);
            Assert.AreEqual(0, weights[1][3], eMeter);
            Assert.AreEqual(470, weights[1][4], eMeter);
            Assert.AreEqual(180, weights[1][5], eMeter);

            // 2F [   0, 400,   0, 550, 466, 740]
            Assert.AreEqual(0, weights[2][0], eMeter);
            Assert.AreEqual(400, weights[2][1], eMeter);
            Assert.AreEqual(0, weights[2][2], eMeter);
            Assert.AreEqual(550, weights[2][3], eMeter);
            Assert.AreEqual(466, weights[2][4], eMeter);
            Assert.AreEqual(740, weights[2][5], eMeter);

            // 2B [ 600, 470,1220,   0,1090,   0]
            Assert.AreEqual(600, weights[3][0], eMeter);
            Assert.AreEqual(470, weights[3][1], eMeter);
            Assert.AreEqual(1220, weights[3][2], eMeter);
            Assert.AreEqual(0, weights[3][3], eMeter);
            Assert.AreEqual(1090, weights[3][4], eMeter);
            Assert.AreEqual(0, weights[3][5], eMeter);

            // 3F [ 180, 580,   0, 730,   0, 920]
            Assert.AreEqual(180, weights[4][0], eMeter);
            Assert.AreEqual(580, weights[4][1], eMeter);
            Assert.AreEqual(0, weights[4][2], eMeter);
            Assert.AreEqual(730, weights[4][3], eMeter);
            Assert.AreEqual(0, weights[4][4], eMeter);
            Assert.AreEqual(920, weights[4][5], eMeter);

            // 3B [ 470, 340,1090, 466, 960,   0]
            Assert.AreEqual(470, weights[5][0], eMeter);
            Assert.AreEqual(340, weights[5][1], eMeter);
            Assert.AreEqual(1090, weights[5][2], eMeter);
            Assert.AreEqual(466, weights[5][3], eMeter);
            Assert.AreEqual(960, weights[5][4], eMeter);
            Assert.AreEqual(0, weights[5][5], eMeter);
        }

        /// <summary>
        /// Tests directed edge based many-to-many weight calculations. 
        /// </summary>
        [Test]
        public void TestContractedDirectedEdgeBasedManyToManyWeights()
        {
            var eMeter = 20f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Shortest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var directedIds = new DirectedEdgeId[]
            {
                new DirectedEdgeId(resolved1.EdgeId, true),
                new DirectedEdgeId(resolved1.EdgeId, false),
                new DirectedEdgeId(resolved2.EdgeId, true),
                new DirectedEdgeId(resolved2.EdgeId, false),
                new DirectedEdgeId(resolved3.EdgeId, true),
                new DirectedEdgeId(resolved3.EdgeId, false)
            };

            // route and verify (a few select weights).
            var weightsResult = router.TryCalculateWeight(vehicle, directedIds, directedIds);
            Assert.IsFalse(weightsResult.IsError);
            var weights = weightsResult.Value;

            // matrix of distance results:
            //       1F   1B   2F   2B   3F   3B  
            // 1F [   0,1048, 470, 400, 340, 580]
            // 1B [ 780,   0, 600,   0, 470, 180]
            // 2F [   0, 400,   0, 550, 466, 740]
            // 2B [ 600, 470,1220,   0,1090,   0]
            // 3F [ 180, 580,   0, 730,   0, 920]
            // 3B [ 470, 340,1090, 466, 960,   0]

            // 1F [   0,1048, 470, 400, 340, 580]
            Assert.AreEqual(0, weights[0][0], eMeter);
            Assert.AreEqual(1048, weights[0][1], eMeter);
            Assert.AreEqual(470, weights[0][2], eMeter);
            Assert.AreEqual(400, weights[0][3], eMeter);
            Assert.AreEqual(340, weights[0][4], eMeter);
            Assert.AreEqual(580, weights[0][5], eMeter);

            // 1B [ 780,   0, 600,   0, 470, 180]
            Assert.AreEqual(780, weights[1][0], eMeter);
            Assert.AreEqual(0, weights[1][1], eMeter);
            Assert.AreEqual(600, weights[1][2], eMeter);
            Assert.AreEqual(0, weights[1][3], eMeter);
            Assert.AreEqual(470, weights[1][4], eMeter);
            Assert.AreEqual(180, weights[1][5], eMeter);

            // 2F [   0, 400,   0, 550, 466, 740]
            Assert.AreEqual(0, weights[2][0], eMeter);
            Assert.AreEqual(400, weights[2][1], eMeter);
            Assert.AreEqual(0, weights[2][2], eMeter);
            Assert.AreEqual(550, weights[2][3], eMeter);
            Assert.AreEqual(466, weights[2][4], eMeter);
            Assert.AreEqual(740, weights[2][5], eMeter);

            // 2B [ 600, 470,1220,   0,1090,   0]
            Assert.AreEqual(600, weights[3][0], eMeter);
            Assert.AreEqual(470, weights[3][1], eMeter);
            Assert.AreEqual(1220, weights[3][2], eMeter);
            Assert.AreEqual(0, weights[3][3], eMeter);
            Assert.AreEqual(1090, weights[3][4], eMeter);
            Assert.AreEqual(0, weights[3][5], eMeter);

            // 3F [ 180, 580,   0, 730,   0, 920]
            Assert.AreEqual(180, weights[4][0], eMeter);
            Assert.AreEqual(580, weights[4][1], eMeter);
            Assert.AreEqual(0, weights[4][2], eMeter);
            Assert.AreEqual(730, weights[4][3], eMeter);
            Assert.AreEqual(0, weights[4][4], eMeter);
            Assert.AreEqual(920, weights[4][5], eMeter);

            // 3B [ 470, 340,1090, 466, 960,   0]
            Assert.AreEqual(470, weights[5][0], eMeter);
            Assert.AreEqual(340, weights[5][1], eMeter);
            Assert.AreEqual(1090, weights[5][2], eMeter);
            Assert.AreEqual(466, weights[5][3], eMeter);
            Assert.AreEqual(960, weights[5][4], eMeter);
            Assert.AreEqual(0, weights[5][5], eMeter);
        }
    }
}