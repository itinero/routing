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

using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Algorithms.Contracted.EdgeBased.Contraction;
using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Contains tests for the vertex info data structure.
    /// </summary>
    [TestFixture]
    public class VertexInfoTests
    {
        /// <summary>
        /// Tests adding relevant edge.
        /// </summary>
        [Test]
        public void TestAddRelevantEdges()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);

            // build vertex info.
            var info = new VertexInfo<float>();
            info.Vertex = 0;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());

            Assert.AreEqual(1, info.Count);
        }

        /// <summary>
        /// Tests building shortcuts.
        /// </summary>
        [Test]
        public void TestBuildShortcuts()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(0, 2, 100, null);
            graph.AddEdge(2, 0, 100, null);

            // build vertex info.
            var info = new VertexInfo<float>();
            info.Vertex = 0;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());

            // build shortctus.
            info.BuildShortcuts(new DefaultWeightHandler(null));
        }
    }
}