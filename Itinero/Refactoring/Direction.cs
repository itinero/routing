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

namespace Itinero.Refactoring
{
    /// <summary>
    /// Direction types.
    /// </summary>
    public enum Direction
    {
        /// <summary>
        /// North.
        /// </summary>
        North = 0,
        /// <summary>
        /// Northeast.
        /// </summary>
        NorthEast = 45,
        /// <summary>
        /// East.
        /// </summary>
        East = 90,
        /// <summary>
        /// Southeast.
        /// </summary>
        SouthEast = 135,
        /// <summary>
        /// South.
        /// </summary>
        South = 180,
        /// <summary>
        /// Southwest.
        /// </summary>
        SouthWest = 225,
        /// <summary>
        /// West.
        /// </summary>
        West = 270,
        /// <summary>
        /// Northwest.
        /// </summary>
        NorthWest = 315,
    }
}