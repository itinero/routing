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

using Itinero.IO.Osm;
using Itinero.Profiles;
using Itinero.Test.Functional.Staging;
using OsmSharp.Streams;
using System;
using System.IO;

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
            var routerDb = GetTestBuildRouterDb(Download.LuxembourgLocal, false, true,
                Itinero.Osm.Vehicles.Vehicle.Car).TestPerf("Loading OSM data");

            //GetTestAddContracted(routerDb, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), true).TestPerf("Adding contracted db");
            
            return routerDb;
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
                routerDb.AddContracted(profile, forceEdgeBased);
            };
        }
    }
}