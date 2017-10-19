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
using System.Collections.Generic;
using System.Linq;

namespace Itinero.IO.Shape.Writer
{
    public static class Extensions
    {
        private static HashSet<string> RefKeys = new HashSet<string>(
            new[] { "ref", "int_ref" });

        /// <summary>
        /// Extracts the name field using refs when appropriate.
        /// </summary>
        public static string ExtractName(this Itinero.Attributes.IAttributeCollection attributes)
        {
            string name = string.Empty;
            var refs = new List<string>();
            foreach (var at in attributes)
            {
                if (at.Key == "name")
                {
                    name = at.Value;
                }
                else if (RefKeys.Contains(at.Key))
                {
                    var values = at.Value.Split(';');
                    foreach (var value in values)
                    {
                        refs.Add(value.Replace(" ", string.Empty));
                    }
                }
            }

            if (refs.Count == 0)
            {
                return name;
            }
            else if (refs.Count == 1)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return refs[0];
                }
                return refs[0] + ";" + name;
            }

            var refCount =
                (from refValue in refs
                 group refValue by refValue into g
                 select new { g.Key, Count = g.Count() }).ToList();
            refCount.Sort((x, y) => x.Count.CompareTo(y.Count));
            if (refCount.Count == 1)
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return refCount[0].Key;
                }
                return refCount[0].Key + ";" + name;
            }
            if (string.IsNullOrWhiteSpace(name))
            {
                return refCount[0].Key + ";" + refCount[1].Key;
            }
            return refCount[0].Key + ";" + refCount[1].Key + ";" + name;
        }

        /// <summary>
        /// Adds the given attributes.
        /// </summary>
        public static void AddFrom(this AttributesTable table, string name, Itinero.Attributes.AttributeCollection tags)
        {
            table.AddFrom(name, tags, string.Empty);
        }

        /// <summary>
        /// Adds the given attributes.
        /// </summary>
        public static void AddFrom(this AttributesTable table, string name, Itinero.Attributes.AttributeCollection tags,
            string defaultValue)
        {
            var value = string.Empty;
            if (!tags.TryGetValue(name, out value))
            {
                value = defaultValue;
            }
            table.Add(name, value);
        }
    }
}