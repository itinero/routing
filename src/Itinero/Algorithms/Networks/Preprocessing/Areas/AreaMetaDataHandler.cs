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
using Itinero.Graphs;
using Itinero.LocalGeo;
using Itinero.Profiles.Lua.Interop.LuaStateInterop;

namespace Itinero.Algorithms.Networks.Preprocessing.Areas
{
    /// <summary>
    /// Adds meta-data to all edges in a given area.
    /// </summary>
    public class AreaMetaDataHandler : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly IArea _area;

        /// <summary>
        /// Creates a new area mapper.
        /// </summary>
        /// <param name="network">The network.</param>
        /// <param name="area">The area.</param>
        public AreaMetaDataHandler(RoutingNetwork network, IArea area)
        {
            _network = network;
            _area = area;
        }

        /// <summary>
        /// A delegate to notify listeners of a new vertex.
        /// </summary>
        /// <param name="newVertexId">The new vertex id.</param>
        public delegate void NewVertexDelegate(uint newVertexId);
        
        /// <summary>
        /// Gets or sets a listener to listen to new vertex notifications.
        /// </summary>
        public NewVertexDelegate NewVertex { get; set; }

        /// <summary>
        /// A delegate to notifiy listeners of a new edge.
        /// </summary>
        /// <param name="oldEdgeId">The old id.</param>
        /// <param name="newEdgeId">The new edge id, a part of the old edge.</param>
        /// <remarks>A this time both old and new edges exist.</remarks>
        public delegate void NewEdgeDelegate(uint oldEdgeId, uint newEdgeId);
        
        /// <summary>
        /// Gets or sets a listener to listen to new edge nofications.
        /// </summary>
        public NewEdgeDelegate NewEdge { get; set; }

        /// <summary>
        /// A delegate to notify listeners that an edge was found inside the area.
        /// </summary>
        /// <param name="edgeId">The edge id.</param>
        public delegate void EdgeInsideDelegate(uint edgeId);
        
        /// <summary>
        /// Gets or sets a listener to listen to edge inside notifications.
        /// </summary>
        public EdgeInsideDelegate EdgeInside { get; set; }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            const long INTERSECTION = -1;
            var vertexCountAtStart = _network.VertexCount;

            var newEdges = new List<long>(); // > 0 is inside, < 0 outside.
            var currentShape = new List<Coordinate>();

            var edgeEnumerator = _network.GetEdgeEnumerator();
            for (uint v = 0; v < _network.VertexCount; v++)
            {
                if (v >= vertexCountAtStart)
                { // all new vertices from now on.
                    break;
                }

                if (!edgeEnumerator.MoveTo(v))
                {
                    continue;
                }

                var fLocation = _network.GetVertex(v);
                var fLocationInside = _area.Overlaps(fLocation.Latitude, fLocation.Longitude);
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.DataInverted)
                    { // consider each edge only once and only in the forward direction.
                        continue;
                    }

                    if (edgeEnumerator.To >= vertexCountAtStart)
                    { // the neighbour is already a new vertex.
                        continue;
                    }

                    newEdges.Clear();

                    var edgeData = edgeEnumerator.Data;

                    // build status.
                    var hasIntersections = false;
                    Coordinate[] intersections;
                    var tLocation = _network.GetVertex(edgeEnumerator.To);
                    var status = new List<VertexStatus>(new List<Coordinate>().Count);
                    var previous = new VertexStatus()
                    {
                        Location = fLocation,
                        Vertex = v,
                        Inside = fLocationInside
                    };
                    status.Add(previous);
                    var edgeShape = edgeEnumerator.Shape;
                    if (edgeShape != null)
                    {
                        foreach (var shape in edgeShape)
                        {
                            intersections = _area.Intersect(previous.Location.Latitude, previous.Location.Longitude,
                                shape.Latitude, shape.Longitude);
                            if (intersections != null &&
                                intersections.Length > 0)
                            { // intersections where found.
                                hasIntersections = true;
                                foreach (var intersection in intersections)
                                {
                                    previous = new VertexStatus()
                                    {
                                        Location = intersection,
                                        Vertex = INTERSECTION,
                                        Inside = !previous.Inside
                                    };
                                    status.Add(previous);
                                }
                            }

                            previous = new VertexStatus()
                            {
                                Location = shape,
                                Vertex = Constants.NO_VERTEX,
                                Inside = previous.Inside
                            };
                            status.Add(previous);
                        }
                    }

