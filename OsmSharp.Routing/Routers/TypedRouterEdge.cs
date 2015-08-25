// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Routers
{
    /// <summary>
    /// A version of the typedrouter using edges of type Edge.
    /// </summary>
    internal class TypedRouterEdge : TypedRouter<Edge>
    {
        /// <summary>
        /// Creates a new type router using edges of type Edge.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="router"></param>
        public TypedRouterEdge(IRoutingAlgorithmData<Edge> graph, IRoutingInterpreter interpreter,
                           IRoutingAlgorithm<Edge> router)
            :base(graph, interpreter, router)
        {
            
        }

        /// <summary>
        /// Returns true if the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public override bool SupportsVehicle(Vehicle vehicle)
        {
            // TODO: ask interpreter.
            return true; 
        }
    }
}