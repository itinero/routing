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

using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;
using Itinero.Algorithms.Networks.Preprocessing;

namespace Itinero.Algorithms.Networks
{
    /// <summary>
    /// Contains extension methods for the routerdb.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Optimizes the network by removing irrelevant vertex.
        /// </summary>
        public static void OptimizeNetwork(this RouterDb routerDb, float simplifyEpsilonInMeter = Constants.DEFAULT_SIMPL_E)
        {
            routerDb.OptimizeNetwork(simplifyEpsilonInMeter, CancellationToken.None);
        }

        /// <summary>
        /// Optimizes the network by removing irrelevant vertex.
        /// </summary>
        public static void OptimizeNetwork(this RouterDb routerDb, float simplifyEpsilonInMeter, CancellationToken cancellationToken)
        {
            var router = new Router(routerDb);

            var factorFunctions = new List<Func<ushort, Factor>>();
            foreach(var vehicle in routerDb.GetSupportedVehicles())
            {
                foreach(var profile in vehicle.GetProfiles())
                {
                    factorFunctions.Add(router.GetDefaultGetFactor(profile));
                }
            }

            var algorithm = new Itinero.Algorithms.Networks.NetworkOptimizer(
                routerDb.Network, routerDb.GetHasAnyRestriction(), (Itinero.Data.Network.Edges.EdgeData edgeData1, bool inverted1,
                 Itinero.Data.Network.Edges.EdgeData edgeData2, bool inverted2, out Itinero.Data.Network.Edges.EdgeData mergedEdgeData, out bool mergedInverted) =>
                {
                    mergedEdgeData = new Itinero.Data.Network.Edges.EdgeData()
                    {
                        Distance = edgeData1.Distance + edgeData2.Distance,
                        MetaId = edgeData2.MetaId,
                        Profile = edgeData2.Profile
                    };
                    mergedInverted = inverted2;
                    if (edgeData1.MetaId != edgeData2.MetaId)
                    { // different meta data, do not merge.
                        return false;
                    }

                    if (inverted1 != inverted2)
                    { // directions are the different.
                        foreach (var factorFunction in factorFunctions)
                        {
                            var edge1Factor = factorFunction(edgeData1.Profile);
                            var edge2Factor = factorFunction(edgeData2.Profile);

                            if (edge1Factor.Value != edge2Factor.Value)
                            {
                                return false;
                            }
                            if (edge1Factor.Direction == 0)
                            {
                                if (edge2Factor.Direction == 1)
                                {
                                    return false;
                                }
                                else if (edge2Factor.Direction == 2)
                                {
                                    return false;
                                }
                            }
                            else if (edge1Factor.Direction == 1)
                            {
                                if (edge2Factor.Direction != 2)
                                {
                                    return false;
                                }
                            }
                            else if (edge1Factor.Direction == 2)
                            {
                                if (edge2Factor.Direction != 1)
                                {
                                    return false;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var factorFunction in factorFunctions)
                        {
                            var edge1Factor = factorFunction(edgeData1.Profile);
                            var edge2Factor = factorFunction(edgeData2.Profile);

                            if (edge1Factor.Value != edge2Factor.Value)
                            {
                                return false;
                            }
                            if (edge1Factor.Direction != edge2Factor.Direction)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }, simplifyEpsilonInMeter);
            var edgeMetaDb = routerDb.EdgeData;
            var edgeMetaKeys = edgeMetaDb.Names;
            algorithm.CanMerge = (edgeId1, edgeId2) =>
            {
                foreach (var key in edgeMetaKeys)
                {
                    var edgeMeta = edgeMetaDb.Get(key);
                    return edgeMeta.Equal(edgeId1, edgeId2);
                }
                return true;
            };
            algorithm.NewEdge = (oldEdgeId, newEdgeId) =>
            {
                foreach (var key in edgeMetaKeys)
                {
                    var edgeMeta = edgeMetaDb.Get(key);
                    if (oldEdgeId >= edgeMeta.Count)
                    {
                        continue;
                    }

                    edgeMeta.Copy(newEdgeId, oldEdgeId);
                }
            };
            
            algorithm.Run(cancellationToken);
            algorithm.CheckHasRunAndHasSucceeded();
        }

        /// <summary>
        /// Runs the max distance splitter algorithm to make edge comply with the max distance setting in the routerdb.
        /// </summary>
        public static void SplitLongEdges(this RouterDb db, Action<uint> newVertex = null)
        {
            db.SplitLongEdges(newVertex, CancellationToken.None);
        }

        /// <summary>
        /// Runs the max distance splitter algorithm to make edge comply with the max distance setting in the routerdb.
        /// </summary>
        public static void SplitLongEdges(this RouterDb db, Action<uint> newVertex, CancellationToken cancellationToken)
        {
            var splitter = new Preprocessing.MaxDistanceSplitter(db.Network, (originalEdgeId, newEdgeId) => 
            {
                if (newEdgeId == Constants.NO_EDGE)
                { // original edge was removed.
                    db.EdgeData.SetEmpty(originalEdgeId);
                    return;
                }

                // the edge was split so copy meta-data.
                db.EdgeData.Copy(newEdgeId, originalEdgeId);
            },db.Network.MaxEdgeDistance, (v) => {
                db.VertexData.SetEmpty(v);
                if (newVertex != null)
                {
                    newVertex(v);
                }
            });
            splitter.Run(cancellationToken);

            splitter.CheckHasRunAndHasSucceeded();
        }

        /// <summary>
        /// Runs the algorithm to make sure the loaded graph is a 'simple' graph.
        /// </summary>
        public static void ConvertToSimple(this RouterDb db, Action<uint> newVertex = null)
        {
            db.ConvertToSimple(newVertex, CancellationToken.None);
        }

        /// <summary>
        /// Runs the algorithm to make sure the loaded graph is a 'simple' graph.
        /// </summary>
        public static void ConvertToSimple(this RouterDb db, Action<uint> newVertex, CancellationToken cancellationToken)
        {
            var converter = new Preprocessing.SimpleGraphConverter(db.Network, (originalEdgeId, newEdgeId) => 
            {
                if (newEdgeId == Constants.NO_EDGE)
                { // original edge was removed.
                    db.EdgeData.SetEmpty(originalEdgeId);
                    return;
                }

                // the edge was split so copy meta-data.
                db.EdgeData.Copy(newEdgeId, originalEdgeId);
            }, (v) => {
                db.VertexData.SetEmpty(v);
                if (newVertex != null)
                {
                    newVertex(v);
                }
            });
            converter.Run(cancellationToken);

            converter.CheckHasRunAndHasSucceeded();
        }

        /// <summary>
        /// Removes duplicate edges.
        /// </summary>
        /// <param name="db">The routerdb.</param>
        public static void RemoveDuplicateEdges(this RouterDb db, CancellationToken cancellationToken = default(CancellationToken))
        {
            var duplicateEdgeRemover = new DuplicateEdgeRemover(db);
            duplicateEdgeRemover.Run(cancellationToken);

            duplicateEdgeRemover.CheckHasRunAndHasSucceeded();
        }
    }
}
