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