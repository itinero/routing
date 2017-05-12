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

using NUnit.Framework;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Graphs.Directed;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Contracted.EdgeBased.Contraction;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Contains tests for the dykstra witness calculator.
    /// </summary>
    [TestFixture]
    public class DykstraWitnessCalculatorTests
    {
        /// <summary>
        /// Tests calculating one turn.
        /// </summary>
        [Test]
        public void TestCalculateTurn()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);

            var edge1 = graph.GetEdges(1).Find(x => x.Neighbour == 0);
            var edge2 = graph.GetEdges(1).Find(x => x.Neighbour == 2);

            // calculate turn weights (should be 0).
            float weightForward, weightBackward;
            var witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue);
            witnessCalculator.CalculateTurn(1, edge1, edge2, out weightForward, out weightBackward);
            Assert.AreEqual(200, weightForward);
            Assert.AreEqual(200, weightBackward);

            // calculate turn weights with a restriction in one direction.
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[][] {
                new uint[] { 0, 1, 2 }
            }, int.MaxValue);
            witnessCalculator.CalculateTurn(1, edge1, edge2, out weightForward, out weightBackward);
            Assert.AreEqual(float.MaxValue, weightForward);
            Assert.AreEqual(200, weightBackward);

            // calculate turn weights with a restriction in one direction.
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[][] {
                new uint[] { 2, 1, 0 }
            }, int.MaxValue);
            witnessCalculator.CalculateTurn(1, edge1, edge2, out weightForward, out weightBackward);
            Assert.AreEqual(200, weightForward);
            Assert.AreEqual(float.MaxValue, weightBackward);

            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(1, 1, 100, null, 3, new uint[] { 3 }, new uint[] { 4 });
            graph.AddEdge(1, 1, 100, null, 3, new uint[] { 4 }, new uint[] { 3 });

            edge1 = graph.GetEdges(1).Find(x => x.Neighbour == 0);
            edge2 = graph.GetEdges(1).Find(x => x.Neighbour == 2);

            // calculate turn weights with a restriction in one direction.
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[][] {
                new uint[] { 2, 1, 0 }
            }, int.MaxValue);
            witnessCalculator.CalculateTurn(1, edge1, edge2, out weightForward, out weightBackward);
            Assert.AreEqual(200, weightForward);
            Assert.AreEqual(300, weightBackward);

            // calculate turn weights with a restriction in one direction.
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[][] {
                new uint[] { 0, 1, 2 },
                new uint[] { 2, 1, 0 }
            }, int.MaxValue);
            witnessCalculator.CalculateTurn(1, edge1, edge2, out weightForward, out weightBackward);
            Assert.AreEqual(300, weightForward);
            Assert.AreEqual(300, weightBackward);
        }

        /// <summary>
        /// Test on two edges with two hops.
        /// </summary>
        [Test]
        public void TestTwoEdgeInfiniteHops()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 2, 100, null);
            var witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue);

            // build shortcuts and test witness calculation.
            var shortcuts = new Shortcuts<float>();
            var accessor = shortcuts.GetAccessor();
            accessor.AddSource(new OriginalEdge(0, 1));
            accessor.Add(new Shortcut<float>()
            {
                Backward = 1000,
                Edge = new OriginalEdge(1, 2),
                Forward = 1000
            });
            witnessCalculator.Calculate(shortcuts);
            accessor.Reset();
            Assert.IsTrue(accessor.MoveNextSource());
            Assert.AreEqual(0, accessor.Source.Vertex1);
            Assert.AreEqual(1, accessor.Source.Vertex2);
            Assert.IsTrue(accessor.MoveNextTarget());
            Assert.AreEqual(1, accessor.Current.Edge.Vertex1);
            Assert.AreEqual(2, accessor.Current.Edge.Vertex2);
            Assert.AreEqual(0, accessor.Current.Forward);
            Assert.AreEqual(0, accessor.Current.Backward);
            
            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 4, 100, null, 2, new uint[] { 1 }, new uint[] { 3 });
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue);

            // build shortcuts and test witness calculation.
            shortcuts = new Shortcuts<float>();
            accessor = shortcuts.GetAccessor();
            accessor.AddSource(new OriginalEdge(0, 1));
            accessor.Add(new Shortcut<float>()
            {
                Backward = 1000,
                Edge = new OriginalEdge(3, 4),
                Forward = 1000
            });
            witnessCalculator.Calculate(shortcuts);
            accessor.Reset();
            Assert.IsTrue(accessor.MoveNextSource());
            Assert.AreEqual(0, accessor.Source.Vertex1);
            Assert.AreEqual(1, accessor.Source.Vertex2);
            Assert.IsTrue(accessor.MoveNextTarget());
            Assert.AreEqual(3, accessor.Current.Edge.Vertex1);
            Assert.AreEqual(4, accessor.Current.Edge.Vertex2);
            Assert.AreEqual(0, accessor.Current.Forward);
            Assert.AreEqual(0, accessor.Current.Backward);

            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 4, 100, null, 2, new uint[] { 1 }, new uint[] { 3 });
            graph.AddEdge(4, 8, 100, true, 6, new uint[] { 5 }, new uint[] { 7 });
            //graph.AddEdge(4, 8, 100, false, 6, new uint[] { 5 }, new uint[] { 7 });
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[0][], int.MaxValue);

            // build shortcuts and test witness calculation.
            shortcuts = new Shortcuts<float>();
            accessor = shortcuts.GetAccessor();
            accessor.AddSource(new OriginalEdge(0, 1));
            accessor.Add(new Shortcut<float>()
            {
                Backward = 1000,
                Edge = new OriginalEdge(7, 8),
                Forward = 1000
            });
            //accessor.Add(new Shortcut<float>()
            //{
            //    Backward = 1000,
            //    Edge = new OriginalEdge(3, 4),
            //    Forward = 1000
            //});
            witnessCalculator.Calculate(shortcuts);
            accessor.Reset();
            Assert.IsTrue(accessor.MoveNextSource());
            Assert.AreEqual(0, accessor.Source.Vertex1);
            Assert.AreEqual(1, accessor.Source.Vertex2);
            Assert.IsTrue(accessor.MoveNextTarget());
            Assert.AreEqual(7, accessor.Current.Edge.Vertex1);
            Assert.AreEqual(8, accessor.Current.Edge.Vertex2);
            Assert.AreEqual(0, accessor.Current.Forward);
            Assert.AreEqual(1000, accessor.Current.Backward);

            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 4, 100, null, 3, new uint[] { 3 }, new uint[] { 3 });
            graph.AddEdge(4, 8, 100, true, 6, new uint[] { 5 }, new uint[] { 7 });
            witnessCalculator = new DykstraWitnessCalculator(graph, (v) => new uint[][]{
                new uint[] { 1, 2, 3 }
            }, int.MaxValue);

            // build shortcuts and test witness calculation.
            shortcuts = new Shortcuts<float>();
            accessor = shortcuts.GetAccessor();
            accessor.AddSource(new OriginalEdge(0, 1));
            accessor.Add(new Shortcut<float>()
            {
                Backward = 1000,
                Edge = new OriginalEdge(7, 8),
                Forward = 1000
            });
            accessor.Add(new Shortcut<float>()
            {
                Backward = 1000,
                Edge = new OriginalEdge(3, 4),
                Forward = 1000
            });
            witnessCalculator.Calculate(shortcuts);
            accessor.Reset();
            Assert.IsTrue(accessor.MoveNextSource());
            Assert.AreEqual(0, accessor.Source.Vertex1);
            Assert.AreEqual(1, accessor.Source.Vertex2);
            Assert.IsTrue(accessor.MoveNextTarget());
            Assert.AreEqual(7, accessor.Current.Edge.Vertex1);
            Assert.AreEqual(8, accessor.Current.Edge.Vertex2);
            Assert.AreEqual(1000, accessor.Current.Forward);
            Assert.AreEqual(1000, accessor.Current.Backward);
            Assert.IsTrue(accessor.MoveNextTarget());
            Assert.AreEqual(3, accessor.Current.Edge.Vertex1);
            Assert.AreEqual(4, accessor.Current.Edge.Vertex2);
            Assert.AreEqual(1000, accessor.Current.Forward);
            Assert.AreEqual(0, accessor.Current.Backward);
        }
    }
}