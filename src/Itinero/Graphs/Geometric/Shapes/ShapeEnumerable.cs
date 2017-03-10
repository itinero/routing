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