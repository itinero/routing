/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System.Collections.Generic;

namespace Itinero.Algorithms.Restrictions
{
    /// <summary>
    /// Contains extension methods for restrictions.
    /// </summary>
    public static class RestrictionExtensions
    {
        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public static void Add(this RestrictionCollection restrictions, uint vertex1)
        {
            restrictions.Add(new Restriction(vertex1));
        }

        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public static void Add(this RestrictionCollection restrictions, uint vertex1, uint vertex2)
        {
            restrictions.Add(new Restriction(vertex1, vertex2));
        }

        /// <summary>
        /// Adds a new restriction.
        /// </summary>
        public static void Add(this RestrictionCollection restrictions, uint vertex1, uint vertex2, uint vertex3)
        {
            restrictions.Add(new Restriction(vertex1, vertex2, vertex3));
        }

        /// <summary>
        /// Returns true if one of the restrictions restricts the one vertex.
        /// </summary>
        public static bool Restricts(this RestrictionCollection restrictions, uint vertex)
        {
            for(var r = 0; r < restrictions.Count; r++)
            {
                var restriction = restrictions[r];
                if (restriction.Restricts(vertex))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the restriction restricts the one vertex.
        /// </summary>
        public static bool Restricts(this Restriction restriction, uint vertex)
        {
            if (restriction.Vertex2 != Constants.NO_VERTEX ||
                restriction.Vertex3 != Constants.NO_VERTEX)
            {
                return false;
            }
            return restriction.Vertex1 == vertex;
        }

        /// <summary>
        /// Restricts the given turn.
        /// </summary>
        public static bool Restricts(this RestrictionCollection restrictions, Turn turn)
        {
            return turn.IsRestrictedBy(restrictions);
        }

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