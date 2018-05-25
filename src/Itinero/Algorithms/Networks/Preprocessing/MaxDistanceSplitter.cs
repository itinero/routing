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
    /// An algorithm that splits edges in two or more pieces by adding an intermediate vertex when a given distance is exceeded.
    /// </summary>
    public class MaxDistanceSplitter : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly float _maxDistance;
        private readonly float _tolerance = 1;
        private readonly Action<uint> _newVertex;
        private readonly Action<uint, uint> _edgeSplit;

        /// <summary>
        /// Creates a new distance splitter algorithm instance.
        /// </summary>
        public MaxDistanceSplitter(RoutingNetwork network, Action<uint, uint> edgeSplit, float maxDistance = Constants.DefaultMaxEdgeDistance, Action<uint> newVertex = null)
        {
            _network = network;
            _maxDistance = maxDistance;
            _newVertex = newVertex;
            _edgeSplit = edgeSplit;
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var edgeEnumerator = _network.GetEdgeEnumerator();
            var shape = new List<Coordinate>();
            for (uint v = 0; v < _network.VertexCount; v++)
            {
                if (!edgeEnumerator.MoveTo(v))
                { // no edges here.
                    continue;
                }

                while (edgeEnumerator.MoveNext())
                {
                    var data = edgeEnumerator.Data;
                    if (data.Distance < _maxDistance)
                    { // edge is within bounds.
                        continue;
                    }

                    // edge is too long, get all details and remove it.
                    var vertex1 = v;
                    var vertex2 = edgeEnumerator.To;
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

                    // add the edge again but split in a few pieces.
                    this.AddSplitEdges(edgeEnumerator.Id, vertex1, vertex2, data, shape);

                    // notify the edge was removed.
                    _edgeSplit(edgeEnumerator.Id, Constants.NO_EDGE);

                    // reset enumerator, graph was modified.
                    edgeEnumerator.MoveTo(v);
                }
            }            
            
            this.HasSucceeded = true;
        }

        /// <summary>
        /// Adds the given edge again but in multiple pieces with a given max length.
        /// </summary>
        private void AddSplitEdges(uint originalEdgeId, uint vertex1, uint vertex2, EdgeData data, List<Coordinate> shape)
        {
            var vertex1Location = _network.GetVertex(vertex1);
            var vertex2Location = _network.GetVertex(vertex2);

            // calculate total length.
            var totalDistance = 0f;
            var previousLocation = vertex1Location;
            foreach (var location in shape)
            {
                totalDistance += Coordinate.DistanceEstimateInMeter(previousLocation, location);

                previousLocation = location;
            }
            totalDistance += Coordinate.DistanceEstimateInMeter(previousLocation, 
                vertex2Location);

            // calculate edge length.
            var pieces = 2;
            while (totalDistance / pieces > _maxDistance - _tolerance)
            {
                pieces++;
            }
            var edgeDistance = totalDistance / pieces;
            
            // split edges.
            var currentDistance = 0f;
            var currentVertex1 = vertex1;
            previousLocation = _network.GetVertex(vertex1);
            var currentShape = new List<Coordinate>();
            var shapeEnumerator = shape.GetEnumerator();
            var edgeData = new EdgeData()
            {
                Profile = data.Profile,
                MetaId = data.MetaId,
                Distance = edgeDistance
            };
            var hasNextShape = shapeEnumerator.MoveNext();
                while (true)
                {
                    var location = vertex2Location;
                    if (hasNextShape)
                    {
                        location = shapeEnumerator.Current;
                    }
                    var segmentDistance = Coordinate.DistanceEstimateInMeter(previousLocation, 
                        location);
                    
                    if (currentDistance + segmentDistance > edgeDistance)
                    { // split edge here.
                        var cutoff = Itinero.LocalGeo.Extensions.LocationAfterDistance(
                            previousLocation, location, segmentDistance - (currentDistance + segmentDistance - edgeDistance));
                        
                        var currentVertex2 = _network.VertexCount;
                        _network.AddVertex(currentVertex2, cutoff.Latitude, cutoff.Longitude);
                        if (_newVertex != null)
                        {
                            _newVertex(currentVertex2);
                        }
                        var newEdgeId = _network.AddEdge(currentVertex1, currentVertex2, edgeData, currentShape);
                        _edgeSplit(originalEdgeId, newEdgeId);

                        // reset some stuff for the next segment.
                        currentDistance = 0;
                        currentShape.Clear();
                        currentVertex1 = currentVertex2;
                        previousLocation = cutoff;
                    }
                    else
                    { // just a shape point, distance not exceeded.
                        if (!hasNextShape)
                        {
                            var currentVertex2 = vertex2;
                            var newEdgeId = _network.AddEdge(currentVertex1, currentVertex2, edgeData, currentShape);
                            _edgeSplit(originalEdgeId, newEdgeId);
                            break;
                        }

                        currentShape.Add(location);
                        previousLocation = location;
                        currentDistance += segmentDistance;
                        hasNextShape = shapeEnumerator.MoveNext();
                    }
                }
        }
    }
}