// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Attributes
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
    }
}