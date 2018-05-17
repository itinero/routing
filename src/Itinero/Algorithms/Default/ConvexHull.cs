using System;
using System.Collections.Generic;
using Itinero.Profiles.Lua.Tree.Statements;

namespace Itinero.Algorithms.Default
{
    /// <summary>
    /// Class implementing a convex hull algorithm, based on quickhull
    /// </summary>
    public class ConvexHull
    {
        private float[] lats;
        private float[] lons;
        private int[] points;

        public ConvexHull(float[] lats, float[] lons)
        {
            this.lats = lats;
            this.lons = lons;
            points = new int[lats.Length];
            for (int i = 0; i < points.Length; i++)
            {
                points[i] = i;
            }
        }

        /// <summary>
        /// The quickhull algorithm, with extra options and params for testing
        ///
        /// Does not work well with the antemeridian
        /// </summary>
        /// <param name="lats">The latitudes of the points</param>
        /// <param name="lons">The longitutdes of the points</param>
        /// <param name="iterations">The number of iterations needed to calculate the convex hull. This parameter is only usefull for testing</param>
        /// <param name="considerPoints">Only consider these points in the quickhull algo. Used in the recursive steps</param>
        /// <returns>A list of indices. These points make up the hull</returns>
        public int quickhull_raw()
        {
            // Get the two outer most points
            CalculateMinMaxX(out var pointA, out var pointB);
            var l = lats.Length;

            // Save those points in the total beginning and end of the array
            points[pointA] = 0;
            points[0] = pointA;

            points[pointB] = l - 1;
            points[l-1] = pointB;

            var hull = new List<int>();

            var split = PartitionLeftRight(pointA, pointB, 1, l-1);
            
            var cutoff1 = FindHull(pointA, pointB, 2, split);
            var cutoff2 = FindHull(pointB, pointA, split, l - 1);

            points[cutoff1] = pointB;
            cutoff1++;

            for (var i = split; i < cutoff2; i++)
            {
                points[cutoff1] = points[i];
                cutoff1++;
            }

            return cutoff1;
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
        public int FindHull(int pointA, int pointB, int start, int stop)
        {
            if (start == stop)
            {
                // Just a single points here
                return start;
            }
            
            // Search for the furthest point on this side of the line
            var pointCInd = longestDistance(pointA, pointB, start, stop);
            var pointC = points[pointCInd];
            points[pointCInd] = points[start]; // Point C is an element of the hull -> Save it at the startpoint

            // We have a triangle. Exclude everything in the triangle (thus shorten the array)
            stop = PartitionInTriangle(pointA, pointB, pointC,start, stop);

            // Every element left of A-C will be right of B-C too
            var split = PartitionLeftRight(pointA, pointC, start, stop);
            // The only points left in our set are now:
            // Left of A-C and left of C-B
            // We can use convex hull again on these parts


            var convexHullEls = FindHull(pointA, pointC, start, split);

            var convexHulLElsR = FindHull(pointC, pointB, split, stop);

            points[start] = pointC;

            // Add the point C after the 
            var h = points[convexHullEls];
            points[convexHullEls] = pointC;
            points[pointC] = h;

            for (var i = split; i < convexHulLElsR; i++)
            {
                // Copy all the points from the right run over to the left -> Thats where all the convex hull elements are stored
                h = points[convexHullEls];
                points[convexHullEls] = points[i];
                points[i] = h;
                convexHullEls++;
            }



            return convexHullEls;

        }

        /// <summary>
        /// Calculates the point in the set which has the biggest distance to the given line
        /// </summary>
        /// <returns>The _index_ of the furhtest point</returns>
        public int longestDistance(int pointA, int pointB, int start, int stop)
        {
            var maxInd = start;
            var ax = lons[pointA];
            var ay = lats[pointA];
            var bx = lons[pointB];
            var by = lats[pointB];
            var maxDist = 0f;

            for (int i = start; i < stop; i++)
            {
                var x = lons[points[i]];
                var y = lats[points[i]];
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
        public int PartitionInTriangle(int pointA, int pointB, int pointC, int start, int stop)
        {
            var ax = lons[pointA];
            var ay = lats[pointA];
            var bx = lons[pointB];
            var by = lats[pointB];
            var cx = lons[pointC];
            var cy = lats[pointC];

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
            var last = stop - 1;

            while (i <= stop)
            {
                // We only keep outsiders... A point is on the outide if it is on the right of B->C or C->A
                // It is on the right if the position is negative
                var p = points[i];
                var x = lons[p];
                var y = lats[p];

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
                points[i] = points[last];
                points[last] = p;

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
        public int PartitionLeftRight(int pointA, int pointB, int start, int stop)
        {
            // Note that, if a point is perfectly on the line between A and B, this is not a problem - it'll be eliminated earlier

            var ax = lons[pointA];
            var ay = lats[pointA];
            var bx = lons[pointB];
            var by = lats[pointB];

            var cutOff = start;
            var last = stop - 1;
            var i = start; // current index
            while (i <= last)
            {
                var p = points[i];
                var x = lons[p];
                var y = lats[p];
                var position = (bx - ax) * (y - ay) - (by - ay) * (x - ax);
                if (position < 0)
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
                    points[i] = points[last];
                    points[last] = p;
                    last--; // mark last as being visited
                    // do not move the point to consider! (thus do not increment i)
                }
            }

            return cutOff;
        }


        /// <summary>
        /// Calculates the (index of) the northernmost and southernmost point (using latitude).
        /// </summary>
        /// <param name="minIndex">The index of the most sourthern point</param>
        /// <param name="maxIndex">The index of the most northern point</param>
        /// <returns></returns>
        public void CalculateMinMaxX(out int minIndex, out int maxIndex)
        {
            minIndex = 0;
            maxIndex = 0;

            var minVal = lats[0];
            var maxVal = lats[0];


            for (var i = 1; i < lats.Length; i++)
            {
                if (minVal > lats[i])
                {
                    minIndex = i;
                    minVal = lats[i];
                }

                if (maxVal < lats[i])
                {
                    maxIndex = i;
                    maxVal = lats[i];
                }
            }
        }
    }
}