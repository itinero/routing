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

using System.Collections.Generic;

namespace Itinero.LocalGeo
{
    /// <summary>
    /// Extension methods for the local geo objects.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Converts the coordinates to a double double array.
        /// </summary>
        public static double[][] ToLonLatArray(this List<Coordinate> coordinates)
        {
            var array = new double[coordinates.Count][];

            for(var i = 0; i < coordinates.Count; i++)
            {
                array[i] = new double[]
                {
                    coordinates[i].Longitude,
                    coordinates[i].Latitude
                };
            }

            return array;
        }
    }
}