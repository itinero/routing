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
using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Data.Network;
using Itinero.Data.Network.Edges;
using Itinero.Graphs;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using Itinero.Profiles.Lua.Tree;
using Reminiscence.Arrays;

namespace Itinero.Algorithms.Networks.Islands
{
    /// <summary>
    /// An island detector that builds a meta-graph of islands in the network.
    /// </summary>
    public class EdgeBasedIslandDetector : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly Func<ushort, Factor> _profile;

        /// <summary>
        /// Creates a new island detector.
        /// </summary>
        /// <param name="network">The network.</param>
        /// <param name="profile">The profile.</param>
        public EdgeBasedIslandDetector(RoutingNetwork network, Func<ushort, Factor> profile)
        {
            _network = network;
            _profile = profile;
        }

        // defines some special values that shouldn't be used as island id's.
        public readonly uint NotSet = uint.MaxValue;
        public readonly uint NoAccess = uint.MaxValue - 1;
        
        private MemoryArray<uint> _islandIds; // island id's per edge id.

        /// <summary>
        /// Runs the island detection.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _islandIds = new MemoryArray<uint>(1);

            var islandLabels = new IslandLabels();

            // do a run over each vertex and connect islands with bidirectional edges where possible.
            uint currentVertex = 0;
            var edgeEnumerator1 = _network.GetEdgeEnumerator();
            var edgeEnumerator2 = _network.GetEdgeEnumerator();
            var bestLabels = new Dictionary<uint, uint>();
            while (currentVertex < _network.VertexCount)
            {
                if (!edgeEnumerator1.MoveTo(currentVertex))
                {
                    continue;
                }

                // log all connections.
                bestLabels.Clear();
                while (edgeEnumerator1.MoveNext())
                {
                    var edge1Factor = _profile(edgeEnumerator1.Data.Profile);
                    if (edge1Factor.Value == 0)
                    {
                        // no access, don't evaluate neighbours.
                        islandLabels[edgeEnumerator1.Id] = IslandLabels.NoAccess;
                        continue;
                    }

                    if (edge1Factor.Direction != 0)
                    {
                        // only consider bidirectional edges in this first step.
                        continue;
                    }

                    // get label or provisionally set label to own id.
                    if (!bestLabels.TryGetValue(edgeEnumerator1.Id, out var edge1Label))
                    {
                        // label wasn't set yet locally, check globally.
                        edge1Label = islandLabels[edgeEnumerator1.Id];
                        if (edge1Label == NotSet)
                        {
                            // provisionally set label to own id.
                            edge1Label = edgeEnumerator1.Id;
                        }

                        bestLabels[edgeEnumerator1.Id] = edge1Label;
                    }

                    // evaluate neighbours.
                    edgeEnumerator2.MoveTo(currentVertex);
                    while (edgeEnumerator2.MoveNext())
                    {
                        var edge2Factor = _profile(edgeEnumerator2.Data.Profile);
                        if (edge2Factor.Value == 0)
                        {
                            // should be marked as no access in parent loop.
                            continue;
                        }

                        if (edge2Factor.Direction != 0)
                        {
                            // only considder bidirectional edges in this first step.
                            continue;
                        }

                        // get label or provisionally set label to own id.
                        if (!bestLabels.TryGetValue(edgeEnumerator2.Id, out var edge2Label))
                        {
                            // label wasn't set locally, check globally.
                            edge2Label = islandLabels[edgeEnumerator2.Id];
                            if (edge2Label == NotSet)
                            {
                                // provisionally set label to own id.
                                edge2Label = edgeEnumerator2.Id;
                            }

                            bestLabels[edgeEnumerator2.Id] = edge2Label;
                        }

                        // bidirectional edge, choose best label.
                        if (edge1Label < edge2Label)
                        {
                            bestLabels[edgeEnumerator2.Id] = edge1Label;
                        }
                        else
                        {
                            bestLabels[edgeEnumerator1.Id] = edge2Label;
                        }
                    }
                }

                // update labels based on best labels.
                var labelsToUpdate = new HashSet<uint>();
                foreach (var pair in bestLabels)
                {
                    var edgeId = pair.Key; // the edge key.
                    var label = pair.Value;

                    islandLabels[edgeId] = label;
                    islandLabels.UpdateLowest(edgeId);
                }
            }

            // update all labels to lowest possible.
            for (uint e = 0; e < islandLabels.Count; e++)
            {
                islandLabels.UpdateLowest(e);
            }

            // NOW ALL LABELLED EDGES ARE AT THEIR BEST.
            // do the one-way labels.
            var islandLabelGraph = new IslandLabelGraph();

            currentVertex = 0;
            while (currentVertex < _network.VertexCount)
            {
                if (!edgeEnumerator1.MoveTo(currentVertex))
                {
                    continue;
                }

                // log all connections.
                while (edgeEnumerator1.MoveNext())
                {
                    var edge1Factor = _profile(edgeEnumerator1.Data.Profile);
                    if (edge1Factor.Value == 0)
                    {
                        // no access, don't evaluate neighbours.
                        islandLabels[edgeEnumerator1.Id] = IslandLabels.NoAccess;
                        continue;
                    }

                    if ((edgeEnumerator1.DataInverted && edge1Factor.Direction == 2) ||
                        !edgeEnumerator1.DataInverted && edge1Factor.Direction == 1)
                    {
                        // wrong direction, this edge is oneway away from current vertex.
                        continue;
                    }

                    // get label or set to own id.
                    var edge1Label = islandLabels[edgeEnumerator1.Id];
                    if (edge1Label == NotSet)
                    {
                        edge1Label = edgeEnumerator1.Id;
                    }

                    // evaluate neighbours.
                    edgeEnumerator2.MoveTo(currentVertex);
                    while (edgeEnumerator2.MoveNext())
                    {
                        var edge2Factor = _profile(edgeEnumerator2.Data.Profile);
                        if (edge2Factor.Value == 0)
                        {
                            // should be marked as no access in parent loop.
                            continue;
                        }

                        if ((edgeEnumerator2.DataInverted && edge2Factor.Direction == 1) ||
                            !edgeEnumerator2.DataInverted && edge2Factor.Direction == 2)
                        {
                            // wrong direction, this edge is oneway away from current vertex.
                            continue;
                        }

                        // get label or set to own id.
                        var edge2Label = islandLabels[edgeEnumerator2.Id];
                        if (edge2Label == NotSet)
                        {
                            edge2Label = edgeEnumerator2.Id;
                        }

                        if ((edge1Label == edgeEnumerator1.Id &&
                            edge1Factor.Direction != 0) ||
                            (edge2Label == edgeEnumerator2.Id &&
                             edge2Factor.Direction != 0))
                        { // one of the two is oneway.
                            islandLabelGraph.Connect(edge1Label, edge2Label);   
                        }
                    }
                }
            }
        }
    }
}