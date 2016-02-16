// Itinero - OpenStreetMap (OSM) SDK
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

using System;

namespace Itinero.Algorithms.Collections
{
    /// <summary>
    /// Represents a sparse bitarray.
    /// </summary>
    public class SparseBitArray32
    {
        private readonly int _blockSize; // Holds the blocksize, or the size of the 'sub arrays'.
        private readonly long _length; // Holds the length of this array.
        private readonly BitArray32[] _data; // Holds the actual data blocks.

        /// <summary>
        /// Creates a new sparse bitarray.
        /// </summary>
        public SparseBitArray32(long size, int blockSize)
        {
            if (size % 32 != 0) { throw new ArgumentOutOfRangeException("Size has to be divisible by 32."); }
            if (size % blockSize != 0) { throw new ArgumentOutOfRangeException("Size has to be divisible by blocksize."); }

            _length = size;
            _blockSize = blockSize;
            _data = new BitArray32[_length / _blockSize];
        }

        /// <summary>
        /// Gets or sets the value at the given index.
        /// </summary>
        public bool this[long idx]
        {
            get
            {
                int blockId = (int)(idx / _blockSize);
                var block = _data[blockId];
                if (block != null)
                { // the block actually exists.
                    int blockIdx = (int)(idx % _blockSize);
                    return _data[blockId][blockIdx];
                }
                return false;
            }
            set
            {
                int blockId = (int)(idx / _blockSize);
                var block = _data[blockId];
                if (block == null)
                { // block is not there.
                    if (value)
                    { // only add new block if true.
                        block = new BitArray32(_blockSize);
                        int blockIdx = (int)(idx % _blockSize);
                        block[blockIdx] = true;
                        _data[blockId] = block;
                    }
                }
                else
                { // set value at block.
                    int blockIdx = (int)(idx % _blockSize);
                    block[blockIdx] = value;
                }
            }
        }

        /// <summary>
        /// Returns the length of this array.
        /// </summary>
        public long Length
        {
            get
            {
                return _length;
            }
        }
    }
}