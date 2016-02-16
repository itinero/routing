// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Network;
using Itinero.Profiles;
using System;
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
        public delegate Result<Route> BuildRoute(RouterDb routerDb, Profile vehicleProfile, Func<ushort, Factor> getFactor,
            RouterPoint source, RouterPoint target, List<uint> path);
        
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
                        Attributes = new AttributeCollection(attributes)
                    });
                }
            }
        }
    }
}