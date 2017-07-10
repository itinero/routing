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
using Itinero.IO.Osm.Streams;
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Normalizer
{
    /// <summary>
    /// Holds helper extension methods for normalizing attribute collections.
    /// </summary>
    public static class AttributeCollectionExtensions
    {
        /// <summary>
        /// Normalizes nothing but the access tags.
        /// </summary>
        public static void NormalizeAccess(this IAttributeCollection tags, VehicleCache vehicleCache, Vehicle vehicle, string highwayType, IAttributeCollection profileTags)
        {
            var access = vehicleCache.CanTraverse(new AttributeCollection(new Attribute("highway", highwayType)), vehicle, false);
            tags.NormalizeAccess(profileTags, access, vehicle.VehicleTypes);
        }

        /// <summary>
        /// Normalizes access for the given hierarchy of access tags.
        /// </summary>
        public static void NormalizeAccess(this IAttributeCollection tags, IAttributeCollection profileTags, bool defaultAccess, params string[] accessTags)
        {
            bool? access = Itinero.Osm.Vehicles.Vehicle.InterpretAccessValue(tags, "access");
            for (var i = 0; i < accessTags.Length; i++)
            {
                var currentAccess = Itinero.Osm.Vehicles.Vehicle.InterpretAccessValue(tags, accessTags[i]);
                if (currentAccess != null)
                {
                    access = currentAccess;
                }
            }

            if (access != null && access.Value != defaultAccess)
            {
                if (access.Value)
                {
                    profileTags.AddOrReplace(accessTags[accessTags.Length - 1], "yes");
                }
                else
                {
                    profileTags.AddOrReplace(accessTags[accessTags.Length - 1], "no");
                }
            }
        }

        private static Dictionary<string, bool> _onewayValues = null;

        /// <summary>
        /// Gets the possible values for oneway.
        /// </summary>
        public static Dictionary<string, bool> OnewayValues
        {
            get
            {
                if (_onewayValues == null)
                {
                    _onewayValues = new Dictionary<string, bool>();
                    _onewayValues.Add("yes", true);
                    // _onewayValues.Add("no", false); // no is not a valid value, just drop it, it says nothing.
                    _onewayValues.Add("-1", false);
                    _onewayValues.Add("1", true);
                }
                return _onewayValues;
            }
        }

        /// <summary>
        /// Normalizes the oneway tag.
        /// </summary>
        public static void NormalizeOneway(this IAttributeCollection tags, IAttributeCollection profileTags)
        {
            string oneway;
            if (!tags.TryGetValue("oneway", out oneway))
            { // nothing to normalize.
                return;
            }
            bool defaultOnewayFound;
            if (!OnewayValues.TryGetValue(oneway, out defaultOnewayFound))
            { // invalid value.
                return;
            }

            if (defaultOnewayFound)
            {
                profileTags.AddOrReplace("oneway", "yes");
            }
            else
            {
                profileTags.AddOrReplace("oneway", "-1");
            }
        }

        /// <summary>
        /// Normalize the cycleway tag.
        /// </summary>
        public static void NormalizeCycleway(this IAttributeCollection tags, IAttributeCollection profileTags)
        {
            string cycleway;
            if (!tags.TryGetValue("cycleway", out cycleway))
            { // nothing to normalize.
                return;
            }
            if (cycleway == "cyclestreet")
            {
                profileTags.AddOrReplace("cycleway", "cyclestreet");
            }
            else if (cycleway == "lane")
            {
                profileTags.AddOrReplace("cycleway", "lane");
            }

            // TODO: add the unidirectional cycleway stuff. WARNING: direction of 'left' and 'right' depends on country.
        }

        /// <summary>
        /// Normalizes the oneway bicycle tag.
        /// </summary>
        public static void NormalizeOnewayBicycle(this IAttributeCollection tags, IAttributeCollection profileTags)
        {
            string oneway;
            if (!tags.TryGetValue("oneway:bicycle", out oneway))
            { // nothing to normalize.
                return;
            }
            if (oneway == "no")
            {
                profileTags.AddOrReplace("oneway:bicycle", "no");
            }
        }

        /// <summary>
        /// Normalizes maxspeed.
        /// </summary>
        public static void NormalizeMaxspeed(this IAttributeCollection tags, IAttributeCollection profileTags)
        {
            string maxspeed;
            if (!tags.TryGetValue("maxspeed", out maxspeed))
            { // nothing to normalize.
                return;
            }
            int maxSpeedValue;
            if (int.TryParse(maxspeed, out maxSpeedValue) &&
                maxSpeedValue > 0 && maxSpeedValue <= 200)
            {
                profileTags.AddOrReplace("maxspeed", maxspeed);
            }
            else if (maxspeed.EndsWith("mph"))
            {
                if (int.TryParse(maxspeed.Substring(0, maxspeed.Length - 3), out maxSpeedValue) &&
                    maxSpeedValue > 0 && maxSpeedValue <= 150)
                {
                    profileTags.AddOrReplace("maxspeed", maxspeed);
                }
            }
        }

        /// <summary>
        /// Normalizes the junction tag.
        /// </summary>
        /// <returns></returns>
        public static void NormalizeJunction(this IAttributeCollection tags, IAttributeCollection profileTags)
        {
            string junction;
            if (!tags.TryGetValue("junction", out junction))
            { // nothing to normalize.
                return;
            }
            if (junction == "roundabout")
            {
                profileTags.AddOrReplace("junction", "roundabout");
            }
        }

        private static Dictionary<string, bool?> _rampValues = null;

        /// <summary>
        /// Gets the possible values for ramp.
        /// </summary>
        public static Dictionary<string, bool?> RampValues
        {
            get
            {
                if (_rampValues == null)
                {
                    _rampValues = new Dictionary<string, bool?>();
                    _rampValues.Add("yes", null);
                }
                return _rampValues;
            }
        }

        /// <summary>
        /// Normalizes the ramp tag.
        /// </summary>
        public static void NormalizeRamp(this IAttributeCollection tags, IAttributeCollection profileTags, bool defaultAccess)
        {
            string ramp;
            if (!tags.TryGetValue("ramp", out ramp))
            { // nothing to normalize.
                return;
            }
            bool? defaultAccessFound;
            if (!RampValues.TryGetValue(ramp, out defaultAccessFound))
            { // invalid value.
                return;
            }

            if (defaultAccess != defaultAccessFound)
            {
                profileTags.AddOrReplace("ramp", ramp);
            }
        }
    }
}