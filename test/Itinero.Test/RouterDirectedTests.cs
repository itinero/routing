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
            route = router.TryCalculate(profile, point1, (bool?)null, point2, null);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
        }
        
        /// <summary>
        /// Tests directed routing within one edge using exact direction requirements.
        /// </summary>
        [Test]
        public void TestOneEdgeExactDirectionsWithAngles()
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
            var route = router.TryCalculate(profile, point1, 90, point2, 90);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test forward -> backward
            route = router.TryCalculate(profile, point1, 90, point2, -90);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);
            
            // test forward -> backward
            route = router.TryCalculate(profile, point1, -90, point2, 90);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);
            
            // test forward -> backward
            route = router.TryCalculate(profile, point1, -90, point2, -90);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);
            
            // test forward -> forward.
            route = router.TryCalculate(profile, point1, 90 - 15, point2, 90 + 15);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[route.Value.Shape.Length - 1], point2.LocationOnNetwork(routerDb)) < 1);
        }
        
        /// <summary>
        /// Tests directed routing within one edge using exact direction requirements.
        /// </summary>
        [Test]
        public void TestNetwork3DirectedWithAngles()
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
            routerDb.Network.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(profile.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var location2 = new Coordinate(52.35476168070f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);
            
            var point1 = router.Resolve(profile, location1);
            var point2 = router.Resolve(profile, location2);
            var point3 = router.Resolve(profile, location3);
            var point4 = router.Resolve(profile, location4);
            
            // test with angles along edges without don't care strategy.
            var route = router.TryCalculate(profile, point1, 45, point2, -10);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(3, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex1) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, 45, point2, 170);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(9, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex1) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], vertex4) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[6], vertex3) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[7], vertex0) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[8], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point2, -10);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], vertex1) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[6], point2.LocationOnNetwork(routerDb)) < 1);

            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point2, 170);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex4) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex3) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], vertex0) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[6], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point4, -135);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(6, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], point4.LocationOnNetwork(routerDb)) < 1);

            // test with angles along edges without don't care strategy, this route should fail because the arrival direction is impossible.
            route = router.TryCalculate(profile, point1, -135, point4, 45);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);

            var settings = new RoutingSettings<float>();
            settings.DirectionAbsolute = false;
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point4, null, settings: settings);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(6, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], point4.LocationOnNetwork(routerDb)) < 1);
        }
        
        /// <summary>
        /// Tests directed routing within one edge using exact direction requirements.
        /// </summary>
        [Test]
        public void TestNetwork3ContractedDirectedWithAngles()
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
            routerDb.Network.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(profile.Parent);
            routerDb.AddContracted(profile, true);
            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var location2 = new Coordinate(52.35476168070f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);
            
            var point1 = router.Resolve(profile, location1);
            var point2 = router.Resolve(profile, location2);
            var point3 = router.Resolve(profile, location3);
            var point4 = router.Resolve(profile, location4);
            
            // test with angles along edges without don't care strategy.
            var route = router.TryCalculate(profile, point1, 45, point2, -10);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);

            var json = route.Value.ToGeoJson();

            Assert.AreEqual(3, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex1) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, 45, point2, 170);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(9, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex1) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], vertex4) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[6], vertex3) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[7], vertex0) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[8], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point2, -10);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], vertex1) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[6], point2.LocationOnNetwork(routerDb)) < 1);

            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point2, 170);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex4) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex3) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], vertex0) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[6], point2.LocationOnNetwork(routerDb)) < 1);
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point4, -135);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(6, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], point4.LocationOnNetwork(routerDb)) < 1);

            // test with angles along edges without don't care strategy, this route should fail because the arrival direction is impossible.
            route = router.TryCalculate(profile, point1, -135, point4, 45);
            Assert.IsNotNull(route);
            Assert.IsTrue(route.IsError);

            var settings = new RoutingSettings<float>();
            settings.DirectionAbsolute = false;
            
            // test with angles along edges without don't care strategy.
            route = router.TryCalculate(profile, point1, -135, point4, null, settings: settings);
            Assert.IsNotNull(route);
            Assert.IsFalse(route.IsError);
            Assert.IsNotNull(route.Value.Shape);
            Assert.AreEqual(6, route.Value.Shape.Length);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[0], point1.LocationOnNetwork(routerDb)) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[1], vertex7) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[2], vertex6) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[3], vertex5) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[4], vertex2) < 1);
            Assert.IsTrue(Coordinate.DistanceEstimateInMeter(route.Value.Shape[5], point4.LocationOnNetwork(routerDb)) < 1);
        }
    }
}