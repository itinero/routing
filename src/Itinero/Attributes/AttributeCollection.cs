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
using System.Linq;
using System.Text;

namespace Itinero.Attributes
{
    /// <summary>
    /// Represents an attribute collection.
    /// </summary>
    public class AttributeCollection : IAttributeCollection
    {
        private readonly Dictionary<string, string> _attributes;

        /// <summary>
        /// Creates an attribute collection.
        /// </summary>
        public AttributeCollection()
        {
            _attributes = new Dictionary<string, string>();
        }

        /// <summary>
        /// Creates an attribute collection.
        /// </summary>
        public AttributeCollection(params Attribute[] attributes)
            : this((IEnumerable<Attribute>)attributes)
        {

        }

        /// <summary>
        /// Creates an attribute collection.
        /// </summary>
        public AttributeCollection(IEnumerable<Attribute> attributes)
        {
            _attributes = new Dictionary<string, string>();
            if (attributes != null)
            {
                foreach(var attribute in attributes)
                {
                    _attributes[attribute.Key] = attribute.Value;
                }
            }
        }

        /// <summary>
        /// Gets the count.
        /// </summary>
        public int Count
        {
            get
            {
                return _attributes.Count;
            }
        }

        /// <summary>
        /// Clears all data.
        /// </summary>
        public void Clear()
        {
            _attributes.Clear();
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
        /// Tries to get the value for the given key.
        /// </summary>
        public bool TryGetValue(string key, out string value)
        {
            return _attributes.TryGetValue(key, out value);
        }

        /// <summary>
        /// Adds or replaces an attribute.
        /// </summary>
        public void AddOrReplace(string key, string value)
        {
            _attributes[key] = value;
        }

        /// <summary>
        /// Removes the attribute with this key.
        /// </summary>
        public bool RemoveKey(string key)
        {
            return _attributes.Remove(key);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<Attribute> GetEnumerator()
        {
            return _attributes.Select(x => new Attribute(x.Key, x.Value)).GetEnumerator();
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets a proper description of this attribute collection.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            foreach(var a in this)
            {
                if (builder.Length > 0)
                {
                    builder.Append('|');
                }
                builder.Append(a.ToString());
            }
            return builder.ToString();
        }
        
        /// <summary>
        /// Checks if a given key exists.
        /// </summary>
        public bool ContainsKey(string key)
        {
            return _attributes.ContainsKey(key);
        }
    }
}
