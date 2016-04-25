// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Itinero.Geo;
using Itinero.Osm.Vehicles;
using Itinero.Test.Functional.Staging;
using Itinero.Test.Functional.Tests;
using System;
using System.IO;
using System.Reflection;

namespace Itinero.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}] {1} - {2}", origin, level, message));
            };
            Itinero.Logging.Logger.LogAction = (origin, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}] {1} - {2}", origin, level, message));
            };

            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            // download and extract test-data.
            Console.WriteLine("Downloading Belgium...");
            Download.DownloadBelgiumAll();

            // test building a router db.
            Console.WriteLine("Tests building a router db...");
            var routerDb = Runner.TestBuildRouterDb(@"C:\Users\xivk\Dropbox\SharpSoftware\Projects\Eurostation ReLive\Projects\Gent-Circulatieplan\circulatieplan\circulatieplan.after.osm", 
                Vehicle.Car, Vehicle.Bicycle);
            //routerDb.AddContracted(Vehicle.Car.Fastest());

            //routerDb.Serialize(File.OpenWrite(@"D:\work\data\OSM\belgium.c.cf.routing"));

            //////routerDb.AddContracted(Vehicle.Car.Fastest());
            //var routerDb = RouterDb.Deserialize(
            //    File.OpenRead(@"D:\work\data\OSM\routing\planet\europe\belgium.c.cf.new.routing"));
            var router = new Router(routerDb);
            // 51.0516&fLon=3.7321&tLat=51.0522&tLon=3.7324
            var route = router.Calculate(Vehicle.Car.Fastest(), 51.051600f, 3.732100f,
                51.052100f, 3.732400f);
            var route1 = router.Calculate(Vehicle.Bicycle.Fastest(), 51.051600f, 3.732100f,
                51.052100f, 3.732400f);

            //// test resolving.
            //var embeddedResourceId = "Itinero.Test.Functional.Tests.Belgium.resolve1.geojson";

            //FeatureCollection featureCollection;
            //using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            //{
            //    var jsonReader = new JsonTextReader(stream);
            //    var geoJsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
            //    featureCollection = geoJsonSerializer.Deserialize(jsonReader) as FeatureCollection;
            //}

            //var performanceInfoConsumer = new PerformanceInfoConsumer(embeddedResourceId);
            //performanceInfoConsumer.Start();

            //for (var i = 0; i < 10000; i++)
            //{
            //    Tests.Runner.TestResolve(router, featureCollection,
            //        Tests.Runner.Default);
            //}
            //performanceInfoConsumer.Stop();

            //var performanceInfoConsumer = new PerformanceInfoConsumer("Routing");
            //performanceInfoConsumer.Start();

            //for (var i = 0; i < 10; i++)
            //{
            //    foreach(var source in featureCollection.Features)
            //    {
            //        var sourcePoint = router.TryResolve(Vehicle.Car.Fastest(), 
            //            (source.Geometry as Point).Coordinate);
            //        if(sourcePoint.IsError)
            //        {
            //            continue;
            //        }
            //        foreach (var target in featureCollection.Features)
            //        {
            //            var targetPoint = router.TryResolve(Vehicle.Car.Fastest(),
            //                (target.Geometry as Point).Coordinate);
            //            if (targetPoint.IsError)
            //            {
            //                continue;
            //            }

            //            var route = router.Calculate(Vehicle.Car.Fastest(),
            //                sourcePoint.Value, targetPoint.Value);
            //        }
            //    }
            //}

            //performanceInfoConsumer.Stop();

            Console.ReadLine();
        }
    }
}
