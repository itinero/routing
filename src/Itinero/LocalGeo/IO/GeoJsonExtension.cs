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

using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.LocalGeo.IO
{
    /// <summary>
    /// Contains extension methods for the local geo objects related to the GeoJSON format.
    /// </summary>
    public static class GeoJsonExtension
    {
        /// <summary>
        /// Serializes the polygon to GeoJSON.
        /// </summary>
        public static string ToGeoJson(this Polygon polygon)
        {
            var stringWriter = new StringWriter();
            polygon.WriteGeoJson(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Serializes the polygons to GeoJSON.
        /// </summary>
        public static string ToGeoJson(this IEnumerable<Polygon> polygons)
        {
            var stringWriter = new StringWriter();
            polygons.WriteGeoJson(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the polygon as geojson.
        /// </summary>
        public static void WriteGeoJson(this IEnumerable<Polygon> polygons, TextWriter writer)
        {
            if (polygons == null) { throw new ArgumentNullException("polygons"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new Itinero.IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();
            
            foreach (var polygon in polygons)
            {
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "Feature", true, false);
                jsonWriter.WriteProperty("name", "Shape", true, false);
                jsonWriter.WritePropertyName("properties");
                jsonWriter.WriteOpen();
                jsonWriter.WriteClose();
                jsonWriter.WritePropertyName("geometry", false);

                polygon.WriteGeoJson(jsonWriter);

                jsonWriter.WriteClose(); // closes the feature.
            }

            jsonWriter.WriteArrayClose(); // closes the feature array.
            jsonWriter.WriteClose(); // closes the feature collection.
        }

        /// <summary>
        /// Writes the polygon as geojson.
        /// </summary>
        public static void WriteGeoJson(this Polygon polygon, TextWriter writer)
        {
            if (polygon == null) { throw new ArgumentNullException("polygon"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }
            if (polygon.ExteriorRing == null) { throw new ArgumentNullException("polygon.ExteriorRing"); }

            var jsonWriter = new Itinero.IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Feature", true, false);
            jsonWriter.WriteProperty("name", "Shape", true, false);
            jsonWriter.WritePropertyName("properties");
            jsonWriter.WriteOpen();
            jsonWriter.WriteClose();
            jsonWriter.WritePropertyName("geometry", false);

            polygon.WriteGeoJson(jsonWriter);
            
            jsonWriter.WriteClose(); // closes the feature.
            jsonWriter.WriteArrayClose(); // closes the feature array.
            jsonWriter.WriteClose(); // closes the feature collection.
        }

        /// <summary>
        /// Writes the polygon as geojson.
        /// </summary>
        private static void WriteGeoJson(this Polygon polygon, Itinero.IO.Json.JsonWriter jsonWriter)
        {
            if (polygon == null) { throw new ArgumentNullException("polygon"); }
            if (jsonWriter == null) { throw new ArgumentNullException("jsonWriter"); }
            if (polygon.ExteriorRing == null) { throw new ArgumentNullException("polygon.ExteriorRing"); }

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Polygon", true, false);
            jsonWriter.WritePropertyName("coordinates", false);
            jsonWriter.WriteArrayOpen();

            jsonWriter.WriteArrayOpen();
            for (var i = 0; i < polygon.ExteriorRing.Count; i++)
            {
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(polygon.ExteriorRing[i].Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(polygon.ExteriorRing[i].Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();
            }
            jsonWriter.WriteArrayClose();

            if (polygon.InteriorRings != null)
            {
                foreach(var interior in polygon.InteriorRings)
                {
                    jsonWriter.WriteArrayOpen();
                    for (var i = 0; i < interior.Count; i++)
                    {
                        jsonWriter.WriteArrayOpen();
                        jsonWriter.WriteArrayValue(interior[i].Longitude.ToInvariantString());
                        jsonWriter.WriteArrayValue(interior[i].Latitude.ToInvariantString());
                        jsonWriter.WriteArrayClose();
                    }
                    jsonWriter.WriteArrayClose();
                }
            }

            jsonWriter.WriteArrayClose(); // closes the coordinates top level.
            jsonWriter.WriteClose(); // closes the geometry.
        }

        /// <summary>
        /// Converts all the lines to geojson.
        /// </summary>
        public static string ToGeoJson(this IEnumerable<Tuple<float, float, List<Coordinate>>> lines)
        {
            var stringWriter = new StringWriter();
            lines.WriteGeoJson(stringWriter);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Writes the lines as geojson.
        /// </summary>
        private static void WriteGeoJson(this IEnumerable<Tuple<float, float, List<Coordinate>>> lines, TextWriter writer)
        {
            if (lines == null) { throw new ArgumentNullException("lines"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new Itinero.IO.Json.JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            foreach (var line in lines)
            {
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "Feature", true, false);
                jsonWriter.WriteProperty("name", "Shape", true, false);
                jsonWriter.WritePropertyName("properties");
                jsonWriter.WriteOpen();

                jsonWriter.WritePropertyName("start_weight");
                jsonWriter.WritePropertyValue(line.Item1.ToInvariantString());
                jsonWriter.WritePropertyName("end_weight");
                jsonWriter.WritePropertyValue(line.Item2.ToInvariantString());

                jsonWriter.WriteClose();
                jsonWriter.WritePropertyName("geometry", false);

                line.Item3.WriteGeoJson(jsonWriter);

                jsonWriter.WriteClose(); // closes the feature.
            }

            jsonWriter.WriteArrayClose(); // closes the feature array.
            jsonWriter.WriteClose(); // closes the feature collection.
        }

        /// <summary>
        /// Writes the line as geojson.
        /// </summary>
        private static void WriteGeoJson(this List<Coordinate> line, Itinero.IO.Json.JsonWriter jsonWriter)
        {
            if (line == null) { throw new ArgumentNullException("line"); }
            if (jsonWriter == null) { throw new ArgumentNullException("jsonWriter"); }

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "LineString", true, false);
            jsonWriter.WritePropertyName("coordinates", false);
            jsonWriter.WriteArrayOpen();
            
            for (var i = 0; i < line.Count; i++)
            {
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(line[i].Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(line[i].Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();
            }

            jsonWriter.WriteArrayClose(); // closes the coordinates top level.
            jsonWriter.WriteClose(); // closes the geometry.
        }
    }
}