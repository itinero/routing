// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
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
using Itinero.IO.Osm.Normalizer;
using Itinero.IO.Osm.Streams;
using OsmSharp;
using OsmSharp.Streams;
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
                throw new ArgumentException("Can only load a new routing network into an empty router db.");
            }

            // load the data.
            var source = new PBFOsmStreamSource(data);
            db.LoadOsmData(source, allCore, false, true, new DefaultTagNormalizer(), null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, Stream data, bool allCore = false, bool processRestrictions = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db.");
            }

            // load the data.
            var source = new PBFOsmStreamSource(data);
            db.LoadOsmData(source, allCore, processRestrictions, true, new DefaultTagNormalizer(), null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(source, false, false, true, new DefaultTagNormalizer(), null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, bool allCore = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(source, allCore, false, true, new DefaultTagNormalizer(), null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, bool allCore = false, bool processRestrictions = false, params Itinero.Profiles.Vehicle[] vehicles)
        {
            db.LoadOsmData(source, allCore, processRestrictions, true, new DefaultTagNormalizer(), null, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, IEnumerable<OsmGeo> source, bool allCore = false, bool processRestrictions = false, bool normalizeTags = true, 
            ITagNormalizer tagNormalizer = null, IEnumerable<ITwoPassProcessor> processors = null, params Itinero.Profiles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db.");
            }

            if (normalizeTags && tagNormalizer == null)
            {
                tagNormalizer = new DefaultTagNormalizer();
            }

            // load the data.
            var target = new Streams.RouterDbStreamTarget(db,
                vehicles, tagNormalizer, allCore, processRestrictions: processRestrictions, normalizeTags: normalizeTags, processors: processors);
            target.RegisterSource(source);
            target.Pull();

            // sort the network.
            db.Sort();
        }
    }
}