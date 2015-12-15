// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using OsmSharp.Collections.Tags;
using OsmSharp.Geo.Attributes;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Contracted;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Algorithms.Contracted.Witness;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using OsmSharp.Routing.Network;
using System;
using System.Collections.Generic;
using System.IO;
using OsmSharp.Osm.Streams;

namespace OsmSharp.Routing.Osm
{
    /// <summary>
    /// Contains extension methods for the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, Stream data, params Vehicles.Vehicle[] vehicles)
        {
            db.LoadOsmData(data, false, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, Stream data, bool allCore, params Vehicles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db.");
            }

            // load the data.
            var source = new OsmSharp.Osm.PBF.Streams.PBFOsmStreamSource(data);
            db.LoadOsmData(source, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, params Vehicles.Vehicle[] vehicles)
        {
            db.LoadOsmData(source, false, vehicles);
        }

        /// <summary>
        /// Loads a routing network created from OSM data.
        /// </summary>
        public static void LoadOsmData(this RouterDb db, OsmStreamSource source, bool allCore, params Vehicles.Vehicle[] vehicles)
        {
            if (!db.IsEmpty)
            {
                throw new ArgumentException("Can only load a new routing network into an empty router db.");
            }

            //// load the data.
            //var target = new OsmSharp.Routing.Osm.Streams.RouterDbStreamTarget(db,
            //    vehicles, allCore);
            //target.RegisterSource(source);
            //target.Pull();

            // load the data.
            var target = new OsmSharp.Routing.Osm.Streams.RouterDbStreamTarget(db,
                vehicles, allCore);
            target.RegisterSource(source);
            target.Pull();

            // sort the network.
            db.Network.Sort();
        }
    }
}