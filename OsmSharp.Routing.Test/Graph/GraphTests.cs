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
using OsmSharp.Routing.Graph;
using System;
using System.Linq;

namespace OsmSharp.Routing.Test.Graph
{
    /// <summary>
    /// Tests the graph implementation.
    /// </summary>
    [TestFixture]
    public class GraphTests
    {
        /// <summary>
        /// Tests argument exceptions.
        /// </summary>
        [Test]
        public void TestArgumentExeceptions()
        {
            // create graph with one vertex and start adding vertex2.
            var graph = new Graph<EdgeDataMock>(2);
            uint vertex0 = 0;
            uint vertex1 = 1;
            uint vertex2 = 2;

            // make sure to add vertex1 and vertex2.
            graph.AddEdge(vertex0, vertex1, new EdgeDataMock(1));

            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.GetEdgeEnumerator(vertex2);
            });
        }

        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestAddEdge()
        {
            var graph = new Graph<EdgeDataMock>();
            uint vertex0 = 0;
            uint vertex1 = 1;

            // add edge.
            graph.AddEdge(vertex0, vertex1, new EdgeDataMock(1));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex0, edges.First().Neighbour);

            // add another edge.
            uint vertex2 = 2;
            graph.AddEdge(vertex1, vertex2, new EdgeDataMock(2));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex0));
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(-1, edges.First(x => x.Neighbour == vertex0).EdgeData.Id);
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-2, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            // add another edge.
            uint vertex3 = 3;
            graph.AddEdge(vertex1, vertex3, new EdgeDataMock(3));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex0));
            Assert.AreEqual(-1, edges.First(x => x.Neighbour == vertex0).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-2, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-3, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            // overwrite another edge but in reverse.
            graph.AddEdge(vertex3, vertex1, new EdgeDataMock(3));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex0));
            Assert.AreEqual(-1, edges.First(x => x.Neighbour == vertex0).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(-3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-2, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            // add another edge and start a new island.
            uint vertex4 = 4;
            uint vertex5 = 5;
            graph.AddEdge(vertex4, vertex5, new EdgeDataMock(4));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex0));
            Assert.AreEqual(-1, edges.First(x => x.Neighbour == vertex0).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(-3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-2, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(4, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-4, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex4, edges.First().Neighbour);

            // connect the islands.
            graph.AddEdge(vertex5, vertex3, new EdgeDataMock(5));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex0));
            Assert.AreEqual(-1, edges.First(x => x.Neighbour == vertex0).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(-3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-2, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex1));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == vertex1).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex5));
            Assert.AreEqual(-5, edges.First(x => x.Neighbour == vertex5).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(4, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex4));
            Assert.AreEqual(-4, edges.First(x => x.Neighbour == vertex4).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(5, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);
        }

        /// <summary>
        /// Tests get edge enumerator.
        /// </summary>
        [Test]
        public void TestGetEdgeEnumerator()
        {
            var graph = new Graph<EdgeDataMock>();

            // add edges.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(1, 2, new EdgeDataMock(2));
            graph.AddEdge(1, 3, new EdgeDataMock(3));
            graph.AddEdge(3, 4, new EdgeDataMock(4));
            graph.AddEdge(4, 1, new EdgeDataMock(5));
            graph.AddEdge(5, 1, new EdgeDataMock(6));

            // get empty edge enumerator.
            var edges = graph.GetEdgeEnumerator();
            Assert.IsFalse(edges.HasData);

            // move to vertices and test result.
            Assert.IsTrue(edges.MoveTo(0));
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(1, edges.First().Neighbour);

            Assert.IsTrue(edges.MoveTo(1));
            Assert.AreEqual(5, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 0));
            Assert.AreEqual(-1, edges.First(x => x.Neighbour == 0).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == 2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == 3).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 4));
            Assert.AreEqual(-5, edges.First(x => x.Neighbour == 4).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 5));
            Assert.AreEqual(-6, edges.First(x => x.Neighbour == 5).EdgeData.Id);

            Assert.IsTrue(edges.MoveTo(2));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 1));
            Assert.AreEqual(-2, edges.First(x => x.Neighbour == 1).EdgeData.Id);

            Assert.IsTrue(edges.MoveTo(3));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 1));
            Assert.AreEqual(-3, edges.First(x => x.Neighbour == 1).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 4));
            Assert.AreEqual(4, edges.First(x => x.Neighbour == 4).EdgeData.Id);

            Assert.IsTrue(edges.MoveTo(4));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(-4, edges.First(x => x.Neighbour == 3).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 1));
            Assert.AreEqual(5, edges.First(x => x.Neighbour == 1).EdgeData.Id);

            Assert.IsTrue(edges.MoveTo(5));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 1));
            Assert.AreEqual(6, edges.First(x => x.Neighbour == 1).EdgeData.Id);
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdge()
        {
            var graph = new Graph<EdgeDataMock>();

            // add and remove edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            Assert.IsTrue(graph.RemoveEdge(0, 1));
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdges()
        {
            var graph = new Graph<EdgeDataMock>();

            // add and remove edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            Assert.AreEqual(1, graph.RemoveEdges(0));
            Assert.AreEqual(0, graph.RemoveEdges(1));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            graph = new Graph<EdgeDataMock>();

            // add and remove edges.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 2, new EdgeDataMock(1));
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            graph = new Graph<EdgeDataMock>();

            // add and remove edges.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 2, new EdgeDataMock(2));
            graph.AddEdge(1, 2, new EdgeDataMock(3));
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
        }

        /// <summary>
        /// Tests the vertex count.
        /// </summary>
        [Test]
        public void TestVertexCountAndTrim()
        {
            var graph = new Graph<EdgeDataMock>();

            // add edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));

            // trim.
            graph.Trim();
            Assert.AreEqual(2, graph.VertexCount);

            graph = new Graph<EdgeDataMock>();

            // add edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 11001, new EdgeDataMock(1));

            // trim.
            graph.Trim();
            Assert.AreEqual(11002, graph.VertexCount);

            graph = new Graph<EdgeDataMock>();

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
            var graph = new Graph<EdgeDataMock>();

            // add edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            Assert.AreEqual(1, graph.EdgeCount);

            graph = new Graph<EdgeDataMock>();

            // add edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 11001, new EdgeDataMock(1));
            Assert.AreEqual(2, graph.EdgeCount);

            graph.AddEdge(0, 11001, new EdgeDataMock(2));
            Assert.AreEqual(2, graph.EdgeCount);

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
            var graph = new Graph<EdgeDataMock>();

            // add and compress.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.Compress();

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-1, edges.First().EdgeData.Id);
            Assert.AreEqual(0, edges.First().Neighbour);

            graph = new Graph<EdgeDataMock>();

            // add and compress.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(1, 2, new EdgeDataMock(2));
            graph.AddEdge(2, 3, new EdgeDataMock(3));
            graph.AddEdge(3, 4, new EdgeDataMock(4));
            graph.RemoveEdge(1, 2);
            graph.Compress();

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-1, edges.First().EdgeData.Id);
            Assert.AreEqual(0, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().EdgeData.Id);
            Assert.AreEqual(3, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(-3, edges.First(x => x.Neighbour == 2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 4));
            Assert.AreEqual(4, edges.First(x => x.Neighbour == 4).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(-4, edges.First().EdgeData.Id);
            Assert.AreEqual(3, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests the serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var graph = new Graph<EdgeDataMock>();

            // add one edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));

            // serialize.
            graph.Compress();
            var expectedSize = 8 + 8 + // the header: two longs representing vertex and edge count.
                graph.VertexCount * 4 + // the bytes for the vertex-index: 2 vertices, pointing to 0.
                graph.EdgeCount * 4 * 4 + // the bytes for the one edge: one edge = 4 uints.
                graph.EdgeCount * EdgeDataMock.SizeUInts * 4; // the bytes for the one edge-data: one edge = one edge data object.
            using(var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
            }
            
            graph = new Graph<EdgeDataMock>();

            // add one edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 2, new EdgeDataMock(2));
            graph.AddEdge(0, 3, new EdgeDataMock(3));
            graph.AddEdge(0, 4, new EdgeDataMock(4));
            graph.AddEdge(5, 1, new EdgeDataMock(5));
            graph.AddEdge(5, 2, new EdgeDataMock(6));
            graph.AddEdge(5, 3, new EdgeDataMock(7));
            graph.AddEdge(5, 4, new EdgeDataMock(8));

            // serialize.
            graph.Compress();
            expectedSize = 8 + 8 + // the header: two longs representing vertex and edge count.
                graph.VertexCount * 4 + // the bytes for the vertex-index: 2 vertices, pointing to 0.
                graph.EdgeCount * 4 * 4 + // the bytes for the one edge: one edge = 4 uints.
                graph.EdgeCount * EdgeDataMock.SizeUInts * 4; // the bytes for the one edge-data: one edge = one edge data object.
            using (var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
            }
        }

        /// <summary>
        /// Tests the deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var graph = new Graph<EdgeDataMock>();

            // add one edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = Graph<EdgeDataMock>.Deserialize(stream, 
                    EdgeDataMock.SizeUInts, EdgeDataMock.MapFromDelegate, EdgeDataMock.MapToDelegate, false);

                Assert.AreEqual(2, deserializedGraph.VertexCount);
                Assert.AreEqual(1, deserializedGraph.EdgeCount);

                // verify all edges.
                var edges = deserializedGraph.GetEdgeEnumerator(0);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(1, edges.First().EdgeData.Id);
                Assert.AreEqual(1, edges.First().Neighbour);

                edges = deserializedGraph.GetEdgeEnumerator(1);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(-1, edges.First().EdgeData.Id);
                Assert.AreEqual(0, edges.First().Neighbour);
            }

            graph = new Graph<EdgeDataMock>();

            // add one edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 2, new EdgeDataMock(2));
            graph.AddEdge(0, 3, new EdgeDataMock(3));
            graph.AddEdge(0, 4, new EdgeDataMock(4));
            graph.AddEdge(5, 1, new EdgeDataMock(5));
            graph.AddEdge(5, 2, new EdgeDataMock(6));
            graph.AddEdge(5, 3, new EdgeDataMock(7));
            graph.AddEdge(5, 4, new EdgeDataMock(8));

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = Graph<EdgeDataMock>.Deserialize(stream,
                    EdgeDataMock.SizeUInts, EdgeDataMock.MapFromDelegate, EdgeDataMock.MapToDelegate, false);

                Assert.AreEqual(6, deserializedGraph.VertexCount);
                Assert.AreEqual(8, deserializedGraph.EdgeCount);
            }
        }
    }
}