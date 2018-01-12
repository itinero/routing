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

using Itinero.LocalGeo;
using Reminiscence.Arrays;

namespace Itinero.Graphs.Geometric.Shapes
{
    /// <summary>
    /// Represents a shape, a sequence of coordinates that represents the shape of an edge.
    /// </summary>
    public class Shape : ShapeBase
    {
        private readonly ArrayBase<float> _coordinates;
        private readonly ArrayBase<short> _elevation;
        private readonly long _pointer;
        private readonly int _size;
        private readonly bool _reversed;

        /// <summary>
        /// Creates a new shape.
        /// </summary>
        internal Shape(ArrayBase<float> coordinates, long pointer, int size)
        {
            _coordinates = coordinates;
            _elevation = null;
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
            _elevation = null;
            _pointer = pointer;
            _size = size;
            _reversed = reversed;
        }

        /// <summary>
        /// Creates a new shape.
        /// </summary>
        internal Shape(ArrayBase<float> coordinates, ArrayBase<short> elevation, long pointer, int size)
        {
            _coordinates = coordinates;
            _elevation = elevation;
            _pointer = pointer;
            _size = size;
            _reversed = false;
        }

        /// <summary>
        /// Creates a new shape.
        /// </summary>
        internal Shape(ArrayBase<float> coordinates, ArrayBase<short> elevation, long pointer, int size, bool reversed)
        {
            _coordinates = coordinates;
            _elevation = elevation;
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
                    if (_elevation != null)
                    {
                        return new Coordinate()
                        {
                            Latitude = _coordinates[_pointer + ((_size - 1) * 2) - (i * 2)],
                            Longitude = _coordinates[_pointer + ((_size - 1) * 2) - (i * 2) + 1],
                            Elevation = _elevation[(_pointer / 2) + (_size - 1) - i]
                        };
                    }
                    return new Coordinate()
                    {
                        Latitude = _coordinates[_pointer + ((_size - 1) * 2) - (i * 2)],
                        Longitude = _coordinates[_pointer + ((_size - 1) * 2) - (i * 2) + 1],
                    };
                }
                if (_elevation != null)
                {
                    return new Coordinate()
                    {
                        Latitude = _coordinates[_pointer + (i * 2)],
                        Longitude = _coordinates[_pointer + (i * 2) + 1],
                        Elevation = _elevation[(_pointer / 2) + i]
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
            return new Shape(_coordinates, _elevation, _pointer, _size, !_reversed);
        }
    }
}