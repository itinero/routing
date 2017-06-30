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

namespace Itinero.Test
{
    /// <summary>
    /// Contains extra extension methods for the routerdb just for testing.
    /// </summary>
    public static class RouterDbTestDbExtensions
    {
        /// <summary>
        /// Adds a contracted graph in the old (broken) way, to test backwards compat. with previous itinero versions.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="profile"></param>
        public static void AddContractedOldEdgeBased(this RouterDb db, Profile profile)
        {
            var weightHandler = profile.DefaultWeightHandlerCached(db);

            var contracted = new DirectedDynamicGraph(weightHandler.DynamicSize);
            var directedGraphBuilder = new Itinero.Algorithms.Contracted.EdgeBased.DirectedGraphBuilder<float>(db.Network.GeometricGraph.Graph, contracted,
                weightHandler);
            directedGraphBuilder.Run();

            // contract the graph.
            var priorityCalculator = new Itinero.Algorithms.Contracted.EdgeBased.EdgeDifferencePriorityCalculator<float>(contracted, weightHandler,
                new Itinero.Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator<float>(weightHandler, 4, 64));
            priorityCalculator.DifferenceFactor = 5;
            priorityCalculator.DepthFactor = 5;
            priorityCalculator.ContractedFactor = 8;
            var hierarchyBuilder = new Itinero.Algorithms.Contracted.EdgeBased.HierarchyBuilder<float>(contracted, priorityCalculator,
                    new Itinero.Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator<float>(weightHandler, int.MaxValue, 64), weightHandler, db.GetGetRestrictions(profile, null));
            hierarchyBuilder.Run();

            var contractedDb = new ContractedDb(contracted);
            
            db.AddContracted(profile, contractedDb);
        }
    }
}
