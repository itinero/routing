/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using NetTopologySuite.Features;
using Itinero.Test.Functional.Staging;
using System;
using System.IO;
using Itinero.Logging;
using Itinero.Test.Functional.Tests;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Data.Contracted;

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
            var router = new Router(routerDb);

            //var networkJson = routerDb.GetGeoJson();

            //ContractedDb contracted;
            //routerDb.TryGetContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), out contracted);
            //var graph = contracted.EdgeBasedGraph;

            //var edgeJson = graph.GetEdgesAsGeoJson(routerDb, 2758);
            //var contractedJson = graph.GetContractedEdgesAsGeoJson(routerDb, 2758);
            ////edgeJson = graph.GetEdgesAsGeoJson(routerDb, 975);

            //var f1 = router.Db.Network.GetVertex(2758);
            //var f2 = router.Db.Network.GetVertex(550);

            ////var route1 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(577)).Value.ToGeoJson();
            //var route2 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(579)).Value.ToGeoJson();
            //var route3 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(567)).Value.ToGeoJson();
            ////var route4 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(539)).Value.ToGeoJson();
            ////var route5 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(530)).Value.ToGeoJson();
            ////var route6 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(531)).Value.ToGeoJson();
            ////var route7 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(550)).Value.ToGeoJson();
            ////var route8 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(517)).Value.ToGeoJson();
            ////var route9 = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), router.Db.Network.GetVertex(2758), router.Db.Network.GetVertex(529)).Value.ToGeoJson();


            ////var vertexJson = graph.GetSearchSpaceAsGeoJson(routerDb, 979, true);
            //var edgeJson = graph.GetEdgesAsGeoJson(routerDb, 2758);
            //edgeJson = graph.GetEdgesAsGeoJson(routerDb, 975);
            //vertexJson = graph.GetSearchSpaceAsGeoJson(routerDb, 579, false);

            //var route = router.Calculate(Osm.Vehicles.Vehicle.Car.Fastest(),
            //    router.Resolve(Osm.Vehicles.Vehicle.Car.Fastest(), 49.501803f, 6.066170f),
            //    router.Resolve(Osm.Vehicles.Vehicle.Car.Fastest(), 49.557734f, 5.884209f));
            //var routeJson = route.ToGeoJson();
            //route = router.Calculate(Osm.Vehicles.Vehicle.Car.Fastest(),
            //    router.Resolve(Osm.Vehicles.Vehicle.Car.Fastest(), 49.51941724047908f, 5.971823036670685f),
            //    router.Resolve(Osm.Vehicles.Vehicle.Car.Fastest(), 49.51588234658069f, 5.973671078681946f));
            //routeJson = route.ToGeoJson();

            //// test resolving.
            //ResolvingTests.Run(routerDb);

            //// test routing.
            //RoutingTests.Run(routerDb);
            //RoutingTests.RunFictional();

            //// tests calculate weight matrices.
            //WeightMatrixTests.Run(routerDb);

            //// test instruction generation.
            //InstructionTests.Run(routerDb);

            _logger.Log(TraceEventType.Information, "Testing finished.");
//#if DEBUG
            Console.ReadLine();
//#endif
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
