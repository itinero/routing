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

using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Contracted
{
    /// <summary>
    /// Directed graph extensions assuming it contains contracted data.
    /// </summary>
    public static class DirectedGraphExtensions
    {
        /// <summary>
        /// Expands a the shortest edge between the two given vertices.
        /// </summary>
        public static void ExpandEdge(this DirectedGraph graph, uint vertex1, uint vertex2, List<uint> vertices, bool forward)
        {
            // check if expansion is needed.
            var edge = graph.GetEdge(vertex1, vertex2,
                ContractedEdgeDataSerializer.DeserializeWeightFunc);
            if(edge == null)
            { // no edge found!
                throw new Exception(string.Format("No edge found from {0} to {1}.", vertex1, vertex2));
            }
            var edgeContractedId = edge.GetContractedId();
            if (edgeContractedId != Constants.NO_VERTEX)
            { // further expansion needed.
                if (forward)
                {
                    graph.ExpandEdge(edgeContractedId, vertex1, vertices, false);
                    vertices.Add(edgeContractedId);
                    graph.ExpandEdge(edgeContractedId, vertex2, vertices, true);
                }
                else
                {
                    graph.ExpandEdge(edgeContractedId, vertex2, vertices, false);
                    vertices.Add(edgeContractedId);
                    graph.ExpandEdge(edgeContractedId, vertex1, vertices, true);
                }
            }
        }
    }
}