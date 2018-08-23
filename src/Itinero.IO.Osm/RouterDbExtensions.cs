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

using Itinero.Algorithms.Networks;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.IO.Osm.Streams;
using Itinero.LocalGeo;
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
        public static void LoadOsmData(this RouterDb db, Stream data, LoadSettings settings, params Itinero.Profiles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db, add multiple streams at once to load multiple files.");
            }

            // load the data.
            var source = new PBFOsmStreamSource(data);
            var progress = new OsmSharp.Streams.Filters.OsmStreamFilterProgress();
            progress.RegisterSource(source);
            db.LoadOsmData(new OsmStreamSource[] { progress }, settings, vehicles);
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
            db.LoadOsmData(sources, new LoadSettings()
            {
                AllCore = allCore,
                Processors = processors,
                ProcessRestrictions = processRestrictions
            }, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, LoadSettings settings, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(new OsmStreamSource[] { source }, settings, vehicles);
        }

        /// <summary>
        /// Loads a routing network from OSM data downloaded from Overpass API.
        /// </summary>
        public static void LoadOsmDataFromOverpass(this RouterDb db, Box box, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmDataFromOverpass(box.ToPolygon(), vehicles);
        }

        /// <summary>
        /// Loads a routing network from OSM data downloaded from Overpass API.
        /// </summary>
        public static void LoadOsmDataFromOverpass(this RouterDb db, Polygon polygon, params Itinero.Profiles.Vehicle[] vehicles)
        {
            var stream = new Overpass.OverpassSourceStream(Overpass.OverpassQueryBuilder.BuildQueryForPolygon(polygon));
            db.LoadOsmData(stream, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource[] sources, LoadSettings settings, params Itinero.Profiles.Vehicle[] vehicles)
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

            if (settings == null)
            {
                settings = new LoadSettings();
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

            // make sure the routerdb can handle multiple edges.
            db.Network.GeometricGraph.Graph.MarkAsMulti();

            // load the data.
            var target = new Streams.RouterDbStreamTarget(db,
                vehicles, settings.AllCore, processRestrictions: settings.ProcessRestrictions, processors: settings.Processors,
                    simplifyEpsilonInMeter: settings.NetworkSimplificationEpsilon);
            target.KeepNodeIds = settings.KeepNodeIds;
            target.KeepWayIds = settings.KeepWayIds;
            target.RegisterSource(source);
            target.Pull();

            // optimize the network.
            db.RemoveDuplicateEdges();
            db.SplitLongEdges();
            db.ConvertToSimple();

            // sort the network.
            db.Sort();

            // optimize the network if requested.
            if (settings.NetworkSimplificationEpsilon > 0)
            {
                db.OptimizeNetwork(settings.NetworkSimplificationEpsilon);
            }

            // compress the network.
            db.Compress();
        }
    }
}