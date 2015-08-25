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
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Osm;
using OsmSharp.Osm.Cache;
using OsmSharp.Osm.Streams;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Osm.Streams
{
    /// <summary>
    /// Data Processor Target to fill a dynamic graph object.
    /// </summary>
    public abstract class GraphOsmStreamTargetBase<TEdgeData> : OsmStreamTarget
        where TEdgeData : struct, IGraphEdgeData 
    {
        private readonly RouterDataSourceBase<TEdgeData> _graph;
        private readonly IOsmRoutingInterpreter _interpreter;
        private ITagsIndex _tagsIndex;
        private readonly OsmDataCache _dataCache;
        private bool _preIndexMode;
        private bool _collectIntermediates;
        protected bool _storeTags = true;
        private readonly Func<GraphBase<TEdgeData>, IPreprocessor> _createPreprocessor;

        /// <summary>
        /// Creates a new processor target.
        /// </summary>
        protected GraphOsmStreamTargetBase(RouterDataSourceBase<TEdgeData> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex, Func<GraphBase<TEdgeData>, IPreprocessor> createPreprocessor)
            : this(graph, interpreter, tagsIndex, createPreprocessor, new HugeDictionary<long, uint>(), true, new CoordinateIndex())
        {

        }

        /// <summary>
        /// Creates a new processor target.
        /// </summary>
        protected GraphOsmStreamTargetBase(
            RouterDataSourceBase<TEdgeData> graph, IOsmRoutingInterpreter interpreter,
            ITagsIndex tagsIndex, Func<GraphBase<TEdgeData>, IPreprocessor> createPreprocessor, HugeDictionary<long, uint> idTransformations, bool collectIntermediates, ICoordinateIndex coordinates)
        {
            _graph = graph;
            _interpreter = interpreter;

            _tagsIndex = tagsIndex;
            _idTransformations = idTransformations;
            _preIndexMode = true;
            _preIndex = new OsmSharp.Collections.LongIndex.LongIndex.LongIndex();
            _relevantNodes = new OsmSharp.Collections.LongIndex.LongIndex.LongIndex();
            _restricedWays = new HashSet<long>();
            _collapsedNodes = new Dictionary<long, KeyValuePair<KeyValuePair<long, uint>, KeyValuePair<long, uint>>>();
            _createPreprocessor = createPreprocessor;

            _collectIntermediates = collectIntermediates;
            _dataCache = new OsmDataCacheMemory();
            _coordinates = coordinates;
        }

        /// <summary>
        /// Returns the tags index.
        /// </summary>
        public ITagsIndex TagsIndex
        {
            get
            {
                return _tagsIndex;
            }
        }

        /// <summary>
        /// Returns the target graph.
        /// </summary>
        public RouterDataSourceBase<TEdgeData> Graph
        {
            get { return _graph; }
        }

        /// <summary>
        /// Returns the osm routing interpreter.
        /// </summary>
        public IOsmRoutingInterpreter Interpreter
        {
            get { return _interpreter; }
        }

        /// <summary>
        /// Holds the coordinates.
        /// </summary>
        private ICoordinateIndex _coordinates;

        /// <summary>
        /// Holds the index of all relevant nodes.
        /// </summary>
        private ILongIndex _preIndex;

        /// <summary>
        /// Holds the id transformations.
        /// </summary>
        private readonly HugeDictionary<long, uint> _idTransformations;

        /// <summary>
        /// Initializes the processing.
        /// </summary>
        public override void Initialize()
        {
            _coordinates = new CoordinateIndex();
        }

        /// <summary>
        /// Adds the given node.
        /// </summary>
        /// <param name="node"></param>
        public override void AddNode(Node node)
        {
            if (!_preIndexMode)
            {
                if (_nodesToCache != null &&
                    _nodesToCache.Contains(node.Id.Value))
                { // cache this node?
                    _dataCache.AddNode(node);
                }

                if (_preIndex != null && _preIndex.Contains(node.Id.Value))
                { // only save the coordinates for relevant nodes.
                    // save the node-coordinates.
                    // add the relevant nodes.
                    _coordinates[node.Id.Value] = new GeoCoordinateSimple()
                    {
                        Latitude = (float)node.Latitude.Value,
                        Longitude = (float)node.Longitude.Value
                    };

                    // add the node as a possible restriction.
                    if (_interpreter.IsRestriction(OsmGeoType.Node, node.Tags))
                    { // tests quickly if a given node is possibly a restriction.
                        var vehicleTypes = _interpreter.CalculateRestrictions(node);
                        if (vehicleTypes != null &&
                            vehicleTypes.Count > 0)
                        { // add all the restrictions.
                            this._relevantNodes.Add(node.Id.Value);
                            
                            var vertexId = this.AddRoadNode(node.Id.Value).Value; // will always exists, has just been added to coordinates.
                            var restriction = new uint[] { vertexId };
                            if (vehicleTypes.Contains(null))
                            { // restriction is valid for all vehicles.
                                _graph.AddRestriction(restriction);
                            }
                            else
                            { // restriction is restricted to some vehicles only.
                                foreach (string vehicle in vehicleTypes)
                                {
                                    _graph.AddRestriction(vehicle, restriction);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Holds a list of nodes used twice or more.
        /// </summary>
        private ILongIndex _relevantNodes;

        /// <summary>
        /// Holds all ways that have at least one restrictions.
        /// </summary>
        private HashSet<long> _restricedWays;

        /// <summary>
        /// Holds nodes that have been collapsed because they are considered irrelevant.
        /// </summary>
        private Dictionary<long, KeyValuePair<KeyValuePair<long, uint>, KeyValuePair<long, uint>>> _collapsedNodes;

        /// <summary>
        /// Adds a given way.
        /// </summary>
        /// <param name="way"></param>
        public override void AddWay(Way way)
        {
            if (!_preIndexMode && _waysToCache != null &&
                _waysToCache.Contains(way.Id.Value))
            { // cache this way?
               _dataCache.AddWay(way);
            }

            // initialize the way interpreter.
            if (_interpreter.EdgeInterpreter.IsRoutable(way.Tags))
            { // the way is a road.
                if (_preIndexMode)
                { // index relevant and used nodes.
                    if (way.Nodes != null)
                    { // this way has nodes.
                        // add new routable tags type.
                        var routableWayTags = new TagsCollection(way.Tags);
                        routableWayTags.RemoveAll(x =>
                        {
                            return !_interpreter.IsRelevantRouting(x.Key) &&
                                !Vehicle.IsRelevantForOneOrMore(x.Key);
                        });
                        if (_storeTags)
                        {
                            _tagsIndex.Add(routableWayTags);
                        }

                        int wayNodesCount = way.Nodes.Count;
                        for (int idx = 0; idx < wayNodesCount; idx++)
                        {
                            var node = way.Nodes[idx];
                            if (_preIndex.Contains(node))
                            { // node is relevant.
                                _relevantNodes.Add(node);
                            }
                            else
                            { // node is used.
                                _preIndex.Add(node);
                            }
                        }

                        if (wayNodesCount > 0)
                        { // first node is always relevant.
                            _relevantNodes.Add(way.Nodes[0]);
                            if (wayNodesCount > 1)
                            { // last node is always relevant.
                                _relevantNodes.Add(way.Nodes[wayNodesCount - 1]);
                            }
                        }
                    }
                }
                else
                { // add actual edges.
                    if (way.Nodes != null && way.Nodes.Count > 1)
                    { // way has at least two nodes.
                        if (this.CalculateIsTraversable(_interpreter.EdgeInterpreter, _tagsIndex,
                            way.Tags))
                        { // the edge is traversable, add the edges.
                            uint? from = this.AddRoadNode(way.Nodes[0]);
                            long fromNodeId = way.Nodes[0];
                            List<long> intermediates = new List<long>();
                            for (int idx = 1; idx < way.Nodes.Count; idx++)
                            { // the to-node.
                                long currentNodeId = way.Nodes[idx];
                                if (!_collectIntermediates ||
                                    _relevantNodes.Contains(currentNodeId) ||
                                    idx == way.Nodes.Count - 1)
                                { // node is an important node.
                                    uint? to = this.AddRoadNode(currentNodeId);
                                    long toNodeId = currentNodeId;

                                    // add the edge(s).
                                    if (from.HasValue && to.HasValue)
                                    { // add a road edge.
                                        while(from.Value == to.Value)
                                        {
                                            if(intermediates.Count > 0)
                                            {
                                                uint? dummy = this.AddRoadNode(intermediates[0]);
                                                intermediates.RemoveAt(0);
                                                if(dummy.HasValue && from.Value != dummy.Value)
                                                {
                                                    this.AddRoadEdge(way.Tags, from.Value, dummy.Value, null);
                                                    from = dummy;
                                                }
                                            }
                                            else
                                            { // no use to continue.
                                                break;
                                            }
                                        }
                                        // build coordinates.
                                        var intermediateCoordinates = new List<GeoCoordinateSimple>(intermediates.Count);
                                        for (int coordIdx = 0; coordIdx < intermediates.Count; coordIdx++)
                                        {
                                            ICoordinate coordinate;
                                            if (!_coordinates.TryGet(intermediates[coordIdx], out coordinate))
                                            {
                                                break;
                                            }
                                            intermediateCoordinates.Add(new GeoCoordinateSimple()
                                            {
                                                Latitude = coordinate.Latitude,
                                                Longitude = coordinate.Longitude
                                            });
                                        }

                                        if (intermediateCoordinates.Count == intermediates.Count &&
                                            from.Value != to.Value)
                                        { // all coordinates have been found.
                                            this.AddRoadEdge(way.Tags, from.Value, to.Value, intermediateCoordinates);
                                        }
                                    }

                                    // if this way has a restriction save the collapsed nodes information.
                                    if(_restricedWays.Contains(way.Id.Value) && to.HasValue && from.HasValue)
                                    { // loop over all intermediates and save.
                                        var collapsedInfo = new KeyValuePair<KeyValuePair<long, uint>, KeyValuePair<long, uint>>(
                                            new KeyValuePair<long, uint>(fromNodeId, from.Value), new KeyValuePair<long, uint>(toNodeId, to.Value));
                                        foreach(var intermedidate in intermediates)
                                        {
                                            _collapsedNodes[intermedidate] = collapsedInfo;
                                        }
                                    }

                                    from = to; // the to node becomes the from.
                                    intermediates.Clear();
                                }
                                else
                                { // this node is just an intermediate.
                                    intermediates.Add(currentNodeId);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the given node has an actual road node, meaning a relevant vertex, and outputs the vertex id.
        /// </summary>
        /// <param name="nodeId">The node id.</param>
        /// <param name="id">The vertex id.</param>
        /// <returns></returns>
        private bool TryGetRoadNode(long nodeId, out uint id)
        {
            return _idTransformations.TryGetValue(nodeId, out id);
        }

        /// <summary>
        /// A delegate used to communicate about vertices being added.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="vertex"></param>
        public delegate void VertexAddedDelegate(long nodeId, uint vertex);

        /// <summary>
        /// An event triggered when a new vertex was added for a given node.
        /// </summary>
        public event VertexAddedDelegate VertexAdded;

        /// <summary>
        /// Adds a node that is at least part of one road.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <returns></returns>
        private uint? AddRoadNode(long nodeId)
        {
            uint id;
            // try and get existing node.
            if (!_idTransformations.TryGetValue(nodeId, out id))
            {
                // get coordinates.
                ICoordinate coordinates;
                if (_coordinates.TryGet(nodeId, out coordinates))
                { // the coordinate is present.
                    id = _graph.AddVertex(
                        coordinates.Latitude, coordinates.Longitude);
                    _coordinates.Remove(nodeId); // free the memory again!

                    if (_relevantNodes.Contains(nodeId))
                    {
                        _idTransformations[nodeId] = id;
                    }

                    // trigger event.
                    if(this.VertexAdded != null)
                    {
                        this.VertexAdded(nodeId, id);
                    }

                    return id;
                }
                return null;
            }
            return id;
        }

        /// <summary>
        /// Adds an edge.
        /// </summary>
        protected virtual void AddRoadEdge(TagsCollectionBase tags, uint from, uint to, List<GeoCoordinateSimple> intermediates)
        {
            float latitude;
            float longitude;
            GeoCoordinate fromCoordinate = null;
            if (_graph.GetVertex(from, out latitude, out longitude))
            { // 
                fromCoordinate = new GeoCoordinate(latitude, longitude);
            }
            GeoCoordinate toCoordinate = null;
            if (_graph.GetVertex(to, out latitude, out longitude))
            { // 
                toCoordinate = new GeoCoordinate(latitude, longitude);
            }

            if (fromCoordinate != null && toCoordinate != null)
            { // calculate the edge data.
                TEdgeData existingData;
                ICoordinateCollection forwardShape;
                if (this.GetEdge(_graph, from, to, out existingData, out forwardShape))
                { // oeps, an edge already exists!
                    if (intermediates != null && intermediates.Count > 0)
                    { // add one of the intermediates as new vertex.
                        uint newVertex;
                        if (forwardShape != null && forwardShape.Count > 0)
                        { // the other edge also has a shape, make sure to also split it.
                            var existingIntermediates = new List<GeoCoordinateSimple>(forwardShape.ToSimpleArray());
                            newVertex = _graph.AddVertex(existingIntermediates[0].Latitude, existingIntermediates[0].Longitude);

                            // add edge before.
                            var beforeEdgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                                fromCoordinate, new GeoCoordinate(existingIntermediates[0].Latitude, existingIntermediates[0].Longitude), null);
                            _graph.AddEdge(from, newVertex, beforeEdgeData, null);
                            if(_graph.IsDirected)
                            { // also the need to add the reverse edge.
                                beforeEdgeData = (TEdgeData)beforeEdgeData.Reverse();
                                _graph.AddEdge(newVertex, from, beforeEdgeData, null);
                            }

                            // add edge after.
                            var afterIntermediates = existingIntermediates.GetRange(1, existingIntermediates.Count - 1);
                            var afterEdgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                                new GeoCoordinate(existingIntermediates[0].Latitude, existingIntermediates[0].Longitude), toCoordinate, afterIntermediates);
                            _graph.AddEdge(newVertex, to, afterEdgeData, new CoordinateArrayCollection<GeoCoordinateSimple>(afterIntermediates.ToArray()));
                            if (_graph.IsDirected)
                            { // also the need to add the reverse edge.
                                afterIntermediates.Reverse();
                                afterEdgeData = (TEdgeData)afterEdgeData.Reverse();
                                _graph.AddEdge(to, newVertex, afterEdgeData, new CoordinateArrayCollection<GeoCoordinateSimple>(afterIntermediates.ToArray()));
                            }

                            // remove original edge.
                            _graph.RemoveEdge(from, to, existingData);
                            if(_graph.IsDirected && _graph.CanHaveDuplicates)
                            { // also remove opposite edges.
                                _graph.RemoveEdge(to, from, (TEdgeData)existingData.Reverse());
                            }
                        }
                        
                        newVertex = _graph.AddVertex(intermediates[0].Latitude, intermediates[0].Longitude);
                        var newEdgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                            fromCoordinate, new GeoCoordinate(intermediates[0].Latitude, intermediates[0].Longitude), null);
                        _graph.AddEdge(from, newVertex, newEdgeData, null);
                        if (_graph.IsDirected)
                        { // also the need to add the reverse edge.
                            newEdgeData = (TEdgeData)newEdgeData.Reverse();
                            _graph.AddEdge(newVertex, from, newEdgeData, null);
                        }

                        from = newVertex;
                        fromCoordinate = new GeoCoordinate(intermediates[0].Latitude, intermediates[0].Longitude);
                        intermediates = intermediates.GetRange(1, intermediates.Count - 1);
                    }
                    else
                    { // hmm, no intermediates, the other edge should have them.
                        if (forwardShape != null && forwardShape.Count > 0)
                        { // there is a shape, add one of the intermediates as a new vertex.
                            var existingIntermediates = new List<GeoCoordinateSimple>(forwardShape.ToSimpleArray());
                            var newVertex = _graph.AddVertex(existingIntermediates[0].Latitude, existingIntermediates[0].Longitude);

                            // add edge before.
                            var beforeEdgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                                fromCoordinate, new GeoCoordinate(existingIntermediates[0].Latitude, existingIntermediates[0].Longitude), null);
                            _graph.AddEdge(from, newVertex, beforeEdgeData, null);
                            if (_graph.IsDirected)
                            { // also the need to add the reverse edge.
                                beforeEdgeData = (TEdgeData)beforeEdgeData.Reverse();
                                _graph.AddEdge(newVertex, from, beforeEdgeData, null);
                            }

                            // add edge after.
                            var afterIntermediates = existingIntermediates.GetRange(1, existingIntermediates.Count - 1);
                            var afterEdgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                                new GeoCoordinate(existingIntermediates[0].Latitude, existingIntermediates[0].Longitude), toCoordinate, afterIntermediates);
                            _graph.AddEdge(newVertex, to, afterEdgeData, new CoordinateArrayCollection<GeoCoordinateSimple>(afterIntermediates.ToArray()));
                            if (_graph.IsDirected)
                            { // also the need to add the reverse edge.
                                afterIntermediates.Reverse();
                                afterEdgeData = (TEdgeData)afterEdgeData.Reverse();
                                _graph.AddEdge(to, newVertex, afterEdgeData, new CoordinateArrayCollection<GeoCoordinateSimple>(afterIntermediates.ToArray()));
                            }

                            if (_graph.CanHaveDuplicates)
                            { // make sure to remove the existing edge if graph allows duplicates.
                                _graph.RemoveEdge(from, to); 
                                if(_graph.IsDirected)
                                { // also remove the reverse.
                                    _graph.RemoveEdge(to, from); 
                                }
                            }
                        }
                        else
                        { 
                            // do nothing just overwrite what is there, probably a bug in OSM, two overlapping ways, sharing nodes.
                        }
                    }

                    // edge was there already but was removed,split or needs to be replaced.
                    var edgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                        fromCoordinate, toCoordinate, intermediates);
                    _graph.AddEdge(from, to, edgeData, new CoordinateArrayCollection<GeoCoordinateSimple>(intermediates.ToArray()));
                    if (_graph.IsDirected)
                    { // also the need to add the reverse edge.
                        intermediates.Reverse();
                        edgeData = (TEdgeData)edgeData.Reverse();
                        _graph.AddEdge(to, from, edgeData, new CoordinateArrayCollection<GeoCoordinateSimple>(intermediates.ToArray()));
                    }
                }
                else
                { // edge is not there yet, just add it.
                    ICoordinateCollection intermediatesCollection = null;
                    if (intermediates != null)
                    {
                        intermediatesCollection = new CoordinateArrayCollection<GeoCoordinateSimple>(intermediates.ToArray());
                    }

                    // add new edge.
                    var edgeData = this.CalculateEdgeData(_interpreter.EdgeInterpreter, _tagsIndex, tags, true,
                        fromCoordinate, toCoordinate, intermediates);
                    _graph.AddEdge(from, to, edgeData, intermediatesCollection);
                    if (_graph.IsDirected)
                    { // also the need to add the reverse edge.
                        if (intermediates != null)
                        {
                            intermediates.Reverse();
                            intermediatesCollection = new CoordinateArrayCollection<GeoCoordinateSimple>(intermediates.ToArray());
                        }
                        edgeData = (TEdgeData)edgeData.Reverse();
                        _graph.AddEdge(to, from, edgeData, intermediatesCollection);
                    }
                }
            }
        }

        /// <summary>
        /// Gets an edge from the given graph taking into account 'can have duplicates'.
        /// </summary>
        /// <returns></returns>
        private bool GetEdge(GraphBase<TEdgeData> graph, uint from, uint to, out TEdgeData existingData, out ICoordinateCollection shape)
        {
            if(!graph.CanHaveDuplicates)
            {
                graph.GetEdgeShape(from, to, out shape);
                return graph.GetEdge(from, to, out existingData);
            }
            else
            {
                var edges =  graph.GetEdges(from, to);
                while(edges.MoveNext())
                {
                    if(edges.Neighbour == to)
                    {
                        existingData = edges.EdgeData;
                        shape = edges.Intermediates;
                        return true;
                    }
                }
                existingData = default(TEdgeData);
                shape = null;
                return false;
            }
        }

        /// <summary>
        /// Calculates the edge data.
        /// </summary>
        /// <returns></returns>
        protected abstract TEdgeData CalculateEdgeData(IEdgeInterpreter edgeInterpreter, ITagsIndex tagsIndex, TagsCollectionBase tags, 
            bool tagsForward, GeoCoordinate from, GeoCoordinate to, List<GeoCoordinateSimple> intermediates);

        /// <summary>
        /// Returns true if the edge can be traversed.
        /// </summary>
        /// <param name="edgeInterpreter"></param>
        /// <param name="tagsIndex"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected abstract bool CalculateIsTraversable(IEdgeInterpreter edgeInterpreter, ITagsIndex tagsIndex,
                                              TagsCollectionBase tags);

        /// <summary>
        /// Holds the ways to cache to complete the restriction reations.
        /// </summary>
        private HashSet<long> _waysToCache;

        /// <summary>
        /// Holds the node to cache to complete the restriction relations.
        /// </summary>
        private HashSet<long> _nodesToCache;

        /// <summary>
        /// Adds a given relation.
        /// </summary>
        public override void AddRelation(Relation relation)
        {
            if (_interpreter.IsRestriction(OsmGeoType.Relation, relation.Tags))
            {
                // add the node as a possible restriction.
                if (!_preIndexMode)
                { // tests quickly if a given node is possibly a restriction.
                    // interpret the restriction using the complete object.
                    var vehicleRestrictions = _interpreter.CalculateRestrictions(relation, _dataCache);
                    if (vehicleRestrictions != null &&
                        vehicleRestrictions.Count > 0)
                    { // add all the restrictions.
                        foreach (var vehicleRestriction in vehicleRestrictions)
                        { // translated the restricted route in terms of node-id's to vertex ids.
                            var restriction = new List<uint>(vehicleRestriction.Value.Length);
                            KeyValuePair<KeyValuePair<long, uint>, KeyValuePair<long, uint>>? firstCollapsedInfo = null;
                            uint? previousRelevantId = null;
                            for (int idx = 0; idx < vehicleRestriction.Value.Length; idx++)
                            {
                                // check if relevant node.
                                uint relevantId;
                                if (this.TryGetRoadNode(vehicleRestriction.Value[idx], out relevantId))
                                { // ok, is relevant.
                                    if (firstCollapsedInfo.HasValue)
                                    { // there was an irrelevant node before this one.
                                        if (firstCollapsedInfo.Value.Key.Value == relevantId)
                                        { // ok, take the other relevant one.
                                            restriction.Add(firstCollapsedInfo.Value.Value.Value);
                                        }
                                        else if (firstCollapsedInfo.Value.Value.Value == relevantId)
                                        { // ok, take the other relevant one.
                                            restriction.Add(firstCollapsedInfo.Value.Key.Value);
                                        }
                                        else
                                        { // oeps, invalid info here.
                                            restriction = null;
                                            break;
                                        }
                                        firstCollapsedInfo = null;
                                    }
                                    if (!previousRelevantId.HasValue || previousRelevantId.Value != relevantId)
                                    { // ok, this one is new.
                                        previousRelevantId = relevantId;
                                        restriction.Add(relevantId);
                                    }
                                }
                                else
                                { // ok, not relevant, should be in the collapsed nodes.
                                    KeyValuePair<KeyValuePair<long, uint>, KeyValuePair<long, uint>> collapsedInfo;
                                    if(!_collapsedNodes.TryGetValue(vehicleRestriction.Value[idx], out collapsedInfo))
                                    { // one of the nodes was not found, this restriction is incomplete or invalid, skip it.
                                        restriction = null;
                                        break;
                                    }
                                    if(previousRelevantId.HasValue)
                                    { // ok, there is a previous relevant id, one of them should match collapsedInfo.
                                        if (collapsedInfo.Key.Value == previousRelevantId.Value)
                                        { // ok, take the other relevant one.
                                            restriction.Add(collapsedInfo.Value.Value);
                                        }
                                        else if (collapsedInfo.Value.Value == previousRelevantId.Value)
                                        { // ok, take the other relevant one.
                                            restriction.Add(collapsedInfo.Key.Value);
                                        }
                                        else
                                        { // oeps, invalid info here.
                                            restriction = null;
                                            break;
                                        }
                                    }
                                    else
                                    { // save the collapsedInfo for the first relevant node.
                                        firstCollapsedInfo = collapsedInfo;
                                    }
                                }
                            }
                            if (restriction != null)
                            { // restriction exists.
                                if (vehicleRestriction.Key == null)
                                { // this restriction is for all vehicles.
                                    _graph.AddRestriction(restriction.ToArray());
                                }
                                else
                                { // this restriction is just for the given vehicle.
                                    _graph.AddRestriction(vehicleRestriction.Key, restriction.ToArray());
                                }
                            }
                        }
                    }
                }
                else
                { // pre-index mode.
                    if (relation.Members != null && relation.Members.Count > 0)
                    { // there are members, keep them!
                        foreach (var member in relation.Members)
                        {
                            switch (member.MemberType.Value)
                            {
                                case OsmGeoType.Node:
                                    if (_nodesToCache == null)
                                    {
                                        _nodesToCache = new HashSet<long>();
                                    }
                                    _relevantNodes.Add(member.MemberId.Value);
                                    _nodesToCache.Add(member.MemberId.Value);
                                    break;
                                case OsmGeoType.Way:
                                    if (_waysToCache == null)
                                    {
                                        _waysToCache = new HashSet<long>();
                                    }
                                    _waysToCache.Add(member.MemberId.Value);
                                    _restricedWays.Add(member.MemberId.Value);
                                    break;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Registers the source for this target.
        /// </summary>
        /// <param name="source"></param>
        public override void RegisterSource(OsmStreamSource source)
        {
            this.RegisterSource(source, true);
        }

        /// <summary>
        /// Registers the source for this target.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="filterTags">The filter tags flag.</param>
        public virtual void RegisterSource(OsmStreamSource source, bool filterTags)
        {
            if (filterTags)
            { // add filter to remove all irrelevant tags.
                var tagsFilter = new OsmStreamFilterTagsFilter((TagsCollectionBase tags) =>
                {
                    var tagsToRemove = new List<Tag>();
                    foreach (var tag in tags)
                    {
                        if (!_interpreter.IsRelevant(tag.Key, tag.Value) &&
                            !Vehicle.IsRelevantForOneOrMore(tag.Key, tag.Value))
                        { // not relevant for both interpreter and all registered vehicle profiles.
                            tagsToRemove.Add(tag);
                        }
                    }
                    foreach (Tag tag in tagsToRemove)
                    {
                        tags.RemoveKeyValue(tag.Key, tag.Value);
                    }
                });
                tagsFilter.RegisterSource(source);

                base.RegisterSource(tagsFilter);
            }
            else
            { // no filter!
                base.RegisterSource(source);
            }
        }

        /// <summary>
        /// Called right before pull and right after initialization.
        /// </summary>
        /// <returns></returns>
        public override bool OnBeforePull()
        {
            // do the pull.
            this.DoPull(true, false, false);

            // reset the source.
            this.Source.Reset();

            // resize graph.
            // TODO: study avery cardinality and slightly overestimate here.
            long vertexEstimate = _relevantNodes.Count + (long)(_relevantNodes.Count * 0.1);
            _graph.Resize(vertexEstimate, (long)(vertexEstimate * 4));

            // move out of pre-index mode.
            _preIndexMode = false;

            return true;
        }

        /// <summary>
        /// Called right after pull.
        /// </summary>
        public override void OnAfterPull()
        {
            base.OnAfterPull();

            // execute pre-processor.
            if (_createPreprocessor != null)
            { // there is a function to create a preprocessor.
                var preprocessor = _createPreprocessor(_graph);
                if (preprocessor != null)
                { // there is a pre-processor, trigger execution.
                    OsmSharp.Logging.Log.TraceEvent("GraphOsmStreamTargetBase", Logging.TraceEventType.Information,
                        "Starting preprocessing...");
                    preprocessor.Start();
                }
            }

            // trim the graph.
            _graph.Compress();
        }

        /// <summary>
        /// Closes this target.
        /// </summary>
        public override void Close()
        {
            _coordinates.Clear();
            _dataCache.Clear();
            _idTransformations.Clear();
            if(_nodesToCache != null)
            {
                _nodesToCache.Clear();
            }
            if (_waysToCache != null)
            {
                _waysToCache.Clear();
            }
            _restricedWays = null;
            _collapsedNodes = null;
            _preIndex = null;
            _relevantNodes = null;
            _tagsIndex = null;
        }
    }
}