// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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
