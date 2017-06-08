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

namespace Itinero.Algorithms.Default.EdgeBased
{

    /// <summary>
    /// Contains extension methods related to the routerpoints.
    /// </summary>
    public static class RouterPointExtensions
    {
        /// <summary>
        /// Converts the router point to a directed dykstra source.
        /// </summary>
        public static DirectedDykstraSource<T> ToDirectedDykstraSource<T>(this RouterPoint point, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource)
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
                return new DirectedDykstraSource<T>
                {
                    Edge1 = (new DirectedEdgeId(point.EdgeId, true)),
                    Weight1 = weightHandler.Calculate(profileId, distance * offset),
                    Edge2 = (new DirectedEdgeId(point.EdgeId, false)),
                    Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                };
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway
                if (asSource)
                {
                    return new DirectedDykstraSource<T>
                    {
                        Edge1 = (new DirectedEdgeId(point.EdgeId, true)),
                        Weight1 = weightHandler.Calculate(profileId, distance * offset),
                        Edge2 = DirectedEdgeId.NO_EDGE,
                        Weight2 = weightHandler.Infinite
                    };
                }
                return new DirectedDykstraSource<T>
                {
                    Edge1 = DirectedEdgeId.NO_EDGE,
                    Weight1 = weightHandler.Infinite,
                    Edge2 = (new DirectedEdgeId(point.EdgeId, false)),
                    Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                };
            }
            else
            { // edge is backward oneway.
                if (asSource)
                {
                    return new DirectedDykstraSource<T>
                    {
                        Edge1 = DirectedEdgeId.NO_EDGE,
                        Weight1 = weightHandler.Infinite,
                        Edge2 = (new DirectedEdgeId(point.EdgeId, false)),
                        Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                    };
                }
                return new DirectedDykstraSource<T>
                {
                    Edge1 = (new DirectedEdgeId(point.EdgeId, true)),
                    Weight1 = weightHandler.Calculate(profileId, distance * offset),
                    Edge2 = DirectedEdgeId.NO_EDGE,
                    Weight2 = weightHandler.Infinite
                };
            }
        }

        /// <summary>
        /// Converts the router point to a directed dykstra source.
        /// </summary>
        /// <param name="forward">Only move in the forward direction of the edge associated with the routerpoint.</param>
        /// <param name="routerDb">The routerDb</param>
        /// <param name="weightHandler">The weight handler.</param>
        public static DirectedDykstraSource<T> ToDirectedDykstraSource<T>(this RouterPoint point, bool forward, RouterDb routerDb, WeightHandler<T> weightHandler)
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
                if (forward)
                {
                    return new DirectedDykstraSource<T>
                    {
                        Edge1 = (new DirectedEdgeId(point.EdgeId, true)),
                        Weight1 = weightHandler.Calculate(profileId, distance * offset),
                        Edge2 = DirectedEdgeId.NO_EDGE,
                        Weight2 = weightHandler.Infinite
                    };
                }
                else
                {
                    return new DirectedDykstraSource<T>
                    {
                        Edge1 = DirectedEdgeId.NO_EDGE,
                        Weight1 = weightHandler.Infinite,
                        Edge2 = (new DirectedEdgeId(point.EdgeId, false)),
                        Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                    };
                }
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway
                if (forward)
                {
                    return new DirectedDykstraSource<T>
                    {
                        Edge1 = (new DirectedEdgeId(point.EdgeId, true)),
                        Weight1 = weightHandler.Calculate(profileId, distance * offset),
                        Edge2 = DirectedEdgeId.NO_EDGE,
                        Weight2 = weightHandler.Infinite
                    };
                }
                return new DirectedDykstraSource<T>
                {
                    Edge1 = DirectedEdgeId.NO_EDGE,
                    Weight1 = weightHandler.Infinite,
                    Edge2 = DirectedEdgeId.NO_EDGE,
                    Weight2 = weightHandler.Infinite
                };
            }
            else
            { // edge is backward oneway.
                if (forward)
                {
                    return new DirectedDykstraSource<T>
                    {
                        Edge1 = DirectedEdgeId.NO_EDGE,
                        Weight1 = weightHandler.Infinite,
                        Edge2 = DirectedEdgeId.NO_EDGE,
                        Weight2 = weightHandler.Infinite,
                    };
                }
                return new DirectedDykstraSource<T>
                {
                    Edge1 = DirectedEdgeId.NO_EDGE,
                    Weight1 = weightHandler.Infinite,
                    Edge2 = (new DirectedEdgeId(point.EdgeId, false)),
                    Weight2 = weightHandler.Calculate(profileId, distance * (1 - offset))
                };
            }
        }
        
        /// <summary>
        /// Converts the router points to a directed dykstra source.
        /// </summary>
        public static DirectedDykstraSource<T>[] ToDirectedDykstraSources<T>(this RouterPoint[] points, RouterDb routerDb, WeightHandler<T> weightHandler, bool asSource)
            where T : struct
        {
            var results = new DirectedDykstraSource<T>[points.Length];
            for (var i = 0; i < points.Length; i++) // TODO: this only reads stuff, perfect to parallelise
            {
                results[i] = points[i].ToDirectedDykstraSource(routerDb, weightHandler, asSource);
            }
            return results;
        }

        /// <summary>
        /// Converts the router points to a directed dykstra source.
        /// </summary>
        /// <param name="routerDb">The routerDb.</param>
        /// <param name="forwards">The forward flags.</param>
        /// <param name="weightHandler">The weight handler.</param>
        public static DirectedDykstraSource<T>[] ToDirectedDykstraSources<T>(this RouterPoint[] points, bool[] forwards, RouterDb routerDb, WeightHandler<T> weightHandler)
            where T : struct
        {
            var results = new DirectedDykstraSource<T>[points.Length];
            for (var i = 0; i < points.Length; i++) // TODO: this only reads stuff, perfect to parallelise
            {
                results[i] = points[i].ToDirectedDykstraSource(forwards[i], routerDb, weightHandler);
            }
            return results;
        }

        /// <summary>
        /// Converts the router points to a directed dykstra source.
        /// </summary>
        /// <param name="routerDb">The routerDb.</param>
        /// <param name="forward">The forward flag.</param>
        /// <param name="weightHandler">The weight handler.</param>
        public static DirectedDykstraSource<T>[] ToDirectedDykstraSources<T>(this RouterPoint[] points, bool forward, RouterDb routerDb, WeightHandler<T> weightHandler)
            where T : struct
        {
            var results = new DirectedDykstraSource<T>[points.Length];
            for (var i = 0; i < points.Length; i++) // TODO: this only reads stuff, perfect to parallelise
            {
                results[i] = points[i].ToDirectedDykstraSource(forward, routerDb, weightHandler);
            }
            return results;
        }
    }
}