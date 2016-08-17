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

using Itinero.Attributes;
using Itinero.Data.Network;
using Itinero.Data.Network.Restrictions;
using Itinero.Profiles;
using Reminiscence.IO;
using Reminiscence.IO.Streams;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Itinero.Data.Contracted;

namespace Itinero
{
    /// <summary>
    /// Represents a routing database.
    /// </summary>
    public class RouterDb
    {
        private readonly RoutingNetwork _network;
        private readonly AttributesIndex _edgeProfiles;
        private readonly AttributesIndex _meta;
        private readonly IAttributeCollection _dbMeta;
        private Guid _guid;

        private readonly Dictionary<string, ContractedDb> _contracted;
        private readonly HashSet<string> _supportedProfiles;
        private readonly Dictionary<string, RestrictionsDb> _restrictionDbs;

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _network = new RoutingNetwork(new Graphs.Geometric.GeometricGraph(1), maxEdgeDistance);
            _edgeProfiles = new AttributesIndex(AttributesIndexMode.IncreaseOne 
                | AttributesIndexMode.ReverseAll);
            _meta = new AttributesIndex(AttributesIndexMode.ReverseStringIndexKeysOnly);
            _dbMeta = new AttributeCollection();

            _supportedProfiles = new HashSet<string>();
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(MemoryMap map, float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _network = new RoutingNetwork(map, RoutingNetworkProfile.NoCache, maxEdgeDistance);
            _edgeProfiles = new AttributesIndex(AttributesIndexMode.IncreaseOne
                | AttributesIndexMode.ReverseAll);
            _meta = new AttributesIndex(map, AttributesIndexMode.ReverseStringIndexKeysOnly);
            _dbMeta = new AttributeCollection();

            _supportedProfiles = new HashSet<string>();
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(MemoryMap map, RoutingNetworkProfile profile, float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _network = new RoutingNetwork(map, profile, maxEdgeDistance);
            _edgeProfiles = new AttributesIndex(map, AttributesIndexMode.IncreaseOne | 
                AttributesIndexMode.ReverseCollectionIndex | AttributesIndexMode.ReverseStringIndex);
            _meta = new AttributesIndex(map);
            _dbMeta = new AttributeCollection();

            _supportedProfiles = new HashSet<string>();
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta, IAttributeCollection dbMeta,
            params Profiles.Profile[] supportedProfiles)
        {
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;
            _dbMeta = dbMeta;

            _supportedProfiles = new HashSet<string>();
            foreach (var supportedProfile in supportedProfiles)
            {
                _supportedProfiles.Add(supportedProfile.Name);
            }
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        private RouterDb(Guid guid, RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta, IAttributeCollection dbMeta,
            string[] supportedProfiles)
        {
            _guid = guid;
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;
            _dbMeta = dbMeta;

            _supportedProfiles = new HashSet<string>();
            foreach (var supportedProfile in supportedProfiles)
            {
                _supportedProfiles.Add(supportedProfile);
            }
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
        }

        /// <summary>
        /// Returns the guid for this db.
        /// </summary>
        public Guid Guid
        {
            get
            {
                return _guid;
            }
        }

        /// <summary>
        /// Generates a new guid.
        /// </summary>
        /// <remarks>To use then the network was changed externally and was already writting to disk before.</remarks>
        public void NewGuid()
        {
            _guid = Guid.NewGuid();
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
        /// Gets all restriction dbs.
        /// </summary>
        public IEnumerable<RestrictionsDbMeta> RestrictionDbs
        {
            get
            {
                foreach(var kv in _restrictionDbs)
                {
                    yield return new RestrictionsDbMeta(kv.Key, kv.Value);
                }
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
        /// Gets the meta-data collection.
        /// </summary>
        public IAttributeCollection Meta
        {
            get
            {
                return _dbMeta;
            }
        }

        /// <summary>
        /// Returns true if there is at least one contracted version of the network.
        /// </summary>
        public bool HasContracted
        {
            get
            {
                return _contracted.Count > 0;
            }
        }

        /// <summary>
        /// Adds a contracted version of the routing network for the given profile.
        /// </summary>
        public void AddContracted(Profiles.Profile profile, ContractedDb contracted)
        {
            _contracted[profile.Name] = contracted;
        }

        /// <summary>
        /// Removes the contracted version of the routing network for the given profile.
        /// </summary>
        public bool RemoveContracted(Profile profile)
        {
            return _contracted.Remove(profile.Name);
        }

        /// <summary>
        /// Tries to get a contracted version of the routing network for the given profile.
        /// </summary>
        public bool TryGetContracted(Profiles.Profile profile, out ContractedDb contracted)
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
        /// Returns true if there are restrictions in this database.
        /// </summary>
        public bool HasRestrictions
        {
            get
            {
                return _restrictionDbs.Count > 0;
            }
        }

        /// <summary>
        /// Returns true if this routing db has a restriction db for the given vehicle type.
        /// </summary>
        public bool TryGetRestrictions(string vehicleType, out RestrictionsDb restrictions)
        {
            return _restrictionDbs.TryGetValue(vehicleType, out restrictions);
        }

        /// <summary>
        /// Adds the restrictions for the given vehicle type.
        /// </summary>
        public void AddRestrictions(string vehicleType, RestrictionsDb restrictions)
        {
            _restrictionDbs[vehicleType] = restrictions;
        }

        /// <summary>
        /// Writes this database to the given stream.
        /// </summary>
        public long Serialize(Stream stream)
        {
            return this.Serialize(stream, true);
        }

        /// <summary>
        /// Writes this database to the given stream.
        /// </summary>
        public long Serialize(Stream stream, bool toReadonly)
        {
            var position = stream.Position;

            // write version #.
            long size = 1;
            stream.WriteByte(1);

            // write guid.
            stream.Write(_guid.ToByteArray(), 0, 16);
            size += 16;

            // serialize supported profiles.
            size += stream.WriteWithSize(_supportedProfiles.ToArray());

            // serialize the db-meta.
            size += _dbMeta.WriteWithSize(stream);

            // serialize the # of contracted profiles.
            if(_contracted.Count > byte.MaxValue)
            {
                throw new Exception("Cannot serialize a router db with more than 255 contracted graphs.");
            }
            stream.WriteByte((byte)_contracted.Count);
            size += 1;

            // serialize profiles.
            size += _edgeProfiles.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize meta-data.
            size += _meta.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize network.
            size += _network.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize all contracted networks.
            foreach(var contracted in _contracted)
            {
                size += stream.WriteWithSize(contracted.Key);
                size += contracted.Value.Serialize(
                    new LimitedStream(stream), toReadonly);
            }
            return size;
        }

        /// <summary>
        /// Writes the contracted graph for the given profile to the given stream.
        /// </summary>
        public long SerializeContracted(Profile profile, Stream stream)
        {
            ContractedDb contracted;
            if (!this.TryGetContracted(profile, out contracted))
            {
                throw new Exception(string.Format("Contracted graph for profile {0} not found.", profile.Name));
            }

            // write: guid, name and data.

            var guid = this.Guid;
            long size = 16;
            stream.Write(guid.ToByteArray(), 0, 16);
            size += stream.WriteWithSize(profile.Name);
            size += contracted.Serialize(stream, true);
            return size;
        }

        /// <summary>
        /// Reads a contracted graph from the given stream and adds it to this db.
        /// </summary>
        public void DeserializeAndAddContracted(Stream stream)
        {
            this.DeserializeAndAddContracted(stream, null);
        }

        /// <summary>
        /// Reads a contracted graph from the given stream and adds it to this db.
        /// </summary>
        public void DeserializeAndAddContracted(Stream stream, ContractedDbProfile profile)
        {
            // first read and compare guids.
            var guidBytes = new byte[16];
            stream.Read(guidBytes, 0, 16);
            var guid = new Guid(guidBytes);
            if (guid != this.Guid)
            {
                throw new Exception("Cannot add this contracted graph, guid's do not match.");
            }
            var profileName = stream.ReadWithSizeString();
            var contracted = ContractedDb.Deserialize(stream, profile);
            _contracted[profileName] = contracted;
        }

        /// <summary>
        /// Deserializes a database from the given stream.
        /// </summary>
        public static RouterDb Deserialize(Stream stream)
        {
            return RouterDb.Deserialize(stream, null);
        }

        /// <summary>
        /// Deserializes a database from the given stream.
        /// </summary>
        public static RouterDb Deserialize(Stream stream, RouterDbProfile profile)
        {
            // deserialize all basic data.
            var version = stream.ReadByte();
            if (version != 1)
            {
                throw new Exception(string.Format("Cannot deserialize routing db: Invalid version #: {0}.", version));
            }

            var guidBytes = new byte[16];
            stream.Read(guidBytes, 0, 16);
            var guid = new Guid(guidBytes);

            var supportedProfiles = stream.ReadWithSizeStringArray();
            var metaDb = stream.ReadWithSizeAttributesCollection();
            var contractedCount = stream.ReadByte();
            var profiles = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var meta = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var network = RoutingNetwork.Deserialize(stream, profile == null ? null : profile.RoutingNetworkProfile);

            // create router db.
            var routerDb = new RouterDb(guid, network, profiles, meta, metaDb, supportedProfiles);
            
            // read all contracted versions.
            for (var i = 0; i < contractedCount; i++)
            {
                var profileName = stream.ReadWithSizeString();
                var contracted = ContractedDb.Deserialize(stream, profile == null ? null : profile.ContractedDbProfile);
                routerDb._contracted[profileName] = contracted;
            }
            return routerDb;
        }
    }
}