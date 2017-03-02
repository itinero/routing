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
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains routing tests.
    /// </summary>
    public static class RoutingTests
    {
        /// <summary>
        /// Runs routing tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);

            // just test some random routes.
            GetTestRandomRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 250).TestPerf("Testing random routes");
            GetTestRandomRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 250).TestPerf("Testing random routes in parallel");

            // tests many-to-many route calculation.
            GetTestManyToManyRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 50).TestPerf("Testing calculating manytomany routes");
        }

        /// <summary>
        /// Runs a very simple test with a fictional network with negative id's.
        /// </summary>
        public static void RunFictional()
        {
            var stream = new OsmSharp.Streams.XmlOsmStreamSource(File.OpenRead(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Tests/fictional.osm")));
            var routerDb = new RouterDb();
            routerDb.LoadOsmData(stream, Itinero.Osm.Vehicles.Vehicle.Car);

            var router = new Router(routerDb);
            Assert.IsNotNull(router.Calculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(),
                new Coordinate(51.25758522843f, 4.43582452680f),
                new Coordinate(51.25669892931f, 4.44101728345f)));
        }

        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Action GetTestRandomRoutes(Router router, Profiles.Profile profile, int count)
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
                    var v2 = (uint)random.Next((int)router.Db.Network.VertexCount - 1);
                    if (v1 == v2)
                    {
                        v2++;
                    }

                    var f1 = router.Db.Network.GetVertex(v1);
                    var f2 = router.Db.Network.GetVertex(v2);

                    var route = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), f1, f2);
                    if (route.IsError)
                    {
                        errors++;
                    }
                }

                Itinero.Logging.Logger.Log("Runner", Logging.TraceEventType.Information, "{0}/{1} routes failed.", errors, count);
            };
        }

        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Action GetTestRandomRoutesParallel(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            return () =>
            {
                var errors = 0;
                System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
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
                    if (route.IsError)
                    {
                        errors++;
                    }
                });
                Itinero.Logging.Logger.Log("Runner", Logging.TraceEventType.Information, "{0}/{1} routes failed.", errors, count);
            };
        }
        
        /// <summary>
        /// Tests calculating a collection of one to one routes.
        /// </summary>
        public static Func<EdgePath<float>[][]> GetTestManyToManyRoutes(Router router, Profiles.Profile profile, int size)
        {
            var random = new System.Random();
            var vertices = new HashSet<uint>();
            while (vertices.Count < size)
            {
                var v = (uint)random.Next((int)router.Db.Network.VertexCount);
                if (!vertices.Contains(v))
                {
                    vertices.Add(v);
                }
            }

            var resolvedPoints = new RouterPoint[vertices.Count];
            var i = 0;
            foreach (var v in vertices)
            {
                resolvedPoints[i] = router.Resolve(profile, router.Db.Network.GetVertex(v), 500);
                i++;
            }

            return () =>
            {
                return router.TryCalculateRaw(profile, router.GetDefaultWeightHandler(profile), resolvedPoints, resolvedPoints, null).Value;
            };
        }
    }
}