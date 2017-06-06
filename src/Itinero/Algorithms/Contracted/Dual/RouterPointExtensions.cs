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

using Itinero.Algorithms.Weights;
using Itinero.Data.Edges;
using Itinero.Profiles;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// Contains extension methods related to the routerpoints.
    /// </summary>
    public static class RouterPointExtensions
    {
        /// <summary>
        /// Converts the router point to vertex and weights with vertex id being the directed edge id. This results in one dykstra source of this routerpoint.
        /// </summary>
        public static DykstraSource<T> ToDualDykstraSource<T>(this RouterPoint point, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource)
            where T : struct
        {
            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);

            float distance;
            ushort profileId;
            EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            Factor factor;
            var edgeWeight = weightHandler.Calculate(profileId, distance, out factor);

            var offset = point.Offset / (float)ushort.MaxValue;
            if (factor.Direction == 0)
            { // bidirectional.
                return new DykstraSource<T>
                {
                    Vertex1 = (new DirectedEdgeId(point.EdgeId, true)).Raw,
                    Weight1 = weightHandler.Calculate(profileId, distance * offset),
                    Vertex2 = (new DirectedEdgeId(point.EdgeId, false)).Raw,
                    Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                };
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway
                if (asSource)
                {
                    return new DykstraSource<T>
                    {
                        Vertex1 = (new DirectedEdgeId(point.EdgeId, true)).Raw,
                        Weight1 = weightHandler.Calculate(profileId, distance * offset),
                        Vertex2 = Constants.NO_VERTEX,
                        Weight2 = weightHandler.Infinite
                    };
                }
                return new DykstraSource<T>
                {
                    Vertex1 = Constants.NO_VERTEX,
                    Weight1 = weightHandler.Infinite,
                    Vertex2 = (new DirectedEdgeId(point.EdgeId, false)).Raw,
                    Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                };
            }
            else
            { // edge is backward oneway.
                if (asSource)
                {
                    return new DykstraSource<T>
                    {
                        Vertex1 = Constants.NO_VERTEX,
                        Weight1 = weightHandler.Infinite,
                        Vertex2 = (new DirectedEdgeId(point.EdgeId, false)).Raw,
                        Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                    };
                }
                return new DykstraSource<T>
                {
                    Vertex1 = (new DirectedEdgeId(point.EdgeId, true)).Raw,
                    Weight1 = weightHandler.Calculate(profileId, distance * offset),
                    Vertex2 = Constants.NO_VERTEX,
                    Weight2 = weightHandler.Infinite
                };
            }
        }

        /// <summary>
        /// Converts all the router points to vertex and weights with vertex id being the directed edge id. This results in one dykstra source of this routerpoint.
        /// </summary>
        public static DykstraSource<T>[] ToDualDykstraSources<T>(this RouterPoint[] points, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource)
            where T : struct
        {
            var results = new DykstraSource<T>[points.Length];
            for (var i = 0; i < points.Length; i++) // TODO: this only reads stuff, perfect to parallelise
            {
                results[i] = points[i].ToDualDykstraSource(routerDb, weightHandler, asSource);
            }
            return results;
        }
    }
}