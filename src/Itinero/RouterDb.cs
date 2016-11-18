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
using Itinero.Data.Shortcuts;

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
        private readonly Dictionary<string, Vehicle> _supportedVehicles;
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
            _dbMeta = new AttributeCollection();

            _supportedVehicles = new Dictionary<string, Vehicle>();
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
            _dbMeta = new AttributeCollection();

            _supportedVehicles = new Dictionary<string, Vehicle>();
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
            _meta = new AttributesIndex(map);
            _dbMeta = new AttributeCollection();

            _supportedVehicles = new Dictionary<string, Vehicle>();
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

            _supportedVehicles = new Dictionary<string, Vehicle>();
            foreach (var vehicle in supportedVehicles)
            {
                _supportedVehicles[vehicle.Name.ToLowerInvariant()] = vehicle;
            }
            _contracted = new Dictionary<string, ContractedDb>();
            _restrictionDbs = new Dictionary<string, RestrictionsDb>();
            _shortcutsDbs = new Dictionary<string, ShortcutsDb>();

            _guid = Guid.NewGuid();
        }

        /// <summary>
        /// Creates a new router database.
        /// </summary>
        private RouterDb(Guid guid, RoutingNetwork network, AttributesIndex profiles, AttributesIndex meta, IAttributeCollection dbMeta,
            Profiles.Vehicle[] supportedVehicles)
        {
            _guid = guid;
            _network = network;
            _edgeProfiles = profiles;
            _meta = meta;
            _dbMeta = dbMeta;

            _supportedVehicles = new Dictionary<string, Vehicle>();
            foreach (var vehicle in supportedVehicles)
            {
                _supportedVehicles[vehicle.Name.ToLowerInvariant()] = vehicle;
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
        /// Gets one if the supported vehicles.
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
        /// Gets all the names of the shortcuts databases.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetRestrictedVehicleTypes()
        {
            return _shortcutsDbs.Select(x => x.Key);
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
            long size = 1;
            stream.WriteByte(3);

            // write guid.
            stream.Write(_guid.ToByteArray(), 0, 16);
            size += 16;

            // serialize supported profiles.
            var lengthBytes = BitConverter.GetBytes(_supportedVehicles.Count);
            size += 4;
            stream.Write(lengthBytes, 0, 4);
            foreach(var vehicle in _supportedVehicles)
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

            // serialize edge profiles.
            size += _edgeProfiles.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize meta-data.
            size += _meta.Serialize(new LimitedStream(stream));
            stream.Seek(position + size, System.IO.SeekOrigin.Begin);

            // serialize network.
            size += _network.Serialize(new LimitedStream(stream));
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
            var version = stream.ReadByte();
            if (version != 1 && version != 2 && version != 3)
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
                    vehicle.Register();
                }
            }
            
            var metaDb = stream.ReadWithSizeAttributesCollection();
            var shorcutsCount = (int)0;
            if (version >= 2)
            { // when version < 1 there are no shortcuts and thus no shortcut count.
                shorcutsCount = stream.ReadByte();
            }
            var contractedCount = stream.ReadByte();
            var profiles = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var meta = AttributesIndex.Deserialize(new LimitedStream(stream), true);
            var network = RoutingNetwork.Deserialize(stream, profile == null ? null : profile.RoutingNetworkProfile);

            // create router db.
            var routerDb = new RouterDb(guid, network, profiles, meta, metaDb, supportedVehicleInstances.ToArray());

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
            return routerDb;
        }
    }
}