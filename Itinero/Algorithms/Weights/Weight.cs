// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Profiles;
using System;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// A structure that represents a weight augmented with time and distance.
    /// </summary>
    public struct Weight
    {
        /// <summary>
        /// Gets or sets the weight.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Gets or sets the time in seconds.
        /// </summary>
        public float Time { get; set; }

        /// <summary>
        /// Get or sets the distance in meters.
        /// </summary>
        public float Distance { get; set; }
        
        /// <summary>
        /// Gets a function that calculates the weight.
        /// </summary>
        /// <returns></returns>
        public static Func<float, FactorAndSpeed, Weight> GetGetWeight()
        {
            return (d, f) =>
            {
                return new Weight()
                {
                    Distance = d,
                    Time = d * f.SpeedFactor,
                    Value = d * f.Value
                };
            };
        }

        /// <summary>
        /// Returns a string describing this object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}m {2}s", this.Value, this.Distance, this.Time);
        }
    }
}
