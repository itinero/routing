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

        ///// <summary>
        ///// Test on one edge with one hop.
        ///// </summary>
        //[Test]
        //public void TestOneEdgeOneHop()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 1, 100, null);

        //    var witnessCalculator = new DykstraWitnessCalculator(1);

        //    // calculate witness for weight of 200.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 1 }), new List<float>(new float[] { 200 }),
        //        ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(1, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(1, backwardWitnesses[0].Vertex);

        //    // calculate witness for weight of 50.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 1 }), new List<float>(new float[] { 50 }),
        //        ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        //}

        ///// <summary>
        ///// Test on two edges with two hops.
        ///// </summary>
        //[Test]
        //public void TestTwoEdgeInfiniteHops()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 1, 100, null);
        //    graph.AddEdge(1, 2, 100, null);

        //    var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

        //    // calculate witness for weight of 200.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 2) }),
        //        new List<float>(new float[] { 1000 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(2, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(2, backwardWitnesses[0].Vertex);

        //    // calculate witness for weight of 50.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 2) }),
        //        new List<float>(new float[] { 50 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        //}

        ///// <summary>
        ///// Test on two oneway edges with two hops.
        ///// </summary>
        //[Test]
        //public void TestTwoOnewayEdgeInfiniteHops()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 1, 100, true);
        //    graph.AddEdge(1, 2, 100, true);
        //    graph.AddEdge(0, 2, 300, true);

        //    var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

        //    // calculate witness for weight of 200.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 2) }),
        //        new List<float>(new float[] { 1000 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(2, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witness for weight of 50.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 2) }),
        //        new List<float>(new float[] { 50 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // build graph.
        //    graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(1, 0, 100, true);
        //    graph.AddEdge(2, 1, 100, true);
        //    graph.AddEdge(2, 0, 100, true);

        //    // calculate witness for weight of 200.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(2, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 0) }),
        //        new List<float>(new float[] { 1000 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(0, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witness for weight of 50.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(2, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 0) }),
        //        new List<float>(new float[] { 50 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        //}

        ///// <summary>
        ///// Tests calculating witnesses in a quadrilateral.
        ///// </summary>
        //[Test]
        //public void TestQuadrilateralOneWay()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 2, 100, true);
        //    graph.AddEdge(2, 0, 100, false);
        //    graph.AddEdge(0, 3, 10, false);
        //    graph.AddEdge(3, 0, 10, true);
        //    graph.AddEdge(1, 2, 1000, false);
        //    graph.AddEdge(2, 1, 1000, true);
        //    graph.AddEdge(1, 3, 10000, true);
        //    graph.AddEdge(3, 1, 10000, false);
        //    graph.Compress(false);

        //    var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

        //    // calculate witnesses for 0.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(2, 0), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(0, 3) }),
        //        new List<float>(new float[] { 110 }), ref forwardWitnesses, ref backwardWitnesses, 0);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 0.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(3, 0), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(0, 2) }),
        //        new List<float>(new float[] { 110 }), ref forwardWitnesses, ref backwardWitnesses, 0);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 2.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 2), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(2, 1) }),
        //        new List<float>(new float[] { 1100 }), ref forwardWitnesses, ref backwardWitnesses, 2);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 2.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(1, 2), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(2, 0) }),
        //        new List<float>(new float[] { 1100 }), ref forwardWitnesses, ref backwardWitnesses, 2);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 1.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(3, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 2) }),
        //        new List<float>(new float[] { 11000 }), ref forwardWitnesses, ref backwardWitnesses, 1);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 1.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(2, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 3) }),
        //        new List<float>(new float[] { 11000 }), ref forwardWitnesses, ref backwardWitnesses, 1);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 3.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 3), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(3, 1) }),
        //        new List<float>(new float[] { 10010 }), ref forwardWitnesses, ref backwardWitnesses, 3);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // calculate witnesses for 3.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(1, 3), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(3, 0) }),
        //        new List<float>(new float[] { 10010 }), ref forwardWitnesses, ref backwardWitnesses, 3);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        //}

        ///// <summary>
        ///// Test on two edges with one simple restriction.
        ///// </summary>
        //[Test]
        //public void TestWithSimpleRestriction()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 1, 100, null);
        //    graph.AddEdge(1, 2, 100, null);

        //    var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

        //    // calculate witness for weight of 200.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => { return new uint[][] { new uint[] { 0, 1, 2 } }; }, new OriginalEdge(0, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(1, 2) }),
        //        new List<float>(new float[] { 1000 }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(2, backwardWitnesses[0].Vertex);
        //}

        ///// <summary>
        ///// Test handling u-turns.
        ///// </summary>
        //[Test]
        //public void TestUTurnHandling()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 3, 100, null, 1, null, null);
        //    graph.AddEdge(3, 0, 100, null, 1, null, null);
        //    graph.AddEdge(2, 3, 100, null, 1, null, null);
        //    graph.AddEdge(3, 2, 100, null, 1, null, null);

        //    // a path from 0->2 is impossible because it would involve a u-turn at vertex 3 resulting in an original path 0->1->3->1->2.
        //    // same for 2->0

        //    var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

        //    // calculate witness for weight of 200.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 3), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(3, 2) }),
        //        new List<float>(new float[] { float.MaxValue }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

        //    // build graph.
        //    graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 3, 100, null, 1, null, new uint[] { 100 });
        //    graph.AddEdge(3, 0, 100, null, 1, new uint[] { 100 }, null);
        //    graph.AddEdge(2, 3, 100, null, 1, null, new uint[] { 100 });
        //    graph.AddEdge(3, 2, 100, null, 1, new uint[] { 100 }, null);

        //    // a path from 0->2 is impossible because it would involve a u-turn at vertex 3 resulting in an original path 0->...->100->3->100->...->2.
        //    // same for 2->0

        //    // calculate witness for weight of 200.
        //    forwardWitnesses = new EdgePath<float>[1];
        //    backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 3), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(3, 2) }),
        //        new List<float>(new float[] { float.MaxValue }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        //}

        ///// <summary>
        ///// An extra test to specifically test for witnessed paths.
        ///// </summary>
        ///// <remarks>Test needed to be added after modifications to the witness calculator to properly work with edges.</remarks>
        //[Test]
        //public void TestWitnessed()
        //{
        //    // build graph.
        //    var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
        //    graph.AddEdge(0, 1, 100, null);
        //    graph.AddEdge(1, 2, 100, null);
        //    graph.AddEdge(2, 0, 100, null);

        //    var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

        //    // calculate witness for weight of 200.
        //    var forwardWitnesses = new EdgePath<float>[1];
        //    var backwardWitnesses = new EdgePath<float>[1];
        //    witnessCalculator.Calculate(graph, (i) => null, new OriginalEdge(0, 1), new List<OriginalEdge>(new OriginalEdge[] { new OriginalEdge(2, 0) }),
        //        new List<float>(new float[] { float.MaxValue }), ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
        //    Assert.AreEqual(0, forwardWitnesses[0].Vertex);
        //    Assert.AreEqual(0, backwardWitnesses[0].Vertex);
        //}
    }
}