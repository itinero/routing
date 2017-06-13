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
            GetTestRandomRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 1000).TestPerf("Testing car random routes");
            GetTestRandomRoutes(router, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), 1000).TestPerf("Testing pedestrian random routes");
            GetTestRandomDirectedRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 1000).TestPerf("Testing random directed routes");

            // tests many-to-many route calculation.
            GetTestManyToManyRoutes(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 200).TestPerf("Testing calculating manytomany routes");
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
            var list = new List<RouterPoint>();
            while (list.Count < count * 2)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);
                var resolved = router.TryResolve(profile, f1);
                if (resolved.IsError)
                {
                    continue;
                }
                var direction = random.NextDouble() >= 0.5;
                list.Add(resolved.Value);
            }

            return () =>
            {
                var errors = 0;
                var success = 0;
                for (var i = 0; i < list.Count; i += 2)
                {
                    var route = router.TryCalculate(profile, list[i], list[i + 1]);
                    if (route.IsError)
                    {
#if DEBUG
                        var startJson = list[i].ToGeoJson(router.Db);
                        var endJson = list[i + 1].ToGeoJson(router.Db);
#endif
                        errors++;
                    }
                    else
                    {
#if DEBUG
                        var startJson = list[i].ToGeoJson(router.Db);
                        var endJson = list[i + 1].ToGeoJson(router.Db);
#endif
                        success++;
                    }
                }

                Itinero.Logging.Logger.Log("Runner", Logging.TraceEventType.Information, "{0}/{1} routes failed.", errors, count);
            };
        }

        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Action GetTestRandomDirectedRoutes(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            var list = new List<Tuple<RouterPoint, bool>>();
            while (list.Count < count * 2)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);
                var resolved = router.TryResolve(profile, f1);
                if (resolved.IsError)
                {
                    continue;
                }
                var direction = random.NextDouble() >= 0.5;
                list.Add(new Tuple<RouterPoint, bool>(resolved.Value, direction));
            }

            return () =>
            {
                var errors = 0;
                var success = 0;
                for (var i = 0; i < list.Count; i += 2)
                {
                    var route = router.TryCalculate(profile, list[i].Item1, list[i].Item2, 
                        list[i+1].Item1, list[i+1].Item2);
                    if (route.IsError)
                    {
#if DEBUG
                        var startJson = list[i].Item1.ToGeoJson(router.Db);
                        var endJson = list[i + 1].Item1.ToGeoJson(router.Db);
#endif
                        errors++;
                    }
                    else
                    {
#if DEBUG
                        var startJson = list[i].Item1.ToGeoJson(router.Db);
                        var endJson = list[i + 1].Item1.ToGeoJson(router.Db);
#endif
                        success++;
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
            var list = new List<RouterPoint>();
            while (list.Count < count * 2)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);
                var resolved = router.TryResolve(profile, f1);
                if (resolved.IsError)
                {
                    continue;
                }
                var direction = random.NextDouble() >= 0.5;
                list.Add(resolved.Value);
            }

            return () =>
            {
                var errors = 0;
                var success = 0;
                System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
                {
                    int i = x * 2;
                    var route = router.TryCalculate(profile, list[i], list[i + 1]);
                    if (route.IsError)
                    {
#if DEBUG
                        var startJson = list[i].ToGeoJson(router.Db);
                        var endJson = list[i + 1].ToGeoJson(router.Db);
#endif
                        errors++;
                    }
                    else
                    {
#if DEBUG
                        var startJson = list[i].ToGeoJson(router.Db);
                        var endJson = list[i + 1].ToGeoJson(router.Db);
#endif
                        success++;
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