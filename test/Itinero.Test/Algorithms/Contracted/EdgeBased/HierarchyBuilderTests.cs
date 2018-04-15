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
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Graphs.Directed;
using System.Collections.Generic;
using System.Linq;
using Itinero.Data.Contracted.Edges;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the hierarchy builder.
    /// </summary>
    [TestFixture]
    public class HierarchyBuilderTests
    {
        /// <summary>
        /// Tests contracting a graph with 2 vertices.
        /// </summary>
        [Test]
        public void Test2Vertices()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.Compress();

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)), 
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edge10 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge10);
        }

        /// <summary>
        /// Tests contracting a graph with 3 vertices.
        /// </summary>
        [Test]
        public void Test3Vertices()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.Compress();

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)),
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edge10 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge10);
            var edge12 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge12);
            var edges21 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges21);
        }

        /// <summary>
        /// Tests contracting a complete graph with 3 vertices.
        /// </summary>
        [Test]
        public void Test3VerticesComplete()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(0, 2, 100, null);
            graph.AddEdge(2, 0, 100, null);
            graph.Compress();

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)),
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edges02 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edges02);
            var edge10 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge10);
            var edge12 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edge12);
            var edges20 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edges20);
            var edges21 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges21);
        }

        /// <summary>
        /// Tests contracting a pentagon.
        /// </summary>
        [Test]
        public void TestPentagon()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(2, 3, 100, null);
            graph.AddEdge(3, 2, 100, null);
            graph.AddEdge(3, 4, 100, null);
            graph.AddEdge(4, 3, 100, null);
            graph.AddEdge(4, 0, 100, null);
            graph.AddEdge(0, 4, 100, null);
            graph.Compress();

            // contract graph.
            var priorityCalculator = new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue));
            priorityCalculator.ContractedFactor = 0;
            priorityCalculator.DepthFactor = 0;
            var hierarchyBuilder = new HierarchyBuilder(graph, priorityCalculator,
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edges10 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edges10);

            var edges12 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edges12);
            var edges21 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges21);

            var edges23 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edges23);
            var edges32 = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edges32);

            var edges34 = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNull(edges34);
            var edges43 = graph.GetEdgeEnumerator(4).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edges43);

            var edges40 = graph.GetEdgeEnumerator(4).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edges40);
            var edges04 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNotNull(edges04);

            var edges41 = graph.GetEdgeEnumerator(4).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges41);
            var edges14 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNull(edges14);

            var edges31 = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges31);
            var edges13 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNull(edges13);
        }

        /// <summary>
        /// Tests contracting a pentagon.
        /// </summary>
        [Test]
        public void TestPentagonDirected()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, true);
            graph.AddEdge(1, 0, 100, false);
            graph.AddEdge(1, 2, 100, true);
            graph.AddEdge(2, 1, 100, false);
            graph.AddEdge(2, 3, 100, true);
            graph.AddEdge(3, 2, 100, false);
            graph.AddEdge(3, 4, 100, true);
            graph.AddEdge(4, 3, 100, false);
            graph.AddEdge(4, 0, 100, true);
            graph.AddEdge(0, 4, 100, false);
            graph.Compress();

            // contract graph.
            var priorityCalculator = new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue));
            priorityCalculator.ContractedFactor = 0;
            priorityCalculator.DepthFactor = 0;
            var hierarchyBuilder = new HierarchyBuilder(graph, priorityCalculator,
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edges10 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edges10);

            var edges12 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edges12);
            var edges21 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges21);

            var edges23 = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edges23);
            var edges32 = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edges32);

            var edges34 = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNull(edges34);
            var edges43 = graph.GetEdgeEnumerator(4).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edges43);

            var edges40 = graph.GetEdgeEnumerator(4).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edges40);
            var edges04 = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNotNull(edges04);

            var edges41 = graph.GetEdgeEnumerator(4).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges41);
            var s1 = edges41.GetSequence1();
            var s2 = edges41.GetSequence2();
            Assert.AreEqual(1, s1.Length);
            Assert.AreEqual(0, s1[0]);
            Assert.AreEqual(1, s2.Length);
            Assert.AreEqual(0, s2[0]);
            var edges14 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNull(edges14);

            var edges31 = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges31);
            s1 = edges31.GetSequence1();
            s2 = edges31.GetSequence2();
            Assert.AreEqual(1, s1.Length);
            Assert.AreEqual(4, s1[0]);
            Assert.AreEqual(1, s2.Length);
            Assert.AreEqual(0, s2[0]);
            var edges13 = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNull(edges13);
        }

        /// <summary>
        /// Tests a contraction of two vertices that would result in two edges between the same vertices.
        /// </summary>
        [Test]
        public void TestDoubleContraction()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 2, 100, null);
            graph.AddEdge(2, 0, 100, null);
            graph.AddEdge(0, 3, 100, null);
            graph.AddEdge(3, 0, 100, null);
            graph.AddEdge(1, 2, 200, null);
            graph.AddEdge(2, 1, 200, null);
            graph.AddEdge(1, 3, 200, null);
            graph.AddEdge(3, 1, 200, null);
            graph.Compress();

            // contract graph.
            var priorityCalculator = new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue));
            priorityCalculator.DepthFactor = 0;
            priorityCalculator.ContractedFactor = 0;
            var hierarchyBuilder = new HierarchyBuilder(graph, priorityCalculator,
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edge);
            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNotNull(edge);

            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNull(edge);
            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNotNull(edge);

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNull(edge);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);

            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNull(edge);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
        }

        /// <summary>
        /// Tests a contraction of two vertices that would result in two oneway edges between the same vertices.
        /// </summary>
        [Test]
        public void TestDoubleContractionOneway()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(1, 3, 10, null);
            graph.AddEdge(3, 1, 10, null);
            graph.AddEdge(3, 2, 10, null);
            graph.AddEdge(2, 3, 10, null);
            graph.Compress();

            // contract graph.
            var priorities = new Dictionary<uint, float>();
            priorities.Add(1, 0);
            priorities.Add(0, 1);
            priorities.Add(2, 2);
            priorities.Add(3, 3);
            var hierarchyBuilder = new HierarchyBuilder(graph,
                new MockPriorityCalculator(priorities),
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // edges 1->2 and 2->1 should have been removed.
            var edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            var edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(110, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());

            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);

            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(10, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(10, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
        }

        /// <summary>
        /// Tests an uncontracted edge that already has a witness path.
        /// </summary>
        [Test]
        public void TestUncontractedWitnessed()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(1, 3, 100, null);
            graph.AddEdge(3, 1, 100, null);
            graph.Compress();

            // contract graph.
            var priorities = new Dictionary<uint, float>();
            priorities.Add(1, 0);
            priorities.Add(0, 1);
            priorities.Add(2, 2);
            priorities.Add(3, 3);
            var hierarchyBuilder = new HierarchyBuilder(graph, 
                new MockPriorityCalculator(priorities),
                new DykstraWitnessCalculator(int.MaxValue), (i) =>
                {
                    if (i == 0 || i == 1 || i == 2)
                    {
                        return new uint[][]
                        {
                            new uint[] { 0, 1, 2 }
                        };
                    }
                    return null;
                });
            hierarchyBuilder.Run();

            // check all edges.
            var edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            var edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(false, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
        }

        /// <summary>
        /// Tests contracting a restricted network.
        /// </summary>
        [Test]
        public void TestRestrictedNetwork1()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(1, 3, 100, null);
            graph.AddEdge(3, 1, 100, null);
            graph.Compress();

            // contract graph.
            var priorities = new Dictionary<uint, float>();
            priorities.Add(1, 0);
            priorities.Add(0, 1);
            priorities.Add(2, 2);
            priorities.Add(3, 3);
            var hierarchyBuilder = new HierarchyBuilder(graph,
                new MockPriorityCalculator(priorities),
                new DykstraWitnessCalculator(int.MaxValue), (i) =>
                {
                    if (i == 0 || i == 1 || i == 2)
                    {
                        return new uint[][]
                        {
                            new uint[] { 0, 1, 2 }
                        };
                    }
                    return null;
                });
            hierarchyBuilder.Run();

            // check all edges.
            var edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            var edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(false, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());

            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
        }

        /// <summary>
        /// Tests contracting a restricted network.
        /// </summary>
        [Test]
        public void TestRestrictedNetwork2()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(1, 2, 100, null);
            graph.AddEdge(2, 1, 100, null);
            graph.AddEdge(1, 3, 100, null);
            graph.AddEdge(3, 1, 100, null);
            graph.AddEdge(1, 4, 100, null);
            graph.AddEdge(4, 1, 100, null);
            graph.AddEdge(2, 3, 100, null);
            graph.AddEdge(3, 2, 100, null);
            graph.Compress();
            
            // contract graph.
            var priorities = new Dictionary<uint, float>();
            priorities.Add(1, 0);
            priorities.Add(0, 1);
            priorities.Add(2, 2);
            priorities.Add(3, 3);
            priorities.Add(4, 4);
            var hierarchyBuilder = new HierarchyBuilder(graph,
                new MockPriorityCalculator(priorities),
                new DykstraWitnessCalculator(int.MaxValue), (i) =>
                {
                    if (i == 0 || i == 1 || i == 4)
                    {
                        return new uint[][]
                        {
                            new uint[] { 0, 1, 4 }
                        };
                    }
                    return null;
                });
            hierarchyBuilder.Run();

            // check all edges.
            ContractedEdgeData edgeData;
            var edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x =>
            {
                if (x.Neighbour == 4)
                {
                    edgeData = ContractedEdgeDataSerializer.Deserialize(x.Data[0], Constants.NO_VERTEX);
                    return edgeData.Direction == true;
                }
                return false;
            });
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(500, edgeData.Weight);
            Assert.AreEqual(true, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x =>
            {
                if (x.Neighbour == 4)
                {
                    edgeData = ContractedEdgeDataSerializer.Deserialize(x.Data[0], Constants.NO_VERTEX);
                    return edgeData.Direction == false;
                }
                return false;
            });
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(false, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(null, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(null, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(null, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(null, edge.GetContracted());

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(null, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
            
            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 4);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(200, edgeData.Weight);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(1, edge.GetContracted());
        }
    }
}