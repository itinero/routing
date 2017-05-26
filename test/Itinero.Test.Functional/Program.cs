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
using Itinero.Graphs.Directed;
using Itinero.Algorithms;
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
            RouterDb routerDb = null;
            using (var stream = File.OpenRead("temp.routerdb"))
            {
                routerDb = RouterDb.Deserialize(stream);
            }

            var profile = routerDb.GetSupportedProfile("car");
            var action = new Action(() =>
            {
                routerDb.AddContracted(profile, true);
            });
            action.TestPerf("Contraction");
            
            //    var router = new Router(routerDb);

            //var profile = routerDb.GetSupportedProfile("car");
            //ContractedDb contractedDb;
            //routerDb.TryGetContracted(profile, out contractedDb);
            //var target = contractedDb.NodeBasedGraph;
            //var random = new System.Random();
            //var ticks = DateTime.Now.Ticks;
            //var r = 0;
            //while (true)
            //{
            //    var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
            //    var v2 = (uint)random.Next((int)router.Db.Network.VertexCount - 1);
            //    if (v1 == v2)
            //    {
            //        v2++;
            //    }

            //    try
            //    {
            //        var f1 = router.Db.Network.GetVertex(v1);
            //        var f2 = router.Db.Network.GetVertex(v2);

            //        var resolved1 = router.Resolve(profile, f1);
            //        var resolved2 = router.Resolve(profile, f2);

            //        //var rawPath = router.TryCalculateRaw<float>(profile, router.GetDefaultWeightHandler(profile),
            //        //    resolved1.EdgeId + 1, -resolved2.EdgeId + 1, null);
            //        var route = router.TryCalculate(profile, resolved1, resolved2);
                    
            //        r++;

            //        if (r % 10 == 0)
            //        {
            //            var time = new TimeSpan(DateTime.Now.Ticks - ticks);
            //            Console.WriteLine("Calculated {0} routes in {1}: {2} s/route",
            //                r, time.ToInvariantString(), time.TotalSeconds / r);
            //        }
            //    }
            //    catch (Exception ex)
            //    {

            //    }
            //}

            //var route = router.Calculate(Osm.Vehicles.Vehicle.Car.Fastest(),
            //    router.Resolve(Osm.Vehicles.Vehicle.Car.Fastest(), 49.501803f, 6.066170f),
            //    router.Resolve(Osm.Vehicles.Vehicle.Car.Fastest(), 49.557734f, 5.884209f));

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
