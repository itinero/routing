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

using Itinero.Algorithms.Search.Hilbert;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using OsmSharp.Streams;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using System.Collections.Generic;
using Itinero.Algorithms.Networks.Analytics.Isochrones;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms.Networks.Analytics.Trees;
using Itinero.Algorithms;
using Itinero.Profiles;
using System.Linq;


namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// The test runner.
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Tests resolving all points in the given feature collection.
        /// </summary>
        public static void TestResolve(Router router, FeatureCollection features,
            Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> resolve)
        {
            foreach (var feature in features.Features)
            {
                if (feature.Geometry is Point)
                {
                    Assert.IsNotNull(resolve(router, (feature.Geometry as Point).Coordinate));
                }
            }
        }

        /// <summary>
        /// Tests resolving all points in the given feature collection.
        /// </summary>
        public static void TestResolve(Router router, string embeddedResourceId,
            Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> resolve)
        {
            FeatureCollection featureCollection;
            using (var stream = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedResourceId)))
            {
                var jsonReader = new JsonTextReader(stream);
                var geoJsonSerializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
                featureCollection = geoJsonSerializer.Deserialize(jsonReader) as FeatureCollection;
            }
            TestResolve(router, featureCollection, resolve);
        }

        /// <summary>
        /// Tests reading/writing router db.
        /// </summary>
        public static RouterDb TestReadAndWriterRouterDb(RouterDb routerDb, string file)
        {
            using (var stream = File.OpenWrite(file))
            {
                routerDb.Serialize(stream);
            }

            using (var stream = File.OpenRead(file))
            {
                return RouterDb.Deserialize(stream);
            }
        }
        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Action GetTestInstructionGenerationParallel(Router router, Profiles.Profile profile, int count)
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
                    else
                    {
                        var instructions = route.Value.GenerateInstructions(router.Db);
                    }
                });
                Itinero.Logging.Logger.Log("Runner", Logging.TraceEventType.Information, "{0}/{1} routes failed.", errors, count);
            };
        }

        /// <summary>
        /// Tests detecting islands.
        /// </summary>
        public static Action GetTestIslandDetection(RouterDb routerDb)
        {
            Factor Profile(ushort p)
            {
                var prof = routerDb.EdgeProfiles.Get(p);
                if (prof != null)
                {
                    var highway = string.Empty;
                    if (prof.TryGetValue("highway", out highway))
                    {
                        if (highway == "motorway" || highway == "motorway_link")
                        {
                            return new Profiles.Factor()
                            {
                                Direction = 0,
                                Value = 10
                            };
                        }
                    }
                }

                return new Profiles.Factor()
                {
                    Direction = 0,
                    Value = 0
                };
            }

            return () =>
            {
                var islandDetector = new Itinero.Algorithms.Networks.IslandDetector(routerDb, new Func<ushort, Profiles.Factor>[]
                {
                    Profile
                });
                islandDetector.Run();
            };
        }

        /// <summary>
        /// Gets a test function to calculate isochrones.
        /// </summary>
        public static Func<List<LocalGeo.Polygon>> GetTestIsochroneCalculation(Router router)
        {
            return () => router.CalculateIsochrones(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), new Coordinate(49.80356608186087f, 6.102948188781738f),
                new List<float>() { 900, 1800 }, 18);
        }

        /// <summary>
        /// Gets a test function to calculate heatmaps.
        /// </summary>
        public static Func<HeatmapResult> GetTestHeatmapCalculation(Router router)
        {
            return () => router.CalculateHeatmap(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), new Coordinate(49.80356608186087f, 6.102948188781738f), 1800, 18);
        }

        /// <summary>
        /// Gets a test function to calculate a tree.
        /// </summary>
        public static Func<Algorithms.Networks.Analytics.Trees.Models.Tree> GetTestTreeCalculation(Router router)
        {
            var random = new System.Random();
            return () =>
            {
                var v = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f = router.Db.Network.GetVertex(v);

                return router.CalculateTree(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), f, 360);
            };
        }
    }
}