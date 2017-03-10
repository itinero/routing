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

using Itinero.Algorithms;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains resolving tests.
    /// </summary>
    public static class ResolvingTests
    {
        /// <summary>
        /// Runs resolving tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);

            GetTestRandomResolves(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 1000).TestPerf("Testing random resolves");
            GetTestRandomResolvesParallel(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 1000).TestPerf("Testing random resolves in parallel");
        }
        
        /// <summary>
        /// Tests a number of resolves.
        /// </summary>
        public static Action GetTestRandomResolves(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            return () =>
            {
                var i = count;
                var errors = 0;
                while (i > 0)
                {
                    i--;

                    var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                    var f1 = router.Db.Network.GetVertex(v1);

                    var routerPoint = router.TryResolve(profile, f1);
                    if (routerPoint.IsError)
                    {
                        errors++;
                    }
                }

                Itinero.Logging.Logger.Log("Runner", Logging.TraceEventType.Information, "{0}/{1} resolves failed.", errors, count);
            };
        }

        /// <summary>
        /// Tests a number of resolves.
        /// </summary>
        public static Action GetTestRandomResolvesParallel(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            return () =>
            {
                var errors = 0;
                System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
                {
                    var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                    var f1 = router.Db.Network.GetVertex(v1);

                    var routerPoint = router.TryResolve(profile, f1);
                    if (routerPoint.IsError)
                    {
                        errors++;
                    }
                });
                Itinero.Logging.Logger.Log("Runner", Logging.TraceEventType.Information, "{0}/{1} resolves failed.", errors, count);
            };
        }

    }
}