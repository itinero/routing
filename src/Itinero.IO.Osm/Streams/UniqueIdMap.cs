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

using Itinero.Algorithms.Collections;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A unique id map, only vertex per id.
    /// </summary>
    public class UniqueIdMap<T>
        where T : struct
    {
        private readonly HugeDictionary<long, Block> _blocks;
        private readonly int _blockSize;
        private static T _defaultValue;

        /// <summary>
        /// Creates a new id map.
        /// </summary>
        public UniqueIdMap(T defaultValue, int blockSize = 32)
        {
            _blocks = new HugeDictionary<long, Block>();
            _blockSize = blockSize;
            _defaultValue = defaultValue;
        }

        /// <summary>
        /// Sets a tile id.
        /// </summary>
        public void Set(long id, T vertex)
        {
            var blockIdx = id / _blockSize;
            var offset = id - (blockIdx * _blockSize);

            Block block;
            if (!_blocks.TryGetValue(blockIdx, out block))
            {
                block = new Block()
                {
                    Start = (uint)offset,
                    End = (uint)offset,
                    Data = new T[] { vertex }
                };
                _blocks[blockIdx] = block;
            }
            else
            {
                block.Set(offset, vertex);
                _blocks[blockIdx] = block;
            }
        }

        /// <summary>
        /// Gets or sets the tile id for the given id.
        /// </summary>
        public T this[long id]
        {
            get
            {
                return this.Get(id);
            }
            set
            {
                this.Set(id, value);
            }
        }

        /// <summary>
        /// Gets a tile id.
        /// </summary>
        public T Get(long id)
        {
            var blockIdx = id / _blockSize;
            var offset = id - (blockIdx * _blockSize);

            Block block;
            if (!_blocks.TryGetValue(blockIdx, out block))
            {
                return _defaultValue;
            }
            return block.Get(offset);
        }

        /// <summary>
        /// An enumerable with the non-default indices in this map.
        /// </summary>
        public IEnumerable<long> NonDefaultIndices
        {
            get
            {
                throw new System.NotImplementedException();
            }
        }

        private struct Block
        {
            public uint Start { get; set; }

            public uint End { get; set; }

            public T[] Data { get; set; }

            public T Get(long offset)
            {
                if (Start > offset)
                {
                    return _defaultValue;
                }
                else if (offset > End)
                {
                    return _defaultValue;
                }
                return this.Data[offset - Start];
            }

            public void Set(long offset, T value)
            {
                if (Start > offset)
                { // expand at the beginning.
                    var newData = new T[End - offset + 1];
                    Data.CopyTo(newData, (int)(Start - offset));
                    for (var i = 1; i < Start - offset; i++)
                    {
                        newData[i] = _defaultValue;
                    }
                    Data = newData;
                    Start = (uint)offset;
                    Data[0] = value;
                }
                else if (End < offset)
                { // expand at the end.
                    var newData = new T[offset - Start + 1];
                    Data.CopyTo(newData, 0);
                    for(var i = End + 1 - Start; i < newData.Length - 1; i++)
                    {
                        newData[i] = _defaultValue;
                    }
                    Data = newData;
                    End = (uint)offset;
                    Data[offset - Start] = value;
                }
                else
                {
                    Data[offset - Start] = value;
                }
            }
        }
    }
}