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
using Reminiscence.Indexes;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;

namespace OsmSharp.Routing.Attributes
{
    /// <summary>
    /// An index for attribute collections.
    /// </summary>
    public class AttributesIndex
    {
        private readonly Index<string> _stringIndex;
        private readonly Index<int[]> _tagsIndex;
        private readonly bool _isReadonly = false;
        private const uint NULL_ATTRIBUTES = 0;
        private const uint EMPTY_ATTRIBUTES = 1;

        private readonly System.Collections.Generic.IDictionary<string, int> _stringReverseIndex; // Holds all strings and their id.
        private readonly System.Collections.Generic.IDictionary<int[], uint> _tagsReverseIndex; // Holds all tag collections and their reverse index.

        /// <summary>
        /// Creates a new empty index.
        /// </summary>
        public AttributesIndex()
            : this(false)
        {

        }

        /// <summary>
        /// Creates a new empty index.
        /// </summary>
        public AttributesIndex(bool readOnly)
        {
            _stringIndex = new Index<string>();
            _tagsIndex = new Index<int[]>();
            _isReadonly = readOnly;

            if (!readOnly)
            { // this index is not readonly and duplicates need to be checked.
                _stringReverseIndex = new System.Collections.Generic.Dictionary<string, int>();
                _tagsReverseIndex = new System.Collections.Generic.Dictionary<int[], uint>(
                    new DelegateEqualityComparer<int[]>(
                        (obj) =>
                        { // assumed the array is sorted.
                            var hash = obj.Length.GetHashCode();
                            for(int idx = 0; idx < obj.Length; idx++)
                            {
                                hash = hash ^ obj[idx].GetHashCode();
                            }
                            return hash;
                        },
                        (x, y) =>
                        {
                            if(x.Length == y.Length)
                            {
                                for (int idx = 0; idx < x.Length; idx++)
                                {
                                    if(x[idx] != y[idx])
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
        public AttributesIndex(MemoryMap file)
        {
            _stringIndex = new Index<string>(file);
            _tagsIndex = new Index<int[]>(file);
            _isReadonly = false;

            _tagsReverseIndex = new Reminiscence.Collections.Dictionary<int[], uint>(file);
            _stringReverseIndex = new Reminiscence.Collections.Dictionary<string, int>(file);
        }

        /// <summary>
        /// Creates a new index.
        /// </summary>
        internal AttributesIndex(Index<string> stringIndex, Index<int[]> tagsIndex)
        {
            _stringIndex = stringIndex;
            _tagsIndex = tagsIndex;
            _isReadonly = true;

            _stringReverseIndex = null;
            _tagsReverseIndex = null;
        }

        /// <summary>
        /// Returns true if this tags index is readonly.
        /// </summary>
        public bool IsReadonly
        {
            get { return _isReadonly; }
        }

        /// <summary>
        /// Returns the tags that belong to the given id.
        /// </summary>
        public TagsCollectionBase Get(uint tagsId)
        {
            if(tagsId == 0)
            {
                return null;
            }
            else if(tagsId == 1)
            {
                return new TagsCollection();
            }
            return new InternalTagsCollection(_stringIndex, _tagsIndex.Get(tagsId - 2));
        }

        /// <summary>
        /// Adds new tags.
        /// </summary>
        public uint Add(TagsCollectionBase tags)
        {
            if(tags == null)
            {
                return NULL_ATTRIBUTES;
            }
            else if(tags.Count == 0)
            {
                return EMPTY_ATTRIBUTES;
            }

            if (_stringReverseIndex == null)
            { // this index is readonly.
                throw new System.InvalidOperationException("This tags index is readonly. Check IsReadonly.");
            }
            else
            { // add new collection.
                var sortedSet = new OsmSharp.Collections.SortedSet<long>();
                foreach(var tag in tags)
                {
                    int keyId;
                    if(!_stringReverseIndex.TryGetValue(tag.Key, out keyId))
                    { // the key doesn't exist yet.
                        keyId = (int)_stringIndex.Add(tag.Key);
                        _stringReverseIndex.Add(tag.Key, keyId);
                    }
                    int valueId;
                    if (!_stringReverseIndex.TryGetValue(tag.Value, out valueId))
                    { // the key doesn't exist yet.
                        valueId = (int)_stringIndex.Add(tag.Value);
                        _stringReverseIndex.Add(tag.Value, valueId);
                    }
                    sortedSet.Add((long)keyId + (long)int.MaxValue * (long)valueId);
                }

                // sort keys.
                var sorted = new int[sortedSet.Count * 2];
                var idx = 0;
                foreach (var pair in sortedSet)
                {
                    sorted[idx] = (int)(pair % int.MaxValue);
                    idx++;
                    sorted[idx] = (int)(pair / int.MaxValue);
                    idx++;
                }

                // check duplicates.
                uint tagsId;
                if(_tagsReverseIndex.TryGetValue(sorted, out tagsId))
                { // collection already exists.
                    return tagsId + 2;
                }

                tagsId = (uint)_tagsIndex.Add(sorted);
                _tagsReverseIndex.Add(sorted, tagsId);
                return tagsId + 2;
            }
        }

        /// <summary>
        /// An implementation of a tags collection.
        /// </summary>
        private class InternalTagsCollection : TagsCollectionBase
        {
            private Index<string> _stringIndex; // Holds the string index.
            private int[] _tags; // Holds the tags.

            /// <summary>
            /// Creates a new internal tags collection.
            /// </summary>
            public InternalTagsCollection(Index<string> stringIndex, int[] tags)
            {
                _stringIndex = stringIndex;
                _tags = tags;
            }

            /// <summary>
            /// Returns the number of tags in this collection.
            /// </summary>
            public override int Count
            {
                get { return _tags.Length / 2; }
            }

            /// <summary>
            /// Returns true if this collection is readonly.
            /// </summary>
            public override bool IsReadonly
            {
                get { return true; }
            }

            /// <summary>
            /// Adds a key-value pair to this tags collection.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public override void Add(string key, string value)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Adds a tag.
            /// </summary>
            /// <param name="tag"></param>
            public override void Add(Tag tag)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Adds a tag or replace the existing value if any.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="value"></param>
            public override void AddOrReplace(string key, string value)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Adds a tag or replace the existing value if any.
            /// </summary>
            /// <param name="tag"></param>
            public override void AddOrReplace(Tag tag)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Returns true if the given tag exists.
            /// </summary>
            /// <param name="key"></param>
            /// <returns></returns>
            public override bool ContainsKey(string key)
            {
                for(int idx = 0; idx < _tags.Length; idx = idx + 2)
                {
                    if(key == _stringIndex.Get(_tags[idx]))
                    { // key found!
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns true if the given tag exists.
            /// </summary>
            public override bool TryGetValue(string key, out string value)
            {
                for (int idx = 0; idx < _tags.Length; idx = idx + 2)
                {
                    if (key == _stringIndex.Get(_tags[idx]))
                    { // key found!
                        value = _stringIndex.Get(_tags[idx + 1]);
                        return true;
                    }
                }
                value = null;
                return false;
            }

            /// <summary>
            /// Returns true if the given tag exists with the given value.
            /// </summary>
            public override bool ContainsKeyValue(string key, string value)
            {
                for (int idx = 0; idx < _tags.Length; idx = idx + 2)
                {
                    if (key == _stringIndex.Get(_tags[idx]) &&
                        value == _stringIndex.Get(_tags[idx + 1]))
                    { // key found!
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Removes all tags with the given key.
            /// </summary>
            public override bool RemoveKey(string key)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Removes the given tag.
            /// </summary>
            public override bool RemoveKeyValue(string key, string value)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Clears all tags.
            /// </summary>
            public override void Clear()
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Removes all tags that match the given criteria.
            /// </summary>
            public override void RemoveAll(System.Predicate<Tag> predicate)
            {
                throw new InvalidOperationException("This tags collection is readonly. Check IsReadonly.");
            }

            /// <summary>
            /// Returns the enumerator for this enumerable.
            /// </summary>
            public override System.Collections.Generic.IEnumerator<Tag> GetEnumerator()
            {
                return new InternalTagsEnumerator(_stringIndex, _tags);
            }
        }

        /// <summary>
        /// An internal implementation of a tags enumerator.
        /// </summary>
        private class InternalTagsEnumerator : System.Collections.Generic.IEnumerator<Tag>
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

            public Tag Current
            {
                get
                {
                    return new Tag()
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
                    return new Tag()
                    {
                        Key = _stringIndex.Get(_tags[_idx]),
                        Value = _stringIndex.Get(_tags[_idx + 1])
                    };
                }
            }

            /// <summary>
            /// Move to the next tag.
            /// </summary>
            /// <returns></returns>
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
            var size = _tagsIndex.CopyToWithSize(stream);
            return _stringIndex.CopyToWithSize(stream) + size;
        }

        /// <summary>
        /// Deserializes a tags index from the given stream.
        /// </summary>
        public static AttributesIndex Deserialize(System.IO.Stream stream, bool copy = false)
        {
            long size;
            var tagsIndex = Index<int[]>.CreateFromWithSize(stream, out size, !copy);
            var totalSize = size + 8;
            stream.Seek(totalSize, System.IO.SeekOrigin.Begin);
            var limitedStream = new LimitedStream(stream);
            var stringIndex = Index<string>.CreateFromWithSize(limitedStream, out size, !copy);
            totalSize += size + 8;
            stream.Seek(totalSize, System.IO.SeekOrigin.Begin);

            return new AttributesIndex(stringIndex, tagsIndex);
        }

        #endregion
    }
}