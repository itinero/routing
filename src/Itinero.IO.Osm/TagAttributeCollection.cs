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

using Itinero.Attributes;
using OsmSharp.Tags;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

namespace Itinero.IO.Osm
{
    /// <summary>
    /// A tag collection as an attribute collection.
    /// </summary>
    public class TagAttributeCollection : IAttributeCollection
    {
        private readonly TagsCollectionBase _tagCollection;

        /// <summary>
        /// Creates a tag attribute collection.
        /// </summary>
        public TagAttributeCollection(TagsCollectionBase tagsCollection)
        {
            _tagCollection = tagsCollection;
        }
        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return _tagCollection.Count;
            }
        }

        /// <summary>
        /// Gets the readonly flag.
        /// </summary>
        public bool IsReadonly
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Adds or replaces an attribute.
        /// </summary>
        public void AddOrReplace(string key, string value)
        {
            _tagCollection.AddOrReplace(key, value);
        }

        /// <summary>
        /// Clears all attributes.
        /// </summary>
        public void Clear()
        {
            _tagCollection.Clear();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Attributes.Attribute> GetEnumerator()
        {
            return _tagCollection.Select(x => new Attributes.Attribute(x.Key, x.Value)).GetEnumerator();
        }

        /// <summary>
        /// Removes the attribute with the given key.
        /// </summary>
        public bool RemoveKey(string key)
        {
            return _tagCollection.RemoveKey(key);
        }

        /// <summary>
        /// Tries to get the value for the given key.
        /// </summary>
        public bool TryGetValue(string key, out string value)
        {
            return _tagCollection.TryGetValue(key, out value);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return _tagCollection.Select(x => new Attributes.Attribute(x.Key, x.Value)).GetEnumerator();
        }
    }
}
