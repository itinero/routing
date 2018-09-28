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
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using System;
using System.IO;

namespace Sample.Basic
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
            
            using (var stream = File.OpenRead("luxembourg-latest.osm.pbf"))
            {
                routerDb.LoadOsmData(stream, Vehicle.Car);
            }

            // get the profile from the routerdb.
            // this is best-practice in Itinero, to prevent mis-matches.
            var car = routerDb.GetSupportedProfile("car");

            // add a contraction hierarchy.
            routerDb.AddContracted(car);
            
            // create router.
            var router = new Router(routerDb);
            
            // calculate route.
            // this should be the result: http://geojson.io/#id=gist:anonymous/c944cb9741f1fd511c8213b2dd83d58d&map=17/49.75454/6.09571
            var route = router.Calculate(car, new Coordinate(49.75635954613685f, 6.095362901687622f),
                new Coordinate(49.75263039062888f, 6.098860502243042f));
            var routeGeoJson = route.ToGeoJson();
            File.WriteAllText("route1.geojson", routeGeoJson);
            
            // calculate a sequence.
            // this should be the result: http://geojson.io/#id=gist:xivk/760552b0abbcb37a3026273b165f63b8&map=16/49.5881/6.1115
            var locations = new []
            {
                new Coordinate(49.58562050646863f, 6.1020684242248535f), 
                new Coordinate(49.58645517402537f, 6.1063170433044430f), 
                new Coordinate(49.58976588133606f, 6.1078405380249020f),
                new Coordinate(49.59126814499573f, 6.1184406280517570f),
                new Coordinate(49.58816619787410f, 6.1208438873291010f)
            };
            route = router.Calculate(car, locations);
            routeGeoJson = route.ToGeoJson();
            File.WriteAllText("sequence1-undirected.geojson", routeGeoJson);
            
            // calculate a directed sequence with a turn penalty of 120 secs.
            // this should be the result: http://geojson.io/#id=gist:xivk/49f5d843c16adb68c740f8fc0b4d8583&map=16/49.5881/6.1115
            route = router.Calculate(car, locations, turnPenalty: 120, preferredDirections: null); 
            routeGeoJson = route.ToGeoJson();
            File.WriteAllText("sequence2-turn-penalty-120.geojson", routeGeoJson);
            
            // calculate a directed sequence without turn penalty but with a departure angle.
            // this should be the result: http://geojson.io/#id=gist:xivk/c93be9a18072a78ea931dbc5a772f34f&map=16/49.5881/6.1111
            var angles = new float?[]
            {
                -90, // leave west.
                null, // don't-care
                null, // don't-care
                null, // don't-care
                null // don't-care
            };
            route = router.Calculate(car, locations, preferredDirections: angles);
            routeGeoJson = route.ToGeoJson();
            File.WriteAllText("sequence3-preferred-directions.geojson", routeGeoJson);
            
            // calculate a direction with a turn penalty of 120 secs and more preferred departure/arrival angles.
            // this should be the result: http://geojson.io/#id=gist:xivk/660effe2cff422e183aed8efe1fc72c9&map=16/49.5881/6.1112
            angles = new float?[]
            {
                -90, // leave west.
                -90, // pass in western direction.
                null, // don't-care
                null, // don't-care
                -45 // arrive in north-west direction.
            };
            route = router.Calculate(car, locations, turnPenalty: 120, preferredDirections: angles);
            routeGeoJson = route.ToGeoJson();
            File.WriteAllText("sequence4-turn-penalty-120-preferred-directions.geojson", routeGeoJson);
        }
    }
}
