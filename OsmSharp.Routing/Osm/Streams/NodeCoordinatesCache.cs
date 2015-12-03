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
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Math.Geo.Simple;
using Reminiscence.Arrays;
using Reminiscence.IO;
using System;

namespace OsmSharp.Routing.Osm.Streams
{
    /// <summary>
    /// A cache for node coordinates.
    /// </summary>
    public class NodeCoordinatesCache
    {
        private const long NO_VALUE = -1;
        private readonly ArrayBase<long> _data;

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesCache()
        {
            _data = new MemoryArray<long>(1024 * 2);

            for (var i = 0; i < _data.Length; i++)
            {
                _data[i] = NO_VALUE;
            }
        }

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeCoordinatesCache(MemoryMap map, ArrayProfile profile)
        {
            _data = new Array<long>(map, 1024 * 2);

            for (var i = 0; i < _data.Length; i++)
            {
                _data[i] = NO_VALUE;
            }
        }

        private long _nextIdx = 0;
        private byte[] longBytes = new byte[8];
        private long _lastCheckedIdx = -1;
        private LRUCache<long, long> _cache = new LRUCache<long, long>(1024 * 1024 * 1024);

        /// <summary>
        /// Sets the coordinate for the given node.
        /// </summary>
        public void Add(long id, float latitude, float longitude)
        {
            if(_nextIdx > 0)
            { // check if order is still valid.
                var length = _data.Length;
                while(length <= _nextIdx)
                {
                    length += 2048;
                }
                if (length != _data.Length)
                {
                    var oldLength = _data.Length;
                    _data.Resize(length);

                    for (var i = oldLength; i < _data.Length; i++)
                    {
                        _data[i] = NO_VALUE;
                    }
                }

                if (_data[_nextIdx - 2] >= id)
                {
                    throw new InvalidOperationException("Nodes in source data are not sorted.");
                }
            }

            BitConverter.GetBytes(latitude).CopyTo(longBytes, 0);
            BitConverter.GetBytes(longitude).CopyTo(longBytes, 4);

            _data[_nextIdx + 0] = id;
            _data[_nextIdx + 1] = BitConverter.ToInt64(longBytes, 0);
            _nextIdx += 2;
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
        public bool TryGet(long id, out float latitude, out float longitude)
        {
            // do a binary search for the id.
            long lower = 0;
            long upper = (_nextIdx - 2) / 2;

            long position = long.MinValue;

            // check bounds.
            if(_lastCheckedIdx >= 0 &&
               _lastCheckedIdx + 2 < _data.Length &&
               _data[_lastCheckedIdx + 2] == id)
            {
                position = (_lastCheckedIdx + 2) / 2;
            }
            else if (_lastCheckedIdx >= 2 &&
                _lastCheckedIdx - 2 < _data.Length &&
                _data[_lastCheckedIdx - 2] == id)
            {
                position = (_lastCheckedIdx - 2) / 2;
            }
            else
            {
                var lowerData = _data[lower * 2];
                if (lowerData == id)
                {
                    position = lower;
                }
                else if (lowerData > id)
                {
                    latitude = 0;
                    longitude = 0;
                    return false;
                }
                else
                {
                    var upperData = _data[upper * 2];
                    if (upperData == id)
                    {
                        position = upper;
                    }
                    else if (upperData < id)
                    {
                        latitude = 0;
                        longitude = 0;
                        return false;
                    }
                    else
                    {
                        while (true)
                        {
                            position = ((lower + upper) / 2);
                            var value = this.GetData(position * 2);
                            if (value == id)
                            { // id was found.
                                break;
                            }
                            else if (value >= id)
                            { // position >= than target position.
                                upper = position;
                            }
                            else
                            { // position > than target position.
                                lower = position;
                            }

                            if (upper - lower <= 1)
                            {
                                value = _data[upper * 2];
                                if (value == id)
                                {
                                    position = upper;
                                    break;
                                }
                                value = _data[lower * 2];
                                if (value == id)
                                {
                                    position = lower;
                                    break;
                                }
                                latitude = 0;
                                longitude = 0;
                                return false;
                            }
                        }
                    }
                }
            }

            var longValue = _data[position * 2 + 1];
            var bytes = BitConverter.GetBytes(longValue);
            latitude = BitConverter.ToSingle(bytes, 0);
            longitude = BitConverter.ToSingle(bytes, 4);

            _lastCheckedIdx = position * 2;

            return true;
        }

        /// <summary>
        /// Gets the data at the given index.
        /// </summary>
        /// <returns></returns>
        private long GetData(long idx)
        {
            long value;
            if (!_cache.TryGet(idx, out value))
            {
                value = _data[idx];
                _cache.Add(idx, value);
            }
            return value;
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public bool TryGet(long id, out ICoordinate coordinate)
        {
            float lat, lon;
            if(this.TryGet(id, out lat, out lon))
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
                return _nextIdx / 2;
            }
        }
    }
}