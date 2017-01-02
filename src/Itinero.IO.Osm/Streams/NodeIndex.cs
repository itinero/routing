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

using Itinero.Algorithms.Sorting;
using Itinero.LocalGeo;
using Reminiscence.Arrays;
using Reminiscence.IO;
using System;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A cache for node coordinates.
    /// </summary>
    public class NodeIndex
    {
        // keeps all coordinates in a form [id, lat * 10.000.000, lon * 10.000.000]
        // assumes coordinates are added from a sorted source: TODO: make sure that the source read by the routerdb is sorted.
        // TODO: handle negative coordinates.
        // TODO: fallback to disk when memory usage becomes too much.
        private readonly ArrayBase<int> _data;
        private readonly ArrayBase<int> _index;
        private readonly List<long> _overflows;

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeIndex()
        {
            _index = new MemoryArray<int>(1024 * 1024);
            _data = new MemoryArray<int>(0);
            _overflows = new List<long>();
        }

        /// <summary>
        /// Creates a new node coordinates cache.
        /// </summary>
        public NodeIndex(MemoryMap map)
        {
            _index = new Array<int>(map, 1024 * 1024);
            _data = new Array<int>(map, 0);
            _overflows = new List<long>();
        }

        private long _idx = 0;
        
        /// <summary>
        /// Adds a node id to the index.
        /// </summary>
        public void AddId(long id)
        {
            int int1, int2;
            long2doubleInt(id, out int1, out int2);
            
            if (_idx + 2 >= _index.Length)
            {
                _index.Resize(_index.Length + (1024 * 1024));
            }
            _index[_idx + 0] = int1;
            _index[_idx + 1] = int2;
            _idx += 2;
        }

        static void long2doubleInt(long a, out int a1, out int a2)
        {
            unchecked
            {
                a1 = (int)(a & uint.MaxValue);
                a2 = (int)(a >> 32);
            }
        }

        static long doubleInt2long(int a1, int a2)
        {
            unchecked
            {
                long b = a2;
                b = b << 32;
                b = b | (uint)a1;
                return b;
            }
        }

        /// <summary>
        /// Sorts and converts the index.
        /// </summary>
        public void SortAndConvertIndex()
        {
            _index.Resize(_idx);

            Itinero.Logging.Logger.Log("NodeIndex", Logging.TraceEventType.Information, "Sorting node id's...");
            QuickSort.Sort((i) =>
                {
                    var int1 = _index[i * 2 + 0];
                    var int2 = _index[i * 2 + 1];
                    return doubleInt2long(int1, int2);
                },
                (i, j) =>
                {
                    var int1 = _index[i * 2 + 0];
                    var int2 = _index[i * 2 + 1];
                    _index[i * 2 + 0] = _index[j * 2 + 0];
                    _index[i * 2 + 1] = _index[j * 2 + 1];
                    _index[j * 2 + 0] = int1;
                    _index[j * 2 + 1] = int2;
                }, 0, (_index.Length / 2) - 1);

            for(long i = 0; i < _index.Length / 2; i++)
            {
                var int1 = _index[i * 2 + 0];
                var int2 = _index[i * 2 + 1];
                var id = doubleInt2long(int1, int2);

                if (id >= (long)int.MaxValue * (long)(_overflows.Count + 1))
                { // nodes are overflowing again.
                    _overflows.Add(i);
                }

                _index[i] = (int)(id - ((long)int.MaxValue * (long)_overflows.Count));
            }
            _index.Resize(_index.Length / 2);
            _idx = _index.Length;
        }

        ///// <summary>
        ///// Sets the coordinate for the given node.
        ///// </summary>
        //public void Set(long id, float latitude, float longitude)
        //{
        //    var idx = this.TryGetIndex(id);
        //    if (idx == long.MaxValue)
        //    {
        //        throw new Exception(string.Format("Node {0} not in index!", id));
        //    }

        //    int lat = (int)(latitude * 10000000);
        //    int lon = (int)(longitude * 10000000);

        //    if ((idx * 2) + 1 >= _data.Length)
        //    {
        //        _data.Resize((idx * 2) + 1 + 1024);
        //    }
        //    _data[(idx * 2) + 0] = lat;
        //    _data[(idx * 2) + 1] = lon;
        //}

        ///// <summary>
        ///// Sets the coordinate for the given node.
        ///// </summary>
        //public void Set(long id, Coordinate coordinate)
        //{
        //    this.Set(id, coordinate.Latitude, coordinate.Longitude);
        //}

        /// <summary>
        /// Sets a vertex id for the given vertex.
        /// </summary>
        public void Set(long id, uint vertex)
        {
            var idx = TryGetIndex(id);

            _data[(idx * 2) + 0] = unchecked((int)vertex);
            _data[(idx * 2) + 1] = unchecked((int)uint.MaxValue);
        }

        /// <summary>
        /// Sets the coordinate for the given index.
        /// </summary>
        public void SetIndex(long idx, float latitude, float longitude)
        {
            int lat = (int)(latitude * 10000000);
            int lon = (int)(longitude * 10000000);

            if ((idx * 2) + 1 >= _data.Length)
            {
                var start = _data.Length;
                _data.Resize((idx * 2) + 1 + 1024);
                for(var i = start; i < ((idx * 2) + 1 + 1024); i++)
                {
                    _data[i] = int.MaxValue;
                }
            }
            _data[(idx * 2) + 0] = lat;
            _data[(idx * 2) + 1] = lon;
        }

        /// <summary>
        /// Tries to get a core node and it's matching vertex.
        /// </summary>
        public bool TryGetCoreNode(long id, out uint vertex)
        {
            var idx = TryGetIndex(id);
            if (idx == long.MaxValue)
            {
                vertex = uint.MaxValue;
                return false;
            }

            vertex = unchecked((uint)_data[(idx * 2) + 0]);
            return _data[(idx * 2) + 1] == unchecked((int)uint.MaxValue);
        }

        /// <summary>
        /// Returns true if the given id is a core node.
        /// </summary>
        public bool IsCoreNode(long id)
        {
            if (_previousIndex != long.MaxValue)
            {
                var tempId = GetId(_previousIndex + 1);
                if (tempId == id)
                {
                    if (IsCoreNodeAtIndex(_previousIndex + 1, id))
                    {
                        return true;
                    }
                    return false;
                }
            }
            var idx = TryGetIndex(id);
            if (idx != long.MaxValue)
            {
                _previousIndex = idx;
                if (IsCoreNodeAtIndex(idx, id))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        

        /// <summary>
        /// Returns true if the given id is in this index.
        /// </summary>
        public bool HasId(long id)
        {
            if (_previousIndex != long.MaxValue)
            {
                var tempId = GetId(_previousIndex + 1);
                if (tempId == id)
                {
                    return true;
                }
            }
            var idx = TryGetIndex(id);
            _previousIndex = idx;
            return idx != long.MaxValue;
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public bool TryGetValue(long id, out float latitude, out float longitude, out bool isCore)
        {
            var idx = TryGetIndex(id);
            if (idx == long.MaxValue)
            {
                latitude = float.MaxValue;
                longitude = float.MaxValue;
                isCore = false;
                return false;
            }
            if (!GetLatLon(idx, out latitude, out longitude))
            {
                latitude = float.MaxValue;
                longitude = float.MaxValue;
                isCore = false;
                return false;
            }
            isCore = this.IsCoreNodeAtIndex(idx, id);
            return true;
        }
        
        /// <summary>
        /// Gets all relevant info on the given node.
        /// </summary>
        public bool TryGetValue(long id, out float latitude, out float longitude, out bool isCore, out uint vertex)
        {
            var idx = TryGetIndex(id);
            if (idx == long.MaxValue)
            { // no relevant data here.
                latitude = float.MaxValue;
                longitude = float.MaxValue;
                isCore = false;
                vertex = uint.MaxValue;
                return false;
            }
            else if (_data[(idx * 2) + 1] == unchecked((int)uint.MaxValue))
            { // this is a core-vertex, no coordinates here anymore.
                latitude = float.MaxValue;
                longitude = float.MaxValue;
                isCore = this.IsCoreNodeAtIndex(idx, id);
                vertex = unchecked((uint)_data[(idx * 2) + 0]);
                return true;
            }
            if (GetLatLon(idx, out latitude, out longitude))
            { // no relevant data.
                isCore = this.IsCoreNodeAtIndex(idx, id);
                vertex = uint.MaxValue;
                return true;
            }
            latitude = float.MaxValue;
            longitude = float.MaxValue;
            isCore = false;
            vertex = uint.MaxValue;
            return false;
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public bool TryGetValue(long id, out Coordinate coordinate, out bool isCore)
        {
            float latitude, longitude;
            if (this.TryGetValue(id, out latitude, out longitude, out isCore))
            {
                coordinate = new Coordinate()
                {
                    Latitude = latitude,
                    Longitude = longitude
                };
                return true;
            }
            coordinate = new Coordinate();
            return false;
        }

        /// <summary>
        /// Returns true if a the given index there is an id that represents a core node.
        /// </summary>
        private bool IsCoreNodeAtIndex(long idx, long id)
        {
            if (idx > 0 &&
                GetId(idx - 1) == id)
            {
                return true;
            }
            if (idx < _index.Length - 1 &&
                GetId(idx + 1) == id)
            {
                return true;
            }
            return false;
        }
        
        private long _previousIndex = long.MaxValue;

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public long TryGetIndex(long id)
        {
            //if (_previousIndex != long.MaxValue)
            //{
            //    if (_previousIndex + 1 < _index.Length)
            //    {
            //        var tempId = GetId(_previousIndex + 1);
            //        if (tempId == id)
            //        {
            //            _previousIndex++;
            //            return _previousIndex;
            //        }
            //    }
            //    else
            //    {
            //        _previousIndex = 0;
            //    }
            //}

            // do a binary search.
            long bottom = 0;
            long top = _idx - 1;
            long bottomId = GetId(bottom);
            if (id == bottomId)
            {
                _previousIndex = bottom;
                return bottom;
            }
            long topId = GetId(top);
            if (id == topId)
            {
                while(top - 1 > 0 &&
                    GetId(top - 1) == id)
                {
                    top--;
                }
                _previousIndex = top;
                return top;
            }

            while (top - bottom > 1)
            {
                var middle = (((top - bottom) / 2) + bottom);
                var middleId = GetId(middle);
                if (middleId == id)
                {
                    while (middle - 1 > 0 &&
                        GetId(middle - 1) == id)
                    {
                        middle--;
                    }
                    _previousIndex = middle;
                    return middle;
                }
                if (middleId > id)
                {
                    topId = middleId;
                    top = middle;
                }
                else
                {
                    bottomId = middleId;
                    bottom = middle;
                }
            }
            
            return long.MaxValue;
        }

        private long GetId(long index)
        {
            var overflow = 0;
            for (var i = 0; i < _overflows.Count; i++)
            {
                if (index >= _overflows[i])
                {
                    overflow = i + 1;
                }
                else
                {
                    break;
                }
            }

            return _index[index] + (int.MaxValue * (long)overflow);
        }

        private bool GetLatLon(long index, out float latitude, out float longitude)
        {
            index = index * 2;

            var lat = _data[index + 0];
            var lon = _data[index + 1];

            if (lat == int.MaxValue && lon ==int.MaxValue)
            {
                latitude = float.MaxValue;
                longitude = float.MaxValue;
                return false;
            }

            latitude = (float)(lat / 10000000.0);
            longitude = (float)(lon / 10000000.0);
            return true;
        }
        
        /// <summary>
        /// Returns the number of elements.
        /// </summary>
        public long Count
        {
            get
            {
                return _index.Length;
            }
        }
    }
}