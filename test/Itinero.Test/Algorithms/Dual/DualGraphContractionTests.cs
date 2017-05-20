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

using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.Witness;
using Itinero.Algorithms.Dual;
using Itinero.Algorithms.Restrictions;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Edges;
using Itinero.Graphs;
using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Dual
{
    /// <summary>
    /// Contains tests for dual graph contraction.
    /// </summary>
    [TestFixture]
    public class DualGraphContractionTests
    {
        /// <summary>
        /// Tests 2 edges.
        /// </summary>
        [Test]
        public void Test2Edges()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 10,
                Profile = 1
            }));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));
            graph.AddEdge(2, 0, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 1000,
                Profile = 1
            }));
            // build dual.
            var target = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                    ContractedEdgeDataSerializer.MetaSize);
            var dualGraphBuilder = new DualGraphBuilder(graph,
                target,
                new Itinero.Algorithms.Weights.DefaultWeightHandler((p) => new Itinero.Profiles.Factor()
                {
                    Direction = 0,
                    Value = 1
                }),
                new RestrictionCollection((c, v) => false));
            dualGraphBuilder.Run();

            var priorityCalculator = new EdgeDifferencePriorityCalculator(target,
                new DykstraWitnessCalculator(int.MaxValue));
            priorityCalculator.DifferenceFactor = 5;
            priorityCalculator.DepthFactor = 5;
            priorityCalculator.ContractedFactor = 8;
            var hierarchyBuilder = new HierarchyBuilder(target, priorityCalculator, new DykstraWitnessCalculator(int.MaxValue));
            hierarchyBuilder.Run();
        }
    }
}