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

using Reminiscence.Arrays;
using System;

namespace Itinero.Data.Network.Restrictions
{
    /// <summary>
    /// Represents a restriction database.
    /// </summary>
    /// <remarks>A restriction is a sequence of vertices that is forbidden. A complex restriction is a restriction with > 1 vertex.</remarks>
    public class RestrictionsDb
    {
        private readonly ArrayBase<uint> _hashes; // holds pointers to the actual restriction data for vertices that hash to the locations in the array.
        private readonly ArrayBase<uint> _restrictions; // holds the actual restrictions.
        private const int BLOCKSIZE = 1000;
        private const int DEFAULT_HASHCOUNT = 1024 * 1024;
        private const uint NO_DATA = uint.MaxValue;
        private const uint POS_SIZE = 0;
        private const uint POS_NEXT_POINTER_FIRST = 1;
        private const uint POS_NEXT_POINTER_LAST = 2;
        private const uint POS_FIRST_VERTEX = 3;

        private bool _hasComplexRestrictions; // flag to indicate presence of complex restrictions.

        /// <summary>
        /// Creates a new restrictions db.
        /// </summary>
        public RestrictionsDb(int hashes = DEFAULT_HASHCOUNT)
        {
            _hasComplexRestrictions = false;
            _hashes = new MemoryArray<uint>(hashes * 2);
            for (var i = 0; i < _hashes.Length; i++)
            {
                _hashes[i] = NO_DATA;
            }
            _restrictions = new MemoryArray<uint>(BLOCKSIZE);
        }

