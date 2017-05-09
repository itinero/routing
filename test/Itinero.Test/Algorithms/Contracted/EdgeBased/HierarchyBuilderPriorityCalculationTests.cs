///*
// *  Licensed to SharpSoftware under one or more contributor
// *  license agreements. See the NOTICE file distributed with this work for 
// *  additional information regarding copyright ownership.
// * 
// *  SharpSoftware licenses this file to you under the Apache License, 
// *  Version 2.0 (the "License"); you may not use this file except in 
// *  compliance with the License. You may obtain a copy of the License at
// * 
// *       http://www.apache.org/licenses/LICENSE-2.0
// * 
// *  Unless required by applicable law or agreed to in writing, software
// *  distributed under the License is distributed on an "AS IS" BASIS,
// *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// *  See the License for the specific language governing permissions and
// *  limitations under the License.
// */

//using Itinero.Algorithms;
//using Itinero.Algorithms.Collections;
//using Itinero.Algorithms.Contracted.EdgeBased;
//using Itinero.Algorithms.Contracted.EdgeBased.Witness;
//using Itinero.Data.Contracted.Edges;
//using Itinero.Graphs.Directed;
//using NUnit.Framework;
//using System;

//namespace Itinero.Test.Algorithms.Contracted.EdgeBased
//{
//    /// <summary>
//    /// Containts tests for the edge difference priority calculator.
//    /// </summary>
//    [TestFixture]
//    public class HierarchyBuilderPriorityCalculationTests
//    {
//        /// <summary>
//        /// Tests calculator with no neighbours.
//        /// </summary>
//        [Test]
//        public void TestNoNeighbours()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(1, 0, 100, null);

//            // create a witness calculator and the priority calculator.
//            var hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(0, priority);
//        }

//        /// <summary>
//        /// Tests calculator with one neighbour and no witnesses.
//        /// </summary>
//        [Test]
//        public void TestOneNeighbour()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, null);
//            graph.AddEdge(1, 0, 100, null);

//            // create a witness calculator and the priority calculator.
//            var hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(-1, priority);
//        }

//        /// <summary>
//        /// Tests calculator with two neighbours and no witnesses.
//        /// </summary>
//        [Test]
//        public void TestTwoNeighbours()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, null);
//            graph.AddEdge(1, 0, 100, null);
//            graph.AddEdge(0, 2, 100, null);
//            graph.AddEdge(2, 0, 100, null);

//            // create a witness calculator and the priority calculator.
//            var hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(0, priority);
//        }

//        /// <summary>
//        /// Tests calculator with three neighbours and no witnesses.
//        /// </summary>
//        [Test]
//        public void TestThreeNeighbours()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, null);
//            graph.AddEdge(1, 0, 100, null);
//            graph.AddEdge(0, 2, 100, null);
//            graph.AddEdge(2, 0, 100, null);
//            graph.AddEdge(0, 3, 100, null);
//            graph.AddEdge(3, 0, 100, null);

//            // create a witness calculator and the priority calculator.
//            var hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(3, priority);
//        }

//        /// <summary>
//        /// Tests calculator with two neighbours, oneway edges, and no witnesses.
//        /// </summary>
//        [Test]
//        public void TestTwoNeighboursOneWay()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, true);
//            graph.AddEdge(1, 0, 100, false);
//            graph.AddEdge(0, 2, 100, false);
//            graph.AddEdge(2, 0, 100, true);

//            // create a witness calculator and the priority calculator.
//            var hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(0, priority);

//            // build another graph.
//            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, false);
//            graph.AddEdge(1, 0, 100, true);
//            graph.AddEdge(0, 2, 100, true);
//            graph.AddEdge(2, 0, 100, false);

//            // create a witness calculator and the priority calculator.
//            hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(0, priority);
//        }

//        /// <summary>
//        /// Tests calculator with two neighbours, oneway opposite edges, and no witnesses.
//        /// </summary>
//        [Test]
//        public void TestTwoNeighboursOneWayOpposite()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, true);
//            graph.AddEdge(1, 0, 100, false);
//            graph.AddEdge(0, 2, 100, true);
//            graph.AddEdge(2, 0, 100, false);

//            // create a witness calculator and the priority calculator.
//            var hierarchyBuilder = new HierarchyBuilder(graph, new DykstraWitnessCalculator(int.MaxValue), i => null);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(-2, priority);
//        }

//        /// <summary>
//        /// Tests calculator with one neighbour but with contracted neighbour.
//        /// </summary>
//        [Test]
//        public void TestOneNeighboursContracted()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, true);
//            graph.AddEdge(1, 0, 100, false);
//            graph.AddEdge(0, 2, 100, null);
//            graph.AddEdge(2, 0, 100, null);

//            // create a witness calculator and the priority calculator.
//            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);
//            var hierarchyBuilder = new HierarchyBuilder(graph, witnessCalculator, (v) => null);
//            hierarchyBuilder.NotifyContracted(2);
//            graph.RemoveEdge(0, 2);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(2, priority);
//        }

//        /// <summary>
//        /// Tests calculator with two neighbour but with one contracted neighbour.
//        /// </summary>
//        [Test]
//        public void TestTwoNeighboursContracted()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 1, 100, null);
//            graph.AddEdge(1, 0, 100, null);
//            graph.AddEdge(0, 2, 100, null);
//            graph.AddEdge(2, 0, 100, null);
//            graph.AddEdge(0, 3, 100, null);
//            graph.AddEdge(3, 0, 100, null);
            
//            // create a witness calculator and the priority calculator.
//            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);
//            var hierarchyBuilder = new HierarchyBuilder(graph, witnessCalculator, (v) => null);
//            hierarchyBuilder.DepthFactor = 0;
//            hierarchyBuilder.ContractedFactor = 1;
//            hierarchyBuilder.DifferenceFactor = 1;
//            hierarchyBuilder.NotifyContracted(3);
//            graph.RemoveEdge(0, 3);
//            var priority = hierarchyBuilder.CalculatePriority(0);

//            Assert.AreEqual(1, priority);
//        }

//        /// <summary>
//        /// Tests calculator on a small network with 4 vertices in a quadrilateral.
//        /// </summary>
//        [Test]
//        public void TestQuadrilateralOneWay()
//        {
//            // build graph.
//            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
//            graph.AddEdge(0, 2, 100, true);
//            graph.AddEdge(2, 0, 100, false);
//            graph.AddEdge(0, 3, 10, false);
//            graph.AddEdge(3, 0, 10, true);
//            graph.AddEdge(1, 2, 1000, false);
//            graph.AddEdge(2, 1, 1000, true);
//            graph.AddEdge(1, 3, 10000, true);
//            graph.AddEdge(3, 1, 10000, false);
//            graph.Compress();

//            // create a witness calculator and the priority calculator.
//            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);
//            var hierarchyBuilder = new HierarchyBuilder(graph, witnessCalculator, (v) => null);

//            Assert.AreEqual(0, hierarchyBuilder.CalculatePriority(0));
//            Assert.AreEqual(0, hierarchyBuilder.CalculatePriority(1));
//            Assert.AreEqual(0, hierarchyBuilder.CalculatePriority(2));
//            Assert.AreEqual(0, hierarchyBuilder.CalculatePriority(3));
//        }
//    }
//}