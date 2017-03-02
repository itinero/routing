// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NetTopologySuite.Features;
using Itinero.Test.Functional.Staging;
using System;
using System.IO;
using Itinero.Logging;
using Itinero.Test.Functional.Tests;
using Itinero.IO.Osm;
using Itinero.LocalGeo;

namespace Itinero.Test.Functional
{
    public class Program
    {
        private static Logger _logger;

        public static void Main(string[] args)
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

#if DEBUG
            _logger.Log(TraceEventType.Information, "Performance tests are running in Debug, please run in Release mode.");
#endif
            // download and extract test-data if not already there.
            _logger.Log(TraceEventType.Information, "Downloading Luxembourg...");
            Download.DownloadLuxembourgAll();

            // test building a routerdb.
            var routerDb = RouterDbBuildingTests.Run();

            // test resolving.
            ResolvingTests.Run(routerDb);

            // test routing.
            RoutingTests.Run(routerDb);
            RoutingTests.RunFictional();

            // tests calculate weight matrices.
            WeightMatrixTests.Run(routerDb);

            // test instruction generation.
            InstructionTests.Run(routerDb);

            _logger.Log(TraceEventType.Information, "Testing finished.");
#if DEBUG
            Console.ReadLine();
#endif
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
