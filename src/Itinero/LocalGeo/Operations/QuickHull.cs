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
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Itinero.Test")]

namespace Itinero.LocalGeo.Operations
{
    /// <summary>
    /// Implementation of the quickhull algorithm.
    /// </summary>
    internal static class QuickHull
    {
        /// <summary>
        /// Removes all given points from allPoints, and checks if any are in the hull.
        /// If so, a new hull is calculated.
        /// </summary>
        /// <param name="allPoints"></param>
        /// <param name="hull"></param>
        /// <param name="pointsToRemove"></param>
        internal static List<Coordinate> RemoveFromHull(HashSet<Coordinate> allPoints, List<Coordinate> hull,
            IEnumerable<Coordinate> pointsToRemove)
        {
            var hullBreach = false;
            foreach (var toRemove in pointsToRemove)
            {
                allPoints.Remove(toRemove);
                hullBreach = hullBreach || hull.Remove(toRemove);
            }

            // There might be points in allPoints which were not part of the hull, but will be now
            // For correctness, we have to recalculate from scratch
            return hullBreach ? allPoints.Quickhull() : hull;
        }

        /// <summary>
        /// Checks if newPoint is contained within the hull.
        /// If not, the element is added to the hull-list, at the appropriate position to maintain a with-the-clock order.
        /// (If inOrder is disabled, the point is just appended as last element)
        /// The point of insertion is returned
        /// </summary>
        /// <param name="hull"></param>
        /// <param name="newPoint"></param>
        /// <param name="inOrder"></param>
        /// <returns></returns>
        internal static int UpdateHull(this List<Coordinate> hull, Coordinate newPoint, bool inOrder = true)
        {
            if (PointInPolygon.PointIn(
                hull, newPoint))
            {
                // Point is neatly contained within the polygon; nothing to do here
                return -1;
            }

            if (hull.Contains(newPoint))
            {
                return -1;
            }


            if (!inOrder)
            {
                // No need to figure out where exactly the point should be. Just slap it in the end
                hull.Add(newPoint);
                return hull.Count - 1;
            }


            var i = 0;
            while (i < hull.Count)
            {
                // When the newPoint is on the LEFT of hull[i] -> hull[i+1]; it falls outside of the hull and should be added there.
                // If the point is still left of hull[i+1] -> hull[i+2], it means that hull[i+1] is now absorbed into the hull and can be dropped

                var nxt = (i + 1) % hull.Count;

                // Neatly on the right, as it is supposed to be.. Continue to the next point
                if (!LeftOfLine(hull[i], hull[nxt], newPoint))
                {
                    i++;
                    continue;
                }


                // Point is on the left of hull[i] - hull[i+1]. It should be inserted!
                // There is a catch though: the current point (i) might be shadowed now
                // We detect this by checking with the previouis point (i-1)
                // If the new point is on the left of that point as well, we can simply remove point i

                var prev = (hull.Count + i - 1) % hull.Count;
                if (LeftOfLine(hull[prev], hull[i], newPoint))
                {
                    // Current point i is shadowed
                    // Remove it and continue at the current position, as if pointI never existed
                    hull.RemoveAt(i);
                    continue;
                }

                // When added, it might shadow the next points as well...
                var nxtnxt = (nxt + 1) % hull.Count;
                while (LeftOfLine(hull[nxt], hull[nxtnxt], newPoint))
                {
                    hull.RemoveAt(nxt);
                    nxt %= hull.Count;
                    nxtnxt %= hull.Count;
                }

                hull.Insert(i + 1, newPoint);
                return i + 1;
            }

            return -1;
        }

        internal static List<Coordinate> Convexhull(this IEnumerable<Coordinate> points)
        {
            return new HashSet<Coordinate>(points).Quickhull();
        }

        /// <summary>
        /// Calculates the convex hull of the given points. The hull will be closed.
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        internal static List<Coordinate> Quickhull(this HashSet<Coordinate> points)
        {
            var hull = new List<Coordinate>();
            // Get the two outer most points
            points.CalculateMinMaxX(out var aNullable, out var bNullable);
            if (aNullable == null || bNullable == null)
            {
                throw new InvalidOperationException("A hull should at least contain three elements");
            }

            var a = (Coordinate) aNullable;
            var b = (Coordinate) bNullable;

            points.Remove(a);
            points.Remove(b);
            
            // Split the resting set into a left and right partition...
            points.PartitionLeftRight(a, b, out var leftSet, out var rightSet);

            hull.Add(a);
            FindHull(leftSet, hull, a, b);
            hull.Add(b);
            FindHull(rightSet, hull, b, a);
            hull.Add(a); // Close the loop

            return hull;
        }

