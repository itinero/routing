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
using Itinero.Logging;
using System.Collections.Generic;
using Itinero.Profiles;
using Itinero.Algorithms.Networks;
using Itinero.LocalGeo;
using Itinero;
using Itinero.Algorithms.Networks.Analytics.Isochrones;

namespace Itinero.Test.Functional
{
    class Program
    {
        private static Logger _logger;

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
            _logger = new Logger("Default");

            Itinero.Osm.Vehicles.Vehicle.RegisterVehicles();

            // download and extract test-data if not already there.
            _logger.Log(TraceEventType.Information, "Downloading Luxembourg...");
            Download.DownloadLuxembourgAll();

            // TEST1: Tests building a router db for cars, contracting it and calculating routes.
            // test building a router db.
            var routerDb = Runner.GetTestBuildRouterDb(Download.LuxembourgLocal, false, false, Vehicle.Car).TestPerf("Build belgium router db for Car.");
            var router = new Router(routerDb);

            // build profile cache.
            var profileCache = new Profiles.ProfileFactorAndSpeedCache(routerDb);
            profileCache.CalculateFor(Vehicle.Car.Fastest());
            profileCache.CalculateFor(Vehicle.Bicycle.Fastest());
            profileCache.CalculateFor(Vehicle.Pedestrian.Fastest());
            router.ProfileFactorAndSpeedCache = profileCache;

            Runner.GetTestAddContracted(routerDb, Vehicle.Car.Fastest(), true).TestPerf("Add contracted graph for Car.Fastest()");
            Runner.GetTestRandomRoutes(new Router(routerDb), Vehicle.Car.Fastest(), 1000).TestPerf("Testing route calculation speed.");

            // TEST2: Tests find islands.
            Func<ushort, Factor> profile = (p) =>
            {
                var prof = routerDb.EdgeProfiles.Get(p);
                if (prof != null)
                {
                    var highway = string.Empty;
                    if (prof.TryGetValue("highway", out highway))
                    {
                        if (highway == "motorway" ||
                            highway == "motorway_link")
                        {
                            return new Profiles.Factor()
                            {
                                Direction = 0,
                                Value = 10
                            };
                        }
                    }
                }
                return new Profiles.Factor()
                {
                    Direction = 0,
                    Value = 0
                };
            };
            Runner.GetTestIslandDetection(routerDb, profile).TestPerf("Testing island detection.", 10);

            // TEST3: calulate isochrones.
            var polygons = Runner.GetTestIsochroneCalculation(router).TestPerf("Testing isochrone calculation.", 10);

            _logger.Log(TraceEventType.Information, "Testing finished.");
            Console.ReadLine();
        }

        private static string ToJson(FeatureCollection featureCollection)
        {
            var jsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
            var jsonStream = new StringWriter();
            jsonSerializer.Serialize(jsonStream, featureCollection);
            var json = jsonStream.ToInvariantString();
            return json;
        }
    }
}
