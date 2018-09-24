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

using Itinero.Algorithms.Collections;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;

namespace Itinero.Attributes
{
    /// <summary>
    /// A collection that contains meta-data per unique id, can be used to map meta-data to vertices or edges by their id's.
    /// </summary>
    public class MappedAttributesIndex : IEnumerable<uint>
    {
        private const int _BLOCK_SIZE = 1024;
        private const uint _NO_DATA = uint.MaxValue;
        private readonly ArrayBase<uint> _data; // holds pairs of id's and a pointer to the attribute collection for that id.
        private readonly AttributesIndex _attributes;

        /// <summary>
        /// Creates a new mapped attributes index.
        /// </summary>
        public MappedAttributesIndex(AttributesIndexMode mode = AttributesIndexMode.ReverseCollectionIndex |
                AttributesIndexMode.ReverseStringIndex)
        {
            _data = new MemoryArray<uint>(1024);
            _attributes = new AttributesIndex(mode);
            _reverseIndex = new HugeDictionary<uint, int>();

            for (var p = 0; p < _data.Length; p++)
            {
                _data[p] = _NO_DATA;
            }
        }

        /// <summary>
        /// Creates a new mapped attributes index.
        /// </summary>
        public MappedAttributesIndex(MemoryMap map,
            AttributesIndexMode mode = AttributesIndexMode.ReverseCollectionIndex |
                AttributesIndexMode.ReverseStringIndex)
        {
            _data = new Array<uint>(map, 1024);
            _attributes = new AttributesIndex(map, mode);
            _reverseIndex = new HugeDictionary<uint, int>();

            for (var p = 0; p < _data.Length; p++)
            {
                _data[p] = _NO_DATA;
            }
        }
        
        /// <summary>
        /// Used for deserialization.
        /// </summary>
        private MappedAttributesIndex(ArrayBase<uint> data, AttributesIndex attributes)
        {
            _data = data;
            _pointer = (int)_data.Length;
            _attributes = attributes;

            _reverseIndex = null;
    }

        private HugeDictionary<uint, int> _reverseIndex;
        private int _pointer = 0;

