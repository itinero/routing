// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Graphs.Directed;
using System.Collections.Generic;
using Itinero.Algorithms;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the dykstra witness calculator.
    /// </summary>
    [TestFixture]
    public class DykstraWitnessCalculatorTests
    {
        /// <summary>
        /// Test on one edge with one hop.
        /// </summary>
        [Test]
        public void TestOneEdgeOneHop()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100, null);

            var witnessCalculator = new DykstraWitnessCalculator(1);

            // calculate witness for weight of 200.
            var forwardWitnesses = new EdgePath<float>[1];
            var backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 1 }), new List<float>(new float[] { 200 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(1, forwardWitnesses[0].Vertex);
            Assert.AreEqual(1, backwardWitnesses[0].Vertex);

            // calculate witness for weight of 50.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 1 }), new List<float>(new float[] { 50 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        }

        /// <summary>
        /// Test on two edges with two hops.
        /// </summary>
        [Test]
        public void TestTwoEdgeInfiniteHops()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 2, 100, null);

            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

            // calculate witness for weight of 200.
            var forwardWitnesses = new EdgePath<float>[1];
            var backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 1000 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(2, forwardWitnesses[0].Vertex);
            Assert.AreEqual(2, backwardWitnesses[0].Vertex);

            // calculate witness for weight of 50.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 50 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        }

        /// <summary>
        /// Test on two oneway edges with two hops.
        /// </summary>
        [Test]
        public void TestTwoOnewayEdgeInfiniteHops()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100, true);
            graph.AddEdge(1, 2, 100, true);
            graph.AddEdge(0, 2, 300, true);

            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

            // calculate witness for weight of 200.
            var forwardWitnesses = new EdgePath<float>[1];
            var backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 1000 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(2, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witness for weight of 50.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 50 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // build graph.
            graph = new DirectedDynamicGraph(1);
            graph.AddEdge(1, 0, 100, true);
            graph.AddEdge(2, 1, 100, true);
            graph.AddEdge(2, 0, 100, true);

            // calculate witness for weight of 200.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 2, new List<uint>(new uint[] { 0 }), new List<float>(new float[] { 1000 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(0, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witness for weight of 50.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 2, new List<uint>(new uint[] { 0 }), new List<float>(new float[] { 50 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        }

        /// <summary>
        /// Tests calculating witnesses in a quadrilateral.
        /// </summary>
        [Test]
        public void TestQuadrilateralOneWay()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 2, 100, true);
            graph.AddEdge(2, 0, 100, false);
            graph.AddEdge(0, 3, 10, false);
            graph.AddEdge(3, 0, 10, true);
            graph.AddEdge(1, 2, 1000, false);
            graph.AddEdge(2, 1, 1000, true);
            graph.AddEdge(1, 3, 10000, true);
            graph.AddEdge(3, 1, 10000, false);
            graph.Compress(false);

            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

            // calculate witnesses for 0.
            var forwardWitnesses = new EdgePath<float>[1];
            var backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 2, new List<uint>(new uint[] { 3 }), new List<float>(new float[] { 110 }),
                ref forwardWitnesses, ref backwardWitnesses, 0);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witnesses for 0.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 3, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 110 }),
                ref forwardWitnesses, ref backwardWitnesses, 0);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witnesses for 2.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 1 }), new List<float>(new float[] { 1100 }),
                ref forwardWitnesses, ref backwardWitnesses, 2);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witnesses for 2.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 1, new List<uint>(new uint[] { 0 }), new List<float>(new float[] { 1100 }),
                ref forwardWitnesses, ref backwardWitnesses, 2);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witnesses for 1.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 3, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 11000 }),
                ref forwardWitnesses, ref backwardWitnesses, 1);
            Assert.AreEqual(2, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witnesses for 1.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 2, new List<uint>(new uint[] { 3 }), new List<float>(new float[] { 11000 }),
                ref forwardWitnesses, ref backwardWitnesses, 1);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(3, backwardWitnesses[0].Vertex);

            // calculate witnesses for 3.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 1 }), new List<float>(new float[] { 10010 }),
                ref forwardWitnesses, ref backwardWitnesses, 3);
            Assert.AreEqual(1, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // calculate witnesses for 3.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 1, new List<uint>(new uint[] { 0 }), new List<float>(new float[] { 10010 }),
                ref forwardWitnesses, ref backwardWitnesses, 3);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(0, backwardWitnesses[0].Vertex);
        }

        /// <summary>
        /// Test on two edges with one simple restriction.
        /// </summary>
        [Test]
        public void TestWithSimpleRestriction()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 2, 100, null);

            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

            // calculate witness for weight of 200.
            var forwardWitnesses = new EdgePath<float>[1];
            var backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => { return new uint[][] { new uint[] { 0, 1, 2 } }; }, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { 1000 }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(2, backwardWitnesses[0].Vertex);
        }

        /// <summary>
        /// Test handling u-turns.
        /// </summary>
        [Test]
        public void TestUTurnHandling()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 3, 100, null, 1, null, null);
            graph.AddEdge(3, 0, 100, null, 1, null, null);
            graph.AddEdge(2, 3, 100, null, 1, null, null);
            graph.AddEdge(3, 2, 100, null, 1, null, null);

            // a path from 0->2 is impossible because it would involve a u-turn at vertex 3 resulting in an original path 0->1->3->1->2.
            // same for 2->0

            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);

            // calculate witness for weight of 200.
            var forwardWitnesses = new EdgePath<float>[1];
            var backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { float.MaxValue }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);

            // build graph.
            graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 3, 100, null, 1, null, new uint[] { 100 });
            graph.AddEdge(3, 0, 100, null, 1, new uint[] { 100 }, null);
            graph.AddEdge(2, 3, 100, null, 1, null, new uint[] { 100 });
            graph.AddEdge(3, 2, 100, null, 1, new uint[] { 100 }, null);

            // a path from 0->2 is impossible because it would involve a u-turn at vertex 3 resulting in an original path 0->...->100->3->100->...->2.
            // same for 2->0

            // calculate witness for weight of 200.
            forwardWitnesses = new EdgePath<float>[1];
            backwardWitnesses = new EdgePath<float>[1];
            witnessCalculator.Calculate(graph, (i) => null, 0, new List<uint>(new uint[] { 2 }), new List<float>(new float[] { float.MaxValue }),
                ref forwardWitnesses, ref backwardWitnesses, uint.MaxValue);
            Assert.AreEqual(Constants.NO_VERTEX, forwardWitnesses[0].Vertex);
            Assert.AreEqual(Constants.NO_VERTEX, backwardWitnesses[0].Vertex);
        }
    }
}