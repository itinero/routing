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

using System.Collections.Generic;
using System.Threading;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using Itinero.Logging;

namespace Itinero.Algorithms.Networks.Preprocessing
{
    /// <summary>
    /// An algorithm that detects and removes duplicate edges.
    /// </summary>
    /// <remarks>
    /// This only removes edge that are:
    /// - Truly identical, meaning
    ///   - the same first last vertex
    ///   - the same profile/meta id's.
    ///   - the exact same shape.
    ///
    /// This algorithm only removes duplicate edges that have no impact on routing.
    /// </remarks>
    public class DuplicateEdgeRemover : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        
        /// <summary>
        /// Creates a new duplicate edge remover.
        /// </summary>
        /// <param name="routerDb">The routerdb.</param>
        public DuplicateEdgeRemover(RouterDb routerDb)
        {
            _routerDb = routerDb;
        }

        protected override void DoRun(CancellationToken cancellationToken)
        {
            // basic overview of algorithm:
            // priority is performance
            // 1: do a quick negative check:
            //   - go over all edges and check for duplicates.
            //   - if duplicates then really check the details.

            var removedCount = 0;
            var edgeEnumerator = _routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            var neighbours = new HashSet<uint>();
            var duplicates = new HashSet<uint>();
            var duplicateEdges = new List<RoutingEdge>();
            for (uint v = 0; v < _routerDb.Network.VertexCount; )
            {
                if (!edgeEnumerator.MoveTo(v))
                {
                    v++;
                    continue;
                }

                var removed = false;
                neighbours.Clear();
                var hasDuplicate = false;
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.From > edgeEnumerator.To) continue; // check each edge once.
                    if (neighbours.Contains(edgeEnumerator.To))
                    {
                        hasDuplicate = true;
                        break;
                    }

                    neighbours.Add(edgeEnumerator.To);
                }

                if (hasDuplicate)
                {
                    neighbours.Clear();
                    duplicates.Clear();
                    edgeEnumerator.MoveTo(v);
                    while (edgeEnumerator.MoveNext())
                    {
                        if (neighbours.Contains(edgeEnumerator.To))
                        {
                            duplicates.Add(edgeEnumerator.To);
                        }
                        neighbours.Add(edgeEnumerator.To);
                    }
                    
                    foreach (var duplicate in duplicates)
                    {
                        edgeEnumerator.MoveTo(v);
                        duplicateEdges.Clear();
                        while (edgeEnumerator.MoveNextUntil((e) => e.To == duplicate))
                        {
                            duplicateEdges.Add(_routerDb.Network.GetEdge(edgeEnumerator.Id));
                        }

                        foreach (var edge1 in duplicateEdges)
                        {
                            foreach (var edge2 in duplicateEdges)
                            {
                                if (edge1.Id == edge2.Id) continue;
                                if (!EdgesAreEqual(edge1, edge2)) continue;
                                
                                _routerDb.Network.RemoveEdge(edge2.Id);
                                removedCount++;
                                removed = true;
                                break;
                            }

                            if (removed)
                            {
                                break;
                            }
                        }

                        if (removed)
                        {
                            break;
                        }
                    }
                }

                if (!removed)
                { // do vertex again if any edge was removed.
                    v++;
                }
            }

            this.HasSucceeded = true;
            Logger.Log($"{nameof(DuplicateEdgeRemover)}", TraceEventType.Information,
                $"Removed {removedCount} duplicate edges.");
        }

        private static bool EdgesAreEqual(RoutingEdge edge1, RoutingEdge edge2)
        {
            if (edge1.From != edge2.From)
            {
                return false;
            }

            if (edge1.To != edge2.To)
            {
                return false;
            }

            if (edge1.Data.Profile != edge2.Data.Profile)
            {
                return false;
            }

            if (edge1.Data.MetaId != edge2.Data.MetaId)
            {
                return false;
            }

            var shape1 = edge1.Shape;
            var shape2 = edge2.Shape;
            if (shape1 == null || shape1.Count == 0)
            {
                if (shape2 != null && shape2.Count > 0)
                {
                    return false;
                }
            }

            if (shape2 == null || shape2.Count == 0)
            {
                if (shape1 != null && shape1.Count > 0)
                {
                    return false;
                }
            }

            if (shape1 != null && shape2 != null)
            {
                if (shape1.Count != shape2.Count)
                {
                    return false;
                }

                for (var i = 0; i < shape1.Count; i++)
                {
                    if (Coordinate.DistanceEstimateInMeter(
                            shape1[i], shape2[i]) > 0)
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}