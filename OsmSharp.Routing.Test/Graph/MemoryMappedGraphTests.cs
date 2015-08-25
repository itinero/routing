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
using OsmSharp.IO.MemoryMappedFiles;
using OsmSharp.Routing.Graph;
using System;
using System.Collections.Generic;
using System.IO;

namespace OsmSharp.Test.Unittests.Routing.Graph
{
    /// <summary>
    /// Tests a simple weighed dynamic graph.
    /// </summary>
    [TestFixture]
    public class MemoryMappedGraphTests
    {
        /// <summary>
        /// Tests argument out of range.
        /// </summary>
        [Test]
        public void TestArguments()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, 
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                uint vertex1 = graph.AddVertex(0, 0);
                uint vertex2 = graph.AddVertex(0, 0);
                uint vertex3 = 3;

                Assert.Catch<ArgumentOutOfRangeException>(() =>
                {
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
                Assert.Catch<ArgumentOutOfRangeException>(() =>
                {
                    graph.GetEdge(vertex3, vertex1, out edge);
                });
                Assert.Catch<ArgumentOutOfRangeException>(() =>
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
        }

        /// <summary>
        /// Tests adding a vertex.
        /// </summary>
        [Test]
        public void TestVertex()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
        }

        /// <summary>
        /// Tests adding 10000 vertices.
        /// </summary>
        [Test]
        public void TestVertex10000()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
        }

        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestEdge()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                var vertex1 = graph.AddVertex(51, 1);
                var vertex2 = graph.AddVertex(51, 2);

                graph.AddEdge(vertex1, vertex2, new Edge()
                                                   {
                                                       Forward = true,
                                                       Tags = 0
                                                   }, null);

                var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(0, edges[0].Value.Tags);
                Assert.AreEqual(vertex2, edges[0].Key);

                edges = graph.GetEdges(vertex2).ToKeyValuePairs();
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(0, edges[0].Value.Tags);
                Assert.AreEqual(vertex1, edges[0].Key);

