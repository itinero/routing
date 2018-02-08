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

using Itinero.Algorithms;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Matrices
{
    [TestFixture]
    public class DirectedWeightMatrixAlgorithmTests
    {
        /// <summary>
        /// Tests directed edge based many-to-many weight calculations. 
        /// </summary>
        [Test]
        public void TestContractedDirectedOldEdgeBasedManyToManyWeights()
        {
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
            routerDb.AddContractedOldEdgeBased(vehicle);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);

            // route and verify (a few select weights).
            var matrixCalculator = new Itinero.Algorithms.Matrices.DirectedWeightMatrixAlgorithm(router,
                vehicle, new Coordinate[]
                {
                    location1,
                    location2,
                    location3,
                }, float.MaxValue);
            matrixCalculator.Run();

            Assert.IsTrue(matrixCalculator.HasRun);
            Assert.IsTrue(matrixCalculator.HasSucceeded);
        }

        /// <summary>
        /// Tests directed edge based many-to-many weight calculations. 
        /// </summary>
        [Test]
        public void TestContractedDualManyToManyWeights()
        {
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
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);

            // route and verify (a few select weights).
            var matrixCalculator = new Itinero.Algorithms.Matrices.DirectedWeightMatrixAlgorithm(router,
                vehicle, new Coordinate[]
                {
                    location1,
                    location2,
                    location3,
                }, float.MaxValue);
            matrixCalculator.Run();

            Assert.IsTrue(matrixCalculator.HasRun);
            Assert.IsTrue(matrixCalculator.HasSucceeded);
        }

        /// <summary>
        /// Tests directed edge based many-to-many weight calculations. 
        /// </summary>
        [Test]
        public void TestContractedDualManyToManyWeightsOneway()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network15.geojson"));

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
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            var json = routerDb.GetGeoJson();

            // route between two locations for all 4 options.
            var location1 = new Coordinate(51.222421401217204f, 4.426546096801758f);
            var location2 = new Coordinate(51.22244827903742f, 4.43248987197876f);

            // route and verify (a few select weights).
            var matrixCalculator = new Itinero.Algorithms.Matrices.DirectedWeightMatrixAlgorithm(router,
                vehicle, new Coordinate[]
                {
                    location1,
                    location2
                }, float.MaxValue);
            matrixCalculator.Run();

            Assert.IsTrue(matrixCalculator.HasRun);
            Assert.IsTrue(matrixCalculator.HasSucceeded);

            var weights = matrixCalculator.Weights;
            Assert.IsNotNull(weights);
            Assert.AreEqual(4, weights.Length);
            Assert.AreEqual(4, weights[0].Length);

            Assert.AreEqual(float.MaxValue, weights[0][0]);
            Assert.AreEqual(float.MaxValue, weights[0][1]);
            Assert.AreEqual(float.MaxValue, weights[0][2]);
            Assert.AreEqual(float.MaxValue, weights[0][3]);

            Assert.AreEqual(float.MaxValue, weights[1][0]);
            Assert.AreEqual(0, weights[1][1]);
            Assert.AreEqual(float.MaxValue, weights[1][2]);
            Assert.AreEqual(75, weights[1][3], 10);

            Assert.AreEqual(float.MaxValue, weights[2][0]);
            Assert.AreEqual(float.MaxValue, weights[2][1]);
            Assert.AreEqual(float.MaxValue, weights[2][2]);
            Assert.AreEqual(float.MaxValue, weights[2][3]);

            Assert.AreEqual(float.MaxValue, weights[3][0]);
            Assert.AreEqual(75, weights[3][1], 10);
            Assert.AreEqual(float.MaxValue, weights[3][2]);
            Assert.AreEqual(0, weights[3][3]);
        }
    }
}