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
using System.Threading;

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
            RouterPoint[] sources, RouterPoint[] targets, T max, CancellationToken cancellationToken) where T : struct
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
                dykstraTargets[i] = targets[i].ToDualDykstraSource(routerDb, weightHandler, true);
            }
            
            // calculate weights.
            var algorithm = new ManyToMany.VertexToVertexWeightAlgorithm<T>(contractedDb.NodeBasedGraph, weightHandler, dykstraSources, dykstraTargets, weightHandler.Infinite);
            algorithm.Run(cancellationToken);

            // extract the best weight for each edge pair.
            return new Result<T[][]>(algorithm.Weights);
        }
    }
}