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

using Itinero.LocalGeo;
using System.Collections.Generic;

namespace Itinero.Graphs.Geometric.Shapes
{
    /// <summary>
    /// Abstract representation of a shape.
    /// </summary>
    public abstract class ShapeBase : IEnumerable<Coordinate>
    {
        /// <summary>
        /// Returns the number of coordinates in this shape.
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Gets the coordinate at the given index.
        /// </summary>
        public abstract Coordinate this[int i]
        {
            get;
        }

        /// <summary>
        /// Returns the same shape but with the order of the coordinates reversed.
        /// </summary>
        public abstract ShapeBase Reverse();

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        public IEnumerator<Coordinate> GetEnumerator()
        {
            return new ShapeBaseEnumerator(this);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return new ShapeBaseEnumerator(this);
        }

        private struct ShapeBaseEnumerator : IEnumerator<Coordinate>, System.Collections.IEnumerator
        {
            private readonly ShapeBase _shape;

            public ShapeBaseEnumerator(ShapeBase shape)
            {
                _shape = shape;
                _i = -1;
            }

            private int _i;

            public Coordinate Current
            {
                get { return _shape[_i]; }
            }

            object System.Collections.IEnumerator.Current
            {
                get { return this.Current; }
            }

            public bool MoveNext()
            {
                _i++;
                return _shape.Count > _i;
            }

            public void Reset()
            {
                _i = -1;
            }

            public void Dispose()
            {

            }
        }
    }
}