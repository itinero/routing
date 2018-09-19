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

using Itinero.LocalGeo.Elevation;
using Reminiscence.Arrays;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.IO;

namespace Itinero.Graphs.Geometric.Shapes
{
    /// <summary>
    /// An index for edge-shapes.
    /// </summary>
    public class ShapesArray : ArrayBase<ShapeBase>
    {
        private const int MAX_COLLECTION_SIZE = ushort.MaxValue; // The maximum size of one collection is.
        private const short NO_ELEVATION = short.MaxValue;
        private const int ESTIMATED_SIZE = 5; // The average estimated size.
        private readonly ArrayBase<ulong> _index; // Holds the coordinates index position and count.
        private readonly ArrayBase<float> _coordinates; // holds the coordinates.
        private ArrayBase<short> _elevation; // holds the elevation.
        private readonly Func<long, ArrayBase<short>> _createElevation;

        /// <summary>
        /// A new shape index.
        /// </summary>
        public ShapesArray(long size)
        {
            _index = Context.ArrayFactory.CreateMemoryBackedArray<ulong>(size);
            _coordinates = Context.ArrayFactory.CreateMemoryBackedArray<float>(size * 2 * ESTIMATED_SIZE);
            _createElevation = (s) =>
            {
                var elevation = Context.ArrayFactory.CreateMemoryBackedArray<short>(s);
                for (var i = 0; i < elevation.Length; i++)
                {
                    elevation[i] = NO_ELEVATION;
                }
                return elevation;
            };

            for (long i = 0; i < _index.Length; i++)
            {
                _index[i] = 0;
            }

            for (long i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = float.MinValue;
            }
        }

        /// <summary>
        /// A new shape index.
        /// </summary>
        public ShapesArray(MemoryMap map, long size)
        {
            _index = new Array<ulong>(map, size);
            _coordinates = new Array<float>(map, size * 2 * ESTIMATED_SIZE);
            _createElevation = (s) =>
            {
                var elevation = new Array<short>(map, s);
                for (var i = 0; i < elevation.Length; i++)
                {
                    elevation[i] = NO_ELEVATION;
                }
                return elevation;
            };

            for (long i = 0; i < _index.Length; i++)
            {
                _index[i] = 0;
            }

            for (long i = 0; i < _coordinates.Length; i++)
            {
                _coordinates[i] = float.MinValue;
            }
        }

        /// <summary>
        /// A new shape index.
        /// </summary>
        private ShapesArray(ArrayBase<ulong> index, ArrayBase<float> coordinates, ArrayBase<short> elevation)
        {
            _index = index;
            _coordinates = coordinates;
            _elevation = elevation;

            _createElevation = (s) =>
            {
                var e = Context.ArrayFactory.CreateMemoryBackedArray<short>(s);
                for (var i = 0; i < e.Length; i++)
                {
                    e[i] = NO_ELEVATION;
                }
                return e;
            };

            _nextPointer = _coordinates.Length;
        }

        private long _nextPointer = 0; // Holds the next idx.

        /// <summary>
        /// Quickly switches two elements.
        /// </summary>
        public void Switch(long id1, long id2)
        {
            var data = _index[id1];
            _index[id1] = _index[id2];
            _index[id2] = data;
        }

        /// <summary>
        /// Returns true if this array can be resized.
        /// </summary>
        public override bool CanResize => _index.CanResize && _coordinates.CanResize;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public sealed override long Length => _index.Length;

        /// <summary>
        /// Returns the size in bytes as if serialized.
        /// </summary>
        public long SizeInBytes
        {
            get
            {
                var sizeInBytes = 16 + _index.Length * 8;
                if (_elevation != null)
                {
                    sizeInBytes++;
                }
                for (var i = 0; i < _index.Length; i++)
                {
                    ShapesArray.ExtractPointerAndSize(_index[i], out _, out var size);
                    if (size > 0)
                    {
                        sizeInBytes += size * 4;
                        if (_elevation != null)
                        {
                            sizeInBytes += size; // elevation if any.
                        }
                    }
                }
                return sizeInBytes;
            }
        }

