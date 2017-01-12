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

            GetTestAddContracted(routerDb, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), true).TestPerf("Adding contracted db");
            
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