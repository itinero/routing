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
            var graph = new DirectedDynamicGraph(1);
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
            var graph = new DirectedDynamicGraph(1);
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
            var graph = new DirectedDynamicGraph(1);
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
            var graph = new DirectedDynamicGraph(1);
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
            var graph = new DirectedDynamicGraph(1);
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
            Assert.AreEqual(4, s2[0]);
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
            var graph = new DirectedDynamicGraph(1);
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
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 2, 100, true);
            graph.AddEdge(2, 0, 100, false);
            graph.AddEdge(0, 3, 10, false);
            graph.AddEdge(3, 0, 10, true);
            graph.AddEdge(1, 2, 1000, false);
            graph.AddEdge(2, 1, 1000, true);
            graph.AddEdge(1, 3, 10000, true);
            graph.AddEdge(3, 1, 10000, false);
            graph.Compress();

            // contract graph.
            var priorityCalculator = new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue));
            priorityCalculator.ContractedFactor = 0;
            priorityCalculator.DepthFactor = 0;
            var hierarchyBuilder = new HierarchyBuilder(graph, priorityCalculator,
                new DykstraWitnessCalculator(int.MaxValue), (i) => Enumerable.Empty<uint[]>());
            hierarchyBuilder.Run();

            // check edges.
            var edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge);
            var edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(100, edgeData.Weight);
            Assert.AreEqual(true, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge);

            edge = graph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(10, edgeData.Weight);
            Assert.AreEqual(false, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge);

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(110, edgeData.Weight);
            Assert.AreEqual(false, edgeData.Direction);
            Assert.AreEqual(0, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edge);

            edge = graph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(1000, edgeData.Weight);
            Assert.AreEqual(true, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNull(edge);

            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 1 &&
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], Constants.NO_VERTEX).Direction == true);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(1110, edgeData.Weight);
            Assert.AreEqual(2, edge.GetContracted());
            edge = graph.GetEdgeEnumerator(3).FirstOrDefault(x => x.Neighbour == 1 &&
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], Constants.NO_VERTEX).Direction == false);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], Constants.NO_VERTEX);
            Assert.AreEqual(10000, edgeData.Weight);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            edge = graph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 3);
            Assert.IsNull(edge);
        }

        /// <summary>
        /// Tests an uncontracted edge that already has a witness path.
        /// </summary>
        [Test]
        public void TestUncontractedWitnessed()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
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
    }
}