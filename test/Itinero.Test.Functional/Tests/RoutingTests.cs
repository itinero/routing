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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Itinero.Test.Functional.Staging;

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
            
            // run some routing tests for the 'car' profile.
            // normal.
            var profile = router.Db.GetSupportedProfile("car");
            GetTestRandomRoutes(router, profile, 1000).TestPerf($"{profile.FullName} random routes");
            GetTestSequences(router, profile, 1).TestPerf($"{profile.FullName} sequences.");
            // directed.
            GetTestDirectedRandomRoutes(router, profile, 1000).TestPerf($"{profile.FullName} random directed routes");
            GetTestDirectedSequences(router, profile, 1).TestPerf($"{profile.FullName} directed sequences.");
            // one-to-many & many-to-many.
            GetTestOneToManyRoutes(router, profile, 200).TestPerf($"{profile.FullName} one-to-many routes");
            GetTestManyToManyRoutes(router, profile, 20).TestPerf($"{profile.FullName} many-to-many routes");
            
            // run some routing tests for the 'pedestrian' profile.
            profile = router.Db.GetSupportedProfile("pedestrian");
            GetTestRandomRoutes(router, profile, 1000).TestPerf($"{profile.FullName} random routes");
            GetTestSequences(router, profile, 1).TestPerf($"{profile.FullName} sequences.");
            // one-to-many & many-to-many.
            GetTestOneToManyRoutes(router, profile, 200).TestPerf($"{profile.FullName} one-to-many routes");
            GetTestManyToManyRoutes(router, profile, 20).TestPerf($"{profile.FullName} many-to-many routes");
        }

        /// <summary>
        /// Runs a very simple test with a fictional network with negative id's.
        /// </summary>
        public static void RunFictional()
        {
            var stream = new OsmSharp.Streams.XmlOsmStreamSource(File.OpenRead("Tests/fictional.osm"));
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
        public static Func<string> GetTestRandomRoutes(Router router, Profiles.Profile profile, int count)
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
                    }
                }

                return string.Format("{0}/{1} routes failed.", errors, count);
            };
        }

        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Func<string> GetTestDirectedRandomRoutes(Router router, Profiles.Profile profile, int count)
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
                    }
                }

                return string.Format("{0}/{1} routes failed.", errors, count);
            };
        }
        
        /// <summary>
        /// Tests calculating a number of sequences.
        /// </summary>
        public static Func<string> GetTestSequences(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            var list = new List<RouterPoint[]>();

            var locations = StagingHelper.GetLocations("./Tests/data/sequence1.geojson");
            var routerpoints = router.Resolve(profile, locations);
            list.Add(routerpoints);
            
            locations = StagingHelper.GetLocations("./Tests/data/sequence2.geojson");
            routerpoints = router.Resolve(profile, locations);
            list.Add(routerpoints);

            return () =>
            {
                var errors = 0;
                for (var i = 0; i < list.Count; i++)
                {
                    var route = router.TryCalculate(profile, list[i].ToArray());
                    if (route.IsError)
                    {
#if DEBUG
#endif
                        errors++;
                    }
                    else
                    {
#if DEBUG
                        var json = route.Value.ToGeoJson();
#endif
                    }
                }

                return string.Format("{0}/{1} routes failed.", errors, list.Count);
            };
        }

        /// <summary>
        /// Tests calculating a number of directed sequences.
        /// </summary>
        public static Func<string> GetTestDirectedSequences(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            var list = new List<(RouterPoint[] routerpoints, float?[] preferredDirections)>
            {
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence1.geojson")), null),
                // http://geojson.io/#id=gist:xivk/e4d07e82b1abfb424e4626ab0b6fc2c9&map=15/49.8486/6.0846
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence1.geojson")), 
                    new float?[] { 90f, null, null }), 
                // http://geojson.io/#id=gist:xivk/ec116b8a58c0a2a93c7f53a86603139c&map=16/49.8501/6.0792
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence1.geojson")), 
                    new float?[] { null, -90, null }), 
                // http://geojson.io/#id=gist:xivk/e4d07e82b1abfb424e4626ab0b6fc2c9&map=15/49.8486/6.0846
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence1.geojson")), 
                    new float?[] { 90f, null, 90 }), // the last perferred direction is impossible, but this should still work.
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence2.geojson")), null),
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence2.geojson")), 
                    new float?[] { null, null, null, null, null, null, null, 180, null, -90 }),
                (router.Resolve(profile, StagingHelper.GetLocations("./Tests/data/sequence3.geojson")), null)
            };

            return () =>
            {
                var errors = 0;
                for (var i = 0; i < list.Count; i++)
                {
                    var route = router.TryCalculate(profile, list[i].routerpoints, 60, list[i].preferredDirections);
                    if (route.IsError)
                    {
#if DEBUG
#endif
                        errors++;
                    }
                    else
                    {
#if DEBUG
                        var json = route.Value.ToGeoJson();
#endif
                    }
                }

                return string.Format("{0}/{1} routes failed.", errors, list.Count);
            };
        }

        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Func<string> GetTestRandomRoutesParallel(Router router, Profiles.Profile profile, int count)
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

                return string.Format("{0}/{1} routes failed.", errors, count);
            };
        }
        
        /// <summary>
        /// Tests calculating one-to-many routes.
        /// </summary>
        public static Func<PerformanceTestResult<Result<Route[]>>> GetTestOneToManyRoutes(Router router, Profiles.Profile profile, int size)
        {
            var random = new System.Random();
            var list = new List<RouterPoint>();
            while (list.Count < size)
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
            var resolvedPoints = list.ToArray();

            return () => new PerformanceTestResult<Result<Route[]>>(
                router.TryCalculate(profile, resolvedPoints[0], resolvedPoints));
        }
        
        /// <summary>
        /// Tests calculating many-to-many routes.
        /// </summary>
        public static Func<PerformanceTestResult<Result<Route[][]>>> GetTestManyToManyRoutes(Router router, Profiles.Profile profile, int size)
        {
            var random = new System.Random();
            var list = new List<RouterPoint>();
            while (list.Count < size)
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
            var resolvedPoints = list.ToArray();

            return () => new PerformanceTestResult<Result<Route[][]>>(
                router.TryCalculate(profile, resolvedPoints, resolvedPoints));
        }
    }
}