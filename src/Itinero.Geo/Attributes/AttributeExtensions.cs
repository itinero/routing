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
                attributes.Add(attribute.Key, attribute.Value);
            }
            return attributes;
        }

        /// <summary>
        /// Converts an NTS attributes table to an attributes collection.
        /// </summary>
        public static IAttributeCollection ToAttributesCollection(this IAttributesTable table)
        {
            if (table == null) { return null; }

            var attributes = new AttributeCollection();
            var name = table.GetNames();
            var values = table.GetValues();
            for(var i = 0; i < name.Length; i++)
            {
                var value = values[i];
                if (value == null)
                {
                    attributes.AddOrReplace(name[i], string.Empty);
                }
                else
                {
                    attributes.AddOrReplace(name[i], value.ToInvariantString());
                }
            }
            return attributes;
        }
    }
}