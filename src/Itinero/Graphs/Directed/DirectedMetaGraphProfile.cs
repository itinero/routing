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
    /// A profile with settings for a memory-mapped meta-graph.
    /// </summary>
    public class DirectedMetaGraphProfile
    {
        /// <summary>
        /// Gets or sets the directed graph profile.
        /// </summary>
        public DirectedGraphProfile DirectedGraphProfile { get; set; }

        /// <summary>
        /// Gets or sets the vertex meta array profile.
        /// </summary>
        public ArrayProfile VertexMetaProfile { get; set; }

        /// <summary>
        /// Gets or sets the edge meta array profile.
        /// </summary>
        public ArrayProfile EdgeMetaProfile { get; set; }

        /// <summary>
        /// A profile that tells the graph to do no caching.
        /// </summary>
        public static DirectedMetaGraphProfile NoCache = new DirectedMetaGraphProfile()
        {
            DirectedGraphProfile = DirectedGraphProfile.NoCache,
            EdgeMetaProfile = ArrayProfile.NoCache,
            VertexMetaProfile = ArrayProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static DirectedMetaGraphProfile OneBuffer = new DirectedMetaGraphProfile()
        {
            DirectedGraphProfile = DirectedGraphProfile.OneBuffer,
            EdgeMetaProfile = ArrayProfile.Aggressive8,
            VertexMetaProfile = ArrayProfile.OneBuffer
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 40Kb of cached data.
        /// </summary>
        public static DirectedMetaGraphProfile Aggressive40 = new DirectedMetaGraphProfile()
        {
            DirectedGraphProfile = DirectedGraphProfile.Aggressive24,
            EdgeMetaProfile = ArrayProfile.Aggressive8,
            VertexMetaProfile = ArrayProfile.Aggressive8
        };
    }
}
