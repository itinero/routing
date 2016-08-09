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

using NetTopologySuite.Features;
using Itinero.Attributes;

namespace Itinero.Geo.Attributes
{
    /// <summary>
    /// Creates extensions related to attributes.
    /// </summary>
    public static class AttributeExtensions
    {
        /// <summary>
        /// Converts the attributes collection to an NTS attributes table.
        /// </summary>
        public static AttributesTable ToAttributesTable(this IAttributeCollection collection)
        {
            if(collection == null) { return null; }

            var attributes = new AttributesTable();
            foreach(var attribute in collection)
            {
                attributes.AddAttribute(attribute.Key, attribute.Value);
            }
            return attributes;
        }
    }
}