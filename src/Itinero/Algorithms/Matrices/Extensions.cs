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

using Itinero.Algorithms.Search;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Geometric;

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// Contains extension methods related to the weight matrix algorithms.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the index in the weight matrix, given the orginal location index.
        /// </summary>
        public static int WeightIndex<T>(this IWeightMatrixAlgorithm<T> algorithm, int locationIdx)
        {
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.CorrectedIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Gets the orginal location index for the given corrected routerpoint index.
        /// </summary>
        public static int OriginalLocationIndex<T>(this IWeightMatrixAlgorithm<T> algorithm, int correctedIdx)
        {
            var resolvedIndex = algorithm.OriginalIndexOf(correctedIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.MassResolver.LocationIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Gets the index in the weight matrix, given the orginal location index.
        /// </summary>
        public static int WeightIndex<T>(this IDirectedWeightMatrixAlgorithm<T> algorithm, int locationIdx)
        {
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.CorrectedIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Gets the orginal location index for the given corrected routerpoint index.
        /// </summary>
        public static int OriginalLocationIndex<T>(this IDirectedWeightMatrixAlgorithm<T> algorithm, int correctedIdx)
        {
            var resolvedIndex = algorithm.OriginalIndexOf(correctedIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.MassResolver.LocationIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Returns true if the point at the given original location index is in error.
        /// </summary>
        public static bool IsInError<T>(this IWeightMatrixAlgorithm<T> algorithm, int locationIdx)
        {
            LocationError le;
            RouterPointError rpe;
            return algorithm.TryGetError(locationIdx, out le, out rpe);
        }

        /// <summary>
        /// Tries to get an error for the given original location index.
        /// </summary>
        public static bool TryGetError<T>(this IWeightMatrixAlgorithm<T> algorithm, int locationIdx, out LocationError locationError,
            out RouterPointError routerPointError)
        {
            locationError = null;
            routerPointError = null;
            if (algorithm.MassResolver.Errors.TryGetValue(locationIdx, out locationError))
            {
                return true;
            }
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (algorithm.Errors.TryGetValue(resolvedIndex, out routerPointError))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the point at the given original location index is in error.
        /// </summary>
        public static bool IsInError<T>(this IDirectedWeightMatrixAlgorithm<T> algorithm, int locationIdx)
        {
            LocationError le;
            RouterPointError rpe;
            return algorithm.TryGetError(locationIdx, out le, out rpe);
        }

        /// <summary>
        /// Tries to get an error for the given original location index.
        /// </summary>
        public static bool TryGetError<T>(this IDirectedWeightMatrixAlgorithm<T> algorithm, int locationIdx, out LocationError locationError,
            out RouterPointError routerPointError)
        {
            locationError = null;
            routerPointError = null;
            if (algorithm.MassResolver.Errors.TryGetValue(locationIdx, out locationError))
            {
                return true;
            }
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (algorithm.Errors.TryGetValue(resolvedIndex, out routerPointError))
            {
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Converts the router point to paths leading to the closest 2 vertices always returning the backward path first, forward path second.
        /// </summary>
        public static EdgePath<T>[] ToEdgePathsDirectedFixed<T>(this RouterPoint point, RouterDb routerDb, Weights.WeightHandler<T> weightHandler, bool asSource)
            where T : struct
        {
            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);

            float distance;
            ushort profileId;
            Data.Edges.EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            Profiles.Factor factor;
            var edgeWeight = weightHandler.Calculate(profileId, distance, out factor);

            var offset = point.Offset / (float)ushort.MaxValue;
            if (factor.Direction == 0)
            { // bidirectional.
                if (offset == 0)
                { // the first part is just the first vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Zero, -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                else if (offset == 1)
                { // the second path it just the second vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        new EdgePath<T>(edge.To, weightHandler.Zero, edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                return new EdgePath<T>[] {
                    new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                    new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                };
            }
            else if (factor.Direction == 1)
            { // edge is forward oneway.
                if (asSource)
                {
                    if (offset == 1)
                    { // just return the to-vertex.
                        return new EdgePath<T>[] {
                            null,
                            new EdgePath<T>(edge.To, weightHandler.Zero, edge.IdDirected(), new EdgePath<T>(edge.From))
                        };
                    }
                    return new EdgePath<T>[] {
                        null,
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                if (offset == 0)
                { // just return the from vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Zero, -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        null
                    };
                }
                return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        null
                    };
            }
            else
            { // edge is backward oneway.
                if (!asSource)
                {
                    if (offset == 1)
                    { // just return the to-vertex.
                        return new EdgePath<T>[] {
                            null,
                            new EdgePath<T>(edge.To, weightHandler.Zero, edge.IdDirected(), new EdgePath<T>(edge.From))
                        };
                    }
                    return new EdgePath<T>[] {
                        null,
                        new EdgePath<T>(edge.To, weightHandler.Calculate(profileId, distance * (1 - offset)), edge.IdDirected(), new EdgePath<T>(edge.From))
                    };
                }
                if (offset == 0)
                { // just return the from-vertex.
                    return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Zero, new EdgePath<T>(edge.To)),
                        null
                    };
                }
                return new EdgePath<T>[] {
                        new EdgePath<T>(edge.From, weightHandler.Calculate(profileId, distance * offset), -edge.IdDirected(), new EdgePath<T>(edge.To)),
                        null
                    };
            }
        }

    }
}