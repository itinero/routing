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
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Network
{
    /// <summary>
    /// Represents a resolved point. A hook for the router to route on.
    /// 
    /// The object represents a location and can be tagged.
    /// </summary>
    public class RouterPoint : ILocationObject
    {
        /// <summary>
        /// Creates a new router point.
        /// </summary>
        public RouterPoint(long id, Vehicle vehicle, GeoCoordinate location)
        {
            this.Id = id;
            this.Location = location;
            this.Vehicle = vehicle;
            this.Tags = new TagsCollection();
        }

        /// <summary>
        /// Returns the id of this router point.
        /// </summary>
        public long Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the location of this router point.
        /// </summary>
        public GeoCoordinate Location
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns the vehicle profile this point is valid for.
        /// </summary>
        public Vehicle Vehicle
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
            if (this.Location == null)
            {
                return string.Format("{0} ({1})",
                    this.Id, this.Tags.ToInvariantString());
            }
            return string.Format("{0} {1} ({2})",
                this.Id, this.Location.ToString(), this.Tags.ToInvariantString());
        }
    }
}