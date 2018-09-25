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
using System.Runtime.CompilerServices;
using System.Threading;
using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms.Restrictions;
using Itinero.Data.Network;
using Itinero.Data.Network.Edges;
using Itinero.Graphs;
using Itinero.Graphs.Directed;
using Itinero.Logging;
using Itinero.Profiles;
using Itinero.Profiles.Lua.Tree;
using Reminiscence.Arrays;

[assembly: InternalsVisibleTo("Itinero.Test")]
namespace Itinero.Algorithms.Networks.Islands
{
    /// <summary>
    /// An island detector that builds a meta-graph of islands in the network.
    /// </summary>
    public class EdgeBasedIslandDetector : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly Func<ushort, Factor> _profile;
        private readonly RestrictionCollection _restrictions;

        /// <summary>
        /// Creates a new island detector.
        /// </summary>
        /// <param name="network">The network.</param>
        /// <param name="profile">The profile.</param>
        /// <param name="restrictions">The restrictions.</param>
        public EdgeBasedIslandDetector(RoutingNetwork network, Func<ushort, Factor> profile, 
            RestrictionCollection restrictions = null)
        {
            _network = network;
            _profile = profile;
            _restrictions = restrictions;
        }
        
        private IslandLabels _islandLabels; // island id's per edge id.

        /// <summary>
        /// Runs the island detection.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _islandLabels = new IslandLabels();

