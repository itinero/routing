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