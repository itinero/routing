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
using Itinero.Elevation;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.LocalGeo.Elevation;
using Itinero.Osm.Vehicles;
using SRTM;
using System;
using System.IO;

namespace Sample.Elevation
{
    class Program
    {
        static void Main(string[] args)
        {
            // enable logging.
            OsmSharp.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                Console.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };

            Download.ToFile("http://files.itinero.tech/data/OSM/planet/europe/luxembourg-latest.osm.pbf", "luxembourg-latest.osm.pbf").Wait();

            // load some routing data and create a router.
            var routerDb = new RouterDb();
            var router = new Router(routerDb);
            using (var stream = File.OpenRead("luxembourg-latest.osm.pbf"))
            {
                routerDb.LoadOsmData(stream, Vehicle.Car);
            }

            // create a new srtm data instance.
            // it accepts a folder to download and cache data into.
            var srtmCache = new DirectoryInfo("srtm-cache");
            if (!srtmCache.Exists)
            {
                srtmCache.Create();
            }
            var srtmData = new SRTMData("srtm-cache");
            ElevationHandler.GetElevation = (lat, lon) =>
            {
                return (short)srtmData.GetElevation(lat, lon);
            };

            // add elevation.
            routerDb.AddElevation();

            // calculate route.
            // this should be the result: http://geojson.io/#id=gist:anonymous/c944cb9741f1fd511c8213b2dd83d58d&map=17/49.75454/6.09571
            var route = router.Calculate(Vehicle.Car.Fastest(), new Coordinate(49.75635954613685f, 6.095362901687622f),
                new Coordinate(49.75263039062888f, 6.098860502243042f));
            var routeGeoJson = route.ToGeoJson();
        }
    }
}