                Edge edge;
                Assert.IsTrue(graph.GetEdge(vertex1, vertex2, out edge));
                Assert.AreEqual(0, edge.Tags);
                Assert.AreEqual(true, edge.Forward);
                Assert.IsTrue(graph.GetEdge(vertex2, vertex1, out edge));
                Assert.AreEqual(0, edge.Tags);
                Assert.AreEqual(false, edge.Forward);
            }
        }

        /// <summary>
        /// Tests adding 10000 edges.
        /// </summary>
        [Test]
        public void TestEdge10000()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, 
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                int count = 10000;
                uint vertex1 = graph.AddVertex(51, 1);
                while (count > 0)
                {
                    uint vertex2 = graph.AddVertex(51, 1);

                    graph.AddEdge(vertex1, vertex2, new Edge()
                                                       {
                                                           Tags = 0,
                                                           Forward = true
                                                       }, null);

                    var edges =  graph.GetEdges(vertex1).ToKeyValuePairs();
                    Assert.AreEqual(10000 - count + 1, edges.Length);

                    edges = graph.GetEdges(vertex2).ToKeyValuePairs();
                    Assert.AreEqual(1, edges.Length);
                    Assert.AreEqual(0, edges[0].Value.Tags);
                    Assert.AreEqual(vertex1, edges[0].Key);

                    count--;
                }
            }
        }

        /// <summary>
        /// Tests adding an edge and the reverse edge.
        /// </summary>
        [Test]
        public void TestEdge1()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                uint tagsId = 10;
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

                // test backward edge: backward edge is added automatically.
                edges = graph.GetEdges(vertex2).ToKeyValuePairs();
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(tagsId, edges[0].Value.Tags);
                Assert.AreEqual(vertex1, edges[0].Key);
                Assert.AreEqual(false, edges[0].Value.Forward);

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

                // test backward edge: backward edge is added automatically.
                edges = graph.GetEdges(vertex3).ToKeyValuePairs();
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(tagsId, edges[0].Value.Tags);
                Assert.AreEqual(vertex1, edges[0].Key);
                Assert.AreEqual(false, edges[0].Value.Forward);
            }
        }

        /// <summary>
        /// Tests adding and removing one edge.
        /// </summary>
        [Test]
        public void TestAddRemove1()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                uint tagsId = 10;
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
        }

        /// <summary>
        /// Tests adding and removing two edges.
        /// </summary>
        [Test]
        public void TestAddRemove2()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                uint tagsId = 10;
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
                Assert.AreEqual(2, edges.Length);
                edges = graph.GetEdges(vertex3).ToKeyValuePairs();
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(tagsId, edges[0].Value.Tags);
                Assert.AreEqual(vertex2, edges[0].Key);
                Assert.AreEqual(false, edges[0].Value.Forward);

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
                Assert.AreEqual(1, edges.Length);
                Assert.AreEqual(tagsId, edges[0].Value.Tags);
                Assert.AreEqual(vertex2, edges[0].Key);
                Assert.AreEqual(false, edges[0].Value.Forward);
            }

        }

        /// <summary>
        /// Tests adding and remove an arbitrary number of edges.
        /// </summary>
        [Test]
        public void TestAddRemoveX()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, 
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex1));

                graph.AddEdge(vertex2, vertex3, new Edge()
                {
                    Forward = true,
                    Tags = 2
                }, null);

                Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex1));

                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex3));
                Assert.IsTrue(graph.ContainsEdges(vertex3, vertex2));

                graph.AddEdge(vertex3, vertex4, new Edge()
                {
                    Forward = true,
                    Tags = 3
                }, null);

                Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex1));

                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex3));
                Assert.IsTrue(graph.ContainsEdges(vertex3, vertex2));

                Assert.IsTrue(graph.ContainsEdges(vertex3, vertex4));
                Assert.IsTrue(graph.ContainsEdges(vertex4, vertex3));

                graph.AddEdge(vertex4, vertex2, new Edge()
                {
                    Forward = true,
                    Tags = 4
                }, null);

                Assert.IsTrue(graph.ContainsEdges(vertex1, vertex2));
                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex1));

                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex3));
                Assert.IsTrue(graph.ContainsEdges(vertex3, vertex2));

                Assert.IsTrue(graph.ContainsEdges(vertex3, vertex4));
                Assert.IsTrue(graph.ContainsEdges(vertex4, vertex3));

                Assert.IsTrue(graph.ContainsEdges(vertex2, vertex4));
                Assert.IsTrue(graph.ContainsEdges(vertex4, vertex2));

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 3);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 2);
            }
        }

        /// <summary>
        /// Test removing an edge at the end.
        /// </summary>
        [Test]
        public void TestRemoveEnd()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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

                graph.RemoveEdge(vertex2, vertex4);
                Assert.IsFalse(graph.ContainsEdges(vertex2, vertex4));
                Assert.IsFalse(graph.ContainsEdges(vertex4, vertex2));

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);
            }
        }

        /// <summary>
        /// Test removing an edge in the middle.
        /// </summary>
        [Test]
        public void TestRemoveMiddle()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
                Assert.IsFalse(graph.ContainsEdges(vertex3, vertex2));

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 2);
            }
        }

        /// <summary>
        /// Test removing an edge in the beginning.
        /// </summary>
        [Test]
        public void TestRemoveBegin()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000, Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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

                graph.RemoveEdge(vertex2, vertex1);
                Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));
                Assert.IsFalse(graph.ContainsEdges(vertex1, vertex2));

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 0);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 2);
            }
        }

        /// <summary>
        /// Test removing an edge in the beginning.
        /// </summary>
        [Test]
        public void TestRemoveAll()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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

                graph.RemoveEdge(vertex2, vertex1);
                graph.RemoveEdge(vertex2, vertex3);
                graph.RemoveEdge(vertex4, vertex3);
                graph.RemoveEdge(vertex4, vertex2);
                Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));
                Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));
                Assert.IsFalse(graph.ContainsEdges(vertex4, vertex3));
                Assert.IsFalse(graph.ContainsEdges(vertex4, vertex2));
            }
        }

        /// <summary>
        /// Tests removing all edges for one vertex.
        /// </summary>
        [Test]
        public void TestRemoveAllOneVertex()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
                Assert.IsFalse(graph.ContainsEdges(vertex2, vertex1));
                Assert.IsFalse(graph.ContainsEdges(vertex2, vertex3));
                Assert.IsFalse(graph.ContainsEdges(vertex4, vertex2));
                Assert.IsTrue(graph.ContainsEdges(vertex3, vertex4));
            }
        }

        /// <summary>
        /// Tests trimming the graph but edges only (all vertices are stull used).
        /// </summary>
        [Test]
        public void TestCompressEdges()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
                Assert.IsFalse(graph.ContainsEdges(vertex3, vertex2));

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 2);
            }

            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
                uint vertex1 = graph.AddVertex(51, 1);
                uint vertex2 = graph.AddVertex(51, 2);
                uint vertex3 = graph.AddVertex(51, 3);
                uint vertex4 = graph.AddVertex(51, 3);

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
                Assert.IsFalse(graph.ContainsEdges(vertex4, vertex3));

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 3);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex4).ToKeyValuePairs().Length, 1);

                Edge edge;
                Assert.IsTrue(graph.GetEdge(vertex1, vertex2, out edge));
                Assert.AreEqual(1, edge.Tags);
                Assert.IsTrue(graph.GetEdge(vertex2, vertex3, out edge));
                Assert.AreEqual(2, edge.Tags);
                Assert.IsTrue(graph.GetEdge(vertex4, vertex2, out edge));
                Assert.AreEqual(4, edge.Tags);
            }
        }

        /// <summary>
        /// Tests trimming the graph but edges only (all vertices are stull used).
        /// </summary>
        [Test]
        public void TestCompressVertices()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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

                // make vertex4 obsolete.
                graph.RemoveEdges(vertex4);

                graph.Compress();

                Assert.AreEqual(3, graph.VertexCount);

                Assert.AreEqual(graph.GetEdges(vertex1).ToKeyValuePairs().Length, 1);
                Assert.AreEqual(graph.GetEdges(vertex2).ToKeyValuePairs().Length, 2);
                Assert.AreEqual(graph.GetEdges(vertex3).ToKeyValuePairs().Length, 1);
            }
        }

        /// <summary>
        /// Tests overwrite an edge with a reverse edge.
        /// </summary>
        [Test]
        public void TestAddReverse()
        {
            using (var graph = new Graph<Edge>(new MemoryMappedStream(new MemoryStream()), 1000,
                Edge.MapFromDelegate, Edge.MapToDelegate, Edge.SizeUints))
            {
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
            }
        }
    }
}