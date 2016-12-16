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

using Itinero.Data.Contracted;
using Itinero.Data.Network;
using Itinero.Data.Network.Restrictions;

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
        /// A profile telling the router db to do no caching.
        /// </summary>
        public static RouterDbProfile NoCache = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.NoCache,
            RoutingNetworkProfile = RoutingNetworkProfile.NoCache,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some low-end mobile device.
        /// </summary>
        public static RouterDbProfile MobileLowEnd = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.Aggressive24,
            RoutingNetworkProfile = RoutingNetworkProfile.NoCache,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some high-end mobile device.
        /// </summary>
        public static RouterDbProfile MobileHighEnd = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.Aggressive24,
            RoutingNetworkProfile = RoutingNetworkProfile.Default,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some high-end mobile device.
        /// </summary>
        public static RouterDbProfile Default = new RouterDbProfile()
        {
            ContractedDbProfile = ContractedDbProfile.Aggressive24,
            RoutingNetworkProfile = RoutingNetworkProfile.Default,
            RestrictionDbProfile = RestrictionsDbProfile.NoCache
        };
    }
}