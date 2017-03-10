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
using Itinero.Graphs.Directed;
using System.Linq;

namespace Itinero.Test.Graphs.Directed
{
    /// <summary>
    /// Tests the directed graph implementation.
    /// </summary>
    [TestFixture]
    public class DirectedMetaGraphTests
    {
        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestAddEdge()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add edge.
            graph.AddEdge(0, 1, 10, 100);

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            // add another edge.
            graph.AddEdge(1, 2, 20, 200);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(20, edges.First().Data[0]);
            Assert.AreEqual(200, edges.First().MetaData[0]);
            Assert.AreEqual(2, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            // add another edge.
            graph.AddEdge(1, 3, 30, 300);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(30, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(0, edges.Count());

            // add another edge but in reverse.
            graph.AddEdge(3, 1, 30, 300);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(30, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            // add another edge and start a new island.
            graph.AddEdge(4, 5, 40, 400);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(30, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(5);
            Assert.AreEqual(0, edges.Count());

            // connect the islands.
            graph.AddEdge(5, 3, 50, 500);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(30, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(5);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(50, edges.First().Data[0]);
            Assert.AreEqual(500, edges.First().MetaData[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            graph.AddEdge(1, 6, 60, 600);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(30, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 6));
            Assert.AreEqual(60, edges.First(x => x.Neighbour == 6).Data[0]);
            Assert.AreEqual(600, edges.First(x => x.Neighbour == 6).MetaData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(5);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(50, edges.First().Data[0]);
            Assert.AreEqual(500, edges.First().MetaData[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            graph.AddEdge(1, 7, 70, 700);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(20, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(30, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 6));
            Assert.AreEqual(60, edges.First(x => x.Neighbour == 6).Data[0]);
            Assert.AreEqual(600, edges.First(x => x.Neighbour == 6).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 7));
            Assert.AreEqual(70, edges.First(x => x.Neighbour == 7).Data[0]);
            Assert.AreEqual(700, edges.First(x => x.Neighbour == 7).MetaData[0]);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(5);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(50, edges.First().Data[0]);
            Assert.AreEqual(500, edges.First().MetaData[0]);
            Assert.AreEqual(3, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests get edge enumerator.
        /// </summary>
        [Test]
        public void TestGetEdgeEnumerator()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add edges.
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(1, 2, 2, 200);
            graph.AddEdge(1, 3, 3, 300);
            graph.AddEdge(3, 4, 4, 400);
            graph.AddEdge(4, 1, 5, 500);
            graph.AddEdge(5, 1, 6, 600);

            // get empty edge enumerator.
            var edges = graph.GetEdgeEnumerator();

            // move to vertices and test result.
            Assert.IsTrue(edges.MoveTo(0));
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            Assert.IsTrue(edges.MoveTo(1));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == 2).Data[0]);
            Assert.AreEqual(200, edges.First(x => x.Neighbour == 2).MetaData[0]);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == 3).Data[0]);
            Assert.AreEqual(300, edges.First(x => x.Neighbour == 3).MetaData[0]);

            Assert.IsFalse(edges.MoveTo(2));

            Assert.IsTrue(edges.MoveTo(3));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 4));
            Assert.AreEqual(4, edges.First(x => x.Neighbour == 4).Data[0]);
            Assert.AreEqual(400, edges.First(x => x.Neighbour == 4).MetaData[0]);

            Assert.IsTrue(edges.MoveTo(4));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 1));
            Assert.AreEqual(5, edges.First(x => x.Neighbour == 1).Data[0]);
            Assert.AreEqual(500, edges.First(x => x.Neighbour == 1).MetaData[0]);

            Assert.IsTrue(edges.MoveTo(5));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 1));
            Assert.AreEqual(6, edges.First(x => x.Neighbour == 1).Data[0]);
            Assert.AreEqual(600, edges.First(x => x.Neighbour == 1).MetaData[0]);

            graph = new DirectedMetaGraph(1, 1, 10);

            // add edge.
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(0, 11001, 1, 100);
            graph.AddEdge(0, 11001, 2, 200);

            edges = graph.GetEdgeEnumerator();

            // move to vertices and test result.
            Assert.IsTrue(edges.MoveTo(0));
            Assert.AreEqual(3, edges.Count());
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdge()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add and remove edge.
            graph.AddEdge(0, 1, 10, 100);
            Assert.AreEqual(0, graph.RemoveEdge(1, 0));
            Assert.AreEqual(1, graph.RemoveEdge(0, 1));

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and remove edge.
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(0, 1, 20, 200);
            Assert.AreEqual(2, graph.GetEdgeEnumerator(0).Count);
            Assert.AreEqual(0, graph.RemoveEdge(1, 0));
            Assert.AreEqual(2, graph.RemoveEdge(0, 1));
            Assert.AreEqual(0, graph.GetEdgeEnumerator(0).Count);

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and remove edge.
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(0, 1, 20, 200);
            Assert.AreEqual(2, graph.GetEdgeEnumerator(0).Count);
            Assert.AreEqual(0, graph.RemoveEdge(1, 0));
            Assert.AreEqual(2, graph.RemoveEdge(0, 1));
            Assert.AreEqual(0, graph.GetEdgeEnumerator(0).Count);

            graph = new DirectedMetaGraph(1, 1, 10);
            
            // add and remove edge.
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(0, 2, 20, 200);
            graph.RemoveEdge(0, 1);

