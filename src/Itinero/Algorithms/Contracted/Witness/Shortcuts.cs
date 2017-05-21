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

namespace Itinero.Algorithms.Contracted.Witness
{
    /// <summary>
    /// A collection of shortcuts.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class Shortcuts<T>
        where T : struct
    {
        /// <summary>
        /// Creates a collection of shortcuts.
        /// </summary>
        public Shortcuts()
        {
            _data = new Shortcut<T>[64];
            _count = 0;
        }

        private int _count;
        private Shortcut<T>[] _data;

        /// <summary>
        /// Adds a new shortcut.
        /// </summary>
        public void Add(Shortcut<T> shortcut)
        {
            if (_data.Length <= _count)
            {
                System.Array.Resize(ref _data, _data.Length * 2);
            }
            _data[_count] = shortcut;
            _count++;
        }

        /// <summary>
        /// Gets or sets the shortcut at the given index.
        /// </summary>
        public Shortcut<T> this[int i]
        {
            get
            {
                return _data[i];
            }
            set
            {
                _data[i] = value;
            }
        }

        /// <summary>
        /// Gets the shorctuts.
        /// </summary>
        public int Count
        {
            get
            {
                return _count;
            }
        }

        /// <summary>
        /// Clears all shortcuts.
        /// </summary>
        public void Clear()
        {
            _count = 0;
        }
    }
}
