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

using Itinero.Graphs.Directed;

namespace Itinero.Data.Contracted
{
    /// <summary>
    /// A profile with settings for a memory-mapped graph.
    /// </summary>
    public class ContractedDbProfile
    {
        /// <summary>
        /// Gets or sets the node based profile.
        /// </summary>
        public DirectedMetaGraphProfile NodeBasedProfile { get; set; }

        /// <summary>
        /// Gets or sets the edge based profile.
        /// </summary>
        public DirectedGraphProfile EdgeBasedProfile { get; set; }
        
        /// <summary>
        /// A profile that tells the graph to do no caching.
        /// </summary>
        public static ContractedDbProfile NoCache = new ContractedDbProfile()
        {
            EdgeBasedProfile = DirectedGraphProfile.NoCache,
            NodeBasedProfile = DirectedMetaGraphProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static ContractedDbProfile OneBuffer = new ContractedDbProfile()
        {
            EdgeBasedProfile = DirectedGraphProfile.OneBuffer,
            NodeBasedProfile = DirectedMetaGraphProfile.OneBuffer
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 24Kb of cached data.
        /// </summary>
        public static ContractedDbProfile Aggressive24 = new ContractedDbProfile()
        {
            EdgeBasedProfile = DirectedGraphProfile.Aggressive24,
            NodeBasedProfile = DirectedMetaGraphProfile.Aggressive40
        };
    }
}