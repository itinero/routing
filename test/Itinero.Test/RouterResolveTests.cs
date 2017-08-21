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
using Itinero.LocalGeo;
using Itinero.Profiles;
using Itinero.Test.Algorithms.Search;
using Itinero.Test.Profiles;
using NUnit.Framework;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests related to the resolving code in the router class.
    /// </summary>
    [TestFixture]
    public class RouterResolveTests
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
        /// Tests resolving using island meta data.
        /// </summary>
        [Test]
        public void TestResolveUsingIslandMetaData()
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

            // try resolving some locations.
            var router = new Router(routerDb);
            var location1 = new Coordinate(51.265711288086045f, 4.798069596290588f);
            var location4 = new Coordinate(51.28462836628995f, 4.788472652435303f);

            var resolved1 = router.TryResolve(new IProfileInstance[] { profile }, 
                location1.Latitude, location1.Longitude, null, 50, null);
            var resolved4 = router.TryResolve(new IProfileInstance[] { profile },
                location4.Latitude, location4.Longitude, null, 50, null);
            Assert.IsFalse(resolved1.IsError);
            Assert.IsTrue(resolved4.IsError);
            resolved4 = router.TryResolve(new IProfileInstance[] { profile },
                location4.Latitude, location4.Longitude, null, 5000, null);
            Assert.IsFalse(resolved4.IsError);
        }
    }
}