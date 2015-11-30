// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

namespace OsmSharp.Routing
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
        /// A default search offset.
        /// </summary>
        public const float DefaultSearchOffsetInMeter = Data.EdgeDataSerializer.MAX_DISTANCE;

        /// <summary>
        /// A maximum search distance.
        /// </summary>
        public const float DefaultSearchMaxDistance = 50;
    }
}