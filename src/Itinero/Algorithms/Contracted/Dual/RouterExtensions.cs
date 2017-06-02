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
using Itinero.Data.Contracted;
using Itinero.Profiles;
using System;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// A collection of extension methods 
    /// </summary>
    internal static class RouterExtensions
    {
        /// <summary>
        /// Calculates a many-to-many weight matrix using a dual edge-based graph.
        /// </summary>
        internal static Result<T[][]> CalculateManyToMany<T>(this ContractedDb contractedDb, RouterDb routerDb, Profile profile, WeightHandler<T> weightHandler, 
            RouterPoint[] sources, RouterPoint[] targets, T max) where T : struct
        {
            if (!(contractedDb.HasNodeBasedGraph &&
                  contractedDb.NodeBasedIsEdgedBased))
            {
                throw new ArgumentOutOfRangeException("No dual edge-based graph was found!");
            }
            
            var dykstraSources = new DykstraSource<T>[sources.Length];
            for (var i = 0; i < sources.Length; i++)
            {
                dykstraSources[i] = sources[i].ToDualDykstraSource(routerDb, weightHandler, true);
            }

            var dykstraTargets = new DykstraSource<T>[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                dykstraTargets[i] = targets[i].ToDualDykstraSource(routerDb, weightHandler, false);
            }
            
            // calculate weights.
            var algorithm = new VertexToVertexWeightAlgorithm<T>(contractedDb.NodeBasedGraph, weightHandler, dykstraSources, dykstraTargets, weightHandler.Infinite);
            algorithm.Run();

            // subtract the weight of the first edge from each weight.
            var edgeEnumerator = routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            var weights = algorithm.Weights;
            for (var s = 0; s < dykstraSources.Length; s++)
            {
                var id = new DirectedEdgeId()
                {
                    Raw = dykstraSources[s].Vertex1
                };
                edgeEnumerator.MoveToEdge(id.EdgeId);
                var weight = weightHandler.GetEdgeWeight(edgeEnumerator);
                for (var t = 0; t < dykstraTargets.Length; t++)
                {
                    if (weightHandler.IsSmallerThan(weights[s][t], weightHandler.Infinite))
                    {
                        weights[s][t] = weightHandler.Subtract(weights[s][t], weight.Weight);
                    }
                }
            }

            // extract the best weight for each edge pair.

            return new Result<T[][]>(algorithm.Weights);
        }
    }
}