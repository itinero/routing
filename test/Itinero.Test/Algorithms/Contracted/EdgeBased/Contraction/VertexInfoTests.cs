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
            info.BuildShortcuts(new DefaultWeightHandler(null), new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue));
        }

        /// <summary>
        /// Tests calculating priority.
        /// </summary>
        [Test]
        public void TestPriority()
        {
            var info = new VertexInfo<float>();

            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            var witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue);

            // test 0
            info.Clear();
            info.Vertex = 0;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(-1, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 1.
            info.Clear();
            info.Vertex = 1;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(0, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 2.
            info.Clear();
            info.Vertex = 2;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(-1, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));
            
            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(0, 2, 100, null, 3, new uint[] { 1 }, new uint[] { 1 });
            graph.AddEdge(2, 0, 100, null, 3, new uint[] { 1 }, new uint[] { 1 });
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue);

            // test 0
            info.Clear();
            info.Vertex = 0;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(0, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 1.
            info.Clear();
            info.Vertex = 1;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(-2, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 2.
            info.Clear();
            info.Vertex = 2;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(0, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));
            
            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(1, 3, 100, null);
            graph.AddEdge(3, 1, 100, null);
            graph.Compress();
            witnessCalculator = new DykstraWitnessCalculator(graph, (i) =>
            {
                if (i == 0 || i == 1 || i == 2)
                {
                    return new uint[][]
                    {
                            new uint[] { 0, 1, 2 }
                    };
                }
                return null;
            }, int.MaxValue);

            // test 0
            info.Clear();
            info.Vertex = 0;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(-1, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 1.
            info.Clear();
            info.Vertex = 1;
            info.HasRestrictions = true;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(3, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 2.
            info.Clear();
            info.Vertex = 2;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(-1, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));

            // test 2.
            info.Clear();
            info.Vertex = 2;
            info.AddRelevantEdges(graph.GetEdgeEnumerator());
            info.BuildShortcuts(new DefaultWeightHandler(null), witnessCalculator);
            witnessCalculator.Calculate(info.Vertex, info.Shortcuts);
            Assert.AreEqual(-1, info.Priority(new DefaultWeightHandler(null), 1, 0, 0));
        }
    }
}