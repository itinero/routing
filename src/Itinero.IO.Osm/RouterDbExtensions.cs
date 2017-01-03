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

using Itinero.Algorithms.Search.Hilbert;
using Itinero.IO.Osm.Streams;
using OsmSharp;
using OsmSharp.Streams;
using OsmSharp.Streams.Filters;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.IO.Osm
{
    /// <summary>
    /// Contains extension methods for the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, Stream data, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(data, false, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, Stream data, bool allCore = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db, add multiple streams at once to load multiple files.");
            }

            // load the data.
            var source = new PBFOsmStreamSource(data);
            var progress = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
            progress.RegisterSource(source);
            db.LoadOsmData(progress, allCore, true, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, Stream data, bool allCore = false, bool processRestrictions = true, params Itinero.Profiles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db, add multiple streams at once to load multiple files.");
            }

            // load the data.
            var source = new PBFOsmStreamSource(data);
            var progress = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
            progress.RegisterSource(source);
            db.LoadOsmData(progress, allCore, processRestrictions, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmSharp.Streams.OsmEnumerableStreamSource(source), vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmStreamSource[] { source }, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource[] sources, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(sources, false, true, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, bool allCore = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmSharp.Streams.OsmEnumerableStreamSource(source), allCore, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, bool allCore = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmStreamSource[] { source }, allCore, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource[] sources, bool allCore = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(sources, allCore, true, null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, bool allCore = false, bool processRestrictions = true, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmSharp.Streams.OsmEnumerableStreamSource(source), allCore, processRestrictions, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, bool allCore = false, bool processRestrictions = true, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmStreamSource[] { source }, allCore, processRestrictions, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource[] sources, bool allCore = false, bool processRestrictions = true, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(sources, allCore, processRestrictions, null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, bool allCore = false, bool processRestrictions = true,
            IEnumerable<ITwoPassProcessor> processors = null, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmSharp.Streams.OsmEnumerableStreamSource(source), allCore, processRestrictions, processors, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, bool allCore = false, bool processRestrictions = true,
            IEnumerable<ITwoPassProcessor> processors = null, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmStreamSource[] { source }, allCore, processRestrictions, processors, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource[] sources, bool allCore = false, bool processRestrictions = true, 
            IEnumerable<ITwoPassProcessor> processors = null, params Itinero.Profiles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db, add multiple streams at once to load multiple files.");
            }
            if (vehicles == null || vehicles.Length == 0)
            {
                throw new ArgumentNullException("vehicles", "A least one vehicle is needed to load OSM data.");
            }
            if (sources == null || sources.Length == 0)
            {
                throw new ArgumentNullException("sources", "A least one source is needed to load OSM data.");
            }

            // merge sources if needed.
            var source = sources[0];
            for (var i = 1; i < sources.Length; i++)
            {
                var merger = new OsmSharp.Streams.Filters.OsmStreamFilterMerge();
                merger.RegisterSource(source);
                merger.RegisterSource(sources[i]);
                source = merger;
            }

            if (sources.Length > 1 && !(source is OsmStreamFilterProgress))
            { // just one source the the callee is choosing a progress filter but assumed the default for a merged stream.
                var progress = new OsmStreamFilterProgress();
                progress.RegisterSource(source);
                source = progress;
            }

            // load the data.
            var target = new Streams.RouterDbStreamTarget(db,
                vehicles, allCore, processRestrictions: processRestrictions, processors: processors);
            target.RegisterSource(source);
            target.Pull();

            // sort the network.
            db.Sort();
        }
    }
}