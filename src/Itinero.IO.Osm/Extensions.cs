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

        /// <summary>
        /// Adds or appends the tag value to the value collection.
        /// </summary>
        public static void AddOrAppend(this TagsCollectionBase tags, Tag tag)
        {
            foreach(var t in tags)
            {
                if (t.Key == tag.Key)
                {
                    if (!string.IsNullOrWhiteSpace(t.Value))
                    {
                        var values = t.Value.Split(',');
                        for (var i = 0; i < values.Length; i++)
                        {
                            if (values[i] == tag.Value)
                            {
                                return;
                            }
                        }
                        tags.AddOrReplace(tag.Key, t.Value + "," + tag.Value);
                    }
                    else
                    {
                        tags.AddOrReplace(tag);
                    }
                    return;
                }
            }
            tags.Add(tag);
        }
    }
}
