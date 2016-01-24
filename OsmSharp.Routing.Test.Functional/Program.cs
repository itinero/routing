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

            // test building a router db.
            Console.WriteLine("Tests building a router db...");
            var routerDb = Runner.TestBuildRouterDb("belgium-latest.osm.pbf", 5000,
                Vehicle.Car);

            // create test router.
            Console.WriteLine("Loading routing data for Belgium...");
            //var routerDb = RouterDb.Deserialize(File.OpenRead("belgium.a.routing"));
            var router = new Router(routerDb);

            // test resolving.
            var embeddedResourceId = "OsmSharp.Routing.Test.Functional.Tests.Belgium.resolve1.geojson";

            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                featureCollection = OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToFeatureCollection(stream.ReadToEnd());
            }

            var performanceInfoConsumer = new PerformanceInfoConsumer(embeddedResourceId);
            performanceInfoConsumer.Start();

            for (var i = 0; i < 10000; i++)
            {
                Tests.Runner.TestResolve(router, featureCollection,
                    Tests.Runner.Default);
            }
            performanceInfoConsumer.Stop();

            Console.WriteLine("Done!");
            Console.ReadLine();
        }
    }
}
