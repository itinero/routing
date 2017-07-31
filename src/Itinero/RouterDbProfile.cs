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

using Itinero.Attributes;
using Itinero.Data.Contracted;
using Itinero.Data.Network;
using Itinero.Data.Network.Restrictions;
using Reminiscence.Arrays;

namespace Itinero
{
    /// <summary>
    /// A profile for a memory-mapped router db.
    /// </summary>
    public class RouterDbProfile
    {
        /// <summary>
        /// Gets or sets the routing network profile.
        /// </summary>
        public RoutingNetworkProfile RoutingNetworkProfile { get; set; }

        /// <summary>
        /// Gets or sets the contracted db profile.
        /// </summary>
        public ContractedDbProfile ContractedDbProfile { get; set; }

        /// <summary>
        /// Gets or sets the restriction db profile.
        /// </summary>
        public RestrictionsDbProfile RestrictionDbProfile { get; set; }

        /// <summary>
        /// Gets or sets the vertex meta profile.
        /// </summary>
        public MappedAttributesIndexProfile VertexMetaProfile { get; set; }

        /// <summary>
        /// Gets or sets the vertex data profile.
        /// </summary>
        public ArrayProfile VertexDataProfile { get; set; }

        /// <summary>
        /// A profile telling the router db to do no caching.
        /// </summary>
        public static RouterDbProfile NoCache = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.NoCache,
            RoutingNetworkProfile = RoutingNetworkProfile.NoCache,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache,
            VertexMetaProfile = MappedAttributesIndexProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some low-end mobile device.
        /// </summary>
        public static RouterDbProfile MobileLowEnd = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.Aggressive24,
            RoutingNetworkProfile = RoutingNetworkProfile.NoCache,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache,
            VertexMetaProfile = MappedAttributesIndexProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some high-end mobile device.
        /// </summary>
        public static RouterDbProfile MobileHighEnd = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.Aggressive24,
            RoutingNetworkProfile = RoutingNetworkProfile.Default,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache,
            VertexMetaProfile = MappedAttributesIndexProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some high-end mobile device.
        /// </summary>
        public static RouterDbProfile Default = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.Aggressive24,
            RoutingNetworkProfile = RoutingNetworkProfile.Default,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache,
            VertexMetaProfile = MappedAttributesIndexProfile.NoCache
        };
    }
}