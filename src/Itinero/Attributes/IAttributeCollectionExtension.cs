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

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Itinero.Attributes
{
    // TODO: move this to the OSM namespace, this has nothing to do with Itinero.

    /// <summary>
    /// Contains extension methods for attribute collections.
    /// </summary>
    public static class IAttributeCollectionExtension
    {
        private static HashSet<string> BooleanTrueValues = new HashSet<string>(
            new string[] { "yes", "true", "1" });
        private static HashSet<string> BooleanFalseValues = new HashSet<string>(
            new string[] { "no", "false", "0" });

        private const string RegexDecimal = @"\s*(\d+(?:\.\d*)?)\s*";
        private const string RegexDecimalWhiteSpace = @"\s*" + RegexDecimal + @"\s*"; 
        private const string RegexUnitTons = @"\s*(t|to|tonnes|tonnen)?\s*";
        private const string RegexUnitMeters = @"\s*(m|meters|metres|meter)?\s*";
        private const string RegexUnitKilometers = @"\s*(km)?\s*";
        private const string RegexUnitKilometersPerHour = @"\s*(km/h|kmh|kph|kmph)?\s*";
        private const string RegexUnitKnots = @"\s*(knots)\s*";
        private const string RegexUnitMilesPerHour = @"\s*(mph)\s*";

        /// <summary>
        /// Returns true if the given tags key has an associated value that can be interpreted as true.
        /// </summary>
        public static bool IsTrue(this IReadonlyAttributeCollection tags, string tagKey)
        {
            if (tags == null || string.IsNullOrWhiteSpace(tagKey))
                return false;

            // TryGetValue tests if the 'tagKey' is present, returns true if the associated value can be interpreted as true.
            //                                               returns false if the associated value can be interpreted as false.
            string tagValue;
            return tags.TryGetValue(tagKey, out tagValue) &&
                BooleanTrueValues.Contains(tagValue.ToLowerInvariant());
        }

        /// <summary>
        /// Searches the attributes collection for the access attributes and returns the associated values.
        /// 
        /// http://wiki.openstreetmap.org/wiki/Key:access
        /// </summary>
        /// <param name="tags">The tags to search.</param>
        /// <param name="accessTagHierachy">The hierarchy of <c>Access</c>-Tags for different vehicle types.</param>
        /// <returns>The best fitting value is returned.</returns>
        public static string GetAccessTag(this IReadonlyAttributeCollection tags, IEnumerable<string> accessTagHierachy)
        {
            if (tags == null)
                return null;
            foreach (string s in accessTagHierachy)
            {
                string access;
                if (tags.TryGetValue(s, out access))
                    return access;
            }
            return null;
        }
        /// <summary>
        /// Searches for a maxspeed tag and returns the associated value.
        /// 
        ///  http://wiki.openstreetmap.org/wiki/Key:maxspeed
        /// </summary>
        public static bool TryGetMaxSpeed(this IReadonlyAttributeCollection attributes, out float kmPerHour)
        {
            kmPerHour = float.MaxValue;
            string tagValue;
            if (attributes == null || !attributes.TryGetValue("maxspeed", out tagValue) || string.IsNullOrWhiteSpace(tagValue) ||
                tagValue == "none" || tagValue == "signals" || tagValue == "signs" || tagValue == "no")
                return false;
            return IAttributeCollectionExtension.TryParseSpeed(tagValue, out kmPerHour);
        }

        /// <summary>
        /// Searches for a maxweight tag and returns the associated value.
        /// 
        ///  http://wiki.openstreetmap.org/wiki/Key:maxweight
        /// </summary>
        public static bool TryGetMaxWeight(this IReadonlyAttributeCollection tags, out float kilogram)
        {
            kilogram = float.MaxValue;
            string tagValue;
            if (tags == null || !tags.TryGetValue("maxweight", out tagValue) || string.IsNullOrWhiteSpace(tagValue))
                return false;
            return IAttributeCollectionExtension.TryParseWeight(tagValue, out kilogram);
        }

        /// <summary>
        /// Searches for a max axle load tag and returns the associated value.
        /// 
        /// http://wiki.openstreetmap.org/wiki/Key:maxaxleload
        /// </summary>
        public static bool TryGetMaxAxleLoad(this IReadonlyAttributeCollection tags, out float kilogram)
        {
            kilogram = float.MaxValue;
            string tagValue;
            if (tags == null || !tags.TryGetValue("maxaxleload", out tagValue) || string.IsNullOrWhiteSpace(tagValue))
                return false;
            return IAttributeCollectionExtension.TryParseWeight(tagValue, out kilogram);
        }

        /// <summary>
        /// Searches for a max height tag and returns the associated value.
        /// 
        /// http://wiki.openstreetmap.org/wiki/Maxheight
        /// </summary>
        public static bool TryGetMaxHeight(this IReadonlyAttributeCollection tags, out float meter)
        {
            meter = float.MaxValue;

            string tagValue;
            if (tags == null || !tags.TryGetValue("maxheight", out tagValue) || string.IsNullOrWhiteSpace(tagValue))
                return false;

            return IAttributeCollectionExtension.TryParseLength(tagValue, out meter);
        }

        /// <summary>
        /// Searches for a max width tag and returns the associated value.
        /// 
        /// http://wiki.openstreetmap.org/wiki/Key:maxwidth
        /// </summary>
        /// <param name="attributes">The tags to search.</param>
        /// <param name="meter"></param>
        /// <returns></returns>
        public static bool TryGetMaxWidth(this IReadonlyAttributeCollection attributes, out float meter)
        {
            meter = float.MaxValue;
            string tagValue;

            if (attributes == null || !attributes.TryGetValue("maxwidth", out tagValue) || string.IsNullOrWhiteSpace(tagValue))
                return false;

            return IAttributeCollectionExtension.TryParseLength(tagValue, out meter);
        }

        /// <summary>
        /// Searches for a max length tag and returns the associated value.
        /// 
        /// http://wiki.openstreetmap.org/wiki/Key:maxlength
        /// </summary>
        /// <param name="tags">The tags to search.</param>
        /// <param name="meter"></param>
        /// <returns></returns>
        public static bool TryGetMaxLength(this IDictionary<string, string> tags, out float meter)
        {
            meter = float.MaxValue;

            string tagValue;
            if (tags == null || !tags.TryGetValue("maxlength", out tagValue) || string.IsNullOrWhiteSpace(tagValue))
                return false;

            return IAttributeCollectionExtension.TryParseLength(tagValue, out meter);
        }

        /// <summary>
        /// Returns true if the given tags key has an associated value that can be interpreted as false.
        /// </summary>
        public static bool IsFalse(this IReadonlyAttributeCollection tags, string tagKey)
        {
            if (tags == null || string.IsNullOrWhiteSpace(tagKey))
                return false;
            string tagValue;
            return tags.TryGetValue(tagKey, out tagValue) &&
                BooleanFalseValues.Contains(tagValue.ToLowerInvariant());
        }

        #region Parsing Units

        /// <summary>
        /// Tries to parse a speed value from a given tag-value.
        /// </summary>
        public static bool TryParseSpeed(string s, out float kmPerHour)
        {
            kmPerHour = float.MaxValue;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            if (s[0] != '0' && s[0] != '1' && s[0] != '2' && s[0] != '3' && s[0] != '4' &&
                s[0] != '5' && s[0] != '6' && s[0] != '7' && s[0] != '8' && s[0] != '9')
            { // performance improvement, quick negative answer.
                return false;
            }

            if (s.Contains(","))
            { // refuse comma as a decimal seperator or anywhere else in the number.
                return false;
            }

            // try regular speed: convention in OSM is km/h in this case.
            if (float.TryParse(s, NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out kmPerHour))
            {
                return true;
            }

            // try km/h
            if (IAttributeCollectionExtension.TryParseKilometerPerHour(s, out kmPerHour))
            {
                return true;
            }

            // try mph.
            float milesPerHour;
            if (IAttributeCollectionExtension.TryParseMilesPerHour(s, out milesPerHour))
            {
                kmPerHour = milesPerHour * 1.60934f;
                return true;
            }

            // try knots.
            float resultKnots;
            if (IAttributeCollectionExtension.TryParseKnots(s, out resultKnots))
            {
                kmPerHour = resultKnots * 1.85200f;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse a string containing a kilometer per hour value.
        /// </summary>
        public static bool TryParseKilometerPerHour(string s, out float kmPerHour)
        {
            s = s.ToStringEmptyWhenNull().Trim().ToLower();

            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out kmPerHour))
            { // the value is just a numeric value.
                return true;
            }

            // do some more parsing work.
            var regex = new Regex("^" + RegexDecimalWhiteSpace +
                RegexUnitKilometersPerHour + "$", RegexOptions.IgnoreCase);
            var match = regex.Match(s);
            if (match.Success)
            {
                kmPerHour = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to parse a string containing a miles per hour value.
        /// </summary>
        public static bool TryParseMilesPerHour(string s, out float milesPerHour)
        {
            milesPerHour = 0;
            float value;
            if (float.TryParse(s, out value))
            { // the value is just a numeric value.
                milesPerHour = value;
                return true;
            }

            // do some more parsing work.
            var regex = new Regex("^" + IAttributeCollectionExtension.RegexDecimalWhiteSpace +
                IAttributeCollectionExtension.RegexUnitMilesPerHour + "$", RegexOptions.IgnoreCase);
            var match = regex.Match(s);
            if (match.Success)
            {
                milesPerHour = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tries to parse a string containing knots.
        /// </summary>
        public static bool TryParseKnots(string s, out float knots)
        {
            knots = 0;
            if (float.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out knots))
            { // the value is just a numeric value.
                return true;
            }

            // do some more parsing work.
            var regex = new Regex("^" + IAttributeCollectionExtension.RegexDecimalWhiteSpace + 
                IAttributeCollectionExtension.RegexUnitKnots + "$", RegexOptions.IgnoreCase);
            var match = regex.Match(s);
            if (match.Success)
            {
                knots = float.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the result of the ToString() method or an empty string
        /// when the given object is null.
        /// </summary>
        public static string ToStringEmptyWhenNull(this object obj)
        {
            if (obj == null)
            {
                return string.Empty;
            }
            return obj.ToString();
        }

        /// <summary>
        /// Tries to parse a weight value from a given tag-value.
        /// </summary>
        public static bool TryParseWeight(string s, out float kilogram)
        {
            kilogram = float.MaxValue;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            var tonnesRegex = new Regex("^" + RegexDecimal + RegexUnitTons + "$", RegexOptions.IgnoreCase);
            var tonnesMatch = tonnesRegex.Match(s);
            if (tonnesMatch.Success)
            {
                kilogram = float.Parse(tonnesMatch.Groups[1].Value, CultureInfo.InvariantCulture) * 1000;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse a distance measure from a given tag-value.
        /// </summary>
        public static bool TryParseLength(string s, out float meter)
        {
            meter = float.MaxValue;

            if (string.IsNullOrWhiteSpace(s))
                return false;

            var metresRegex = new Regex("^" + RegexDecimal + RegexUnitMeters + "$", RegexOptions.IgnoreCase);
            var metresMatch = metresRegex.Match(s);
            if (metresMatch.Success)
            {
                meter = float.Parse(metresMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                return true;
            }

            var feetInchesRegex = new Regex("^(\\d+)\\'(\\d+)\\\"$", RegexOptions.IgnoreCase);
            var feetInchesMatch = feetInchesRegex.Match(s);
            if (feetInchesMatch.Success)
            {
                var feet = int.Parse(feetInchesMatch.Groups[1].Value, CultureInfo.InvariantCulture);
                var inches = int.Parse(feetInchesMatch.Groups[2].Value, CultureInfo.InvariantCulture);

                meter = feet * 0.3048f + inches * 0.0254f;
                return true;
            }

            return false;
        }

        #endregion

        /// <summary>
        /// Adds a new attribute.
        /// </summary>
        public static void AddOrReplace(this IAttributeCollection attributes, Attribute attribute)
        {
            attributes.AddOrReplace(attribute.Key, attribute.Value);
        }

        /// <summary>
        /// Adds or replaces the existing attributes to/in the given collection of attributes.
        /// </summary>
        public static void AddOrReplace(this IAttributeCollection attributes, IEnumerable<Attribute> other)
        {
            if (other == null) { return; }

            foreach (var attribute in other)
            {
                attributes.AddOrReplace(attribute.Key, attribute.Value);
            }
        }

        /// <summary>
        /// Deserializes the attribute collection.
        /// </summary>
        public static IAttributeCollection DeserializeWithSize(Stream stream)
        {
            var position = stream.Position;
            var compressed = stream.ReadByte();
            var sizeBytes = new byte[4];
            stream.Read(sizeBytes, 0, 4);
            var size = BitConverter.ToInt32(sizeBytes, 0);

            var attributes = new AttributeCollection();
            while (stream.Position - position < size + 5)
            {
                var key = stream.ReadWithSizeString();
                var value = stream.ReadWithSizeString();

                attributes.AddOrReplace(key, value);
            }
            return attributes;
        }

        /// <summary>
        /// Serializes this attribute collection with size.
        /// </summary>
        public static long SerializeWithSize(this IAttributeCollection attributes, Stream stream)
        {
            var position = stream.Position;
            stream.WriteByte(1);
            stream.Seek(4, SeekOrigin.Current);

            foreach(var attribute in attributes)
            {
                stream.WriteWithSize(attribute.Key);
                stream.WriteWithSize(attribute.Value);
            }

            var size = stream.Position - position - 5;
            stream.Seek(position + 1, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes((int)size), 0, 4);
            stream.Seek(position + size + 5, SeekOrigin.Begin);
            return size + 5;
        }

        /// <summary>
        /// Tries to get a single value for the given key.
        /// </summary>
        public static bool TryGetSingle(this IReadonlyAttributeCollection attributes, string key, out float value)
        {
            string stringValue;
            if (!attributes.TryGetValue(key, out stringValue))
            {
                value = 0;
                return false;
            }
            return float.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        /// <summary>
        /// Sets a single value.
        /// </summary>
        public static void SetSingle(this IAttributeCollection attributes, string key, float value)
        {
            attributes.AddOrReplace(key, value.ToInvariantString());
        }

        /// <summary>
        /// Returns true if the given attribute is found.
        /// </summary>
        public static bool Contains(this IReadonlyAttributeCollection attributes, string key, string value)
        {
            string foundValue;
            if (!attributes.TryGetValue(key, out foundValue))
            {
                return false;
            }
            return value == foundValue;
        }

        /// <summary>
        /// Returns true if the given attribute collection contains the same attributes than the given collection.
        /// </summary>
        public static bool ContainsSame(this IReadonlyAttributeCollection attributes, IReadonlyAttributeCollection other)
        {
            if (attributes == null && other == null)
            {
                return true;
            }
            else if(attributes == null)
            {
                return other.Count == 0;
            }
            else if (other == null)
            {
                return attributes.Count == 0;
            }

            if (attributes.Count != other.Count)
            {
                return false;
            }

            foreach(var a in attributes)
            {
                if (!other.Contains(a.Key, a.Value))
                {
                    return false;
                }
            }

            foreach (var a in other)
            {
                if (!attributes.Contains(a.Key, a.Value))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given attribute collection contains the same attributes than the given collection.
        /// </summary>
        public static bool ContainsSame(this IReadonlyAttributeCollection attributes, IReadonlyAttributeCollection other, params string[] exclude)
        {            
            var attributesCount = 0;
            var otherCount = 0;
            if (attributes != null)
            {
                foreach (var a in attributes)
                {
                    if (!exclude.Contains(a.Key))
                    {
                        attributesCount++;
                        if (!other.Contains(a.Key, a.Value))
                        {
                            return false;
                        }
                    }
                }
            }

            if (other != null)
            {
                foreach (var a in other)
                {
                    if (!exclude.Contains(a.Key))
                    {
                        otherCount++;
                        if (!attributes.Contains(a.Key, a.Value))
                        {
                            return false;
                        }
                    }
                }
            }
            return attributesCount == otherCount;
        }
    }
}