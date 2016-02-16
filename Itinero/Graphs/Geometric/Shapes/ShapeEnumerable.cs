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

using Itinero.LocalGeo;
using System.Collections.Generic;

namespace Itinero.Graphs.Geometric.Shapes
{
    /// <summary>
    /// An implementation of a shape based on a coordinate enumerable.
    /// </summary>
    public class ShapeEnumerable : ShapeBase
    {
        private readonly List<Coordinate> _coordinates;
        private readonly bool _reversed;

        /// <summary>
        /// Creates a new shape based on a coordinate enumerable.
        /// </summary>
        public ShapeEnumerable(IEnumerable<Coordinate> coordinates)
        {
            _coordinates = new List<Coordinate>(coordinates);
            _reversed = false;
        }

        /// <summary>
        /// Creates a new shape based on a coordinate enumerable.
        /// </summary>
        public ShapeEnumerable(IEnumerable<Coordinate> coordinates, bool reversed)
        {
            _coordinates = new List<Coordinate>(coordinates);
            _reversed = reversed;
        }

        /// <summary>
        /// Returns the number of coordinates.
        /// </summary>
        public override int Count
        {
            get { return _coordinates.Count; }
        }

        /// <summary>
        /// Gets or sets the coordinate.
        /// </summary>
        public override Coordinate this[int i]
        {
            get
            {
                if(_reversed)
                {
                    return _coordinates[_coordinates.Count - i - 1];
                }
                return _coordinates[i];
            }
        }

        /// <summary>
        /// Returns the same shape but with the order of the coordinates reversed.
        /// </summary>
        public override ShapeBase Reverse()
        {
            return new ShapeEnumerable(_coordinates, !_reversed);
        }
    }
}