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

using System.Collections.Generic;

namespace Itinero.Algorithms.Restrictions
{
    /// <summary>
    /// Contains extension methods for restrictions.
    /// </summary>
    public static class RestrictionExtensions
    {
        /// <summary>
        /// Returns true if the given seqence is allowed for the given restriction.
        /// </summary>
        public static bool IsSequenceAllowed(this uint[] restriction, uint[] sequence)
        {
            int start;
            // restriction restricts the sequence only if the entire restriction is part of the sequance.
            return !restriction.Contains(sequence, out start); 
        }

        /// <summary>
        /// Returns true if the given sequence is allowed for all the given restrictions.
        /// </summary>
        public static bool IsSequenceAllowed(this IEnumerable<uint[]> restrictions, uint[] sequence)
        {
            foreach (var restriction in restrictions)
            {
                if (!restriction.IsSequenceAllowed(sequence))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given seqence is allowed for the given restriction.
        /// </summary>
        public static bool IsSequenceAllowed(this uint[] restriction, List<uint> sequence)
        {
            int start;
            // restriction restricts the sequence only if the entire restriction is part of the sequance.
            return !restriction.Contains(sequence, out start);
        }

        /// <summary>
        /// Returns true if the given sequence is allowed for all the given restrictions.
        /// </summary>
        public static bool IsSequenceAllowed(this IEnumerable<uint[]> restrictions, List<uint> sequence)
        {
            foreach (var restriction in restrictions)
            {
                if (!restriction.IsSequenceAllowed(sequence))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the number of vertices in the sequence starting at the beginning that matches any part of the given restriction.
        /// </summary>
        public static int Match(this uint[] sequence, uint[] restriction)
        {
            var c = 0;
            for(var i = 0; i < restriction.Length; i++)
            {
                for(var s = 0; s < sequence.Length; s++)
                {
                    if (i + s >= restriction.Length ||
                        sequence[s] != restriction[i + s])
                    {
                        break;
                    }
                    if (c < s)
                    {
                        c = s;
                    }
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the largest number of vertices in the sequence starting at the beginning that matches any part of any of the given restrictions.
        /// </summary>
        public static int MatchAny(this uint[] sequence, IEnumerable<uint[]> restrictions)
        {
            var c = 0;
            foreach (var restriction in restrictions)
            {
                var localC = sequence.Match(restriction);
                if (localC > c)
                {
                    c = localC;
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the number of vertices in the sequence starting at the beginning that matches any part of the given restriction in reverse direction.
        /// </summary>
        public static int MatchReverse(this uint[] sequence, uint[] restriction)
        {
            var c = 0;
            for (var i = restriction.Length - 1; i >= 0; i--)
            {
                for (var s = 0; s < sequence.Length; s++)
                {
                    if (i - s < 0 ||
                        sequence[s] != restriction[i - s])
                    {
                        break;
                    }
                    if (c < s)
                    {
                        c = s;
                    }
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the largest number of vertices in the sequence starting at the beginning that matches any part of any of the given restrictions.
        /// </summary>
        public static int MatchAnyReverse(this uint[] sequence, IEnumerable<uint[]> restrictions)
        {
            var c = 0;
            foreach (var restriction in restrictions)
            {
                var localC = sequence.MatchReverse(restriction);
                if (localC > c)
                {
                    c = localC;
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the number of vertices in the sequence starting at the beginning that matches any part of the given restriction.
        /// </summary>
        public static int Match(this List<uint> sequence, uint[] restriction)
        {
            var c = 0;
            for (var i = 0; i < restriction.Length; i++)
            {
                for (var s = 0; s < sequence.Count; s++)
                {
                    if (sequence[s] != restriction[i])
                    {
                        break;
                    }
                    if (c < s)
                    {
                        c = s;
                    }
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the largest number of vertices in the sequence starting at the beginning that matches any part of any of the given restrictions.
        /// </summary>
        public static int MatchAny(this List<uint> sequence, IEnumerable<uint[]> restrictions)
        {
            var c = 0;
            foreach (var restriction in restrictions)
            {
                var localC = sequence.Match(restriction);
                if (localC > c)
                {
                    c = localC;
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the number of vertices in the sequence starting at the beginning that matches any part of the given restriction in reverse direction.
        /// </summary>
        public static int MatchReverse(this List<uint> sequence, uint[] restriction)
        {
            var c = 0;
            for (var i = restriction.Length - 1; i >= 0; i--)
            {
                for (var s = 0; s < sequence.Count; s++)
                {
                    if (i - s < 0 ||
                        sequence[s] != restriction[i - s])
                    {
                        break;
                    }
                    if (c < s)
                    {
                        c = s;
                    }
                }
            }
            return c;
        }

        /// <summary>
        /// Returns the largest number of vertices in the sequence starting at the beginning that matches any part of any of the given restrictions.
        /// </summary>
        public static int MatchAnyReverse(this List<uint> sequence, IEnumerable<uint[]> restrictions)
        {
            var c = 0;
            foreach (var restriction in restrictions)
            {
                var localC = sequence.MatchReverse(restriction);
                if (localC > c)
                {
                    c = localC;
                }
            }
            return c;
        }
    }
}