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

using System;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests for some routerdb extension methods.
    /// </summary>
    public static class RouterDbExtensionsTests
    {
        /// <summary>
        /// Runs tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var result = GetExtractBox(routerDb, 49.611432945371156f, 6.207876205444336f,
                49.6213593071641f, 6.2299346923828125f).TestPerf<RouterDb>("Extracting area...");

            var resultJson = result.GetGeoJson();

            // just test some random routes.
            Itinero.Logging.Logger.Log("RouterDbExtensionTests", Logging.TraceEventType.Information,
                "Testing routing on database extract...");
            var router = new Router(result);
            RoutingTests.GetTestRandomRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 1000).TestPerf("Car random routes on extracted db.");
            RoutingTests.GetTestRandomRoutes(router, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), 1000).TestPerf("Pedestrian random routes on extracted db.");
        }

        /// <summary>
        /// Tests writing a shapefile.
        /// </summary>
        public static Func<PerformanceTestResult<RouterDb>> GetExtractBox(RouterDb routerDb, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            return () =>
            {
                return new PerformanceTestResult<RouterDb>(
                    routerDb.ExtractArea(minLatitude, minLongitude, maxLatitude, maxLongitude));
            };
        }
    }
}
