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

using System;
using Itinero.Graphs.Geometric.Shapes;

namespace Itinero.Geo
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
        /// Returns true if the line potentially intersects with this box.
        /// </summary>
        public bool IntersectsPotentially(float longitude1, float latitude1, float longitude2, float latitude2)
        {
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