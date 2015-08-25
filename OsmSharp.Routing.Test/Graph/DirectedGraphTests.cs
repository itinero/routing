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
using OsmSharp.Collections.Tags.Index;
using OsmSharp.IO.MemoryMappedFiles;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;
using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing.Graph
{
    /// <summary>
    /// Tests a directed dynamic graph.
    /// </summary>
    [TestFixture]
    public class DirectedGraphTests
    {
        /// <summary>
        /// Returns a graph.
        /// </summary>
        /// <returns></returns>
        protected DirectedGraph<Edge> CreateGraph()
        {
            return new DirectedGraph<Edge>();
        }

        /// <summary>
        /// Tests argument out of range.
        /// </summary>
        [Test]
        public void TestArgumentExceptions()
        {
            // create graph with one vertex and start adding vertex2.
            var graph = this.CreateGraph();
            uint vertex1 = graph.AddVertex(0, 0);
            uint vertex2 = graph.AddVertex(0, 0);
            uint vertex3 = 3;

            Assert.Catch<ArgumentOutOfRangeException>(() => {
                graph.AddEdge(vertex3, vertex1, new Edge(), null);
            });
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.AddEdge(vertex1, vertex3, new Edge(), null);
            });
            Assert.Catch<ArgumentException>(() =>
            {
                graph.AddEdge(vertex1, vertex1, new Edge(), null);
            });
            Assert.Catch<ArgumentException>(() =>
            {
                graph.AddEdge(vertex1, vertex1, new Edge(), null);
            });
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.ContainsEdges(vertex3, vertex1);
            });
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.ContainsEdges(vertex1, vertex3);
            });
            Edge edge;
            Assert.Catch<InvalidOperationException>(() => // get edge is not allowed on graph that allows duplicates!
            {
                graph.GetEdge(vertex3, vertex1, out edge);
            });
            Assert.Catch<InvalidOperationException>(() => // get edge is not allowed on graph that allows duplicates!
            {
                graph.GetEdge(vertex1, vertex3, out edge);
            });
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.GetEdges(vertex3);
            });
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.SetVertex(vertex3, 10, 10);
            });
        }

        /// <summary>
        /// Tests adding a vertex.
        /// </summary>
        [Test]
        public void TestVertex()
        {
            var graph = this.CreateGraph();
            var vertex = graph.AddVertex(51, 4);

            float latitude, longitude;
            graph.GetVertex(vertex, out latitude, out longitude);
            Assert.AreEqual(51, latitude);
            Assert.AreEqual(4, longitude);
            graph.SetVertex(vertex, 52, 5);
            graph.GetVertex(vertex, out latitude, out longitude);
            Assert.AreEqual(52, latitude);
            Assert.AreEqual(5, longitude);

            var edges =  graph.GetEdges(vertex).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);

            Assert.IsFalse(graph.GetVertex(100, out latitude, out longitude));
        }

        /// <summary>
        /// Tests adding 10000 vertices.
        /// </summary>
        [Test]
        public void TestVertex10000()
        {
            var graph = this.CreateGraph();
            int count = 10000;
            while (count > 0)
            {
                var vertex = graph.AddVertex(51, 4);

                float latitude, longitude;
                graph.GetVertex(vertex, out latitude, out longitude);

                Assert.AreEqual(51, latitude);
                Assert.AreEqual(4, longitude);

                var edges =  graph.GetEdges(vertex).ToKeyValuePairs();
                Assert.AreEqual(0, edges.Length);

                count--;
            }

            Assert.AreEqual((uint)10000, graph.VertexCount);
        }

        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestEdge()
        {
            var graph = this.CreateGraph();
            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge(), null);

            var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(0, edges[0].Value.Tags);
            Assert.AreEqual(vertex2, edges[0].Key);

            edges = graph.GetEdges(vertex2).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);

            edges =  graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(0, edges[0].Value.Tags);
            Assert.AreEqual(true, edges[0].Value.Forward);
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));
        }

        /// <summary>
        /// Tests adding 10000 edges.
        /// </summary>
        [Test]
        public void TestEdge1001()
        {
            var count = 1001;
            var graph = this.CreateGraph();
            var vertex1 = graph.AddVertex(51, 1);
            while (count > 0)
            {
                var vertex2 = graph.AddVertex(51, 1);
                graph.AddEdge(vertex1, vertex2, new Edge(), null);

                var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
                Assert.AreEqual(1001 - count + 1, edges.Length);

                edges = graph.GetEdges(vertex2).ToKeyValuePairs();
                Assert.AreEqual(0, edges.Length);

                count--;
            }
        }

        /// <summary>
        /// Tests adding an edge and the reverse edge.
        /// </summary>
        [Test]
        public void TestEdge1()
        {
            uint tagsId = 10;
            var graph = this.CreateGraph();
            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = tagsId
            }, null);

            // test forward edge.
            var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(tagsId, edges[0].Value.Tags);
            Assert.AreEqual(vertex2, edges[0].Key);
            Assert.AreEqual(true, edges[0].Value.Forward);

            // test backward edge: backward edge NOT added automatically.
            edges = graph.GetEdges(vertex2).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);

            // add a third vertex.
            var vertex3 = graph.AddVertex(51, 2);
            var edge = new Edge()
            {
                Forward = true,
                Tags = tagsId
            };
            graph.AddEdge(vertex1, vertex3, edge, null);

            // test forward edges.
            edges = graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(2, edges.Length);
            Assert.AreEqual(tagsId, edges[0].Value.Tags);
            Assert.AreEqual(vertex2, edges[0].Key);
            Assert.AreEqual(true, edges[0].Value.Forward);
            Assert.AreEqual(tagsId, edges[1].Value.Tags);
            Assert.AreEqual(vertex3, edges[1].Key);
            Assert.AreEqual(true, edges[1].Value.Forward);

            // test backward edge: backward edge NOT added automatically.
            edges = graph.GetEdges(vertex3).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);
        }

        /// <summary>
        /// Tests adding and removing one edge.
        /// </summary>
        [Test]
        public void TestAddRemove1()
        {
            uint tagsId = 10;
            var graph = this.CreateGraph();
            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = tagsId
            }, null);

            // test forward edge.
            var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(tagsId, edges[0].Value.Tags);
            Assert.AreEqual(vertex2, edges[0].Key);
            Assert.AreEqual(true, edges[0].Value.Forward);

            // remove edge again.
            graph.RemoveEdge(vertex1, vertex2);

            // check if the edge is gone.
            edges = graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);
        }

        /// <summary>
        /// Tests adding and removing two edges.
        /// </summary>
        [Test]
        public void TestAddRemove2()
        {
            uint tagsId = 10;
            var graph = this.CreateGraph();
            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = tagsId
            }, null);

            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = tagsId
            }, null);

            // test edges.
            var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(tagsId, edges[0].Value.Tags);
            Assert.AreEqual(vertex2, edges[0].Key);
            Assert.AreEqual(true, edges[0].Value.Forward);
            edges = graph.GetEdges(vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            edges = graph.GetEdges(vertex3).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);

            // remove edge again.
            graph.RemoveEdge(vertex1, vertex2);

            // test edges.
            edges = graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);
            edges = graph.GetEdges(vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(tagsId, edges[0].Value.Tags);
            Assert.AreEqual(vertex3, edges[0].Key);
            Assert.AreEqual(true, edges[0].Value.Forward);
            edges = graph.GetEdges(vertex3).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);
        }

        /// <summary>
        /// Tests adding and remove an arbitrary number of edges.
        /// </summary>
        [Test]
        public void TestAddRemoveX()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);

            Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));

            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);

            Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));

            Assert.IsTrue(graph.ContainsEdges(vertex2, vertex3));
            Assert.IsFalse(graph.ContainsEdges(vertex3, vertex2));

            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));

            Assert.IsTrue(graph.ContainsEdges(vertex2, vertex3));
            Assert.IsFalse(graph.ContainsEdges(vertex3, vertex2));

            Assert.IsTrue(graph.ContainsEdges(vertex3, vertex4));
            Assert.IsFalse(graph.ContainsEdges(vertex4, vertex3));

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));

            Assert.IsTrue(graph.ContainsEdges(vertex2, vertex3));
            Assert.IsFalse(graph.ContainsEdges(vertex3, vertex2));

            Assert.IsTrue(graph.ContainsEdges(vertex3, vertex4));
            Assert.IsFalse(graph.ContainsEdges(vertex4, vertex3));

            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex4));
            Assert.IsTrue(graph.ContainsEdges(vertex4, vertex2));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);
        }

        /// <summary>
        /// Test removing an edge at the end.
        /// </summary>
        [Test]
        public void TestRemoveEnd()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex4, vertex2);
            Assert.IsFalse(graph.ContainsEdges(vertex4, vertex2));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 0);
        }

        /// <summary>
        /// Test removing an edge in the middle.
        /// </summary>
        [Test]
        public void TestRemoveMiddle()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex2, vertex3);
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 0);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);
        }

        /// <summary>
        /// Test removing an edge in the beginning.
        /// </summary>
        [Test]
        public void TestRemoveBegin()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex1, vertex2);
            Assert.IsFalse(graph.ContainsEdges(vertex1, vertex2));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 0);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);
        }

        /// <summary>
        /// Test removing an edge in the beginning.
        /// </summary>
        [Test]
        public void TestRemoveAll()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex1, vertex2);
            graph.RemoveEdge(vertex2, vertex3);
            graph.RemoveEdge(vertex3, vertex4);
            graph.RemoveEdge(vertex4, vertex2);
            Assert.IsFalse(graph.ContainsEdges(vertex1, vertex2));
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));
            Assert.IsFalse(graph.ContainsEdges(vertex3, vertex4));
            Assert.IsFalse(graph.ContainsEdges(vertex4, vertex2));
        }

        /// <summary>
        /// Tests removing all edges for one vertex.
        /// </summary>
        [Test]
        public void TestRemoveAllOneVertex()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            });
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            });
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            });

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            });

            graph.RemoveEdges(vertex2);
            Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));
            Assert.IsTrue(graph.ContainsEdges(vertex4, vertex2));
            Assert.IsTrue(graph.ContainsEdges(vertex3, vertex4));
        }

        /// <summary>
        /// Tests overwrite an edge with a reverse edge.
        /// </summary>
        [Test]
        public void TestAddReverse()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);

            graph.AddEdge(vertex2, vertex1, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);

            var edges =  graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(1, edges[0].Value.Tags);
            edges = graph.GetEdges(vertex2, vertex1).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(2, edges[0].Value.Tags);
        }

        /// <summary>
        /// Tests adding a duplicate or overwriting the existing edge.
        /// </summary>
        [Test]
        public void TestAddingDuplicateEdge()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);

            // should not overwrite but add duplicate.
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);

            var edges =  graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(2, edges.Length);
            Assert.IsTrue(edges.Any(x => x.Value.Tags == 1));
            Assert.IsTrue(edges.Any(x => x.Value.Tags == 2));

            // should not overwrite, even when identical data.
            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);

            edges = graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(3, edges.Length);
            Assert.IsTrue(edges.Any(x => x.Value.Tags == 1));
            Assert.IsTrue(edges.Any(x => x.Value.Tags == 2));
        }

        /// <summary>
        /// Tests adding an edge and compressing.
        /// </summary>
        [Test]
        public void TestDirectedGraphCompress1()
        {
            var graph = this.CreateGraph();
            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge(), null);

            graph.Compress(true);

            Assert.IsTrue(graph.IsReadonly);
            var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(0, edges[0].Value.Tags);
            Assert.AreEqual(vertex2, edges[0].Key);

            edges = graph.GetEdges(vertex2).ToKeyValuePairs();
            Assert.AreEqual(0, edges.Length);

            edges =  graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(0, edges[0].Value.Tags);
            Assert.AreEqual(true, edges[0].Value.Forward);
            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));
        }

        /// <summary>
        /// Tests adding data and compressing.
        /// </summary>
        [Test]
        public void TestDirectedGraphCompress2()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex2, vertex3);

            graph.Compress();

            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 0);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);

            graph = this.CreateGraph();

            vertex1 = graph.AddVertex(51, 1);
            vertex2 = graph.AddVertex(51, 2);
            vertex3 = graph.AddVertex(51, 3);
            vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex3, vertex4);

            graph.Compress();

            Assert.IsFalse(graph.ContainsEdges(vertex3, vertex4));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 0);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);

            var edges =  graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(1, edges[0].Value.Tags);
            edges = graph.GetEdges(vertex2, vertex3).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(2, edges[0].Value.Tags);
            edges = graph.GetEdges(vertex4, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(4, edges[0].Value.Tags);
        }

        /// <summary>
        /// Tests adding data and compressing.
        /// </summary>
        [Test]
        public void TestDirectedGraphCompress3()
        {
            var graph = this.CreateGraph();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);
            var vertex3 = graph.AddVertex(51, 3);
            var vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex2, vertex3);

            graph.Compress(true);

            Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 0);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);

            graph = this.CreateGraph();

            vertex1 = graph.AddVertex(51, 1);
            vertex2 = graph.AddVertex(51, 2);
            vertex3 = graph.AddVertex(51, 3);
            vertex4 = graph.AddVertex(51, 3);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);
            graph.AddEdge(vertex2, vertex3, new Edge()
            {
                Forward = true,
                Tags = 2
            }, null);
            graph.AddEdge(vertex3, vertex4, new Edge()
            {
                Forward = true,
                Tags = 3
            }, null);

            graph.AddEdge(vertex4, vertex2, new Edge()
            {
                Forward = true,
                Tags = 4
            }, null);

            graph.RemoveEdge(vertex3, vertex4);

            graph.Compress(true);

            Assert.IsFalse(graph.ContainsEdges(vertex3, vertex4));

            Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 1);
            Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 0);
            Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);

            var edges =  graph.GetEdges(vertex1, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(1, edges[0].Value.Tags);
            edges = graph.GetEdges(vertex2, vertex3).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(2, edges[0].Value.Tags);
            edges = graph.GetEdges(vertex4, vertex2).ToKeyValuePairs();
            Assert.AreEqual(1, edges.Length);
            Assert.AreEqual(4, edges[0].Value.Tags);
        }

        /// <summary>
        /// Test serializing a graph.
        /// </summary>
        [Test]
        public void TestGraphSerialize1()
        {
            var graph = new DirectedGraph<Edge>();

            var vertex1 = graph.AddVertex(51, 1);
            var vertex2 = graph.AddVertex(51, 2);

            graph.AddEdge(vertex1, vertex2, new Edge()
            {
                Forward = true,
                Tags = 1
            }, null);

            // serialize.
            using (var stream = new MemoryStream())
            {
                graph.Serialize(stream, Edge.SizeUints, Edge.MapFromDelegate, Edge.MapToDelegate);

                // deserialize.
                stream.Seek(0, SeekOrigin.Begin);
                var graphDeserialized = DirectedGraph<Edge>.Deserialize(stream, Edge.SizeUints, Edge.MapFromDelegate, Edge.MapToDelegate, false);

                // compare.
                Assert.AreEqual(graph.VertexCount, graphDeserialized.VertexCount);
                for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
                {
                    float latitude1, longitude1, latitude2, longitude2;
                    if (graph.GetVertex(vertex, out latitude1, out longitude1) &&
                        graphDeserialized.GetVertex(vertex, out latitude2, out longitude2))
                    {
                        Assert.AreEqual(latitude1, latitude2, 0.000001);
                        Assert.AreEqual(longitude1, longitude2, 0.000001);
                    }
                }
                var edges =  graphDeserialized.GetEdges(vertex1, vertex2).ToKeyValuePairs();
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(1, edges[0].Value.Tags);
            }
        }

        /// <summary>
        /// Test serializing a graph.
        /// </summary>
        [Test]
        public void TestGraphSerialize2()
        {
            const string embeddedString = "OsmSharp.Routing.Test.data.test_network.osm";

            // creates a new interpreter.
            var interpreter = new OsmRoutingInterpreter();

            // do the data processing.
            var graph = OsmSharp.Routing.Osm.Streams.CHEdgeGraphOsmStreamTarget.Preprocess(new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString)),
                new TagsIndex(new MemoryMappedStream(new MemoryStream())), interpreter, Vehicle.Car);

            // serialize.
            using (var stream = new MemoryStream())
            {
                graph.Serialize(stream, CHEdgeData.SizeUints, CHEdgeData.MapFromDelegate, CHEdgeData.MapToDelegate);

                // deserialize.
                stream.Seek(0, SeekOrigin.Begin);
                var graphDeserialized = RouterDataSource<CHEdgeData>.Deserialize(stream, CHEdgeData.SizeUints, CHEdgeData.MapFromDelegate, CHEdgeData.MapToDelegate, false);

                // compare.
                Assert.AreEqual(graph.VertexCount, graphDeserialized.VertexCount);
                for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
                {
                    float latitude1, longitude1, latitude2, longitude2;
                    if (graph.GetVertex(vertex, out latitude1, out longitude1) &&
                        graphDeserialized.GetVertex(vertex, out latitude2, out longitude2))
                    {
                        Assert.AreEqual(latitude1, latitude2, 0.000001);
                        Assert.AreEqual(longitude1, longitude2, 0.000001);
                    }
                    var edges =  graph.GetEdges(vertex).ToKeyValuePairs();
                    var edgesDeserialized = graphDeserialized.GetEdges(vertex).ToKeyValuePairs();
                    Assert.AreEqual(edges.Length, edgesDeserialized.Length);
                    for (int idx = 0; idx < edges.Length; idx++)
                    {
                        Assert.AreEqual(edges[idx].Value.Weight, edgesDeserialized[idx].Value.Weight);
                        Assert.AreEqual(edges[idx].Value.Tags, edgesDeserialized[idx].Value.Tags);
                        Assert.AreEqual(edges[idx].Value.Forward, edgesDeserialized[idx].Value.Forward);
                    }
                }
            }
        }
    }
}