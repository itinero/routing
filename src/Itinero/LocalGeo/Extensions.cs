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
        /// <param name="epsilon">Epsilon.</param>
        public static Coordinate[] Simplify(this Coordinate[] points, float epsilonInMeter)
        {
            return SimplifyBetween(points, epsilonInMeter, 0, points.Length - 1);
        }

        /// <summary>
        /// Simplifies the specified points using episilon.
        /// </summary>
        /// <param name="points"></param>
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
        /// <param name="epsilon">Epsilon.</param>
        /// <param name="first">First.</param>
        /// <param name="last">Last.</param>
        public static List<Coordinate> SimplifyBetween(this List<Coordinate> points, float epsilonInMeter, int first, int last)
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
        /// <param name="epsilon">Epsilon.</param>
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
        /// Returns true if the given point lies within the polygon.
        /// 
        /// Note that polygons spanning a pole, without a point at the pole itself, will fail to detect points within the polygon;
        /// (e.g. Polygon=[(lat=80°, 0), (80, 90), (80, 180)] will *not* detect the point (85, 90))
        /// </summary>
        public static bool PointIn(this Polygon poly, Coordinate point)
        {
            // For startes, the point should lie within the outer

            var inOuter = PointIn(poly.ExteriorRing, point);
            if (!inOuter)
            {
                return false;
            }

            // and it should *not* lay within any inner ring
            for (int i = 0; i < poly.InteriorRings.Count; i++)
            {
                var inInner = PointIn(poly.InteriorRings[i], point);
                if (inInner)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given point lies within the ring.
        /// </summary>
        public static bool PointIn(List<Coordinate> ring, Coordinate point)
        {

            // Coordinate of the point. Longitude might be changed in the antemeridian-crossing case
            var longitude = point.Longitude;
            var latitude = point.Latitude;

            /*
            Some preprocessing. If the polygon crosses the antemeridian, we have some troubles and and 360 to all the negative longitudes.
            This crossing is detected by using the bounding box. If there are longitudes between [+90..-90], we know that the antemeridian is crossed
            */


            float bbNorth;
            float bbEast;
            float bbSouth;
            float bbWest;

            BoundingBox(ring, out bbNorth, out bbEast, out bbSouth, out bbWest);

            var antemeridianCrossed = false;
            if (Math.Sign(bbEast) != Math.Sign(bbWest))
            {
                // opposite signs: we either cross the prime meridian or antemeridian
                if (Math.Min(bbEast, bbWest) <= -90 && Math.Max(bbEast, bbWest) >= 90)
                {
                    // The lowest longitude is really low, the highest really high => We probably cross the antemeridian
                    antemeridianCrossed = true;
                    // We flip the bounding box to be entirely between [0, 360]
                    Flip(ref bbWest);
                    Flip(ref bbEast);
                    // this means we have to swap the east and west sides of the bounding box
                    float x = bbWest;
                    bbWest = bbEast;
                    bbEast = x;
                    // We might have to update the point as well, to this new coordinate system
                    Flip(ref longitude);
                }
            }



            // As we now have a neat bounding box laying around, it would be a pity not to use this
            // If the point is not within the BB, it certainly is not within the polygon
            if (!(bbWest <= longitude && bbNorth >= latitude && bbEast >= longitude && bbSouth <= latitude))
            {
                return false;
            }






            /* The basic, actual algorithm
            The algorithm is based on the ray casting algorthm, where the point moves horizontally
            If an even number of intersections are counted, the point lies outside of the polygon
            */

            // no intersections passed yet -> not within the polygon
            var result = false;


            for (int i = 0; i < ring.Count; i++)
            {
                var start = ring[i];
                var end = ring[(i + 1) % ring.Count];

                // Again, longitudes might be changed in the ante-meridian-crossing case
                var stLat = start.Latitude;
                var stLong = start.Longitude;
                var endLat = end.Latitude;
                var endLong = end.Longitude;
                if(antemeridianCrossed){
                    Flip(ref stLong);
                    Flip(ref endLong);
                }

                // The raycast is from west to east - thus at the same latitude level of the point
                // Thus, if the latitude of the point is not between the latitudes of the segment ends, we can skip the segment
                // Note that this fails for polygons spanning a pole (but that's okay, it's documented)
                if (!(Math.Min(stLat, endLat) < latitude
                        && latitude < Math.Max(stLat, endLat)))
                {
                    continue;
                }

                // Here, we know that: the latitude of the point falls between the latitudes of the end points of the segment


                // If both ends of the segment fall to the right, the line will intersect: we toggle our switch and continue with the next segment
                // The following code however misses the case that the segment crosses the ante-primemeridian (longitude=180=-180)
                // We take care of that in the next if
                if (Math.Min(stLong, endLong) >= longitude)
                {
                    result = !result;
                    continue;
                }

                // Analogously, at least one point of the segments should be on the right (east) of the point;
                // otherwise, no intersection is possible (as the raycast goes right)
                if (!(Math.Max(stLong, endLong) >= longitude))
                {
                    continue;
                }

                // we calculate the longitude on the segment for the latitude of the point
                // x = y_p * (x1 - x2)/(y1 - y2) + (x2y1-x1y1)/(y1-y2)
                var longit = latitude * (stLong - endLong) + //
                                (endLong * stLat - stLong * endLat);
                longit /= (stLat - endLat);

                // If the longitude lays on the right of the point AND lays within the segment (only right bound is needed to check)
                // the segment intersects the raycast and we flip the bit
                if (longit >= longitude && longit <= Math.Max(stLong, endLong))
                {
                    result = !result;
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates a bounding box for the polygon.
        /// TODO: if this is needed a lot, we should cache it in the polygon, see https://github.com/itinero/routing/issues/138
        /// </summary>
        public static void BoundingBox(this Polygon polygon, out float north, out float east, out float south, out float west)
        {
            BoundingBox(polygon.ExteriorRing, out north, out east, out south, out west);
        }


        /// <summary>
        /// Calculates a bounding box for the ring.
        /// </summary>
        public static void BoundingBox(List<Coordinate> exteriorRing, out float north, out float east, out float south, out float west)
        {
            east = exteriorRing[0].Longitude;
            west = exteriorRing[0].Longitude;
            north = exteriorRing[0].Latitude;
            south = exteriorRing[0].Latitude;

            for (int i = 1; i < exteriorRing.Count; i++)
            {
                var c = exteriorRing[i];
                if (c.Longitude < west)
                {
                    west = c.Longitude;
                }

                if (c.Longitude > east)
                {
                    east = c.Longitude;
                }

                if (c.Latitude < south)
                {
                    south = c.Latitude;
                }

                if (c.Latitude > north)
                {
                    north = c.Latitude;
                }
            }
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
    }
}