        /// <summary>
        /// Finds the hull for a part of the points
        /// </summary>
        internal static void FindHull(this HashSet<Coordinate> points, List<Coordinate> hull, Coordinate a,
            Coordinate b)
        {
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (points.Count == 0)
            {
                return;
            }

            if (points.Count == 1)
            {
                foreach (var p in points)
                {
                    hull.Add(p);
                }

                return;
            }


            // Search for the furthest point on this side of the line
            var cNullable = LongestDistance(points, a, b);
            if (cNullable == null)
            {
                throw new InvalidOperationException("Not enough elements in longest distance, should not happen");
            }

            var c = (Coordinate) cNullable;
            points.Remove(c);

            // We have a triangle. Exclude everything in the triangle
            points = RemoveInTriangle(points, a, b, c);

            // Now we find which points are on one side, and which on another
            // Every element left of A-C will be right of B-C too
            PartitionLeftRight(points, a, c, out var leftSet, out var rightSet);


            // We can use quick hull again on these parts

            FindHull(leftSet, hull, a, c);
            hull.Add(c);
            FindHull(rightSet, hull, c, b);
        }

        /// <summary>
        /// Calculates the point in the set which has the biggest distance to the given line
        /// </summary>
        /// <returns>The _index_ of the furhtest point</returns>
        internal static Coordinate? LongestDistance(this HashSet<Coordinate> coors, Coordinate a, Coordinate b)
        {
            var ax = a.Longitude;
            var ay = a.Latitude;
            var bx = b.Longitude;
            var by = b.Latitude;
            var maxDist = float.MinValue;
            Coordinate? max = null;


            foreach (var p in coors)
            {
                var x = p.Longitude;
                var y = p.Latitude;

                // If we needed the actual euclidian distance, we'd also have to calculate by some squarerootstuff
                // However, this is constant if point A/B stay constant, so we just drop it
                var d = Math.Abs((by - ay) * x - (bx - ax) * y + bx * ay - by * ax);
                if (maxDist < d)
                {
                    maxDist = d;
                    max = p;
                }
            }

            return max;
        }


        /// <summary>
        /// Gives a new hashset containing all elements not within the triange a - b - c.
        /// </summary>
        /// <param name="points"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        internal static HashSet<Coordinate> RemoveInTriangle(this HashSet<Coordinate> points, Coordinate a,
            Coordinate b,
            Coordinate c)
        {
            if (!LeftOfLine(a, b, c))
            {
                // Corner case
                return points.RemoveInTriangle(b, a, c);
            }


            var outsiders = new HashSet<Coordinate>();
            foreach (var p in points)
            {
                if (!(LeftOfLine(a, b, p) && LeftOfLine(b, c, p) && LeftOfLine(c, a, p)))
                {
                    // Left of all the lines: element of the triangle -> we remove it
                    outsiders.Add(p);
                }
            }

            return outsiders;
        }

        /// <summary>
        /// Partitions the points in two parts, one set left of the line; one right off the line-
        /// </summary>
        internal static void PartitionLeftRight(this HashSet<Coordinate> points, Coordinate a, Coordinate b,
            out HashSet<Coordinate> pointsLeft, out HashSet<Coordinate> pointsRight)
        {
            // Note that, if a point is perfectly on the line between A and B, this is not a problem - it'll be eliminated earlier

            pointsLeft = new HashSet<Coordinate>();
            pointsRight = new HashSet<Coordinate>();
            foreach (var p in points)
            {
                if (LeftOfLine(a, b, p))
                {
                    pointsLeft.Add(p);
                }
                else
                {
                    pointsRight.Add(p);
                }
            }
        }

        /// <summary>
        /// Returns true if 'p' lies on the left of the line from A to B
        /// </summary>
        /// <returns>If the point lies on the left</returns>
        private static bool LeftOfLine(Coordinate a, Coordinate b, Coordinate p)
        {
            var ax = a.Longitude;
            var ay = a.Latitude;

            var bx = b.Longitude;
            var by = b.Latitude;

            var x = p.Longitude;
            var y = p.Latitude;
            var position = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
            return position > 0;
        }

        /// <summary>
        /// Calculates the (index of) the northernmost and southernmost point (using latitude).
        /// </summary>
        internal static void CalculateMinMaxX(this IEnumerable<Coordinate> coors, out Coordinate? minPoint,
            out Coordinate? maxPoint)
        {
            var minVal = float.MaxValue;
            var maxVal = float.MinValue;

            minPoint = null;
            maxPoint = null;

            foreach (var coor in coors)
            {
                if (minVal > coor.Latitude)
                {
                    minPoint = coor;
                    minVal = coor.Latitude;
                }

                // ReSharper disable once InvertIf
                if (maxVal < coor.Latitude)
                {
                    maxPoint = coor;
                    maxVal = coor.Latitude;
                }
            }
        }
    }
}