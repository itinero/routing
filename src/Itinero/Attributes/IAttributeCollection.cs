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

namespace Itinero.Attributes
{
    /// <summary>
    /// Abstract representation of an attribute collection.
    /// </summary>
    public interface IAttributeCollection : IEnumerable<Attribute>
    {
        /// <summary>
        /// Gets the count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Gets the readonly flag.
        /// </summary>
        bool IsReadonly { get; }

        /// <summary>
        /// Adds or replaces an attribute.
        /// </summary>
        void AddOrReplace(string key, string value);

        /// <summary>
        /// Tries to get the value for the given key.
        /// </summary>
        bool TryGetValue(string key, out string value);

        /// <summary>
        /// Removes the attribute with the given key.
        /// </summary>
        bool RemoveKey(string key);

        /// <summary>
        /// Clears all attributes.
        /// </summary>
        void Clear();
    }
}