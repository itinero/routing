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

using Itinero.Graphs.Geometric;
using Reminiscence.Arrays;

namespace Itinero.Data.Network
{
    /// <summary>
    /// A profile with settings for a memory-mapped routing network.
    /// </summary>
    public class RoutingNetworkProfile
    {
        /// <summary>
        /// Gets or sets the geometric graph profile.
        /// </summary>
        public GeometricGraphProfile GeometricGraphProfile { get; set; }

        /// <summary>
        /// Gets or sets the edge data profile.
        /// </summary>
        public ArrayProfile EdgeDataProfile { get; set; }

        /// <summary>
        /// A profile that tells the routing network not to cache anything.
        /// </summary>
        public static RoutingNetworkProfile NoCache = new RoutingNetworkProfile()
        {
            GeometricGraphProfile = GeometricGraphProfile.NoCache,
            EdgeDataProfile = ArrayProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the routing network to prepare for sequential access.
        /// </summary>
        public static RoutingNetworkProfile OneBuffer = new RoutingNetworkProfile()
        {
            GeometricGraphProfile = GeometricGraphProfile.OneBuffer,
            EdgeDataProfile = ArrayProfile.OneBuffer
        };

        /// <summary>
        /// A default profile with almost no caching at network level but aggressive in the core graph.
        /// </summary>
        public static RoutingNetworkProfile Default = new RoutingNetworkProfile()
        {
            GeometricGraphProfile = GeometricGraphProfile.Default,
            EdgeDataProfile = ArrayProfile.Aggressive8
        };
    }
}