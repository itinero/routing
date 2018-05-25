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
    internal class QuickHull
    {
        // TODO: refactor this without creating a quickhull object.
        
        private readonly float[] _lats;
        private readonly float[] _lons;

        internal QuickHull(float[] lats, float[] lons)
        {
            _lats = lats;
            _lons = lons;
            Points = new int[lats.Length];
            for (var i = 0; i < Points.Length; i++)
            {
                Points[i] = i;
            }
        }

        internal int[] Points { get; }

        /// <summary>
        /// The quickhull algorithm in a convenient coordinate format.
        /// Contains all you need
        /// </summary>
        /// <param name="coors"></param>
        /// <returns></returns>
        internal static List<Coordinate> Quickhull(List<Coordinate> coors)
        {
            var lats = new float[coors.Count];
            var lons = new float[coors.Count];

            for (var i = 0; i < coors.Count; i++)
            {
                lats[i] = coors[i].Latitude;
                lons[i] = coors[i].Longitude;
            }

            var cv = new QuickHull(lats, lons);

            var cutoff = cv.Quickhull();

            var results = new List<Coordinate>();
            for (var i = 0; i < cutoff; i++)
            {
                results.Add(coors[cv.Points[i]]);
            }
            results.Add(coors[cv.Points[0]]);

            return results;
        }

        /// <summary>
        /// Removes all given points from allPoints, and checks if any are in the hull.
        /// If so, a new hull is calculated.
        /// </summary>
        /// <param name="allPoints"></param>
        /// <param name="hull"></param>
        /// <param name="pointsToRemove"></param>
        internal static List<Coordinate> RemoveFromHull(List<Coordinate> allPoints, List<Coordinate> hull,
            List<Coordinate> pointsToRemove)
        {
            var hullBreach = false;
            foreach (var toRemove in pointsToRemove)
            {
                allPoints.Remove(toRemove);
                hullBreach = hullBreach || hull.Remove(toRemove);
            }
            // There might be points in allPoints which were not part of the hull, but will be now
            // For correctness, we have to recalculate from scratch
            return hullBreach ? Quickhull(allPoints) : hull;
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
        internal static int UpdateHull(List<Coordinate> hull, Coordinate newPoint, bool inOrder = true)
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

                hull.Insert(i+1, newPoint);
                return i+1;
            }

            return -1;
        }

        /// <summary>
        /// Reoorders the points in 'Points' so that the elements of the quickhull are in front (the hull will be in order),
        /// Returns the number of quickhull elements 
        /// </summary>
        internal int Quickhull()
        {
            // Get the two outer most points
            CalculateMinMaxX(out var pointA, out var pointB);
            var l = _lats.Length;

            // Save pointA in the total beginning and end of the array
            SwapP(pointA, 0);

            // Move pointB out of the way to the end
            SwapP(pointB, l - 1);

            // Split the resting set into a left and right partition...
            var split = PartitionLeftRight(pointA, pointB, 1, l - 1);
            // ...And calculate the hull on the left
            var cutoff1 = FindHull(pointA, pointB, 1, split);

            // Move pointB just after this hull
            SwapP(cutoff1, l - 1);
            cutoff1++;

            // split might equal cutoff1; in that case:
            split = Math.Max(cutoff1, split);

            // And calculate the second part of the hull
            var cutoff2 = FindHull(pointB, pointA, split, l);


            return MergeP(cutoff1, split, cutoff2) - 1;
        }

        /// <summary>
        /// The recursive part of the convex hull algo
        /// It starts with a line and points only on the left side of that line.
        /// 
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>The cutoff of convex hull elements</returns>
        internal int FindHull(int pointA, int pointB, int start, int stop)
        {
            if (start == stop)
            {
                // No points here: return a negative value
                return start;
            }

            if (start + 1 == stop)
            {
                // only a single point here -> part of the hull
                return start + 1;
            }

            // Search for the furthest point on this side of the line
            var pointCInd = LongestDistance(pointA, pointB, start, stop);
            var pointC = Points[pointCInd];

            // Move pointC out of the way to the end
            SwapP(stop - 1, pointCInd);

            // We have a triangle. Exclude everything in the triangle (thus shorten the array)
            // Everything between start and stop should still be considered for the convexhull
            var split = PartitionInTriangle(pointA, pointB, pointC, start, stop - 1);

            // Now we find which points are on one side, and which on another
            // Every element left of A-C will be right of B-C too
            split = PartitionLeftRight(pointA, pointC, start, split);

            // The only points left in our set are now:
            // Left of A-C and left of C-B

            // We can use quick hull again on these parts
            var convexHullEls = FindHull(pointA, pointC, start, split);

            // Swap pointC to the middle of the hulls
            SwapP(convexHullEls, stop - 1);
            convexHullEls++;

            split = Math.Max(convexHullEls, split);

            var convexHulLElsR = FindHull(pointC, pointB, split, stop); // no stop-1 here

            return MergeP(convexHullEls, split, convexHulLElsR);
        }

        /// <summary>
        /// Calculates the point in the set which has the biggest distance to the given line
        /// </summary>
        /// <returns>The _index_ of the furhtest point</returns>
        internal int LongestDistance(int pointA, int pointB, int start, int stop)
        {
            var maxInd = -1;
            var ax = _lons[pointA];
            var ay = _lats[pointA];
            var bx = _lons[pointB];
            var by = _lats[pointB];
            var maxDist = 0f;

            for (var i = start; i < stop; i++)
            {
                var x = _lons[Points[i]];
                var y = _lats[Points[i]];
                // If we needed the actual euclidian distance, we'd also have to calculate by some squarerootstuff
                // However, this is constant if point A/B stay constant, so we just drop it
                var d = Math.Abs((by - ay) * x - (bx - ax) * y + bx * ay - by * ax);
                if (maxDist < d)
                {
                    maxDist = d;
                    maxInd = i;
                }
            }

            return maxInd;
        }


        /// <summary>
        /// Will partition the points[] array so that all elements not in the triangle will be placed left in the array.
        /// Expects the given point to be on one side of the line A-B, namely the same side as C!
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="pointC"></param>
        /// <param name="start"></param>
        /// <param name="stop"></param>
        /// <returns>The new endpoint</returns>
        internal int PartitionInTriangle(int pointA, int pointB, int pointC, int start, int stop)
        {
            var ax = _lons[pointA];
            var ay = _lats[pointA];
            var bx = _lons[pointB];
            var by = _lats[pointB];
            var cx = _lons[pointC];
            var cy = _lats[pointC];

            // a and b are already tested
            // a 

            // is C on the left side or on the right side of A - B?
            // We always want it on the LEFT (positive position)
            var position = (bx - ax) * (cy - ay) - (by - ay) * (cx - ax);
            if (position < 0)
            {
                // seems like C is on the right. Swap direction of A and B...
                return PartitionInTriangle(pointB, pointA, pointC, start, stop);
            }


            var cutOff = start;
            var i = start;
            var last = stop - 1; // Last index = stop - 1;

            while (i <= last)
            {
                // We only keep outsiders... A point is on the outide if it is on the right of B->C or C->A
                // It is on the right if the position is negative
                var p = Points[i];
                var x = _lons[p];
                var y = _lats[p];

                position = (cx - bx) * (y - by) - (cy - by) * (x - bx);
                if (position < 0)
                {
                    // seems like C is on the right. This is an outsider
                    // the point can stay where it is, but we increase the index and cutoffs
                    i++;
                    cutOff++;
                    continue;
                }

                position = (ax - cx) * (y - cy) - (ay - cy) * (x - cx);
                if (position < 0)
                {
                    // seems like C is on the right. This is an outsider
                    // the point can stay where it is, but we increase the index and cutoffs
                    i++;
                    cutOff++;
                    continue;
                }

                // this point is an insider, we'll wont need it anymore
                // move it to the back
                Points[i] = Points[last];
                Points[last] = p;

                last--;
                // No increase of the counter!
            }

            return cutOff;
        }

        /// <summary>
        /// Shuffles the array "points[0..lengthToConsider]" in such a way that all points in the start are on the left of the line
        /// and all visits in the end are on the right. The cutoff point between the two is given by the return value
        ///
        /// points[start..cutOff[ are on the left, points[cutoff..lengthToConsider[ are on the right
        /// </summary>
        /// <param name="pointA"></param>
        /// <param name="pointB"></param>
        /// <param name="start">The startindex in the list to start searching</param>
        /// <param name="stop">The endindex of points to search</param>
        /// <returns>The border between the left and right partitions</returns>
        internal int PartitionLeftRight(int pointA, int pointB, int start, int stop)
        {
            // Note that, if a point is perfectly on the line between A and B, this is not a problem - it'll be eliminated earlier

            var ax = _lons[pointA];
            var ay = _lats[pointA];
            var bx = _lons[pointB];
            var by = _lats[pointB];

            var cutOff = start;
            var last = stop - 1;
            var i = start; // current index
            while (i <= last)
            {
                var p = Points[i];
                var x = _lons[p];
                var y = _lats[p];
                var position = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
                if (position > 0)
                {
                    // on the left
                    // we leave the point where it is, but move the cutoff and the current point to consider
                    cutOff++;
                    i++;
                }
                else
                {
                    // on the right
                    // we swap the point with the rightmost location (and won't visit that point anymore)
                    SwapP(i, last);
                    last--; // mark last as being visited
                    // do not move the point to consider! (thus do not increment i)
                }
            }

            return cutOff;
        }

        /// <summary>
        /// Returns true if (x, y) lies on the left of the line from A to B
        /// </summary>
        /// <param name="ax"></param>
        /// <param name="ay"></param>
        /// <param name="bx"></param>
        /// <param name="by"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private static bool LeftOfLine(float ax, float ay, float bx, float by, float x, float y)
        {
            var position = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
            return position > 0;
        }

        private static bool LeftOfLine(Coordinate a, Coordinate b, Coordinate x)
        {
            return LeftOfLine(a.Longitude, a.Latitude, b.Longitude, b.Latitude, x.Longitude, x.Latitude);
        }


        /// <summary>
        /// Calculates the (index of) the northernmost and southernmost point (using latitude).
        /// </summary>
        /// <param name="minIndex">The index of the most sourthern point</param>
        /// <param name="maxIndex">The index of the most northern point</param>
        /// <returns></returns>
        internal void CalculateMinMaxX(out int minIndex, out int maxIndex)
        {
            minIndex = 0;
            maxIndex = 0;

            var minVal = _lats[0];
            var maxVal = _lats[0];


            for (var i = 1; i < _lats.Length; i++)
            {
                if (minVal > _lats[i])
                {
                    minIndex = i;
                    minVal = _lats[i];
                }

                if (maxVal < _lats[i])
                {
                    maxIndex = i;
                    maxVal = _lats[i];
                }
            }
        }

        /// <summary>
        /// Swaps the contents of [copyStart..copyStop[ just behind 'appendAfter'
        /// </summary>
        /// <param name="array">The array that where the copying takes place</param>
        /// <param name="appendAfter">The index where swapping will start</param>
        /// <param name="copyStart">Where to start the swapping</param>
        /// <param name="copyStop">Where to stop swapping</param>
        /// <returns>The index (+1) of the last changed element. Should equal (appendAfter + copyStop - copyStart)</returns>
        internal static int Merge(int[] array, int appendAfter, int copyStart, int copyStop)
        {
            for (var i = copyStart; i < copyStop; i++)
            {
                var h = array[appendAfter];
                array[appendAfter] = array[i];
                array[i] = h;
                appendAfter++;
            }

            return appendAfter;
        }

        private int MergeP(int appendAfter, int copyStart, int copyStop)
        {
            return Merge(Points, appendAfter, copyStart, copyStop);
        }

        // TODO: isn't there a generic .NET method for this?
        internal static void Swap(int[] array, int a, int b)
        {
            var h = array[a];
            array[a] = array[b];
            array[b] = h;
        }

        private void SwapP(int a, int b)
        {
            Swap(Points, a, b);
        }
    }
}