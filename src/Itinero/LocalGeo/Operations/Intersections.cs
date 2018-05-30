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

namespace Itinero.LocalGeo.Operations
{
    /// <summary>
    /// Contains intersection operations.
    /// </summary>
    internal static class Intersections
    {
        /// <summary>
        /// Returns intersection points with the given polygon.
        /// </summary>
        /// <param name="polygon">The polygon</param>
        /// <param name="latitude1"></param>
        /// <param name="longitude1"></param>
        /// <param name="latitude2"></param>
        /// <param name="longitude2"></param>
        /// <returns></returns>
        internal static IEnumerable<Coordinate> Intersect(this Polygon polygon, float latitude1, float longitude1,
            float latitude2, float longitude2)
        {
            var E = 0.001f; // this is 1mm.

            var line = new Line(new Coordinate(latitude1, longitude1), new Coordinate(latitude2, longitude2));

            // REMARK: yes, this can be way way faster but it's only used in preprocessing.
            // use a real geo library like NTS if you want this faster or submit a pull request.
            var sortedList = new SortedList<float, Coordinate>();
            var intersections = polygon.ExteriorRing.IntersectInternal(line);
            foreach (var intersection in intersections)
            {
                sortedList.Add(intersection.Key, intersection.Value);
            }

            if (polygon.InteriorRings != null)
            {
                foreach (var innerRing in polygon.InteriorRings)
                {
                    intersections = innerRing.IntersectInternal(line);
                    foreach (var intersection in intersections)
                    {
                        sortedList.Add(intersection.Key, intersection.Value);
                    }
                }
            }

            var previousDistance = 0f;
            var previous = new Coordinate(latitude1, longitude1);
            var previousInside = polygon.PointIn(previous);

            var cleanIntersections = new List<Coordinate>();
            foreach (var intersection in sortedList)
            {
                var offset = intersection.Key - previousDistance;
                if (offset < E)
                { // just over on
                    continue;
                }

                // calculate in or out.
                var middle = new Coordinate((previous.Latitude + intersection.Value.Latitude) / 2,
                    (previous.Longitude + intersection.Value.Longitude) / 2);
                var middleInside = polygon.PointIn(middle);
                if (previousInside != middleInside ||
                    cleanIntersections.Count == 0)
                { // in or out change or this is the first intersection.
                    cleanIntersections.Add(intersection.Value);
                    previousDistance = intersection.Key;
                    previous = intersection.Value;
                    previousInside = middleInside;
                }
            }

            return cleanIntersections;
        }

        private static IEnumerable<KeyValuePair<float, Coordinate>> IntersectInternal(this List<Coordinate> ring,
            Line line)
        {
            var intersections = new List<KeyValuePair<float, Coordinate>>();
            var lineLength = line.Length;

            var E = 0.001f; // this is 1mm.
            var projected = line.ProjectOn(ring[0]);
            if (projected != null)
            {
                var dist = Coordinate.DistanceEstimateInMeter(projected.Value, ring[0]);
                if (dist < E / 2)
                {
                    intersections.Add(new KeyValuePair<float, Coordinate>(0, projected.Value));
                }
            }
            for (var s = 1; s < ring.Count; s++)
            {
                var segment = new Line(ring[s - 1], ring[s]);
                projected = segment.Intersect(line);
                if (projected != null)
                {
                    var dist = Coordinate.DistanceEstimateInMeter(projected.Value, line.Coordinate1);
                    var dist2 = Coordinate.DistanceEstimateInMeter(projected.Value, line.Coordinate2);
                    if (dist < lineLength &&
                        dist2 < lineLength)
                    {
                        intersections.Add(new KeyValuePair<float, Coordinate>(dist, projected.Value));
                    }
                }

                projected = line.ProjectOn(ring[s]);
                if (projected != null)
                {
                    var dist = Coordinate.DistanceEstimateInMeter(projected.Value, ring[s]);
                    if (dist < E / 2)
                    {
                        dist = Coordinate.DistanceEstimateInMeter(projected.Value, line.Coordinate1);
                        intersections.Add(new KeyValuePair<float, Coordinate>(dist, projected.Value));
                    }
                }
            }

            return intersections;
        } 
    }
}