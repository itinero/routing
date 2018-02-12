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
using System.Collections.Generic;
using System;
using System.Collections;
using System.IO;
using Reminiscence.IO;
using Reminiscence.IO.Streams;

namespace Itinero.Data
{
    /// <summary>
    /// Abstract representation of a meta collection.
    /// </summary>
    public abstract class MetaCollection
    {
        private static Tuple<Type, byte>[] SupportedTypes = new Tuple<Type, byte>[]
        {
            new Tuple<Type, byte>(typeof(int), 0),
            new Tuple<Type, byte>(typeof(uint), 1),
            new Tuple<Type, byte>(typeof(long), 2),
            new Tuple<Type, byte>(typeof(ulong), 3),
            new Tuple<Type, byte>(typeof(float), 4),
            new Tuple<Type, byte>(typeof(double), 5),
            new Tuple<Type, byte>(typeof(short), 6),
            new Tuple<Type, byte>(typeof(ushort), 7)
        };

        /// <summary>
        /// Gets the data.
        /// </summary>
        public abstract object GetRaw(uint idx);

        /// <summary>
        /// Gets the type of the elements in this meta collection.
        /// </summary>
        public abstract Type ElementType
        {
            get;
        }

        /// <summary>
        /// Serializes the meta collection to the given stream.
        /// </summary>
        public abstract long Serialize(Stream stream);

        /// <summary>
        /// Copies an element from the other collection to this one. Collections must have the same type.
        /// </summary>
        public abstract void CopyFrom(MetaCollection other, uint idx, uint otherIdx);

        /// <summary>
        /// Switches the two vertices around.
        /// </summary>
        public abstract void Switch(uint vertex1, uint vertex2);

        /// <summary>
        /// Deserializes a meta-collection from the given stream.
        /// </summary>
        public static MetaCollection Deserialize(Stream stream, ArrayProfile profile)
        {
            var version = stream.ReadByte();
            if (version != 1 && version != 2)
            {
                throw new Exception(string.Format("Cannot deserialize meta-data collection: Invalid version #: {0}, upgrade Itinero.", version));
            }

            var byteHeader = stream.ReadByte();
            var type = MetaCollection.GetTypeForHeader(byteHeader);

            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            var length = BitConverter.ToInt64(bytes, 0);

            if (type == typeof(int))
            {
                return new MetaCollection<int>(MetaCollection.DeserializeArray<int>(
                    stream, profile, length, 4));
            }
            if (type == typeof(uint))
            {
                return new MetaCollection<uint>(MetaCollection.DeserializeArray<uint>(
                    stream, profile, length, 4));
            }
            if (type == typeof(short))
            {
                return new MetaCollection<short>(MetaCollection.DeserializeArray<short>(
                    stream, profile, length, 2));
            }
            if (type == typeof(ushort))
            {
                return new MetaCollection<ushort>(MetaCollection.DeserializeArray<ushort>(
                    stream, profile, length, 2));
            }
            if (type == typeof(long))
            {
                return new MetaCollection<long>(MetaCollection.DeserializeArray<long>(
                    stream, profile, length, 8));
            }
            if (type == typeof(ulong))
            {
                return new MetaCollection<ulong>(MetaCollection.DeserializeArray<ulong>(
                    stream, profile, length, 8));
            }
            if (type == typeof(float))
            {
                return new MetaCollection<float>(MetaCollection.DeserializeArray<float>(
                    stream, profile, length, 4));
            }
            if (type == typeof(double))
            {
                return new MetaCollection<double>(MetaCollection.DeserializeArray<double>(
                    stream, profile, length, 8));
            }
            throw new Exception(string.Format(
                "Meta collection not supported for type {0}: MetaCollection can only handle integer types or float and double.",
                    type));
        }

        /// <summary>
        /// Verifies if the current element type is supported.
        /// </summary>
        protected void VerifyType()
        {
            for (var i = 0; i < SupportedTypes.Length; i++)
            {
                if (SupportedTypes[i].Item1 == this.ElementType)
                {
                    return;
                }
            }
            throw new Exception(string.Format(
                "Meta collection not supported for type {0}: MetaCollection can only handle integer types or float and double.",
                    this.ElementType.ToInvariantString()));
        }

        /// <summary>
        /// Returns a byte header that describes the type.
        /// </summary>
        /// <returns></returns>
        protected byte GetTypeHeader()
        {
            for (var i = 0; i < SupportedTypes.Length; i++)
            {
                if (SupportedTypes[i].Item1 == this.ElementType)
                {
                    return SupportedTypes[i].Item2;
                }
            }
            throw new Exception(string.Format(
                "Meta collection not supported for type {0}: MetaCollection can only handle integer types or float and double.",
                    this.ElementType.ToInvariantString()));
        }

