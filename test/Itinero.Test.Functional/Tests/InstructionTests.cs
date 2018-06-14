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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itinero.Test.Functional.Tests
{
    public static class InstructionTests
    {
        /// <summary>
        /// Executes generating instructions on the given routerdb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);

            GetTestInstructionGenerationParallel(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 25).TestPerf("Routing instructions parallel");
        }
        
        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Action GetTestInstructionGenerationParallel(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();

            var routes = new List<Route>();
            while(routes.Count < count)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var v2 = (uint)random.Next((int)router.Db.Network.VertexCount - 1);
                if (v1 == v2)
                {
                    v2++;
                }

                var f1 = router.Db.Network.GetVertex(v1);
                var f2 = router.Db.Network.GetVertex(v2);

                var route = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), f1, f2);
                if (!route.IsError)
                {
                    routes.Add(route.Value);
                }
            }

            return () =>
            {
                foreach (var route in routes)
                {
                    var instructions = route.GenerateInstructions(profile);
                }
            };
        }
    }
}
