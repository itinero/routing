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
        public static void OptimizeNetwork(this RouterDb routerDb)
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
                    { // directions are the same.
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
                                else if (edgeData2.Distance == 2)
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
                });
            algorithm.Run();
        }
    }
}
