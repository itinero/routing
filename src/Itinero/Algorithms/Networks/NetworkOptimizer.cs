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

using Itinero.LocalGeo;
using Itinero.Data.Network;
using System.Collections.Generic;
using Itinero.Algorithms.Restrictions;
using System;
using System.Threading;

namespace Itinero.Algorithms.Networks
{
    /// <summary>
    /// An algorithm that optimizes a network by removing obsolete vertices.
    /// </summary>
    public class NetworkOptimizer : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly MergeDelegate _merge;
        private readonly Func<uint, bool> _hasRestriction;
        private readonly float _simplifyEpsilonInMeter;

        /// <summary>
        /// A delegate to verify if anything external is blocking the merging of the given edges.
        /// </summary>
        /// <param name="edgeId1">The first edge.</param>
        /// <param name="edgeId2">The second edge.</param>
        public delegate bool CanMergeDelegate(uint edgeId1, uint edgeId2);
        
        /// <summary>
        /// Gets or sets a listener to verify if edge can be merged or not.
        /// </summary>
        public CanMergeDelegate CanMerge { get; set; }

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
        /// A delegate to control how to merging two edges.
        /// </summary>
        public delegate bool MergeDelegate(Data.Network.Edges.EdgeData edgeData1, bool inverted1,
            Data.Network.Edges.EdgeData edgeData2, bool inverted2, out Data.Network.Edges.EdgeData mergedEdgeData, out bool mergedInverted);

        /// <summary>
        /// Creates a new network optimizer algorithm.
        /// </summary>
        public NetworkOptimizer(RoutingNetwork network, Func<uint, bool> hasRestriction, MergeDelegate merge, float simplifyEpsilonInMeter = 0.1f)
        {
            _network = network;
            _merge = merge;
            _hasRestriction = hasRestriction;
            _simplifyEpsilonInMeter = simplifyEpsilonInMeter;
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var edges = new List<RoutingEdge>();
            for (uint vertex = 0; vertex < _network.VertexCount; vertex++)
            {
                edges.Clear();
                edges.AddRange(_network.GetEdgeEnumerator(vertex));
                if (edges.Count == 2)
                {
                    if (_hasRestriction(vertex))
                    { // don't remove vertices that have restrictions.
                        continue;
                    }
                    
                    bool inverted;
                    Data.Network.Edges.EdgeData edgeData;
                    if (edges[0].To == edges[1].To ||
                        (this.CanMerge != null && !this.CanMerge(edges[0].Id, edges[1].Id)) || !_merge(edges[0].Data,
                            !edges[0].DataInverted, edges[1].Data, edges[1].DataInverted,
                            out edgeData, out inverted)) continue; // targets cannot be merged.

                    if (edgeData.Distance < _network.MaxEdgeDistance &&
                        !_network.ContainsEdge(edges[0].To, edges[1].To))
                    { // network does not contain edge yet and new edge < MAX_DISTANCE.
                        var shape = new List<Coordinate>();
                        var shape1 = edges[0].Shape;
                        if (shape1 != null)
                        { // add coordinates of first shape.
                            if (!edges[0].DataInverted)
                            { // data is not inverted.
                                shape.AddRange(shape1.Reverse());
                            }
                            else
                            { // data is inverted.
                                shape.AddRange(shape1);
                            }
                        }
                        shape.Add(_network.GetVertex(vertex));
                        var shape2 = edges[1].Shape;
                        if (shape2 != null)
                        { // add coordinates of first shape.
                            if (!edges[1].DataInverted)
                            { // data is not inverted.
                                shape.AddRange(shape2);
                            }
                            else
                            { // data is inverted.
                                shape.AddRange(shape2.Reverse());
                            }
                        }

                        // add edges.
                        if (!inverted)
                        { // just add 0->1
                            var newEdgeId = _network.AddEdge(edges[0].To, edges[1].To, edgeData, shape.Simplify(_simplifyEpsilonInMeter));
                            this.NewEdge?.Invoke(edges[0].Id, newEdgeId);
                            this.NewEdge?.Invoke(edges[1].Id, newEdgeId);
                        }
                        else
                        { // add the reverse 1->0
                            shape.Reverse();
                            var newEdgeId = _network.AddEdge(edges[1].To, edges[0].To, edgeData, shape.Simplify(_simplifyEpsilonInMeter));
                            this.NewEdge?.Invoke(edges[0].Id, newEdgeId);
                            this.NewEdge?.Invoke(edges[1].Id, newEdgeId);
                        }

                        // remove edges.
                        _network.RemoveEdges(vertex);
                    }
                }
            }

            this.HasSucceeded = true;
        }
    }
}