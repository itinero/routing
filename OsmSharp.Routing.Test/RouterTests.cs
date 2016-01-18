// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Test.Algorithms.Search;
using OsmSharp.Routing.Test.Profiles;

namespace OsmSharp.Routing.Test
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
            router.Resolve(new Routing.Profiles.Profile[] { MockProfile.CarMock() }, 0, 0);

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
                    "OsmSharp.Routing.Test.test_data.networks.network3.geojson"));

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

            var car = OsmSharp.Routing.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedProfile(car);

            var location1 = new GeoCoordinate(52.35286546406, 6.66554092450);
            var location2 = new GeoCoordinate(52.35476168070, 6.66636669078);
            var location3 = new GeoCoordinate(52.35502840541, 6.66461193744);
            var location4 = new GeoCoordinate(52.35361232125, 6.66458017720);

            var resolved1 = new GeoCoordinate(52.352949189200494, 6.665348410606384);
            var resolved2 = new GeoCoordinate(52.354736518079150, 6.666120886802673);
            var resolved3 = new GeoCoordinate(52.354974058638945, 6.664675176143646);
            var resolved4 = new GeoCoordinate(52.353594667362720, 6.664688587188721);

            var router = new Router(routerDb);

            var point = router.TryResolve(car, location1);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved1, point.Value.Location()) < 10);

            point = router.TryResolve(car, location2);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved2, point.Value.Location()) < 10);

            point = router.TryResolve(car, location3);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved3, point.Value.Location()) < 10);

            point = router.TryResolve(car, location4);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved4, point.Value.Location()) < 10);

            router.ProfileFactorCache = new Routing.Profiles.ProfileFactorCache(router.Db);
            router.ProfileFactorCache.CalculateFor(car);

            point = router.TryResolve(car, location1);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved1, point.Value.Location()) < 10);

            point = router.TryResolve(car, location2);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved2, point.Value.Location()) < 10);

            point = router.TryResolve(car, location3);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved3, point.Value.Location()) < 10);

            point = router.TryResolve(car, location4);
            Assert.IsFalse(point.IsError);
            Assert.IsTrue(GeoCoordinate.DistanceEstimateInMeter(resolved4, point.Value.Location()) < 10);
        }
    }
}