            var edges = graph.GetEdgeEnumerator(0);
            Assert.IsNotNull(edges);
            Assert.AreEqual(2, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdges()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add and remove edge.
            graph.AddEdge(0, 1, 1, 100);
            Assert.AreEqual(0, graph.RemoveEdges(1));
            Assert.AreEqual(1, graph.RemoveEdges(0));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and remove edges.
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(0, 2, 1, 100);
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and remove edges.
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(0, 2, 2, 200);
            graph.AddEdge(1, 2, 3, 300);
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and compress.
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(1, 2, 20, 200);
            graph.AddEdge(2, 3, 30, 300);
            graph.AddEdge(3, 4, 40, 400);
            Assert.AreEqual(1, graph.RemoveEdge(1, 2));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(4, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests the vertex count.
        /// </summary>
        [Test]
        public void TestVertexCountAndTrim()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add edge.
            graph.AddEdge(0, 1, 1, 100);

            // trim.
            graph.Trim();
            Assert.AreEqual(2, graph.VertexCount);

            graph = new DirectedMetaGraph(1, 1, 10);

            // add edge.
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(0, 11001, 1, 100);

            // trim.
            graph.Trim();
            Assert.AreEqual(11002, graph.VertexCount);

            graph = new DirectedMetaGraph(1, 1, 10);

            // trim.
            graph.Trim(); // keep minimum one vertex.
            Assert.AreEqual(1, graph.VertexCount);
        }

        /// <summary>
        /// Tests the edge count.
        /// </summary>
        [Test]
        public void TestEdgeCount()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add edge.
            graph.AddEdge(0, 1, 1, 100);
            Assert.AreEqual(1, graph.EdgeCount);

            graph = new DirectedMetaGraph(1, 1, 10);

            // add edge.
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(0, 11001, 1, 100);
            Assert.AreEqual(2, graph.EdgeCount);

            graph.AddEdge(0, 11001, 2, 200);
            Assert.AreEqual(3, graph.EdgeCount);

            graph.RemoveEdge(0, 11001);
            Assert.AreEqual(1, graph.EdgeCount);

            graph.RemoveEdge(0, 1);
            Assert.AreEqual(0, graph.EdgeCount);
        }

        /// <summary>
        /// Tests the compression.
        /// </summary>
        [Test]
        public void TestCompress()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add and compress.
            graph.AddEdge(0, 1, 1, 100);
            graph.Compress();

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and compress.
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(1, 2, 20, 200);
            graph.AddEdge(2, 3, 30, 300);
            graph.AddEdge(3, 4, 40, 400);
            graph.RemoveEdge(1, 2);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(4, edges.First().Neighbour);

            // compress.
            graph.Compress();

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(10, edges.First().Data[0]);
            Assert.AreEqual(100, edges.First().MetaData[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(30, edges.First().Data[0]);
            Assert.AreEqual(300, edges.First().MetaData[0]);
            Assert.AreEqual(3, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(40, edges.First().Data[0]);
            Assert.AreEqual(400, edges.First().MetaData[0]);
            Assert.AreEqual(4, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var graph = new DirectedMetaGraph(1, 1, 10);

            // add and compress.
            graph.AddEdge(0, 1, 1, 100);
            graph.Compress();
            var expectedSize =  1 + 1 + 8 + 8 + 4 + 4 + // the header: two longs representing vertex and edge count and one int for edge size and one for vertex size.
                graph.VertexCount * 2 * 4 + // the bytes for the vertex-index: 2 uint's.
                graph.EdgeCount * 2 * 4 + // the bytes for the edges: one edge 1 uint.
                4 + 4 + // the header for the meta-data: 2 uint for sizes.
                graph.EdgeCount * 4; // the bytes for the edges: one edge 1 uint.
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

            graph = new DirectedMetaGraph(1, 1, 10);

            // add and compress.
            graph.AddEdge(0, 1, 10, 100);
            graph.AddEdge(1, 2, 20, 200);
            graph.AddEdge(2, 3, 30, 300);
            graph.AddEdge(3, 4, 40, 400);
            graph.RemoveEdge(1, 2);
            graph.Compress();
            expectedSize = 1 + 1 + 8 + 8 + 4 + 4 + // the header: two longs representing vertex and edge count and one int for edge size and one for vertex size.
                graph.VertexCount * 2 * 4 + // the bytes for the vertex-index: 2 uint's.
                graph.EdgeCount * 2 * 4 + // the bytes for the edges: one edge 1 uint.
                4 + 4 + // the header for the meta-data: 2 uint for sizes.
                graph.EdgeCount * 4; // the bytes for the edges: one edge 1 uint.
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
            var graph = new DirectedMetaGraph(1, 1, 10);
            graph.AddEdge(0, 1, 1, 100);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = DirectedMetaGraph.Deserialize(stream, DirectedMetaGraphProfile.Aggressive40);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(2, deserializedGraph.VertexCount);
                Assert.AreEqual(1, deserializedGraph.EdgeCount);

                // verify all edges.
                var edges = deserializedGraph.GetEdgeEnumerator(0);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(1, edges.First().Data[0]);
                Assert.AreEqual(1, edges.First().Neighbour);

                edges = deserializedGraph.GetEdgeEnumerator(1);
                Assert.AreEqual(0, edges.Count());
            }

            graph = new DirectedMetaGraph(1, 1, 10);
            graph.AddEdge(0, 1, 1, 100);
            graph.AddEdge(0, 2, 2, 200);
            graph.AddEdge(0, 3, 3, 300);
            graph.AddEdge(0, 4, 4, 400);
            graph.AddEdge(5, 1, 5, 500);
            graph.AddEdge(5, 2, 6, 600);
            graph.AddEdge(5, 3, 7, 700);
            graph.AddEdge(5, 4, 8, 800);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                var size = graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = DirectedMetaGraph.Deserialize(stream, DirectedMetaGraphProfile.Aggressive40);
                Assert.AreEqual(size, stream.Position);

                Assert.AreEqual(6, deserializedGraph.VertexCount);
                Assert.AreEqual(8, deserializedGraph.EdgeCount);
            }
        }
    }
}