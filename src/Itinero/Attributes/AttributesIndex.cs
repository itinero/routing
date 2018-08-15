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

using System;
using System.Collections;
using Itinero.Algorithms.Collections;
using Reminiscence.Arrays;
using Reminiscence.Indexes;
using Reminiscence.IO;
using Reminiscence.IO.Streams;

namespace Itinero.Attributes
{
    /// <summary>
    /// An index for attribute collections.
    /// </summary>
    public class AttributesIndex
    {
        private readonly Index<string> _stringIndex;
        private readonly Index<int[]> _collectionIndex;
        private readonly ArrayBase<uint> _index;
        private bool _isReadonly = false;
        private readonly AttributesIndexMode _mode;
        private const uint NULL_ATTRIBUTES = 0;
        private const uint EMPTY_ATTRIBUTES = 1;

        private System.Collections.Generic.IDictionary<string, int> _stringReverseIndex; // Holds all strings and their id.
        private System.Collections.Generic.IDictionary<int[], uint> _collectionReverseIndex; // Holds all tag collections and their reverse index.

        /// <summary>
        /// Creates a new empty index.
        /// </summary>
        public AttributesIndex(AttributesIndexMode mode = AttributesIndexMode.ReverseCollectionIndex |
            AttributesIndexMode.ReverseStringIndex)
        {
            _stringIndex = new Index<string>();
            _collectionIndex = new Index<int[]>();
            _isReadonly = false;
            _mode = mode;
            _stringReverseIndex = null;
            _collectionReverseIndex = null;

            if ((_mode & AttributesIndexMode.IncreaseOne) == AttributesIndexMode.IncreaseOne)
            {
                _index = Context.ArrayFactory.CreateMemoryBackedArray<uint>(1024);
                _nextId = 0;
            }

            if ((_mode & AttributesIndexMode.ReverseStringIndex) == AttributesIndexMode.ReverseStringIndex ||
                (_mode & AttributesIndexMode.ReverseStringIndexKeysOnly) == AttributesIndexMode.ReverseStringIndexKeysOnly)
            {
                _stringReverseIndex = new System.Collections.Generic.Dictionary<string, int>();
            }
            if ((_mode & AttributesIndexMode.ReverseCollectionIndex) == AttributesIndexMode.ReverseCollectionIndex)
            {
                _collectionReverseIndex = new System.Collections.Generic.Dictionary<int[], uint>(
                    new DelegateEqualityComparer<int[]>(
                        (obj) =>
                        { // assumed the array is sorted.
                            var hash = obj.Length.GetHashCode();
                            for (int idx = 0; idx < obj.Length; idx++)
                            {
                                hash = hash ^ obj[idx].GetHashCode();
                            }
                            return hash;
                        },
                        (x, y) =>
                        {
                            if (x.Length == y.Length)
                            {
                                for (int idx = 0; idx < x.Length; idx++)
                                {
                                    if (x[idx] != y[idx])
                                    {
                                        return false;
                                    }
                                }
                                return true;
                            }
                            return false;
                        }));
            }
        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        public AttributesIndex(MemoryMap map,
            AttributesIndexMode mode = AttributesIndexMode.ReverseCollectionIndex |
            AttributesIndexMode.ReverseStringIndex)
        {
            if (mode == AttributesIndexMode.None) { throw new ArgumentException("Cannot create a new index without a valid operating mode."); }

            _stringIndex = new Index<string>(map);
            _collectionIndex = new Index<int[]>(map);
            _isReadonly = false;
            _mode = mode;
            _stringReverseIndex = null;
            _collectionReverseIndex = null;

            if ((_mode & AttributesIndexMode.IncreaseOne) == AttributesIndexMode.IncreaseOne)
            { // create the increment-by-one data structures.
                _index = new Array<uint>(map, 1024);
                _nextId = 0;
            }

            if ((_mode & AttributesIndexMode.ReverseStringIndex) == AttributesIndexMode.ReverseStringIndex ||
                (_mode & AttributesIndexMode.ReverseStringIndexKeysOnly) == AttributesIndexMode.ReverseStringIndexKeysOnly)
            {
                _stringReverseIndex = new Reminiscence.Collections.Dictionary<string, int>(map, 1024 * 16);
            }
            if ((_mode & AttributesIndexMode.ReverseCollectionIndex) == AttributesIndexMode.ReverseCollectionIndex)
            {
                _collectionReverseIndex = new Reminiscence.Collections.Dictionary<int[], uint>(map, 1024 * 16,
                    new DelegateEqualityComparer<int[]>(
                        (obj) =>
                        { // assumed the array is sorted.
                            var hash = obj.Length.GetHashCode();
                            for (int idx = 0; idx < obj.Length; idx++)
                            {
                                hash = hash ^ obj[idx].GetHashCode();
                            }
                            return hash;
                        },
                        (x, y) =>
                        {
                            if (x.Length == y.Length)
                            {
                                for (int idx = 0; idx < x.Length; idx++)
                                {
                                    if (x[idx] != y[idx])
                                    {
                                        return false;
                                    }
                                }
                                return true;
                            }
                            return false;
                        }));
            }
        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        internal AttributesIndex(AttributesIndexMode mode, Index<string> stringIndex, Index<int[]> tagsIndex)
        {
            _stringIndex = stringIndex;
            _collectionIndex = tagsIndex;
            _isReadonly = true;
            _index = null;
            _nextId = uint.MaxValue;
            _mode = mode;

            _stringReverseIndex = null;
            _collectionReverseIndex = null;
        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        internal AttributesIndex(AttributesIndexMode mode, Index<string> stringIndex, Index<int[]> tagsIndex, ArrayBase<uint> index)
        {
            _stringIndex = stringIndex;
            _collectionIndex = tagsIndex;
            _isReadonly = true;
            _index = index;
            _nextId = (uint) index.Length;
            _mode = mode;

            _stringReverseIndex = null;
            _collectionReverseIndex = null;
        }

        private uint _nextId;

        /// <summary>
        /// Returns true if this index is readonly.
        /// </summary>
        public bool IsReadonly
        {
            get { return _isReadonly; }
        }

        /// <summary>
        /// Returns true if this index checks for duplicates.
        /// </summary>
        public bool CheckDuplicates
        {
            get { return _stringReverseIndex != null; }
        }

        /// <summary>
        /// Gets the number of collections.
        /// </summary>
        public uint Count
        {
            get
            {
                if ((_mode & AttributesIndexMode.IncreaseOne) == AttributesIndexMode.IncreaseOne)
                { // uses increase one.
                    return _nextId + 2;
                }
                if ((_mode & AttributesIndexMode.None) == AttributesIndexMode.None && _index != null)
                { // deserialized but used increase one before.
                    return _nextId + 2;
                }
                throw new Exception("Count cannot be calculated on a index that doesn't use 'IncreaseOne' mode.");
            }
        }

        /// <summary>
        /// Returns the index mode.
        /// </summary>
        public AttributesIndexMode IndexMode
        {
            get
            {
                return _mode;
            }
        }

        /// <summary>
        /// Returns the attributes that belong to the given id.
        /// </summary>
        public IAttributeCollection Get(uint tagsId)
        {
            if (tagsId == 0)
            {
                return null;
            }
            else if (tagsId == 1)
            {
                return new AttributeCollection();
            }
            if (_index != null)
            { // use the index if it's there.
                tagsId = _index[tagsId - 2];
                return new InternalAttributeCollection(_stringIndex, _collectionIndex.Get(tagsId));
            }
            return new InternalAttributeCollection(_stringIndex, _collectionIndex.Get(tagsId - 2));
        }

        /// <summary>
        /// Adds new attributes.
        /// </summary>
        public uint Add(IAttributeCollection tags)
        {
            if (tags == null)
            {
                return NULL_ATTRIBUTES;
            }
            else if (tags.Count == 0)
            {
                return EMPTY_ATTRIBUTES;
            }

            if (_isReadonly)
            { // this index is readonly.
                // TODO: make writeable.
                // - set nextId.
                // - create reverse indexes if needed.
                if (_index != null)
                { // this should be an increase-one index.
                    if ((_mode & AttributesIndexMode.IncreaseOne) != AttributesIndexMode.IncreaseOne)
                    {
                        throw new Exception("Invalid combination of data: There is an index but mode isn't increase one.");
                    }

                    _nextId = (uint) _index.Length;
                }

                // build reverse indexes if needed.
                if ((_mode & AttributesIndexMode.ReverseStringIndex) == AttributesIndexMode.ReverseStringIndex ||
                    (_mode & AttributesIndexMode.ReverseStringIndexKeysOnly) == AttributesIndexMode.ReverseStringIndexKeysOnly)
                {
                    _stringReverseIndex = new Reminiscence.Collections.Dictionary<string, int>(
                        new MemoryMapStream(), 1024 * 16);

                    // add existing data.
                    if ((_mode & AttributesIndexMode.ReverseStringIndex) == AttributesIndexMode.ReverseStringIndex)
                    { // build reverse index for all data.
                        foreach (var pair in _stringIndex)
                        {
                            _stringReverseIndex[pair.Value] = (int) pair.Key;
                        }
                    }
                    else
                    { // build reverse index for keys only.
                        foreach (var collectionPair in _collectionIndex)
                        {
                            foreach (var stringId in collectionPair.Value)
                            {
                                _stringReverseIndex[_stringIndex.Get(stringId)] = stringId;
                            }
                        }
                    }
                }

                if ((_mode & AttributesIndexMode.ReverseCollectionIndex) == AttributesIndexMode.ReverseCollectionIndex)
                {
                    _collectionReverseIndex = new Reminiscence.Collections.Dictionary<int[], uint>(new MemoryMapStream(), 1024 * 16,
                        new DelegateEqualityComparer<int[]>(
                            (obj) =>
                            { // assumed the array is sorted.
                                var hash = obj.Length.GetHashCode();
                                for (int idx = 0; idx < obj.Length; idx++)
                                {
                                    hash = hash ^ obj[idx].GetHashCode();
                                }
                                return hash;
                            },
                            (x, y) =>
                            {
                                if (x.Length == y.Length)
                                {
                                    for (int idx = 0; idx < x.Length; idx++)
                                    {
                                        if (x[idx] != y[idx])
                                        {
                                            return false;
                                        }
                                    }
                                    return true;
                                }
                                return false;
                            }));
                    if (_index != null)
                    {
                        for (uint col = 0; col < _nextId; col++)
                        {
                            var pointer = _index[col];
                            _collectionReverseIndex[_collectionIndex.Get(pointer)] = col;
                        }
                    }
                    else
                    {
                        foreach (var pair in _collectionIndex)
                        {
                            _collectionReverseIndex[pair.Value] = (uint)pair.Key;
                        }
                    }
                }

                _isReadonly = false;
            }

            // add new collection.
            var sortedSet = new SortedSet<long>();
            foreach (var tag in tags)
            {
                sortedSet.Add((long) this.AddString(tag.Key, true) +
                    (long) int.MaxValue * (long) this.AddString(tag.Value, false));
            }

            // sort keys.
            var sorted = new int[sortedSet.Count * 2];
            var i = 0;
            foreach (var pair in sortedSet)
            {
                sorted[i] = (int) (pair % int.MaxValue);
                i++;
                sorted[i] = (int) (pair / int.MaxValue);
                i++;
            }

            // add sorted collection.
            return this.AddCollection(sorted);
        }

        /// <summary>
        /// Adds a new string.
        /// </summary>
        private int AddString(string value, bool key)
        {
            int id;
            if ((_mode & AttributesIndexMode.ReverseStringIndex) == AttributesIndexMode.ReverseStringIndex ||
                (((_mode & AttributesIndexMode.ReverseStringIndexKeysOnly) == AttributesIndexMode.ReverseStringIndexKeysOnly) && key))
            {
                if (!_stringReverseIndex.TryGetValue(value, out id))
                { // the key doesn't exist yet.
                    id = (int) _stringIndex.Add(value);
                    _stringReverseIndex.Add(value, id);
                }
                return id;
            }
            return (int) _stringIndex.Add(value);
        }

        /// <summary>
        /// Adds a new collection, it's assumed to be sorted.
        /// </summary>
        private uint AddCollection(int[] collection)
        {
            uint id;
            if (_collectionReverseIndex != null)
            {
                // check duplicates.
                if (_collectionReverseIndex.TryGetValue(collection, out id))
                { // collection already exists.
                    return id + 2;
                }
            }

            id = (uint) _collectionIndex.Add(collection);
            if (_index != null)
            { // use next id.
                _index.EnsureMinimumSize(_nextId + 1);
                _index[_nextId] = id;
                id = _nextId;
                _nextId++;
            }
            if (_collectionReverseIndex != null)
            {
                _collectionReverseIndex.Add(collection, id);
            }
            return id + 2;
        }

        /// <summary>
        /// An implementation of a tags collection.
        /// </summary>
        private class InternalAttributeCollection : IAttributeCollection
        {
            private Index<string> _stringIndex; // Holds the string index.
            private int[] _tags; // Holds the tags.

            /// <summary>
            /// Creates a new internal attributes collection.
            /// </summary>
            public InternalAttributeCollection(Index<string> stringIndex, int[] tags)
            {
                _stringIndex = stringIndex;
                _tags = tags;
            }

            /// <summary>
            /// Returns the number of attributes in this collection.
            /// </summary>
            public int Count
            {
                get { return _tags.Length / 2; }
            }

            /// <summary>
            /// Returns true if this collection is readonly.
            /// </summary>
            public bool IsReadonly
            {
                get { return true; }
            }

            /// <summary>
            /// Returns true if the given tag exists.
            /// </summary>
            public bool TryGetValue(string key, out string value)
            {
                for (var i = 0; i < _tags.Length; i = i + 2)
                {
                    if (key == _stringIndex.Get(_tags[i]))
                    {
                        value = _stringIndex.Get(_tags[i + 1]);
                        return true;
                    }
                }
                value = null;
                return false;
            }

            /// <summary>
            /// Removes the attribute with the given key.
            /// </summary>
            public bool RemoveKey(string key)
            {
                throw new InvalidOperationException("This attribute collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Adds or replaces an attribute.
            /// </summary>
            public void AddOrReplace(string key, string value)
            {
                throw new InvalidOperationException("This attribute collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Clears all attributes.
            /// </summary>
            public void Clear()
            {
                throw new InvalidOperationException("This attribute collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Returns the enumerator for this enumerable.
            /// </summary>
            public System.Collections.Generic.IEnumerator<Attribute> GetEnumerator()
            {
                return new InternalTagsEnumerator(_stringIndex, _tags);
            }

            /// <summary>
            /// Returns the enumerator for this enumerable.
            /// </summary>
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            /// <summary>
            /// Gets a proper description of this attribute collection.
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                var builder = new System.Text.StringBuilder();
                foreach (var a in this)
                {
                    if (builder.Length > 0)
                    {
                        builder.Append('|');
                    }
                    builder.Append(a.ToString());
                }
                return builder.ToString();
            }
        }

        /// <summary>
        /// An internal implementation of an attribute enumerator.
        /// </summary>
        private class InternalTagsEnumerator : System.Collections.Generic.IEnumerator<Attribute>
        {
            private Index<string> _stringIndex; // Holds the string index.
            private int[] _tags; // Holds the tags.

            /// <summary>
            /// Creates a new internal tags collection.
            /// </summary>
            public InternalTagsEnumerator(Index<string> stringIndex, int[] tags)
            {
                _stringIndex = stringIndex;
                _tags = tags;
            }

            /// <summary>
            /// Holds the current idx.
            /// </summary>
            private int _idx = -2;

            /// <summary>
            /// Returns the current tag.
            /// </summary>

            public Attribute Current
            {
                get
                {
                    return new Attribute()
                    {
                        Key = _stringIndex.Get(_tags[_idx]),
                            Value = _stringIndex.Get(_tags[_idx + 1])
                    };
                }
            }

            /// <summary>
            /// Returns the current tag.
            /// </summary>
            object System.Collections.IEnumerator.Current
            {
                get
                {
                    return new Attribute()
                    {
                        Key = _stringIndex.Get(_tags[_idx]),
                            Value = _stringIndex.Get(_tags[_idx + 1])
                    };
                }
            }

            /// <summary>
            /// Move to the next attribute.
            /// </summary>
            public bool MoveNext()
            {
                _idx = _idx + 2;
                return _idx < _tags.Length;
            }

            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _idx = -2;
            }

            /// <summary>
            /// Disposes this enumerator.
            /// </summary>
            public void Dispose()
            {
                _tags = null;
                _stringIndex = null;
            }
        }

        #region Serialization

        /// <summary>
        /// Serializes this tags index to the given stream.
        /// </summary>
        public long Serialize(System.IO.Stream stream)
        {
            // version history.
            // version 0-1: unused, fallback to unversioned.
            // version 2: first version that contains information to make indexed writable again.

            // write version #.
            long size = 1;
            stream.WriteByte(2);

            // write index type flags.
            size++;
            stream.WriteByte((byte) _mode);

            // write the actual data.
            if (_index == null)
            { // this is a regular index.
                stream.WriteByte(0);
                size++;
                size += _collectionIndex.CopyToWithSize(stream);
                size += _stringIndex.CopyToWithSize(stream);
            }
            else
            { // this is an increase one index.
                // compress index.
                _index.Resize(_nextId);

                stream.WriteByte(1);
                size++;
                size += _collectionIndex.CopyToWithSize(stream);
                size += _stringIndex.CopyToWithSize(stream);
                stream.Write(BitConverter.GetBytes(_index.Length), 0, 8);
                size += 8;
                size += _index.CopyTo(stream);
            }

            return size;
        }

        /// <summary>
        /// Deserializes a tags index from the given stream.
        /// </summary>
        public static AttributesIndex Deserialize(System.IO.Stream stream, bool copy = false,
            AttributesIndexMode defaultIndexMode = AttributesIndexMode.ReverseStringIndexKeysOnly)
        {
            // read version byte.
            long position = 1;
            var version = stream.ReadByte();

            int type = 0;
            if (version < 2)
            { // unversioned version.
                type = (byte) version;
            }
            else
            { // versioned.
                // read the index mode.
                var indexModeByte = stream.ReadByte();
                position++;
                defaultIndexMode = (AttributesIndexMode) indexModeByte;

                // read the type.
                type = stream.ReadByte();
                position++;
            }

            // read the actual data.
            long size;
            if (type == 0)
            { // regular index.
                var tagsIndex = Index<int[]>.CreateFromWithSize(stream, out size, !copy);
                position += size + 8;
                stream.Seek(position, System.IO.SeekOrigin.Begin);
                var limitedStream = new LimitedStream(stream);
                var stringIndex = Index<string>.CreateFromWithSize(limitedStream, out size, !copy);
                position += size + 8;
                stream.Seek(position, System.IO.SeekOrigin.Begin);
                return new AttributesIndex(defaultIndexMode, stringIndex, tagsIndex);
            }
            else
            { // increase one index.
                var tagsIndex = Index<int[]>.CreateFromWithSize(stream, out size, !copy);
                position += size + 8;
                stream.Seek(position, System.IO.SeekOrigin.Begin);
                var limitedStream = new LimitedStream(stream);
                var stringIndex = Index<string>.CreateFromWithSize(limitedStream, out size, !copy);
                position += size + 8;
                stream.Seek(position, System.IO.SeekOrigin.Begin);
                var indexLengthBytes = new byte[8];
                stream.Read(indexLengthBytes, 0, 8);
                var indexLength = BitConverter.ToInt64(indexLengthBytes, 0);
                var index = Context.ArrayFactory.CreateMemoryBackedArray<uint>(indexLength);
                index.CopyFrom(stream);
                return new AttributesIndex(defaultIndexMode, stringIndex, tagsIndex, index);
            }
        }

        #endregion
    }

    /// <summary>
    /// Attributes index mode flags.
    /// </summary>
    [Flags]
    public enum AttributesIndexMode
    {
        /// <summary>
        /// No specific mode, mode is about writing, used only when readonly.
        /// </summary>
        None = 0x0,
        /// <summary>
        /// Increase id's by one.
        /// </summary>
        IncreaseOne = 0x1,
        /// <summary>
        /// Keep a reverse collection index.
        /// </summary>
        ReverseCollectionIndex = 0x2,
        /// <summary>
        /// Keep a reverse string index.
        /// </summary>
        ReverseStringIndex = 0x4,
        /// <summary>
        /// Only keep a reverse index of keys.
        /// </summary>
        ReverseStringIndexKeysOnly = 0x8,
        /// <summary>
        /// All reverse indexes active.
        /// </summary>
        ReverseAll = 0x2 + 0x4
    }
}