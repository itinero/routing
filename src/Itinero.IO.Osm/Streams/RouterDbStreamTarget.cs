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

using Itinero.Algorithms.Collections;
using Itinero.LocalGeo;
using Itinero.Data.Network;
using OsmSharp.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Attributes;
using OsmSharp;
using Itinero.Data.Network.Edges;
using Itinero.IO.Osm.Restrictions;
using Itinero.Data.Network.Restrictions;
using Itinero.IO.Osm.Relations;
using Itinero.Profiles;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A stream target to load a routing database.
    /// </summary>
    public class RouterDbStreamTarget : OsmStreamTarget
    {
        private readonly RouterDb _db;
        private readonly Vehicle[] _vehicles;
        private readonly VehicleCache _vehicleCache;
        private readonly bool _allNodesAreCore;
        private readonly int _minimumStages = 1;
        private readonly Func<NodeCoordinatesDictionary> _createNodeCoordinatesDictionary;
        private readonly bool _normalizeTags = true;
        private readonly HashSet<string> _vehicleTypes;

        /// <summary>
        /// Creates a new router db stream target.
        /// </summary>
        public RouterDbStreamTarget(RouterDb db, Vehicle[] vehicles, bool allCore = false,
            int minimumStages = 1, IEnumerable<ITwoPassProcessor> processors = null, bool processRestrictions = false)
        {
            _db = db;
            _vehicles = vehicles;
            _vehicleCache = new VehicleCache(vehicles);

            _vehicleTypes = new HashSet<string>();
            foreach(var vehicle in _vehicles)
            {
                foreach (var vehicleType in vehicle.VehicleTypes)
                {
                    _vehicleTypes.Add(vehicleType);
                }
            }

            _allNodesAreCore = allCore;

            _createNodeCoordinatesDictionary = () =>
            {
                return new NodeCoordinatesDictionary();
            };
            _stageCoordinates = _createNodeCoordinatesDictionary();
            _allRoutingNodes = new SparseLongIndex();
            _anyStageNodes = new SparseLongIndex();
            _coreNodes = new SparseLongIndex();
            _coreNodeIdMap = new CoreNodeIdMap();
            _processedWays = new SparseLongIndex();
            _minimumStages = minimumStages;

            foreach (var vehicle in vehicles)
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
        private SparseLongIndex _allRoutingNodes; // nodes that are in a routable way.
        private SparseLongIndex _anyStageNodes; // nodes that are in a routable way that needs to be included in all stages.
        private SparseLongIndex _processedWays; // ways that have been processed already.
        private NodeCoordinatesDictionary _stageCoordinates; // coordinates of nodes that are part of a routable way in the current stage.
        private SparseLongIndex _coreNodes; // node that are in more than one routable way.
        private CoreNodeIdMap _coreNodeIdMap; // maps nodes in the core onto routing network id's.

        private long _nodeCount = 0;
        private float _minLatitude = float.MaxValue, _minLongitude = float.MaxValue,
            _maxLatitude = float.MinValue, _maxLongitude = float.MinValue;
        private List<Box> _stages = new List<Box>();
        private int _stage = -1;

        /// <summary>
        /// Setups default add-on processors.
        /// </summary>
        private void InitializeDefaultProcessors(bool processRestrictions)
        {
            // add all vehicle relation processors.
            foreach(var vehicle in _vehicles)
            {
                if (vehicle is Itinero.Osm.Vehicles.Vehicle)
                {
                    this.Processors.Add(new VehicleRelationTagProcessor(vehicle as Itinero.Osm.Vehicles.Vehicle));
                }
            }

            // add restriction processor if needed.
            if (processRestrictions)
            {
                this.Processors.Add(new RestrictionProcessor(_vehicleTypes, (node) =>
                {
                    uint vertex;
                    if (!_coreNodeIdMap.TryGetFirst(node, out vertex))
                    {
                        return uint.MaxValue;
                    }
                    return vertex;
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
        }

        /// <summary>
        /// Called right before pull and right after initialization.
        /// </summary>
        /// <returns></returns>
        public override bool OnBeforePull()
        {
            // execute the first pass.
            this.DoPull();

            // move to first stage and initial first pass.
            _stage = 0;
            _firstPass = false;
            while (_stage < _stages.Count)
            { // execute next stage, reset source and pull data again.
                this.Source.Reset();
                this.DoPull();
                _stage++;

                _stageCoordinates = _createNodeCoordinatesDictionary();
            }

            return false;
        }

        /// <summary>
        /// Gets or sets extra two-pass processors.
        /// </summary>
        public List<ITwoPassProcessor> Processors { get; set; }

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
                        var tags = osmGeo.Tags.ToAttributes();

                        foreach (var tag in tags)
                        {
                            if (!_vehicles.IsOnProfileWhiteList(tag.Key) &&
                                !_vehicles.IsOnMetaWhiteList(tag.Key))
                            {
                                osmGeo.Tags.RemoveKey(tag.Key);
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
                _nodeCount++;
                var latitude = node.Latitude.Value;
                if (latitude < _minLatitude)
                {
                    _minLatitude = latitude;
                }
                if (latitude > _maxLatitude)
                {
                    _maxLatitude = latitude;
                }
                var longitude = node.Longitude.Value;
                if (longitude < _minLongitude)
                {
                    _minLongitude = longitude;
                }
                if (longitude > _maxLongitude)
                {
                    _maxLongitude = longitude;
                }

                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.FirstPass(node);
                    }
                }
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

                if (_stages[_stage].Overlaps(node.Latitude.Value, node.Longitude.Value) ||
                    _anyStageNodes.Contains(node.Id.Value))
                {
                    if (_allRoutingNodes.Contains(node.Id.Value))
                    { // node is a routing node, store it's coordinates.
                        _stageCoordinates.Add(node.Id.Value, new Coordinate()
                        {
                            Latitude = (float)node.Latitude.Value,
                            Longitude = (float)node.Longitude.Value
                        });
                    }
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

            if (_firstPass)
            { // just keep.
                if (this.Processors != null)
                {
                    foreach (var processor in this.Processors)
                    {
                        processor.FirstPass(way);
                    }
                }

                // check boundingbox and node count and descide on # stages.                    
                var box = new Box(
                    new Coordinate(_minLatitude, _minLongitude),
                    new Coordinate(_maxLatitude, _maxLongitude));
                var e = 0.00001f;
                if (_stages.Count == 0)
                {
                    if ((_nodeCount > 500000000 ||
                         _minimumStages > 1))
                    { // more than half a billion nodes, split in different stages.
                        var stages = System.Math.Max(System.Math.Ceiling((double)_nodeCount / 500000000), _minimumStages);

                        if (stages >= 4)
                        {
                            stages = 4;
                            _stages.Add(new Box(
                                new Coordinate(_minLatitude, _minLongitude),
                                new Coordinate(box.Center.Latitude, box.Center.Longitude)));
                            _stages[0] = _stages[0].Resize(e);
                            _stages.Add(new Box(
                                new Coordinate(_minLatitude, box.Center.Longitude),
                                new Coordinate(box.Center.Latitude, _maxLongitude)));
                            _stages[1] = _stages[1].Resize(e);
                            _stages.Add(new Box(
                                new Coordinate(box.Center.Latitude, _minLongitude),
                                new Coordinate(_maxLatitude, box.Center.Longitude)));
                            _stages[2] = _stages[2].Resize(e);
                            _stages.Add(new Box(
                                new Coordinate(box.Center.Latitude, box.Center.Longitude),
                                new Coordinate(_maxLatitude, _maxLongitude)));
                            _stages[3] = _stages[3].Resize(e);
                        }
                        else if (stages >= 2)
                        {
                            stages = 2;
                            _stages.Add(new Box(
                                new Coordinate(_minLatitude, _minLongitude),
                                new Coordinate(_maxLatitude, box.Center.Longitude)));
                            _stages[0] = _stages[0].Resize(e);
                            _stages.Add(new Box(
                                new Coordinate(_minLatitude, box.Center.Longitude),
                                new Coordinate(_maxLatitude, _maxLongitude)));
                            _stages[1] = _stages[1].Resize(e);
                        }
                        else
                        {
                            stages = 1;
                            _stages.Add(box);
                            _stages[0] = _stages[0].Resize(e);
                        }
                    }
                    else
                    {
                        _stages.Add(box);
                        _stages[0] = _stages[0].Resize(e);
                    }
                }

                if (_vehicleCache.AnyCanTraverse(way.Tags.ToAttributes()))
                { // way has some use.
                    for (var i = 0; i < way.Nodes.Length; i++)
                    {
                        var node = way.Nodes[i];
                        if (_allRoutingNodes.Contains(node) ||
                            _allNodesAreCore)
                        { // node already part of another way, definetly part of core.
                            _coreNodes.Add(node);
                        }
                        _allRoutingNodes.Add(node);
                    }
                    _coreNodes.Add(way.Nodes[0]);
                    _coreNodes.Add(way.Nodes[way.Nodes.Length - 1]);
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
                    if (_processedWays.Contains(way.Id.Value))
                    { // way was already processed.
                        return;
                    }

                    // build profile and meta-data.
                    var profileTags = new AttributeCollection();
                    var metaTags = new AttributeCollection();
                    foreach (var tag in way.Tags)
                    {
                        if (profileWhiteList.Contains(tag.Key) || 
                            _vehicles.IsOnProfileWhiteList(tag.Key))
                        {
                            profileTags.Add(tag);
                        }
                        else if (_vehicles.IsOnMetaWhiteList(tag.Key))
                        {
                            metaTags.Add(tag);
                        }
                    }

                    // get profile and meta-data id's.
                    var profile = _db.EdgeProfiles.Add(profileTags);
                    if (profile > Data.Edges.EdgeDataSerializer.MAX_PROFILE_COUNT)
                    {
                        throw new Exception("Maximum supported profiles exeeded, make sure only routing tags are included in the profiles.");
                    }
                    var meta = _db.EdgeMeta.Add(metaTags);

                    // convert way into one or more edges.
                    var node = 0;
                    while (node < way.Nodes.Length - 1)
                    {
                        // build edge to add.
                        var intermediates = new List<Coordinate>();
                        var distance = 0.0f;
                        Coordinate coordinate;
                        if (!_stageCoordinates.TryGetValue(way.Nodes[node], out coordinate))
                        { // an incomplete way, node not in source.
                            // add all the others to the any stage index.
                            for (var i = 0; i < way.Nodes.Length; i++)
                            {
                                _anyStageNodes.Add(way.Nodes[i]);
                            }
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
                            if (!_stageCoordinates.TryGetValue(way.Nodes[node], out coordinate))
                            { // an incomplete way, node not in source.
                                // add all the others to the any stage index.
                                for (var i = 0; i < way.Nodes.Length; i++)
                                {
                                    _anyStageNodes.Add(way.Nodes[i]);
                                }
                                return;
                            }
                            distance += Coordinate.DistanceEstimateInMeter(
                                previousCoordinate, coordinate);
                            if (_coreNodes.Contains(way.Nodes[node]))
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
                    _processedWays.Add(way.Id.Value);
                }
            }
        }

        /// <summary>
        /// Adds a core-node.
        /// </summary>
        /// <returns></returns>
        private uint AddCoreNode(long node, float latitude, float longitude)
        {
            var vertex = uint.MaxValue;
            if (_coreNodeIdMap.TryGetFirst(node, out vertex))
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
            _coreNodeIdMap.Add(node, vertex);
            return vertex;
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public void AddCoreEdge(uint vertex1, uint vertex2, Data.Network.Edges.EdgeData data, List<Coordinate> shape)
        {
            if (data.Distance < _db.Network.MaxEdgeDistance)
            { // edge is ok, smaller than max distance.
                _db.Network.AddEdge(vertex1, vertex2, data, shape);
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
                        }, shortShape);
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
                }, shortShape);
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
        public CoreNodeIdMap CoreNodeIdMap
        {
            get
            {
                return _coreNodeIdMap;
            }
        }
    }
}