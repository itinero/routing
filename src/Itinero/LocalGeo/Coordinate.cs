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
using Itinero.Navigation.Directions;

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Represents a coordinate.
    /// </summary>
    public struct Coordinate
    {
        const double RadiusOfEarth = 6371000;

        /// <summary>
        /// Creates a new coordinate.
        /// </summary>
        public Coordinate(float latitude, float longitude)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Elevation = null;
        }

        /// <summary>
        /// Creates a new coordinate.
        /// </summary>
        public Coordinate(float latitude, float longitude, short elevation)
        {
            this.Latitude = latitude;
            this.Longitude = longitude;
            this.Elevation = elevation;
        }

        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Gets or sets the elevation in meter.
        /// </summary>
        public short? Elevation { get; set; }

        /// <summary>
        /// Offsets this coordinate for a given distance in a given direction.
        /// </summary>
        public Coordinate OffsetWithDirection(float distance, DirectionEnum direction)
        {
            var ratioInRadians = distance / RadiusOfEarth;

            var oldLat = this.Latitude.ToRadians();
            var oldLon = this.Longitude.ToRadians();
            var bearing = ((double)(int)direction).ToRadians();

            var newLatitude = System.Math.Asin(
                                     System.Math.Sin(oldLat) *
                                     System.Math.Cos(ratioInRadians) +
                                     System.Math.Cos(oldLat) *
                                     System.Math.Sin(ratioInRadians) *
                                     System.Math.Cos(bearing));

            var newLongitude = oldLon + System.Math.Atan2(
                                      System.Math.Sin(bearing) *
                                      System.Math.Sin(ratioInRadians) *
                                      System.Math.Cos(oldLat),
                                      System.Math.Cos(ratioInRadians) -
                                      System.Math.Sin(oldLat) *
                                      System.Math.Sin(newLatitude));
            
            var newLat = newLatitude.ToDegrees();
            if (newLat > 180)
            {
                newLat = newLat - 360;
            }
            var newLon = newLongitude.ToDegrees();
            if (newLon > 180)
            {
                newLon = newLon - 360;
            }
            return new Coordinate((float)newLat, (float)newLon);
        }

        /// <summary>
        /// Returns an estimate of the distance between the two given coordinates.
        /// </summary>
        /// <remarks>Accuraccy decreases with distance.</remarks>
        public static float DistanceEstimateInMeter(Coordinate coordinate1, Coordinate coordinate2)
        {
            return Coordinate.DistanceEstimateInMeter(coordinate1.Latitude, coordinate1.Longitude,
                coordinate2.Latitude, coordinate2.Longitude);
        }

        /// <summary>
        /// Returns an estimate of the distance between the two given coordinates.
        /// </summary>
        /// <remarks>Accuraccy decreases with distance.</remarks>
        public static float DistanceEstimateInMeter(float latitude1, float longitude1, float latitude2, float longitude2)
        {
            var lat1Rad = (latitude1 / 180d) * System.Math.PI;
            var lon1Rad = (longitude1 / 180d) * System.Math.PI;
            var lat2Rad = (latitude2 / 180d) * System.Math.PI;
            var lon2Rad = (longitude2 / 180d) * System.Math.PI;

            var x = (lon2Rad - lon1Rad) * System.Math.Cos((lat1Rad + lat2Rad) / 2.0);
            var y = lat2Rad - lat1Rad;

            var m = System.Math.Sqrt(x * x + y * y) * RadiusOfEarth;

            return (float)m;
        }

        /// <summary>
        /// Returns an estimate of the distance between the given sequence of coordinates.
        /// </summary>
        public static float DistanceEstimateInMeter(System.Collections.Generic.List<Coordinate> coordinates)
        {
            var length = 0f;
            for(var i = 1; i < coordinates.Count; i++)
            {
                length += Coordinate.DistanceEstimateInMeter(coordinates[i - 1].Latitude, coordinates[i - 1].Longitude,
                    coordinates[i].Latitude, coordinates[i].Longitude);
            }
            return length;
        }

        /// <summary>
        /// Offsets this coordinate with a given distance.
        /// </summary>
        public Coordinate OffsetWithDistances(float meter)
        {
            var offsetLat = new Coordinate(
                this.Latitude + 0.1f, this.Longitude);
            var offsetLon = new Coordinate(
                this.Latitude, this.Longitude + 0.1f);
            var latDistance = Coordinate.DistanceEstimateInMeter(offsetLat, this);
            var lonDistance = Coordinate.DistanceEstimateInMeter(offsetLon, this);

            return new Coordinate(this.Latitude + (meter / latDistance) * 0.1f,
                this.Longitude + (meter / lonDistance) * 0.1f);
        }
        
        /// <summary>
        /// Returns a description of this object.
        /// </summary>
        public override string ToString()
        {
            if (this.Elevation.HasValue)
            {
                return string.Format("{0},{1}@{2}m", this.Latitude.ToInvariantString(), this.Longitude.ToInvariantString(),
                    this.Elevation.Value.ToInvariantString());
            }
            return string.Format("{0},{1}", this.Latitude.ToInvariantString(), this.Longitude.ToInvariantString());
        }
    }
}