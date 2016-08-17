// Itinero - Routing for .NET
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
        public static GeoAPI.Geometries.Coordinate ToCoordinate(this LocalGeo.Coordinate coordinate)
        {
            return new GeoAPI.Geometries.Coordinate(coordinate.Longitude, coordinate.Latitude);
        }

        /// <summary>
        /// Converts a geoapi coordinate to a coordinate.
        /// </summary>
        public static LocalGeo.Coordinate FromCoordinate(this GeoAPI.Geometries.Coordinate coordinate)
        {
            return new LocalGeo.Coordinate((float)coordinate.Y, (float)coordinate.X);
        }

        /// <summary>
        /// Converts a list of coordinates to geoapi coordinates.
        /// </summary>
        public static List<GeoAPI.Geometries.Coordinate> ToCoordinates(this List<LocalGeo.Coordinate> coordinates)
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
        public static GeoAPI.Geometries.Coordinate[] ToCoordinatesArray(this List<LocalGeo.Coordinate> coordinates)
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
        public static List<GeoAPI.Geometries.Coordinate> ToCoordinates(this LocalGeo.Coordinate[] coordinates)
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
        public static GeoAPI.Geometries.Coordinate[] ToCoordinatesArray(this LocalGeo.Coordinate[] coordinates)
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

        /// <summary>
        /// Converts the given coordinates list to the a linear ring.
        /// </summary>
        public static NetTopologySuite.Geometries.LinearRing ToLinearRing(this List<LocalGeo.Coordinate> coordinates)
        {
            return new NetTopologySuite.Geometries.LinearRing(coordinates.ToCoordinatesArray());
        }

        /// <summary>
        /// Converts the given coordinates list list to the an array of linear rings.
        /// </summary>
        public static NetTopologySuite.Geometries.LinearRing[] ToLinearRings(this List<List<LocalGeo.Coordinate>> coordinates)
        {
            var rings = new NetTopologySuite.Geometries.LinearRing[coordinates.Count];
            for(var i = 0; i < rings.Length; i++)
            {
                rings[i] = coordinates[i].ToLinearRing();
            }
            return rings;
        }

        /// <summary>
        /// Converts the given polygon to and NTS polygon.
        /// </summary>
        public static NetTopologySuite.Geometries.Polygon ToPolygon(this LocalGeo.Polygon polygon)
        {
            return new NetTopologySuite.Geometries.Polygon(polygon.ExteriorRing.ToLinearRing(),
                polygon.InteriorRings.ToLinearRings());
        }

        /// <summary>
        /// Converts the given polygon enumerable to a feature collection.
        /// </summary>
        public static NetTopologySuite.Features.FeatureCollection ToFeatureCollection(this IEnumerable<LocalGeo.Polygon> polygons)
        {
            var featureCollection = new NetTopologySuite.Features.FeatureCollection();
            foreach(var polygon in polygons)
            {
                featureCollection.Add(new NetTopologySuite.Features.Feature(
                    polygon.ToPolygon(), new NetTopologySuite.Features.AttributesTable()));
            }
            return featureCollection;
        }
    }
}