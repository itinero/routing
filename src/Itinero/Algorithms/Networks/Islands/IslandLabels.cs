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
using System.Runtime.CompilerServices;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Profiles.Lua.Tree;
using Reminiscence;
using Reminiscence.Arrays;

[assembly: InternalsVisibleTo("Itinero.Test")]
namespace Itinero.Algorithms.Networks.Islands
{
    /// <summary>
    /// Holds island labels for each edge.
    /// </summary>
    public class IslandLabels
    {
        private readonly MemoryArray<uint> _labels;

        public const uint NotSet = uint.MaxValue;
        public const uint NoAccess = uint.MaxValue - 1;

        private uint _count = 0;

        /// <summary>
        /// Creates a new island labels data structure.
        /// </summary>
        internal IslandLabels()
        {
            _labels = new MemoryArray<uint>(1);
        }
        
        /// <summary>
        /// Gets or sets the label.
        /// </summary>
        /// <param name="id">The label.</param>
        public uint this[uint id]
        {
            get
            {
                if (id >= _labels.Length)
                {
                    return NotSet;
                }

                return _labels[id];
            }
            internal set
            {
                if (id >= _labels.Length)
                {
                    var l = _labels.Length;
                    _labels.EnsureMinimumSize(id + 1);
                    for (var i = l; i < _labels.Length; i++)
                    {
                        _labels[i] = NotSet;
                    }
                }

                if (id > _count)
                {
                    _count = id;
                }

                _labels[id] = value;
            }
        }

        /// <summary>
        /// Updates the label of this id and all labels along the way to their lowest equivalent.
        /// </summary>
        /// <param name="id"></param>
        internal uint UpdateLowest(uint id)
        {
            var label = this[id];

            if (label == NotSet ||
                label == NoAccess)
            {
                return label;
            }

            if (label == id)
            {
                return id;
            }

            var lowest = UpdateLowest(label);
            if (lowest == NoAccess ||
                lowest == NotSet)
            {
                throw new Exception($"Label at {label} cannot be {nameof(NoAccess)} or {nameof(NotSet)} because it has parent labels.");
            }
            this[id] = lowest;
            return lowest;
        }

        /// <summary>
        /// Returns the # of labels.
        /// </summary>
        public uint Count => _count + 1;
    }
}