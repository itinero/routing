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
using OsmSharp.Geo;
using OsmSharp.Routing.Profiles;
using System.Collections.Generic;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Represents a route and all associate meta-data.
    /// </summary>
    public class Route
    {
        /// <summary>
        /// Tags for this route.
        /// </summary>
        public List<RouteTags> Tags { get; set; }

        /// <summary>
        /// A number of route metrics.
        /// </summary>
        /// <remarks>Can also be use for CO2 calculations or quality estimates.</remarks>
        public List<RouteMetric> Metrics { get; set; }
        
        /// <summary>
        /// An ordered array of route segments.
        /// </summary>
        public List<RouteSegment> Segments { get; set; }

        /// <summary>
        /// The distance in meter.
        /// </summary>
        public double TotalDistance { get; set; }

        /// <summary>
        /// The time in seconds.
        /// </summary>
        public double TotalTime { get; set; }
    }

    /// <summary>
    /// Represents a point along the route that is a stop.
    /// </summary>
    public class RouteStop : ICloneable
    {
        /// <summary>
        /// The latitude of the entry.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// The longitude of the entry.
        /// </summary>
        public float Longitude { get; set; }
        
        /// <summary>
        /// Tags for this route point.
        /// </summary>
        public RouteTags[] Tags { get; set; }

        /// <summary>
        /// A number of route metrics, usually containing time/distance.
        /// </summary>
        /// <remarks>Can also be use for CO2 calculations or quality estimates.</remarks>
        public RouteMetric[] Metrics { get; set; }

        #region ICloneable Members

        /// <summary>
        /// Clones this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new RouteStop();
            clone.Latitude = this.Latitude;
            clone.Longitude = this.Longitude;
            if (this.Metrics != null)
            {
                clone.Metrics = new RouteMetric[this.Metrics.Length];
                for (int idx = 0; idx < this.Metrics.Length; idx++)
                {
                    clone.Metrics[idx] = this.Metrics[idx].Clone() as RouteMetric;
                }
            }
            if (this.Tags != null)
            {
                clone.Tags = new RouteTags[this.Tags.Length];
                for (int idx = 0; idx < this.Tags.Length; idx++)
                {
                    clone.Tags[idx] = this.Tags[idx].Clone() as RouteTags;
                }
            }
            return clone;            
        }

        #endregion

        /// <summary>
        /// Returns true if the given point has the same name tags and positiong.
        /// </summary>
        /// <param name="routePoint"></param>
        /// <returns></returns>
        internal bool RepresentsSame(RouteStop routePoint)
        {
            if (routePoint == null) return false;

            if (this.Longitude == routePoint.Longitude &&
                this.Latitude == routePoint.Latitude)
            {
                if (routePoint.Tags != null || routePoint.Tags.Length == 0)
                { // there are tags in the other point.
                    if (this.Tags != null || this.Tags.Length == 0)
                    { // there are also tags in this point.
                        if (this.Tags.Length == routePoint.Tags.Length)
                        { // and they have the same number of tags!
                            for (int idx = 0; idx < this.Tags.Length; idx++)
                            {
                                if (this.Tags[idx].Key != routePoint.Tags[idx].Key ||
                                    this.Tags[idx].Value != routePoint.Tags[idx].Value)
                                { // tags don't equal.
                                    return false;
                                }
                            }
                            return true;
                        }
                        return false;
                    }
                }
                return (this.Tags != null || this.Tags.Length == 0);
            }
            return false;
        }
    }

    /// <summary>
    /// Represents a segment, or the smallest possible part of a route.
    /// </summary>
    public class RouteSegment : ICloneable
    {
        /// <summary>
        /// The latitude of the entry.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// The longitude of the entry.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// The name of the vehicle that this entry was calculated for.
        /// </summary>
        /// <remarks>This vehicle name is empty for unimodal routes.</remarks>
        public string Profile { get; set; }

        /// <summary>
        /// Tags of this entry.
        /// </summary>
        public RouteTags[] Tags { get; set; }

        /// <summary>
        /// A number of route metrics, usually containing time/distance.
        /// </summary>
        /// <remarks>Can also be use for CO2 calculations or quality estimates.</remarks>
        public RouteMetric[] Metrics { get; set; }

        /// <summary>
        /// Distance in meter to reach this part of the route.
        /// </summary>
        public double Distance { get; set; }

        /// <summary>
        /// Estimated time in seconds to reach this part of the route.
        /// </summary>
        public double Time { get; set; }

        /// <summary>
        /// The important or relevant points for this route at this point.
        /// </summary>
        public RouteStop[] Points { get; set; }

        /// <summary>
        /// The side streets entries.
        /// </summary>
        public RouteSegmentBranch[] SideStreets { get; set; }

        #region ICloneable Members

        /// <summary>
        /// Clones this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new RouteSegment();
            clone.Distance = this.Distance;
            clone.Latitude = this.Latitude;
            clone.Longitude = this.Longitude;
            if (this.Metrics != null)
            {
                clone.Metrics = new RouteMetric[this.Metrics.Length];
                for (int idx = 0; idx < this.Metrics.Length; idx++)
                {
                    clone.Metrics[idx] = this.Metrics[idx].Clone() as RouteMetric;
                }
            }
            if (this.Points != null)
            {
                clone.Points = new RouteStop[this.Points.Length];
                for (int idx = 0; idx < this.Points.Length; idx++)
                {
                    clone.Points[idx] = this.Points[idx].Clone() as RouteStop;
                }
            }
            if (this.SideStreets != null)
            {
                clone.SideStreets = new RouteSegmentBranch[this.SideStreets.Length];
                for (int idx = 0; idx < this.SideStreets.Length; idx++)
                {
                    clone.SideStreets[idx] = this.SideStreets[idx].Clone() as RouteSegmentBranch;
                }
            }
            if (this.Tags != null)
            {
                clone.Tags = new RouteTags[this.Tags.Length];
                for (int idx = 0; idx < this.Tags.Length; idx++)
                {
                    clone.Tags[idx] = this.Tags[idx].Clone() as RouteTags;
                }
            }
            clone.Profile = this.Profile;
            clone.Time = this.Time;
            return clone;
        }

        #endregion

        /// <summary>
        /// Creates a new route segment.
        /// </summary>
        /// <returns></returns>
        public static RouteSegment CreateNew(ICoordinate coordinate, Profile profile)
        {
            return new RouteSegment()
            {
                Latitude = coordinate.Latitude,
                Longitude = coordinate.Longitude,
                Profile = profile.Name
            };
        }

        /// <summary>
        /// Creates a new route segment.
        /// </summary>
        /// <returns></returns>
        public static RouteSegment CreateNew(float latitude, float longitude, Profile profile)
        {
            return new RouteSegment()
            {
                Latitude = latitude,
                Longitude = longitude,
                Profile = profile.Name
            };
        }

        /// <summary>
        /// Returns a string representation of this segment.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{2} - @{0}s {1}m",
                this.Time, this.Distance, this.Profile);
        }
    }

    /// <summary>
    /// Represents a segment that has not been taken but is important to the route.
    /// </summary>
    public class RouteSegmentBranch : ICloneable
    {
        /// <summary>
        /// The latitude of the entry.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// The longitude of the entry.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Tags of this entry.
        /// </summary>
        public RouteTags[] Tags { get; set; }

        #region ICloneable Members

        /// <summary>
        /// Returns a clone of this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            RouteSegmentBranch clone = new RouteSegmentBranch();
            clone.Latitude = this.Latitude;
            clone.Longitude = this.Longitude;
            if (this.Tags != null)
            {
                clone.Tags = new RouteTags[this.Tags.Length];
                for (int idx = 0; idx < this.Tags.Length; idx++)
                {
                    clone.Tags[idx] = this.Tags[idx].Clone() as RouteTags;
                }
            }
            return clone;
        }

        #endregion
    }

    /// <summary>
    /// Represents a key value pair.
    /// </summary>
    public class RouteTags : ICloneable
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public string Value { get; set; }

        #region ICloneable Members

        /// <summary>
        /// Returns a clone of this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            var clone = new RouteTags();
            clone.Key = this.Key;
            clone.Value = this.Value;
            return clone;
        }

        #endregion

        /// <summary>
        /// Returns a System.String that represents the current System.Object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}={1}",
                this.Key, this.Value);
        }
    }

    /// <summary>
    /// Contains extensions for route tags.
    /// </summary>
    public static class RouteTagsExtensions
    {        
        /// <summary>
        /// Converts a dictionary of tags to a RouteTags array.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static RouteTags[] ConvertFrom(this TagsCollectionBase tags)
        {
            var tagsList = new List<RouteTags>();
            foreach (Tag pair in tags)
            {
                var tag = new RouteTags();
                tag.Key = pair.Key;
                tag.Value = pair.Value;
                tagsList.Add(tag);
            }
            return tagsList.ToArray();
        }

        /// <summary>
        /// Converts a RouteTags array to a list of KeyValuePairs.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static TagsCollectionBase ConvertToTagsCollection(this RouteTags[] tags)
        {
            var tagsList = new TagsCollection();
            if (tags != null)
            {
                foreach (var pair in tags)
                {
                    tagsList.Add(new Tag(pair.Key, pair.Value));
                }
            }
            return tagsList;
        }

        /// <summary>
        /// Converts a dictionary of tags to a RouteTags array.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static RouteTags[] ConvertFrom(this IDictionary<string, string> tags)
        {
            var tags_list = new List<RouteTags>();
            foreach (var pair in tags)
            {
                RouteTags tag = new RouteTags();
                tag.Key = pair.Key;
                tag.Value = pair.Value;
                tags_list.Add(tag);
            }
            return tags_list.ToArray();
        }

        /// <summary>
        /// Converts a list of KeyValuePairs to a RouteTags array.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static RouteTags[] ConvertFrom(this List<KeyValuePair<string, string>> tags)
        {
            var tagsList = new List<RouteTags>();
            if (tags != null)
            {
                foreach (var pair in tags)
                {
                    var tag = new RouteTags();
                    tag.Key = pair.Key;
                    tag.Value = pair.Value;
                    tagsList.Add(tag);
                }
            }
            return tagsList.ToArray();
        }

        /// <summary>
        /// Converts a RouteTags array to a list of KeyValuePairs.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, string>> ConvertTo(this RouteTags[] tags)
        {
            var tagsList = new List<KeyValuePair<string, string>>();
            if (tags != null)
            {
                foreach (RouteTags pair in tags)
                {
                    tagsList.Add(new KeyValuePair<string, string>(pair.Key, pair.Value));
                }
            }
            return tagsList;
        }

        /// <summary>
        /// Returns the value of the first tag with the key given.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetValueFirst(this RouteTags[] tags, string key)
        {
            string first_value = null;
            if (tags != null)
            {
                foreach (RouteTags tag in tags)
                {
                    if (tag.Key == key)
                    {
                        first_value = tag.Value;
                        break;
                    }
                }
            }
            return first_value;
        }

        /// <summary>
        /// Returns all values for a given key.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static List<string> GetValues(this RouteTags[] tags, string key)
        {
            List<string> values = new List<string>();
            if (tags != null)
            {
                foreach (RouteTags tag in tags)
                {
                    if (tag.Key == key)
                    {
                        values.Add(tag.Value);
                    }
                }
            }
            return values;
        }
    }

    /// <summary>
    /// Represents a key value pair.
    /// </summary>
    public class RouteMetric : ICloneable
    {
        /// <summary>
        /// The key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// The value.
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// Convert from a regular tag dictionary.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static RouteMetric[] ConvertFrom(IDictionary<string, double> tags)
        {
            var tagsList = new List<RouteMetric>();
            foreach (KeyValuePair<string, double> pair in tags)
            {
                RouteMetric tag = new RouteMetric();
                tag.Key = pair.Key;
                tag.Value = pair.Value;
                tagsList.Add(tag);
            }
            return tagsList.ToArray();
        }

        /// <summary>
        /// Converts to regular tags list.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public static List<KeyValuePair<string, double>> ConvertTo(RouteMetric[] tags)
        {
            var tagsList = new List<KeyValuePair<string, double>>();
            if (tags != null)
            {
                foreach (RouteMetric pair in tags)
                {
                    tagsList.Add(new KeyValuePair<string, double>(pair.Key, pair.Value));
                }
            }
            return tagsList;
        }

        #region ICloneable Members

        /// <summary>
        /// Returns a clone of this object.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            RouteMetric clone = new RouteMetric();
            clone.Key = this.Key;
            clone.Value = this.Value;
            return clone;
        }

        #endregion
    }
}