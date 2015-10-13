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

using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Graphs.Directed;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Network.Data;
using System.Collections.Generic;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Represents a routing database.
    /// </summary>
    public class RouterDb
    {
        private readonly RoutingNetwork _network;
        private readonly Dictionary<Profiles.Profile, DirectedGraph> _contracted;
        private readonly ITagsIndex _profiles;
        private readonly ITagsIndex _meta;

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb()
        {
            _network = new RoutingNetwork(new Graphs.Geometric.GeometricGraph(1));
            _profiles = new TagsIndex();
            _meta = new TagsIndex();

            _contracted = new Dictionary<Profiles.Profile, DirectedGraph>();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(RoutingNetwork network, ITagsIndex profiles, ITagsIndex meta)
        {
            _network = network;
            _profiles = profiles;
            _meta = meta;

            _contracted = new Dictionary<Profiles.Profile, DirectedGraph>();
        }

        /// <summary>
        /// Returns the network.
        /// </summary>
        public RoutingNetwork Network
        {
            get
            {
                return _network;
            }
        }

        /// <summary>
        /// Returns the profiles index.
        /// </summary>
        public ITagsIndex Profiles
        {
            get
            {
                return _profiles;
            }
        }

        /// <summary>
        /// Returns the meta-data index.
        /// </summary>
        public ITagsIndex Meta
        {
            get
            {
                return _meta;
            }
        }

        /// <summary>
        /// Adds a contracted version of the routing network for the given profile.
        /// </summary>
        public void AddContracted(Profiles.Profile profile, DirectedGraph contracted)
        {
            _contracted.Add(profile, contracted);
        }

        /// <summary>
        /// Tries to get a contracted version of the routing network for the given profile.
        /// </summary>
        /// <returns></returns>
        public bool TryGetContracted(Profiles.Profile profile, out DirectedGraph contracted)
        {
            return _contracted.TryGetValue(profile, out contracted);
        }

        /// <summary>
        /// Returns true if this routing db has a contracted version of the routing network for the given profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool HasContractedFor(Profiles.Profile profile)
        {
            return _contracted.ContainsKey(profile);
        }
    }
}