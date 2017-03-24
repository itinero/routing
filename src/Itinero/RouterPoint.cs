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

using Itinero.Algorithms;
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
        /// <param name="edgeId">The edge id.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="latitude">The latitude of the original location.</param>
        /// <param name="longitude">The longitude of the original location.</param>
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
        /// <param name="edgeId">The edge id.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="latitude">The latitude of the original location.</param>
        /// <param name="longitude">The longitude of the original location.</param>
        /// <param name="attributes">Meta-data about this point.</param>
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
        /// Gets the latitude of the original location.
        /// </summary>
        public float Latitude
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the longitude of the original location.
        /// </summary>
        public float Longitude
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets/sets meta-data about this point..
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