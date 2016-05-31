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
using System.Collections.Generic;

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
        private readonly Dictionary<uint, LinkedNode> _helperIndex; // an index to help at write-time, specifically to help switch vertices.

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
            _helperIndex = new Dictionary<uint, LinkedNode>();
        }
        
        /// <summary>
        /// Creates a new restrictions db.
        /// </summary>
        private RestrictionsDb()
        {

        }
        
        private uint _nextPointer = 0;
        private int _count = 0;

        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public void Add(params uint[] restriction)
        {
            if (this.IsReadonly) { throw new InvalidOperationException("Restrictions db is readonly, check IsReadonly before adding new restrictions."); }
            if (restriction == null) { throw new ArgumentNullException("restriction"); }
            if (restriction.Length == 0) { throw new ArgumentException("Restriction should contain one or more vertices.", "restriction"); }

            if (restriction.Length > 1)
            {
                _hasComplexRestrictions = true;
            }

            while (_nextPointer + (uint)restriction.Length + POS_FIRST_VERTEX >= _restrictions.Length)
            {
                _restrictions.Resize(_restrictions.Length + BLOCKSIZE);
            }

            // add the data.
            _restrictions[_nextPointer + POS_SIZE] = (uint)restriction.Length;
            _restrictions[_nextPointer + POS_NEXT_POINTER_FIRST] = NO_DATA;
            _restrictions[_nextPointer + POS_NEXT_POINTER_LAST] = NO_DATA;
            for (var i = 0; i < restriction.Length; i++)
            {
                _restrictions[_nextPointer + POS_FIRST_VERTEX + i] = restriction[i];
            }

            // add by pointer.
            this.AddByPointer(_nextPointer);

            // add to helper index.
            this.AddToHelperIndex(_nextPointer, restriction);

            _nextPointer = _nextPointer + POS_FIRST_VERTEX + (uint)restriction.Length;
            _count++;
        }

        /// <summary>
        /// Gets the number of restrictions in this db.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Switches the given two vertices.
        /// </summary>
        public void Switch(uint vertex1, uint vertex2)
        {
            if (this.IsReadonly) { throw new InvalidOperationException("Restrictions db is readonly, check IsReadonly before adding new restrictions."); }

            // collect all relevant restrictions.
            var pointers = new HashSet<uint>();
            LinkedNode nodeVertex1;
            if (_helperIndex.TryGetValue(vertex1, out nodeVertex1))
            {
                var node = nodeVertex1;
                while (node != null)
                {
                    pointers.Add(node.Pointer);
                    node = node.Next;
                }
            }
            LinkedNode nodeVertex2;
            if (_helperIndex.TryGetValue(vertex2, out nodeVertex2))
            {
                var node = nodeVertex2;
                while (node != null)
                {
                    pointers.Add(node.Pointer);
                    node = node.Next;
                }
            }
            
            foreach(var pointer in pointers)
            {
                // remove using pointer.
                this.RemoveByPointer(pointer);
            }

            foreach(var pointer in pointers)
            {
                // switch vertices in restrictions.
                this.SwitchAtPointer(pointer, vertex1, vertex2);

                // add using pointer.
                this.AddByPointer(pointer);
            }

            if (nodeVertex1 != null)
            {
                _helperIndex[vertex2] = nodeVertex1;
            }

            if (nodeVertex2 != null)
            {
                _helperIndex[vertex1] = nodeVertex2;
            }
        }

        /// <summary>
        /// Adds a restriction using just it's pointer.
        /// </summary>
        private void AddByPointer(uint pointer)
        {
            var restrictionSize = _restrictions[pointer + POS_SIZE];
            var size = restrictionSize + POS_FIRST_VERTEX;
            var firstVertex = _restrictions[pointer + POS_FIRST_VERTEX];
            var firstHash = CalculateHash(firstVertex);
            var lastVertex = _restrictions[pointer + POS_FIRST_VERTEX + restrictionSize - 1];
            var lastHash = CalculateHash(lastVertex) + 1;

            var firstPointer = _hashes[firstHash];
            if (firstPointer == NO_DATA)
            { // start new.
                _hashes[firstHash] = pointer;
            }
            else
            { // add at the end and change last pointer.
                while (_restrictions[firstPointer + POS_NEXT_POINTER_FIRST] != NO_DATA)
                {
                    firstPointer = _restrictions[firstPointer + POS_NEXT_POINTER_FIRST];
                }
                _restrictions[firstPointer + POS_NEXT_POINTER_FIRST] = pointer;
            }

            var lastPointer = _hashes[lastHash];
            if (lastPointer == NO_DATA)
            { // start new.
                _hashes[lastHash] = pointer;
            }
            else
            { // add at the end and change last pointer.
                while (_restrictions[lastPointer + POS_NEXT_POINTER_LAST] != NO_DATA)
                {
                    lastPointer = _restrictions[lastPointer + POS_NEXT_POINTER_LAST];
                }
                _restrictions[lastPointer + POS_NEXT_POINTER_LAST] = pointer;
            }
        }

        /// <summary>
        /// Removes a restriction by pointer. Leaves the data in place.
        /// </summary>
        private void RemoveByPointer(uint pointer)
        {
            // possible cases for first and last:
            // - pointer is at hash and there is no next restriction.
            // - pointer is at hash but there is a next restrictions.
            // - pointer is not at hash and there is no next restriction.
            // - pointer is not at hash and there is a next restriction.
            
            // do the first vertex.
            var vertex = _restrictions[pointer + POS_FIRST_VERTEX];
            var hash = CalculateHash(vertex);
            var p = _hashes[hash];
            if (p == NO_DATA)
            {
                throw new Exception("Cannot remove this pointer, has it already been removed?");
            }
            if (p == pointer)
            {
                _hashes[hash] = _restrictions[p + POS_NEXT_POINTER_FIRST];
                _restrictions[p + POS_NEXT_POINTER_FIRST] = NO_DATA;
            }
            else
            {
                var previous = p;
                var next = _restrictions[p + POS_NEXT_POINTER_FIRST];
                while (true)
                {
                    p = next;
                    if (p == NO_DATA)
                    {
                        throw new Exception("Cannot remove this pointer, has it already been removed?");
                    }
                    next = _restrictions[p + POS_NEXT_POINTER_FIRST];
                    if (p == pointer)
                    { // skip/bypass this restriction but leave it in place.
                        _restrictions[previous + POS_NEXT_POINTER_FIRST] = next;
                        _restrictions[p + POS_NEXT_POINTER_FIRST] = NO_DATA;
                        break;
                    }
                    previous = p;
                }
            }

            // do the last vertex.
            var size = _restrictions[pointer + POS_SIZE];
            vertex = _restrictions[pointer + POS_FIRST_VERTEX + size - 1];
            hash = CalculateHash(vertex) + 1;
            p = _hashes[hash];
            if (p == NO_DATA)
            {
                throw new Exception("Cannot remove this pointer, has it already been removed?");
            }
            if (p == pointer)
            {
                _hashes[hash] = _restrictions[p + POS_NEXT_POINTER_LAST];
                _restrictions[p + POS_NEXT_POINTER_LAST] = NO_DATA;
            }
            else
            {
                var previous = p;
                var next = _restrictions[p + POS_NEXT_POINTER_LAST];
                while (true)
                {
                    p = next;
                    if (p == NO_DATA)
                    {
                        throw new Exception("Cannot remove this pointer, has it already been removed?");
                    }
                    next = _restrictions[p + POS_NEXT_POINTER_LAST];
                    if (p == pointer)
                    { // skip/bypass this restriction but leave it in place.
                        _restrictions[previous + POS_NEXT_POINTER_LAST] = next;
                        _restrictions[p + POS_NEXT_POINTER_LAST] = NO_DATA;
                        break;
                    }
                    previous = p;
                }
            }
        }

        /// <summary>
        /// Switches vertices in the restriction at the given pointer.
        /// </summary>
        private void SwitchAtPointer(uint pointer, uint vertex1, uint vertex2)
        {
            var size = _restrictions[pointer + POS_SIZE];
            for (var p = pointer + POS_FIRST_VERTEX; p < pointer + size + POS_FIRST_VERTEX; p++)
            {
                var vertex = _restrictions[p];
                if (vertex == vertex1)
                {
                    _restrictions[p] = vertex2;
                }
                else if(vertex == vertex2)
                { 
                    _restrictions[p] = vertex1;
                }
            }
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
        /// Returns true if this db is readonly.
        /// </summary>
        public bool IsReadonly
        {
            get
            {
                return _helperIndex == null;
            }
        }

        /// <summary>
        /// Calculates a hashcode with a fixed size.
        /// </summary>
        private int CalculateHash(uint vertex)
        {
            var hash = (vertex.GetHashCode() % (_hashes.Length / 2)) * 2;
            if (hash > 0)
            {
                return (int)hash;
            }
            return (int)-hash;
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
                var hash = _db.CalculateHash(vertex);
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
                var hash = _db.CalculateHash(vertex) + 1;
                var pointer = _db._hashes[hash];
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
                    if (_data != null)
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
                    if (_data != null)
                    {
                        throw new InvalidOperationException("No current data available.");
                    }
                    return _db._restrictions[_pointer + POS_FIRST_VERTEX + i];
                }
            }
        }

        /// <summary>
        /// Adds a new restriction and corresponding pointer.
        /// </summary>
        private void AddToHelperIndex(uint pointer, uint[] restriction)
        {
            foreach(var vertex in restriction)
            {
                LinkedNode next = null;
                _helperIndex.TryGetValue(vertex, out next);
                _helperIndex[vertex] = new LinkedNode()
                {
                    Next = next,
                    Pointer = pointer
                };
            }
        }

        private class LinkedNode
        {
            public uint Pointer { get; set; }
            public LinkedNode Next { get; set; }
        }
    }
}