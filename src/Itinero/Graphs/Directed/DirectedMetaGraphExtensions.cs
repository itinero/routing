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

using System;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// Contains extension methods for the directed graph.
    /// </summary>
    public static class DirectedMetaGraphExtensions
    {
        /// <summary>
        /// Extracts a part of the graph transforming the vertices along the way.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="transform">The transform function, returns Constant.NO_VERTEX when a vertex needs to be skipped.</param>
        /// <returns></returns>
        public static DirectedMetaGraph Extract(this DirectedMetaGraph graph, Func<uint, uint> transform)
        {
            var newGraph = new DirectedMetaGraph(graph.Graph.EdgeDataSize, graph.EdgeDataSize);

            var enumerator = graph.GetEdgeEnumerator();
            for (uint v = 0; v < graph.VertexCount; v++)
            {
                var newV = transform(v);
                if (newV == Constants.NO_VERTEX)
                {
                    continue;
                }

                enumerator.MoveTo(v);
                while (enumerator.MoveNext())
                {
                    var newNeighbour = transform(enumerator.Neighbour);
                    if (newNeighbour == Constants.NO_VERTEX)
                    {
                        continue;
                    }

                    var data = enumerator.Data;
                    var metaData = enumerator.MetaData;

                    // transform contracted vertex.
                    if (metaData[0] != Constants.NO_VERTEX)
                    {
                        metaData[0] = transform(metaData[0]);
                        if (metaData[0] == Constants.NO_VERTEX)
                        {
                            continue;
                        }
                    }

                    newGraph.AddEdge(newV, newNeighbour, data, metaData);
                }
            }

            return newGraph;
        }
    }
}