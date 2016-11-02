// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Search.Hilbert;
using Itinero.Geo;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using NUnit.Framework;
using Itinero.Osm.Vehicles;
using System;
using System.IO;
using System.Reflection;
using OsmSharp.Streams;
using Itinero.IO.Osm;
using Itinero.Profiles;
using Itinero.LocalGeo;
using System.Collections.Generic;
using Itinero.Algorithms.Networks.Analytics.Isochrones;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms.Networks.Analytics.Trees;
using Itinero.Algorithms;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// The test runner.
    /// </summary>
    public static class Runner
    {
        /// <summary>
        /// Default resolver test function.
        /// </summary>
        public static Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> Default = (router, coordinate) => 
            {
                return router.TryResolve(Vehicle.Car.Fastest(), coordinate);
            };

        /// <summary>
        /// Tests resolving all points in the given feature collection.
        /// </summary>
        public static void TestResolve(Router router, FeatureCollection features, 
            Func<Router, GeoAPI.Geometries.Coordinate, Result<RouterPoint>> resolve)
        {
            foreach(var feature in features.Features)
            {
                if(feature.Geometry is Point)
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
                var geoJsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
                featureCollection = geoJsonSerializer.Deserialize(jsonReader) as FeatureCollection;
            }
            TestResolve(router, featureCollection, resolve);
        }

        /// <summary>
        /// Tests building a router db.
        /// </summary>
        public static Func<RouterDb> GetTestBuildRouterDb(string file, bool allcore, bool processRestrictions, params Vehicle[] vehicles)
        {
            return () =>
              {
                  OsmStreamSource source;
                  using (var stream = File.OpenRead(file))
                  {
                      var routerdb = new RouterDb();
                      if (file.ToLowerInvariant().EndsWith("osm.pbf"))
                      {
                          source = new OsmSharp.Streams.PBFOsmStreamSource(stream);
                      }
                      else
                      {
                          source = new OsmSharp.Streams.XmlOsmStreamSource(stream);
                      }
                      var progress = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
                      progress.RegisterSource(source);

                      routerdb.LoadOsmData(progress, allcore, processRestrictions, vehicles);

                      return routerdb;
                  }
              };
        }

        /// <summary>
        /// Tests adding a contracted graph.
        /// </summary>
        public static Action GetTestAddContracted(RouterDb routerDb, Profiles.Profile profile, bool forceEdgeBased)
        {
            return () =>
            {
                routerDb.AddContracted(profile.Definition, forceEdgeBased);
            };
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
        public static Action GetTestRandomRoutes(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            return () =>
            {
                var i = count;
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

                    var route = router.TryCalculate(Vehicle.Car.Fastest(), f1, f2);
                }
            };
        }

        /// <summary>
        /// Tests calculating a collection of one to one routes.
        /// </summary>
        public static Func<EdgePath<float>[][]> GetTestManyToManyRoutes(Router router, Profile profile, int size)
        {
            var random = new System.Random();
            var vertices = new HashSet<uint>();
            while(vertices.Count < size)
            {
                var v = (uint)random.Next((int)router.Db.Network.VertexCount);
                if (!vertices.Contains(v))
                {
                    vertices.Add(v);
                }
            }

            var resolvedPoints = new RouterPoint[vertices.Count];
            var i = 0;
            foreach(var v in vertices)
            {
                resolvedPoints[i] = router.Resolve(profile, router.Db.Network.GetVertex(v), 500);
                i++;
            }

            return () =>
             {
                 return router.TryCalculateRaw(profile, router.GetDefaultWeightHandler(profile), resolvedPoints, resolvedPoints, null).Value;
             };
        }

        /// <summary>
        /// Tests detecting islands.
        /// </summary>
        public static Action GetTestIslandDetection(RouterDb routerDb)
        {
            Func<ushort, Factor> profile = (p) =>
            {
                var prof = routerDb.EdgeProfiles.Get(p);
                if (prof != null)
                {
                    var highway = string.Empty;
                    if (prof.TryGetValue("highway", out highway))
                    {
                        if (highway == "motorway" ||
                            highway == "motorway_link")
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
            };

            return () =>
            {
                var islandDetector = new Itinero.Algorithms.Networks.IslandDetector(routerDb, new Func<ushort, Profiles.Factor>[]
                {
                    profile
                });
                islandDetector.Run();
            };
        }

        /// <summary>
        /// Gets a test function to calculate isochrones.
        /// </summary>
        public static Func<List<LocalGeo.Polygon>> GetTestIsochroneCalculation(Router router)
        {
            return () =>
            {
                return router.CalculateIsochrones(Vehicle.Car.Fastest(), new Coordinate(49.80356608186087f, 6.102948188781738f),
                    new List<float>() { 900, 1800 }, 18);
            };
        }

        /// <summary>
        /// Gets a test function to calculate heatmaps.
        /// </summary>
        public static Func<HeatmapResult> GetTestHeatmapCalculation(Router router)
        {
            return () =>
            {
                return router.CalculateHeatmap(Vehicle.Car.Fastest(), new Coordinate(49.80356608186087f, 6.102948188781738f), 1800, 18);
            };
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

                return router.CalculateTree(Vehicle.Car.Fastest(), f, 360);
            };
        }
    }
}