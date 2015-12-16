// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
using System.Collections.Generic;

namespace OsmSharp.Routing.Osm
{
    /// <summary>
    /// A normalizer for OSM-routing tags.
    /// </summary>
    public static class OsmRoutingTagNormalizer
    {
        /// <summary>
        /// Splits the given tags into a normalized version, profile tags, and the rest in metatags.
        /// </summary>
        /// <returns></returns>
        public static bool Normalize(this TagsCollection tags, TagsCollection profileTags, 
            TagsCollection metaTags)
        {
            string highway;
            if(!tags.TryGetValue("highway", out highway))
            { // there is no highway tag, don't continue the search.
                return false;
            }

            // normalize access tags.
            if(!tags.NormalizeAccess(profileTags, metaTags))
            { // access is denied, don't use this way.
                return false;
            }

            // normalize maxspeed tags.
            tags.NormalizeMaxspeed(profileTags, metaTags);

            // normalize oneway tags.
            tags.NormalizeOneway(profileTags, metaTags);

            // normalize junction=roundabout tag.
            tags.NormalizeJunction(profileTags, metaTags);

            switch(highway)
            {
                case "motorway":
                case "motorway_link":
                case "trunk":
                case "trunk_link":
                case "primary":
                case "primary_link":
                    tags.NormalizeFoot(profileTags, metaTags, false);
                    tags.NormalizeBicycle(profileTags, metaTags, false);
                    tags.NormalizeMotorvehicle(profileTags, metaTags, true);
                    profileTags.Add("highway", highway);
                    break;
                case "secondary":
                case "secondary_link":
                case "tertiary":
                case "tertiary_link":
                case "unclassified":
                case "residential":
                case "road":
                case "service":
                case "services":
                case "living_street":
                case "track":
                    tags.NormalizeFoot(profileTags, metaTags, true);
                    tags.NormalizeBicycle(profileTags, metaTags, true);
                    tags.NormalizeMotorvehicle(profileTags, metaTags, true);
                    profileTags.Add("highway", highway);
                    break;
                case "cycleway":
                    tags.NormalizeFoot(profileTags, metaTags, false);
                    tags.NormalizeBicycle(profileTags, metaTags, true);
                    tags.NormalizeMotorvehicle(profileTags, metaTags, false);
                    profileTags.Add("highway", highway);
                    break;
                case "path":
                    tags.NormalizeFoot(profileTags, metaTags, true);
                    tags.NormalizeBicycle(profileTags, metaTags, true);
                    tags.NormalizeMotorvehicle(profileTags, metaTags, false);
                    profileTags.Add("highway", highway);
                    break;
                case "pedestrian":
                case "footway":
                case "steps":
                    tags.NormalizeFoot(profileTags, metaTags, true);
                    tags.NormalizeBicycle(profileTags, metaTags, false);
                    tags.NormalizeMotorvehicle(profileTags, metaTags, false);
                    profileTags.Add("highway", highway);
                    break;
            }

            return true;
        }

        private static Dictionary<string, bool?> _accessValues = null;

        /// <summary>
        /// Gets the possible values for access.
        /// </summary>
        public static Dictionary<string, bool?> AccessValues
        {
            get
            {
                if (_accessValues == null)
                {
                    _accessValues = new Dictionary<string, bool?>();
                    _accessValues.Add("private", false);
                    _accessValues.Add("yes", true);
                    _accessValues.Add("no", false);
                    _accessValues.Add("permissive", true);
                    _accessValues.Add("destination", true);
                    _accessValues.Add("customers", false);
                    _accessValues.Add("agricultural", null);
                    _accessValues.Add("forestry", null);
                    _accessValues.Add("designated", true);
                    _accessValues.Add("public", true);
                    _accessValues.Add("discouraged", null);
                    _accessValues.Add("delivery", true);
                }
                return _accessValues;
            }
        }

        /// <summary>
        /// Normalizes the access tags and adds them to the profile tags or meta tags.
        /// </summary>
        /// <returns></returns>
        public static bool NormalizeAccess(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags)
        {
            string access;
            if (!tags.TryGetValue("access", out access))
            { // nothing to normalize.
                return true;
            }
            bool? defaultAccessFound;
            if (!AccessValues.TryGetValue(access, out defaultAccessFound))
            { // invalid value.
                return true;
            }

            if (!defaultAccessFound.HasValue)
            { // access needs to be descided on a vehicle by vehicle basis.
                profileTags.Add("access", access);
            }
            return defaultAccessFound.Value;
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
        /// <returns></returns>
        public static void NormalizeOneway(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags)
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
                profileTags.Add("oneway", "yes");
            }
            else
            {
                profileTags.Add("oneway", "-1");
            }
        }

