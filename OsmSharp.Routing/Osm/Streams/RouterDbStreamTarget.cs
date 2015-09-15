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

        /// <summary>
        /// Creates a new router db stream target.
        /// </summary>
        public RouterDbStreamTarget(RouterDb db, Vehicle[] vehicles)
        {
            _db = db;
            _vehicles = vehicles;

            _routingNodeCoordinates = new CoordinateIndex();
            _routingNodes = new LongIndex();
            _coreNodes = new LongIndex();
            _coreNodeIdMap = new HugeDictionary<long, uint>();
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
        /// Adds a node.
        /// </summary>
        public override void AddNode(Node node)
        {
            if(!_firstPass)
            {
                if(_routingNodes.Contains(node.Id.Value))
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
            if(way == null) { return; }
            if(way.Nodes == null) { return; }
            if(way.Nodes.Count == 0) { return; }

            if(_firstPass)
            { // just keep 
                if (_vehicles.AnyCanTraverse(way.Tags))
                { // way has some use.
                    for(var i = 0; i < way.Nodes.Count; i++)
                    {
                        var node = way.Nodes[i];
                        if (_routingNodes.Contains(node))
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
                    foreach(var tag in way.Tags)
                    {
                        if(_vehicles.IsRelevantForProfile(tag.Key))
                        {
                            profileTags.Add(tag);
                        }
                        else if(_vehicles.IsRelevantForMeta(tag.Key))
                        {
                            metaTags.Add(tag);
                        }
                    }

                    // get profile and meta-data id's.
                    var profile = _db.Profiles.Add(profileTags);
                    if(profile > ushort.MaxValue) { throw new Exception("Maximum supported profiles exeeded, make sure only routing tags are included in the profiles."); }
                    var meta = _db.Meta.Add(metaTags);

                    // convert way into one or more edges.
                    var node = 0;
                    while (node < way.Nodes.Count)
                    {
                        // build edge to add.
                        var intermediates = new List<ICoordinate>();
                        var distance = 0.0f;
                        var coordinate = _routingNodeCoordinates[way.Nodes[node]];
                        var fromVertex = this.AddCoreNode(way.Nodes[node],
                            coordinate.Latitude, coordinate.Longitude);
                        var previousCoordinate = coordinate;
                        node++;

                        var toVertex = uint.MaxValue;
                        while(true)
                        {
                            coordinate = _routingNodeCoordinates[way.Nodes[node]];
                            distance += (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                previousCoordinate, coordinate);
                            if(_coreNodes.Contains(way.Nodes[node]))
                            { // node is part of the core.
                                toVertex = this.AddCoreNode(way.Nodes[node],
                                    coordinate.Latitude, coordinate.Longitude);
                                node++;
                                break;
                            }
                            intermediates.Add(coordinate);
                            node++;
                        }

                        // try to add edge.
                        if(_db.Network.GetEdgeEnumerator(fromVertex).Any(x => x.To == toVertex))
                        { // oeps, already an edge there, try and use intermediate points.
                            if(intermediates.Count >= 0)
                            { // intermediates found, just use the first intermediate as the core-node.
                                var newCoreVertex = _db.Network.VertexCount;
                                _db.Network.AddVertex(newCoreVertex, intermediates[0].Latitude, intermediates[0].Longitude);

                                // calculate new distance and update old distance.
                                var newDistance = (float)OsmSharp.Math.Geo.GeoCoordinate.DistanceEstimateInMeter(
                                    _db.Network.GetVertex(fromVertex), intermediates[0]);
                                distance -= newDistance;

                                _db.Network.AddEdge(fromVertex, newCoreVertex, new Network.Data.EdgeData()
                                    {
                                        MetaId = meta,
                                        Distance = newDistance,
                                        Profile = (ushort)profile
                                    }, null);

                                // make sure to update new fromVertex and intermediates to reflect new situation.
                                fromVertex = newCoreVertex;
                                intermediates.RemoveAt(0);
                            }
                        }
                        _db.Network.AddEdge(fromVertex, toVertex, new Network.Data.EdgeData()
                            {
                                MetaId = meta,
                                Distance = distance,
                                Profile = (ushort)profile
                            }, new CoordinateArrayCollection<ICoordinate>(intermediates.ToArray()));
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
            if(_coreNodeIdMap.TryGetValue(node, out vertex))
            { // node was already added.
                return vertex;
            }
            vertex = _db.Network.VertexCount;
            _db.Network.AddVertex(vertex, latitude, longitude);
            return vertex;
        }

        /// <summary>
        /// Adds a relation.
        /// </summary>
        public override void AddRelation(Relation simpleRelation)
        {

        }
    }
}