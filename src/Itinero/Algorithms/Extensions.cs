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

namespace Itinero.Algorithms
{
    /// <summary>
    /// Contains generic extension methods.
    /// </summary>
    public static class Extensions
    { 
        /// <summary>
        /// Returns true if this array contains the given sequence.
        /// </summary>
        public static bool Contains(this uint[] array, uint[] sequence, out int start)
        {
            var s = 0;
            for(var i = 0; i < array.Length; i++)
            {
                if (sequence.Length - s > array.Length - i)
                {
                    start = -1;
                    return false;
                }
                if (sequence[s] == array[i])
                {
                    var t = i + 1;
                    s++;
                    while (s != sequence.Length)
                    {
                        if (sequence[s] != array[t])
                        {
                            break;
                        }
                        s++;
                        t++;
                    }
                    if (s == sequence.Length)
                    {
                        start = t - sequence.Length;
                        return true;
                    }
                }
                s = 0;
            }
            start = -1;
            return false;
        }

        /// <summary>
        /// Returns true if this array contains the given sequence.
        /// </summary>
        public static bool Contains(this uint[] array, List<uint> sequence, out int start)
        {
            var s = 0;
            for (var i = 0; i < array.Length; i++)
            {
                if (sequence.Count - s > array.Length - i)
                {
                    start = -1;
                    return false;
                }
                if (sequence[s] == array[i])
                {
                    var t = i + 1;
                    s++;
                    while (s != sequence.Count)
                    {
                        if (sequence[s] != array[t])
                        {
                            break;
                        }
                        s++;
                        t++;
                    }
                    if (s == sequence.Count)
                    {
                        start = t - sequence.Count;
                        return true;
                    }
                }
                s = 0;
            }
            start = -1;
            return false;
        }
    }
}
