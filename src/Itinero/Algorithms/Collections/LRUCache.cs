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

using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Itinero.Algorithms.Collections
{
    /// <summary>
    /// Generic LRU cache implementation.
    /// </summary>
    /// <typeparam name="TKey">The key type.</typeparam>
    /// <typeparam name="TValue">The value type.</typeparam>
    public class LRUCache<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, CacheEntry> _data;

        /// <summary>
        /// Initializes this cache.
        /// </summary>
        /// <param name="capacity"></param>
        public LRUCache(int capacity)
        {
            _id = long.MinValue;
            _lastId = _id;
            _data = new ConcurrentDictionary<TKey, CacheEntry>();

            this.Capacity = capacity;
        }
        
        private long _id;
        private long _lastId;

        /// <summary>
        /// A delegate to use for when an item is pushed out of the cache.
        /// </summary>
        /// <param name="item"></param>
        public delegate void OnRemoveDelegate(TValue item);

        /// <summary>
        /// Called when an item is pushed out of the cache.
        /// </summary>
        public OnRemoveDelegate OnRemove;

        /// <summary>
        /// Capacity.
        /// </summary>
        public int Capacity { get; set; }

        /// <summary>
        /// Adds a new value for the given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void Add(TKey key, TValue value)
        {
            var entry = new CacheEntry
            {
                Id = _id,
                Value = value
            };
            _id++;
            _data[key] = entry;

            this.ResizeCache();
        }

        /// <summary>
        /// Returns the amount of entries in this cache.
        /// </summary>
        public int Count => _data.Count;

        /// <summary>
        /// Returns the value for this given key.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryGet(TKey key, out TValue value)
        {
            _id++;
            if (_data.TryGetValue(key, out var entry))
            {
                entry.Id = _id;
                value = entry.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Returns the value for this given key but does not effect the cache.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool TryPeek(TKey key, out TValue value)
        {
            if (_data.TryGetValue(key, out var entry))
            {
                value = entry.Value;
                return true;
            }

            value = default(TValue);
            return false;
        }

        /// <summary>
        /// Clears this cache.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
            _lastId = _id;
            _id = long.MinValue;
        }

        /// <summary>
        /// Removes the value for the given key.
        /// </summary>
        /// <param name="id"></param>
        public void Remove(TKey id)
        {
            _data.TryRemove(id, out _);
        }

        /// <summary>
        /// Resizes the cache.
        /// </summary>
        private void ResizeCache()
        {
            while (_data.Count > this.Capacity)
            {
                // oops: too much data.
                // remove the 'oldest' item.
                var minKey = default(TKey);
                var minId = long.MaxValue;
                foreach (var pair in _data)
                {
                    if (pair.Value.Id >= minId) continue;
                    minId = pair.Value.Id;
                    minKey = pair.Key;
                }

                // call the OnRemove delegate.
                OnRemove?.Invoke(_data[minKey].Value);

                _data.TryRemove(minKey, out _);

                // update the 'last_id'
                _lastId++;
            }
        }

        /// <summary>
        /// An entry in this cache.
        /// </summary>
        private struct CacheEntry
        {
            /// <summary>
            /// The id of the object.
            /// </summary>
            public long Id { get; set; }

            /// <summary>
            /// The object being cached.
            /// </summary>
            public TValue Value { get; set; }
        }
    }
}