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

using System;
using System.Collections.Generic;
using System.Threading;
using Itinero.Data.Network;
using Itinero.Data.Network.Edges;
using Itinero.Graphs.Geometric.Shapes;
using Itinero.LocalGeo;

namespace Itinero.Algorithms.Networks.Preprocessing
{
    /// <summary>
    /// An algorithm to convert a multi graph to a simple graph by removing duplicate edges and loops.
    /// </summary>
    public class SimpleGraphConverter : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly Action<uint> _newVertex;
        private readonly Action<uint, uint> _edgeSplit;

        /// <summary>
        /// Creates a new simple graph converter.
        /// </summary>
        /// <param name="network">The network, graph, to convert to a simple version.</param>
        /// <param name="newVertex">A function to report on new vertices.</param>
        /// <param name="edgeSplit">A function called when an edge is split, to properly maintain external data about edges.</param>
        public SimpleGraphConverter(RoutingNetwork network, Action<uint, uint> edgeSplit, Action<uint> newVertex = null)
        {
            _network = network;
            _newVertex = newVertex;
            _edgeSplit = edgeSplit;
        }

        /// <summary>
        /// Excutes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var edgeEnumerator = _network.GetEdgeEnumerator();
            var neighbours = new HashSet<uint>();
            var shape = new List<Coordinate>();
            for (uint v = 0; v < _network.VertexCount; v++)
            {
                if (!edgeEnumerator.MoveTo(v))
                { // no edges here.
                    continue;
                }

                neighbours.Clear();
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.To == v ||
                        neighbours.Contains(edgeEnumerator.To))
                    { // this is a loop, split it by adding a virtual vertex.
                      // OR
                      // a duplicate edge, add a virtual vertex.

                        // collect and copy all edge info.
                        var vertex1 = v;
                        var vertex2 = edgeEnumerator.To;
                        var data = edgeEnumerator.Data;
                        shape.Clear();
                        if (edgeEnumerator.Shape != null)
                        {
                            shape.AddRange(edgeEnumerator.Shape);
                        }
                        if (edgeEnumerator.DataInverted)
                        { // cannot invert data so invert the rest.
                            vertex1 = edgeEnumerator.To;
                            vertex2 = v;
                        }
                        
                        // remove the duplicate.
                        _network.RemoveEdge(edgeEnumerator.Id);

                        // add two new edges.
                        this.AddAndSplitEdge(edgeEnumerator.Id, vertex1, vertex2, data, shape);

                        _edgeSplit(edgeEnumerator.Id, Constants.NO_EDGE);

                        // reset enumerator, graph has changed.
                        edgeEnumerator = _network.GetEdgeEnumerator();
                        edgeEnumerator.MoveTo(v);
                        neighbours.Clear();
                    }
                    else
                    {
                        neighbours.Add(edgeEnumerator.To);
                    }
                }
            }

            this.HasSucceeded = _network.GeometricGraph.Graph.MarkAsSimple();
        }

        /// <summary>
        /// Splits the given edge in two pieces.
        /// </summary>
        /// <param name="originalEdgeId">The original edge.</param>
        /// <param name="vertex1">The first vertex.</param>
        /// <param name="vertex2">The second vertex.</param>
        /// <param name="data">The data.</param>
        /// <param name="shape">The shape.</param>
        private void AddAndSplitEdge(uint originalEdgeId, uint vertex1, uint vertex2, EdgeData data, List<Coordinate> shape)
        {
            var newVertex = _network.VertexCount;

            // use the first shapepoint or the middel of the edge as the new vertex.

            if (shape.Count == 0)
            { // add a fictional shapepoint.
                if (vertex1 == vertex2)
                { // this is a loop with no shape point, it represents nothing, just remove it.
                    return;
                }
                var line = new Line(_network.GetVertex(vertex1), _network.GetVertex(vertex2));
                shape.Add(line.Middle);
            }

            // add new vertex.
            _network.AddVertex(newVertex, shape[0].Latitude, shape[0].Longitude);
            if (_newVertex != null)
            {
                _newVertex(newVertex);
            }

            var distance = Coordinate.DistanceEstimateInMeter(_network.GetVertex(vertex1), 
                shape[0]);
            if (distance > _network.MaxEdgeDistance)
            {
                throw new Exception(string.Format("Edge deteted that's too long in: {0}->{1}",
                        vertex1, newVertex));
            }
            var newEdgeId = _network.AddEdge(vertex1, newVertex, new EdgeData()
            {
                Profile = data.Profile,
                MetaId = data.MetaId,
                Distance = distance
            });
            _edgeSplit(originalEdgeId, newEdgeId);
            shape.RemoveAt(0);

            distance = System.Math.Max(data.Distance - distance, 0);
            if (distance > _network.MaxEdgeDistance)
            {
                throw new Exception(string.Format("Edge deteted that's too long in: {0}->{1}",
                        newVertex, vertex2));
            }
            newEdgeId = _network.AddEdge(newVertex, vertex2, new EdgeData()
            {
                Profile = data.Profile,
                MetaId = data.MetaId,
                Distance = distance
            }, shape);
            _edgeSplit(originalEdgeId, newEdgeId);
        }
    }
}