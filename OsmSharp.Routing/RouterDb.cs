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
        private readonly ITagsIndex _profiles;
        private readonly ITagsIndex _meta;

        private readonly Dictionary<string, DirectedMetaGraph> _contracted;
        private readonly HashSet<string> _supportedProfiles;

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb()
        {
            _network = new RoutingNetwork(new Graphs.Geometric.GeometricGraph(1));
            _profiles = new TagsIndex();
            _meta = new TagsIndex();

            _supportedProfiles = new HashSet<string>();
            _contracted = new Dictionary<string, DirectedMetaGraph>();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(RoutingNetwork network, ITagsIndex profiles, ITagsIndex meta,
            params Profiles.Profile[] supportedProfiles)
        {
            _network = network;
            _profiles = profiles;
            _meta = meta;

            _supportedProfiles = new HashSet<string>();
            foreach(var supportedProfile in supportedProfiles)
            {
                _supportedProfiles.Add(supportedProfile.Name);
            }
            _contracted = new Dictionary<string, DirectedMetaGraph>();
        }

        /// <summary>
        /// Returns true if this router db is empty.
        /// </summary>
        public bool IsEmpty
        {
            get
            {
                return _network.VertexCount == 0;
            }
        }

        /// <summary>
        /// Returns true if the given profile is supported.
        /// </summary>
        /// <returns></returns>
        public bool Supports(Profiles.Profile profile)
        {
            return _supportedProfiles.Contains(profile.Name);
        }

        /// <summary>
        /// Adds a supported profile.
        /// </summary>
        internal void AddSupportedProfile(Profiles.Profile profile)
        {
            _supportedProfiles.Add(profile.Name);
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
        internal void AddContracted(Profiles.Profile profile, DirectedMetaGraph contracted)
        {
            _contracted.Add(profile.Name, contracted);
        }

        /// <summary>
        /// Tries to get a contracted version of the routing network for the given profile.
        /// </summary>
        /// <returns></returns>
        public bool TryGetContracted(Profiles.Profile profile, out DirectedMetaGraph contracted)
        {
            return _contracted.TryGetValue(profile.Name, out contracted);
        }

        /// <summary>
        /// Returns true if this routing db has a contracted version of the routing network for the given profile.
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public bool HasContractedFor(Profiles.Profile profile)
        {
            return _contracted.ContainsKey(profile.Name);
        }
    }
}