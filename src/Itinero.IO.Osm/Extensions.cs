// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Attributes;
using OsmSharp.Tags;

namespace Itinero.IO.Osm
{
    /// <summary>
    /// Contains extension methods related to both Itinero and Itinero.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the given tags collection to an attributes collection.
        /// </summary>
        public static IAttributeCollection ToAttributes(this TagsCollectionBase tagsCollection)
        {
            if(tagsCollection == null)
            {
                return null;
            }

            var attributeCollection = new AttributeCollection();
            foreach(var tag in tagsCollection)
            {
                attributeCollection.AddOrReplace(tag.Key, tag.Value);
            }
            return attributeCollection;
        }

        /// <summary>
        /// Adds a tag as an attribute.
        /// </summary>
        public static void Add(this IAttributeCollection attributes, Tag tag)
        {
            attributes.AddOrReplace(tag.Key, tag.Value);
        }
    }
}
