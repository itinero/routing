using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test
{
    /// <summary>
    /// Tests related to the directed routing functionality.
    /// </summary>
    [TestFixture]
    public class RouterDirectedTests
    {
        /// <summary>
        /// Tests directed routing within one edge using exact direction requirements.
        /// </summary>
        [Test]
        public void TestOneEdgeExactDirections()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));
            routerDb.Network.Sort();
            routerDb.Network.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(profile.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(51.22972151230172f, 4.461323618888854f);
            var location2 = new Coordinate(51.22973830827689f, 4.462852478027344f);
            var point1 = router.Resolve(profile, location1);
            var point2 = router.Resolve(profile, location2);
            
            // test forward -> forward.
            var route = router.TryCalculate(profile, point1, true, point2, true);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test forward -> backward
            route = router.TryCalculate(profile, point1, true, point2, false);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);
            
            // test forward -> backward
            route = router.TryCalculate(profile, point1, false, point2, true);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);
            
            // test forward -> backward
            route = router.TryCalculate(profile, point1, false, point2, false);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// Tests directed routing within one edge using exact direction requirements.
        /// </summary>
        [Test]
        public void TestOneEdgeFuzzyDirections()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));
            routerDb.Network.Sort();
            routerDb.Network.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(profile.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(51.22972151230172f, 4.461323618888854f);
            var location2 = new Coordinate(51.22973830827689f, 4.462852478027344f);
            var point1 = router.Resolve(profile, location1);
            var point2 = router.Resolve(profile, location2);

            var settings = new RoutingSettings<float>();
            settings.DirectionAbsolute = false;
            
            // test forward -> forward.
            var route = router.TryCalculate(profile, point1, true, point2, true, settings);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test forward -> backward
            // route is impossible but should still return a route ignoring the direction requirements because of the routing settings.
            route = router.TryCalculate(profile, point1, true, point2, false, settings);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test forward -> backward
            // route is impossible but should still return a route ignoring the direction requirements because of the routing settings.
            route = router.TryCalculate(profile, point1, false, point2, true, settings);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test forward -> backward
            // route is impossible but should still return a route ignoring the direction requirements because of the routing settings.
            route = router.TryCalculate(profile, point1, false, point2, false, settings);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
        }

        /// <summary>
        /// Tests directed routing within one edge using don't-care direction requirements.
        /// </summary>
        [Test]
        public void TestOneEdgeDontCareDirections()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));
            routerDb.Network.Sort();
            routerDb.Network.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(profile.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(51.22972151230172f, 4.461323618888854f);
            var location2 = new Coordinate(51.22973830827689f, 4.462852478027344f); 
            var point1 = router.Resolve(profile, location1);
            var point2 = router.Resolve(profile, location2);
            
            // test forward -> don't care
            var route = router.TryCalculate(profile, point1, true, point2, null);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test don't care -> forward
            route = router.TryCalculate(profile, point1, null, point2, true);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test don't care -> don't care
            route = router.TryCalculate(profile, point1, null, point2, null);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
        }
    }
}