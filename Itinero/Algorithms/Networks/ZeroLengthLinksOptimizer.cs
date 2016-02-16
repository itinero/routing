// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using Itinero.Network;

namespace Itinero.Algorithms.Networks
{
    /// <summary>
    /// An algorithm to remove links of length zero.
    /// </summary>
    public class ZeroLengthLinksOptimizer : AlgorithmBase
    {
        private readonly Network.RoutingNetwork _network;
        private readonly CanRemoveDelegate _canRemove;

        /// <summary>
        /// A delegate to control the removal of an edge.
        /// </summary>
        public delegate bool CanRemoveDelegate(Network.Data.EdgeData edge);

        /// <summary>
        /// Creates a new network optimizer algorithm.
        /// </summary>
        public ZeroLengthLinksOptimizer(Network.RoutingNetwork network, CanRemoveDelegate canRemove)
        {
            _network = network;
            _canRemove = canRemove;
        }

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            var edgeEnumerator = _network.GetEdgeEnumerator();
            for(uint vertex = 0;  vertex < _network.VertexCount; vertex++)
            {
                edgeEnumerator.MoveTo(vertex);
                while (edgeEnumerator.MoveNext())
                {
                    var edgeData = edgeEnumerator.Data;
                    if (edgeData.Distance == 0 &&
                        _canRemove(edgeData))
                    { // edge can be removed.
                        var vertex1 = edgeEnumerator.From;
                        var vertex2 = edgeEnumerator.To;

                        // remove the actual edge.
                        _network.RemoveEdge(edgeEnumerator.Id);

                        // merge vertex2 into vertex1.
                        _network.MergeVertices(vertex1, vertex2); // this leaves vertex2 disconnected.

                        vertex--; // try again for this vertex.
                        break;
                    }
                }
            }
        }
    }
}