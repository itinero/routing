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

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the router.
    /// </summary>
    [TestFixture]
    public class RouterTests
    {
        /// <summary>
        /// Tests setting the custom resolver delegate.
        /// </summary>
        [Test]
        public void TestCustomResolverDelegate()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            var router = new Router(routerDb);
            var called = false;
            router.CreateCustomResolver = (latitude, longitude, isAcceptable, isBetter) =>
                {
                    called = true;
                    return new MockResolver(new RouterPoint(latitude, longitude, 0, 0));
                };
            router.Resolve(new Itinero.Profiles.Profile[] { VehicleMock.Car().Fastest() }, 0, 0);

            Assert.IsTrue(called);
        }

        /// <summary>
        /// Tests resolving some points on test network 3.
        /// </summary>
        [Test]
        public void TestResolveNetwork3()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network3.geojson"));

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);
            var vertex9 = routerDb.Network.GetVertex(9);

            routerDb.Network.Sort();

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var location2 = new Coordinate(52.35476168070f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);

            var resolved1 = new Coordinate(52.352949189200494f, 6.665348410606384f);
            var resolved2 = new Coordinate(52.354736518079150f, 6.666120886802673f);
            var resolved3 = new Coordinate(52.354974058638945f, 6.664675176143646f);
            var resolved4 = new Coordinate(52.353594667362720f, 6.664688587188721f);

            var router = new Router(routerDb);

            var point = router.TryResolve(car, location1);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved1, point.Value.LocationOnNetwork(routerDb)) < 10);

            point = router.TryResolve(car, location2);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved2, point.Value.LocationOnNetwork(routerDb)) < 10);

            point = router.TryResolve(car, location3);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved3, point.Value.LocationOnNetwork(routerDb)) < 10);

            point = router.TryResolve(car, location4);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved4, point.Value.LocationOnNetwork(routerDb)) < 10);

            router.ProfileFactorAndSpeedCache = new Itinero.Profiles.ProfileFactorAndSpeedCache(router.Db);
            router.ProfileFactorAndSpeedCache.CalculateFor(car);

            point = router.TryResolve(car, location1);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved1, point.Value.LocationOnNetwork(routerDb)) < 10);

            point = router.TryResolve(car, location2);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved2, point.Value.LocationOnNetwork(routerDb)) < 10);

            point = router.TryResolve(car, location3);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved3, point.Value.LocationOnNetwork(routerDb)) < 10);

            point = router.TryResolve(car, location4);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved4, point.Value.LocationOnNetwork(routerDb)) < 10);
        }

        /// <summary>
        /// Tests routing using and edge-based contracted network.
        /// </summary>
        [Test]
        public void TestEdgeBasedContractedNetwork3()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network3.geojson"));

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);

            routerDb.Network.Sort();

            var pedestrian = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(pedestrian.Parent);
            routerDb.AddContracted(pedestrian, true);
            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var location2 = new Coordinate(52.35476168070f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);

            var resolved1 = new Coordinate(52.352949189200494f, 6.665348410606384f);
            var resolved2 = new Coordinate(52.354736518079150f, 6.666120886802673f);
            var resolved3 = new Coordinate(52.354974058638945f, 6.664675176143646f);
            var resolved4 = new Coordinate(52.353594667362720f, 6.664688587188721f);

            var vertex0id = routerDb.SearchVertexFor(vertex0.Latitude, vertex0.Longitude);
            var vertex1id = routerDb.SearchVertexFor(vertex1.Latitude, vertex1.Longitude);
            var vertex2id = routerDb.SearchVertexFor(vertex2.Latitude, vertex2.Longitude);
            var vertex3id = routerDb.SearchVertexFor(vertex3.Latitude, vertex3.Longitude);
            var vertex4id = routerDb.SearchVertexFor(vertex4.Latitude, vertex4.Longitude);
            var vertex5id = routerDb.SearchVertexFor(vertex5.Latitude, vertex5.Longitude);
            var vertex6id = routerDb.SearchVertexFor(vertex6.Latitude, vertex6.Longitude);
            var vertex7id = routerDb.SearchVertexFor(vertex7.Latitude, vertex7.Longitude);
            var vertex8id = routerDb.SearchVertexFor(vertex8.Latitude, vertex8.Longitude);

            var route = router.Calculate(pedestrian, vertex0, vertex0);
            route = router.Calculate(pedestrian, vertex0, vertex1);
            route = router.Calculate(pedestrian, vertex0, vertex2);
            route = router.Calculate(pedestrian, vertex0, vertex3);
            route = router.Calculate(pedestrian, vertex0, vertex4);
            route = router.Calculate(pedestrian, vertex0, vertex5);
            route = router.Calculate(pedestrian, vertex0, vertex6);
            route = router.Calculate(pedestrian, vertex0, vertex7);
            route = router.Calculate(pedestrian, vertex0, vertex8);

            route = router.Calculate(pedestrian, vertex1, vertex0);
            route = router.Calculate(pedestrian, vertex1, vertex1);
            route = router.Calculate(pedestrian, vertex1, vertex2);
            route = router.Calculate(pedestrian, vertex1, vertex3);
            route = router.Calculate(pedestrian, vertex1, vertex4);
            route = router.Calculate(pedestrian, vertex1, vertex5);
            route = router.Calculate(pedestrian, vertex1, vertex6);
            route = router.Calculate(pedestrian, vertex1, vertex7);
            route = router.Calculate(pedestrian, vertex1, vertex8);

            route = router.Calculate(pedestrian, vertex2, vertex0);
            route = router.Calculate(pedestrian, vertex2, vertex1);
            route = router.Calculate(pedestrian, vertex2, vertex2);
            route = router.Calculate(pedestrian, vertex2, vertex3);
            route = router.Calculate(pedestrian, vertex2, vertex4);
            route = router.Calculate(pedestrian, vertex2, vertex5);
            route = router.Calculate(pedestrian, vertex2, vertex6);
            route = router.Calculate(pedestrian, vertex2, vertex7);
            route = router.Calculate(pedestrian, vertex2, vertex8);

            route = router.Calculate(pedestrian, vertex3, vertex0);
            route = router.Calculate(pedestrian, vertex3, vertex1);
            route = router.Calculate(pedestrian, vertex3, vertex2);
            route = router.Calculate(pedestrian, vertex3, vertex3);
            route = router.Calculate(pedestrian, vertex3, vertex4);
            route = router.Calculate(pedestrian, vertex3, vertex5);
            route = router.Calculate(pedestrian, vertex3, vertex6);
            route = router.Calculate(pedestrian, vertex3, vertex7);
            route = router.Calculate(pedestrian, vertex3, vertex8);

            route = router.Calculate(pedestrian, vertex4, vertex0);
            route = router.Calculate(pedestrian, vertex4, vertex1);
            route = router.Calculate(pedestrian, vertex4, vertex2);
            route = router.Calculate(pedestrian, vertex4, vertex3);
            route = router.Calculate(pedestrian, vertex4, vertex4);
            route = router.Calculate(pedestrian, vertex4, vertex5);
            route = router.Calculate(pedestrian, vertex4, vertex6);
            route = router.Calculate(pedestrian, vertex4, vertex7);
            route = router.Calculate(pedestrian, vertex4, vertex8);

            route = router.Calculate(pedestrian, vertex5, vertex0);
            route = router.Calculate(pedestrian, vertex5, vertex1);
            route = router.Calculate(pedestrian, vertex5, vertex2);
            route = router.Calculate(pedestrian, vertex5, vertex3);
            route = router.Calculate(pedestrian, vertex5, vertex4);
            route = router.Calculate(pedestrian, vertex5, vertex5);
            route = router.Calculate(pedestrian, vertex5, vertex6);
            route = router.Calculate(pedestrian, vertex5, vertex7);
            route = router.Calculate(pedestrian, vertex5, vertex8);

            route = router.Calculate(pedestrian, vertex6, vertex0);
            route = router.Calculate(pedestrian, vertex6, vertex1);
            route = router.Calculate(pedestrian, vertex6, vertex2);
            route = router.Calculate(pedestrian, vertex6, vertex3);
            route = router.Calculate(pedestrian, vertex6, vertex4);
            route = router.Calculate(pedestrian, vertex6, vertex5);
            route = router.Calculate(pedestrian, vertex6, vertex6);
            route = router.Calculate(pedestrian, vertex6, vertex7);
            route = router.Calculate(pedestrian, vertex6, vertex8);

            route = router.Calculate(pedestrian, vertex7, vertex0);
            route = router.Calculate(pedestrian, vertex7, vertex1);
            route = router.Calculate(pedestrian, vertex7, vertex2);
            route = router.Calculate(pedestrian, vertex7, vertex3);
            route = router.Calculate(pedestrian, vertex7, vertex4);
            route = router.Calculate(pedestrian, vertex7, vertex5);
            route = router.Calculate(pedestrian, vertex7, vertex6);
            route = router.Calculate(pedestrian, vertex7, vertex7);
            route = router.Calculate(pedestrian, vertex7, vertex8);

            route = router.Calculate(pedestrian, vertex8, vertex0);
            route = router.Calculate(pedestrian, vertex8, vertex1);
            route = router.Calculate(pedestrian, vertex8, vertex2);
            route = router.Calculate(pedestrian, vertex8, vertex3);
            route = router.Calculate(pedestrian, vertex8, vertex4);
            route = router.Calculate(pedestrian, vertex8, vertex5);
            route = router.Calculate(pedestrian, vertex8, vertex6);
            route = router.Calculate(pedestrian, vertex8, vertex7);
            route = router.Calculate(pedestrian, vertex8, vertex8);

            route = router.Calculate(pedestrian, vertex0, vertex8);
            route = router.Calculate(pedestrian, vertex3, vertex7);
            route = router.Calculate(pedestrian, resolved4, resolved2);
        }

        /// <summary>
        /// Tests routing using and edge-based contracted network.
        /// </summary>
        [Test]
        public void TestEdgeBasedContractedNetwork5()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network5.geojson"));

            var pedestrian = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(pedestrian.Parent);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);
            var vertex9 = routerDb.Network.GetVertex(9);
            var vertex10 = routerDb.Network.GetVertex(10);
            var vertex11 = routerDb.Network.GetVertex(11);
            var vertex12 = routerDb.Network.GetVertex(12);
            var vertex13 = routerDb.Network.GetVertex(13);
            var vertex14 = routerDb.Network.GetVertex(14);
            var vertex15 = routerDb.Network.GetVertex(15);
            var vertex16 = routerDb.Network.GetVertex(16);
            var vertex17 = routerDb.Network.GetVertex(17);

            routerDb.Network.Sort();
            routerDb.AddContracted(pedestrian, true);

            var vertex0sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex0.Latitude, vertex0.Longitude, 0.0001f, 0.0001f);
            var vertex1sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex1.Latitude, vertex1.Longitude, 0.0001f, 0.0001f);
            var vertex2sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex2.Latitude, vertex2.Longitude, 0.0001f, 0.0001f);
            var vertex3sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex3.Latitude, vertex3.Longitude, 0.0001f, 0.0001f);
            var vertex4sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex4.Latitude, vertex4.Longitude, 0.0001f, 0.0001f);
            var vertex5sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex5.Latitude, vertex5.Longitude, 0.0001f, 0.0001f);
            var vertex6sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex6.Latitude, vertex6.Longitude, 0.0001f, 0.0001f);
            var vertex7sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex7.Latitude, vertex7.Longitude, 0.0001f, 0.0001f);
            var vertex8sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex8.Latitude, vertex8.Longitude, 0.0001f, 0.0001f);
            var vertex9sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex9.Latitude, vertex9.Longitude, 0.0001f, 0.0001f);
            var vertex10sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex10.Latitude, vertex10.Longitude, 0.0001f, 0.0001f);
            var vertex11sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex11.Latitude, vertex11.Longitude, 0.0001f, 0.0001f);
            var vertex12sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex12.Latitude, vertex12.Longitude, 0.0001f, 0.0001f);
            var vertex13sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex13.Latitude, vertex13.Longitude, 0.0001f, 0.0001f);
            var vertex14sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex14.Latitude, vertex14.Longitude, 0.0001f, 0.0001f);
            var vertex15sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex15.Latitude, vertex15.Longitude, 0.0001f, 0.0001f);
            var vertex16sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex16.Latitude, vertex16.Longitude, 0.0001f, 0.0001f);
            var vertex17sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex17.Latitude, vertex17.Longitude, 0.0001f, 0.0001f);

            var router = new Router(routerDb);

            var vertices = new Coordinate[] { vertex0, vertex1, vertex2, vertex3, vertex4, vertex5, vertex6, vertex7, vertex8, vertex9,
                vertex10, vertex11, vertex12, vertex13, vertex14, vertex15, vertex16, vertex17 };

            for (int f = 0; f < vertices.Length; f++)
            {
                for (int t = 0; t < vertices.Length; t++)
                {
                    var route = router.Calculate(pedestrian, vertices[f], vertices[t]);
                }
            }
        }

        /// <summary>
        /// Tests routing using and edge-based contracted network.
        /// </summary>
        [Test]
        public void TestEdgeBasedContractedNetwork6()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network6.geojson"));

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);

            routerDb.Network.Sort();
            routerDb.AddContracted(car, true);

            var router = new Router(routerDb);

            var vertices = new Coordinate[] { vertex0, vertex1, vertex2, vertex3, vertex4, vertex5, vertex6, vertex7 };

            for (int f = 0; f < vertices.Length; f++)
            {
                for (int t = 0; t < vertices.Length; t++)
                {
                    var route = router.TryCalculate(car, vertices[f], vertices[t]);
                    Assert.IsFalse(route.IsError);
                }
            }
        }

        /// <summary>
        /// Tests routing on a network with a restriction.
        /// </summary>
        [Test]
        public void TestNetwork7()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network7.geojson"));

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);

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

            routerDb.Sort();
            routerDb.AddContracted(car, true);

            var ids = new uint[vertices.Length];
            for(uint v = 0; v < ids.Length; v++)
            {
                ids[v] = routerDb.SearchVertexFor(vertices[v].Latitude, vertices[v].Longitude);
            }
            
            var router = new Router(routerDb);

            var route = router.Calculate(car, vertices[0], vertices[2]);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(vertices[0].Latitude, route.Shape[0].Latitude);
            Assert.AreEqual(vertices[0].Longitude, route.Shape[0].Longitude);
            Assert.AreEqual(vertices[1].Latitude, route.Shape[1].Latitude);
            Assert.AreEqual(vertices[1].Longitude, route.Shape[1].Longitude);
            Assert.AreEqual(vertices[2].Latitude, route.Shape[2].Latitude);
            Assert.AreEqual(vertices[2].Longitude, route.Shape[2].Longitude);
            route = router.Calculate(car, vertices[2], vertices[0]);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(5, route.Shape.Length);
            Assert.AreEqual(vertices[2].Latitude, route.Shape[0].Latitude);
            Assert.AreEqual(vertices[2].Longitude, route.Shape[0].Longitude);
            Assert.AreEqual(vertices[5].Latitude, route.Shape[1].Latitude);
            Assert.AreEqual(vertices[5].Longitude, route.Shape[1].Longitude);
            Assert.AreEqual(vertices[4].Latitude, route.Shape[2].Latitude);
            Assert.AreEqual(vertices[4].Longitude, route.Shape[2].Longitude);
            Assert.AreEqual(vertices[1].Latitude, route.Shape[3].Latitude);
            Assert.AreEqual(vertices[1].Longitude, route.Shape[3].Longitude);
            Assert.AreEqual(vertices[0].Latitude, route.Shape[4].Latitude);
            Assert.AreEqual(vertices[0].Longitude, route.Shape[4].Longitude);
        }

        /// <summary>
        /// Tests resolving with connectivity checks.
        /// </summary>
        [Test]
        public void TestResolveConnectedNetwork8()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network8.geojson"));
            routerDb.Network.Sort();
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);            
            var router = new Router(routerDb);

            // define 3 location that resolve on an unconnected part of the network.
            var location5 = new Coordinate(52.3531703566375f, 6.6664910316467285f);
            var location6 = new Coordinate(52.3542941977726f, 6.6631704568862915f);
            var location7 = new Coordinate(52.3554049048100f, 6.6648924350738520f);
            
            var point = router.TryResolve(car, location5);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location5, point.Value.LocationOnNetwork(routerDb)) < 20);
            point = router.TryResolveConnected(car, location5, 100, 100);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location5, point.Value.LocationOnNetwork(routerDb)) > 20 &&
                Coordinate.DistanceEstimateInMeter(location5, point.Value.LocationOnNetwork(routerDb)) < 60);

            point = router.TryResolve(car, location6);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location6, point.Value.LocationOnNetwork(routerDb)) < 20);
            point = router.TryResolveConnected(car, location6, 100, 100);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location6, point.Value.LocationOnNetwork(routerDb)) < 20);
            point = router.TryResolveConnected(car, location6, 200, 100);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location6, point.Value.LocationOnNetwork(routerDb)) > 20 &&
                Coordinate.DistanceEstimateInMeter(location6, point.Value.LocationOnNetwork(routerDb)) < 60);

            point = router.TryResolve(car, location7);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location7, point.Value.LocationOnNetwork(routerDb)) < 20);
            point = router.TryResolveConnected(car, location7, 100, 100);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location7, point.Value.LocationOnNetwork(routerDb)) < 20);
            point = router.TryResolveConnected(car, location7, 200, 100);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(location7, point.Value.LocationOnNetwork(routerDb)) > 20 &&
                Coordinate.DistanceEstimateInMeter(location7, point.Value.LocationOnNetwork(routerDb)) < 60);
        }

        /// <summary>
        /// Tests an error causing a U-turn in the resulting route at the end of a dead-end.
        /// </summary>
        [Test]
        public void TestRegressionEdgeBasedRoute1()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            routerDb.Network.Sort();

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var edge1 = router.Resolve(car, location1).EdgeId;
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);
            var edge4 = router.Resolve(car, location4).EdgeId;

            var networkEdge1 = routerDb.Network.GetEdge(edge1);
            var edge1VertexFrom = routerDb.Network.GetVertex(networkEdge1.From);
            var edge1VertexTo = routerDb.Network.GetVertex(networkEdge1.To);
            var networkEdge4 = routerDb.Network.GetEdge(edge4);
            var edge4VertexFrom = routerDb.Network.GetVertex(networkEdge4.From);
            var edge4VertexTo = routerDb.Network.GetVertex(networkEdge4.To);

            // There should be no route found here, we are searching in the dead-end direction of a dead-end street, by 
            // design no route should be returned.
            // [edge1 -> edge4]
            var routeResult = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), edge1 + 1, edge4 + 1, null);
            Assert.AreEqual(true, routeResult.IsError);
        }

        /// <summary>
        /// Tests an error causing a route with identical source and target edge to be of zero length.
        /// </summary>
        [Test]
        public void TestRegressionEdgeBasedRoute2()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "Itinero.Test.test_data.networks.network9.geojson"));
            routerDb.Network.Sort();
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            //routerDb.AddContracted(car, true);
            var router = new Router(routerDb);

            // build the edges array.
            var edges = new uint[]
            {
                router.Resolve(car, new Coordinate(52.35286546406f, 6.66554092450f)).EdgeId,
                router.Resolve(car, new Coordinate(52.35476168070f, 6.66636669078f)).EdgeId,
                router.Resolve(car, new Coordinate(52.35502840541f, 6.66461193744f)).EdgeId,
                router.Resolve(car, new Coordinate(52.35361232125f, 6.66458017720f)).EdgeId,
            };

            var path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), edges[0] + 1, edges[0] + 1, null);
            Assert.IsFalse(path.IsError);
            Assert.IsNotNull(path.Value.From.From);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), -(edges[0] + 1), -(edges[0] + 1), null);
            Assert.IsFalse(path.IsError);
            Assert.IsNotNull(path.Value.From.From);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), edges[1] + 1, edges[1] + 1, null);
            Assert.IsFalse(path.IsError);
            Assert.IsNotNull(path.Value.From.From);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), -(edges[1] + 1), -(edges[1] + 1), null);
            Assert.IsFalse(path.IsError);
            Assert.IsNotNull(path.Value.From.From);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), edges[2] + 1, edges[2] + 1, null);
            Assert.IsFalse(path.IsError);
            Assert.IsNotNull(path.Value.From.From);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), -(edges[2] + 1), -(edges[2] + 1), null);
            Assert.IsFalse(path.IsError);
            Assert.IsNotNull(path.Value.From.From);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), edges[3] + 1, edges[3] + 1, null);
            Assert.IsTrue(path.IsError);
            path = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), -(edges[3] + 1), -(edges[3] + 1), null);
            Assert.IsTrue(path.IsError);
        }
        
        /// <summary>
        /// Tests calculating a directed route.
        /// </summary>
        [Test]
        public void TestDirectedTryCalculate()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "Itinero.Test.test_data.networks.network9.geojson"));

            routerDb.Network.Sort();

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(car, location1);
            var location2 = new Coordinate(52.35476168070f, 6.66636669078f);
            var resolved2 = router.Resolve(car, location2);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(car, location3);
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);
            var resolved4 = router.Resolve(car, location4);
            var location5 = new Coordinate(52.35535575907f, 6.665294766426f);
            var resolved5 = router.Resolve(car, location5);
            var location6 = new Coordinate(52.35345869178f, 6.6661906242371f);
            var resolved6 = router.Resolve(car, location6);

            // 1->5: forward, forward.
            var route = router.Calculate(car, resolved3, resolved5, false, false);
            route = router.Calculate(car, resolved3, resolved5, false, true);
            route = router.Calculate(car, resolved3, resolved5, true, false);
            route = router.Calculate(car, resolved3, resolved5, true, true);

            // contract and try again.
            routerDb.AddContracted(car, true);

            // 1->5: forward, forward.
            route = router.Calculate(car, resolved3, resolved5, false, false);
            route = router.Calculate(car, resolved3, resolved5, false, true);
            route = router.Calculate(car, resolved3, resolved5, true, false);
            route = router.Calculate(car, resolved3, resolved5, true, true);
        }
    }
}