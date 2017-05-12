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

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Represents a collection of shortcuts.
    /// </summary>
    public sealed class Shortcuts<T>
        where T : struct
    {
        /// <summary>
        /// Creates a new collections of shortcuts.
        /// </summary>
        public Shortcuts()
        {
            _sources = new OriginalEdge[64];

            _targetsCount = new int[64];
            _targets = new Shortcut<T>[64][];
        }

        private OriginalEdge[] _sources;
        private int _sourceCount; // the # of sources.
        private Shortcut<T>[][] _targets;
        private int[] _targetsCount; // the # of targets per source.

        /// <summary>
        /// Gets the shortcut accessor.
        /// </summary>
        /// <returns></returns>
        public Accessor GetAccessor()
        {
            return new Accessor(this);
        }

        /// <summary>
        /// Clears this witnesses collection.
        /// </summary>
        public void Clear()
        {
            _sourceCount = 0;
        }

        /// <summary>
        /// A shortcut enumerator.
        /// </summary>
        public sealed class Accessor
        {
            private readonly Shortcuts<T> _data;

            public Accessor(Shortcuts<T> data)
            {
                _data = data;
                _source = -1;
                _target = -1;
            }

            private int _source;
            private int _target;

            /// <summary>
            /// Move to the next source.
            /// </summary>
            public bool MoveNextSource()
            {
                _source++;
                _target = -1;
                return _source < _data._sourceCount;
            }

            /// <summary>
            /// Gets the current source.
            /// </summary>
            public OriginalEdge Source
            {
                get
                {
                    return _data._sources[_source];
                }
            }

            /// <summary>
            /// Returns true if there is a source selected.
            /// </summary>
            public bool HasSource
            {
                get
                {
                    return _source >= 0 &&
                        _source < _data._sourceCount;
                }
            }

            /// <summary>
            /// Gets the sources count.
            /// </summary>
            public int SourceCount
            {
                get
                {
                    return _data._sourceCount;
                }
            }
            
            /// <summary>
            /// Adds a new source.
            /// </summary>
            public void AddSource(OriginalEdge source)
            {
                _data._sourceCount++;
                if (_data._sources.Length <= _data._sourceCount)
                {
                    Array.Resize(ref _data._sources, _data._sources.Length * 2);
                    Array.Resize(ref _data._targetsCount, _data._targetsCount.Length * 2);
                    Array.Resize(ref _data._targets, _data._targets.Length * 2);
                }
                _source = _data._sourceCount - 1;
                _target = -1;
                _data._sources[_source] = source;
                _data._targetsCount[_source] = 0;
            }

            /// <summary>
            /// Moves to the next target.
            /// </summary>
            /// <returns></returns>
            public bool MoveNextTarget()
            {
                _target++;
                return _target < _data._targetsCount[_source];
            }

            /// <summary>
            /// Gets the target count.
            /// </summary>
            public int TargetCount
            {
                get
                {
                    return _data._targetsCount[_source];
                }
            }

            /// <summary>
            /// Gets the shortcut.
            /// </summary>
            public Shortcut<T> Target
            {
                get
                {
                    return _data._targets[_source][_target];
                }
                set
                {
                    _data._targets[_source][_target] = value;
                }
            }

            /// <summary>
            /// Returns true if there is a target selected.
            /// </summary>
            public bool HasTarget
            {
                get
                {
                    if (!this.HasSource)
                    {
                        return false;
                    }
                    return _target >= 0 &&
                        _target < _data._targetsCount[_source];
                }
            }

            /// <summary>
            /// Adds a new shortcut using the current source.
            /// </summary>
            public void Add(Shortcut<T> witness)
            {
                _data._targetsCount[_source]++;
                if (_data._targets[_source] == null)
                {
                    _data._targets[_source] = new Shortcut<T>[64];
                }
                if (_data._targets[_source].Length <= _data._targetsCount[_source])
                {
                    Array.Resize(ref _data._targets[_source], _data._targets[_source].Length * 2);
                }
                _target = _data._targetsCount[_source] - 1;
                _data._targets[_source][_target] = witness;
            }

            /// <summary>
            /// Resets this enumerator until the last source.
            /// </summary>
            public void ResetTarget()
            {
                _target = -1;
            }
            
            /// <summary>
            /// Resets this enumerator.
            /// </summary>
            public void Reset()
            {
                _source = -1;
                _target = -1;
            }
        }
    }
}