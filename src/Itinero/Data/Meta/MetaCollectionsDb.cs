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
        private readonly long _length;

        /// <summary>
        /// Creates a new edge meta collection db.
        /// </summary>
        public MetaCollectionDb(long length)
        {
            _collections = new Dictionary<string, MetaCollection>();
        }

        /// <summary>
        /// Creates a new edge meta collection db.
        /// </summary>
        private MetaCollectionDb(long length, Dictionary<string, MetaCollection> collections)
        {
            _collections = collections;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<int> AddInt32(string name)
        {
            var metaCollection = new MetaCollection<int>(_length);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<uint> AddUInt32(string name)
        {
            var metaCollection = new MetaCollection<uint>(_length);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<long> AddInt64(string name)
        {
            var metaCollection = new MetaCollection<long>(_length);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<float> AddSingle(string name)
        {
            var metaCollection = new MetaCollection<float>(_length);
            _collections[name] = metaCollection;
            return metaCollection;
        }

        /// <summary>
        /// Adds a new meta collection.
        /// </summary>
        public MetaCollection<double> AddDouble(string name)
        {
            var metaCollection = new MetaCollection<double>(_length);
            _collections[name] = metaCollection;
            return metaCollection;
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

            // write length.
            var bytes = BitConverter.GetBytes(_length);
            stream.Write(bytes, 0, 8);
            size += 8;

            // write collections count.
            stream.WriteByte((byte)_collections.Count);

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

            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            var length = BitConverter.ToInt64(bytes, 0);

            var collections = new Dictionary<string, MetaCollection>();
            var count = stream.ReadByte();
            for (var i = 0; i < count; i++)
            {
                var name = stream.ReadWithSizeString();
                var collection = MetaCollection.Deserialize(stream, profile);

                collections[name] = collection;
            }
            return new MetaCollectionDb(length, collections);
        }
    }
}