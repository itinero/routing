using System;
using System.IO;
using Itinero;
using Itinero.Attributes;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Attribute = Itinero.Attributes.Attribute;
using Vehicle = Itinero.Osm.Vehicles.Vehicle;

namespace Sample.DynamicProfiles
{
    class Program
    {
        static void Main(string[] args)
        {            
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                Console.WriteLine($"[{o}] {level} - {message}");
            };
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                Console.WriteLine($"[{o}] {level} - {message}");
            };

            Download.ToFile("http://planet.anyways.eu/planet/europe/luxembourg/luxembourg-latest.osm.pbf", "luxembourg-latest.osm.pbf").Wait();

            // load some routing data and create a router.
            var routerDb = new RouterDb();
            
            var customCar = DynamicVehicle.Load(File.ReadAllText("custom-car.lua"));
            using (var stream = File.OpenRead("luxembourg-latest.osm.pbf"))
            {
                routerDb.LoadOsmData(stream, customCar);
            }
            
            // add custom profiles.
            var speed10 = routerDb.EdgeProfiles.Add(new AttributeCollection(
                new Attribute("highway", "residential"),
                new Attribute("custom-speed", "10")));
            var speed20 = routerDb.EdgeProfiles.Add(new AttributeCollection(
                new Attribute("highway", "residential"),
                new Attribute("custom-speed", "20")));
            var speed30 = routerDb.EdgeProfiles.Add(new AttributeCollection(
                new Attribute("highway", "residential"),
                new Attribute("custom-speed", "30")));
            var speed40 = routerDb.EdgeProfiles.Add(new AttributeCollection(
                new Attribute("highway", "residential"),
                new Attribute("custom-speed", "40")));

            // define locations, profile and router.
            var location1 = new Coordinate(49.88826851632804f, 5.815232992172241f);
            var location2 = new Coordinate(49.88775699771737f, 5.8133286237716675f);
            var router = new Router(routerDb);
            
            // calculate route before.
            var routeBefore = router.Calculate(customCar.Fastest(), location1, location2);
            var routeBeforeGeoJson = routeBefore.ToGeoJson();
            
            // resolve an edge.
            var edgeLocation = new Coordinate(49.888040407347006f, 5.8142513036727905f);
            var resolved = router.Resolve(customCar.Fastest(), edgeLocation);
            
            // update the speed profile of this edge.
            var edgeData = routerDb.Network.GetEdge(resolved.EdgeId).Data;
            edgeData.Profile = (ushort)speed10;
            routerDb.Network.UpdateEdgeData(resolved.EdgeId, edgeData);
            
            // calculate route.
            var routeAfter = router.Calculate(customCar.Fastest(), location1, location2);
            var routeAfterGeoJson = routeAfter.ToGeoJson();
            
            // calculate route to middle of edge.
            var location3 = new Coordinate(49.888035223039466f, 5.814205706119537f);
            var routeAfter13 = router.Calculate(customCar.Fastest(), location1, location3);
            var routeAfter13GeoJson = routeAfter13.ToGeoJson();
        }
    }
}