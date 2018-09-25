// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Itinero;
using Itinero.Data.Edges;
using Itinero.IO.Shape;
using Itinero.LocalGeo;
using Itinero.Profiles;
using Itinero.Geo;
using System;
using System.IO;

namespace Sample.Shape
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };

            // download test data and extract to 'temp' directory relative to application base directory.
            Download.DownloadAndExtractShape("http://files.itinero.tech/data/open-data/NWB/WGS84_2016-09-01.zip", "WGS84_2016-09-01.zip");

            // create a new router db and load the shapefile.
            var vehicle = DynamicVehicle.LoadFromStream(File.OpenRead(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "car.lua")));
            var routerDb = new RouterDb(EdgeDataSerializer.MAX_DISTANCE);
            routerDb.LoadFromShape(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp"), "wegvakken.shp", vehicle);

            // OPTIONAL: build a contracted version of the routing graph.
            // routerDb.AddContracted(vehicle.Fastest());

            // write the router db to disk for later use.
            routerDb.Serialize(File.OpenWrite(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "nwb.routerdb")));

            // calculate a test route.
            var router = new Router(routerDb);
            var route = router.Calculate(vehicle.Fastest(), new Coordinate(52.954718f, 6.338811f),
                new Coordinate(52.95359f, 6.337916f));
            route = router.Calculate(vehicle.Fastest(), new Coordinate(51.57060821506861f, 5.46792984008789f),
                new Coordinate(51.58711643524425f, 5.4957228899002075f));

            // generate instructions based on lua profile.
            var instructions = route.GenerateInstructions(routerDb);
        }
    }
}