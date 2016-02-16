// Itinero - OpenStreetMap (OSM) SDK
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

using Itinero.Geo;
using System;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A cache for node coordinates.
    /// </summary>
    public class NodeCoordinatesDictionary
    {
        private readonly IDictionary<long, long> _data;

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesDictionary()
        {
            _data = new HugeDictionary<long, long>();
        }

        private byte[] longBytes = new byte[8];

        /// <summary>
        /// Sets the coordinate for the given node.
        /// </summary>
        public void Add(long id, float latitude, float longitude)
        {
            BitConverter.GetBytes(latitude).CopyTo(longBytes, 0);
            BitConverter.GetBytes(longitude).CopyTo(longBytes, 4);

            var value = BitConverter.ToInt64(longBytes, 0);
            _data[id] = value;
        }

        /// <summary>
        /// Sets the coordinate for the given node.
        /// </summary>
        public void Add(long id, Coordinate coordinate)
        {
            this.Add(id, coordinate.Latitude, coordinate.Longitude);
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public bool TryGetValue(long id, out float latitude, out float longitude)
        {
            long longValue;
            if (_data.TryGetValue(id, out longValue))
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
        public bool TryGetValue(long id, out Coordinate coordinate)
        {
            float lat, lon;
            if (this.TryGetValue(id, out lat, out lon))
            {
                coordinate = new Coordinate()
                {
                    Latitude = lat,
                    Longitude = lon
                };
                return true;
            }
            coordinate = default(Coordinate);
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