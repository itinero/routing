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
using Itinero.Data;
using Itinero.Data.Network;
using Itinero.Data.Network.Edges;
using Itinero.Data.Network.Restrictions;
using Itinero.IO.Osm.Nodes;
using Itinero.IO.Osm.Normalizer;
using Itinero.IO.Osm.Relations;
using Itinero.IO.Osm.Restrictions;
using Itinero.LocalGeo;
using Itinero.Profiles;
using OsmSharp;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A stream target to load a routing database.
    /// </summary>
    public class RouterDbStreamTarget : OsmStreamTarget
    {
        private readonly RouterDb _db;
        private readonly VehicleCache _vehicleCache;
        private readonly bool _allCore;
        private readonly NodeIndex _nodeIndex;
        private readonly HashSet<string> _vehicleTypes;
        private readonly float _simplifyEpsilonInMeter;

        /// <summary>
        /// Creates a new router db stream target.
        /// </summary>
        public RouterDbStreamTarget(RouterDb db, Vehicle[] vehicles, bool allCore = false,
            int minimumStages = 1, IEnumerable<ITwoPassProcessor> processors = null, bool processRestrictions = false,
            float simplifyEpsilonInMeter = .1f)
            : this(db, new VehicleCache(vehicles), allCore, minimumStages, processors, processRestrictions, simplifyEpsilonInMeter)
        {

        }

        /// <summary>
        /// Creates a new router db stream target.
        /// </summary>
        public RouterDbStreamTarget(RouterDb db, VehicleCache vehicleCache, bool allCore = false,
            int minimumStages = 1, IEnumerable<ITwoPassProcessor> processors = null, bool processRestrictions = false,
            float simplifyEpsilonInMeter = .1f)
        {
            _db = db;
            _allCore = allCore;
            _simplifyEpsilonInMeter = simplifyEpsilonInMeter;

            _vehicleCache = vehicleCache;
            _vehicleTypes = new HashSet<string>();
            _nodeIndex = new NodeIndex();

            foreach (var vehicle in _vehicleCache.Vehicles)
            {
                foreach (var vehicleType in vehicle.VehicleTypes)
                {
                    _vehicleTypes.Add(vehicleType);
                }
            }
            
            foreach (var vehicle in _vehicleCache.Vehicles)
            {
                db.AddSupportedVehicle(vehicle);
            }

            if (processors == null)
            {
                processors = new List<ITwoPassProcessor>();
            }
            this.Processors = new List<ITwoPassProcessor>(processors);

            this.InitializeDefaultProcessors(processRestrictions);
        }

        private bool _firstPass = true; // flag for first/second pass.
        private MetaCollection<long> _nodeData = null;

        /// <summary>
        /// Setups default add-on processors.
        /// </summary>
        private void InitializeDefaultProcessors(bool processRestrictions)
        {
            // add all vehicle relation processors.
            foreach(var vehicle in _vehicleCache.Vehicles)
            {
                if (vehicle is Itinero.Profiles.DynamicVehicle)
                {
                    var dynamicVehicle = vehicle as Itinero.Profiles.DynamicVehicle;
                    this.Processors.Add(new DynamicVehicleRelationTagProcessor(dynamicVehicle));
                    this.Processors.Add(new DynamicVehicleNodeTagProcessor(_db, dynamicVehicle, (node) =>
                    {
                        var index = _nodeIndex.TryGetIndex(node.Id.Value);
                        if (index == long.MaxValue)
                        { // node is not a vertex.
                            return Constants.NO_VERTEX;
                        }
                        return this.AddCoreNode(node.Id.Value, node.Latitude.Value, node.Longitude.Value);
                    }));
                }
            }

            // add restriction processor if needed.
            if (processRestrictions)
            {
                this.Processors.Add(new RestrictionProcessor(_vehicleTypes, (node) =>
                {
                    uint vertex;
                    if (!_nodeIndex.TryGetCoreNode(node, out vertex))
                    {
                        return uint.MaxValue;
                    }
                    return vertex;
                },
                (node) =>
                {
                    var index = _nodeIndex.TryGetIndex(node.Id.Value);
                    if (index == long.MaxValue)
                    { // node is not a vertex.
                        return Constants.NO_VERTEX;
                    }
                    return this.AddCoreNode(node.Id.Value, node.Latitude.Value, node.Longitude.Value);
                },
                (vehicleType, sequence) =>
                {
                    if (vehicleType == null)
                    {
                        vehicleType = string.Empty;
                    }
                    RestrictionsDb restrictions;
                    if (!_db.TryGetRestrictions(vehicleType, out restrictions))
                    {
                        restrictions = new RestrictionsDb();
                        _db.AddRestrictions(vehicleType, restrictions);
                    }

                    restrictions.Add(sequence.ToArray());
                }));
            }
        }

        /// <summary>
        /// Intializes this target.
        /// </summary>
        public override void Initialize()
        {
            _firstPass = true;
            
            if (this.KeepNodeIds)
            {
                _nodeData = _db.VertexData.AddInt64("node_ids");
            }
        }

        /// <summary>
        /// Called right before pull and right after initialization.
        /// </summary>
        /// <returns></returns>
        public override bool OnBeforePull()
        {
            // execute the first pass.
            this.DoPull(true, false, false);

            // move to second pass.
            _firstPass = false;
            _nodeIndex.SortAndConvertIndex();
            this.Source.Reset();
            this.DoPull();

            return false;
        }

        /// <summary>
        /// Gets or sets extra two-pass processors.
        /// </summary>
        public List<ITwoPassProcessor> Processors { get; set; }

        /// <summary>
        /// Gets the vehicle cache.
        /// </summary>
        public VehicleCache VehicleCache
        {
            get
            {
                return _vehicleCache;
            }
        }

        /// <summary>
        /// Gets or sets a flag to keep node id's.
        /// </summary>
        public bool KeepNodeIds { get; set; }

        /// <summary>
        /// Registers the source.
        /// </summary>
        public virtual void RegisterSource(OsmStreamSource source, bool filterNonRoutingTags)
        {
            if (filterNonRoutingTags)
            { // add filtering.
                var eventsFilter = new OsmSharp.Streams.Filters.OsmStreamFilterDelegate();
                eventsFilter.MoveToNextEvent += (osmGeo, param) =>
                {
                    if (osmGeo.Type == OsmGeoType.Way)
                    {
                        // normalize tags, reduce the combination of tags meaning the same thing.
                        var tags = osmGeo.Tags.ToAttributes();
                        var normalizedTags = new AttributeCollection();
                        if (!DefaultTagNormalizer.Normalize(tags, normalizedTags, _vehicleCache))
                        { // invalid data, no access, or tags make no sense at all.
                            return osmGeo;
                        }

                        // rewrite tags and keep whitelisted meta-tags.
                        osmGeo.Tags.Clear();
                        foreach (var tag in normalizedTags)
                        {
                            osmGeo.Tags.Add(tag.Key, tag.Value);
                        }
                        foreach (var tag in tags)
                        {
                            if (_vehicleCache.Vehicles.IsOnMetaWhiteList(tag.Key))
                            {
                                osmGeo.Tags.Add(tag.Key, tag.Value);
                            }
                        }
                    }
                    return osmGeo;
                };
                eventsFilter.RegisterSource(source);

                base.RegisterSource(eventsFilter);
            }
            else
            { // no filtering.
                base.RegisterSource(source);
            }
        }

        /// <summary>
        /// Registers the source.
        /// </summary>
        public override void RegisterSource(OsmStreamSource source)
        {
            this.RegisterSource(source, true);
        }

        /// <summary>
        /// Adds a node.
        /// </summary>
        public override void AddNode(Node node)
        {
            if (_firstPass)
            {
                //if (this.Processors != null)
                //{
                //    foreach (var processor in this.Processors)
                //    {
                //        processor.FirstPass(node);
                //    }
                //}
            }
            else
            {
                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.SecondPass(node);
                    }
                }

                // check if the node is a routing node and if yes, store it's coordinate.
                var index = _nodeIndex.TryGetIndex(node.Id.Value);
                if (index != long.MaxValue)
                { // node is a routing node, store it's coordinates.
                    _nodeIndex.SetIndex(index, node.Latitude.Value, node.Longitude.Value);
                }
            }
        }

        /// <summary>
        /// Adds a way.
        /// </summary>
        public override void AddWay(Way way)
        {
            if (way == null) { return; }
            if (way.Nodes == null) { return; }
            if (way.Nodes.Length == 0) { return; }
            if (way.Tags == null || way.Tags.Count == 0) { return; }

            if (_firstPass)
            { // just keep.
                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.FirstPass(way);
                    }
                }

                if (_vehicleCache.AnyCanTraverse(way.Tags.ToAttributes()))
                { // way has some use, add all of it's nodes to the index.
                    _nodeIndex.AddId(way.Nodes[0]);
                    for (var i = 0; i < way.Nodes.Length; i++)
                    {
                        _nodeIndex.AddId(way.Nodes[i]);
                    }
                    _nodeIndex.AddId(way.Nodes[way.Nodes.Length - 1]);
                }
            }
            else
            {
                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.SecondPass(way);
                    }
                }
                
                var wayAttributes = way.Tags.ToAttributes();
                var profileWhiteList = new Whitelist();
                if (_vehicleCache.AddToWhiteList(wayAttributes, profileWhiteList))
                { // way has some use.
                    // build profile and meta-data.
                    var profileTags = new AttributeCollection();
                    var metaTags = new AttributeCollection();
                    foreach (var tag in way.Tags)
                    {
                        if (profileWhiteList.Contains(tag.Key) ||
                            _vehicleCache.Vehicles.IsOnProfileWhiteList(tag.Key))
                        {
                            profileTags.Add(tag);
                        }
                        if (_vehicleCache.Vehicles.IsOnMetaWhiteList(tag.Key))
                        {
                            metaTags.Add(tag);
                        }
                    }

                    // get profile and meta-data id's.
                    var profileCount = _db.EdgeProfiles.Count;
                    var profile = _db.EdgeProfiles.Add(profileTags);
                    if (profileCount != _db.EdgeProfiles.Count)
                    {
                        var stringBuilder = new StringBuilder();
                        foreach(var att in profileTags)
                        {
                            stringBuilder.Append(att.Key);
                            stringBuilder.Append('=');
                            stringBuilder.Append(att.Value);
                            stringBuilder.Append(' ');
                        }
                        Itinero.Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Information,
                            "Normalized: # profiles {0}: {1}", _db.EdgeProfiles.Count, stringBuilder.ToInvariantString());
                    }
                    if (profile > Data.Edges.EdgeDataSerializer.MAX_PROFILE_COUNT)
                    {
                        throw new Exception("Maximum supported profiles exeeded, make sure only routing tags are included in the profiles.");
                    }
                    var meta = _db.EdgeMeta.Add(metaTags);

                    // convert way into one or more edges.
                    var node = 0;
                    var isCore = false;
                    while (node < way.Nodes.Length - 1)
                    {
                        // build edge to add.
                        var intermediates = new List<Coordinate>();
                        var distance = 0.0f;
                        Coordinate coordinate;
                        if (!this.TryGetValue(way.Nodes[node], out coordinate, out isCore))
                        { // an incomplete way, node not in source.
                            return;
                        }
                        var fromVertex = this.AddCoreNode(way.Nodes[node],
                            coordinate.Latitude, coordinate.Longitude);
                        var fromNode = way.Nodes[node];
                        var previousCoordinate = coordinate;
                        node++;

                        var toVertex = uint.MaxValue;
                        var toNode = long.MaxValue;
                        while (true)
                        {
                            if (!this.TryGetValue(way.Nodes[node], out coordinate, out isCore))
                            { // an incomplete way, node not in source.
                                return;
                            }
                            distance += Coordinate.DistanceEstimateInMeter(
                                previousCoordinate, coordinate);
                            if (isCore)
                            { // node is part of the core.
                                toVertex = this.AddCoreNode(way.Nodes[node],
                                    coordinate.Latitude, coordinate.Longitude);
                                toNode = way.Nodes[node];
                                break;
                            }
                            intermediates.Add(coordinate);
                            previousCoordinate = coordinate;
                            node++;
                        }

                        // try to add edge.
                        if (fromVertex == toVertex)
                        { // target and source vertex are identical, this must be a loop.
                            if (intermediates.Count == 1)
                            { // there is just one intermediate, add that one as a vertex.
                                var newCoreVertex = _db.Network.VertexCount;
                                _db.Network.AddVertex(newCoreVertex, intermediates[0].Latitude, intermediates[0].Longitude);
                                this.AddCoreEdge(fromVertex, newCoreVertex, new Data.Network.Edges.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = Coordinate.DistanceEstimateInMeter(
                                        _db.Network.GetVertex(fromVertex), intermediates[0]),
                                    Profile = (ushort)profile
                                }, null);
                            }
                            else if (intermediates.Count >= 2)
                            { // there is more than one intermediate, add two new core vertices.
                                var newCoreVertex1 = _db.Network.VertexCount;
                                _db.Network.AddVertex(newCoreVertex1, intermediates[0].Latitude, intermediates[0].Longitude);
                                var newCoreVertex2 = _db.Network.VertexCount;
                                _db.Network.AddVertex(newCoreVertex2, intermediates[intermediates.Count - 1].Latitude,
                                    intermediates[intermediates.Count - 1].Longitude);
                                var distance1 = Coordinate.DistanceEstimateInMeter(
                                    _db.Network.GetVertex(fromVertex), intermediates[0]);
                                var distance2 = Coordinate.DistanceEstimateInMeter(
                                    _db.Network.GetVertex(toVertex), intermediates[intermediates.Count - 1]);
                                intermediates.RemoveAt(0);
                                intermediates.RemoveAt(intermediates.Count - 1);
                                this.AddCoreEdge(fromVertex, newCoreVertex1, new Data.Network.Edges.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = distance1,
                                    Profile = (ushort)profile
                                }, null);
                                this.AddCoreEdge(newCoreVertex1, newCoreVertex2, new Data.Network.Edges.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = distance - distance2 - distance1,
                                    Profile = (ushort)profile
                                }, intermediates);
                                this.AddCoreEdge(newCoreVertex2, toVertex, new Data.Network.Edges.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = distance2,
                                    Profile = (ushort)profile
                                }, null);
                            }
                            continue;
                        }

                        var edge = _db.Network.GetEdgeEnumerator(fromVertex).FirstOrDefault(x => x.To == toVertex);
                        if (edge == null && fromVertex != toVertex)
                        { // just add edge.
                            this.AddCoreEdge(fromVertex, toVertex, new Data.Network.Edges.EdgeData()
                            {
                                MetaId = meta,
                                Distance = distance,
                                Profile = (ushort)profile
                            }, intermediates);
                        }
                        else
                        { // oeps, already an edge there.
                            if (edge.Data.Distance == distance &&
                                edge.Data.Profile == profile &&
                                edge.Data.MetaId == meta)
                            {
                                // do nothing, identical duplicate data.
                            }
                            else
                            { // try and use intermediate points if any.
                              // try and use intermediate points.
                                var splitMeta = meta;
                                var splitProfile = profile;
                                var splitDistance = distance;
                                if (intermediates.Count == 0 &&
                                    edge != null &&
                                    edge.Shape != null)
                                { // no intermediates in current edge.
                                  // save old edge data.
                                    intermediates = new List<Coordinate>(edge.Shape);
                                    fromVertex = edge.From;
                                    toVertex = edge.To;
                                    splitMeta = edge.Data.MetaId;
                                    splitProfile = edge.Data.Profile;
                                    splitDistance = edge.Data.Distance;

                                    // just add edge.
                                    _db.Network.RemoveEdges(fromVertex, toVertex); // make sure to overwrite and not add an extra edge.
                                    this.AddCoreEdge(fromVertex, toVertex, new EdgeData()
                                    {
                                        MetaId = meta,
                                        Distance = System.Math.Max(distance, 0.0f),
                                        Profile = (ushort)profile
                                    }, null);
                                }

                                if (intermediates.Count > 0)
                                { // intermediates found, use the first intermediate as the core-node.
                                    var newCoreVertex = _db.Network.VertexCount;
                                    _db.Network.AddVertex(newCoreVertex, intermediates[0].Latitude, intermediates[0].Longitude);

                                    // calculate new distance and update old distance.
                                    var newDistance = Coordinate.DistanceEstimateInMeter(
                                        _db.Network.GetVertex(fromVertex), intermediates[0]);
                                    splitDistance -= newDistance;

                                    // add first part.
                                    this.AddCoreEdge(fromVertex, newCoreVertex, new EdgeData()
                                    {
                                        MetaId = splitMeta,
                                        Distance = System.Math.Max(newDistance, 0.0f),
                                        Profile = (ushort)splitProfile
                                    }, null);

                                    // add second part.
                                    intermediates.RemoveAt(0);
                                    this.AddCoreEdge(newCoreVertex, toVertex, new EdgeData()
                                    {
                                        MetaId = splitMeta,
                                        Distance = System.Math.Max(splitDistance, 0.0f),
                                        Profile = (ushort)splitProfile
                                    }, intermediates);
                                }
                                else
                                { // no intermediate or shapepoint found in either one. two identical edge overlayed with different profiles.
                                  // add two other vertices with identical positions as the ones given.
                                  // connect them with an edge of length '0'.
                                    var fromLocation = _db.Network.GetVertex(fromVertex);
                                    var newFromVertex = this.AddNewCoreNode(fromNode, fromLocation.Latitude, fromLocation.Longitude);
                                    this.AddCoreEdge(fromVertex, newFromVertex, new EdgeData()
                                    {
                                        Distance = 0,
                                        MetaId = splitMeta,
                                        Profile = (ushort)splitProfile
                                    }, null);
                                    var toLocation = _db.Network.GetVertex(toVertex);
                                    var newToVertex = this.AddNewCoreNode(toNode, toLocation.Latitude, toLocation.Longitude);
                                    this.AddCoreEdge(newToVertex, toVertex, new EdgeData()
                                    {
                                        Distance = 0,
                                        MetaId = splitMeta,
                                        Profile = (ushort)splitProfile
                                    }, null);

                                    this.AddCoreEdge(newFromVertex, newToVertex, new EdgeData()
                                    {
                                        Distance = splitDistance,
                                        MetaId = splitMeta,
                                        Profile = (ushort)splitProfile
                                    }, null);
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool TryGetValue(long node, out Coordinate coordinate, out bool isCore)
        {
            uint vertex;
            float latitude, longitude;
            if (!_nodeIndex.TryGetValue(node, out latitude, out longitude, out isCore, out vertex))
            { // an incomplete way, node not in source.
                coordinate = new Coordinate();
                isCore = false;
                return false;
            }
            if (vertex != uint.MaxValue)
            {
                coordinate = _db.Network.GetVertex(vertex);
                isCore = true;
                return true;
            }
            coordinate = new Coordinate()
            {
                Latitude = latitude,
                Longitude = longitude
            };
            return true;
        }

        /// <summary>
        /// Adds a core-node.
        /// </summary>
        /// <returns></returns>
        private uint AddCoreNode(long node, float latitude, float longitude)
        {
            var vertex = uint.MaxValue;
            if (_nodeIndex.TryGetCoreNode(node, out vertex))
            { // node was already added.
                return vertex;
            }
            return this.AddNewCoreNode(node, latitude, longitude);
        }

        /// <summary>
        /// Adds a new core-node, doesn't check if there is already a vertex.
        /// </summary>
        private uint AddNewCoreNode(long node, float latitude, float longitude)
        {
            var vertex = _db.Network.VertexCount;
            _db.Network.AddVertex(vertex, latitude, longitude);
            _nodeIndex.Set(node, vertex);

            if (_nodeData != null)
            {
                _nodeData[vertex] = node;
            }
            return vertex;
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public void AddCoreEdge(uint vertex1, uint vertex2, Data.Network.Edges.EdgeData data, List<Coordinate> shape)
        {
            if (data.Distance < _db.Network.MaxEdgeDistance)
            { // edge is ok, smaller than max distance.
                _db.Network.AddEdge(vertex1, vertex2, data, shape.Simplify(_simplifyEpsilonInMeter));
            }
            else
            { // edge is too big.
                if (shape == null)
                { // make sure there is a shape.
                    shape = new List<Coordinate>();
                }

                shape = new List<Coordinate>(shape);
                shape.Insert(0, _db.Network.GetVertex(vertex1));
                shape.Add(_db.Network.GetVertex(vertex2));

                for (var s = 1; s < shape.Count; s++)
                {
                    var distance = Coordinate.DistanceEstimateInMeter(shape[s - 1], shape[s]);
                    if (distance >= _db.Network.MaxEdgeDistance)
                    { // insert a new intermediate.
                        shape.Insert(s,
                            new Coordinate()
                            {
                                Latitude = (float)(((double)shape[s - 1].Latitude +
                                    (double)shape[s].Latitude) / 2.0),
                                Longitude = (float)(((double)shape[s - 1].Longitude +
                                    (double)shape[s].Longitude) / 2.0),
                            });
                        s--;
                    }
                }

                var i = 0;
                var shortShape = new List<Coordinate>();
                var shortDistance = 0.0f;
                uint shortVertex = Constants.NO_VERTEX;
                Coordinate? shortPoint;
                i++;
                while (i < shape.Count)
                {
                    var distance = Coordinate.DistanceEstimateInMeter(shape[i - 1], shape[i]);
                    if (distance + shortDistance > _db.Network.MaxEdgeDistance)
                    { // ok, previous shapepoint was the maximum one.
                        shortPoint = shortShape[shortShape.Count - 1];
                        shortShape.RemoveAt(shortShape.Count - 1);

                        // add vertex.            
                        shortVertex = _db.Network.VertexCount;
                        _db.Network.AddVertex(shortVertex, shortPoint.Value.Latitude, 
                            shortPoint.Value.Longitude);

                        // add edge.
                        _db.Network.AddEdge(vertex1, shortVertex, new Data.Network.Edges.EdgeData()
                        {
                            Distance = (float)shortDistance,
                            MetaId = data.MetaId,
                            Profile = data.Profile
                        }, shortShape.Simplify(_simplifyEpsilonInMeter));
                        vertex1 = shortVertex;

                        // set new short distance, empty shape.
                        shortShape.Clear();
                        shortShape.Add(shape[i]);
                        shortDistance = distance;
                        i++;
                    }
                    else
                    { // just add short distance and move to the next shape point.
                        shortShape.Add(shape[i]);
                        shortDistance += distance;
                        i++;
                    }
                }

                // add final segment.
                if (shortShape.Count > 0)
                {
                    shortShape.RemoveAt(shortShape.Count - 1);
                }

                // add edge.
                _db.Network.AddEdge(vertex1, vertex2, new Data.Network.Edges.EdgeData()
                {
                    Distance = (float)shortDistance,
                    MetaId = data.MetaId,
                    Profile = data.Profile
                }, shortShape.Simplify(_simplifyEpsilonInMeter));
            }
        }

        /// <summary>
        /// Adds a relation.
        /// </summary>
        public override void AddRelation(Relation relation)
        {
            if (_firstPass)
            {
                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.FirstPass(relation);
                    }
                }
            }
            else
            {
                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.SecondPass(relation);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the core node id map.
        /// </summary>
        public NodeIndex NodeIndex
        {
            get
            {
                return _nodeIndex;
            }
        }
    }
}