            // do a run over each vertex and connect islands with bidirectional edges where possible.
            uint currentVertex = 0;
            var edgeEnumerator1 = _network.GetEdgeEnumerator();
            var edgeEnumerator2 = _network.GetEdgeEnumerator();
            var bestLabels = new Dictionary<uint, uint>();
            while (currentVertex < _network.VertexCount)
            {
                if (!edgeEnumerator1.MoveTo(currentVertex))
                {
                    currentVertex++;
                    continue;
                }

                // update restrictions.
                _restrictions?.Update(currentVertex);

                // log all connections.
                bestLabels.Clear();
                var incomingOneWayEdge = -1L;
                uint incomingVertex = 0;
                var edges = 0;
                while (edgeEnumerator1.MoveNext())
                {
                    var edge1Factor = _profile(edgeEnumerator1.Data.Profile);
                    if (edge1Factor.Value == 0)
                    {
                        // no access, don't evaluate neighbours.
                        _islandLabels[edgeEnumerator1.Id] = IslandLabels.NoAccess;
                        continue;
                    }

                    edges++;
                    if (edge1Factor.Direction != 0)
                    {
                        if (incomingOneWayEdge != Constants.NO_EDGE)
                        { // there was no bidirectional or second edge detected.
                            if ((edgeEnumerator1.DataInverted && edge1Factor.Direction == 1) ||
                                (!edgeEnumerator1.DataInverted && edge1Factor.Direction == 2))
                            {
                                if (incomingOneWayEdge < 0)
                                { // keep edge.
                                    incomingOneWayEdge = edgeEnumerator1.Id;
                                    incomingVertex = edgeEnumerator1.To;
                                }
                                else
                                { // oeps, a second incoming edge, don't keep.
                                    incomingOneWayEdge = Constants.NO_EDGE;
                                }
                            }
                        }
                        // only consider bidirectional edges in this first step.
                        continue;
                    }
                    incomingOneWayEdge = Constants.NO_EDGE; // a bidirectional edge, not a candidate for one-way label propagation.

                    // get label or provisionally set label to own id.
                    if (!bestLabels.TryGetValue(edgeEnumerator1.Id, out var edge1Label))
                    {
                        // label wasn't set yet locally, check globally.
                        edge1Label = _islandLabels[edgeEnumerator1.Id];
                        if (edge1Label == IslandLabels.NotSet)
                        {
                            // provisionally set label to own id.
                            edge1Label = edgeEnumerator1.Id;
                        }

                        bestLabels[edgeEnumerator1.Id] = edge1Label;
                    }
                    
                    var turn = new Turn(new OriginalEdge(edgeEnumerator1.To, currentVertex), Constants.NO_VERTEX);

                    // evaluate neighbours.
                    edgeEnumerator2.MoveTo(currentVertex);
                    while (edgeEnumerator2.MoveNext())
                    {
                        if (edgeEnumerator1.Id == edgeEnumerator2.Id)
                        { // don't evaluate u-turns.
                            // TODO: what about loops?
                            continue;
                        }
                        
                        var edge2Factor = _profile(edgeEnumerator2.Data.Profile);
                        if (edge2Factor.Value == 0)
                        {
                            // should be marked as no access in parent loop.
                            continue;
                        }

                        if (edge2Factor.Direction != 0)
                        {
                            // only consider bidirectional edges in this first step.
                            continue;
                        }
                        
                        // check restrictions if needed.
                        if (_restrictions != null)
                        {
                            turn.Vertex3 = edgeEnumerator2.To;
                            if (turn.IsRestrictedBy(_restrictions))
                            { // turn is restricted.
                                continue;
                            }
                        }

                        // get label or provisionally set label to own id.
                        if (!bestLabels.TryGetValue(edgeEnumerator2.Id, out var edge2Label))
                        {
                            // label wasn't set locally, check globally.
                            edge2Label = _islandLabels[edgeEnumerator2.Id];
                            if (edge2Label == IslandLabels.NotSet)
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

                if (incomingOneWayEdge != Constants.NO_EDGE &&
                    incomingOneWayEdge >= 0 && 
                    edges == 2)
                { // link sequences of oneways together.
                    var edge1Id = (uint)incomingOneWayEdge;
                    // we can also update neighbours if there is only one incoming edge.
                    
                    // get label or provisionally set label to own id.
                    if (!bestLabels.TryGetValue(edge1Id, out var edge1Label))
                    {
                        // label wasn't set yet locally, check globally.
                        edge1Label = _islandLabels[edge1Id];
                        if (edge1Label == IslandLabels.NotSet)
                        {
                            // provisionally set label to own id.
                            edge1Label = edge1Id;
                        }

                        bestLabels[edge1Id] = edge1Label;
                    }

                    var turn = new Turn(new OriginalEdge(incomingVertex, currentVertex), Constants.NO_VERTEX);

                    // evaluate neighbours.
                    edgeEnumerator2.MoveTo(currentVertex);
                    while (edgeEnumerator2.MoveNext())
                    {
                        if (edge1Id == edgeEnumerator2.Id)
                        {
                            // don't evaluate u-turns.
                            // TODO: what about loops?
                            continue;
                        }

                        var edge2Factor = _profile(edgeEnumerator2.Data.Profile);
                        if (edge2Factor.Value == 0)
                        {
                            // should be marked as no access in parent loop.
                            continue;
                        }

                        if ((edgeEnumerator2.DataInverted && edge2Factor.Direction == 1) ||
                            (!edgeEnumerator2.DataInverted && edge2Factor.Direction == 2))
                        { // REMARK: no need to check for bidirectionals, already done above.
                            // only consider outgoing oneway edges.
                            continue;
                        }
                        
                        // check restrictions if needed.
                        if (_restrictions != null)
                        {
                            turn.Vertex3 = edgeEnumerator2.To;
                            if (turn.IsRestrictedBy(_restrictions))
                            { // turn is restricted.
                                continue;
                            }
                        }

                        // get label or provisionally set label to own id.
                        if (!bestLabels.TryGetValue(edgeEnumerator2.Id, out var edge2Label))
                        {
                            // label wasn't set locally, check globally.
                            edge2Label = _islandLabels[edgeEnumerator2.Id];
                            if (edge2Label == IslandLabels.NotSet)
                            {
                                // provisionally set label to own id.
                                edge2Label = edgeEnumerator2.Id;
                            }

                            bestLabels[edgeEnumerator2.Id] = edge2Label;
                        }

                        // choose best label.
                        if (edge1Label < edge2Label)
                        {
                            bestLabels[edgeEnumerator2.Id] = edge1Label;
                        }
                        else
                        {
                            bestLabels[edge1Id] = edge2Label;
                        }
                    }
                }

                // update labels based on best labels.
                var labelsToUpdate = new HashSet<uint>();
                foreach (var pair in bestLabels)
                {
                    var edgeId = pair.Key; // the edge key.
                    var label = pair.Value;

                    var existing = _islandLabels[edgeId];
                    if (existing != IslandLabels.NotSet &&
                        existing != label &&
                        edgeId != existing)
                    { // also make sure to update the existing label.
                        _islandLabels[existing] = label;
                        labelsToUpdate.Add(existing);
                    }
                    _islandLabels[edgeId] = label;
                    labelsToUpdate.Add(edgeId);
                }
                foreach (var label in labelsToUpdate)
                {
                    _islandLabels.UpdateLowest(label);
                }

                currentVertex++;
            }

            // update all labels to lowest possible.
            for (uint e = 0; e < _islandLabels.Count; e++)
            {
                _islandLabels.UpdateLowest(e);
            }

            // NOW ALL LABELLED EDGES ARE AT THEIR BEST.
            // do the one-way labels.
            var islandLabelGraph = new IslandLabelGraph();

            currentVertex = 0;
            while (currentVertex < _network.VertexCount)
            {
                if (!edgeEnumerator1.MoveTo(currentVertex))
                {
                    currentVertex++;
                    continue;
                }
                
                // update restrictions.
                _restrictions?.Update(currentVertex);
                
                // log all connections.
                while (edgeEnumerator1.MoveNext())
                {
                    var edge1Factor = _profile(edgeEnumerator1.Data.Profile);
                    if (edge1Factor.Value == 0)
                    {
                        // no access, don't evaluate neighbours.
                        _islandLabels[edgeEnumerator1.Id] = IslandLabels.NoAccess;
                        continue;
                    }

                    if ((edgeEnumerator1.DataInverted && edge1Factor.Direction == 2) ||
                        !edgeEnumerator1.DataInverted && edge1Factor.Direction == 1)
                    {
                        // wrong direction, this edge is oneway away from current vertex.
                        continue;
                    }
                    
                    var turn = new Turn(new OriginalEdge(edgeEnumerator1.To, currentVertex), Constants.NO_VERTEX);

                    // get label or set to own id.
                    var edge1Label = _islandLabels[edgeEnumerator1.Id];
                    if (edge1Label == IslandLabels.NotSet)
                    {
                        edge1Label = edgeEnumerator1.Id;
                        _islandLabels[edge1Label] = edge1Label;
                    }

                    // evaluate neighbours.
                    edgeEnumerator2.MoveTo(currentVertex);
                    while (edgeEnumerator2.MoveNext())
                    {
                        if (edgeEnumerator1.Id == edgeEnumerator2.Id)
                        { // don't evaluate u-turns.
                            // TODO: what about loops?
                            continue;
                        }
                        
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
                        
                        // check restrictions if needed.
                        if (_restrictions != null)
                        {
                            turn.Vertex3 = edgeEnumerator2.To;
                            if (turn.IsRestrictedBy(_restrictions))
                            { // turn is restricted.
                                continue;
                            }
                        }

                        // get label or set to own id.
                        var edge2Label = _islandLabels[edgeEnumerator2.Id];
                        if (edge2Label == IslandLabels.NotSet)
                        {
                            edge2Label = edgeEnumerator2.Id;
                            _islandLabels[edge2Label] = edge2Label;
                        }

                        if (edge1Label != edge2Label)
                        {
                            islandLabelGraph.Connect(edge1Label, edge2Label);   
                        }
                    }
                }

                currentVertex++;
            }
            Itinero.Logging.Logger.Log($"{nameof(EdgeBasedIslandDetector)}.{nameof(Run)}", TraceEventType.Verbose,
                "Built directional graph.");
            
            // calculate all loops with increasing max settle settings until they are unlimited and all loops are removed.
            uint maxSettles = 1;
            Itinero.Logging.Logger.Log($"{nameof(EdgeBasedIslandDetector)}.{nameof(Run)}", TraceEventType.Verbose,
                $"Label graph has {islandLabelGraph.LabelCount} labels.");
            while (true)
            {
                Itinero.Logging.Logger.Log($"{nameof(EdgeBasedIslandDetector)}.{nameof(Run)}", TraceEventType.Verbose,
                    $"Running loop detection with {maxSettles}.");

                var lastRun = maxSettles > islandLabelGraph.LabelCount;
                
                // find loops.
                islandLabelGraph.FindLoops(maxSettles, _islandLabels, (oldLabel, newLabel) =>
                {
                    _islandLabels[oldLabel] = newLabel;
                });
                
                // update all labels to lowest possible.
                for (uint e = 0; e < _islandLabels.Count; e++)
                {
                    _islandLabels.UpdateLowest(e);
                }
                
                if (lastRun)
                {
                    break;
                }
                
                // reduce graph.
                var labelCount = islandLabelGraph.Reduce(_islandLabels);       
                Itinero.Logging.Logger.Log($"{nameof(EdgeBasedIslandDetector)}.{nameof(Run)}", TraceEventType.Verbose,
                    $"Label graph now has {labelCount} non-empty labels.");     
                maxSettles = (uint) _network.EdgeCount;
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the island labels.
        /// </summary>
        public IslandLabels IslandLabels => _islandLabels;
    }
}