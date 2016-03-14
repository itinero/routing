// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Graphs.Directed
{
    /// <summary>
    /// Contains tests for the directed dynamic graph.
    /// </summary>
    [TestFixture]
    public class DirectedDynamicGraphTests
    {
        /// <summary>
        /// Tests adding one edge.
        /// </summary>
        [Test]
        public void TestAddEdge()
        {
            var graph = new DirectedDynamicGraph();
            graph.AddEdge(1, 2, 0102);

            Assert.AreEqual(1, graph.EdgeCount);
            var enumerator = graph.GetEdgeEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0102, enumerator.Data0);

            graph = new DirectedDynamicGraph();
            graph.AddEdge(1, 2, 0102, 010200);

            Assert.AreEqual(1, graph.EdgeCount);
            enumerator = graph.GetEdgeEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(0102, enumerator.Data0);
            Assert.AreEqual(010200, enumerator.DynamicData[0]);
        }

        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestAddMultipleEdges()
        {
            // a new graph.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(1, 0, 10, 100);
            graph.AddEdge(0, 2, 20, 200);
            graph.AddEdge(2, 0, 20, 200);

            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 1).Data0);
            Assert.AreEqual(100, edges.First(x => x.Neighbour == 1).DynamicData[0]);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).DynamicData[0]);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 0).Data[0]);
            Assert.AreEqual(100, edges.First(x => x.Neighbour == 0).DynamicData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 0).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 0).DynamicData[0]);

            // a new graph.
            graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(0, 2, 20, 200);
            graph.AddEdge(1, 0, 10, 100);
            graph.AddEdge(2, 0, 20, 200);

            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 1).Data0);
            Assert.AreEqual(100, edges.First(x => x.Neighbour == 1).DynamicData[0]);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).DynamicData[0]);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 0).Data[0]);
            Assert.AreEqual(100, edges.First(x => x.Neighbour == 0).DynamicData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 0).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 0).DynamicData[0]);
            
            // a new graph.
            graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 10);
            graph.AddEdge(1, 0, 10);
            graph.AddEdge(0, 2, 20);
            graph.AddEdge(2, 0, 20);

            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 1).Data0);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 0).Data[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 0).Data[0]);

            // a new graph.
            graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 10);
            graph.AddEdge(0, 2, 20);
            graph.AddEdge(1, 0, 10);
            graph.AddEdge(2, 0, 20);

            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 1).Data0);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(10, edges.First(x => x.Neighbour == 0).Data[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 0).Data[0]);
        }

        /// <summary>
        /// Tests removing an edge.
        /// </summary>
        [Test]
        public void TestRemoveEdge()
        {
            var graph = new DirectedDynamicGraph();

            graph.AddEdge(1, 2, 0102, 010200);
            graph.AddEdge(1, 3, 0103);

            Assert.AreEqual(1, graph.RemoveEdge(1, 2));

            Assert.AreEqual(1, graph.EdgeCount);
            var enumerator = graph.GetEdgeEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Neighbour);
            Assert.AreEqual(0103, enumerator.Data0);

            graph = new DirectedDynamicGraph();
            graph.AddEdge(1, 2, 0102, 010200);
            graph.AddEdge(1, 3, 0103);
            graph.AddEdge(1, 4, 0104, 010400, 01040000);

            Assert.AreEqual(1, graph.RemoveEdge(1, 3));
            enumerator = graph.GetEdgeEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator.Neighbour);
            Assert.AreEqual(0102, enumerator.Data0);
            Assert.AreEqual(010200, enumerator.DynamicData[0]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Neighbour);
            Assert.AreEqual(0104, enumerator.Data0);
            Assert.AreEqual(010400, enumerator.DynamicData[0]);
            Assert.AreEqual(01040000, enumerator.DynamicData[1]);
        }
        /// <summary>
        /// Tests the vertex count.
        /// </summary>
        [Test]
        public void TestVertexCountAndTrim()
        {
            var graph = new DirectedDynamicGraph(10, 1);

            // add edge.
            graph.AddEdge(0, 1, 1);

            // trim.
            graph.Trim();
            Assert.AreEqual(2, graph.VertexCount);

            graph = new DirectedDynamicGraph(10, 1);

            // add edge.
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 11001, 1);

            // trim.
            graph.Trim();
            Assert.AreEqual(11002, graph.VertexCount);

            graph = new DirectedDynamicGraph(10, 1);

            // trim.
            graph.Trim(); // keep minimum one vertex.
            Assert.AreEqual(1, graph.VertexCount);
        }

        /// <summary>
        /// Tests the compression.
        /// </summary>
        [Test]
        public void TestCompress()
        {
            var graph = new DirectedDynamicGraph(10, 1);

            // add and compress.
            graph.AddEdge(0, 1, 1);
            graph.Compress();

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            graph = new DirectedDynamicGraph(10, 1);

            // add and compress.
            graph.AddEdge(0, 1, 10);
            graph.AddEdge(1, 2, 20);
            graph.AddEdge(2, 3, 30);
            graph.AddEdge(3, 4, 40);
            graph.RemoveEdge(1, 2);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator();
            Assert.IsFalse(edges.MoveTo(1));

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(4, edges.First().Neighbour);

            // compress.
            graph.Compress();

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator();
            Assert.IsFalse(edges.MoveTo(1));

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(4, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var graph = new DirectedDynamicGraph(10, 1);

            // add and compress.
            graph.AddEdge(0, 1, 1);
            graph.Compress();
            var expectedSize = 1 + 8 + 8 + 8+ 4 + // the header: version byte two longs representing vertex, edge count and the size of the edge array and one int for minimum edge size.
                graph.VertexCount * 1 * 4 + // the bytes for the vertex-index: 1 uint.
                graph.EdgeCount * 2 * 4; // the bytes for the edges: one edge 2 uint's.
            using (var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
                Assert.AreEqual(expectedSize, stream.Position);
            }

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            graph = new DirectedDynamicGraph(10, 1);

            // add and compress.
            graph.AddEdge(0, 1, 10);
            graph.AddEdge(1, 2, 20);
            graph.AddEdge(2, 3, 30);
            graph.AddEdge(3, 4, 40);
            graph.RemoveEdge(1, 2);
            graph.Compress();
            expectedSize = 1 + 8 + 8 + 8 + 4 + // the header: version bytes, three longs representing vertex and edge count and the size of the edge array and one int for fixed edge size.
                graph.VertexCount * 1 * 4 + // the bytes for the vertex-index: 2 uint's.
                graph.EdgeCount * 2 * 4; // the bytes for the edges: one edge 1 uint.
            using (var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
                Assert.AreEqual(expectedSize, stream.Position);
            }
        }

        /// <summary>
        /// Tests the deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var graph = new DirectedDynamicGraph(10, 1);
            graph.AddEdge(0, 1, 1);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = DirectedDynamicGraph.Deserialize(stream, DirectedGraphProfile.Aggressive24);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(2, deserializedGraph.VertexCount);
                Assert.AreEqual(1, deserializedGraph.EdgeCount);

                // verify all edges.
                var edges = deserializedGraph.GetEdgeEnumerator(0);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(1, edges.First().Data[0]);
                Assert.AreEqual(1, edges.First().Neighbour);

                edges = deserializedGraph.GetEdgeEnumerator();
                Assert.IsFalse(edges.MoveTo(1));
            }

            graph = new DirectedDynamicGraph(10, 1);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 2, 2);
            graph.AddEdge(0, 3, 3);
            graph.AddEdge(0, 4, 4);
            graph.AddEdge(5, 1, 5);
            graph.AddEdge(5, 2, 6);
            graph.AddEdge(5, 3, 7);
            graph.AddEdge(5, 4, 8);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = DirectedDynamicGraph.Deserialize(stream, DirectedGraphProfile.Aggressive24);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(6, deserializedGraph.VertexCount);
                Assert.AreEqual(8, deserializedGraph.EdgeCount);
            }
        }
    }
}