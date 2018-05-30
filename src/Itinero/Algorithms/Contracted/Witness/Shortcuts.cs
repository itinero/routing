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

using Itinero.Algorithms.Weights;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Text;

namespace Itinero.Algorithms.Contracted.Witness
{
    /// <summary>
    /// Contains the shorcuts around one vertex.
    /// </summary>
    public class Shortcuts<T> : IEnumerable<KeyValuePair<OriginalEdge, Shortcut<T>>>
        where T : struct
    {
        private readonly Dictionary<OriginalEdge, Shortcut<T>> _data;

        /// <summary>
        /// Creates a new shortcuts collection.
        /// </summary>
        public Shortcuts()
        {
            _data = new Dictionary<OriginalEdge, Shortcut<T>>();
        }

        private Shortcuts(Dictionary<OriginalEdge, Shortcut<T>> data)
        {
            _data = data;
        }

        /// <summary>
        /// Tries to get the shortcut associated with the given edge.
        /// </summary>
        public bool TryGetValue(OriginalEdge edge, out Shortcut<T> shortcut)
        {
            return _data.TryGetValue(edge, out shortcut);
        }

        /// <summary>
        /// Gets or sets a shortcut.
        /// </summary>
        public Shortcut<T> this[OriginalEdge edge]
        {
            get
            {
                return _data[edge];
            }
            set
            {
                _data[edge] = value;
            }
        }

        /// <summary>
        /// Adds or updates the weight for the given edge.
        /// </summary>
        public void AddOrUpdate(OriginalEdge edge, Shortcut<T> shortcut, WeightHandler<T> weightHandler)
        {
            Shortcut<T> existing;
            if (!_data.TryGetValue(edge, out existing))
            {
                _data[edge] = shortcut;
                return;
            }

            // update existing.
            var existingForward = weightHandler.GetMetric(existing.Forward);
            var newForward = weightHandler.GetMetric(shortcut.Forward);
            if (existingForward == 0 ||
                existingForward > newForward)
            {
                existing.Forward = shortcut.Forward;
            }
            var existingBackward = weightHandler.GetMetric(existing.Backward);
            var newBackward = weightHandler.GetMetric(shortcut.Backward);
            if (existingBackward == 0 ||
                existingBackward > newBackward)
            {
                existing.Backward = shortcut.Backward;
            }
            _data[edge] = existing;
        }

        public IEnumerable<OriginalEdge> Edges()
        {
            return _data.Keys;
        }

        /// <summary>
        /// Clears all shortcuts.
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<KeyValuePair<OriginalEdge, Shortcut<T>>> GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        /// <summary>
        /// Returns a description of this shortcuts collection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            foreach (var pair in this)
            {
                if (stringBuilder.Length > 0)
                {
                    stringBuilder.Append(Environment.NewLine);
                }
                stringBuilder.Append(pair.ToString());
            }
            return stringBuilder.ToString();
        }

        public Shortcuts<T> Clone()
        {
            var data = new Dictionary<OriginalEdge, Shortcut<T>>();
            foreach (var pair in _data)
            {
                data[pair.Key] = pair.Value;
            }
            return new Shortcuts<T>(data);
        }
    }
}