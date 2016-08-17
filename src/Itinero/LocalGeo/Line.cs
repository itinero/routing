// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
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

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Represents a line.
    /// </summary>
    public struct Line
    {
        const double E = 0.0000001;
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
        internal float A
        {
            get
            {
                return _coordinate2.Latitude - _coordinate1.Latitude;
            }
        }

        /// <summary>
        /// Returns parameter B of an equation describing this line as Ax + By = C
        /// </summary>
        internal float B
        {
            get
            {
                return _coordinate1.Longitude - _coordinate2.Longitude;
            }
        }

        /// <summary>
        /// Returns parameter C of an equation describing this line as Ax + By = C
        /// </summary>
        internal float C
        {
            get
            {
                return this.A * _coordinate1.Longitude + this.B * _coordinate1.Latitude;
            }
        }

        /// <summary>
        /// Gets the length of this line.
        /// </summary>
        public float Length
        {
            get
            {
                return Coordinate.DistanceEstimateInMeter(_coordinate1, _coordinate2);
            }
        }

        /// <summary>
        /// Calculates the intersection point of the given line with this line. 
        /// 
        /// Returns null if the lines have the same direction or don't intersect.
        /// 
        /// Assumes the given line is not a segement and this line is a segment.
        /// </summary>
        internal Coordinate? Intersect(Line line)
        {
            var det = (double)(line.A * this.B - this.A * line.B);
            if (System.Math.Abs(det) <= E)
            { // lines are parallel; no intersections.
                return null;
            }
            else
            { // lines are not the same and not parallel so they will intersect.
                double x = (this.B * line.C - line.B * this.C) / det;
                double y = (line.A * this.C - this.A * line.C) / det;

                var coordinate = new Coordinate((float)y, (float)x);

                // check if the coordinate is on this line.
                var dist = this.A * this.A + this.B * this.B;
                var line1 = new Line(coordinate, _coordinate1);
                var distTo1 = line1.A * line1.A + line1.B * line1.B;
                if (distTo1 > dist)
                {
                    return null;
                }
                var line2 = new Line(coordinate, _coordinate2);
                var distTo2 = line2.A * line2.A + line2.B * line2.B;
                if (distTo2 > dist)
                {
                    return null;
                }
                return coordinate;
            }
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
            var diffLat = ((double)_coordinate2.Latitude - (double)_coordinate1.Latitude) * 100.0;
            var diffLon = ((double)_coordinate2.Longitude - (double)_coordinate1.Longitude) * 100.0;

            // increase this line in length if needed.
            var thisLine = this;
            if (this.Length < 50)
            {
                thisLine = new Line(_coordinate1, new Coordinate((float)(diffLat + coordinate.Latitude), 
                    (float)(diffLon + coordinate.Longitude)));
            }

            // rotate 90°.
            var temp = diffLon;
            diffLon = -diffLat;
            diffLat = temp;

            // create second point from the given coordinate.
            var second = new Coordinate((float)(diffLat + coordinate.Latitude), (float)(diffLon + coordinate.Longitude));

            // create a second line.
            var line = new Line(coordinate, second);

            // calculate intersection.
            var projected = thisLine.Intersect(line);

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
    }
}