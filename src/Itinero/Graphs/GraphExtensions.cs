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

using Itinero.Algorithms;
using Itinero.Algorithms.Weights;
using Itinero.Data.Edges;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs
{
    /// <summary>
    /// Contains extension methods for the graph.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
        /// <returns></returns>
        public static uint GetOther(this Edge edge, uint vertex)
        {
            if (edge.From == vertex)
            {
                return edge.To;
            }
            else if (edge.To == vertex)
            {
                return edge.From;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }

        /// <summary>
        /// Finds the best edge between the two given vertices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <param name="weightHandler"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        public static long FindBestEdge<T>(this Graph.EdgeEnumerator edgeEnumerator, WeightHandler<T> weightHandler, uint vertex1, uint vertex2, out T bestWeight)
            where T : struct
        {
            edgeEnumerator.MoveTo(vertex1);
            bestWeight = weightHandler.Infinite;
            long bestEdge = Constants.NO_EDGE;
            Factor factor;
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.To == vertex2)
                {
                    float distance;
                    ushort edgeProfile;
                    EdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out distance, out edgeProfile);
                    var weight = weightHandler.Calculate(edgeProfile, distance, out factor);

                    if (factor.Value > 0 && (factor.Direction == 0 ||
                        ((factor.Direction == 1) && !edgeEnumerator.DataInverted) ||
                        ((factor.Direction == 2) && edgeEnumerator.DataInverted)))
                    { // it's ok; the edge can be traversed by the given vehicle.
                        if (weightHandler.IsSmallerThan(weight, bestWeight))
                        {
                            bestWeight = weight;
                            bestEdge = edgeEnumerator.IdDirected();
                        }
                    }
                }
            }
            if (bestEdge == Constants.NO_EDGE)
            {
                edgeEnumerator.Reset();
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.To == vertex2)
                    {
                        float distance;
                        ushort edgeProfile;
                        EdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out distance, out edgeProfile);
                        var weight = weightHandler.Calculate(edgeProfile, distance, out factor);

                        //if (factor.Value > 0 && (factor.Direction == 0 ||
                        //    ((factor.Direction == 1) && !edgeEnumerator.DataInverted) ||
                        //    ((factor.Direction == 2) && edgeEnumerator.DataInverted)))
                        //{ // it's ok; the edge can be traversed by the given vehicle.
                            if (weightHandler.IsSmallerThan(weight, bestWeight))
                            {
                                bestWeight = weight;
                                bestEdge = edgeEnumerator.IdDirected();
                            }
                        //}
                    }
                }
            }
            return bestEdge;
        }

        /// <summary>
        /// Gets the given edge.
        /// </summary>
        public static Edge GetEdge(this Graph graph, long directedEdgeId)
        {
            if (directedEdgeId == 0) { throw new ArgumentOutOfRangeException("directedEdgeId"); }

            uint edgeId;
            if (directedEdgeId > 0)
            {
                edgeId = (uint)directedEdgeId - 1;
            }
            else
            {
                edgeId = (uint)((-directedEdgeId) - 1);
            }
            return graph.GetEdge(edgeId);
        }
    }
}