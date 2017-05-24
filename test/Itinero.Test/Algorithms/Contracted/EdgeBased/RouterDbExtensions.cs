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
using Itinero.Graphs.Directed;
using Itinero.Profiles;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains test-only extension methods for the routerdb.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddEdgeBasedContractedForTesting(this RouterDb db, Profile profile)
        {
            db.AddEdgeBasedContractedForTesting(profile, profile.DefaultWeightHandlerCached(db));
        }

        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddEdgeBasedContractedForTesting<T>(this RouterDb db, Profile profile, WeightHandler<T> weightHandler)
            where T : struct
        {
            // create the raw directed graph.
            ContractedDb contractedDb = null;

            var contracted = new DirectedDynamicGraph(weightHandler.DynamicSize);
            var directedGraphBuilder = new Itinero.Algorithms.Contracted.EdgeBased.DirectedGraphBuilder<T>(db.Network.GeometricGraph.Graph, contracted,
                weightHandler);
            directedGraphBuilder.Run();

            // contract the graph.
            var priorityCalculator = new Itinero.Algorithms.Contracted.EdgeBased.EdgeDifferencePriorityCalculator<T>(contracted, weightHandler,
                new Itinero.Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator<T>(weightHandler, 4, 64));
            priorityCalculator.DifferenceFactor = 5;
            priorityCalculator.DepthFactor = 5;
            priorityCalculator.ContractedFactor = 8;
            var hierarchyBuilder = new Itinero.Algorithms.Contracted.EdgeBased.HierarchyBuilder<T>(contracted, priorityCalculator,
                    new Itinero.Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator<T>(weightHandler, int.MaxValue, 64), weightHandler, db.GetGetRestrictions(profile, null));
            hierarchyBuilder.Run();

            contractedDb = new ContractedDb(contracted);

            // add the graph.
            db.AddContracted(profile, contractedDb);
        }
    }
}