        /// <summary>
        /// Gets or sets the shape at the given id.
        /// </summary>
        public sealed override ShapeBase this[long id]
        {
            get
            {
                if (this.TryGet(id, out var shape))
                {
                    return shape;
                }
                return null;
            }
            set
            {
                this.Set(id, value);
            }
        }

        /// <summary>
        /// Adds elevation.
        /// </summary>
        public void AddElevation(ElevationHandler.GetElevationDelegate getElevationFunc)
        {
            if (_elevation == null)
            {
                _elevation = _createElevation(_coordinates.Length / 2);
            }

            for (var i = 0; i < _index.Length; i++)
            {
                ShapesArray.ExtractPointerAndSize(_index[i], out var pointer, out var size);
                if (size > 0)
                {
                    for (var p = 0; p < size; p++)
                    {
                        var lat = _coordinates[pointer + (p * 2)];
                        var lon = _coordinates[pointer + (p * 2) + 1];

                        var e = getElevationFunc(lat, lon);
                        if (e.HasValue)
                        {
                            _elevation[pointer / 2 + p] = e.Value;
                        }
                        else
                        {
                            _elevation[pointer / 2 + p] = NO_ELEVATION;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Resizes this array.
        /// </summary>
        public sealed override void Resize(long size)
        {
            if (_index.Length == size) return;
            if (!this.CanResize) { throw new InvalidOperationException("Cannot resize a fixed-sized array."); }

            _index.Resize(size);
        }

        /// <summary>
        /// Sets the coordinates for the given id.
        /// </summary>
        private void Set(long id, ShapeBase shape)
        {
            if (id < 0 || id >= _index.Length) { throw new IndexOutOfRangeException(); }
            
            if(shape == null)
            { // reset.
                _index[id] = 0;
                return;
            }

            GetPointerAndSize(id, out var pointer, out var size);
            if (pointer < 0 || shape.Count < size)
            { // add the coordinates at the end.
                SetPointerAndSize(id, _nextPointer, shape.Count);

                // increase the size of the coordinates if needed.
                _coordinates.EnsureMinimumSize(_nextPointer + (2 * shape.Count));
                _elevation?.EnsureMinimumSize((_nextPointer / 2) + shape.Count);

                for (var i = 0; i < shape.Count; i++)
                {
                    var coordinate = shape[i];
                    _coordinates[_nextPointer + (i * 2)] = coordinate.Latitude;
                    _coordinates[_nextPointer + (i * 2) + 1] = coordinate.Longitude;

                    if (coordinate.Elevation.HasValue)
                    {
                        if (_elevation == null)
                        {
                            _elevation = _createElevation(_coordinates.Length / 2);
                        }
                        _elevation[_nextPointer / 2 + i] = coordinate.Elevation.Value;
                    }
                }
                _nextPointer += (2 * shape.Count);
            }
            else
            { // update coordinates in-place.
                SetPointerAndSize(id, pointer, shape.Count);
                for (var i = 0; i < shape.Count; i++)
                {
                    var coordinate = shape[i];
                    _coordinates[pointer + (i * 2)] = coordinate.Latitude;
                    _coordinates[pointer + (i * 2) + 1] = coordinate.Longitude;

                    if (coordinate.Elevation.HasValue)
                    {
                        if (_elevation == null)
                        {
                            _elevation = _createElevation(_coordinates.Length / 2);
                        }
                        _elevation[_nextPointer / 2 + i] = coordinate.Elevation.Value;
                    }
                }
            }
        }

        /// <summary>
        /// Returns the coordinate collection at the given id.
        /// </summary>
        private bool TryGet(long id, out ShapeBase shape)
        {
            GetPointerAndSize(id, out var index, out var size);
            if (index >= 0)
            {
                shape = new Shape(_coordinates, _elevation, index, size);
                return true;
            }
            shape = null;
            return false;
        }

        /// <summary>
        /// Gets the index and the size (if any).
        /// </summary>
        private void GetPointerAndSize(long id, out long pointer, out int size)
        {
            if (id >= _index.Length)
            {
                pointer = -1;
                size = -1;
                return;
            }
            ShapesArray.ExtractPointerAndSize(_index[id], out pointer, out size);
        }

        /// <summary>
        /// Resets the index and size.
        /// </summary>
        private void ResetIndexAndSize(long id)
        {
            _index[id] = 0;
        }

        /// <summary>
        /// Sets the index and size.
        /// </summary>
        private void SetPointerAndSize(long id, long pointer, int size)
        {
            _index[id] = ShapesArray.BuildPointerAndSize(pointer, size);
        }

        /// <summary>
        /// Builds an unsinged long containing pointer and size.
        /// </summary>
        private static ulong BuildPointerAndSize(long pointer, int size)
        {
            if (size > ushort.MaxValue) { throw new ArgumentOutOfRangeException(); }

            return (ulong)((pointer / 2) * MAX_COLLECTION_SIZE) + (ulong)size;
        }

        /// <summary>
        /// Extracts pointer and size from an unsigned long.
        /// </summary>
        private static void ExtractPointerAndSize(ulong value, out long pointer, out int size)
        {
            if (value == 0)
            {
                pointer = -1;
                size = -1;
                return;
            }
            pointer = ((long)(value / (ulong)MAX_COLLECTION_SIZE)) * 2;
            size = (int)(value % (ulong)MAX_COLLECTION_SIZE);
        }

        /// <summary>
        /// Trims the internal data structure(s).
        /// </summary>
        public void Trim()
        {
            _coordinates.Resize(_nextPointer);
            _elevation.Resize(_nextPointer / 2);
        }

        /// <summary>
        /// Copies to the given stream.
        /// </summary>
        public override long CopyTo(Stream stream)
        {
            var initialPosition = stream.Position;
            stream.Write(BitConverter.GetBytes(_index.Length), 0, 8);
            stream.Seek(stream.Position + 8, System.IO.SeekOrigin.Begin); // leave room for the coordinates-size.

            // rewrite the coordinates array in the process removing all empty spaces after removing items.
            var position = stream.Position;
            var coordinatesPosition = position + (_index.Length * 8);
            long newPointer = 0;
            long elevationNewPointer = 0;
            using (var indexStream = new BinaryWriter(new LimitedStream(stream, position)))
            {
                using(var coordinatesStream = new BinaryWriter(new LimitedStream(stream, coordinatesPosition)))
                {
                    for(var i = 0; i < _index.Length; i++)
                    {
                        coordinatesStream.SeekBegin(newPointer * 4);
                        ShapesArray.ExtractPointerAndSize(_index[i], out var pointer, out var size);
                        if (size > 0)
                        {
                            for (var p = 0; p < size; p++)
                            {
                                coordinatesStream.Write(_coordinates[pointer + (p * 2)]);
                                coordinatesStream.Write(_coordinates[pointer + (p * 2) + 1]);
                            }

                            indexStream.SeekBegin(i * 8);
                            indexStream.Write(ShapesArray.BuildPointerAndSize(newPointer, size));
                            newPointer += size * 2;
                        }
                        else
                        {
                            indexStream.SeekBegin(i * 8);
                            indexStream.Write((ulong)0);
                        }
                    }
                }

                if (_elevation != null)
                {
                    var elevationPosition = coordinatesPosition + newPointer * 4;
                    using (var elevationStream = new BinaryWriter(new LimitedStream(stream, elevationPosition)))
                    {
                        for (var i = 0; i < _index.Length; i++)
                        {
                            elevationStream.SeekBegin(elevationNewPointer * 2);
                            ShapesArray.ExtractPointerAndSize(_index[i], out var pointer, out var size);
                            if (size > 0)
                            {
                                for (var p = 0; p < size; p++)
                                {
                                    elevationStream.Write(_elevation[pointer / 2 + p]);
                                }

                                // ALREADY WRITTEN
                                //indexStream.SeekBegin(i * 8);
                                //indexStream.Write(ShapesArray.BuildPointerAndSize(newPointer, size));
                                elevationNewPointer += size;
                            }
                            else
                            {
                                // ALREADY WRITTEN
                                //indexStream.SeekBegin(i * 8);
                                //indexStream.Write((ulong)0);
                            }
                        }
                    }
                }
            }

            // write coordinates size.
            stream.Seek(initialPosition + 8, System.IO.SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(newPointer), 0, 8);

            // seek until after.
            var sizeInBytes = 16 + (_index.Length * 8) + (newPointer * 4) + (elevationNewPointer * 2);
            stream.Seek(initialPosition + sizeInBytes, System.IO.SeekOrigin.Begin);
            return sizeInBytes;
        }
        /// <summary>
        /// Copies from the given stream.
        /// </summary>
        public void CopyFrom(Stream stream, bool hasElevation = false)
        {
            var array = CreateFrom(stream, false, hasElevation);

            for (var i = 0; i < array.Length; i++)
            {
                this[i] = array[i];
            }
        }

        /// <summary>
        /// Copies from the given stream.
        /// </summary>
        public override void CopyFrom(Stream stream)
        {
            this.CopyFrom(stream, false);
        }

        /// <summary>
        /// Deserializes an shapes index from the given stream.
        /// </summary>
        public static ShapesArray CreateFrom(Stream stream, bool copy, bool hasElevation = false)
        {
            return ShapesArray.CreateFrom(stream, copy, out var _, hasElevation);
        }

        /// <summary>
        /// Deserializes an shapes index from the given stream.
        /// </summary>
        public static ShapesArray CreateFrom(Stream stream, bool copy, out long size, bool hasElevation = false)
        {
            var initialPosition = stream.Position;

            size = 0;
            var longBytes = new byte[8];
            stream.Read(longBytes, 0, 8);
            size += 8;
            var indexLength = BitConverter.ToInt64(longBytes, 0);
            stream.Read(longBytes, 0, 8);
            size += 8;
            var coordinatesLength = BitConverter.ToInt64(longBytes, 0);

            ArrayBase<ulong> index;
            ArrayBase<float> coordinates;
            ArrayBase<short> elevation = null;
            if (copy)
            { // just create arrays and read the data.
                index = Context.ArrayFactory.CreateMemoryBackedArray<ulong>(indexLength);
                index.CopyFrom(stream);
                size += indexLength * 8;
                coordinates = Context.ArrayFactory.CreateMemoryBackedArray<float>(coordinatesLength);
                size += coordinatesLength * 4;
                coordinates.CopyFrom(stream);
                if (hasElevation)
                {
                    elevation = Context.ArrayFactory.CreateMemoryBackedArray<short>(coordinatesLength / 2);
                    size += coordinatesLength;
                    elevation.CopyFrom(stream);
                }
            }
            else
            { // create accessors over the exact part of the stream that represents vertices/edges.
                var position = stream.Position;
                var map1 = new MemoryMapStream(new CappedStream(stream, position, indexLength * 8));
                index = new Array<ulong>(map1.CreateUInt64(indexLength));
                size += indexLength * 8;
                var map2 = new MemoryMapStream(new CappedStream(stream, position + indexLength * 8, 
                    coordinatesLength * 4));
                coordinates = new Array<float>(map2.CreateSingle(coordinatesLength));
                size += coordinatesLength * 4;
                if (hasElevation)
                {
                    var map3 = new MemoryMapStream(new CappedStream(stream, position + indexLength * 8 + coordinatesLength * 4,
                        coordinatesLength));
                    elevation = new Array<short>(map3.CreateInt16(coordinatesLength / 2));
                    size += coordinatesLength;
                }
            }

            // make stream is positioned correctly.
            stream.Seek(initialPosition + size, System.IO.SeekOrigin.Begin);

            return new ShapesArray(index, coordinates, elevation);
        }

        /// <summary>
        /// Disposes of all native resources associated with this object.
        /// </summary>
        public sealed override void Dispose()
        {
            _index.Dispose();
            _coordinates.Dispose();
            _elevation.Dispose();
        }
    }
}