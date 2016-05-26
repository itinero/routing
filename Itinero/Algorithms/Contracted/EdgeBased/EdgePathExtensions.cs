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

using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Extension method for the edge path.
    /// </summary>
    public static class EdgePathExtensions
    {
        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath Expand(this EdgePath edgePath, DirectedDynamicGraph graph)
        {
            return edgePath.Expand(graph.GetEdgeEnumerator());
        }

        /// <summary>
        /// Expands all edges in the given edge path.
        /// </summary>
        public static EdgePath Expand(this EdgePath edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            if (edgePath.From == null)
            {
                return edgePath;
            }

            // expand everything before.
            edgePath = new EdgePath(edgePath.Vertex, edgePath.Weight, edgePath.Edge, edgePath.From.Expand(enumerator));

            // expand list.
            return edgePath.ExpandLast(enumerator);
        }

        /// <summary>
        /// Expands the last edge in the given edge path.
        /// </summary>
        private static EdgePath ExpandLast(this EdgePath edgePath, DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            bool? direction;
            if (edgePath.Edge != Constants.NO_EDGE &&
                edgePath.From != null)
            {
                enumerator.MoveToEdge(edgePath.Edge);
                var contractedId = enumerator.GetContracted();
                if (contractedId.HasValue)
                { // there is a contracted vertex here!
                    // get source/target sequences.
                    var sequence1 = enumerator.GetSequence1();
                    sequence1 = sequence1.Reverse();
                    var sequence2 = enumerator.GetSequence2();

                    // move to the first edge (contracted -> from vertex) and keep details.
                    enumerator.MoveToEdge(contractedId.Value, edgePath.From.Vertex, sequence1);
                    float weight1;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0, out weight1, out direction);
                    var edge1 = enumerator.IdDirected();

                    // move to the second edge (contracted -> to vertex) and keep details.
                    enumerator.MoveToEdge(contractedId.Value, edgePath.Vertex, sequence2);
                    float weight2;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0, out weight2, out direction);
                    var edge2 = enumerator.IdDirected();

                    if (edgePath.Edge > 0)
                    {
                        var contractedPath = new EdgePath(contractedId.Value, edgePath.From.Weight + weight1, -edge1, edgePath.From);
                        contractedPath = contractedPath.ExpandLast(enumerator);
                        return (new EdgePath(edgePath.Vertex, edgePath.Weight, edge2, contractedPath)).ExpandLast(enumerator);
                    }
                    else
                    {
                        var contractedPath = new EdgePath(contractedId.Value, edgePath.From.Weight + weight1, edge1, edgePath.From);
                        contractedPath = contractedPath.ExpandLast(enumerator);
                        return (new EdgePath(edgePath.Vertex, edgePath.Weight, -edge2, contractedPath)).ExpandLast(enumerator);
                    }
                }
            }
            return edgePath;
        }
    }
}
