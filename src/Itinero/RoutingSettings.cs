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

namespace Itinero
{
    /// <summary>
    /// Represents routing settings.
    /// </summary>
    public class RoutingSettings<T>
    {
        private readonly Dictionary<string, T> _maxSearch;

        /// <summary>
        /// Creates new routing settings.
        /// </summary>
        public RoutingSettings()
        {
            _maxSearch = new Dictionary<string, T>();
        }

        /// <summary>
        /// Sets the maximum search weight for the given profile.
        /// </summary>
        public void SetMaxSearch(string profile, T weight)
        {
            _maxSearch[profile] = weight;
        }

        /// <summary>
        /// Gets the maximum search weight for the given profile.
        /// </summary>am>
        /// <returns></returns>
        public bool TryGetMaxSearch(string profile, out T weight)
        {
            return _maxSearch.TryGetValue(profile, out weight);
        }
    }
}