        /// <summary>
        /// Normalizes maxspeed.
        /// </summary>
        public static void NormalizeMaxspeed(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags)
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
                profileTags.Add("maxspeed", maxspeed);
            }
            else if(maxspeed.EndsWith("mph"))
            {
                if (int.TryParse(maxspeed.Substring(0, maxspeed.Length - 4), out maxSpeedValue) &&
                    maxSpeedValue > 0 && maxSpeedValue <= 150)
                {
                    profileTags.Add("maxspeed", maxspeed);
                }
            }
        }

        /// <summary>
        /// Normalizes the junction tag.
        /// </summary>
        /// <returns></returns>
        public static void NormalizeJunction(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags)
        {
            string junction;
            if (!tags.TryGetValue("junction", out junction))
            { // nothing to normalize.
                return;
            }
            if(junction == "roundabout")
            {
                profileTags.Add("junction", "roundabout");
            }
        }

        private static Dictionary<string, bool?> _footValues = null;

        /// <summary>
        /// Gets the possible values for foots.
        /// </summary>
        public static Dictionary<string, bool?> FootValues
        {
            get
            {
                if (_footValues == null)
                {
                    _footValues = new Dictionary<string, bool?>();
                    _footValues.Add("yes", true);
                    _footValues.Add("designated", true);
                    _footValues.Add("no", false);
                    _footValues.Add("permissive", true);
                    _footValues.Add("official", true);
                    _footValues.Add("destination", true);
                    _footValues.Add("private", false);
                    _footValues.Add("use_sidewalk", true);
                }
                return _footValues;
            }
        }

        /// <summary>
        /// Normalizes the foot tag.
        /// </summary>
        public static void NormalizeFoot(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags, bool defaultAccess)
        {
            string foot;
            if (!tags.TryGetValue("foot", out foot))
            { // nothing to normalize.
                return;
            }
            bool? defaultAccessFound;
            if (!FootValues.TryGetValue(foot, out defaultAccessFound))
            { // invalid value.
                return;
            }

            if (defaultAccess != defaultAccessFound)
            {
                profileTags.Add("foot", foot);
            }
        }

        private static Dictionary<string, bool?> _bicycleValues = null;

        /// <summary>
        /// Gets the possible values for bicycles.
        /// </summary>
        public static Dictionary<string, bool?> BicycleValues
        {
            get
            {
                if(_bicycleValues == null)
                {
                    _bicycleValues = new Dictionary<string, bool?>();
                    _bicycleValues.Add("yes", true);
                    _bicycleValues.Add("no", false);
                    _bicycleValues.Add("designated", true);
                    _bicycleValues.Add("dismount", null);
                    _bicycleValues.Add("use_sidepath", true);
                    _bicycleValues.Add("private", false);
                    _bicycleValues.Add("official", true);
                    _bicycleValues.Add("destination", true);
                }
                return _bicycleValues;
            }
        }

        /// <summary>
        /// Normalizes the bicycle tag.
        /// </summary>
        public static void NormalizeBicycle(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags, bool defaultAccess)
        {
            string bicycle;
            if(!tags.TryGetValue("bicycle", out bicycle))
            { // nothing to normalize.
                return;
            }
            bool? defaultAccessFound;
            if(!BicycleValues.TryGetValue(bicycle, out defaultAccessFound))
            { // invalid value.
                return;
            }

            if (defaultAccess != defaultAccessFound)
            {
                profileTags.Add("bicycle", bicycle);
            }
        }


        private static Dictionary<string, bool?> _motorvehicleValues = null;

        /// <summary>
        /// Gets the possible values for motorvehicles.
        /// </summary>
        public static Dictionary<string, bool?> MotorvehicleValues
        {
            get
            {
                if (_motorvehicleValues == null)
                {
                    _motorvehicleValues = new Dictionary<string, bool?>();
                    _motorvehicleValues.Add("no", false);
                    _motorvehicleValues.Add("yes", true);
                    _motorvehicleValues.Add("private", false);
                    _motorvehicleValues.Add("agricultural", null);
                    _motorvehicleValues.Add("destination", true);
                    _motorvehicleValues.Add("forestry", null);
                    _motorvehicleValues.Add("designated", true);
                    _motorvehicleValues.Add("permissive", false);
                    _motorvehicleValues.Add("delivery", null);
                    _motorvehicleValues.Add("official", true);
                }
                return _motorvehicleValues;
            }
        }

        /// <summary>
        /// Normalizes the motorvehicle tag.
        /// </summary>
        public static void NormalizeMotorvehicle(this TagsCollection tags, TagsCollection profileTags,
            TagsCollection metaTags, bool defaultAccess)
        {
            string motorvehicle;
            if (!tags.TryGetValue("motorvehicle", out motorvehicle))
            { // nothing to normalize.
                return;
            }
            bool? defaultAccessFound;
            if (!MotorvehicleValues.TryGetValue(motorvehicle, out defaultAccessFound))
            { // invalid value.
                return;
            }

            if (defaultAccess != defaultAccessFound)
            {
                profileTags.Add("motorvehicle", motorvehicle);
            }
        }
    }
}