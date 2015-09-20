// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using OsmSharp.Collections.Coordinates.Collections;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Network
{
    /// <summary>
    /// Contains extension methods for the routing network.
    /// </summary>
    public static class RoutingNetworkExtensions
    {
        /// <summary>
        /// Gets the first point on the given edge starting a the given vertex.
        /// </summary>
        /// <returns></returns>
        public static ICoordinate GetFirstPoint(this RoutingNetwork graph, RoutingEdge edge, uint vertex)
        {
            var points = new List<ICoordinate>();
            if (edge.From == vertex)
            { // start at from.
                if (edge.Shape == null)
                {
                    return graph.GetVertex(edge.To);
                }
                var shape = edge.Shape;
                shape.MoveNext();
                return shape.Current;
            }
            else if (edge.To == vertex)
            { // start at to.
                if (edge.Shape == null)
                {
                    return graph.GetVertex(edge.From);
                }
                var shape = edge.Shape.Reverse();
                shape.MoveNext();
                return shape.Current;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }

        /// <summary>
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
        /// <returns></returns>
        public static uint GetOther(this RoutingEdge edge, uint vertex)
        {
            if(edge.From == vertex)
            {
                return edge.To;
            }
            else if(edge.To == vertex)
            {
                return edge.From;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }
    }
}
