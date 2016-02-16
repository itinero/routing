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

using OsmSharp.Routing.Geo;
using Reminiscence.Arrays;

namespace OsmSharp.Routing.Graphs.Geometric.Shapes
{
    /// <summary>
    /// Represents a shape, a sequence of coordinates that represents the shape of an edge.
    /// </summary>
    public class Shape : ShapeBase
    {
        private readonly ArrayBase<float> _coordinates;
        private readonly long _pointer;
        private readonly int _size;
        private readonly bool _reversed;

        /// <summary>
        /// Creates a new shape.
        /// </summary>
        internal Shape(ArrayBase<float> coordinates, long pointer, int size)
        {
            _coordinates = coordinates;
            _pointer = pointer;
            _size = size;
            _reversed = false;
        }

        /// <summary>
        /// Creates a new shape.
        /// </summary>
        internal Shape(ArrayBase<float> coordinates, long pointer, int size, bool reversed)
        {
            _coordinates = coordinates;
            _pointer = pointer;
            _size = size;
            _reversed = reversed;
        }

        /// <summary>
        /// Returns the number of coordinates.
        /// </summary>
        public override int Count
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the coordinate at the given index.
        /// </summary>
        public override Coordinate this[int i]
        {
            get
            {
                if (_reversed)
                {
                    return new Coordinate()
                    {
                        Latitude = _coordinates[_pointer + ((_size - 1) * 2) - (i * 2)],
                        Longitude = _coordinates[_pointer + ((_size - 1) * 2) - (i * 2) + 1],
                    };
                }
                return new Coordinate()
                {
                    Latitude = _coordinates[_pointer + (i * 2)],
                    Longitude = _coordinates[_pointer + (i * 2) + 1]
                };
            }
        }

        /// <summary>
        /// Returns the same shape but with the order of the coordinates reversed.
        /// </summary>
        public override ShapeBase Reverse()
        {
            return new Shape(_coordinates, _pointer, _size, !_reversed);
        }
    }
}