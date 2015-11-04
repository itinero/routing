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
using OsmSharp.Routing.Network.Data;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using OsmSharp.Routing.Attributes;
using OsmSharp.Routing.Profiles;
using System;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Represents a routing database.
    /// </summary>
    public class RouterDb
    {
        private readonly RoutingNetwork _network;
        private readonly AttributesIndex _edgeProfiles;
        private readonly AttributesIndex _meta;

        private readonly Dictionary<string, DirectedMetaGraph> _contracted;
        private readonly HashSet<string> _supportedProfiles;

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb()
        {
            _network = new RoutingNetwork(new Graphs.Geometric.GeometricGraph(1));
            _edgeProfiles = new AttributesIndex(false, true);
            _meta = new AttributesIndex();

            _supportedProfiles = new HashSet<string>();
            _contracted = new Dictionary<string, DirectedMetaGraph>();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta,
            params Profiles.Profile[] supportedProfiles)
        {
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;

            _supportedProfiles = new HashSet<string>();
            foreach(var supportedProfile in supportedProfiles)
            {
                _supportedProfiles.Add(supportedProfile.Name);
            }
            _contracted = new Dictionary<string, DirectedMetaGraph>();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        private RouterDb(RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta,
            string[] supportedProfiles)
        {
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;

            _supportedProfiles = new HashSet<string>();
            foreach (var supportedProfile in supportedProfiles)
            {
                _supportedProfiles.Add(supportedProfile);
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
        public bool Supports(Profiles.Profile profile)
        {
            return _supportedProfiles.Contains(profile.Name);
        }

        /// <summary>
        /// Adds a supported profile.
        /// </summary>
        public void AddSupportedProfile(Profiles.Profile profile)
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
        public AttributesIndex EdgeProfiles
        {
            get
            {
                return _edgeProfiles;
            }
        }

        /// <summary>
        /// Returns the meta-data index.
        /// </summary>
        public AttributesIndex EdgeMeta
        {
            get
            {
                return _meta;
            }
        }

        /// <summary>
        /// Adds a contracted version of the routing network for the given profile.
        /// </summary>
        public void AddContracted(Profiles.Profile profile, DirectedMetaGraph contracted)
        {
            _contracted.Add(profile.Name, contracted);
        }

        /// <summary>
        /// Tries to get a contracted version of the routing network for the given profile.
        /// </summary>
        public bool TryGetContracted(Profiles.Profile profile, out DirectedMetaGraph contracted)
        {
            return _contracted.TryGetValue(profile.Name, out contracted);
        }

        /// <summary>
        /// Returns true if this routing db has a contracted version of the routing network for the given profile.
        /// </summary>
        public bool HasContractedFor(Profiles.Profile profile)
        {
            return _contracted.ContainsKey(profile.Name);
        }

        /// <summary>
        /// Saves the database to the given stream.
        /// </summary>
        public long Serialize(Stream stream)
        {
            var position = stream.Position;

            // serialize supported profiles.
            var size = stream.WriteWithSize(_supportedProfiles.ToArray());

            // serialize the # of contracted profiles.
            if(_contracted.Count > byte.MaxValue)
            {
                throw new Exception("Cannot serialize a router db with more than 255 contracted graphs.");
            }
            stream.WriteByte((byte)_contracted.Count);
            size += 1;

            // serialize profiles.
            size += _edgeProfiles.Serialize(new OsmSharp.IO.LimitedStream(stream));
            stream.Seek(position + size, SeekOrigin.Begin);

            // serialize meta-data.
            size += _meta.Serialize(new OsmSharp.IO.LimitedStream(stream));
            stream.Seek(position + size, SeekOrigin.Begin);

            // serialize network.
            size += _network.Serialize(new OsmSharp.IO.LimitedStream(stream));
            stream.Seek(position + size, SeekOrigin.Begin);

            // serialize all contracted networks.
            foreach(var contracted in _contracted)
            {
                size += stream.WriteWithSize(contracted.Key);
                size += contracted.Value.Serialize(
                    new OsmSharp.IO.LimitedStream(stream));
            }
            return size;
        }

        /// <summary>
        /// Deserializes a database from the given stream.
        /// </summary>
        public static RouterDb Deserialize(Stream stream, RouterDbProfile profile)
        {
            // deserialize all basic data.
            var supportedProfiles = stream.ReadWithSizeStringArray();
            var contractedCount = stream.ReadByte();
            var profiles = AttributesIndex.Deserialize(new OsmSharp.IO.LimitedStream(stream), true);
            var meta = AttributesIndex.Deserialize(new OsmSharp.IO.LimitedStream(stream), true);
            var network = RoutingNetwork.Deserialize(stream, profile == null ? null : profile.RoutingNetworkProfile);

            // create router db.
            var routerDb = new RouterDb(network, profiles, meta, supportedProfiles);
            
            // read all contracted versions.
            for (var i = 0; i < contractedCount; i++)
            {
                var profileName = stream.ReadWithSizeString();
                var contracted = DirectedMetaGraph.Deserialize(stream, true);
                routerDb._contracted[profileName] = contracted;
            }
            return routerDb;
        }
    }
}