// Itinero - Routing for .NET
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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

using System.Collections.Generic;

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Represents a polygon.
    /// </summary>
    public class Polygon
    {
        /// <summary>
        /// The exterior ring.
        /// </summary>
        public List<Coordinate> ExteriorRing { get; set; } = new List<Coordinate>();

        /// <summary>
        /// The interior rings.
        /// </summary>
        public List<List<Coordinate>> InteriorRings { get; set; } = new List<List<Coordinate>>();
    }
}