                    // test intersections in the last segment.
                    intersections = _area.Intersect(previous.Location.Latitude, previous.Location.Longitude,
                        tLocation.Latitude, tLocation.Longitude);
                    if (intersections != null &&
                        intersections.Length > 0)
                    { // intersections where found.
                        hasIntersections = true;
                        foreach (var intersection in intersections)
                        {
                            previous = new VertexStatus()
                            {
                                Location = intersection,
                                Vertex = INTERSECTION,
                                Inside = !previous.Inside
                            };
                            status.Add(previous);
                        }
                    }

                    previous = new VertexStatus()
                    {
                        Location = _network.GetVertex(edgeEnumerator.To),
                        Vertex = edgeEnumerator.To,
                        Inside = previous.Inside
                    };
                    status.Add(previous);

                    // continue to the next edge if nothing is to be done.
                    if (!hasIntersections &&
                        !status[0].Inside)
                    { // edge not inside and no intersections.
                        continue;
                    }

                    // handle the easy case, no intersections but inside.
                    if (!hasIntersections &&
                        status[0].Inside)
                    { // just update edge meta data.
                        this.EdgeInside?.Invoke(edgeEnumerator.Id);
                        continue;
                    }

                    // loop over all status pairs and if inside do stuff.
                    currentShape.Clear();
                    newEdges.Clear();
                    var previousRelevant = status[0];
                    var previousDistance = 0f;
                    for (var s = 1; s < status.Count; s++)
                    {
                        var current = status[s];

                        // update distance.
                        previousDistance += Coordinate.DistanceEstimateInMeter(previousRelevant.Location,
                            current.Location);

                        if (current.Vertex == Constants.NO_VERTEX)
                        { // just a shapepoint.
                            currentShape.Add(current.Location);
                            continue;
                        }

                        if (current.Vertex == INTERSECTION)
                        { // this needs to become a new vertex and we need a new edge.
                            // add the new verex.
                            current.Vertex = _network.VertexCount;
                            _network.AddVertex((uint) current.Vertex, current.Location.Latitude,
                                current.Location.Longitude);
                            this.NewVertex?.Invoke((uint) current.Vertex);
                        }

                        // add the new edge for the segment.                  
                        var newEdgeData = edgeData;
                        newEdgeData.Distance = previousDistance;
                        if (previousDistance > _network.MaxEdgeDistance)
                        {
                            newEdgeData.Distance = _network.MaxEdgeDistance;
                        }
                        long edgeId = _network.AddEdge((uint) previousRelevant.Vertex, (uint) current.Vertex,
                            newEdgeData, currentShape);
                        if (!previousRelevant.Inside)
                        {
                            edgeId = -edgeId;
                        }
                        newEdges.Add(edgeId);

                        // prepare for the next segment.
                        currentShape.Clear();
                        previousRelevant = current;
                        previousDistance = 0;
                    }

                    // notify listeners on all new edges now that we still have the old one around.
                    foreach (var newEdgeId in newEdges)
                    {
                        if (newEdgeId < 0)
                        {
                            this.NewEdge?.Invoke(edgeEnumerator.Id, (uint)-newEdgeId);
                        }
                        else
                        {
                            this.NewEdge?.Invoke(edgeEnumerator.Id, (uint)newEdgeId);
                        }
                    }

                    // remove the old edge.
                    _network.RemoveEdge(edgeEnumerator.Id);
                    edgeEnumerator.Reset(); // don't use after this point.
                    
                    // notify listeners about the edges inside.
                    foreach (var newEdgeId in newEdges)
                    {
                        if (newEdgeId > 0)
                        {
                            this.EdgeInside?.Invoke((uint)newEdgeId);
                        }
                    }
                }
            }

            this.HasSucceeded = true;
        }

        private struct VertexStatus
        {
            /// <summary>
            /// The location of this 'vertex'.
            /// </summary>
            /// <returns></returns>
            public Coordinate Location { get; set; }

            /// <summary>
            /// True if the next segment is inside the area;
            /// </summary>
            /// <returns></returns>
            public bool Inside { get; set; }

            /// <summary>
            /// Holds the future status:
            /// - original vertex: vertexid
            /// - intersection   : -1 (is new vertex to be)
            /// - shapepoint     : Constants.NO_VERTEX
            /// </summary>
            /// <returns></returns>
            public long Vertex { get; set; }

            public override string ToString()
            {
                return string.Format("{0}-{1} @ {2}",
                    this.Vertex, this.Inside, this.Location);
            }
        }
    }
}