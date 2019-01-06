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

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// Contains resolver settings.
    /// </summary>
    public class ResolveSettings
    {
        /// <summary>
        /// The default minimum island size in vertices.
        /// </summary>
        public static int DefaultMinIslandSize = 1024;

        /// <summary>
        /// The minimum island size in vertices.
        /// </summary>
        public int MinIslandSize { get; set; } = DefaultMinIslandSize;

        /// <summary>
        /// Creates a deep-copy.
        /// </summary>
        /// <returns></returns>
        public ResolveSettings Clone()
        {
            return new ResolveSettings()
            {
                MinIslandSize = this.MinIslandSize
            };
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hash = 17;
                hash = hash * 23 + this.MinIslandSize.GetHashCode();
                return hash;
            }
        }

        /// <summary>
        /// Returns true if the other object has the same settings in it.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the other object has the same settings in it.</returns>
        public override bool Equals(object obj)
        {
            return obj is ResolveSettings other && other.MinIslandSize == this.MinIslandSize;
        }
    }
}