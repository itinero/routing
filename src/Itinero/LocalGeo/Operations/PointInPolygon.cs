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

namespace Itinero.LocalGeo.Operations
{
    /// <summary>
    /// An algorithm checking if a point lies within a polygon
    /// </summary>
    internal static class PointInPolygon
    {
        /// <summary>
        /// Returns true if the given point lies within the polygon.
        /// 
        /// Note that polygons spanning a pole, without a point at the pole itself, will fail to detect points within the polygon;
        /// (e.g. Polygon=[(lat=80°, 0), (80, 90), (80, 180)] will *not* detect the point (85, 90))
        /// </summary>
        internal static bool PointIn(this Polygon poly, Coordinate point)
        {
            // For startes, the point should lie within the outer

            var inOuter = PointIn(poly.ExteriorRing, point);
            if (!inOuter)
            {
                return false;
            }

            // and it should *not* lay within any inner ring
            for (var i = 0; i < poly.InteriorRings.Count; i++)
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
        internal static bool PointIn(List<Coordinate> ring, Coordinate point)
        {

            // Coordinate of the point. Longitude might be changed in the antemeridian-crossing case
            var longitude = point.Longitude;
            var latitude = point.Latitude;

            /*
            Some preprocessing. If the polygon crosses the antemeridian, we have some troubles and and 360 to all the negative longitudes.
            This crossing is detected by using the bounding box. If there are longitudes between [+90..-90], we know that the antemeridian is crossed
            */

            BoundingBox(ring, out var bbNorth, out var bbEast, out var bbSouth, out var bbWest);

            var antemeridianCrossed = false;
            if (Math.Sign(bbEast) != Math.Sign(bbWest))
            {
                // opposite signs: we either cross the prime meridian or antemeridian
                if (Math.Min(bbEast, bbWest) <= -90 && Math.Max(bbEast, bbWest) >= 90)
                {
                    // The lowest longitude is really low, the highest really high => We probably cross the antemeridian
                    antemeridianCrossed = true;
                    // We flip the bounding box to be entirely between [0, 360]
                    Extensions.Flip(ref bbWest);
                    Extensions.Flip(ref bbEast);
                    // this means we have to swap the east and west sides of the bounding box
                    float x = bbWest;
                    bbWest = bbEast;
                    bbEast = x;
                    // We might have to update the point as well, to this new coordinate system
                    Extensions.Flip(ref longitude);
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
                if (antemeridianCrossed)
                {
                    Extensions.Flip(ref stLong);
                    Extensions.Flip(ref endLong);
                }

                // The raycast is from west to east - thus at the same latitude level of the point
                // Thus, if the latitude of the point is not between the latitudes of the segment ends, we can skip the segment
                // Note that this fails for polygons spanning a pole (but that's okay, it's documented)
                if (!(Math.Min(stLat, endLat) < latitude &&
                        latitude < Math.Max(stLat, endLat)))
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
        internal static void BoundingBox(this Polygon polygon, out float north, out float east, out float south, out float west)
        {
            BoundingBox(polygon.ExteriorRing, out north, out east, out south, out west);
        }

        /// <summary>
        /// Calculates a bounding box for the ring.
        /// </summary>
        internal static void BoundingBox(this List<Coordinate> exteriorRing, out float north, out float east, out float south, out float west)
        {
            east = exteriorRing[0].Longitude;
            west = exteriorRing[0].Longitude;
            north = exteriorRing[0].Latitude;
            south = exteriorRing[0].Latitude;

            for (var i = 1; i < exteriorRing.Count; i++)
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
    }
}