// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
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
    }
}