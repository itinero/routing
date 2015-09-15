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
using OsmSharp.Math;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Osm.Vehicles;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Represents a resolved point. A hook for the router to route on.
    /// 
    /// The object represents a location and can be tagged.
    /// </summary>
    public class RouterPoint
    {
        /// <summary>
        /// Creates a new router point.
        /// </summary>
        public RouterPoint(float latitude, float longitude, uint edgeId, float offset)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.EdgeId = edgeId;
            this.Offset = offset;
            this.Tags = new TagsCollection();
        }

        /// <summary>
        /// Gets the edge id.
        /// </summary>
        public uint EdgeId
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the offset in meters.
        /// </summary>
        public float Offset
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the latitude.
        /// </summary>
        public float Latitude
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the longitude.
        /// </summary>
        public float Longitude
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/sets the tags.
        /// </summary>
        public TagsCollectionBase Tags
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a description of this router point.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0},{1}] ({2})",
                this.Latitude.ToInvariantString(), this.Longitude.ToInvariantString(), 
                this.Tags.ToInvariantString());
        }
    }
}