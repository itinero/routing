// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

namespace OsmSharp.Routing.Geo
{
    /// <summary>
    /// Some tools for angles and other geo-stuff.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static double ToRadians(this double degrees)
        {
            return (degrees / 180d) * System.Math.PI;
        }

        /// <summary>
        /// Converts degrees to radians.
        /// </summary>
        public static double ToRadians(this float degrees)
        {
            return (degrees / 180d) * System.Math.PI;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static double ToDegrees(this double radians)
        {
            return (radians / System.Math.PI) * 180d;
        }

        /// <summary>
        /// Converts radians to degrees.
        /// </summary>
        public static double ToDegrees(this float radians)
        {
            return (radians / System.Math.PI) * 180d;
        }

        /// <summary>
        /// Normalizes this angle to the range of [0-360[.
        /// </summary>
        public static double NormalizeDegrees(this double degrees)
        {
            if (degrees >= 360)
            {
                var count = System.Math.Floor(degrees / 360.0);
                degrees = degrees - (360.0 * count);
            }
            else if(degrees < 0)
            {
                var count = System.Math.Floor(-degrees / 360.0) + 1;
                degrees = degrees + (360.0 * count);
            }
            return degrees;
        }
    }
}