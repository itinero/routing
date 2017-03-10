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

namespace Itinero.Attributes
{
    /// <summary>
    /// Abstract representation of an attribute collection.
    /// </summary>
    public interface IAttributeCollection : IEnumerable<Attribute>
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the readonly flag.
        /// </summary>
        bool IsReadonly { get; }

        /// <summary>
        /// Adds or replaces an attribute.
        /// </summary>
        void AddOrReplace(string key, string value);

        /// <summary>
        /// Tries to get the value for the given key.
        /// </summary>
        bool TryGetValue(string key, out string value);

        /// <summary>
        /// Removes the attribute with the given key.
        /// </summary>
        bool RemoveKey(string key);

        /// <summary>
        /// Clears all attributes.
        /// </summary>
        void Clear();
    }
}