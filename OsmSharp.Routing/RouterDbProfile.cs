// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Graphs.Directed;
using OsmSharp.Routing.Network;
using System.Collections.Generic;

namespace OsmSharp.Routing
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
        /// Gets or sets the directed meta graph profile.
        /// </summary>
        public DirectedMetaGraphProfile DirectedMetaGraphProfile { get; set; }

        /// <summary>
        /// A profile telling the router db to do no caching.
        /// </summary>
        public static RouterDbProfile NoCache = new RouterDbProfile()
        {
            DirectedMetaGraphProfile = DirectedMetaGraphProfile.NoCache,
            RoutingNetworkProfile = RoutingNetworkProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some low-end mobile device.
        /// </summary>
        public static RouterDbProfile MobileLowEnd = new RouterDbProfile()
        {
            DirectedMetaGraphProfile = DirectedMetaGraphProfile.Aggressive40,
            RoutingNetworkProfile = RoutingNetworkProfile.NoCache
        };

        /// <summary>
        /// A profile telling the router db it's on some high-end mobile device.
        /// </summary>
        public static RouterDbProfile MobileHighEnd = new RouterDbProfile()
        {
            DirectedMetaGraphProfile = DirectedMetaGraphProfile.Aggressive40,
            RoutingNetworkProfile = RoutingNetworkProfile.Default
        };

        /// <summary>
        /// A profile telling the router db it's on some high-end mobile device.
        /// </summary>
        public static RouterDbProfile Default = new RouterDbProfile()
        {
            DirectedMetaGraphProfile = DirectedMetaGraphProfile.Aggressive40,
            RoutingNetworkProfile = RoutingNetworkProfile.Default
        };
    }
}