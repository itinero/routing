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

using Itinero.Data.Contracted;
using Itinero.Algorithms.Networks;
using Itinero.IO.Osm;
using Itinero.Profiles;
using Itinero.Test.Functional.Staging;
using OsmSharp.Streams;
using System;
using System.IO;
using Itinero.Data;
using Itinero.LocalGeo;
using SRTM;
using Itinero.Elevation;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests for routerdb building, loading data and contraction.
    /// </summary>
    public static class RouterDbBuildingTests
    {
        /// <summary>
        /// Tests build a routerdb for luxembourg and returns the result.
        /// </summary>
        /// <returns></returns>
        public static RouterDb Run()
        {
            // GetRouterDbFromOverpass().TestPerf("Loading a routerdb from overpass.");

            var sourcePBF = Download.LuxembourgLocal;
            var routerDb = GetTestBuildRouterDb(sourcePBF, false, true,
                Osm.Vehicles.Vehicle.Car,
                Osm.Vehicles.Vehicle.Bicycle,
                Osm.Vehicles.Vehicle.Pedestrian).TestPerf("Loading OSM data");

            GetTestAddElevation(routerDb).TestPerf("Adding elevation based on SRTM.");

            GetTestAddIslandData(routerDb, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest()).TestPerf("Adding islands for pedestrians.");
            GetTestAddIslandData(routerDb, Itinero.Osm.Vehicles.Vehicle.Car.Fastest()).TestPerf("Adding islands for cars.");

            GetTestAddContracted(routerDb, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest(), false).TestPerf("Build contracted db for pedestrian");
            GetTestAddContracted(routerDb, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), true).TestPerf("Build contracted db for car");

            routerDb = GetTestSerializeDeserialize(routerDb, "luxembourg.c.cf.opt.routerdb").TestPerf("Testing serializing/deserializing routerdb.");

            return routerDb;
        }

        /// <summary>
        /// Tests downloading a routerdb from overpass.
        /// </summary>
        public static Func<PerformanceTestResult<RouterDb>> GetRouterDbFromOverpass()
        {
            return () =>
            {
                var routerDb = new RouterDb();

                routerDb.LoadOsmDataFromOverpass(new Box(51.25380399985758f, 4.809179306030273f,
                    51.273138772415194f, 4.765233993530273f), Itinero.Osm.Vehicles.Vehicle.Car);

                return new PerformanceTestResult<RouterDb>(routerDb)
                {
                    Message = "RouterDb loaded from overpass."
                };
            };
        }
        
        /// <summary>
        /// Tests serialize/deserialize.
        /// </summary>
        public static Func<PerformanceTestResult<RouterDb>> GetTestSerializeDeserialize(RouterDb routerDb, string fileName)
        {
            return () =>
            {
                var bytes = 0L;
                using (var stream = File.Open(fileName, FileMode.Create))
                {
                    routerDb.Serialize(stream);
                    bytes = stream.Position;
                }
                using (var stream1 = File.OpenRead(fileName))
                {
                    routerDb = RouterDb.Deserialize(stream1, RouterDbProfile.NoCache);
                }
                using (var stream1 = File.OpenRead(fileName))
                {
                    routerDb = RouterDb.Deserialize(stream1);
                }
                return new PerformanceTestResult<RouterDb>(routerDb)
                {
                    Message = string.Format("Size of routerdb: {0}", FormatBytes(bytes))
                };
            };
        }

        /// <summary>
        /// Tests building a router db.
        /// </summary>
        public static Func<PerformanceTestResult<RouterDb>> GetTestBuildRouterDb(string file, bool allcore, bool processRestrictions, params Vehicle[] vehicles)
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

                    routerdb.LoadOsmData(progress, new LoadSettings()
                    {
                        AllCore = allcore,
                        ProcessRestrictions = processRestrictions,
                        OptimizeNetwork = true,
                        NetworkSimplificationEpsilon = 1,
                        KeepNodeIds = true,
                        KeepWayIds = true
                    }, vehicles);

                    return new PerformanceTestResult<RouterDb>(routerdb);
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
                routerDb.AddContracted(profile, forceEdgeBased);
            };
        }

        /// <summary>
        /// Tests adding islands data.
        /// </summary>
        public static Action GetTestAddIslandData(RouterDb routerDb, Profiles.Profile profile)
        {
            return () =>
            {
                routerDb.AddIslandData(profile);

#if DEBUG
                MetaCollection<ushort> islands;
                if (routerDb.VertexData.TryGet("islands_" + profile.FullName, out islands))
                {
                    var islandDb = routerDb.ExtractArea(v =>
                        {
                            return islands[v] != ushort.MaxValue;
                        },
                        (f, t) =>
                        {
                            var fromCount = islands[f];
                            var toCount = islands[t];

                            //if (fromCount != Constants.ISLAND_SINGLETON &&
                            //    toCount != Constants.ISLAND_SINGLETON)
                            //{
                            //    return false;
                            //}

                            if (fromCount < 1024 &&
                                fromCount != 0 && fromCount != Constants.ISLAND_RESTRICTED)
                            {
                                return true;
                            }

                            if (toCount < 1024 &&
                                toCount != 0 && toCount != Constants.ISLAND_RESTRICTED)
                            {
                                return true;
                            }

                            if (toCount == Constants.ISLAND_RESTRICTED && fromCount == Constants.ISLAND_RESTRICTED)
                            { // single-vertex restrictions on both sides.
                                return true;
                            }

                            return false;
                        });

                    File.WriteAllText("islands_" + profile.FullName + ".geojson",
                        islandDb.GetGeoJson(true, false));
               }
#endif
            };
        }

        /// <summary>
        /// Tests adding elevation data.
        /// </summary>
        public static Action GetTestAddElevation(RouterDb routerDb)
        {
            // create a new srtm data instance.
            // it accepts a folder to download and cache data into.
            var srtmCache = new DirectoryInfo("srtm-cache");
            if (!srtmCache.Exists)
            {
                srtmCache.Create();
            }
            var srtmData = new SRTMData("srtm-cache");
            LocalGeo.Elevation.ElevationHandler.GetElevation = (lat, lon) =>
            {
                return (short)srtmData.GetElevation(lat, lon);
            };

            return () =>
            {
                routerDb.AddElevation();
            };
        }

        private static string FormatBytes(long bytes)
        {
            string[] Suffix = { "B", "KB", "MB", "GB", "TB" };
            int i;
            double dblSByte = bytes;
            for (i = 0; i < Suffix.Length && bytes >= 1024; i++, bytes /= 1024)
            {
                dblSByte = bytes / 1024.0;
            }

            return String.Format("{0:0.##} {1}", dblSByte, Suffix[i]);
        }

    }
}