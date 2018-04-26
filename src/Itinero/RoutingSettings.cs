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

using System.Collections.Generic;

namespace Itinero
{
    /// <summary>
    /// Represents routing settings.
    /// </summary>
    public class RoutingSettings<T>
    {
        private readonly Dictionary<string, T> _maxSearch;

        /// <summary>
        /// Creates new routing settings.
        /// </summary>
        public RoutingSettings()
        {
            _maxSearch = new Dictionary<string, T>();
            this.DirectionAbsolute = true;
        }

        /// <summary>
        /// Sets the maximum search weight for the given profile.
        /// </summary>
        public void SetMaxSearch(string profile, T weight)
        {
            _maxSearch[profile] = weight;
        }

        /// <summary>
        /// Gets the maximum search weight for the given profile.
        /// </summary>am>
        /// <returns></returns>
        public bool TryGetMaxSearch(string profile, out T weight)
        {
            return _maxSearch.TryGetValue(profile, out weight);
        }

        /// <summary>
        /// Gets or sets the direction absolute flag.
        /// </summary>
        /// <remarks>When true any route not following the source or target directions will not be considered.</remarks>
        public bool DirectionAbsolute { get; set; }

        /// <summary>
        /// Creates a deep-copy of this object.
        /// </summary>
        /// <returns></returns>
        public RoutingSettings<T> Clone()
        {
            var clone = new RoutingSettings<T>()
            {
                DirectionAbsolute = this.DirectionAbsolute
            };
            foreach (var item in _maxSearch)
            {
                clone.SetMaxSearch(item.Key, item.Value);
            }
            return clone;
        }
    }
}