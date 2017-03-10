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

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Data.Network;
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.Algorithms.Routes
{
    /// <summary>
    /// Contains extension methods related to route builders.
    /// </summary>
    public static class RouteBuilderExtensions
    {
        /// <summary>
        /// Delegate to create a resolver.
        /// </summary>
        public delegate Result<Route> BuildRoute<T>(RouterDb routerDb, Profile vehicleProfile, RouterPoint source, RouterPoint target, EdgePath<T> path);
        
        /// <summary>
        /// Adds branches for the given vertex.
        /// </summary>
        public static void AddBranches(this List<Route.Branch> branches, RouterDb routerDb, int shape, uint vertex, uint previousEdge, uint nextVertex)
        {
            var edges = routerDb.Network.GetEdgeEnumerator(vertex);
            while (edges.MoveNext())
            {
                if (edges.Id != previousEdge &&
                    edges.To != nextVertex)
                {
                    var edge = edges.Current;
                    var direction = !edges.DataInverted;
                    var attributes = routerDb.GetProfileAndMeta(edge.Data.Profile, edge.Data.MetaId);

                    var point = routerDb.Network.GetFirstPoint(edge, edges.From);
                    branches.Add(new Route.Branch()
                    {
                        Shape = shape,
                        Coordinate = new Coordinate()
                        {
                            Latitude = point.Latitude,
                            Longitude = point.Longitude
                        },
                        Attributes = new AttributeCollection(attributes),
                        AttributesDirection = direction
                    });
                }
            }
        }
    }
}