        /// <summary>
        /// Returns the type for the given header.
        /// </summary>
        /// <param name="header"></param>
        /// <returns></returns>
        protected static Type GetTypeForHeader(int header)
        {
            for (var i = 0; i < SupportedTypes.Length; i++)
            {
                if (SupportedTypes[i].Item2 == header)
                {
                    return SupportedTypes[i].Item1;
                }
            }
            throw new Exception(string.Format(
                "Meta collection cannot be deserialized:Type header {0} not recognized.",
                    header));
        }

        /// <summary>
        /// Deserializes an array.
        /// </summary>
        protected static ArrayBase<T> DeserializeArray<T>(Stream stream, ArrayProfile profile, long length, int elementSize)
        {
            ArrayBase<T> data;
            if (profile == null)
            { // just create arrays and read the data.
                data = new MemoryArray<T>(length);
                data.CopyFrom(stream);
            }
            else
            { // create accessors over the exact part of the stream that represents vertices/edges.
                var position = stream.Position;
                var byteCount = length * elementSize;
                var map = new MemoryMapStream(new CappedStream(stream, position, byteCount));
                var accessor = MemoryMap.GetCreateAccessorFuncFor<T>()(map, length);
                data = new Array<T>(accessor, profile);
                stream.Seek(position + byteCount, SeekOrigin.Begin);
            }
            return data;
        }
    }

    /// <summary>
    /// A meta-data collection containing meta-data linked to vertices or edges.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class MetaCollection<T> : MetaCollection, IEnumerable<T>
        where T : struct
    {
        private readonly ArrayBase<T> _data;
        private uint _length;
        private const int BLOCK_SIZE = 1024;

        /// <summary>
        /// Creates a new meta-data collection.
        /// </summary>
        public MetaCollection(long capacity)
        {
            this.VerifyType();

            _length = 0;
            _data = new MemoryArray<T>(capacity);
        }

        /// <summary>
        /// Creates a new meta-data collection.
        /// </summary>
        internal MetaCollection(ArrayBase<T> data)
        {
            _data = data;
            _length = (uint)data.Length;
        }

        /// <summary>
        /// Gets the type of the elements.
        /// </summary>
        public override Type ElementType => typeof(T);

        /// <summary>
        /// Copies elements from the other collection to this collection.
        /// </summary>
        public override void CopyFrom(MetaCollection other, uint idx, uint otherIdx)
        {
            if (other.ElementType != this.ElementType)
            {
                throw new Exception(string.Format(
                    "Cannot copy from a meta collection with type {0} to one with type {1}.",
                        other.ElementType.ToInvariantString(), this.ElementType.ToInvariantString()));
            }

            var otherTyped = (MetaCollection<T>)other;
            this[idx] = otherTyped[otherIdx];
        }

        /// <summary>
        /// Gets the raw data.
        /// </summary>
        public override object GetRaw(uint idx)
        {
            return this[idx];
        }

        /// <summary>
        /// Gets or sets the meta-data.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public T this[uint i]
        {
            get
            {
                return _data[i];
            }
            set
            {
                if (i >= _data.Length)
                {
                    var diff = i - _data.Length + 1;
                    var blocks = (diff / BLOCK_SIZE) + 1;

                    _data.Resize(_data.Length + (blocks * BLOCK_SIZE));
                }

                if (i >= _length)
                {
                    _length = i + 1;
                }

                _data[i] = value;
            }
        }

        /// <summary>
        /// Switches the two vertices around.
        /// </summary>
        public override void Switch(uint vertex1, uint vertex2)
        {
            if (vertex1 < this.Count &&
                vertex2 < this.Count)
            {
                var data = this[vertex1];
                this[vertex1] = this[vertex2];
                this[vertex2] = data;
            }
            else
            {
                if (vertex1 < this.Count)
                {
                    this[vertex1] = default(T);
                }

                if (vertex2 < this.Count)
                {
                    this[vertex2] = default(T);
                }
            }
        }

        /// <summary>
        /// Gets the # of elements.
        /// </summary>
        public long Count
        {
            get
            {
                return _length;
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<T> GetEnumerator()
        {
            for (var i = 0; i < _length; i++)
            {
                yield return _data[i];
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
        /// Trims the internal data structures to it's minimum possible size.
        /// </summary>
        public void Trim()
        {
            _data.Resize(_length);
        }
        
        /// <summary>
        /// Serializes this collection.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public override long Serialize(Stream stream)
        {
            this.Trim();

            // VERSION HISTORY:
            // VERSION1: Initial implementatioN.
            // VERSION2: Added support for short and ushort.
            long size = 1;
            stream.WriteByte(2);

            // write type header.
            stream.WriteByte(this.GetTypeHeader());
            size++;

            var bytes = BitConverter.GetBytes(_data.Length);
            stream.Write(bytes, 0, 8);
            size += 8;

            size += _data.CopyTo(stream);
            return size;
        }
    }
}