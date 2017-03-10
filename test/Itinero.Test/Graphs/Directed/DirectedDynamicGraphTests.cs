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
using Itinero.Data.Contracted.Edges;
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
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
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
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
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
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
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
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
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
        /// Tests removing an edge.
        /// </summary>
        [Test]
        public void TestRemoveEdgeById()
        {
            var graph = new DirectedDynamicGraph();

            var id = graph.AddEdge(1, 2, 0102, 010200);
            graph.AddEdge(1, 3, 0103);

            Assert.AreEqual(1, graph.RemoveEdgeById(1, id));

            Assert.AreEqual(1, graph.EdgeCount);
            var enumerator = graph.GetEdgeEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Neighbour);
            Assert.AreEqual(0103, enumerator.Data0);

            graph = new DirectedDynamicGraph();
            graph.AddEdge(1, 2, 0102, 010200);
            id = graph.AddEdge(1, 3, 0103);
            graph.AddEdge(1, 4, 0104, 010400, 01040000);

            Assert.AreEqual(1, graph.RemoveEdgeById(1, id));
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

            edges = graph.GetEdgeEnumerator(1);
            Assert.IsFalse(edges.MoveNext());

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

            edges = graph.GetEdgeEnumerator(1);
            Assert.IsFalse(edges.MoveNext());

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
                graph.VertexCount * 4 + // the bytes for the vertex-index: 2 uint's.
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

                edges = deserializedGraph.GetEdgeEnumerator(1);
                Assert.IsFalse(edges.MoveNext());
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

        /// <summary>
        /// Tests adding a simple network.
        /// </summary>
        [Test]
        public void TestAddSmallNetwork1()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(5, 1);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 101, null);
            graph.AddEdge(1, 2, 102, null);
            graph.AddEdge(2, 1, 103, null);
            graph.AddEdge(1, 3, 104, null);
            graph.AddEdge(3, 1, 105, null);
            graph.AddEdge(1, 4, 106, null);
            graph.AddEdge(4, 1, 107, null);
            graph.AddEdge(2, 3, 108, null);
            graph.AddEdge(3, 2, 109, null);

            Assert.AreEqual(5, graph.VertexCount);
            
            // verify all edges for 0.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            var edge = edges.First(e => e.Neighbour == 0);
            Assert.AreEqual(0, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(101, edge.Weight());
            edge = edges.First(e => e.Neighbour == 0);
            Assert.AreEqual(0, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(101, edge.Weight());
            edge = edges.First(e => e.Neighbour == 2);
            Assert.AreEqual(2, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(102, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(104, edge.Weight());
            edge = edges.First(e => e.Neighbour == 4);
            Assert.AreEqual(4, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(106, edge.Weight());

            // verify all edges for 2.
            edges = graph.GetEdgeEnumerator(2);
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(103, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(108, edge.Weight());

            // verify all edges for 3.
            edges = graph.GetEdgeEnumerator(3);
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(105, edge.Weight());
            edge = edges.First(e => e.Neighbour == 2);
            Assert.AreEqual(2, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(109, edge.Weight());

            // verify all edges for 4.
            edges = graph.GetEdgeEnumerator(4);
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(107, edge.Weight());
        }

        /// <summary>
        /// Tests adding a simple network and simulates what happens during contraction.
        /// </summary>
        [Test]
        public void TestAddSmallNetwork1AndContract()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(5, 1);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 0, 101, null);
            graph.AddEdge(1, 2, 102, null);
            graph.AddEdge(2, 1, 103, null);
            graph.AddEdge(1, 3, 104, null);
            graph.AddEdge(3, 1, 105, null);
            graph.AddEdge(1, 4, 106, null);
            graph.AddEdge(4, 1, 107, null);
            graph.AddEdge(2, 3, 108, null);
            graph.AddEdge(3, 2, 109, null);

            Assert.AreEqual(5, graph.VertexCount);

            // verify all edges for 0.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            var edge = edges.First(e => e.Neighbour == 0);
            Assert.AreEqual(0, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(101, edge.Weight());
            edge = edges.First(e => e.Neighbour == 0);
            Assert.AreEqual(0, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(101, edge.Weight());
            edge = edges.First(e => e.Neighbour == 2);
            Assert.AreEqual(2, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(102, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(104, edge.Weight());
            edge = edges.First(e => e.Neighbour == 4);
            Assert.AreEqual(4, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(106, edge.Weight());

            // verify all edges for 2.
            edges = graph.GetEdgeEnumerator(2);
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(103, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(108, edge.Weight());

            // verify all edges for 3.
            edges = graph.GetEdgeEnumerator(3);
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(105, edge.Weight());
            edge = edges.First(e => e.Neighbour == 2);
            Assert.AreEqual(2, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(109, edge.Weight());

            // verify all edges for 4.
            edges = graph.GetEdgeEnumerator(4);
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(107, edge.Weight());

            // remove 3->2 and 1->2.
            graph.RemoveEdge(3, 2);
            graph.RemoveEdge(1, 2);

            // verify all edges for 0.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(3, edges.Count());
            edge = edges.First(e => e.Neighbour == 0);
            Assert.AreEqual(0, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(101, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(104, edge.Weight());
            edge = edges.First(e => e.Neighbour == 4);
            Assert.AreEqual(4, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(106, edge.Weight());

            // verify all edges for 2.
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(103, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(108, edge.Weight());

            // verify all edges for 3.
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(105, edge.Weight());

            // verify all edges for 4.
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(107, edge.Weight());

            // remove 1->0.
            graph.RemoveEdge(1, 0);

            // verify all edges for 0.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(104, edge.Weight());
            edge = edges.First(e => e.Neighbour == 4);
            Assert.AreEqual(4, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(106, edge.Weight());

            // verify all edges for 2.
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(103, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(108, edge.Weight());

            // verify all edges for 3.
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(105, edge.Weight());

            // verify all edges for 4.
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(107, edge.Weight());

            // remove 1->4.
            graph.RemoveEdge(1, 4);

            // verify all edges for 0.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(104, edge.Weight());

            // verify all edges for 2.
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(103, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(108, edge.Weight());

            // verify all edges for 3.
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(105, edge.Weight());

            // verify all edges for 4.
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(107, edge.Weight());

            // remove 1->3.
            graph.RemoveEdge(1, 3);

            // verify all edges for 0.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            // verify all edges for 2.
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(103, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(108, edge.Weight());

            // verify all edges for 3.
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(105, edge.Weight());

            // verify all edges for 4.
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(107, edge.Weight());
        }

        /// <summary>
        /// Tests building an original path.
        /// </summary>
        [Test]
        public void TestBuildPath()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(5, 1);
            var edge01 = graph.AddEdge(0, 1, 100, null);
            //graph.AddEdge(1, 0, 101, null);
            //graph.AddEdge(1, 2, 102, null);
            var edge21 = graph.AddEdge(2, 1, 103, null);
            //graph.AddEdge(1, 3, 104, null);
            var edge31 = graph.AddEdge(3, 1, 105, null);
            //graph.AddEdge(1, 4, 106, null);
            var edge41 = graph.AddEdge(4, 1, 107, null);
            var edge23 = graph.AddEdge(2, 3, 108, null);
            //graph.AddEdge(3, 2, 109, null);

            var path = graph.BuildPath(new uint[] { 1 });
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);

            path = graph.BuildPath(new uint[] { 1, 2 });
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Vertex);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(103, path.Weight);
            Assert.AreEqual(-graph.GetOriginal(2, 1).IdDirected(), path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);

            path = graph.BuildPath(new uint[] { 2, 1 }, true);
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Vertex);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(103, path.Weight);
            Assert.AreEqual(-graph.GetOriginal(2, 1).IdDirected(), path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);

            path = graph.BuildPath(new uint[] { 2, 1 });
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(103, path.Weight);
            Assert.AreEqual(graph.GetOriginal(2, 1).IdDirected(), path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);

            path = graph.BuildPath(new uint[] { 1, 2 }, true);
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(103, path.Weight);
            Assert.AreEqual(graph.GetOriginal(2, 1).IdDirected(), path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);
            
            path = graph.BuildPath(new uint[] { 1, 2 }, true, true);
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.IsNotNull(path.From);
            Assert.AreEqual(103, path.Weight);
            Assert.AreEqual(graph.GetOriginal(2, 1).IdDirected(), path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Vertex);
            Assert.IsNull(path.From);
            Assert.AreEqual(0, path.Weight);
        }
    }
}