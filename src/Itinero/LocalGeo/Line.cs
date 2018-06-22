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
using System.Collections;
using System.Collections.Generic;

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Represents a line.
    /// </summary>
    public struct Line
    {
        const double E = 0.0000000001;
        private readonly Coordinate _coordinate1;
        private readonly Coordinate _coordinate2;

        /// <summary>
        /// Creates a new line.
        /// </summary>
        public Line(Coordinate coordinate1, Coordinate coordinate2)
        {
            _coordinate1 = coordinate1;
            _coordinate2 = coordinate2;
        }

        /// <summary>
        /// Returns parameter A of an equation describing this line as Ax + By = C
        /// </summary>
        public float A
        {
            get { return _coordinate2.Latitude - _coordinate1.Latitude; }
        }

        /// <summary>
        /// Returns parameter B of an equation describing this line as Ax + By = C
        /// </summary>
        public float B
        {
            get { return _coordinate1.Longitude - _coordinate2.Longitude; }
        }

        /// <summary>
        /// Returns parameter C of an equation describing this line as Ax + By = C
        /// </summary>
        public float C
        {
            get { return this.A * _coordinate1.Longitude + this.B * _coordinate1.Latitude; }
        }

        /// <summary>
        /// Gets the first coordinate.
        /// </summary>
        public Coordinate Coordinate1
        {
            get { return _coordinate1; }
        }
        
        /// <summary>
        /// Gets the second coordinate.
        /// </summary>
        public Coordinate Coordinate2
        {
            get { return _coordinate2; }
        }

       
        /// <summary>
        /// Gets the middle of this line.
        /// </summary>
        /// <returns></returns>
        public Coordinate Middle
        {
            get
            {
                return new Coordinate((this.Coordinate1.Latitude + this.Coordinate2.Latitude) / 2,
                    (this.Coordinate1.Longitude + this.Coordinate2.Longitude) / 2);
            }
        }


        /// <summary>
        /// Gets the length of this line.
        /// </summary>
        public float Length
        {
            get { return Coordinate.DistanceEstimateInMeter(_coordinate1, _coordinate2); }
        }


        /// <summary>
        /// Calculates the intersection point of the given line with this line. 
        /// 
        /// Returns null if the lines have the same direction or don't intersect.
        /// 
        /// Assumes the given line is not a segment and this line is a segment.
        /// </summary>
        public Coordinate? Intersect(Line l2, bool useBoundingBoxChecks = true)
        {
            // We get two lines and try to calculate the intersection between them
            // We are only interested in intersections that are actually between the two coordinates
            // This is quickly checked with bounding boxes
            // In order to do this, we do some normalization of the lines first

            var l1 = this.Normalize();
           l2 = l2.Normalize();
            
            if (useBoundingBoxChecks && (l1.MinLon() - l2.MaxLon() > E|| l1.MaxLon() - l2.MinLon() < E))
            {
                // No intersection possible
                return null;
            }


            if (useBoundingBoxChecks && (l1.MinLat() - l2.MaxLat() > E || l1.MaxLat() - l2.MinLat() < E))
            {
                // No intersection possible
                return null;
            }


            var det = (double) (l2.A * l1.B - l1.A * l2.B);
            if (Math.Abs(det) <= E)
            {
                // lines are parallel; no intersections.
                return null;
            }


            // lines are not the same and not parallel so they will intersect.
            var x = (l1.B * l2.C - l2.B * l1.C) / det;
            var y = (l2.A * l1.C - l1.A * l2.C) / det;

            // We have a coordinate!
            var coordinate = new Coordinate((float) y, (float) x);

            
            // It the point the within the bounding box of both lines?
            if (useBoundingBoxChecks && !(l1.InBBox(coordinate) && l2.InBBox(coordinate)))
            {
                return null;
            }

            if (!l1.Coordinate1.Elevation.HasValue || !l1.Coordinate2.Elevation.HasValue)
            {
                // No elevation data. We are done
                return coordinate;
            }


            // There is elevation data we have to take into account
            if (l1.Coordinate1.Elevation == l2.Coordinate2.Elevation)
            {
                // don't calculate anything, elevation is identical.
                coordinate.Elevation = l1.Coordinate1.Elevation;
                return coordinate;
            }

            if (Math.Abs(l1.A) < E && Math.Abs(l1.B) < E)
            {
                // tiny segment, not stable enough to calculate offset
                coordinate.Elevation = l1.Coordinate1.Elevation;
                return coordinate;
            }


            // calculate offset and calculate an estimiate of the elevation.
            if (Math.Abs(l1.A) > Math.Abs(l1.B))
            {
                var diffLat = Math.Abs(l1.A);
                var diffLatIntersection = Math.Abs(coordinate.Latitude - l1.Coordinate1.Latitude);

                coordinate.Elevation =
                    (short) ((l1.Coordinate2.Elevation - l1.Coordinate1.Elevation) *
                             (diffLatIntersection / diffLat) +
                             l1.Coordinate1.Elevation);
            }
            else
            {
                var diffLon = System.Math.Abs(l1.B);
                var diffLonIntersection = Math.Abs(coordinate.Longitude - l1.Coordinate1.Longitude);

                coordinate.Elevation =
                    (short) ((l1.Coordinate2.Elevation - l1.Coordinate1.Elevation) *
                             (diffLonIntersection / diffLon) +
                             l1.Coordinate1.Elevation);
            }

            return coordinate;
        }

        /// <summary>
        /// Calculates the slope number of this line (richtingscoëfficient)
        /// </summary>
        /// <returns></returns>
        public float Slope()
        {
            return (Coordinate1.Latitude - Coordinate2.Latitude) / (Coordinate1.Longitude - Coordinate2.Longitude);
        }

        /// <summary>
        /// Projects for coordinate on this line.
        /// </summary>
        public Coordinate? ProjectOn(Coordinate coordinate)
        {
            if (this.Length < E)
            {
                return null;
            }

            // get direction vector.
            var diffLat = ((double) _coordinate2.Latitude - (double) _coordinate1.Latitude) * 100.0;
            var diffLon = ((double) _coordinate2.Longitude - (double) _coordinate1.Longitude) * 100.0;

            // increase this line in length if needed.
            var thisLine = this;
            if (this.Length < 50)
            {
                thisLine = new Line(_coordinate1, new Coordinate((float) (diffLat + coordinate.Latitude),
                    (float) (diffLon + coordinate.Longitude)));
            }

            // rotate 90°.
            var temp = diffLon;
            diffLon = -diffLat;
            diffLat = temp;

            // create second point from the given coordinate.
            var second = new Coordinate((float) (diffLat + coordinate.Latitude),
                (float) (diffLon + coordinate.Longitude));

            // create a second line.
            var line = new Line(coordinate, second);

            // calculate intersection.
            var projected = thisLine.Intersect(line, useBoundingBoxChecks:false);

            // check if coordinate is on this line.
            if (!projected.HasValue)
            {
                return null;
            }

            if (!thisLine.Equals(this))
            {
                // check if the coordinate is on this line.
                var dist = this.A * this.A + this.B * this.B;
                var line1 = new Line(projected.Value, _coordinate1);
                var distTo1 = line1.A * line1.A + line1.B * line1.B;
                if (distTo1 > dist)
                {
                    return null;
                }

                var line2 = new Line(projected.Value, _coordinate2);
                var distTo2 = line2.A * line2.A + line2.B * line2.B;
                if (distTo2 > dist)
                {
                    return null;
                }

                return projected;
            }

            return projected;
        }

        /// <summary>
        /// Returns the distance from the point to this line.
        /// </summary>
        public float? DistanceInMeter(Coordinate coordinate)
        {
            var projected = this.ProjectOn(coordinate);
            if (projected.HasValue)
            {
                return Coordinate.DistanceEstimateInMeter(coordinate, projected.Value);
            }

            return null;
        }

        /// <summary>
        /// Returns a line where the coordinate with the lowest Latitude will be saved as Coordinate1
        /// The line might be a fresh clone of a pointer to this.
        /// </summary>
        /// <returns></returns>
        public Line OrderedLatitude()
        {
            return Coordinate1.Latitude < Coordinate2.Latitude ? this : new Line(Coordinate2, Coordinate1);
        }

        /// <summary>
        /// Returns a line where the coordinate with the lowest Longitude will be saved as Coordinate1
        /// The line might be a fresh clone of a pointer to this.
        /// </summary>
        /// <returns></returns>
        public Line OrderedLongitude()
        {
            return Coordinate1.Longitude < Coordinate2.Longitude ? this : new Line(Coordinate2, Coordinate1);
        }

        /// <summary>
        /// If (and only if) this line crossed the ante-meridian, the line segment is normalized.
        /// This is done by taking the negative coordinate and adding a 360 degrees to it.
        /// We assume that a line crosses the antemeridian if abs(longitude) are both more then 90°
        /// This will construct a new Line if normalization happens, or return 'this' if the coordinate is well behaved.
        ///
        /// Note: the lowest longitude will always be coordinate1 afterwards; an OrderedLongitude is called as well
        /// </summary>
        /// <returns></returns>
        public Line Normalize()
        {
            var c1 = Coordinate1.Longitude;
            var c2 = Coordinate2.Longitude;
            if (Math.Sign(c1) != Math.Sign(c2) &&
                Math.Abs(c1) > 90 && Math.Abs(c2) > 90)
            {
                // We cross the ante-meridian. We 'normalize'
                // Which one is the culprit?
                return c1 < 0
                    ? new Line(Coordinate2, Coordinate1 + new Coordinate(0, 360))
                    : new Line(Coordinate1, Coordinate2 + new Coordinate(0, 360));
            }

            return this.OrderedLongitude();
        }

        public float MaxLat()
        {
            return Math.Max(Coordinate1.Latitude, Coordinate2.Latitude);
        }

        public float MinLat()
        {
            return Math.Min(Coordinate1.Latitude, Coordinate2.Latitude);
        }

        public float MaxLon()
        {
            return Math.Max(Coordinate1.Longitude, Coordinate2.Longitude);
        }

        public float MinLon()
        {
            return Math.Min(Coordinate1.Longitude, Coordinate2.Longitude);
        }

        public bool InBBox(Coordinate c)
        {
            return MinLat() <= c.Latitude && c.Latitude <= MaxLat() && MinLon() <= c.Longitude && c.Longitude <= MaxLon();
        }

        /// <inheritdoc />
        /// <summary>
        /// Comparer which compares the latitudes of Coordinate1 of both lines
        /// </summary>
        public class Latitude1Comparer : IComparer<Line>
        {
            public int Compare(Line x, Line y)
            {
                return x.Coordinate1.Latitude.CompareTo(y.Coordinate1.Latitude);
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// Comparer which compares the longitudes of Coordinate1 of both lines
        /// </summary>
        public class Longitude1Comparer : IComparer<Line>
        {
            public int Compare(Line x, Line y)
            {
                return x.Coordinate1.Longitude.CompareTo(y.Coordinate1.Longitude);
            }
        }
    }
}