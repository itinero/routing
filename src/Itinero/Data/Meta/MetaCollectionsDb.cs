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

using Reminiscence.Arrays;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.Data.Network.Edges
{
    /// <summary>
    /// An edge meta collections db.
    /// </summary>
    public sealed class MetaCollectionDb
    {
        private readonly Dictionary<string, MetaCollection> _collections;
        private const int BLOCK_SIZE = 1024;

        /// <summary>
        /// Creates a new edge meta collection db.
        /// </summary>
        public MetaCollectionDb()
        {
            _collections = new Dictionary<string, MetaCollection>();
        }

        /// <summary>
        /// Creates a new edge meta collection db.
        /// </summary>
        private MetaCollectionDb(Dictionary<string, MetaCollection> collections)
        {
            _collections = collections;
        }

        /// <summary>
        /// Gets the meta collection names.
        /// </summary>
        public IEnumerable<string> Names
        {
            get
            {
                return _collections.Keys;
            }
        }

        /// <summary>
        /// Removes all data.
        /// </summary>
        public void Clear()
        {
            _collections.Clear();
        }

        /// <summary>
        /// Gets the type of the collection with the given name.
        /// </summary>
        public Type GetType(string name)
        {
            MetaCollection collection;
            if (_collections.TryGetValue(name, out collection))
            {
                return collection.ElementType;
            }
            return null;
        }

        /// <summary>
        /// Switches the two items around.
        /// </summary>
        public void Switch(uint item1, uint item2)
        {
            foreach(var collection in _collections)
            {
                collection.Value.Switch(item1, item2);
            }
        }

        /// <summary>
        /// Copies whatever data is in 'from' to 'to'.
        /// </summary>
        public void Copy(uint to, uint from)
        {
            foreach(var collection in _collections)
            {
                collection.Value.Copy(to, from);
            }
        }

        /// <summary>
        /// Sets the item to the default empty value.
        /// </summary>
        public void SetEmpty(uint item)
        {
            foreach(var collection in _collections)
            {
                collection.Value.SetEmpty(item);
            }
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public MetaCollection Add(string name, Type type)
        {
            if (type == typeof(int))
            {
                return this.AddInt32(name);
            }
            if (type == typeof(uint))
            {
                return this.AddUInt32(name);
            }
            if (type == typeof(long))
            {
                return this.AddInt64(name);
            }
            if (type == typeof(ulong))
            {
                return this.AddUInt64(name);
            }
            if (type == typeof(float))
            {
                return this.AddSingle(name);
            }
            if (type == typeof(double))
            {
                return this.AddDouble(name);
            }
            if (type == typeof(short))
            {
                return this.AddInt16(name);
            }
            if (type == typeof(ushort))
            {
                return this.AddUInt16(name);
            }
            throw new Exception(string.Format(
                "Meta collection not supported for type {0}: MetaCollection can only handle integer types or float and double.",
                    type));
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<int> AddInt32(string name)
        {
            var metaCollection = new MetaCollection<int>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<uint> AddUInt32(string name)
        {
            var metaCollection = new MetaCollection<uint>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<long> AddInt64(string name)
        {
            var metaCollection = new MetaCollection<long>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<ulong> AddUInt64(string name)
        {
            var metaCollection = new MetaCollection<ulong>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<float> AddSingle(string name)
        {
            var metaCollection = new MetaCollection<float>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<double> AddDouble(string name)
        {
            var metaCollection = new MetaCollection<double>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<short> AddInt16(string name)
        {
            var metaCollection = new MetaCollection<short>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<ushort> AddUInt16(string name)
        {
            var metaCollection = new MetaCollection<ushort>(BLOCK_SIZE);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Tries to get an edge meta collection for the given key.
        /// </summary>
        public MetaCollection Get(string name)
        {
            MetaCollection metaCollection;
            if (!this.TryGet(name, out metaCollection))
            {
                return null;
            }
            return metaCollection;
        }

        /// <summary>
        /// Tries to get an edge meta collection for the given key.
        /// </summary>
        public bool TryGet(string name, out MetaCollection metaCollection)
        {
            return _collections.TryGetValue(name, out metaCollection);
        }

        /// <summary>
        /// Tries to get an edge meta collection for the given key.
        /// </summary>
        public MetaCollection<T> Get<T>(string name)
            where T : struct
        {
            MetaCollection<T> metaCollection;
            if (!this.TryGet<T>(name, out metaCollection))
            {
                return null;
            }
            return metaCollection;
        }

        /// <summary>
        /// Tries to get an edge meta collection for the given key.
        /// </summary>
        public bool TryGet<T>(string name, out MetaCollection<T> metaCollection)
            where T : struct
        {
            MetaCollection collection;
            if (_collections.TryGetValue(name, out collection))
            {
                metaCollection = (collection as MetaCollection<T>);
                return metaCollection != null;
            }
            metaCollection = null;
            return false;
        }

        /// <summary>
        /// Serializes this db.
        /// </summary>
        public long Serialize(Stream stream)
        {
            long size = 1;
            stream.WriteByte(1); // write version header.

            // write collections count.
            stream.WriteByte((byte)_collections.Count);
            size++;

            foreach (var collection in _collections)
            {
                size += stream.WriteWithSize(collection.Key);
                size += collection.Value.Serialize(stream);
            }

            return size;
        }

        /// <summary>
        /// Deserializes a db.
        /// </summary>
        public static MetaCollectionDb Deserialize(Stream stream, ArrayProfile profile = null)
        {
            var version = stream.ReadByte();
            if (version != 1)
            {
                if (version != 1)
                {
                    throw new Exception(string.Format("Cannot deserialize meta-data db: Invalid version #: {0}, upgrade Itinero.", version));
                }
            }

            var collections = new Dictionary<string, MetaCollection>();
            var count = stream.ReadByte();
            for (var i = 0; i < count; i++)
            {
                var name = stream.ReadWithSizeString();
                var collection = MetaCollection.Deserialize(stream, profile);

                collections[name] = collection;
            }
            return new MetaCollectionDb(collections);
        }
    }
}