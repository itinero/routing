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

using System;

namespace OsmSharp.Routing.Graphs.Directed
{
    /// <summary>
    /// Contains extension methods for the directed graph.
    /// </summary>
    public static class DirectedGraphException
    {
        /// <summary>
        /// Gets the shortest edge between two vertices.
        /// </summary>
        /// <returns></returns>
        public static Edge GetShortestEdge(this DirectedGraph graph, uint vertex1, uint vertex2, Func<uint[], float?> getWeight)
        {
            var minWeight = float.MaxValue;
            var edges = graph.GetEdgeEnumerator(vertex1);
            Edge edge = null;
            while(edges.MoveNext())
            {
                if(edges.Neighbour == vertex2)
                { // the correct neighbour, get the weight.
                    var weight = getWeight(edges.Data);
                    if(weight.HasValue && 
                        weight.Value < minWeight)
                    { // weight is better.
                        edge = edges.Current;
                    }
                }
            }
            return edge;
        }
    }
}