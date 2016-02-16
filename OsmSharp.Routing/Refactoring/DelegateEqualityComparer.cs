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

using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Refactoring
{
    /// <summary>
    /// An implementation of the EqualityComparer that allows the use of delegates.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        /// <summary>
        /// A delegate to calculate the hashcode.
        /// </summary>
        public delegate int GetHashCodeDelegate(T obj);

        /// <summary>
        /// A delegate to compare two objects.
        /// </summary>
        public delegate bool EqualsDelegate(T x, T y);

        /// <summary>
        /// Creates a new equality comparer.
        /// </summary>
        public DelegateEqualityComparer(GetHashCodeDelegate hashCodeDelegate, EqualsDelegate equalsDelegate)
        {
            if (hashCodeDelegate == null) { throw new ArgumentNullException("hashCodeDelegate"); }
            if (equalsDelegate == null) { throw new ArgumentNullException("equalsDelegate"); }

            _equalsDelegate = equalsDelegate;
            _hashCodeDelegate = hashCodeDelegate;
        }

        /// <summary>
        /// Holds the equals delegate.
        /// </summary>
        private EqualsDelegate _equalsDelegate;

        /// <summary>
        /// Returns true if the two given objects are considered equal.
        /// </summary>
        public bool Equals(T x, T y)
        {
            return _equalsDelegate.Invoke(x, y);
        }

        /// <summary>
        /// Holds the hashcode delegate.
        /// </summary>
        private GetHashCodeDelegate _hashCodeDelegate;

        /// <summary>
        /// Calculates the hashcode for the given object.
        /// </summary>
        public int GetHashCode(T obj)
        {
            return _hashCodeDelegate.Invoke(obj);
        }
    }
}