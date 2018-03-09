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
using Itinero.Data.Network;
using Itinero.LocalGeo;

namespace Itinero.Algorithms.Networks.Preprocessing.Areas
{
    /// <summary>
    /// Adds meta-data to all edges in a given area.
    /// </summary>
    public class AreaMetaDataHandler : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly IArea _area;
        private readonly NotifyNewEdgeDelegate _newEdgeCallback;

        /// <summary>
        /// Creates a new area mapper.
        /// </summary>
        /// <param name="routerDb">The router db.</param>
        /// <param name="area">The area.</param>
        /// <param name="newEdgeCallback">The callback to handle new edges.</param>
        public AreaMetaDataHandler(RouterDb routerDb, IArea area,
            NotifyNewEdgeDelegate newEdgeCallback)
        {
            _routerDb = routerDb;
            _area = area;
            _newEdgeCallback = newEdgeCallback;
        }

        /// <summary>
        /// A delegate to notifiy an edge that has split.
        /// </summary>
        /// <param name="edgeId">The edge id.</param>
        /// <param name="vertices">The vertices being inserted.</param>
        public delegate void NotifyEdgeSplitDelegate(uint edgeId, IReadOnlyList<uint> vertices);

        /// <summary>
        /// Gets or sets a function to notify about an edge that was split.
        /// </summary>
        /// <returns></returns>
        public NotifyEdgeSplitDelegate NotifyEdgeSplit { get; set; }

        /// <summary>
        /// A delegate to notify listeners when there is a new edge.
        /// </summary>
        /// <param name="edgeId">The edge id.</param>
        /// <param name="inside">True if the edge is inside the area.</param>
        public delegate void NotifyNewEdgeDelegate(uint edgeId, bool inside);

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            var vertexCountAtStart = _routerDb.Network.VertexCount;

            var currentShape = new List<Coordinate>();
            var newVertices = new List<uint>();

            var edgeEnumerator = _routerDb.Network.GetEdgeEnumerator();
            for (uint v = 0; v < _routerDb.Network.VertexCount; v++)
            {
                if (v >= vertexCountAtStart)
                { // all new vertices from now on.
                    break;
                }

                if (!edgeEnumerator.MoveTo(v))
                {
                    continue;
                }

                var fLocation = _routerDb.Network.GetVertex(v);
                var fLocationInside = _area.Overlaps(fLocation.Latitude, fLocation.Longitude);
                while (edgeEnumerator.MoveNext())
                {
                    if (v > edgeEnumerator.To)
                    { // consider each edge only once.
                        continue;
                    }

                    if (edgeEnumerator.To >= vertexCountAtStart)
                    { // the neighbour is already a new vertex.
                        continue;
                    }

                    newVertices.Clear();

                    var edgeData = edgeEnumerator.Data;

                    // build status.
                    var hasIntersections = false;
                    Coordinate[] intersections;
                    var tLocation = _routerDb.Network.GetVertex(edgeEnumerator.To);
                    var status = new List<VertexStatus>(currentShape.Count);
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
                                        Vertex = Constants.NO_VERTEX,
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
                                Vertex = Constants.NO_VERTEX,
                                Inside = !previous.Inside
                            };
                            status.Add(previous);
                        }
                    }

                    previous = new VertexStatus()
                    {
                        Location = _routerDb.Network.GetVertex(edgeEnumerator.To),
                        Vertex = edgeEnumerator.To,
                        Inside = previous.Inside
                    };
                    status.Add(previous);

                    // continue to the next edge if nothing is to be done.
                    if (!hasIntersections &&
                        !status[0].Inside)
                    {
                        continue;
                    }

                    // build the new meta-data for inside segments.
                    uint metaId = 0;
                    ushort profileId = 0;

                    // handle the easy case, no intersections but inside.
                    if (!hasIntersections &&
                        status[0].Inside)
                    { // just update edge meta data.
                        _routerDb.Network.UpdateEdgeData(edgeEnumerator.Id,
                            new Data.Network.Edges.EdgeData()
                            {
                                MetaId = metaId,
                                    Profile = profileId,
                                    Distance = edgeEnumerator.Data.Distance
                            });
                        continue;
                    }

                    // loop over all status pairs and if inside add meta-data.
                    currentShape.Clear();
                    newVertices.Clear();
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

                        if (current.Vertex == -1)
                        { // this needs to become a new vertex and we need a new edge.
                            // add the new verex.
                            current.Vertex = _routerDb.Network.VertexCount;
                            newVertices.Add((uint) current.Vertex);
                            _routerDb.Network.AddVertex((uint) current.Vertex, current.Location.Latitude,
                                current.Location.Longitude);
                        }

                        // add the new edge for the segment.
                        var newEdgeData = edgeData;
                        newEdgeData.Distance = previousDistance;
                        if (previousDistance > _routerDb.Network.MaxEdgeDistance)
                        {
                            newEdgeData.Distance = _routerDb.Network.MaxEdgeDistance;
                        }
                        if (previousRelevant.Inside)
                        {
                            newEdgeData.MetaId = metaId;
                            newEdgeData.Profile = profileId;
                        }
                        var edgeId = _routerDb.Network.AddEdge((uint) previousRelevant.Vertex, (uint) current.Vertex,
                            newEdgeData, currentShape);

                        // notify listeners about the new edge.
                        _newEdgeCallback(edgeId, previousRelevant.Inside);

                        // prepare for the next segment.
                        currentShape.Clear();
                        previousRelevant = current;
                        previousDistance = 0;
                    }

                    // notify edge was split if anyone is listening.
                    if (this.NotifyEdgeSplit != null)
                    {
                        this.NotifyEdgeSplit(edgeEnumerator.Id, newVertices);
                    }

                    // remove the old edge.
                    _routerDb.Network.RemoveEdge(edgeEnumerator.Id);
                    edgeEnumerator.Reset(); // don't use after this point.
                }
            }
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
        }
    }
}