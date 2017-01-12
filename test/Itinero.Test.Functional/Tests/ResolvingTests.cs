// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
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