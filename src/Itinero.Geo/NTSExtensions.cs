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

namespace Itinero.Geo
{
    /// <summary>
    /// Contains extension methods for NTS-related functionality.
    /// </summary>
    public static class NTSExtensions
    {
        /// <summary>
        /// Tries to get a value from the attribute table.
        /// </summary>
        public static bool TryGetValue(this IAttributesTable table, string name, out object value)
        {
            var names = table.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    value = table.GetValues()[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Tries to get a value as a string from the attribute table.
        /// </summary>
        public static bool TryGetValueAsString(this IAttributesTable table, string name, out string value)
        {
            var names = table.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    var objValue = table.GetValues()[i];
                    if (objValue == null)
                    {
                        value = null;
                    }
                    else
                    {
                        value = objValue.ToInvariantString();
                    }
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Returns true if the given table contains the given attribute with the given value.
        /// </summary>
        public static bool Contains(this IAttributesTable table, string name, object value)
        {
            var names = table.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    return value.Equals(table.GetValues()[i]);
                }
            }
            return false;
        }

        /// <summary>
        /// Temporary extension method to prepare for NTS 1.15.
        /// </summary>
        public static void Add(this AttributesTable table, string name, object value)
        {
            table.Add(name, value);
        }
    }
}