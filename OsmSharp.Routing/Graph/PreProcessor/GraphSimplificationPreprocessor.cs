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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Logging;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Graphs.PreProcessing
{
    /// <summary>
    /// Pre-processor to simplify a graph.
    /// </summary>
    public class GraphSimplificationPreprocessor : IPreprocessor
    {
        /// <summary>
        /// Holds the graph.
        /// </summary>
        private GraphBase<Edge> _graph;

        /// <summary>
        /// Creates a new pre-processor.
        /// </summary>
        /// <param name="graph"></param>
        public GraphSimplificationPreprocessor(GraphBase<Edge> graph)
        {
            _graph = graph;
        }

        /// <summary>
        /// Starts pre-processing all nodes.
        /// </summary>
        public void Start()
        {
            // build the empty coordinate list.
            var emptyCoordinateList = new GeoCoordinateSimple[0];
            var verticesList = new HashSet<uint>();

            // initialize status variables.
            uint nextToProcess = 0;
            uint nextPosition = 0;

            // search edge until a real node.
            double latestProgress = 0;
            while(nextToProcess < _graph.VertexCount)
            { // keep looping until all vertices have been processed.
                // select a new vertext to select.
                var vertexToProcess = nextToProcess;
                var edges = _graph.GetEdges(vertexToProcess).ToList();
                if(edges.Count == 2)
                { // find one of the neighbours that is usefull.
                    vertexToProcess = edges[0].Neighbour;
                    edges = _graph.GetEdges(vertexToProcess).ToList();
                    verticesList.Clear();
                    verticesList.Add(vertexToProcess);
                    while(edges.Count == 2)
                    { // keep looping until there is a vertex that is usefull.
                        vertexToProcess = edges[0].Neighbour;
                        if (verticesList.Contains(vertexToProcess))
                        { // take the other vertex.
                            vertexToProcess = edges[1].Neighbour;
                            if (verticesList.Contains(vertexToProcess))
                            { // an island was detected with only vertices having two neighbours.
                                // TODO: find a way to handle this!
                                edges = new List<Edge<Edge>>(0);
                                break;
                            }
                        }
                        verticesList.Add(vertexToProcess);
                        edges = _graph.GetEdges(vertexToProcess).ToList();
                    }
                }
                if(edges.Count > 0)
                { // ok, the vertex was not already processed.
                    nextPosition++;
                    var oldEdges = new List<Edge<Edge>>(edges);
                    var ignoreList = new HashSet<uint>();
                    foreach (var oldEdge in oldEdges)
                    { 
                        if(ignoreList.Contains(oldEdge.Neighbour))
                        { // ignore this edge: already removed in a previous iteration.
                            break;
                        }

                        // don't re-process edges that already have coordinates.
                        ICoordinateCollection oldEdgeValueCoordinates;
                        _graph.GetEdgeShape(vertexToProcess, oldEdge.Neighbour, out oldEdgeValueCoordinates);
                        if (oldEdgeValueCoordinates != null)
                        { // this edge has already been processed.
                            break;
                        }

                        // STEP1: Build list of vertices that are only for form.

                        // set current/previous.
                        var distance = oldEdge.EdgeData.Distance;
                        var current = oldEdge.Neighbour;
                        var previous = vertexToProcess;

                        // build list of vertices.
                        var vertices = new List<uint>();
                        vertices.Add(previous);
                        vertices.Add(current);

                        // get next edges list.
                        var nextEdges = _graph.GetEdges(current).ToList();
                        while (nextEdges.Count == 2)
                        { // ok the current vertex can be removed.
                            var nextEdge = nextEdges[0];
                            if (nextEdge.Neighbour == previous)
                            { // it's the other edge!
                                nextEdge = nextEdges[1];
                            }

                            // compare edges.
                            if(nextEdge.EdgeData.Forward != oldEdge.EdgeData.Forward ||
                                nextEdge.EdgeData.Tags != oldEdge.EdgeData.Tags)
                            { // oeps, edges are different!
                                break;
                            }

                            // check for intermediates.
                            ICoordinateCollection nextEdgeValueCoordinates;
                            _graph.GetEdgeShape(current, nextEdge.Neighbour, out nextEdgeValueCoordinates);
                            if (nextEdgeValueCoordinates != null)
                            { // oeps, there are intermediates already, this can occur when two osm-ways are drawn on top of eachother.
                                break;
                            }

                            // add distance.
                            distance = distance + nextEdge.EdgeData.Distance;

                            // set current/previous.
                            previous = current;
                            current = nextEdge.Neighbour;
                            vertices.Add(current);

                            // get next edges.
                            nextEdges = _graph.GetEdges(current).ToList();
                        }

                        // check if the edge contains intermediate points.
                        if (vertices.Count == 2)
                        { // no intermediate points: add the empty coordinate list.
                            var oldEdgeValue = oldEdge.EdgeData;
                            
                            // keep edges that already have intermediates.
                            ICoordinateCollection edgeToKeepValueCoordinates = null;
                            var edgesToKeep = new List<Tuple<uint, Edge, ICoordinateCollection>>();
                            foreach (var edgeToKeep in _graph.GetEdges(vertexToProcess).ToList())
                            {
                                edgeToKeepValueCoordinates = null;
                                if(edgeToKeep.Neighbour == oldEdge.Neighbour && 
                                   _graph.GetEdgeShape(vertexToProcess, edgeToKeep.Neighbour, out edgeToKeepValueCoordinates))
                                {
                                    edgesToKeep.Add(new Tuple<uint, Edge, ICoordinateCollection>(
                                        edgeToKeep.Neighbour, edgeToKeep.EdgeData, edgeToKeepValueCoordinates));
                                }
                            }

                            // delete olds arcs.
                            _graph.RemoveEdge(vertexToProcess, oldEdge.Neighbour);

                            // add new arc.
                            if (oldEdgeValue.Forward)
                            {
                                _graph.AddEdge(vertexToProcess, oldEdge.Neighbour, oldEdgeValue, null);
                            }
                            else
                            {
                                _graph.AddEdge(vertexToProcess, oldEdge.Neighbour, (Edge)oldEdgeValue.Reverse(), null);
                            }

                            // add edges to keep.
                            foreach(var edgeToKeep in edgesToKeep)
                            {
                                _graph.AddEdge(vertexToProcess, edgeToKeep.Item1, edgeToKeep.Item2, edgeToKeep.Item3);
                            }
                        }
                        else
                        { // intermediate points: build array.
                            // STEP2: Build array of coordinates.
                            var coordinates = new GeoCoordinateSimple[vertices.Count - 2];
                            float latitude, longitude;
                            for (int idx = 1; idx < vertices.Count - 1; idx++)
                            {
                                _graph.GetVertex(vertices[idx], out latitude, out longitude);
                                coordinates[idx - 1] = new GeoCoordinateSimple()
                                {
                                    Latitude = latitude,
                                    Longitude = longitude
                                };
                            }

                            // STEP3: Remove all unneeded edges.
                            _graph.RemoveEdge(vertices[0], vertices[1]); // remove first edge.
                            for (int idx = 1; idx < vertices.Count - 1; idx++)
                            { // delete all intermidiate arcs.
                                _graph.RemoveEdges(vertices[idx]);
                            }
                            _graph.RemoveEdge(vertices[vertices.Count - 1], vertices[vertices.Count - 2]); // remove last edge.
                            if (vertices[0] == vertices[vertices.Count - 1])
                            { // also remove outgoing edge.
                                ignoreList.Add(vertices[vertices.Count - 2]); // make sure this arc is ignored in next iteration.
                            }

                            // STEP4: Add new edge.
                            if (oldEdge.EdgeData.Forward)
                            {
                                _graph.AddEdge(vertices[0], vertices[vertices.Count - 1], new Edge()
                                {
                                    Forward = oldEdge.EdgeData.Forward,
                                    Tags = oldEdge.EdgeData.Tags,
                                    Distance = distance
                                }, new CoordinateArrayCollection<GeoCoordinateSimple>(coordinates));
                            }
                            else
                            {
                                var reverse = new GeoCoordinateSimple[coordinates.Length];
                                coordinates.CopyToReverse(reverse, 0);
                                _graph.AddEdge(vertices[vertices.Count - 1], vertices[0], new Edge()
                                {
                                    Forward = !oldEdge.EdgeData.Forward,
                                    Tags = oldEdge.EdgeData.Tags,
                                    Distance = distance
                                }, new CoordinateArrayCollection<GeoCoordinateSimple>(reverse));
                            }
                        }
                    }
                }
                // move to the next position.
                nextToProcess++;

                // report progress.
                float progress = (float)System.Math.Round((((double)nextToProcess / (double)_graph.VertexCount) * 100));
                if (progress != latestProgress)
                {
                    OsmSharp.Logging.Log.TraceEvent("Preprocessor", TraceEventType.Information,
                        "Removing edges... {0}%", progress);
                    latestProgress = progress;
                }
            }

            // compress the graph.
            this.CompressGraph();
        }

        /// <summary>
        /// Compresses the graph by deleting vertices.
        /// </summary>
        private void CompressGraph()
        {
            // initialize status variables.
            uint vertex = 1;
            uint nextCompressedPosition = 1;

            // search edge until a real node.
            float latitude, longitude;
            float latestProgress = -1;
            while (vertex <= _graph.VertexCount)
            {
                var edges =  _graph.GetEdges(vertex).ToList();
                if (edges != null && edges.Count > 0)
                { // ok, this vertex has edges.
                    if (nextCompressedPosition != vertex)
                    { // this vertex should go in another place.
                        _graph.GetVertex(vertex, out latitude, out longitude);

                        // set the next coordinates.
                        _graph.SetVertex(nextCompressedPosition, latitude, longitude);

                        // set the new edges.
                        _graph.RemoveEdges(nextCompressedPosition);
                        foreach (var edge in edges)
                        { // add all arcs.
                            if (edge.Neighbour != vertex)
                            { // this edge is not an edge that has the same end-start point.
                                if(edge.EdgeData.Forward)
                                {
                                    _graph.AddEdge(nextCompressedPosition, edge.Neighbour, edge.EdgeData, null);
                                }
                                else
                                {
                                    _graph.AddEdge(nextCompressedPosition, edge.Neighbour, (Edge)edge.EdgeData.Reverse(), null);
                                }
                            }
                            else
                            { // this edge is an edge that has the same end-start point.
                                _graph.AddEdge(nextCompressedPosition, nextCompressedPosition, edge.EdgeData, null);
                            }

                            // update other arcs.
                            if (edge.Neighbour != vertex)
                            { // do not update other arcs if other vertex is the same.
                                var reverseEdges = _graph.GetEdges(edge.Neighbour).ToList();
                                if (reverseEdges != null)
                                { // there are reverse edges, check if there is a reference to vertex.
                                    reverseEdges = new List<Edge<Edge>>(reverseEdges);
                                    foreach (var reverseEdge in reverseEdges)
                                    { // check each edge for vertex.
                                        if (reverseEdge.Neighbour == vertex)
                                        { // ok, replace this edge.
                                            ICoordinateCollection reverseEdgeCoordinates;
                                            if (!_graph.GetEdgeShape(edge.Neighbour, reverseEdge.Neighbour, out reverseEdgeCoordinates))
                                            {
                                                reverseEdgeCoordinates = null;
                                            }

                                            _graph.RemoveEdge(edge.Neighbour, vertex);
                                            if(reverseEdgeCoordinates == null)
                                            {
                                                _graph.AddEdge(edge.Neighbour, nextCompressedPosition, (Edge)reverseEdge.EdgeData.Reverse(), null);
                                            }
                                            else if (reverseEdge.EdgeData.Forward)
                                            {
                                                _graph.AddEdge(edge.Neighbour, nextCompressedPosition, reverseEdge.EdgeData, reverseEdgeCoordinates);
                                            }
                                            else
                                            {
                                                _graph.AddEdge(edge.Neighbour, nextCompressedPosition, (Edge)reverseEdge.EdgeData.Reverse(), reverseEdgeCoordinates.Reverse());
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    nextCompressedPosition++;
                }

                // move to the next vertex.
                vertex++;

                // report progress.
                float progress = (float)System.Math.Round((((double)vertex / (double)_graph.VertexCount) * 100));
                if (progress != latestProgress)
                {
                    OsmSharp.Logging.Log.TraceEvent("Preprocessor", TraceEventType.Information,
                        "Compressing graph... {0}%", progress);
                    latestProgress = progress;
                }
            }

            // remove all extra space.
            _graph.Compress();
            _graph.Trim();
        }

        /// <summary>
        /// Returns true if the given edge1 overlaps the given edge2.
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <returns></returns>
        public bool Overlaps(Edge edge1, Edge edge2)
        {
            if (edge1.Forward == edge2.Forward &&
                edge1.Tags == edge2.Tags)
            {
                return true;
            }
            return false;
        }
    }
}