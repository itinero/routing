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

using System.Collections.Generic;

namespace Itinero.Algorithms.Search.Hilbert
{
    /// <summary>
    /// Contains all hilbert curve calculations.
    /// </summary>
    public static class HilbertCurve
    {
        // DISCLAIMER: some of this stuff is straight from wikipedia:
        // http://en.wikipedia.org/wiki/Hilbert_curve#Applications_and_mapping_algorithms

        /// <summary>
        /// Calculates hilbert distance.
        /// </summary>
        public static long HilbertDistance(float latitude, float longitude, long n)
        {
            // calculate x, y.
            var x = (long)((((double)longitude + 180.0) / 360.0) * n);
            if (x >= n) { x = n - 1; }
            var y = (long)((((double)latitude + 90.0) / 180.0) * n);
            if (y >= n) { y = n - 1; }

            // calculate hilbert value for x-y and n.
            return HilbertCurve.xy2d(n, x, y);
        }

        /// <summary>
        /// Calculates all distinct hilbert distances inside of the given bounding box.
        /// </summary>
        public static List<long> HilbertDistances(float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude, long n)
        {
            var deltaLat = 180.0f / n;
            var deltaLon = 360.0f / n;
            var distances = new List<long>((int)(
                ((maxLatitude - minLatitude) / deltaLat) *
                ((maxLongitude - minLongitude) / deltaLon)));

            minLatitude = System.Math.Max(minLatitude - deltaLat, -90);
            minLongitude = System.Math.Max(minLongitude - deltaLon, -180);
            maxLatitude = System.Math.Min(maxLatitude + deltaLat, 90);
            maxLongitude = System.Math.Min(maxLongitude + deltaLon, 180);

            for (var latitude = minLatitude; latitude < maxLatitude; latitude = latitude + deltaLat)
            {
                for (var longitude = minLongitude; longitude < maxLongitude; longitude = longitude + deltaLon)
                {
                    distances.Add(HilbertCurve.HilbertDistance(latitude, longitude, n));
                }
            }
            return distances;
        }

        /// <summary>
        /// Calculates the hilbert distance.
        /// </summary>
        /// <param name="n">Size of space (height/width).</param>
        /// <param name="x">The x-coordinate.</param>
        /// <param name="y">The y-coordinate.</param>
        private static long xy2d(long n, long x, long y)
        {
            long rx, ry, s;
            long d = 0;
            for (s = n / 2; s > 0; s /= 2)
            {
                rx = (x & s) > 0 ? 1 : 0;
                ry = (y & s) > 0 ? 1 : 0;
                d += (s * s * ((3 * rx) ^ ry));
                rot(s, ref x, ref y, rx, ry);
            }
            return d;
        }

        /// <summary>
        /// Calculates x and y from hilbert distance.
        /// </summary>
        /// <param name="n">Size of space (height/width).</param>
        /// <param name="d">The hibert distance.</param>
        /// <param name="x">The output x-coordinate.</param>
        /// <param name="y">The output y-coordinate.</param>
        private static void d2xy(long n, long d, out long x, long y)
        {
            long rx, ry, s;
            long t = d;
            x = y = 0;
            for (s = 1; s < n; s *= 2)
            {
                rx = 1 & (t / 2);
                ry = 1 & (t ^ rx);
                rot(s, ref x, ref y, rx, ry);
                x += (s * rx);
                y += (s * ry);
                t /= 4;
            }
        }

        /// <summary>
        /// Rotate/flip a quadrant appropriately
        /// </summary>
        private static void rot(long n, ref long x, ref long y, long rx, long ry)
        {
            if (ry == 0)
            {
                if (rx == 1)
                {
                    x = n - 1 - x;
                    y = n - 1 - y;
                }
                
                var t = x;
                x = y;
                y = t;
            }
        }
    }
}