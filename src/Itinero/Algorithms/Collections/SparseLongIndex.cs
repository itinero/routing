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

using System;
using System.Collections;
using System.Collections.Generic;

namespace Itinero.Algorithms.Collections
{

    /// <summary>
    /// An efficient index for a large number of bitflags that can handle both negative and positive ids.
    /// </summary>
    public class SparseLongIndex : IEnumerable<long>
    {
        private readonly long _size = (long)(1024 * 1024) * (long)(1024 * 32); // Holds the total size.
        private readonly int _blockSize = 32; // Holds the block size.

        /// <summary>
        /// Creates a new longindex.
        /// </summary>
        public SparseLongIndex(int blockSize = 32)
        {
            _blockSize = blockSize;
        }

        private long _count = 0; // Holds the number of flags.
        private SparseBitArray32 _negativeFlags = null; // Holds the negative flags array
        private SparseBitArray32 _positiveFlags = null; // Holds the positive flags array.


        /// <summary>
        /// Sets an id.
        /// </summary>
        public void Add(long number)
        {
            if (number >= 0)
            {
                this.PositiveAdd(number);
            }
            else
            {
                this.NegativeAdd(-number);
            }
        }

        /// <summary>
        /// Removes an id.
        /// </summary>
        public void Remove(long number)
        {
            if (number >= 0)
            {
                this.PositiveRemove(number);
            }
            else
            {
                this.NegativeAdd(-number);
            }
        }

        /// <summary>
        /// Returns true if the id is there.
        /// </summary>
        public bool Contains(long number)
        {
            if (number >= 0)
            {
                return this.PositiveContains(number);
            }
            else
            {
                return this.NegativeContains(-number);
            }
        }

        #region Positive

        /// <summary>
        /// Adds an id.
        /// </summary>
        private void PositiveAdd(long number)
        {
            if (_positiveFlags == null)
            {
                _positiveFlags = new SparseBitArray32(_size, _blockSize);
            }

            if (!_positiveFlags[number])
            { // there is a new positive flag.
                _count++;
            }
            _positiveFlags[number] = true;
        }

        /// <summary>
        /// Removes an id.
        /// </summary>
        private void PositiveRemove(long number)
        {
            if (_positiveFlags == null)
            {
                _positiveFlags = new SparseBitArray32(_size, _blockSize);
            }

            if (_positiveFlags[number])
            { // there is one less positive flag.
                _count--;
            }
            _positiveFlags[number] = false;
        }

        /// <summary>
        /// Returns true if the id is there.
        /// </summary>
        private bool PositiveContains(long number)
        {
            if (_positiveFlags == null)
            {
                return false;
            }

            return _positiveFlags[number];
        }

        #endregion

        #region Negative

        /// <summary>
        /// Adds an id.
        /// </summary>
        private void NegativeAdd(long number)
        {
            if (_negativeFlags == null)
            {
                _negativeFlags = new SparseBitArray32(_size, _blockSize);
            }

            if (!_negativeFlags[number])
            { // there is one more negative flag.
                _count++;
            }
            _negativeFlags[number] = true;
        }

        /// <summary>
        /// Removes an id.
        /// </summary>
        private void NegativeRemove(long number)
        {
            if (_negativeFlags == null)
            {
                _negativeFlags = new SparseBitArray32(_size, _blockSize);
            }

            if (_negativeFlags[number])
            { // there is one less negative flag.
                _count--;
            }
            _negativeFlags[number] = false;
        }

        /// <summary>
        /// Returns true if the id is there.
        /// </summary>
        private bool NegativeContains(long number)
        {
            if (_negativeFlags == null)
            {
                return false;
            }

            return _negativeFlags[number];
        }

        #endregion

        /// <summary>
        /// Returns the number of positive flags.
        /// </summary>
        public long Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Clears this index.
        /// </summary>
        public void Clear()
        {
            _negativeFlags = null;
            _positiveFlags = null;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<long> GetEnumerator()
        {
            if (_positiveFlags != null && _negativeFlags == null)
            {
                return _positiveFlags.GetEnumerator();
            }
            if (_positiveFlags == null && _negativeFlags != null)
            {
                return _negativeFlags.GetEnumerator();
            }
            return System.Linq.Enumerable.Concat<long>(_negativeFlags, _positiveFlags).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}