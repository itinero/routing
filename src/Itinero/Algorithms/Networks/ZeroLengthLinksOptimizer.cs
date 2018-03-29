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

using Itinero.Data.Network;
using System.Threading;

namespace Itinero.Algorithms.Networks
{
    /// <summary>
    /// An algorithm to remove links of length zero.
    /// </summary>
    public class ZeroLengthLinksOptimizer : AlgorithmBase
    {
        private readonly RoutingNetwork _network;
        private readonly CanRemoveDelegate _canRemove;

        /// <summary>
        /// A delegate to control the removal of an edge.
        /// </summary>
        public delegate bool CanRemoveDelegate(Data.Network.Edges.EdgeData edge);

        /// <summary>
        /// Creates a new network optimizer algorithm.
        /// </summary>
        public ZeroLengthLinksOptimizer(RoutingNetwork network, CanRemoveDelegate canRemove)
        {
            _network = network;
            _canRemove = canRemove;
        }

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
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