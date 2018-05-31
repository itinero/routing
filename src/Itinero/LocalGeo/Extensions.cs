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
using System.Collections.Generic;
using System.Linq;
using Itinero.LocalGeo.Operations;

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Extension methods for the local geo objects.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the coordinates to a double double array.
        /// </summary>
        public static double[][] ToLonLatArray(this List<Coordinate> coordinates)
        {
            var array = new double[coordinates.Count][];

            for (var i = 0; i < coordinates.Count; i++)
            {
                array[i] = new double[]
                {
                    coordinates[i].Longitude,
                    coordinates[i].Latitude
                };
            }

            return array;
        }

        /// <summary>
        /// Simplify the specified points using epsilon.
        /// </summary>
        /// <param name="points">Points.</param>
        /// <param name="epsilonInMeter">Epsilon.</param>
        public static Coordinate[] Simplify(this Coordinate[] points, float epsilonInMeter)
        {
            return SimplifyBetween(points, epsilonInMeter, 0, points.Length - 1);
        }

        /// <summary>
        /// Simplifies the specified points using episilon.
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="epsilonInMeter"></param>
        /// <returns></returns>
        public static List<Coordinate> Simplify(this List<Coordinate> shape, float epsilonInMeter = .1f)
        {
            if (epsilonInMeter == 0)
            {
                return shape;
            }

            if (shape != null && shape.Count > 2)
            {
                var simplified = SimplifyBetween(shape, epsilonInMeter, 0, shape.Count - 1);
                if (simplified.Count != shape.Count)
                {
                    return simplified;
                }
            }
            return shape;
        }

        /// <summary>
        /// Simplify the specified points using epsilon.
        /// </summary>
        /// <param name="points">Points.</param>
        /// <param name="epsilonInMeter">Epsilon.</param>
        /// <param name="first">First.</param>
        /// <param name="last">Last.</param>
        public static List<Coordinate> SimplifyBetween(this List<Coordinate> points, float epsilonInMeter, int first, int last)
        {
            if (points == null)
                throw new ArgumentNullException(nameof(points));
            if (epsilonInMeter <= 0)
                throw new ArgumentOutOfRangeException(nameof(points));
            if (first > last)
                throw new ArgumentException(string.Format("first[{0}] must be smaller or equal than last[{1}]!",
                    first, last));

            if (first + 1 != last)
            {
                // find point with the maximum distance.
                float maxDistance = 0;
                int foundIndex = -1;

                // create the line between first-last.
                var line = new Line(points[first], points[last]);
                for (int idx = first + 1; idx < last; idx++)
                {
                    var distance = line.DistanceInMeter(points[idx]);
                    if (distance.HasValue && distance.Value > maxDistance)
                    {
                        // larger distance found.
                        maxDistance = distance.Value;
                        foundIndex = idx;
                    }
                }

                if (foundIndex > 0 && maxDistance > epsilonInMeter)
                { // a point was found and it is far enough.
                    var before = SimplifyBetween(points, epsilonInMeter, first, foundIndex);
                    var after = SimplifyBetween(points, epsilonInMeter, foundIndex, last);

                    // build result.
                    var result = new List<Coordinate>(before.Count + after.Count - 1);
                    for (int idx = 0; idx < before.Count - 1; idx++)
                    {
                        result.Add(before[idx]);
                    }
                    for (int idx = 0; idx < after.Count; idx++)
                    {
                        result.Add(after[idx]);
                    }
                    return result;
                }
            }
            return new List<Coordinate>(new Coordinate[] { points[first], points[last] });
        }

        /// <summary>
        /// Simplify the specified points using epsilon.
        /// </summary>
        /// <param name="points">Points.</param>
        /// <param name="epsilonInMeter">Epsilon.</param>
        /// <param name="first">First.</param>
        /// <param name="last">Last.</param>
        public static Coordinate[] SimplifyBetween(this Coordinate[] points, float epsilonInMeter, int first, int last)
        {
            if (points == null)
                throw new ArgumentNullException("points");
            if (epsilonInMeter <= 0)
                throw new ArgumentOutOfRangeException("epsilon");
            if (first > last)
                throw new ArgumentException(string.Format("first[{0}] must be smaller or equal than last[{1}]!",
                    first, last));

            if (first + 1 != last)
            {
                // find point with the maximum distance.
                float maxDistance = 0;
                int foundIndex = -1;

                // create the line between first-last.
                var line = new Line(points[first], points[last]);
                for (int idx = first + 1; idx < last; idx++)
                {
                    var distance = line.DistanceInMeter(points[idx]);
                    if (distance.HasValue && distance.Value > maxDistance)
                    {
                        // larger distance found.
                        maxDistance = distance.Value;
                        foundIndex = idx;
                    }
                }

                if (foundIndex > 0 && maxDistance > epsilonInMeter)
                { // a point was found and it is far enough.
                    var before = SimplifyBetween(points, epsilonInMeter, first, foundIndex);
                    var after = SimplifyBetween(points, epsilonInMeter, foundIndex, last);

                    // build result.
                    var result = new Coordinate[before.Length + after.Length - 1];
                    for (int idx = 0; idx < before.Length - 1; idx++)
                    {
                        result[idx] = before[idx];
                    }
                    for (int idx = 0; idx < after.Length; idx++)
                    {
                        result[idx + before.Length - 1] = after[idx];
                    }
                    return result;
                }
            }
            return new Coordinate[] { points[first], points[last] };
        }

        /// <summary>
        /// Converts the box to a polygon.
        /// </summary>
        public static Polygon ToPolygon(this Box box)
        {
            return new Polygon()
            {
                ExteriorRing = new List<Coordinate>(new Coordinate[]
                {
                    new Coordinate(box.MinLat, box.MinLon),
                    new Coordinate(box.MaxLat, box.MinLon),
                    new Coordinate(box.MaxLat, box.MaxLon),
                    new Coordinate(box.MinLat, box.MaxLon),
                    new Coordinate(box.MinLat, box.MinLon)
                    })
            };
        }

        /// <summary>
        /// Calculates a bounding box for the polygon.
        /// TODO: if this is needed a lot, we should cache it in the polygon, see https://github.com/itinero/routing/issues/138
        /// </summary>
        public static void BoundingBox(this Polygon polygon, out float north, out float east, out float south, out float west)
        {
            polygon.ExteriorRing.BoundingBox(out north, out east, out south, out west);
        }

        /// <summary>
        /// Calculates a bounding box for the ring.
        /// </summary>
        public static void BoundingBox(this List<Coordinate> exteriorRing, out float north, out float east, out float south, out float west)
        {
            PointInPolygon.BoundingBox(exteriorRing, out north, out east, out south, out west);
        }

        /// <summary>
        /// If the longitude is smaller then 0, add 360.
        /// Usefull when coordinates are near the antemeridian
        /// </summary>
        /// <param name="longitude">The paramater that is flipped</param>
        public static void Flip(ref float longitude)
        {
            if (longitude < 0)
            {
                longitude += 360;
            }
        }

        /// <summary>
        /// Returns the location along the line at the given offset distance in meter.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="offset">The offset in meter.</param>
        /// <param name="forward">When true the offset starts at the first location, otherwise the second..</param>
        /// <returns></returns>
        public static Coordinate LocationAfterDistance(this Line line, float offset, bool forward = true)
        {
            if (forward)
            {
                return LocationAfterDistance(line.Coordinate1, line.Coordinate2, offset);
            }
            return LocationAfterDistance(line.Coordinate2, line.Coordinate1, offset);
        }

        /// <summary>
        /// Returns the location between the two coordinates at the given offset distance in meter.
        /// </summary>
        /// <param name="coordinate1">The first coordinate.</param>
        /// <param name="coordinate2">The second coordinate.</param>
        /// <param name="offset">The offset in meter starting at coordinate1.</param>
        /// <returns></returns>
        public static Coordinate LocationAfterDistance(Coordinate coordinate1, Coordinate coordinate2, float offset)
        {
            return LocationAfterDistance(coordinate1, coordinate2, Coordinate.DistanceEstimateInMeter(coordinate1, coordinate2),
                offset);
        }

        /// <summary>
        /// Returns the location between the two coordinates at the given offset distance in meter.
        /// </summary>
        /// <param name="coordinate1">The first coordinate.</param>
        /// <param name="coordinate2">The second coordinate.</param>
        /// <param name="distanceBetween">The distance between the two, when we already calculated it before.</param>
        /// <param name="offset">The offset in meter starting at coordinate1.</param>
        /// <returns></returns>
        public static Coordinate LocationAfterDistance(Coordinate coordinate1, Coordinate coordinate2, float distanceBetween, float offset)
        {
            var ratio = offset / distanceBetween;
            return new Coordinate(
                (coordinate2.Latitude - coordinate1.Latitude) * ratio + coordinate1.Latitude,
                (coordinate2.Longitude - coordinate1.Longitude) * ratio + coordinate1.Longitude
            );
        }

        /// <summary>
        /// Returns intersection points with the given polygon.
        /// </summary>
        /// <param name="polygon">The polygon</param>
        /// <param name="latitude1"></param>
        /// <param name="longitude1"></param>
        /// <param name="latitude2"></param>
        /// <param name="longitude2"></param>
        /// <returns></returns>
        public static IEnumerable<Coordinate> Intersect(this Polygon polygon, float latitude1, float longitude1,
            float latitude2, float longitude2)
        {
            return Intersections.Intersect(polygon, latitude1, longitude1, latitude2, longitude2);
        }

        /// <summary>
        /// Returns true if the given point lies within the polygon.
        /// 
        /// Note that polygons spanning a pole, without a point at the pole itself, will fail to detect points within the polygon;
        /// (e.g. Polygon=[(lat=80°, 0), (80, 90), (80, 180)] will *not* detect the point (85, 90))
        /// </summary>
        public static bool PointIn(this Polygon poly, Coordinate point)
        {
            return PointInPolygon.PointIn(poly, point);
        }

        /// <summary>
        /// Returns true if the given point lies within the ring.
        /// </summary>
        public static bool PointIn(List<Coordinate> ring, Coordinate point)
        {
            return PointInPolygon.PointIn(ring, point);
        }

        /// <summary>
        /// Calculates the convex hull around the given points.
        /// </summary>
        /// <param name="points">The points.</param>
        /// <returns>A convex hull ring around the given points.</returns>
        public static Polygon ConvexHull(this IEnumerable<Coordinate> points)
        {
            return new Polygon()
            {
                ExteriorRing = points.Convexhull()
            };
        }

        /// <summary>
        /// Calculates the area of the polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <returns></returns>
        public static float SurfaceArea(this Polygon poly)
        {
            var internalArea = 0f;
            foreach (var ring in poly.InteriorRings)
            {
                internalArea += ring.SurfaceArea();
            }

            return poly.ExteriorRing.SurfaceArea() - internalArea;
        }
    }
}