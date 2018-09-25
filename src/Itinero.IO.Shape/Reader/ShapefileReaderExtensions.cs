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
using NetTopologySuite.IO;

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