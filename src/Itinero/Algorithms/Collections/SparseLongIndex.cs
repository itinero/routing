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