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

namespace Itinero.Algorithms.Restrictions
{
    /// <summary>
    /// Contains extension methods for restrictions.
    /// </summary>
    public static class RestrictionExtensions
    {
        /// <summary>
        /// Compares two non-null sequences and checks if they have identical elements.
        /// </summary>
        public static bool IsSequenceIdentical(this uint[] s1, uint[] s2)
        {
            if (s1.Length != s2.Length)
            {
                return false;
            }
            for(var i = 0; i < s1.Length; i++)
            {
                if (s1[i] != s2[i])
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given seqence is allowed for the given restriction.
        /// </summary>
        public static bool IsSequenceAllowed(this uint[] restriction, uint[] sequence)
        {
            int start;
            // restriction restricts the sequence only if the entire restriction is part of the sequance.
            return !sequence.Contains(restriction, out start); 
        }

        /// <summary>
        /// Returns true if the given sequence is allowed for all the given restrictions.
        /// </summary>
        public static bool IsSequenceAllowed(this IEnumerable<uint[]> restrictions, uint[] sequence)
        {
            if (restrictions == null)
            {
                return true;
            }

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
        
        /// <summary>
        /// Shrinks this restrictions assuming the given sequence has already been travelled. Sequence needs to match first part of the restriction.
        /// 
        /// [0, 1, 2, 3] for sequence [0, 1] returns [1, 2, 3]
        /// [0, 1, 2, 3] for sequence [0, 2] returns []
        /// [0, 1, 2, 3] for sequence [1, 2] returns []
        /// </summary>
        public static uint[] ShrinkFor(this uint[] restriction, uint[] sequence)
        {
            if (sequence.Length == 0)
            {
                return restriction;
            }
            if (restriction.Length <= sequence.Length)
            {
                return Constants.EMPTY_SEQUENCE;
            }
            for(var i = 0; i < sequence.Length; i++)
            {
                if (sequence[i] != restriction[i])
                {
                    return Constants.EMPTY_SEQUENCE;
                }
            }
            return restriction.SubArray(sequence.Length - 1, restriction.Length - sequence.Length + 1);
        }

        /// <summary>
        /// Shrinks this restrictions assuming the given sequence has already been travelled. Sequence needs to match first part of the restriction.
        /// 
        /// [0, 1, 2, 3] for sequence [0, 1] returns [1, 2, 3]
        /// [0, 1, 2, 3] for sequence [0, 2] returns []
        /// [0, 1, 2, 3] for sequence [1, 2] returns []
        /// </summary>
        public static uint[] ShrinkFor(this uint[] restriction, List<uint> sequence)
        {
            if (sequence.Count == 0)
            {
                return restriction;
            }
            if (restriction.Length <= sequence.Count)
            {
                return Constants.EMPTY_SEQUENCE;
            }
            for (var i = 0; i < sequence.Count; i++)
            {
                if (sequence[i] != restriction[i])
                {
                    return Constants.EMPTY_SEQUENCE;
                }
            }
            return restriction.SubArray(sequence.Count - 1, restriction.Length - sequence.Count + 1);
        }

        /// <summary>
        /// Shrinks this restriction assuming the given sequence has already been travelled. Last part of the sequence needs to match some of the first part of the restriction.
        /// </summary>
        /// 
        /// [0, 1, 2, 3] for sequence [0, 1] returns [1, 2, 3] because [0, 1] matches.
        /// [0, 1, 2, 3] for sequence [0, 2] returns [] because no matches.
        /// [0, 1, 2, 3] for sequence [1, 2] returns [] because no matches.
        /// [0, 1, 2, 3] for sequence [3, 0, 1] returns [1, 2, 3] because [0, 1] matches.
        public static uint[] ShrinkForPart(this uint[] restriction, uint[] sequence)
        {
            return restriction.ShrinkForPart(new List<uint>(sequence));
        }

        /// <summary>
        /// Shrinks this restriction assuming the given sequence has already been travelled. Last part of the sequence needs to match some of the first part of the restriction.
        /// </summary>
        /// 
        /// [0, 1, 2, 3] for sequence [0, 1] returns [1, 2, 3] because [0, 1] matches.
        /// [0, 1, 2, 3] for sequence [0, 2] returns [] because no matches.
        /// [0, 1, 2, 3] for sequence [1, 2] returns [] because no matches.
        /// [0, 1, 2, 3] for sequence [3, 0, 1] returns [1, 2, 3] because [0, 1] matches.
        public static uint[] ShrinkForPart(this uint[] restriction, List<uint> sequence)
        {
            for(var m = System.Math.Min(sequence.Count, restriction.Length); m >= 1 ; m--)
            {
                var match = true;
                for(var i = 0; i < m; i++)
                {
                    if (restriction[i] != sequence[sequence.Count - m + i])
                    {
                        match = false;
                        break;
                    }
                }
                if(match)
                { // definetly the best match.
                    return restriction.SubArray(m - 1, restriction.Length - m + 1);
                }
            }
            return new uint[0];
        }
    }
}