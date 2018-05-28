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
using Itinero.Data.Network;
using Itinero.Graphs;
using Itinero.Profiles;
using Reminiscence.Arrays;

namespace Itinero.Algorithms.Networks.Islands
{
    /// <summary>
    /// An island detector that builds a meta-graph of islands in the network.
    /// </summary>
    public class EdgeBasedIslandDetector : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly Func<ushort, Factor> _profile;

        /// <summary>
        /// Creates a new island detector.
        /// </summary>
        /// <param name="routerDb">The router db.</param>
        /// <param name="profile">The profile.</param>
        public EdgeBasedIslandDetector(RouterDb routerDb, Func<ushort, Factor> profile)
        {
            _routerDb = routerDb;
            _profile = profile;
            
            _islandGraph = new Graph(1);
        }

        // defines some special values that shouldn't be used as island id's.
        private readonly uint NotSet = uint.MaxValue;
        private readonly uint NoAccess = uint.MaxValue - 1;
        
        private readonly Graph _islandGraph; // connects islands by id.
        private MemoryArray<uint> _islandIds; // island id's per edge id.

        private uint GetIslandId(DirectedEdgeId edge)
        {
            var rawId = edge.Raw;
            if (_islandIds.Length <= rawId)
            {
                var length = _islandIds.Length;
                _islandIds.EnsureMinimumSize(rawId);
                for (var i = length; i < _islandIds.Length; i++)
                {
                    _islandIds[i] = NotSet;
                }
            }

            return _islandIds[rawId];
        }

        private void SetIslandId(DirectedEdgeId edge, uint islandId)
        {
            _islandIds[edge.Raw] = islandId;
        }

        private uint NewIslandId()
        {
            var newId = _islandGraph.VertexCount;
            _islandGraph.AddVertex(newId);
            return newId;
        }

        private uint ConnectIslands(uint island1Id, uint island2Id)
        {
            
        }

        /// <summary>
        /// Runs the island detection.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _islandIds = new MemoryArray<uint>(1);
            uint nextIslandId = 0;
            
