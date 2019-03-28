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

using Itinero.Logging;
using Itinero.Test.Functional.Staging;
using Itinero.Test.Functional.Tests;
using NetTopologySuite.Features;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using Itinero.LocalGeo;
using System.Linq;
using Itinero.Algorithms.Networks.Islands;
using Itinero.Test.Functional.Tests.IO.Shape;

namespace Itinero.Test.Functional
{
    public static class Program
    {
        private static Logger _logger;

        public static void Main(string[] args)
        {
            // enable logging.
            EnableLogging();
            _logger = new Logger("Default");
#if DEBUG
            _logger.Log(TraceEventType.Information, "Performance tests are running in Debug, please run in Release mode.");
#endif
            // download and extract test-data if not already there.
            _logger.Log(TraceEventType.Information, "Downloading Luxembourg...");
            Download.DownloadLuxembourgAll();

            // test building a routerdb.
            _logger.Log(TraceEventType.Information, "Starting tests...");
            var routerDb = RouterDbBuildingTests.Run();
            var router = new Router(routerDb);

            // test some routerdb extensions.
            RouterDbExtensionsTests.Run(routerDb);

            // test resolving.
            ResolvingTests.Run(routerDb);

            // test routing.
            RoutingTests.Run(routerDb);

            // tests calculate weight matrices.
            WeightMatrixTests.Run(routerDb);
            
            // test instruction generation.
            InstructionTests.Run(routerDb);

            // test writing shapefile.
            ShapeFileWriterTests.Run(routerDb);

            _logger.Log(TraceEventType.Information, "Testing finished.");
#if DEBUG
            Console.ReadLine();
#endif
        }

        private static void EnableLogging()
        {
            var loggingBlacklist = new HashSet<string>();
#if DEBUG
#else
            loggingBlacklist = new HashSet<string>(
                new string[] { 
                    "StreamProgress",
                    "RouterDbStreamTarget",
                    "RouterBaseExtensions",
                    "HierarchyBuilder",
                    "RestrictionProcessor",
                    "NodeIndex",
                    "RouterDb",
                    "DuplicateEdgeRemover"
                });
#endif
            OsmSharp.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                if (loggingBlacklist.Contains(o))
                {
                    return;
                }
                Console.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };
            Itinero.Logging.Logger.LogAction = (o, level, message, parameters) =>
            {
                if (loggingBlacklist.Contains(o))
                {
                    return;
                }
                Console.WriteLine(string.Format("[{0}] {1} - {2}", o, level, message));
            };
        }
    }
}
