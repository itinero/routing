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

using NUnit.Framework;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Test.Algorithms.Search;
using Itinero.Test.Profiles;

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
            routerDb.AddSupportedProfile(MockProfile.CarMock());
            var router = new Router(routerDb);
            var called = false;
            router.CreateCustomResolver = (latitude, longitude, isBetter) =>
                {
                    called = true;
                    return new MockResolver(new RouterPoint(latitude, longitude, 0, 0));
                };
            router.Resolve(new Itinero.Profiles.Profile[] { MockProfile.CarMock() }, 0, 0);

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
            routerDb.AddSupportedProfile(car);

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
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved1, point.Value.Location()) < 10);

            point = router.TryResolve(car, location2);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved2, point.Value.Location()) < 10);

            point = router.TryResolve(car, location3);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved3, point.Value.Location()) < 10);

            point = router.TryResolve(car, location4);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved4, point.Value.Location()) < 10);

            router.ProfileFactorCache = new Itinero.Profiles.ProfileFactorCache(router.Db);
            router.ProfileFactorCache.CalculateFor(car);

            point = router.TryResolve(car, location1);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved1, point.Value.Location()) < 10);

            point = router.TryResolve(car, location2);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved2, point.Value.Location()) < 10);

            point = router.TryResolve(car, location3);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved3, point.Value.Location()) < 10);

            point = router.TryResolve(car, location4);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(resolved4, point.Value.Location()) < 10);
        }
    }
}