// Itinero - OpenStreetMap (OSM) SDK
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

namespace Itinero
{
    /// <summary>
    /// Contains constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// A constant to use when no edge was found, is available or set.
        /// </summary>
        public const uint NO_EDGE = uint.MaxValue;

        /// <summary>
        /// A constant to use when no vertex was found, is available or set.
        /// </summary>
        public const uint NO_VERTEX = uint.MaxValue - 1;

        /// <summary>
        /// A maximum search distance.
        /// </summary>
        public const float SearchDistanceInMeter = 50;

        /// <summary>
        /// A default maximum edge distance.
        /// </summary>
        public const float DefaultMaxEdgeDistance = 5000f;
        
        /// <summary>
        /// An empty sequence/restriction.
        /// </summary>
        public static uint[] EMPTY_SEQUENCE = new uint[0];
    }
}