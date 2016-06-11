// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NetTopologySuite.Features;

namespace Itinero.Geo
{
    /// <summary>
    /// Contains extension methods for NTS-related functionality.
    /// </summary>
    public static class NTSExtensions
    {
        /// <summary>
        /// Tries to get a value from the attribute table.
        /// </summary>
        public static bool TryGetValue(this IAttributesTable table, string name, out object value)
        {
            var names = table.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    value = table.GetValues()[i];
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Tries to get a value as a string from the attribute table.
        /// </summary>
        public static bool TryGetValueAsString(this IAttributesTable table, string name, out string value)
        {
            var names = table.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    var objValue = table.GetValues()[i];
                    if (objValue == null)
                    {
                        value = null;
                    }
                    else
                    {
                        value = objValue.ToInvariantString();
                    }
                    return true;
                }
            }
            value = null;
            return false;
        }

        /// <summary>
        /// Returns true if the given table contains the given attribute with the given value.
        /// </summary>
        public static bool Contains(this IAttributesTable table, string name, object value)
        {
            var names = table.GetNames();
            for (var i = 0; i < names.Length; i++)
            {
                if (names[i] == name)
                {
                    return value.Equals(table.GetValues()[i]);
                }
            }
            return false;
        }
    }
}