            var network = _routerDb.Network;
            var edgeEnumerator1 = network.GetEdgeEnumerator();
            var edgeEnumerator2 = network.GetEdgeEnumerator();
            var usedAsStart = new HashSet<uint>();
            for (uint v = 0; v < network.VertexCount; v++)
            {
                if (!edgeEnumerator1.MoveTo(v))
                {
                    continue;
                }
                edgeEnumerator2.MoveTo(v);

                // mark edge using it's neigbours.
                while (true)
                {
                    // find the edge with the lowest island id as a start.
                    while (edgeEnumerator1.MoveNext())
                    {
                        var edge1Id = edgeEnumerator1.Id;
                        var edge1BackwardId = edgeEnumerator1.DirectedEdgeId(); // from current vertex -> neighbour.
                        var edge1ForwardId = edge1BackwardId.Reverse; // from neighbour -> current vertex.
                        
                        // get edge factor.
                        var edge1Factor = _profile(edgeEnumerator1.Data.Profile);
                        
                        // evaluate forward.
                        var forward1IslandId = this.GetIslandId(edge1ForwardId);
                        if (forward1IslandId == NotSet)
                        {
                            if (edge1Factor.Value == 0)
                            { // no access.
                                forward1IslandId = NoAccess;
                                this.SetIslandId(edge1ForwardId, NoAccess);
                            }
                            else if ((edge1ForwardId.Forward && edge1Factor.Direction == 2) ||
                                (!edge1ForwardId.Forward && edge1Factor.Direction == 1))
                            { // no access in the correct direction.
                                forward1IslandId = NoAccess;
                                this.SetIslandId(edge1ForwardId, NoAccess);
                            }
                        }
                        
                        // evaluate backward.
                        var backward1IslandId = this.GetIslandId(edge1BackwardId);
                        if (backward1IslandId == NotSet)
                        {
                            if (edge1Factor.Value == 0)
                            { // no access.
                                backward1IslandId = NoAccess;
                                this.SetIslandId(edge1BackwardId, NoAccess);
                            }
                            else if ((edge1BackwardId.Forward && edge1Factor.Direction == 2) ||
                                (!edge1BackwardId.Forward && edge1Factor.Direction == 1))
                            { // no access in the correct direction.
                                backward1IslandId = NoAccess;
                                this.SetIslandId(edge1BackwardId, NoAccess);
                            }
                        }

                        if (backward1IslandId == NoAccess &&
                            forward1IslandId == NoAccess)
                        { // no need to check if neighbours need an update here.
                            continue;
                        }

                        edgeEnumerator2.Reset();
                        while (edgeEnumerator2.MoveNext())
                        {
                            var edge2Id = edgeEnumerator2.Id;
                            if (edge2Id == edge1Id)
                            { // don't check u-turns.
                                continue;
                            }
                            
                            var edge2ForwardId = edgeEnumerator2.DirectedEdgeId(); // from current vertex -> neighbour.
                            var edge2BackwardId = edge2ForwardId.Reverse; // from neighbour -> current vertex.
                            
                            // get edge factor.
                            var edge2Factor = _profile(edgeEnumerator1.Data.Profile);
                            
                            // evaluate forward.
                            var forward2IslandId = this.GetIslandId(edge2ForwardId);
                            if (forward2IslandId == NotSet)
                            { // not set yet.
                                if (edge2Factor.Value == 0)
                                { // no access.
                                    forward2IslandId = NoAccess;
                                    this.SetIslandId(edge2ForwardId, NoAccess);
                                }
                                else if ((edge2ForwardId.Forward && edge2Factor.Direction == 2) ||
                                         (!edge2ForwardId.Forward && edge2Factor.Direction == 1))
                                { // no access in the correct direction.
                                    forward2IslandId = NoAccess;
                                    this.SetIslandId(edge2ForwardId, NoAccess);
                                }
                            }
                            if (forward2IslandId != NoAccess)
                            { // just update with whatever comes from forward1.
                                if (forward1IslandId == NoAccess &&
                                    forward2IslandId == NotSet)
                                { // start a new island here, the incoming edge has not access.
                                    forward2IslandId = this.NewIslandId();
                                }
                                else if (forward2IslandId == NotSet)
                                { // take island id from forward1.
                                    forward2IslandId = forward1IslandId;
                                }
                                else
                                { // connect island1 -> island2.
                                    forward2IslandId = this.ConnectIslands(forward1IslandId, forward2IslandId);
                                }
                                this.SetIslandId(edge2ForwardId, forward2IslandId);
                            }
                            
                            // evaluate backward.
                            var backward2IslandId = this.GetIslandId(edge2BackwardId);
                            if (backward2IslandId == NotSet)
                            { // not set yet.
                                if (edge2Factor.Value == 0)
                                { // no access.
                                    backward2IslandId = NoAccess;
                                    this.SetIslandId(edge2BackwardId, NoAccess);
                                }
                                else if ((edge2BackwardId.Forward && edge2Factor.Direction == 2) ||
                                         (!edge2BackwardId.Forward && edge2Factor.Direction == 1))
                                { // no access in the correct direction.
                                    backward2IslandId = NoAccess;
                                    this.SetIslandId(edge2BackwardId, NoAccess);
                                }
                            }
                            if (backward2IslandId != NoAccess)
                            { // just update with whatever comes from backward1.
                                if (backward2IslandId == NoAccess &&
                                    backward2IslandId == NotSet)
                                { // start a new island here, the incoming edge has not access.
                                    backward2IslandId = this.NewIslandId();
                                }
                                else if (backward2IslandId == NotSet)
                                { // take island id from backward1.
                                    backward2IslandId = backward2IslandId;
                                }
                                else
                                { // connect island1 -> island2.
                                    backward2IslandId = this.ConnectIslands(backward2IslandId, backward2IslandId);
                                }
                                this.SetIslandId(edge2BackwardId, backward2IslandId);
                            }
                        }
                    }
                }
            }
        }
    }
}