        /// <summary>
        /// Gets or sets attributes for the given id.
        /// </summary>
        public IAttributeCollection this[uint id]
        {
            get
            {
                var p = this.Search(id, out _);
                if (p == _NO_DATA)
                {
                    return null;
                }
                return _attributes.Get(p);
            }
            set
            {
                int idx;
                var p = this.Search(id, out idx);
                if (p == _NO_DATA)
                {
                    this.Add(id, _attributes.Add(value));
                    return;
                }
                _data[idx + 1] = _attributes.Add(value);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<uint> GetEnumerator()
        {
            for (var p = 0; p < _pointer; p += 2)
            {
                yield return _data[p];
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Returns true if this index is sorted and optimized.
        /// </summary>
        public bool IsOptimized
        {
            get
            {
                return _reverseIndex == null;
            }
        }

        /// <summary>
        /// Optimizes this index once it's fully loaded.
        /// </summary>
        public void Optimize()
        {
            // sort array.
            Itinero.Algorithms.Sorting.QuickSort.Sort(i =>
            {
                return _data[i * 2];
            }, (i, j) =>
            {
                var t1 = _data[i * 2 + 0];
                var t2 = _data[i * 2 + 1];
                _data[i * 2 + 0] = _data[j * 2 + 0];
                _data[i * 2 + 1] = _data[j * 2 + 1];
                _data[j * 2 + 0] = t1;
                _data[j * 2 + 1] = t2;
            },
            0, (_pointer / 2) - 1);

            // remove reverse index.
            _reverseIndex = null;

            // reduce array size to exact data size.
            _data.Resize(_pointer);
        }

        /// <summary>
        /// Makes this index writable again, once made writeable it will use more memory and be less efficient, use optimize again once the data is updated.
        /// </summary>
        public void MakeWriteable()
        {
            _reverseIndex = new HugeDictionary<uint, int>();
            for (var p = 0; p < _data.Length; p += 2)
            {
                if (_data[p + 0] == _NO_DATA)
                {
                    continue;
                }
                _reverseIndex[_data[p + 0]] = p + 0;
            }
        }

        /// <summary>
        /// Serializes to the given stream, after optimizing the index, returns the # of bytes written.
        /// </summary>
        public long Serialize(Stream stream)
        {
            if (!this.IsOptimized)
            {
                this.Optimize();
            }

            long size = 1;
            // write the version #
            // 1: initial version.
            stream.WriteByte(1);

            // write data size.
            var bytes = BitConverter.GetBytes((uint)_data.Length);
            stream.Write(bytes, 0, 4);
            size += 4;

            // write data.
            size += _data.CopyTo(stream);

            // write attributes.
            size += _attributes.Serialize(stream);

            return size;
        }

        /// <summary>
        /// Switches the two id's.
        /// </summary>
        public void Switch(uint id1, uint id2)
        {
            if (_reverseIndex == null)
            {
                this.MakeWriteable();
            }

            // remove the two from the index and keep their pointers.
            int pointer1, pointer2;
            if (!_reverseIndex.TryGetValue(id1, out pointer1))
            {
                pointer1 = -1;
            }
            else
            {
                _reverseIndex.Remove(id1);
            }
            if (!_reverseIndex.TryGetValue(id2, out pointer2))
            {
                pointer2 = -1;
            }
            else
            {
                _reverseIndex.Remove(id2);
            }

            // add them again but in reverse.
            if (pointer1 != -1)
            {
                _data[pointer1] = id2;
                _reverseIndex[id2] = pointer1;
            }
            if (pointer2 != -1)
            {
                _data[pointer2] = id1;
                _reverseIndex[id1] = pointer2;
            }
        }

        /// <summary>
        /// Deserializes from the given stream, returns an optimized index.
        /// </summary>
        public static MappedAttributesIndex Deserialize(Stream stream, MappedAttributesIndexProfile profile)
        {
            var version = stream.ReadByte();
            if (version > 1)
            {
                throw new Exception(string.Format("Cannot deserialize mapped attributes index: Invalid version #: {0}, upgrade Itinero.", version));
            }

            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            var length = BitConverter.ToUInt32(bytes, 0);

            ArrayBase<uint> data;
            if (profile == null || profile.DataProfile == null)
            {
                data = Context.ArrayFactory.CreateMemoryBackedArray<uint>(length);
                data.CopyFrom(stream);
            }
            else
            {
                var position = stream.Position;
                var map = new MemoryMapStream(new CappedStream(stream, position,
                    length * 4));
                data = new Array<uint>(map.CreateUInt32(length), profile.DataProfile);
                stream.Seek(length * 4, SeekOrigin.Current);
            }

            var attributes = AttributesIndex.Deserialize(new LimitedStream(stream, stream.Position), true);

            return new MappedAttributesIndex(data, attributes);
        }

        /// <summary>
        /// Searches pointer of the given id, returns uint.maxvalue is no data was found.
        /// </summary>
        private uint Search(uint id, out int idx)
        {
            if (_reverseIndex != null)
            {
                if (_reverseIndex.TryGetValue(id, out idx))
                {
                    return _data[idx + 1];
                }
                return _NO_DATA;
            }

            if (_data == null ||
                _data.Length == 0)
            {
                idx = -1;
                return _NO_DATA;
            }

            // do binary search.
            var left = 0;
            var right = (_pointer - 2) / 2;
            var leftData = _data[left * 2];
            var rightData = _data[right * 2];

            if (leftData == id)
            {
                idx = left;
                return _data[left * 2 + 1];
            }
            if (rightData == id)
            {
                idx = right;
                return _data[right * 2 + 1];
            }

            while (left < right)
            {
                var middle = (left + right) / 2;
                var middleData = _data[middle * 2];

                if (right - left == 1)
                {
                    if (_data[left * 2] == id)
                    {
                        right = left;
                    }
                    else if (_data[right * 2] == id)
                    {
                        left = right;
                    }
                    break; // id doesn't exist.
                }
                if (id < middleData)
                {
                    right = middle;
                }
                else if (id > middleData)
                {
                    left = middle;
                }
                else
                {
                    idx = middle;
                    return _data[middle * 2 + 1];
                }
            }

            idx = -1;
            return _NO_DATA;
        }

        /// <summary>
        /// Adds a new id-attributeId pair.
        /// </summary>
        private void Add(uint id, uint attributeId)
        {
            if (_reverseIndex == null)
            {
                throw new InvalidOperationException(string.Format(
                    "Cannot add new id's to a readonly MappedAttributesIndex, only update existing data. Make index writable again first: {0} not found.", id));
            }
            else
            {
                if (_data.Length <= _pointer + 2)
                {
                    _data.Resize(_data.Length + _BLOCK_SIZE);
                }

                _reverseIndex[id] = _pointer + 0;
                _data[_pointer + 0] = id;
                _data[_pointer + 1] = attributeId;

                _pointer += 2;
            }
        }
    }
}