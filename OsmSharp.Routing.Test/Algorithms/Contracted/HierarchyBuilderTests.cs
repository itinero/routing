// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using OsmSharp.Routing.Algorithms.Contracted;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System.Linq;

namespace OsmSharp.Routing.Test.Algorithms.Contracted
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.Compress(false);

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)), 
                new DykstraWitnessCalculator(int.MaxValue));
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.Compress(false);

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)),
                new DykstraWitnessCalculator(int.MaxValue));
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(0, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.Compress(false);

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)),
                new DykstraWitnessCalculator(int.MaxValue));
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(3, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(3, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(4, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(4, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(0, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.Compress(false);

            // contract graph.
            var hierarchyBuilder = new HierarchyBuilder(graph, new EdgeDifferencePriorityCalculator(graph, new DykstraWitnessCalculator(int.MaxValue)),
                new DykstraWitnessCalculator(int.MaxValue));
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
    }
}