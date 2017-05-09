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

using Itinero;
using Itinero.Attributes;
using Itinero.Profiles;
using Reminiscence.Arrays;
using Reminiscence.IO.Streams;
using System;
using System.IO;

namespace Itinero.Data.Shortcuts
{
    /// <summary>
    /// Represents a shortcuts db between a collection of predefined stops. 
    /// </summary>
    /// <remarks>
    /// A shortcut is a pre-calculated route added to the network as an edge. This shortcut can be reconstructed by using this DB.
    /// </remarks>
    public class ShortcutsDb
    {
        private readonly IAttributeCollection _dbMeta;
        private readonly AttributesIndex _stopsMeta;
        private readonly ArrayBase<uint> _stops;
        private readonly AttributesIndex _shortcutsMeta;
        private readonly ArrayBase<uint> _shortcuts;
        private readonly Profile _profile;

        /// <summary>
        /// Creates a new shortcuts db.
        /// </summary>
        public ShortcutsDb(Profile profile)
        {
            _dbMeta = new AttributeCollection();
            _profile = profile;

            _stopsMeta = new AttributesIndex(AttributesIndexMode.ReverseAll);
            _stops = Context.ArrayFactory.CreateMemoryBackedArray<uint>(100);
            _shortcutsMeta = new AttributesIndex(AttributesIndexMode.ReverseAll);
            _shortcuts = Context.ArrayFactory.CreateMemoryBackedArray<uint>(100);
        }

        /// <summary>
        /// Creates a new shortcuts db.
        /// </summary>
        private ShortcutsDb(Profile profile, IAttributeCollection dbMeta, AttributesIndex stopsMeta, ArrayBase<uint> stops, 
            AttributesIndex shortcutsMeta, ArrayBase<uint> shortcuts)
        {
            _dbMeta = dbMeta;
            _profile = profile;

            _stops = stops;
            _stopsMeta = stopsMeta;
            _shortcuts = shortcuts;
            _shortcutsMeta = shortcutsMeta;

            _shortcutsPointer = (uint)_shortcuts.Length;
            _stopsPointer = (uint)_stops.Length;
        }

        private uint _stopsPointer = 0;
        private uint _shortcutsPointer = 0;

        /// <summary>
        /// Gets the meta-data collection.
        /// </summary>
        public IAttributeCollection Meta
        {
            get
            {
                return _dbMeta;
            }
        }

        /// <summary>
        /// Gets the profile that built these shortcuts.
        /// </summary>
        public Profile Profile
        {
            get
            {
                return _profile;
            }
        }

        /// <summary>
        /// Adds a stop with associated meta-data.
        /// </summary>
        public void AddStop(uint vertex, IAttributeCollection meta)
        {
            var stopsMetaId = _stopsMeta.Add(meta);

            _stops.EnsureMinimumSize(_stopsPointer + 2);
            _stops[_stopsPointer + 0] = vertex;
            _stops[_stopsPointer + 1] = stopsMetaId;

            _stopsPointer += 2;
        }

        /// <summary>
        /// Gets a stop if there is one for the given vertex and the associated meta-data. Returns null if no stop is found.
        /// </summary>
        public IAttributeCollection GetStop(uint vertex)
        {
            for(uint p = 0; p < _stops.Length; p += 2)
            {
                if (_stops[p + 0] == vertex)
                {
                    return _stopsMeta.Get(_stops[p + 1]);
                }
            }
            return null;
        }

        /// <summary>
        /// Adds a new shortcut.
        /// </summary>
        public uint Add(uint[] vertices, IAttributeCollection meta)
        {
            var shortcutMetaId = _shortcutsMeta.Add(meta);
            var size = (uint)vertices.Length + 2;

            _shortcuts.EnsureMinimumSize(_shortcutsPointer + vertices.Length + 2);
            _shortcuts[_shortcutsPointer + 0] = size;
            _shortcuts[_shortcutsPointer + 1] = shortcutMetaId;
            for (uint i = 0; i < vertices.Length; i++)
            {
                _shortcuts[_shortcutsPointer + 2 + i] = vertices[i];
            }

            var id = _shortcutsPointer;
            _shortcutsPointer += size;
            return id;
        }

