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
using Itinero.Graphs.Geometric.Shapes;

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Represents a box.
    /// </summary>
    public struct Box
    {
        private readonly float _minLat;
        private readonly float _minLon;
        private readonly float _maxLat;
        private readonly float _maxLon;
        
        /// <summary>
        /// Creates a new box.
        /// </summary>
        public Box(Coordinate coordinate1, Coordinate coordinate2)
            : this(coordinate1.Latitude, coordinate1.Longitude, coordinate2.Latitude, coordinate2.Longitude)
        {

        }

        /// <summary>
        /// Creates a new box.
        /// </summary>
        public Box(float lat1, float lon1, float lat2, float lon2)
        {
            if (lat1 < lat2)
            {
                _minLat = lat1;
                _maxLat = lat2;
            }
            else
            {
                _minLat = lat2;
                _maxLat = lat1;
            }

            if (lon1 < lon2)
            {
                _minLon = lon1;
                _maxLon = lon2;
            }
            else
            {
                _minLon = lon2;
                _maxLon = lon1;
            }
        }

        /// <summary>
        /// Gets the minimum latitude.
        /// </summary>
        public float MinLat
        {
            get
            {
                return _minLat;
            }
        }

        /// <summary>
        /// Gets the maximum latitude.
        /// </summary>
        public float MaxLat
        {
            get
            {
                return _maxLat;
            }
        }

        /// <summary>
        /// Gets the minimum longitude.
        /// </summary>
        public float MinLon
        {
            get
            {
                return _minLon;
            }
        }

        /// <summary>
        /// Gets the maximum longitude.
        /// </summary>
        public float MaxLon
        {
            get
            {
                return _maxLon;
            }
        }

        /// <summary>
        /// Returns true if this box overlaps the given coordinates.
        /// </summary>
        public bool Overlaps(float lat, float lon)
        {
            return _minLat < lat && lat <= _maxLat &&
                _minLon < lon && lon <= _maxLon;
        }

        /// <summary>
        /// Returns true if the given box overlaps with this one. Partial overlaps also return true.
        /// </summary>
        /// <param name="box">The other box.</param>
        /// <returns>True if any parts of the two boxes overlap.</returns>
        public bool Overlaps(Box box)
        {
            var thisCenter = this.Center;
            if (box.Overlaps(thisCenter.Latitude, thisCenter.Latitude))
            {
                return true;
            }
            var otherCenter = box.Center;
            if (this.Overlaps(otherCenter.Latitude, otherCenter.Latitude))
            {
                return true;
            }
            return this.IntersectsPotentially(box.MinLon, box.MinLat, box.MaxLon, box.MaxLat);
        }

        /// <summary>
        /// Expands this box (if needed) to incluce the given coordinate.
        /// </summary>
        public Box ExpandWith(float lat, float lon)
        {
            if (this.Overlaps(lat, lon))
            { // assume this happens in most cases.
                return this;
            }

            return new Box(System.Math.Min(this.MinLat, lat), System.Math.Min(this.MinLon, lon),
                System.Math.Max(this.MaxLat, lat), System.Math.Max(this.MaxLon, lon));
        }

        /// <summary>
        /// Returns true if the line potentially intersects with this box.
        /// </summary>
        public bool IntersectsPotentially(float longitude1, float latitude1, float longitude2, float latitude2)
        { // TODO: auwch, switch longitude and latitude, this is very very bad!
            if (longitude1 > _maxLon && longitude2 > _maxLon)
            {
                return false;
            }
            if (longitude1 < _minLon && longitude2 < _minLon)
            {
                return false;
            }
            if (latitude1 > _maxLat && latitude2 > _maxLat)
            {
                return false;
            }
            if (latitude1 < _minLat && latitude2 < _minLat)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the exact center of this box.
        /// </summary>
        public Coordinate Center
        {
            get
            {
                return new Coordinate()
                {
                    Latitude = (_maxLat + _minLat) / 2f,
                    Longitude = (_minLon + _maxLon) / 2f
                };
            }
        }

        /// <summary>
        /// Returns a resized version of this box.
        /// </summary>
        public Box Resize(float e)
        {
            return new Box(_minLat - e, _minLon - e, _maxLat + e, _maxLon + e);
        }
    }
}