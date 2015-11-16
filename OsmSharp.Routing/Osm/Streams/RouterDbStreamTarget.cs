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

using OsmSharp.Collections;
using OsmSharp.Collections.Coordinates;
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.LongIndex;
using OsmSharp.Collections.LongIndex.LongIndex;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Osm;
using OsmSharp.Osm.Streams;
using OsmSharp.Routing.Graphs.Geometric.Shapes;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Network.Data;
using OsmSharp.Routing.Osm.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Osm.Streams
{
    /// <summary>
    /// A stream target to load a routing database.
    /// </summary>
    public class RouterDbStreamTarget : OsmStreamTarget
    {
        private readonly RouterDb _db;
        private readonly Vehicle[] _vehicles;
        private readonly bool _allNodesAreCore;

        /// <summary>
        /// Creates a new router db stream target.
        /// </summary>
        public RouterDbStreamTarget(RouterDb db, Vehicle[] vehicles)
            : this(db, vehicles, false)
        {

        }

        /// <summary>
        /// Creates a new router db stream target.
        /// </summary>
        public RouterDbStreamTarget(RouterDb db, Vehicle[] vehicles, bool allCore)
        {
            _db = db;
            _vehicles = vehicles;
            _allNodesAreCore = allCore;

            _routingNodeCoordinates = new CoordinateIndex();
            _routingNodes = new LongIndex();
            _coreNodes = new LongIndex();
            _coreNodeIdMap = new HugeDictionary<long, uint>();

            foreach (var vehicle in vehicles)
            {
                db.AddSupportedProfile(vehicle.Fastest());
                db.AddSupportedProfile(vehicle.Shortest());
            }
        }

        private bool _firstPass = true; // flag for first/second pass.
        private ILongIndex _routingNodes; // nodes that are in one routable way.
        private ICoordinateIndex _routingNodeCoordinates; // coordinates of nodes that are part of a routable way.
        private ILongIndex _coreNodes; // node that are in more than one routable way.
        private HugeDictionary<long, uint> _coreNodeIdMap; // maps nodes in the core onto routing network id's.

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
            // execute the first pass but ignore nodes.
            this.DoPull(true, false, true);

            // reset the source.
            this.Source.Reset();

            _firstPass = false;

            return true;
        }

        /// <summary>
        /// Registers the source.
        /// </summary>
        public virtual void RegisterSource(OsmStreamSource source, bool filterNonRoutingTags)
        {
            if (filterNonRoutingTags)
            { // add filtering.
                var eventsFilter = new OsmSharp.Osm.Streams.Filters.OsmStreamFilterWithEvents();
                eventsFilter.MovedToNextEvent += (osmGeo, param) =>
                {
                    if (osmGeo.Type == OsmSharp.Osm.OsmGeoType.Way)
                    {
                        var tags = new TagsCollection(osmGeo.Tags);
                        foreach (var tag in tags)
                        {
                            var relevant = false;
                            for (var i = 0; i < _vehicles.Length; i++)
                            {
                                if (_vehicles[i].IsRelevant(tag.Key, tag.Value))
                                {
                                    relevant = true;
                                    break;
                                }
                            }

                            if (!relevant)
                            {
                                osmGeo.Tags.RemoveKeyValue(tag.Key, tag.Value);
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
            if (!_firstPass)
            {
                if (_routingNodes.Contains(node.Id.Value))
                { // node is a routing node, store it's coordinates.
                    _routingNodeCoordinates[node.Id.Value] = new GeoCoordinateSimple()
                    {
                        Latitude = (float)node.Latitude.Value,
                        Longitude = (float)node.Longitude.Value
                    };
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
            if (way.Nodes.Count == 0) { return; }

            if (_firstPass)
            { // just keep 
                if (_vehicles.AnyCanTraverse(way.Tags))
                { // way has some use.
                    for (var i = 0; i < way.Nodes.Count; i++)
                    {
                        var node = way.Nodes[i];
                        if (_routingNodes.Contains(node) ||
                            _allNodesAreCore)
                        { // node already part of another way, definetly part of core.
                            _coreNodes.Add(node);
                        }
                        _routingNodes.Add(node);
                    }
                    _coreNodes.Add(way.Nodes[0]);
                    _coreNodes.Add(way.Nodes[way.Nodes.Count - 1]);
                }
            }
            else
            {
                if (_vehicles.AnyCanTraverse(way.Tags))
                { // way has some use.
                    // build profile and meta-data.
                    var profileTags = new TagsCollection(way.Tags.Count);
                    var metaTags = new TagsCollection(way.Tags.Count);
                    foreach (var tag in way.Tags)
                    {
                        if (_vehicles.IsRelevantForProfile(tag.Key))
                        {
                            profileTags.Add(tag);
                        }
                        else
                        {
                            metaTags.Add(tag);
                        }
                    }

                    // get profile and meta-data id's.
                    var profile = _db.EdgeProfiles.Add(profileTags);
                    if (profile > ushort.MaxValue) { throw new Exception("Maximum supported profiles exeeded, make sure only routing tags are included in the profiles."); }
                    var meta = _db.EdgeMeta.Add(metaTags);

                    // convert way into one or more edges.
                    var node = 0;
                    while (node < way.Nodes.Count - 1)
                    {
                        // build edge to add.
                        var intermediates = new List<ICoordinate>();
                        var distance = 0.0f;
                        ICoordinate coordinate;
                        if (!_routingNodeCoordinates.TryGet(way.Nodes[node], out coordinate))
                        { // an incomplete way, node not in source.
                            return;
                        }
                        var fromVertex = this.AddCoreNode(way.Nodes[node],
                            coordinate.Latitude, coordinate.Longitude);
                        var previousCoordinate = coordinate;
                        node++;

                        var toVertex = uint.MaxValue;
                        while (true)
                        {
                            if (!_routingNodeCoordinates.TryGet(way.Nodes[node], out coordinate))
                            { // an incomplete way, node not in source.
                                return;
                            }
                            distance += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                previousCoordinate, coordinate);
                            if (_coreNodes.Contains(way.Nodes[node]))
                            { // node is part of the core.
                                toVertex = this.AddCoreNode(way.Nodes[node],
                                    coordinate.Latitude, coordinate.Longitude);
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
                                this.AddCoreEdge(fromVertex, newCoreVertex, new Network.Data.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
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
                                var distance1 = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                    _db.Network.GetVertex(fromVertex), intermediates[0]);
                                var distance2 = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                    _db.Network.GetVertex(toVertex), intermediates[intermediates.Count - 1]);
                                intermediates.RemoveAt(0);
                                intermediates.RemoveAt(intermediates.Count - 1);
                                this.AddCoreEdge(fromVertex, newCoreVertex1, new Network.Data.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = distance1,
                                    Profile = (ushort)profile
                                }, null);
                                this.AddCoreEdge(newCoreVertex1, newCoreVertex2, new Network.Data.EdgeData()
                                {
                                    MetaId = meta,
                                    Distance = distance - distance2 - distance1,
                                    Profile = (ushort)profile
                                }, intermediates);
                                this.AddCoreEdge(newCoreVertex2, toVertex, new Network.Data.EdgeData()
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
                            this.AddCoreEdge(fromVertex, toVertex, new Network.Data.EdgeData()
                            {
                                MetaId = meta,
                                Distance = distance,
                                Profile = (ushort)profile
                            }, intermediates);
                        }
                        else
                        { // oeps, already an edge there, try and use intermediate points.
                            var splitMeta = meta;
                            var splitProfile = profile;
                            var splitDistance = distance;
                            if (intermediates.Count == 0 &&
                                edge != null &&
                                edge.Shape != null)
                            { // no intermediates in current edge.
                                // save old edge data.
                                intermediates = new List<ICoordinate>(edge.Shape);
                                fromVertex = edge.From;
                                toVertex = edge.To;
                                splitMeta = edge.Data.MetaId;
                                splitProfile = edge.Data.Profile;
                                splitDistance = edge.Data.Distance;

                                // just add edge.
                                this.AddCoreEdge(fromVertex, toVertex, new Network.Data.EdgeData()
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
                                var newDistance = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                    _db.Network.GetVertex(fromVertex), intermediates[0]);
                                splitDistance -= newDistance;

                                // add first part.
                                this.AddCoreEdge(fromVertex, newCoreVertex, new Network.Data.EdgeData()
                                {
                                    MetaId = splitMeta,
                                    Distance = System.Math.Max(newDistance, 0.0f),
                                    Profile = (ushort)splitProfile
                                }, null);

                                // add second part.
                                intermediates.RemoveAt(0);
                                this.AddCoreEdge(newCoreVertex, toVertex, new Network.Data.EdgeData()
                                {
                                    MetaId = splitMeta,
                                    Distance = System.Math.Max(splitDistance, 0.0f),
                                    Profile = (ushort)splitProfile
                                }, intermediates);
                            }
                        }
                    }
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
            if (_coreNodeIdMap.TryGetValue(node, out vertex))
            { // node was already added.
                return vertex;
            }
            vertex = _db.Network.VertexCount;
            _db.Network.AddVertex(vertex, latitude, longitude);
            _coreNodeIdMap[node] = vertex;
            return vertex;
        }

        /// <summary>
        /// Adds a new edge.
        /// </summary>
        public void AddCoreEdge(uint vertex1, uint vertex2, EdgeData data, List<ICoordinate> shape)
        {
            if (data.Distance < OsmSharp.Routing.Data.EdgeDataSerializer.MAX_DISTANCE)
            { // edge is ok, smaller than max distance.
                _db.Network.AddEdge(vertex1, vertex2, data, shape);
            }
            else
            { // edge is too big.
                if (shape == null ||
                    shape.Count == 0)
                {
                    throw new Exception(string.Format("An edge was found longer than {0}m without an intermediate shapepoint.",
                        OsmSharp.Routing.Data.EdgeDataSerializer.MAX_DISTANCE.ToInvariantString()));
                }

                shape = new List<ICoordinate>(shape);
                shape.Insert(0, _db.Network.GetVertex(vertex1));
                shape.Add(_db.Network.GetVertex(vertex2));

                var i = 0;
                var shortShape = new List<ICoordinate>();
                var shortDistance = 0.0f;
                uint shortVertex = Constants.NO_VERTEX;
                ICoordinate shortPoint;
                i++;
                while (i < shape.Count)
                {
                    var distance = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(shape[i - 1], shape[i]);
                    if (distance >= OsmSharp.Routing.Data.EdgeDataSerializer.MAX_DISTANCE)
                    {
                        throw new Exception(string.Format("A segment was found longer than {0}m without an intermediate shapepoint.",
                            OsmSharp.Routing.Data.EdgeDataSerializer.MAX_DISTANCE.ToInvariantString()));
                    }

                    if (distance + shortDistance > OsmSharp.Routing.Data.EdgeDataSerializer.MAX_DISTANCE)
                    { // ok, previous shapepoint was the maximum one.
                        shortPoint = shortShape[shortShape.Count - 1];
                        shortShape.RemoveAt(shortShape.Count - 1);

                        // add vertex.            
                        shortVertex = _db.Network.VertexCount;
                        _db.Network.AddVertex(shortVertex, shortPoint.Latitude, shortPoint.Longitude);

                        // add edge.
                        _db.Network.AddEdge(vertex1, shortVertex, new EdgeData()
                        {
                            Distance = (float)shortDistance,
                            MetaId = data.MetaId,
                            Profile = data.Profile
                        }, shortShape);
                        vertex1 = shortVertex;

                        // set new short distance, empty shape.
                        shortShape.Clear();
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
                _db.Network.AddEdge(vertex1, vertex2, new EdgeData()
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
        public override void AddRelation(Relation simpleRelation)
        {

        }

        /// <summary>
        /// Gets the core node id map.
        /// </summary>
        public HugeDictionary<long, uint> CoreNodeIdMap
        {
            get
            {
                return _coreNodeIdMap;
            }
        }
    }
}