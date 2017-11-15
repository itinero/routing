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

            for(var i = 0; i < coordinates.Count; i++)
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
    }
}