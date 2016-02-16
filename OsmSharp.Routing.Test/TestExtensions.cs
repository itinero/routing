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

using OsmSharp.Routing.Geo;
using System;

namespace OsmSharp.Routing.Test
{
    /// <summary>
    /// Contains extension methods for tests.
    /// </summary>
    public static class TestExtensions
    {
        private static Random _random = new Random();

        /// <summary>
        /// Generates a random coordinate in the given box.
        /// </summary>
        public static Coordinate GenerateRandomIn(this Box box)
        {
            var xNext = (float)_random.NextDouble();
            var yNext = (float)_random.NextDouble();

            return new Coordinate(box.MinLat + (box.MaxLat - box.MinLat) * xNext,
                box.MinLon + (box.MaxLon - box.MinLon) * yNext);
        }
    }
}