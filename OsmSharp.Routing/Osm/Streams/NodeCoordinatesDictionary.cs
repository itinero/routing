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

using OsmSharp.Collections.Cache;
using OsmSharp.Geo;
using OsmSharp.Math.Geo.Simple;
using Reminiscence.Arrays;
using Reminiscence.IO;
using System;

namespace OsmSharp.Routing.Osm.Streams
{
    /// <summary>
    /// A cache for node coordinates.
    /// </summary>
    public class NodeCoordinatesDictionary
    {
        private readonly Reminiscence.Collections.Dictionary<long, long> _data;

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesDictionary()
            : this(new MemoryMapStream(new System.IO.MemoryStream()), 65536)
        {

        }

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesDictionary(int hashes)
            : this(new MemoryMapStream(new System.IO.MemoryStream()), hashes)
        {

        }

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesDictionary(MemoryMap map)
        {
            _data = new Reminiscence.Collections.Dictionary<long, long>(map, 65536);
        }

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesDictionary(MemoryMap map, int hashes)
        {
            _data = new Reminiscence.Collections.Dictionary<long, long>(map, hashes);
        }

        private byte[] longBytes = new byte[8];

        /// <summary>
        /// Sets the coordinate for the given node.
        /// </summary>
        public void Add(long id, float latitude, float longitude)
        {
            BitConverter.GetBytes(latitude).CopyTo(longBytes, 0);
            BitConverter.GetBytes(longitude).CopyTo(longBytes, 4);

            _data[id] = BitConverter.ToInt64(longBytes, 0);
        }

        /// <summary>
        /// Sets the coordinate for the given node.
        /// </summary>
        public void Add(long id, ICoordinate coordinate)
        {
            this.Add(id, coordinate.Latitude, coordinate.Longitude);
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public bool TryGetValue(long id, out float latitude, out float longitude)
        {
            long longValue;
            if(_data.TryGetValue(id, out longValue))
            {
                var bytes = BitConverter.GetBytes(longValue);
                latitude = BitConverter.ToSingle(bytes, 0);
                longitude = BitConverter.ToSingle(bytes, 4);
                return true;
            }
            latitude = 0;
            longitude = 0;
            return false;
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public bool TryGetValue(long id, out ICoordinate coordinate)
        {
            float lat, lon;
            if (this.TryGetValue(id, out lat, out lon))
            {
                coordinate = new GeoCoordinateSimple()
                {
                    Latitude = lat,
                    Longitude = lon
                };
                return true;
            }
            coordinate = null;
            return false;
        }

        /// <summary>
        /// Returns the number of elements.
        /// </summary>
        public long Count
        {
            get
            {
                return _data.Count;
            }
        }
    }
}