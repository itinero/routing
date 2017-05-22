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

namespace Itinero.Algorithms.Restrictions
{
    /// <summary>
    /// Represents restriction collection.
    /// </summary>
    public sealed class RestrictionCollection
    {
        private readonly Func<RestrictionCollection, uint, bool> _update;

        /// <summary>
        /// Creates a new restriction collection.
        /// </summary>
        public RestrictionCollection(Func<RestrictionCollection, uint, bool> update)
        {
            _update = update;
            _restrictions = new Restriction[64];
            _count = 0;
            _vertex = Constants.NO_VERTEX;
        }

        private Restriction[] _restrictions;
        private int _count;
        private uint _vertex;

        /// <summary>
        /// Updates this restriction collection.
        /// </summary>
        /// <return>Returns true if there are restrictions.</return>
        public bool Update(uint vertex)
        {
            if (_vertex == vertex)
            {
                return true;
            }

            _vertex = vertex;

            return _update(this, _vertex);
        }

        /// <summary>
        /// Returns the restriction at the given index.
        /// </summary>
        public Restriction this[int i]
        {
            get
            {
                return _restrictions[i];
            }
        }

        /// <summary>
        /// Gets the # of restrictions.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Returns the vertex.
        /// </summary>
        public uint Vertex
        {
            get
            {
                return _vertex;
            }
        }

        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public void Add(Restriction restriction)
        {
            if (_count >= _restrictions.Length)
            {
                Array.Resize(ref _restrictions, _restrictions.Length * 2);
            }
            _restrictions[_count] = restriction;
            _count++;
        }

        /// <summary>
        /// Clears all restrictions.
        /// </summary>
        public void Clear()
        {
            _count = 0;
        }
    }
}