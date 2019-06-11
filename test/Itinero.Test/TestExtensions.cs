﻿/*
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Polygon = Itinero.LocalGeo.Polygon;

namespace Itinero.Test
{
    /// <summary>
    /// Contains extension methods for tests.
    /// </summary>
    public static class TestExtensions
    {
        private static Random _random = new Random();

        /// <summary>
        /// Generates a random coordinate in the given box.
        /// </summary>
        public static Coordinate GenerateRandomIn(this Box box)
        {
            var xNext = (float) _random.NextDouble();
            var yNext = (float) _random.NextDouble();

            return new Coordinate(box.MinLat + (box.MaxLat - box.MinLat) * xNext,
                box.MinLon + (box.MaxLon - box.MinLon) * yNext);
        }

        /// <summary>
        /// Adds a test edge.
        /// </summary>
        public static uint AddTestEdge(this RouterDb routerDb, float latitude1, float longitude1,
            float latitude2, float longitude2)
        {
            var vertex1 = routerDb.Network.VertexCount;
            routerDb.Network.AddVertex(vertex1, latitude1, longitude1);
            var vertex2 = routerDb.Network.VertexCount;
            routerDb.Network.AddVertex(vertex2, latitude2, longitude2);

            return routerDb.Network.AddEdge(vertex1, vertex2,
                new Itinero.Data.Network.Edges.EdgeData()
                {
                    Distance = Coordinate.DistanceEstimateInMeter(latitude1, longitude1, latitude2, longitude2),
                    Profile = 0,
                    MetaId = 0
                });
        }

        /// <summary>
        /// Loads a set of test points.
        /// </summary>
        public static IEnumerable<Coordinate> LoadTestPoints(this Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return LoadTestPoints(streamReader.ReadToEnd());
            }
        }

        public static Polygon LoadTestPolygon(this Stream stream)
        {
            var points = new List<Coordinate>(stream.LoadTestPoints());

            if (!points.Last().Equals(points[0]))
            {
                // Polygon is not closed yet
                points.Add(points[0]);
            }
            
            var poly = new Polygon() {ExteriorRing = points};
            
            if (poly.ExteriorRing.Count <= 2)
            {
                throw new ArgumentException($"Too little elements in the exterior ring of the polygon (only {poly.ExteriorRing.Count} found)");
            }
            return poly;
        }
        

        /// <summary>
        /// Loads a test network from geojson.
        /// </summary>
        private static IEnumerable<Coordinate> LoadTestPoints(string geoJson)
        {
            var geoJsonReader = new NetTopologySuite.IO.GeoJsonReader();
            var features = geoJsonReader.Read<FeatureCollection>(geoJson);

            foreach (var feature in features.Features)
            {
                var point = feature.Geometry as Point;
                if (point == null)
                {
                    continue;
                }

                yield return new Coordinate((float) point.Coordinate.Y, (float) point.Coordinate.X);
            }
        }

        /// <summary>
        /// Loads a test file.
        /// </summary>
        /// <param name="path">A path in the format of "Itinero.Test.test_data.points.geojson"</param>
        public static Stream LoadAsStream(this string path)
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                path);
        }
        
        public static Stream LoadTestStream(this string path)
        {
            return ("Itinero.Test.test_data." + path).LoadAsStream();
        }
    }
}