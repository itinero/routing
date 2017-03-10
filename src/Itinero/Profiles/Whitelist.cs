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

namespace Itinero.Profiles
{
    /// <summary>
    /// Represents a whitelist of attribute keys.
    /// </summary>
    public class Whitelist : IEnumerable<string>
    {
        private readonly HashSet<string> _whitelist;

        /// <summary>
        /// Creates a new whitelist.
        /// </summary>
        public Whitelist()
        {
            _whitelist = new HashSet<string>();
        }

        /// <summary>
        /// Creates a new whitelist.
        /// </summary>
        public Whitelist(HashSet<string> whitelist)
        {
            _whitelist = whitelist;
        }

        /// <summary>
        /// Sets the given key.
        /// </summary>
        public void Add(string key)
        {
            if (_whitelist != null)
            {
                _whitelist.Add(key);
            }
        }

        /// <summary>
        /// Returns true if the given keys in this whitelist.
        /// </summary>
        public bool Contains(string key)
        {
            if (_whitelist != null)
            {
                return _whitelist.Contains(key);
            }
            return false;
        }

        /// <summary>
        /// Gets the number of items.
        /// </summary>
        public int Count
        {
            get
            {
                return _whitelist.Count;
            }
        }

        /// <summary>
        /// Returns true if this is a dummy whitelist.
        /// </summary>
        public bool IsDummy
        {
            get
            {
                return _whitelist == null;
            }
        }

        /// <summary>
        /// Clears this whitelist.
        /// </summary>
        public void Clear()
        {
            if (_whitelist != null)
            {
                _whitelist.Clear();
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator()
        {
            return _whitelist.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _whitelist.GetEnumerator();
        }

        /// <summary>
        /// The dummy whitelist.
        /// </summary>
        public static Whitelist Dummy = new Whitelist(null);
    }
}