        /// <summary>
        /// Gets a shortcut.
        /// </summary>
        public uint[] Get(uint id, out IAttributeCollection meta)
        {
            var size = _shortcuts[id];

            meta = _shortcutsMeta.Get(_shortcuts[id + 1]);
            var vertices = new uint[size - 2];
            for(var pointer = id + 2; pointer < id + size; pointer++)
            {
                vertices[pointer - id - 2] = _shortcuts[pointer];
            }

            return vertices;
        }

        /// <summary>
        /// Gets a shortcut but it's source and target vertex.
        /// </summary>
        public uint[] Get(uint vertex1, uint vertex2, out IAttributeCollection meta)
        {
            uint id = 0;
            while(id < _shortcuts.Length)
            {
                var size = _shortcuts[id];
                if (_shortcuts[id + 2] == vertex1 &&
                    _shortcuts[id + size - 1] == vertex2)
                {
                    return this.Get(id, out meta);
                }
                id += size;
            }
            meta = null;
            return null;
        }

        /// <summary>
        /// Serializes this shortcuts db to the given stream and returns the # of bytes written.
        /// </summary>
        public long Serialize(Stream stream)
        {
            // trim data structures.
            _shortcuts.Resize(_shortcutsPointer);
            _stops.Resize(_stopsPointer);

            // write version #.
            long size = 1;
            stream.WriteByte(1);

            // write profile name.
            size += stream.WriteWithSize(_profile.FullName);

            // serialize the db-meta.
            size += _dbMeta.WriteWithSize(stream);

            // write the stops count and the shortcuts count.
            var bytes = BitConverter.GetBytes(_stopsPointer);
            stream.Write(bytes, 0, 4);
            size += 4;
            bytes = BitConverter.GetBytes(_shortcutsPointer);
            stream.Write(bytes, 0, 4);
            size += 4;

            // write stops meta and data.
            size += _stopsMeta.Serialize(stream);
            size += _stops.CopyTo(stream);

            // write shortcut meta and data.
            size += _shortcutsMeta.Serialize(stream);
            size += _shortcuts.CopyTo(stream);

            return size;
        }

        /// <summary>
        /// Deserializes a shortcuts db and leaves the stream position at the end of the shortcut db data.
        /// </summary>
        public static ShortcutsDb Deserialize(Stream stream)
        {
            var version = stream.ReadByte();
            if (version != 1)
            {
                throw new Exception(string.Format("Cannot deserialize shortcuts db: Invalid version #: {0}. Try upgrading Itinero or rebuild routing file with older version.", version));
            }

            // read profile name.
            var profileName = stream.ReadWithSizeString();
            Profile profile;
            if (!Profile.TryGet(profileName, out profile))
            {
                throw new Exception(string.Format("Cannot deserialize shortcuts db: Profile not found: {0}. Make sure to register all vehicle profile before deserializing.", profileName));
            }
            
            // read meta-data.
            var metaDb = stream.ReadWithSizeAttributesCollection();

            // read stops and shortcuts data sizes.
            var bytes = new byte[4];
            stream.Read(bytes, 0, 4);
            var stopsPointer = BitConverter.ToUInt32(bytes, 0);
            stream.Read(bytes, 0, 4);
            var shortcutsPointer = BitConverter.ToUInt32(bytes, 0);

            // read stops meta and data.
            var stopsMeta = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var stops = Context.ArrayFactory.CreateMemoryBackedArray<uint>(stopsPointer);
            stops.CopyFrom(stream);

            // read shortcuts meta and data.
            var shortcutsMeta = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var shortcuts = Context.ArrayFactory.CreateMemoryBackedArray<uint>(shortcutsPointer);
            shortcuts.CopyFrom(stream);

            return new ShortcutsDb(profile, metaDb, stopsMeta, stops, shortcutsMeta, shortcuts);
        }
    }
}