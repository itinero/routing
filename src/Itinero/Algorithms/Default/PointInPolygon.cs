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

using Itinero.LocalGeo;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default
{

    /// <summary>
    /// An algorithm checking if a point lies within a polygon
    /// </summary>
    public static class PointInPolygon
    {


        public static bool PointLiesWithin(this Polygon poly, Coordinate point){
            // For startes, the point should lie within the outer

            var inOuter = PointLiesWithin(poly.ExteriorRing, point);
            if(!inOuter){
                return false;
            }

            // and it should *not* lay within any inner ring
            for(var i = 0; i < poly.InteriorRings.Count; i++){
                var inInner = PointLiesWithin(poly.InteriorRings[i], point);
                if(inInner){
                    return false;
                }
            }
            return true;
        }

        /* The basic, actual algorithm
        The algorithm is based on the ray casting algorthm, where the point moves horizontally
        If an even number of intersections are counted, the point lies outside of the polygon
        */
        public static bool PointLiesWithin(List<Coordinate> polygon, Coordinate point)
        {
            // no intersections passed yet -> not within the polygon
            var result = false;


            for (var i = 0; i < polygon.Count; i++)
            {
                var start = polygon[i];
                var end = polygon[(i + 1) % polygon.Count];

                // The raycast is from west to east - thus at the same latitude level of the point
                // Thus, if the longitude is not between the longitude of the segments, we skip the segment
                // Note that this fails for polygons spanning a pole (e.g. every latitude is 80°, around the world, but the point is at lat. 85°)
                if (!(Math.Min(start.Latitude, end.Latitude) < point.Latitude
                        && point.Latitude < Math.Max(start.Latitude, end.Latitude)))
                {
                    continue;
                }

                // Here, we know that: the latitude of the point falls between the latitudes of the end points of the segment


                // If both ends of the segment fall to the right, the line will intersect: we toggle our switch and continue with the next segment
                if (Math.Min(start.Longitude, end.Longitude) >= point.Longitude)
                {
                    result = !result;
                    continue;
                }

                // Analogously, at least one point of the segments should be on the right (east) of the point;
                // otherwise, no intersection is possible (as the raycast goes right)
                if (!(Math.Max(start.Longitude, end.Longitude) >= point.Longitude))
                {
                    continue;
                }

                // we calculate the longitude on the segment for the latitude of the point
                // x = y_p * (x1 - x2)/(y1 - y2) + (x2y1-x1y1)/(y1-y2)
                var longit = point.Latitude * (start.Longitude - end.Longitude) + //
                                (end.Longitude * start.Latitude - start.Longitude * end.Latitude);
                longit /= (start.Latitude - end.Latitude);

                // If the longitude lays on the right of the point AND lays within the segment (only right bound is needed to check)
                // the segment intersects the raycast and we flip the bit
                if(longit >= point.Longitude && longit <= Math.Max(start.Longitude, end.Longitude)){
                    result = !result;
                }

            }



            return result;
        }

    }

}