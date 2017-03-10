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

namespace Itinero.LocalGeo
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