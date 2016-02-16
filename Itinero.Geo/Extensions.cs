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

using System.Collections.Generic;

namespace Itinero.Geo
{
    /// <summary>
    /// Contains general extensions related to GeoAPI/NTS and Itinero.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the coordinate to a geoapi coordinate.
        /// </summary>
        public static GeoAPI.Geometries.Coordinate ToCoordinate(this Coordinate coordinate)
        {
            return new GeoAPI.Geometries.Coordinate(coordinate.Longitude, coordinate.Latitude);
        }

        /// <summary>
        /// Converts a geoapi coordinate to a coordinate.
        /// </summary>
        public static Coordinate FromCoordinate(this GeoAPI.Geometries.Coordinate coordinate)
        {
            return new Coordinate((float)coordinate.Y, (float)coordinate.X);
        }

        /// <summary>
        /// Converts a list of coordinates to geoapi coordinates.
        /// </summary>
        public static List<GeoAPI.Geometries.Coordinate> ToCoordinates(this List<Coordinate> coordinates)
        {
            if (coordinates == null)
            {
                return null;
            }

            var geoApiCoordinates = new List<GeoAPI.Geometries.Coordinate>(coordinates.Count);
            for (var i = 0; i < coordinates.Count; i++)
            {
                geoApiCoordinates.Add(coordinates[i].ToCoordinate());
            }
            return geoApiCoordinates;
        }

        /// <summary>
        /// Converts a list of coordinates to geoapi coordinates.
        /// </summary>
        public static GeoAPI.Geometries.Coordinate[] ToCoordinatesArray(this List<Coordinate> coordinates)
        {
            if (coordinates == null)
            {
                return null;
            }

            var geoApiCoordinates = new GeoAPI.Geometries.Coordinate[coordinates.Count];
            for (var i = 0; i < coordinates.Count; i++)
            {
                geoApiCoordinates[i] = coordinates[i].ToCoordinate();
            }
            return geoApiCoordinates;
        }

        /// <summary>
        /// Converts a list of coordinates to geoapi coordinates.
        /// </summary>
        public static List<GeoAPI.Geometries.Coordinate> ToCoordinates(this Coordinate[] coordinates)
        {
            if (coordinates == null)
            {
                return null;
            }

            var geoApiCoordinates = new List<GeoAPI.Geometries.Coordinate>(coordinates.Length);
            for (var i = 0; i < coordinates.Length; i++)
            {
                geoApiCoordinates.Add(coordinates[i].ToCoordinate());
            }
            return geoApiCoordinates;
        }

        /// <summary>
        /// Converts an array of coordinates to an array of geoapi coordinates.
        /// </summary>
        public static GeoAPI.Geometries.Coordinate[] ToCoordinatesArray(this Coordinate[] coordinates)
        {
            if (coordinates == null)
            {
                return null;
            }

            var geoApiCoordinates = new GeoAPI.Geometries.Coordinate[coordinates.Length];
            for (var i = 0; i < coordinates.Length; i++)
            {
                geoApiCoordinates[i] = coordinates[i].ToCoordinate();
            }
            return geoApiCoordinates;
        }
    }
}
