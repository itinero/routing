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

using Reminiscence.Arrays;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// A profile with settings for a memory-mapped graph.
    /// </summary>
    public class DirectedGraphProfile
    {
        /// <summary>
        /// Gets or sets the vertex array profile.
        /// </summary>
        public ArrayProfile VertexProfile { get; set; }

        /// <summary>
        /// Gets or sets the edge array profile.
        /// </summary>
        public ArrayProfile EdgeProfile { get; set; }

        /// <summary>
        /// A profile that tells the graph to do no caching.
        /// </summary>
        public static DirectedGraphProfile NoCache = new DirectedGraphProfile()
        {
            VertexProfile = ArrayProfile.NoCache,
            EdgeProfile = ArrayProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static DirectedGraphProfile OneBuffer = new DirectedGraphProfile()
        {
            VertexProfile = ArrayProfile.OneBuffer,
            EdgeProfile = ArrayProfile.Aggressive8
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 24Kb of cached data.
        /// </summary>
        public static DirectedGraphProfile Aggressive24 = new DirectedGraphProfile()
        {
            VertexProfile = ArrayProfile.Aggressive8,
            EdgeProfile = new ArrayProfile()
            {
                BufferSize = 1024,
                CacheSize = 16
            }
        };
    }
}
