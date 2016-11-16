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

using Itinero.Attributes;

namespace Itinero.Profiles.Lua
{
    /// <summary>
    /// Containts extension methods related to Lua and Moonscharp.
    /// </summary>
    public static class LuaExtensions
    {
        /// <summary>
        /// Tries to get a number as a float for the given key.
        /// </summary>
        public static bool TryGetFloat(this Table table, string key, out float value)
        {
            var dynValue = table.Get(key);
            if (dynValue != null)
            {
                var number = dynValue.CastToNumber();
                if (number.HasValue)
                {
                    value = (float)number.Value;
                    return true;
                }
            }
            value = float.MaxValue;
            return false;
        }

        /// <summary>
        /// Tries to get a bool for the given key.
        /// </summary>
        public static bool TryGetBool(this Table table, string key, out bool value)
        {
            var dynValue = table.Get(key);
            if (dynValue != null)
            {
                value = dynValue.CastToBool();
                return true;
            }
            value = false;
            return false;
        }

        /// <summary>
        /// Converts the given attribute collection to a lua table.
        /// </summary>
        public static Table ToTable(this IAttributeCollection attributes, Script script)
        {
            var table = new Table(script);
            if (attributes == null)
            {
                return table;
            }
            foreach(var attribute in attributes)
            {
                table[attribute.Key] = attribute.Value;
            }
            return table;
        }
    }
}