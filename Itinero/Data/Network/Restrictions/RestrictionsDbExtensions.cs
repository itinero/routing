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

namespace Itinero.Data.Network.Restrictions
{
    /// <summary>
    /// Contains extension methods for every restriction db related.
    /// </summary>
    public static class RestrictionsDbExtensions
    {
        /// <summary>
        /// Returns an array representing the current restriction.
        /// </summary>
        public static uint[] ToArray(this RestrictionsDb.RestrictionEnumerator enumerator, bool reverse = false)
        {
            if (reverse)
            {
                return enumerator.ToArrayReverse();
            }

            var restriction = new uint[enumerator.Count];
            for(var i = 0; i < enumerator.Count; i++)
            {
                restriction[i] = enumerator[i];
            }
            return restriction;
        }

        /// <summary>
        /// Returns an array representing the current restriction but reversed.
        /// </summary>
        public static uint[] ToArrayReverse(this RestrictionsDb.RestrictionEnumerator enumerator)
        {
            var restriction = new uint[enumerator.Count];
            for (var i = 0; i < enumerator.Count; i++)
            {
                restriction[i] = enumerator[(int)enumerator.Count - i - 1];
            }
            return restriction;
        }

        ///// <summary>
        ///// Moves to the restrictions for the given vertex.
        ///// </summary>
        //public static bool MoveTo(this RestrictionsDb.RestrictionEnumerator enumerator, uint vertex, bool first)
        //{
        //    if (first)
        //    {
        //        return enumerator.MoveToFirst(vertex);
        //    }
        //    else
        //    {
        //        return enumerator.MoveToLast(vertex);
        //    }
        //}

        //public static bool MoveToFirst(this Rest)
    }
}