        private uint _nextPointer = 0;

        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public void Add(params uint[] restriction)
        {
            if (restriction == null) { throw new ArgumentNullException("restriction"); }
            if (restriction.Length == 0) { throw new ArgumentException("Restriction should contain one or more vertices.", "restriction"); }

            var firstVertex = restriction[0];
            var firstHash = CalculateHash(firstVertex, (int)(_hashes.Length / 2));
            var lastVertex = restriction[restriction.Length - 1];
            var lastHash = CalculateHash(lastVertex, (int)(_hashes.Length / 2));

            uint size = (uint)(POS_FIRST_VERTEX + restriction.Length);
            while (_nextPointer + size >= _restrictions.Length)
            {
                _restrictions.Resize(_restrictions.Length + BLOCKSIZE);
            }

            var firstPointer = _hashes[firstHash];
            if (firstPointer == NO_DATA)
            { // start new.
                _hashes[firstHash] = _nextPointer;
            }
            else
            { // add at the end and change last pointer.
                while (_restrictions[firstPointer + POS_NEXT_POINTER_FIRST] != NO_DATA)
                {
                    firstPointer = _restrictions[firstPointer + POS_NEXT_POINTER_FIRST];
                }
                _restrictions[firstPointer + POS_NEXT_POINTER_FIRST] = _nextPointer;
            }

            var lastPointer = _hashes[lastHash + 1];
            if (lastPointer == NO_DATA)
            { // start new.
                _hashes[lastHash + 1] = _nextPointer;
            }
            else
            { // add at the end and change last pointer.
                while (_restrictions[lastPointer + POS_NEXT_POINTER_LAST] != NO_DATA)
                {
                    lastPointer = _restrictions[lastPointer + POS_NEXT_POINTER_LAST];
                }
                _restrictions[lastPointer + POS_NEXT_POINTER_LAST] = _nextPointer;
            }

            // add the data.
            _restrictions[_nextPointer + POS_SIZE] = (uint)restriction.Length;
            _restrictions[_nextPointer + POS_NEXT_POINTER_FIRST] = NO_DATA;
            _restrictions[_nextPointer + POS_NEXT_POINTER_LAST] = NO_DATA;
            for (var i = 0; i < restriction.Length; i++)
            {
                _restrictions[_nextPointer + POS_FIRST_VERTEX + i] = restriction[i];
            }
            _nextPointer = _nextPointer + size;
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public RestrictionEnumerator GetEnumerator()
        {
            return new RestrictionEnumerator(this);
        }

        /// <summary>
        /// Returns true if this db has complex restrictions.
        /// </summary>
        public bool HasComplexRestrictions
        {
            get
            {
                return _hasComplexRestrictions;
            }
        }

        /// <summary>
        /// Calculates a hashcode with a fixed size.
        /// </summary>
        private static int CalculateHash(uint vertex, int size)
        {
            var hash = (vertex.GetHashCode() % size);
            if (hash > 0)
            {
                return hash;
            }
            return -hash;
        }

        /// <summary>
        /// An enumerator to access restrictions.
        /// </summary>
        public class RestrictionEnumerator
        {
            private readonly RestrictionsDb _db;

            /// <summary>
            /// Creates a new restriction enumerator.
            /// </summary>
            internal RestrictionEnumerator(RestrictionsDb db)
            {
                _db = db;
            }

            private uint _vertex;
            private uint _pointer;
            private bool? _data = false; // keep status: false, no data available, null, data available, true no data available but ready to move next.
            private bool? _isFirst = null;

            /// <summary>
            /// Moves to restrictions with the given vertex as the start. 
            /// </summary>
            /// <returns>Returns true if there is a restriction for this vertex.</returns>
            public bool MoveToFirst(uint vertex)
            {
                var hash = CalculateHash(vertex, (int)_db._hashes.Length);
                var pointer = _db._hashes[hash];
                if (pointer == NO_DATA)
                { // a quick negative answer is the important part here!
                    _data = false;
                    _isFirst = null;
                    return false;
                }

                while (pointer != NO_DATA)
                {
                    if (_db._restrictions[pointer + POS_FIRST_VERTEX] == vertex)
                    {
                        _data = true;
                        _pointer = pointer;
                        _vertex = vertex;
                        _isFirst = true;
                        return true;
                    }
                    pointer = _db._restrictions[pointer + POS_NEXT_POINTER_FIRST];
                }
                _isFirst = null;
                return false;
            }

            /// <summary>
            /// Moves to restrictions with the given vertex as the end. 
            /// </summary>
            /// <returns>Returns true if there is a restriction for this vertex.</returns>
            public bool MoveToLast(uint vertex)
            {
                var hash = CalculateHash(vertex, (int)_db._hashes.Length);
                var pointer = _db._hashes[hash + 1];
                if (pointer == NO_DATA)
                { // a quick negative answer is the important part here!
                    _data = false;
                    _isFirst = null;
                    return false;
                }

                while (pointer != NO_DATA)
                {
                    var size = _db._restrictions[pointer + POS_SIZE];
                    if (_db._restrictions[pointer + size - 1 + POS_FIRST_VERTEX] == vertex)
                    {
                        _data = true;
                        _pointer = pointer;
                        _vertex = vertex;
                        _isFirst = false;
                        return true;
                    }
                    pointer = _db._restrictions[pointer + POS_NEXT_POINTER_LAST];
                    size = _db._restrictions[pointer + POS_SIZE];
                }
                _isFirst = null;
                return false;
            }

            /// <summary>
            /// Move to the next restriction.
            /// </summary>
            public bool MoveNext()
            {
                if (_data.HasValue)
                {
                    if (_data.Value)
                    {
                        _data = null;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                if (_isFirst == null)
                {
                    return false;
                }

                if (_isFirst.Value)
                {
                    // move next.
                    _pointer = _db._restrictions[_pointer + POS_NEXT_POINTER_FIRST];

                    // make sure the move is to the correct vertex.
                    while (_pointer != NO_DATA)
                    {
                        if (_db._restrictions[_pointer + POS_FIRST_VERTEX] == _vertex)
                        {
                            return true;
                        }
                        _pointer = _db._restrictions[_pointer + POS_NEXT_POINTER_FIRST];
                    }
                    _data = false;
                    return false;
                }
                else
                {
                    // move next.
                    _pointer = _db._restrictions[_pointer + POS_NEXT_POINTER_LAST];

                    // make sure the move is to the correct vertex.
                    while (_pointer != NO_DATA)
                    {
                        var size = _db._restrictions[_pointer + POS_SIZE];
                        if (_db._restrictions[_pointer + size - 1 + POS_FIRST_VERTEX] == _vertex)
                        {
                            return true;
                        }
                        _pointer = _db._restrictions[_pointer + POS_NEXT_POINTER_LAST];
                    }
                    _data = false;
                    return false;
                }
            }

            /// <summary>
            /// Gets the number of vertices in the current restriction.
            /// </summary>
            public uint Count
            {
                get
                {
                    if (_data == null)
                    {
                        throw new InvalidOperationException("No current data available.");
                    }
                    return _db._restrictions[_pointer + POS_SIZE];
                }
            }

            /// <summary>
            /// Returns the vertex at the given position.
            /// </summary>
            public uint this[int i]
            {
                get
                {
                    if (_data == null)
                    {
                        throw new InvalidOperationException("No current data available.");
                    }
                    return _db._restrictions[_pointer + POS_FIRST_VERTEX + i];
                }
            }
        }
    }
}