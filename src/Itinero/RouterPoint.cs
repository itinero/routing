// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Attributes;

namespace Itinero
{
    /// <summary>
    /// Represents a resolved point. A hook for the router to route on.
    /// </summary>
    public class RouterPoint
    {
        /// <summary>
        /// Creates a new router point.
        /// </summary>
        public RouterPoint(float latitude, float longitude, uint edgeId, ushort offset)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.EdgeId = edgeId;
            this.Offset = offset;
            this.Attributes = new AttributeCollection();
        }

        /// <summary>
        /// Creates a new router point.
        /// </summary>
        public RouterPoint(float latitude, float longitude, uint edgeId, ushort offset,
            params Attribute[] attributes)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.EdgeId = edgeId;
            this.Offset = offset;
            this.Attributes = new AttributeCollection(attributes);
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
        /// Gets the offset.
        /// </summary>
        public ushort Offset
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
        public IAttributeCollection Attributes
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
            return string.Format("{0}@{1}% [{2},{3}] {4}",
                this.EdgeId,
                System.Math.Round(((float)this.Offset / ushort.MaxValue) * 100, 1).ToInvariantString(),
                this.Latitude.ToInvariantString(), 
                this.Longitude.ToInvariantString(), 
                this.Attributes.ToInvariantString());
        }
    }
}