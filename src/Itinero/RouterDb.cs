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
using Itinero.Data.Shortcuts;
using Itinero.Data.Network.Edges;

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
        private readonly MappedAttributesIndex _metaVertex;
        private readonly MetaCollectionDb _vertexData;
        private readonly MetaCollectionDb _edgeData;
        private readonly IAttributeCollection _dbMeta;
        private Guid _guid;
        private readonly Dictionary<string, ContractedDb> _contracted;
        private readonly Dictionary<string, Vehicle> _supportedVehicles;
        private readonly Dictionary<string, Profile> _supportedProfiles;
        private readonly Dictionary<string, RestrictionsDb> _restrictionDbs;
        private readonly Dictionary<string, ShortcutsDb> _shortcutsDbs;

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(float maxEdgeDistance = Constants.DefaultMaxEdgeDistance)
        {
            _network = new RoutingNetwork(new Graphs.Geometric.GeometricGraph(1), maxEdgeDistance);
            _edgeProfiles = new AttributesIndex(AttributesIndexMode.IncreaseOne
                | AttributesIndexMode.ReverseAll);
            _meta = new AttributesIndex(AttributesIndexMode.ReverseStringIndexKeysOnly);
            _metaVertex = new MappedAttributesIndex();
            _vertexData = new MetaCollectionDb();
            _edgeData = new MetaCollectionDb();
            _dbMeta = new AttributeCollection();

            _supportedVehicles = new Dictionary<string, Vehicle>();
            _supportedProfiles = new Dictionary<string, Profile>();
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
            _shortcutsDbs = new Dictionary<string, ShortcutsDb>();

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
            _metaVertex = new MappedAttributesIndex();
            _vertexData = new MetaCollectionDb();
            _edgeData = new MetaCollectionDb();
            _dbMeta = new AttributeCollection();

            _supportedVehicles = new Dictionary<string, Vehicle>();
            _supportedProfiles = new Dictionary<string, Profile>();
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
            _shortcutsDbs = new Dictionary<string, ShortcutsDb>();

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
            _metaVertex = new MappedAttributesIndex();
            _vertexData = new MetaCollectionDb();
            _edgeData = new MetaCollectionDb();
            _meta = new AttributesIndex(map);
            _dbMeta = new AttributeCollection();

            _supportedVehicles = new Dictionary<string, Vehicle>();
            _supportedProfiles = new Dictionary<string, Profile>();
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
            _shortcutsDbs = new Dictionary<string, ShortcutsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        public RouterDb(RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta, IAttributeCollection dbMeta,
            params Profiles.Vehicle[] supportedVehicles)
        {
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;
            _dbMeta = dbMeta;

            _metaVertex = new MappedAttributesIndex();
            _vertexData = new MetaCollectionDb();
            _edgeData = new MetaCollectionDb();

            _supportedVehicles = new Dictionary<string, Vehicle>();
            _supportedProfiles = new Dictionary<string, Profile>();
            foreach (var vehicle in supportedVehicles)
            {
                _supportedVehicles[vehicle.Name.ToLowerInvariant()] = vehicle;
                foreach (var profile in vehicle.GetProfiles())
                {
                    _supportedProfiles[profile.FullName.ToLowerInvariant()] = profile;
                }
            }
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
            _shortcutsDbs = new Dictionary<string, ShortcutsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        private RouterDb(Guid guid, RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta, MappedAttributesIndex metaVertex, 
            MetaCollectionDb vertexData, MetaCollectionDb edgeData, IAttributeCollection dbMeta, Profiles.Vehicle[] supportedVehicles)
        {
            _guid = guid;
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;
            _metaVertex = metaVertex;
            _vertexData = vertexData;
            _edgeData = edgeData;
            _dbMeta = dbMeta;

            _supportedVehicles = new Dictionary<string, Vehicle>();
            _supportedProfiles = new Dictionary<string, Profile>();
            foreach (var vehicle in supportedVehicles)
            {
                _supportedVehicles[vehicle.Name.ToLowerInvariant()] = vehicle;
                foreach (var profile in vehicle.GetProfiles())
                {
                    _supportedProfiles[profile.FullName.ToLowerInvariant()] = profile;
                }
            }
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
            _shortcutsDbs = new Dictionary<string, ShortcutsDb>();
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
        /// Returns true if the given vehicle is supported.
        /// </summary>
        public bool Supports(string vehicleName)
        {
            return _supportedVehicles.ContainsKey(vehicleName.ToLowerInvariant());
        }

        /// <summary>
        /// Gets one of the supported vehicles.
        /// </summary>
        public Vehicle GetSupportedVehicle(string vehicleName)
        {
            return _supportedVehicles[vehicleName.ToLowerInvariant()];
        }

        /// <summary>
        /// Gets all the supported vehicle.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Vehicle> GetSupportedVehicles()
        {
            return _supportedVehicles.Values;
        }

        /// <summary>
        /// Adds a supported vehicle.
        /// </summary>
        public void AddSupportedVehicle(Profiles.Vehicle vehicle)
        {
            _supportedVehicles[vehicle.Name.ToLowerInvariant()] = vehicle;
            foreach(var profile in vehicle.GetProfiles())
            {
                _supportedProfiles[profile.FullName.ToLowerInvariant()] = profile;
            }
        }

        /// <summary>
        /// Returns true if the profile with the given name is supported.
        /// </summary>
        public bool SupportProfile(string profileName)
        {
            return _supportedProfiles.ContainsKey(profileName);
        }

        /// <summary>
        /// Gets one of the supported vehicles.
        /// </summary>
        public Profile GetSupportedProfile(string profileName)
        {
            return _supportedProfiles[profileName.ToLowerInvariant()];
        }

        /// <summary>
        /// Gets one of the supported profiles.
        /// </summary>
        public IEnumerable<Profile> GetSupportedProfiles()
        {
            foreach (var vehicle in this.GetSupportedVehicles())
            {
                foreach (var profile in vehicle.GetProfiles())
                {
                    yield return profile;
                }
            }
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
                foreach (var kv in _restrictionDbs)
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
        /// Gets or sets the vertex data.
        /// </summary>
        public MetaCollectionDb VertexData
        {
            get
            {
                return _vertexData;
            }
        }

        /// <summary>
        /// Gets or sets the edge data.
        /// </summary>
        public MetaCollectionDb EdgeData
        {
            get
            {
                return _edgeData;
            }
        }

        /// <summary>
        /// Returns the vertex meta-date index.
        /// </summary>
        public MappedAttributesIndex VertexMeta
        {
            get
            {
                return _metaVertex;
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
            _contracted[profile.FullName] = contracted;
        }

        /// <summary>
        /// Removes the contracted version of the routing network for the given profile.
        /// </summary>
        public bool RemoveContracted(Profile profile)
        {
            return _contracted.Remove(profile.FullName);
        }

        /// <summary>
        /// Tries to get a contracted version of the routing network for the given profile.
        /// </summary>
        public bool TryGetContracted(Profiles.Profile profile, out ContractedDb contracted)
        {
            return _contracted.TryGetValue(profile.FullName, out contracted);
        }

        /// <summary>
        /// Gets all the profiles that have contracted db's.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetContractedProfiles()
        {
            return _contracted.Select(x => x.Key);
        }

        /// <summary>
        /// Returns true if this routing db has a contracted version of the routing network for the given profile.
        /// </summary>
        public bool HasContractedFor(Profiles.Profile profile)
        {
            return _contracted.ContainsKey(profile.FullName);
        }

        /// <summary>
        /// Adds a shortcuts db.
        /// </summary>
        public void AddShortcuts(string name, ShortcutsDb shortcutsDb)
        {
            _shortcutsDbs[name] = shortcutsDb;
        }

        /// <summary>
        /// Removes a shortcuts db.
        /// </summary>
        public bool RemoveShortcuts(string name)
        {
            return _shortcutsDbs.Remove(name);
        }

        /// <summary>
        /// Tries to get a shortcuts db.
        /// </summary>
        public bool TryGetShortcuts(string name, out ShortcutsDb shortcutsDb)
        {
            return _shortcutsDbs.TryGetValue(name, out shortcutsDb);
        }

        /// <summary>
        /// Gets the names of the restricted vehicle types.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetRestrictedVehicleTypes()
        {
            return _restrictionDbs.Select(x => x.Key);
        }

        /// <summary>
        /// Returns true if there are shortcuts in this database.
        /// </summary>
        public bool HasShortcuts
        {
            get
            {
                return _shortcutsDbs.Count > 0;
            }
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
        /// Removes the restrictions for the given vehicle type.
        /// </summary>
        public bool RemoveRestrictions(string vehicleType)
        {
            return _restrictionDbs.Remove(vehicleType);
        }

        /// <summary>
        /// Compresses the network and rearranges all id's as needed.
        /// </summary>
        public void Compress()
        {
            _network.Compress((originalId, newId) => 
            {
                if (_edgeData != null)
                {
                    _edgeData.Switch(originalId, newId);
                }
            });
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
            // version1: OsmSharp.Routing state of layout.
            // version2: Added ShortcutsDbs.
            // version3: Add advanced profile serialization.
            // version4: Added missing restriction dbs.
            // version5: Added new dual edge-based contracted graph.
            // version6: Added vertex meta-data.
            // version7: Added support for shorts in vertex meta-data.
            // version8: Added edge meta-data.
            // version9: Writable attribute indexes.
            long size = 1;
            stream.WriteByte(9);

            // write guid.
            stream.Write(_guid.ToByteArray(), 0, 16);
            size += 16;

            // serialize supported profiles.
            var lengthBytes = BitConverter.GetBytes(_supportedVehicles.Count);
            size += 4;
            stream.Write(lengthBytes, 0, 4);
            foreach (var vehicle in _supportedVehicles)
            {
                size += vehicle.Value.Serialize(stream);
            }

            // serialize the db-meta.
            size += _dbMeta.WriteWithSize(stream);

            // serialize the # of shortcutsdbs profiles.
            if (_shortcutsDbs.Count > byte.MaxValue)
            {
                throw new Exception("Cannot serialize a router db with more than 255 shortcut dbs.");
            }
            stream.WriteByte((byte)_shortcutsDbs.Count);
            size += 1;

            // serialize the # of contracted profiles.
            if (_contracted.Count > byte.MaxValue)
            {
                throw new Exception("Cannot serialize a router db with more than 255 contracted graphs.");
            }
            stream.WriteByte((byte)_contracted.Count);
            size += 1;

            // serialize the # of restriction dbs.
            if (_restrictionDbs.Count > byte.MaxValue)
            {
                throw new Exception("Cannot serialize a router db with more than 255 restriction dbs.");
            }
            stream.WriteByte((byte)_restrictionDbs.Count);
            size += 1;

            // serialize edge profiles.
            size += _edgeProfiles.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize meta-data.
            size += _meta.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize vertex meta-data.
            size += _metaVertex.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize vertex data.
            size += _vertexData.Serialize(stream);
            stream.Seek(position + size, SeekOrigin.Begin);

            // serialize edge data.
            size += _edgeData.Serialize(stream);
            stream.Seek(position + size, SeekOrigin.Begin);

            // serialize network.
            size += _network.Serialize(new LimitedStream(stream), (originalId, newId) => 
            {
                if (_edgeData != null)
                {
                    _edgeData.Switch(originalId, newId);
                }
            });
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize all shortcut dbs.
            foreach (var shortcutsDb in _shortcutsDbs)
            {
                size += stream.WriteWithSize(shortcutsDb.Key);
                size += shortcutsDb.Value.Serialize(
                    new LimitedStream(stream));
            }

            // serialize all contracted networks.
            foreach (var contracted in _contracted)
            {
                size += stream.WriteWithSize(contracted.Key);
                size += contracted.Value.Serialize(
                    new LimitedStream(stream), toReadonly);
            }

            // serialize all restriction dbs.
            foreach (var restrictionDb in _restrictionDbs)
            {
                size += stream.WriteWithSize(restrictionDb.Key);
                size += restrictionDb.Value.Serialize(stream);
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
                throw new Exception(string.Format("Contracted graph for profile {0} not found.", profile.FullName));
            }

            // write: guid, name and data.

            var guid = this.Guid;
            long size = 16;
            stream.Write(guid.ToByteArray(), 0, 16);
            size += stream.WriteWithSize(profile.FullName);
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
            // version1: OsmSharp.Routing state of layout.
            // version2: Added ShortcutsDbs.
            // version3: Add advanced profile serialization.
            // version4: Added missing restriction dbs.
            // version5: Added new dual edge-based contracted graph.
            // version6: Added vertex meta-data.
            // version7: Added support for shorts in vertex meta-data.
            // version8: Added edge meta-data.
            // version9: Writable attribute indexes.
            var version = stream.ReadByte();
            if (version != 1 && version != 2 && version != 3 && version != 4 && version != 5 && version != 6 && 
                version != 7 && version != 8 && version != 9)
            {
                throw new Exception(string.Format("Cannot deserialize routing db: Invalid version #: {0}.", version));
            }

            var guidBytes = new byte[16];
            stream.Read(guidBytes, 0, 16);
            var guid = new Guid(guidBytes);

            var supportedVehicleInstances = new List<Vehicle>();
            if (version <= 2)
            { // just contains vehicle names.
                var supportedVehicles = stream.ReadWithSizeStringArray();
                foreach (var vehicleName in supportedVehicles)
                {
                    Profile vehicleProfile;
                    if (Profile.TryGet(vehicleName, out vehicleProfile))
                    {
                        supportedVehicleInstances.Add(vehicleProfile.Parent);
                    }
                    else
                    {
                        Itinero.Logging.Logger.Log("RouterDb", Logging.TraceEventType.Warning, "Vehicle with name {0} was not found, register all vehicle profiles before deserializing the router db.",
                            vehicleName);
                    }
                }
            }
            else
            { // contains the full vehicles.
                var lengthBytes = new byte[4];
                stream.Read(lengthBytes, 0, 4);
                var size = BitConverter.ToInt32(lengthBytes, 0);
                for(var i = 0; i < size; i++)
                {
                    var vehicle = Vehicle.Deserialize(stream);
                    supportedVehicleInstances.Add(vehicle);
                }
            }
            
            var metaDb = stream.ReadWithSizeAttributesCollection();
            var shorcutsCount = (int)0;
            if (version >= 2)
            { // when version < 1 there are no shortcuts and thus no shortcut count.
                shorcutsCount = stream.ReadByte();
            }
            var contractedCount = stream.ReadByte();

            var restrictionDbCount = 0;
            if (version >= 4)
            {
                restrictionDbCount = stream.ReadByte();
            }

            var profiles = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var meta = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            MappedAttributesIndex metaVertex = null;
            MetaCollectionDb vertexData = null;
            if (version >= 6)
            {
                metaVertex = MappedAttributesIndex.Deserialize(new LimitedStream(stream), profile == null ? null : profile.VertexMetaProfile);
                vertexData = MetaCollectionDb.Deserialize(new LimitedStream(stream), profile == null ? null : profile.VertexDataProfile);
            }
            MetaCollectionDb edgeData = null;
            if (version >= 8)
            {
                edgeData = MetaCollectionDb.Deserialize(new LimitedStream(stream), profile == null ? null : profile.VertexDataProfile);
            }
            var network = RoutingNetwork.Deserialize(stream, profile == null ? null : profile.RoutingNetworkProfile);

            // create router db.
            var routerDb = new RouterDb(guid, network, profiles, meta, metaVertex, vertexData, edgeData, metaDb, supportedVehicleInstances.ToArray());

            // read all shortcut dbs.
            for (var i = 0; i < shorcutsCount; i++)
            {
                var shortcutsName = stream.ReadWithSizeString();
                var shorcutsDb = ShortcutsDb.Deserialize(stream);
                routerDb._shortcutsDbs[shortcutsName] = shorcutsDb;
            }

            // read all contracted versions.
            for (var i = 0; i < contractedCount; i++)
            {
                var profileName = stream.ReadWithSizeString();
                var contracted = ContractedDb.Deserialize(stream, profile == null ? null : profile.ContractedDbProfile);
                routerDb._contracted[profileName] = contracted;
            }

            // read all restriction dbs.
            for (var i = 0; i < restrictionDbCount; i++)
            {
                var restrictionDbName = stream.ReadWithSizeString();
                var restrictionDb = RestrictionsDb.Deserialize(stream, profile == null ? null : profile.RestrictionDbProfile);
                routerDb._restrictionDbs[restrictionDbName] = restrictionDb;
            }

            return routerDb;
        }
    }
}