// Itinero - Routing for .NET
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
using NetTopologySuite.IO;
using System.Collections.Generic;

namespace Itinero.IO.Shape.Reader
{
    /// <summary>
    /// Contains extension method for the shapefile reader.
    /// </summary>
    public static class ShapefileReaderExtensions
    {
        /// <summary>
        /// Gets an attribute collection containing all the attributes in the current record in the shapefile reader.
        /// </summary>
        public static AttributeCollection ToAttributeCollection(this ShapefileDataReader reader)
        {
            var attributes = new AttributeCollection();
            reader.AddToAttributeCollection(attributes);
            return attributes;
        }

        /// <summary>
        /// Adds the attributes in the current record in the shapefile reader to the given attribute collection.
        /// </summary>
        public static void AddToAttributeCollection(this ShapefileDataReader reader, IAttributeCollection collection)
        {
            var valueString = string.Empty;
            for (var i = 1; i < reader.FieldCount; i++)
            {
                var name = reader.GetName(i);
                var value = reader.GetValue(i - 1);
                valueString = string.Empty;
                if (value != null)
                {
                    valueString = value.ToInvariantString();
                }
                collection.AddOrReplace(name, valueString);
            }
        }
    }
}