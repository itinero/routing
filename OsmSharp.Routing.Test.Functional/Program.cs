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

using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.IO.Json.Linq;
using OsmSharp.Routing.Osm.Vehicles;
using OsmSharp.Routing.Test.Functional.Staging;
using OsmSharp.Routing.Test.Functional.Tests;
using System;
using System.IO;
using System.Reflection;

namespace OsmSharp.Routing.Test.Functional
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging.
            OsmSharp.Logging.Log.Enable();
            OsmSharp.Logging.Log.RegisterListener(new ConsoleTraceListener());

            OsmSharp.Routing.Osm.Vehicles.Vehicle.RegisterVehicles();

            // download and extract test-data.
            Console.WriteLine("Downloading Belgium...");
            Download.DownloadBelgiumAll();

            //// test building a router db.
            //Console.WriteLine("Tests building a router db...");
            //var routerDb = Runner.TestBuildRouterDb("belgium-latest.osm.pbf", Vehicle.Car);
            //routerDb.AddContracted(Vehicle.Car.Fastest());
            var routerDb = RouterDb.Deserialize(
                File.OpenRead(@"D:\work\data\routing\planet\europe\belgium.c.cf.routing"));
            var router = new Router(routerDb);

            // test resolving.
            var embeddedResourceId = "OsmSharp.Routing.Test.Functional.Tests.Belgium.resolve1.geojson";

            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                featureCollection = OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToFeatureCollection(stream.ReadToEnd());
            }

            //var performanceInfoConsumer = new PerformanceInfoConsumer(embeddedResourceId);
            //performanceInfoConsumer.Start();

            //for (var i = 0; i < 10000; i++)
            //{
            //    Tests.Runner.TestResolve(router, featureCollection,
            //        Tests.Runner.Default);
            //}
            //performanceInfoConsumer.Stop();

            var performanceInfoConsumer = new PerformanceInfoConsumer("Routing");
            performanceInfoConsumer.Start();

            for (var i = 0; i < 10; i++)
            {
                foreach(var source in featureCollection)
                {
                    var sourcePoint = router.TryResolve(Vehicle.Car.Fastest(), 
                        (source.Geometry as Point).Coordinate);
                    if(sourcePoint.IsError)
                    {
                        continue;
                    }
                    foreach (var target in featureCollection)
                    {
                        var targetPoint = router.TryResolve(Vehicle.Car.Fastest(),
                            (target.Geometry as Point).Coordinate);
                        if (targetPoint.IsError)
                        {
                            continue;
                        }
                        
                        var route = router.Calculate(Vehicle.Car.Fastest(),
                            sourcePoint.Value, targetPoint.Value);
                    }
                }
            }

            performanceInfoConsumer.Stop();

            Console.ReadLine();
        }
    }
}
