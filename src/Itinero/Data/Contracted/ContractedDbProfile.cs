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