// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using Reminiscence.Arrays;

namespace Itinero.Graphs
{
    /// <summary>
    /// A profile with settings for a memory-mapped graph.
    /// </summary>
    public class GraphProfile
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
        public static GraphProfile NoCache = new GraphProfile()
        {
            VertexProfile = ArrayProfile.NoCache,
            EdgeProfile = ArrayProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static GraphProfile OneBuffer = new GraphProfile()
        {
            VertexProfile = ArrayProfile.OneBuffer,
            EdgeProfile = ArrayProfile.Aggressive8
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 24Kb of cached data.
        /// </summary>
        public static GraphProfile Aggressive24 = new GraphProfile()
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