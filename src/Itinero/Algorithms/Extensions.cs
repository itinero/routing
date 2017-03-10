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
