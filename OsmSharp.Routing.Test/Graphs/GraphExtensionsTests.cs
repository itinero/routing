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
using OsmSharp.Math.Algorithms;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing.Graph
{
    /// <summary>
    /// Contains tests for the GraphExtensions.
    /// </summary>
    [TestFixture]
    public class GraphExtensionsTests
    {
        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestGraphSwitchTwoVertices()
        {
            // two vertices 
            var graph = new Graph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);

            graph.Switch(vertex1, vertex2);

            float latitude, longitude;
            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestGraphSwitchTwoVerticesOneEdge()
        {
            float latitude, longitude;
            var graph = new Graph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);
            graph.AddEdge(1, 2, new Edge()
            {
                Tags = 1,
                Forward = true
            });

            graph.Switch(vertex1, vertex2);

            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            var edges = graph.GetEdges(1);
            Assert.AreEqual(1, edges.Count);
            foreach (var edge in edges)
            {
                Assert.AreEqual(2, edge.Neighbour);
                Assert.AreEqual(1, edge.EdgeData.Tags);
                Assert.IsFalse(edge.EdgeData.Forward);
            }
            edges = graph.GetEdges(2);
            Assert.AreEqual(1, edges.Count);
            foreach (var edge in edges)
            {
                Assert.AreEqual(1, edge.Neighbour);
                Assert.AreEqual(1, edge.EdgeData.Tags);
                Assert.IsTrue(edge.EdgeData.Forward);
            }
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestGraphSwitchThreeVerticesTwoEdges()
        {
            float latitude, longitude;
            var graph = new Graph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);
            var vertex3 = graph.AddVertex(3, 3);
            graph.AddEdge(1, 2, new Edge()
            {
                Tags = 1,
                Forward = true
            });
            graph.AddEdge(1, 3, new Edge()
            {
                Tags = 2,
                Forward = true
            });

            graph.Switch(vertex1, vertex2);

            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            var edges = graph.GetEdges(1);
            Assert.AreEqual(1, edges.Count);
            foreach (var edge in edges)
            {
                if(edge.Neighbour == 2)
                {
                    Assert.AreEqual(1, edge.EdgeData.Tags);
                    Assert.IsFalse(edge.EdgeData.Forward);
                }
                else
                {
                    Assert.Fail("The only edge should be an edge with neighbour equal to 2.");
                }
            }
            edges = graph.GetEdges(2);
            Assert.AreEqual(2, edges.Count);
            foreach (var edge in edges)
            {
                if (edge.Neighbour == 1)
                {
                    Assert.AreEqual(1, edge.EdgeData.Tags);
                    Assert.IsTrue(edge.EdgeData.Forward);
                }
                else if (edge.Neighbour == 3)
                {
                    Assert.AreEqual(2, edge.EdgeData.Tags);
                    Assert.IsTrue(edge.EdgeData.Forward);
                }
                else
                {
                    Assert.Fail("The only edge should be an edge with neighbour equal to 2.");
                }
            }
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestDirectedGraphSwitchTwoVertices()
        {
            // two vertices 
            var graph = new DirectedGraph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);
            var reverse = new Dictionary<uint, List<uint>>();

            graph.Switch(vertex1, vertex2, reverse);

            float latitude, longitude;
            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestDirectedGraphSwitchTwoVerticesOneEdge()
        {
            float latitude, longitude;
            var graph = new DirectedGraph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);
            graph.AddEdge(1, 2, new Edge()
            {
                Tags = 1,
                Forward = true
            });
            var reverse = new Dictionary<uint, List<uint>>();
            reverse.Add(2, new List<uint>(new uint[] { 1 }));

            graph.Switch(vertex1, vertex2, reverse);

            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            var edges = graph.GetEdges(1);
            Assert.AreEqual(0, edges.Count);
            edges = graph.GetEdges(2);
            Assert.AreEqual(1, edges.Count);
            foreach (var edge in edges)
            {
                Assert.AreEqual(1, edge.Neighbour);
                Assert.AreEqual(1, edge.EdgeData.Tags);
                Assert.IsTrue(edge.EdgeData.Forward);
            }
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestDirectedGraphSwitchThreeVerticesTwoEdges()
        {
            float latitude, longitude;
            var graph = new DirectedGraph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);
            var vertex3 = graph.AddVertex(3, 3);
            graph.AddEdge(1, 2, new Edge()
            {
                Tags = 1,
                Forward = true
            });
            graph.AddEdge(1, 3, new Edge()
            {
                Tags = 2,
                Forward = true
            });
            var reverse = new Dictionary<uint, List<uint>>();
            reverse.Add(2, new List<uint>(new uint[] { 1 }));
            reverse.Add(3, new List<uint>(new uint[] { 1 }));

            graph.Switch(vertex1, vertex2, reverse);

            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            var edges = graph.GetEdges(1);
            Assert.AreEqual(0, edges.Count);
            edges = graph.GetEdges(2);
            Assert.AreEqual(2, edges.Count);
            foreach (var edge in edges)
            {
                if (edge.Neighbour == 1)
                {
                    Assert.AreEqual(1, edge.EdgeData.Tags);
                    Assert.IsTrue(edge.EdgeData.Forward);
                }
                else if (edge.Neighbour == 3)
                {
                    Assert.AreEqual(2, edge.EdgeData.Tags);
                    Assert.IsTrue(edge.EdgeData.Forward);
                }
                else
                {
                    Assert.Fail("The only edge should be an edge with neighbour equal to 2.");
                }
            }
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        /// <remarks>
        /// network:    3----(2)---->1----(1)---->2
        /// is tranformed into
        ///             2----(2)---->3----(1)---->1
        /// by switching 1 - 2 and 2 - 3
        /// </remarks>
        [Test]
        public void TestDirectedGraphSwitchTwiceThreeVerticesTwoEdges()
        {
            float latitude, longitude;
            var graph = new DirectedGraph<Edge>();
            var vertex1 = graph.AddVertex(1, 1);
            var vertex2 = graph.AddVertex(2, 2);
            var vertex3 = graph.AddVertex(3, 3);
            graph.AddEdge(1, 2, new Edge()
            {
                Tags = 1,
                Forward = true
            });
            graph.AddEdge(3, 1, new Edge()
            {
                Tags = 2,
                Forward = true
            });
            var reverse = new Dictionary<uint, List<uint>>();
            reverse.Add(2, new List<uint>(new uint[] { 1 }));
            reverse.Add(1, new List<uint>(new uint[] { 3 }));

            graph.Switch(vertex1, vertex2, reverse);
            graph.Switch(vertex2, vertex3, reverse);

            graph.GetVertex(1, out latitude, out longitude);
            Assert.AreEqual(2, latitude);
            Assert.AreEqual(2, longitude);
            graph.GetVertex(2, out latitude, out longitude);
            Assert.AreEqual(3, latitude);
            Assert.AreEqual(3, longitude);
            graph.GetVertex(3, out latitude, out longitude);
            Assert.AreEqual(1, latitude);
            Assert.AreEqual(1, longitude);
            var edges = graph.GetEdges(1);
            Assert.AreEqual(0, edges.Count);
            edges = graph.GetEdges(2);
            Assert.AreEqual(1, edges.Count);
            foreach (var edge in edges)
            {
                Assert.AreEqual(3, edge.Neighbour);
                Assert.AreEqual(2, edge.EdgeData.Tags);
                Assert.IsTrue(edge.EdgeData.Forward);
            }
            edges = graph.GetEdges(3);
            Assert.AreEqual(1, edges.Count);
            foreach (var edge in edges)
            {
                Assert.AreEqual(1, edge.Neighbour);
                Assert.AreEqual(1, edge.EdgeData.Tags);
                Assert.IsTrue(edge.EdgeData.Forward);
            }
        }

        /// <summary>
        /// Tests the sort hilbert function with order #4.
        /// </summary>
        [Test]
        public void SortHilbertTestSteps4()
        {
            var n = 4;

            // build locations.
            var locations = new List<GeoCoordinate>();
            locations.Add(new GeoCoordinate(-90, -180));
            locations.Add(new GeoCoordinate(-90, -60));
            locations.Add(new GeoCoordinate(-90, 60));
            locations.Add(new GeoCoordinate(-90, 180));
            locations.Add(new GeoCoordinate(-30, -180));
            locations.Add(new GeoCoordinate(-30, -60));
            locations.Add(new GeoCoordinate(-30, 60));
            locations.Add(new GeoCoordinate(-30, 180));
            locations.Add(new GeoCoordinate(30, -180));
            locations.Add(new GeoCoordinate(30, -60));
            locations.Add(new GeoCoordinate(30, 60));
            locations.Add(new GeoCoordinate(30, 180));
            locations.Add(new GeoCoordinate(90, -180));
            locations.Add(new GeoCoordinate(90, -60));
            locations.Add(new GeoCoordinate(90, 60));
            locations.Add(new GeoCoordinate(90, 180));

            // build graph.
            var graph = new Graph<Edge>();
            for (var idx = 0; idx < locations.Count; idx++)
            {
                graph.AddVertex((float)locations[idx].Latitude, 
                    (float)locations[idx].Longitude);
            }

            // build a sorted version in-place.
            graph.SortHilbert(n);

            // test if sorted.
            for (uint vertex = 1; vertex < graph.VertexCount; vertex++)
            {
                Assert.IsTrue(
                    GraphExtensions.HilbertDistance(graph, n, vertex) <=
                    GraphExtensions.HilbertDistance(graph, n, vertex + 1));
            }

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Latitude, (float)x.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Latitude, (float)y.Longitude, n));
            });

            // confirm sort.
            for(uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                float latitude, longitude;
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)(vertex - 1)].Latitude);
                Assert.AreEqual(longitude, locations[(int)(vertex - 1)].Longitude);
            }
        }

        /// <summary>
        /// Tests the sort hilbert function with order #4.
        /// </summary>
        [Test]
        public void SortDirectedHilbertTestSteps4()
        {
            var n = 4;

            // build locations.
            var locations = new List<GeoCoordinate>();
            locations.Add(new GeoCoordinate(-90, -180));
            locations.Add(new GeoCoordinate(-90, -60));
            locations.Add(new GeoCoordinate(-90, 60));
            locations.Add(new GeoCoordinate(-90, 180));
            locations.Add(new GeoCoordinate(-30, -180));
            locations.Add(new GeoCoordinate(-30, -60));
            locations.Add(new GeoCoordinate(-30, 60));
            locations.Add(new GeoCoordinate(-30, 180));
            locations.Add(new GeoCoordinate(30, -180));
            locations.Add(new GeoCoordinate(30, -60));
            locations.Add(new GeoCoordinate(30, 60));
            locations.Add(new GeoCoordinate(30, 180));
            locations.Add(new GeoCoordinate(90, -180));
            locations.Add(new GeoCoordinate(90, -60));
            locations.Add(new GeoCoordinate(90, 60));
            locations.Add(new GeoCoordinate(90, 180));

            // build graph.
            var graph = new DirectedGraph<Edge>();
            for (var idx = 0; idx < locations.Count; idx++)
            {
                graph.AddVertex((float)locations[idx].Latitude,
                    (float)locations[idx].Longitude);
            }

            // build a sorted version.
            graph.SortHilbert(n);

            // test if sorted.
            for (uint vertex = 1; vertex < graph.VertexCount; vertex++)
            {
                Assert.IsTrue(
                    GraphExtensions.HilbertDistance(graph, n, vertex) <=
                    GraphExtensions.HilbertDistance(graph, n, vertex + 1));
            }

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Latitude, (float)x.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Latitude, (float)y.Longitude, n));
            });

            // confirm sort.
            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                float latitude, longitude;
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)(vertex - 1)].Latitude);
                Assert.AreEqual(longitude, locations[(int)(vertex - 1)].Longitude);
            }
        }

        /// <summary>
        /// Tests the sort hilbert function with the default order.
        /// </summary>
        [Test]
        public void SortHilbertTestStepsDefault()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            // build locations.
            var locations = new List<GeoCoordinate>();
            locations.Add(new GeoCoordinate(-90, -180));
            locations.Add(new GeoCoordinate(-90, -60));
            locations.Add(new GeoCoordinate(-90, 60));
            locations.Add(new GeoCoordinate(-90, 180));
            locations.Add(new GeoCoordinate(-30, -180));
            locations.Add(new GeoCoordinate(-30, -60));
            locations.Add(new GeoCoordinate(-30, 60));
            locations.Add(new GeoCoordinate(-30, 180));
            locations.Add(new GeoCoordinate(30, -180));
            locations.Add(new GeoCoordinate(30, -60));
            locations.Add(new GeoCoordinate(30, 60));
            locations.Add(new GeoCoordinate(30, 180));
            locations.Add(new GeoCoordinate(90, -180));
            locations.Add(new GeoCoordinate(90, -60));
            locations.Add(new GeoCoordinate(90, 60));
            locations.Add(new GeoCoordinate(90, 180));

            // build graph.
            var graph = new Graph<Edge>();
            for (var idx = 0; idx < locations.Count; idx++)
            {
                graph.AddVertex((float)locations[idx].Latitude,
                    (float)locations[idx].Longitude);
            }

            // build a sorted version.
            graph.SortHilbert(n);

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Latitude, (float)x.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Latitude, (float)y.Longitude, n));
            });

            // confirm sort.
            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                float latitude, longitude;
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)(vertex - 1)].Latitude);
                Assert.AreEqual(longitude, locations[(int)(vertex - 1)].Longitude);
            }
        }

        /// <summary>
        /// Tests the sort hilbert function with the default order.
        /// </summary>
        [Test]
        public void SortHilbertTestDefaultStepsWithEdges()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            // build locations.
            var locations = new List<Tuple<GeoCoordinate, uint>>();
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, -180), 1));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, -60), 2));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, 60), 3));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, 180), 4));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, -180), 5));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, -60), 6));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, 60), 7));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, 180), 8));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, -180), 9));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, -60), 10));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, 60), 11));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, 180), 12));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, -180), 13));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, -60), 14));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, 60), 15));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, 180), 16));

            // build graph.
            var graph = new Graph<Edge>();
            for (var idx = 0; idx < locations.Count; idx++)
            {
                graph.AddVertex((float)locations[idx].Item1.Latitude,
                    (float)locations[idx].Item1.Longitude);
            }

            // add edges.
            graph.AddEdge(1, 2, new Edge() { Tags = 1 });
            graph.AddEdge(2, 3, new Edge() { Tags = 2 });
            graph.AddEdge(3, 4, new Edge() { Tags = 3 });
            graph.AddEdge(4, 5, new Edge() { Tags = 4 });
            graph.AddEdge(5, 6, new Edge() { Tags = 5 });
            graph.AddEdge(6, 7, new Edge() { Tags = 6 });
            graph.AddEdge(7, 8, new Edge() { Tags = 7 });
            graph.AddEdge(8, 9, new Edge() { Tags = 8 });
            graph.AddEdge(9, 10, new Edge() { Tags = 9 });
            graph.AddEdge(10, 11, new Edge() { Tags = 10 });
            graph.AddEdge(11, 12, new Edge() { Tags = 11 });
            graph.AddEdge(12, 13, new Edge() { Tags = 12 });
            graph.AddEdge(13, 14, new Edge() { Tags = 13 });
            graph.AddEdge(14, 15, new Edge() { Tags = 14 });

            // build a sorted version.
            graph.SortHilbert(n);

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Item1.Latitude, (float)x.Item1.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Item1.Latitude, (float)y.Item1.Longitude, n));
            });

            // confirm sort.
            float latitude, longitude;
            var newToOld = new Dictionary<uint, uint>();
            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)(vertex - 1)].Item1.Latitude);
                Assert.AreEqual(longitude, locations[(int)(vertex - 1)].Item1.Longitude);

                newToOld.Add(vertex, locations[(int)(vertex - 1)].Item2);
            }

            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                var edges = graph.GetEdges(vertex);
                var originalVertex = newToOld[vertex];
                foreach (var edge in edges)
                {
                    var originalNeighbour = newToOld[edges.Neighbour];
                    Assert.IsTrue(originalVertex - 1 == originalNeighbour ||
                        originalVertex + 1 == originalNeighbour);
                }
            }
        }

        /// <summary>
        /// Tests the sort hilbert function with the default order.
        /// </summary>
        [Test]
        public void SortDirectedHilbertTestDefaultStepsWithEdges()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            // build locations.
            var locations = new List<Tuple<GeoCoordinate, uint>>();
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, -180), 1));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, -60), 2));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, 60), 3));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, 180), 4));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, -180), 5));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, -60), 6));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, 60), 7));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, 180), 8));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, -180), 9));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, -60), 10));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, 60), 11));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, 180), 12));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, -180), 13));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, -60), 14));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, 60), 15));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, 180), 16));

            // build graph.
            var graph = new DirectedGraph<Edge>();
            for (var idx = 0; idx < locations.Count; idx++)
            {
                graph.AddVertex((float)locations[idx].Item1.Latitude,
                    (float)locations[idx].Item1.Longitude);
            }

            // add edges.
            graph.AddEdge(1, 2, new Edge() { Tags = 1 });
            graph.AddEdge(2, 3, new Edge() { Tags = 2 });
            graph.AddEdge(3, 4, new Edge() { Tags = 3 });
            graph.AddEdge(4, 5, new Edge() { Tags = 4 });
            graph.AddEdge(5, 6, new Edge() { Tags = 5 });
            graph.AddEdge(6, 7, new Edge() { Tags = 6 });
            graph.AddEdge(7, 8, new Edge() { Tags = 7 });
            graph.AddEdge(8, 9, new Edge() { Tags = 8 });
            graph.AddEdge(9, 10, new Edge() { Tags = 9 });
            graph.AddEdge(10, 11, new Edge() { Tags = 10 });
            graph.AddEdge(11, 12, new Edge() { Tags = 11 });
            graph.AddEdge(12, 13, new Edge() { Tags = 12 });
            graph.AddEdge(13, 14, new Edge() { Tags = 13 });
            graph.AddEdge(14, 15, new Edge() { Tags = 14 });

            // build a sorted version.
            graph.SortHilbert(n);

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Item1.Latitude, (float)x.Item1.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Item1.Latitude, (float)y.Item1.Longitude, n));
            });

            // confirm sort.
            float latitude, longitude;
            var newToOld = new Dictionary<uint, uint>();
            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)(vertex - 1)].Item1.Latitude);
                Assert.AreEqual(longitude, locations[(int)(vertex - 1)].Item1.Longitude);

                newToOld.Add(vertex, locations[(int)(vertex - 1)].Item2);
            }

            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                var edges = graph.GetEdges(vertex);
                var originalVertex = newToOld[vertex];
                foreach (var edge in edges)
                {
                    var originalNeighbour = newToOld[edges.Neighbour];
                    Assert.IsTrue(originalVertex - 1 == originalNeighbour ||
                        originalVertex + 1 == originalNeighbour);
                }
            }
        }

        /// <summary>
        /// Tests the hilbert search function.
        /// </summary>
        [Test]
        public void SearchHilbertTest4()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            // build graph.
            var graph = new Graph<Edge>();
            var vertex1 = graph.AddVertex(-90, -180);
            var vertex2 = graph.AddVertex(-90, -60);
            var vertex3 = graph.AddVertex(-30, -60);
            var vertex4 = graph.AddVertex(-30, -180);
            var vertex5 = graph.AddVertex(30, -180);
            var vertex6 = graph.AddVertex(90, -180);
            var vertex7 = graph.AddVertex(90, -60);
            var vertex8 = graph.AddVertex(30, -60);
            var vertex9 = graph.AddVertex(30, 60);
            var vertex10 = graph.AddVertex(90, 60);
            var vertex11 = graph.AddVertex(90, 180);
            var vertex12 = graph.AddVertex(30, 180);
            var vertex13 = graph.AddVertex(-30, 180);
            var vertex14 = graph.AddVertex(-30, 60);
            var vertex15 = graph.AddVertex(-90, 60);
            var vertex16 = graph.AddVertex(-90, 180);

            // search vertex.
            uint vertex;
            int count;

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-90, -180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex1, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-90, -60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex2, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-30, -60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex3, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-30, -180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex4, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(30, -180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex5, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(90, -180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex6, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(90, -60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex7, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(30, -60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex8, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(30, 60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex9, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(90, 60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex10, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(90, 180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex11, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(30, 180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex12, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-30, 180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex13, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-30, 60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex14, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-90, 60, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex15, vertex);
            Assert.AreEqual(1, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-90, 180, n), n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex16, vertex);
            Assert.AreEqual(1, count);
            
            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(30, 60, n) + 1, n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex9, vertex);
            Assert.AreEqual(0, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(90, -180, n) + 1, n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex6, vertex);
            Assert.AreEqual(0, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(-30, 60, n) + 1, n, 1, 16, out vertex, out count));
            Assert.AreEqual(vertex14, vertex);
            Assert.AreEqual(0, count);

            // build graph.
            graph = new Graph<Edge>();
            vertex1 = graph.AddVertex(0, 0);
            vertex2 = graph.AddVertex(0.00001f, 0.00001f);
            vertex3 = graph.AddVertex(0.00002f, 0.00002f);
            vertex4 = graph.AddVertex(0.00003f, 0.00003f);
            vertex5 = graph.AddVertex(0.00004f, 0.00004f);
            vertex6 = graph.AddVertex(0.00005f, 0.00005f);

            // build a sorted version.
            graph.SortHilbert(n);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(0, 0, n), n, vertex1, vertex6, 
                out vertex, out count));
            Assert.AreEqual(vertex1, vertex);
            Assert.AreEqual(6, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(0, 0, n), n, vertex3, vertex6, 
                out vertex, out count));
            Assert.AreEqual(vertex1, vertex);
            Assert.AreEqual(6, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(0, 0, n), n, vertex1, vertex3, 
                out vertex, out count));
            Assert.AreEqual(vertex1, vertex);
            Assert.AreEqual(6, count);

            Assert.IsTrue(graph.SearchHilbert(HilbertCurve.HilbertDistance(0, 0, n), n, vertex3, vertex3, 
                out vertex, out count));
            Assert.AreEqual(vertex1, vertex);
            Assert.AreEqual(6, count);
        }

        /// <summary>
        /// Tests the hilbert search function.
        /// </summary>
        [Test]
        public void SearchHilbertTest5()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            // build graph.
            var graph = new Graph<Edge>();
            var vertex1 = graph.AddVertex(-90, -180);
            var vertex2 = graph.AddVertex(-90, -60);
            var vertex3 = graph.AddVertex(-30, -60);
            var vertex4 = graph.AddVertex(-30, -180);
            var vertex5 = graph.AddVertex(30, -180);
            var vertex6 = graph.AddVertex(90, -180);
            var vertex7 = graph.AddVertex(90, -60);
            var vertex8 = graph.AddVertex(30, -60);
            var vertex9 = graph.AddVertex(30, 60);
            var vertex10 = graph.AddVertex(90, 60);
            var vertex11 = graph.AddVertex(90, 180);
            var vertex12 = graph.AddVertex(30, 180);
            var vertex13 = graph.AddVertex(-30, 180);
            var vertex14 = graph.AddVertex(-30, 60);
            var vertex15 = graph.AddVertex(-90, 60);
            var vertex16 = graph.AddVertex(-90, 180);

            // search.
            var found = graph.SearchHilbert(30, 60, 0.001f);
            Assert.IsNotNull(found);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(vertex9, found[0]);

            found = graph.SearchHilbert(30, -180, 0.001f);
            Assert.IsNotNull(found);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(vertex5, found[0]);

            found = graph.SearchHilbert(30, 180, 0.001f);
            Assert.IsNotNull(found);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(vertex12, found[0]);

            found = graph.SearchHilbert(-30, -60, 0.001f);
            Assert.IsNotNull(found);
            Assert.AreEqual(1, found.Count);
            Assert.AreEqual(vertex3, found[0]);

            // build graph.
            graph = new Graph<Edge>();
            vertex1 = graph.AddVertex(0, 0);
            vertex2 = graph.AddVertex(0.00001f, 0.00001f);
            vertex3 = graph.AddVertex(0.00002f, 0.00002f);
            vertex4 = graph.AddVertex(0.00003f, 0.00003f);
            vertex5 = graph.AddVertex(0.00004f, 0.00004f);
            vertex6 = graph.AddVertex(0.00005f, 0.00005f);

            // build a sorted version.
            graph.SortHilbert(n);

            found = graph.SearchHilbert(0, 0, 0.001f);
            Assert.IsNotNull(found);
            Assert.AreEqual(6, found.Count);
        }

        /// <summary>
        /// Tests the hilbert sorting function.
        /// </summary>
        [Test]
        public void SortHilbertRealTest6()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "OsmSharp.Routing.Test.data.test_network_real1.osm"));
            var data = GraphOsmStreamTarget.Preprocess(source, new OsmRoutingInterpreter());

            // test if sorted.
            for(uint vertex = 1; vertex < data.VertexCount; vertex++)
            {
                Assert.IsTrue(
                    GraphExtensions.HilbertDistance(data, n, vertex) <=
                    GraphExtensions.HilbertDistance(data, n, vertex + 1));
            }
        }

        /// <summary>
        /// Tests the sort hilbert function with the default order.
        /// </summary>
        [Test]
        public void SortHilbertTest7()
        {
            var n = GraphExtensions.DefaultHilbertSteps;

            // build locations.
            var locations = new List<Tuple<GeoCoordinate, uint>>();
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, -180), 1));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, -60), 2));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, 60), 3));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-90, 180), 4));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, -180), 5));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, -60), 6));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, 60), 7));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(-30, 180), 8));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, -180), 9));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, -60), 10));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, 60), 11));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(30, 180), 12));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, -180), 13));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, -60), 14));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, 60), 15));
            locations.Add(new Tuple<GeoCoordinate, uint>(new GeoCoordinate(90, 180), 16));

            // build graph.
            var graph = new Graph<Edge>();
            for (var idx = 0; idx < locations.Count; idx++)
            {
                graph.AddVertex((float)locations[idx].Item1.Latitude,
                    (float)locations[idx].Item1.Longitude);
            }

            // add edges.
            Func<uint, uint, uint> buildTagsId = (v1, v2) => { return v1 + (v2 * 16); };
            graph.AddEdge(1, 2, new Edge() { Tags = buildTagsId(1, 2) });
            graph.AddEdge(2, 3, new Edge() { Tags = buildTagsId(2, 3) });
            graph.AddEdge(3, 4, new Edge() { Tags = buildTagsId(3, 4) });
            graph.AddEdge(4, 5, new Edge() { Tags = buildTagsId(4, 5) });
            graph.AddEdge(5, 6, new Edge() { Tags = buildTagsId(5, 6) });
            graph.AddEdge(6, 7, new Edge() { Tags = buildTagsId(6, 7) });
            graph.AddEdge(7, 8, new Edge() { Tags = buildTagsId(7, 8) });
            graph.AddEdge(8, 9, new Edge() { Tags = buildTagsId(8, 9) });
            graph.AddEdge(9, 10, new Edge() { Tags = buildTagsId(9, 10) });
            graph.AddEdge(10, 11, new Edge() { Tags = buildTagsId(10, 11) });
            graph.AddEdge(11, 12, new Edge() { Tags = buildTagsId(11, 12) });
            graph.AddEdge(12, 13, new Edge() { Tags = buildTagsId(12, 13) });
            graph.AddEdge(13, 14, new Edge() { Tags = buildTagsId(13, 14) });
            graph.AddEdge(14, 15, new Edge() { Tags = buildTagsId(14, 15) });

            // add 10 random edges.
            for (var i = 0; i < 10; i++)
            {
                var from = Convert.ToUInt32(OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(15) + 1);
                var to = Convert.ToUInt32(OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(14) + 1);
                if(from <= to)
                {
                    to = to + 1;
                }
                graph.AddEdge(from, to, new Edge() { Tags = buildTagsId(from, to) });
            }

            // sort vertices in-place.
            graph.SortHilbert();
            graph.Compress();

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Item1.Latitude, (float)x.Item1.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Item1.Latitude, (float)y.Item1.Longitude, n));
            });

            // confirm sort.
            float latitude, longitude;
            var convert = new Dictionary<uint, uint>();
            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)(vertex - 1)].Item1.Latitude);
                Assert.AreEqual(longitude, locations[(int)(vertex - 1)].Item1.Longitude);

                convert.Add(vertex, locations[(int)(vertex - 1)].Item2);
            }

            for (uint vertex = 1; vertex <= graph.VertexCount; vertex++)
            {
                var edges = graph.GetEdges(vertex);
                var originalVertex = convert[vertex];
                foreach (var edge in edges)
                {
                    var originalNeighbour = convert[edges.Neighbour];
                    if (edge.EdgeData.Forward)
                    { // edge is forward.
                        Assert.AreEqual(buildTagsId(originalVertex, originalNeighbour), edge.EdgeData.Tags);
                    }
                    else
                    { // edge is backward.
                        Assert.AreEqual(buildTagsId(originalNeighbour, originalVertex), edge.EdgeData.Tags);
                    }
                }
            }
        }

        /// <summary>
        /// Tests the hilbert sorting function.
        /// </summary>
        [Test]
        public void SortHilbertRealTest()
        {
            var embeddedString = "OsmSharp.Routing.Test.data.test_network.osm";
            var n = GraphExtensions.DefaultHilbertSteps;

            // do the data processing without preprocessing.
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();
            var unsortedData = new RouterDataSource<Edge>(new DirectedGraph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(unsortedData, interpreter, tagsIndex, (g) => { return null; });
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // do the data processing with only hilbert sorting preprocessing.
            interpreter = new OsmRoutingInterpreter();
            tagsIndex = new TagsIndex();
            var data = new RouterDataSource<Edge>(new DirectedGraph<Edge>(), tagsIndex);
            targetData = new GraphOsmStreamTarget(data, interpreter, tagsIndex, (g) => { 
                return new HilbertSortingPreprocessor<Edge>(g); });
            dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // test if sorted.
            for (uint sortedVertex = 1; sortedVertex < data.VertexCount; sortedVertex++)
            {
                var coordinate = data.GetLocation(sortedVertex);
                var neighbours = new Dictionary<GeoCoordinateSimple, Edge>();
                foreach (var edge in data.GetEdges(sortedVertex))
                {
                    neighbours[data.GetLocation(edge.Neighbour)] = edge.EdgeData;
                }

                for (uint unsortedVertex = 1; unsortedVertex < unsortedData.VertexCount; unsortedVertex++)
                {
                    var unsortedCoordinate = unsortedData.GetLocation(unsortedVertex);
                    if(coordinate.Latitude == unsortedCoordinate.Latitude &&
                        coordinate.Longitude == unsortedCoordinate.Longitude)
                    { // this is the same vertex, all vertices is the test network have different coordinates.
                        // compare their neighbours.
                        var unsortedNeighbours = new Dictionary<GeoCoordinateSimple, Edge>();
                        foreach(var edge in unsortedData.GetEdges(unsortedVertex))
                        {
                            unsortedNeighbours[unsortedData.GetLocation(edge.Neighbour)] = edge.EdgeData;
                        }
                        Assert.AreEqual(neighbours.Count, unsortedNeighbours.Count);
                        foreach(var neighbour in neighbours)
                        {
                            Assert.IsTrue(unsortedNeighbours.ContainsKey(neighbour.Key));
                            var value = unsortedNeighbours[neighbour.Key];
                            Assert.AreEqual(neighbour.Value.Distance, value.Distance);
                            Assert.AreEqual(neighbour.Value.Forward, value.Forward);
                            Assert.AreEqual(neighbour.Value.RepresentsNeighbourRelations, value.RepresentsNeighbourRelations);
                            Assert.AreEqual(neighbour.Value.Tags, value.Tags);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Tests the hilbert sorting function.
        /// </summary>
        [Test]
        public void SortDirectedHilbertRealTest()
        {
            var embeddedString = "OsmSharp.Routing.Test.data.test_network.osm";
            var n = GraphExtensions.DefaultHilbertSteps;

            // do the data processing without preprocessing.
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();
            var unsortedData = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            var targetData = new CHEdgeGraphOsmStreamTarget(unsortedData, interpreter, tagsIndex, OsmSharp.Routing.Vehicles.Vehicle.Car, 
                (g) => { return null; });
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // do the data processing with only hilbert sorting preprocessing.
            interpreter = new OsmRoutingInterpreter();
            tagsIndex = new TagsIndex();
            var data = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            targetData = new CHEdgeGraphOsmStreamTarget(data, interpreter, tagsIndex, OsmSharp.Routing.Vehicles.Vehicle.Car, (g) =>
            {
                return new HilbertSortingPreprocessor<CHEdgeData>(g);
            });
            dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // test if sorted.
            for (uint sortedVertex = 1; sortedVertex < data.VertexCount; sortedVertex++)
            {
                var coordinate = data.GetLocation(sortedVertex);
                var neighbours = new Dictionary<GeoCoordinateSimple, CHEdgeData>();
                foreach (var edge in data.GetEdges(sortedVertex))
                {
                    neighbours[data.GetLocation(edge.Neighbour)] = edge.EdgeData;
                }

                for (uint unsortedVertex = 1; unsortedVertex < unsortedData.VertexCount; unsortedVertex++)
                {
                    var unsortedCoordinate = unsortedData.GetLocation(unsortedVertex);
                    if (coordinate.Latitude == unsortedCoordinate.Latitude &&
                        coordinate.Longitude == unsortedCoordinate.Longitude)
                    { // this is the same vertex, all vertices is the test network have different coordinates.
                        // compare their neighbours.
                        var unsortedNeighbours = new Dictionary<GeoCoordinateSimple, CHEdgeData>();
                        foreach (var edge in unsortedData.GetEdges(unsortedVertex))
                        {
                            unsortedNeighbours[unsortedData.GetLocation(edge.Neighbour)] = edge.EdgeData;
                        }
                        Assert.AreEqual(neighbours.Count, unsortedNeighbours.Count);
                        foreach (var neighbour in neighbours)
                        {
                            Assert.IsTrue(unsortedNeighbours.ContainsKey(neighbour.Key));
                            var value = unsortedNeighbours[neighbour.Key];
                            Assert.AreEqual(neighbour.Value.Weight, value.Weight);
                            Assert.AreEqual(neighbour.Value.Forward, value.Forward);
                            Assert.AreEqual(neighbour.Value.RepresentsNeighbourRelations, value.RepresentsNeighbourRelations);
                            Assert.AreEqual(neighbour.Value.Tags, value.Tags);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Tests the hilbert sorting function.
        /// </summary>
        [Test]
        public void SortHilbertRealBigTest()
        {
            var embeddedString = "OsmSharp.Routing.Test.data.test_network_real1.osm";
            var n = GraphExtensions.DefaultHilbertSteps;

            // do the data processing without preprocessing.
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();
            var unsortedData = new RouterDataSource<Edge>(new DirectedGraph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(unsortedData, interpreter, tagsIndex, (g) => { return null; });
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // do the data processing with only hilbert sorting preprocessing.
            interpreter = new OsmRoutingInterpreter();
            tagsIndex = new TagsIndex();
            var data = new RouterDataSource<Edge>(new DirectedGraph<Edge>(), tagsIndex);
            targetData = new GraphOsmStreamTarget(data, interpreter, tagsIndex, (g) =>
            {
                return new HilbertSortingPreprocessor<Edge>(g);
            });
            dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // test if sorted.
            for (uint sortedVertex = 1; sortedVertex < data.VertexCount; sortedVertex++)
            {
                var coordinate = data.GetLocation(sortedVertex);
                var neighbours = new Dictionary<GeoCoordinateSimple, Edge>();
                foreach (var edge in data.GetEdges(sortedVertex))
                {
                    neighbours[data.GetLocation(edge.Neighbour)] = edge.EdgeData;
                }

                for (uint unsortedVertex = 1; unsortedVertex < unsortedData.VertexCount; unsortedVertex++)
                {
                    var unsortedCoordinate = unsortedData.GetLocation(unsortedVertex);
                    if (coordinate.Latitude == unsortedCoordinate.Latitude &&
                        coordinate.Longitude == unsortedCoordinate.Longitude)
                    { // this is the same vertex, all vertices is the test network have different coordinates.
                        // compare their neighbours.
                        var unsortedNeighbours = new Dictionary<GeoCoordinateSimple, Edge>();
                        foreach (var edge in unsortedData.GetEdges(unsortedVertex))
                        {
                            unsortedNeighbours[unsortedData.GetLocation(edge.Neighbour)] = edge.EdgeData;
                        }
                        Assert.AreEqual(neighbours.Count, unsortedNeighbours.Count);
                        foreach (var neighbour in neighbours)
                        {
                            Assert.IsTrue(unsortedNeighbours.ContainsKey(neighbour.Key));
                            var value = unsortedNeighbours[neighbour.Key];
                            Assert.AreEqual(neighbour.Value.Distance, value.Distance);
                            Assert.AreEqual(neighbour.Value.Forward, value.Forward);
                            Assert.AreEqual(neighbour.Value.RepresentsNeighbourRelations, value.RepresentsNeighbourRelations);
                            Assert.AreEqual(neighbour.Value.Tags, value.Tags);
                        }
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// Tests the hilbert sorting function.
        /// </summary>
        [Test]
        public void SortDirectedHilbertRealBigTest()
        {
            var embeddedString = "OsmSharp.Routing.Test.data.test_network_big.osm";
            var n = GraphExtensions.DefaultHilbertSteps;

            // do the data processing without preprocessing.
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();
            var unsortedData = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            var targetData = new CHEdgeGraphOsmStreamTarget(unsortedData, interpreter, tagsIndex, OsmSharp.Routing.Vehicles.Vehicle.Car,
                (g) => { return null; });
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // do the data processing with only hilbert sorting preprocessing.
            interpreter = new OsmRoutingInterpreter();
            tagsIndex = new TagsIndex();
            var data = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            targetData = new CHEdgeGraphOsmStreamTarget(data, interpreter, tagsIndex, OsmSharp.Routing.Vehicles.Vehicle.Car, (g) =>
            {
                return new HilbertSortingPreprocessor<CHEdgeData>(g);
            });
            dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // test if sorted.
            for (uint sortedVertex = 1; sortedVertex < data.VertexCount; sortedVertex++)
            {
                var coordinate = data.GetLocation(sortedVertex);
                var neighbours = new Dictionary<GeoCoordinateSimple, CHEdgeData>();
                foreach (var edge in data.GetEdges(sortedVertex))
                {
                    neighbours[data.GetLocation(edge.Neighbour)] = edge.EdgeData;
                }

                for (uint unsortedVertex = 1; unsortedVertex < unsortedData.VertexCount; unsortedVertex++)
                {
                    var unsortedCoordinate = unsortedData.GetLocation(unsortedVertex);
                    if (coordinate.Latitude == unsortedCoordinate.Latitude &&
                        coordinate.Longitude == unsortedCoordinate.Longitude)
                    { // this is the same vertex, all vertices is the test network have different coordinates.
                        // compare their neighbours.
                        var unsortedNeighbours = new Dictionary<GeoCoordinateSimple, CHEdgeData>();
                        foreach (var edge in unsortedData.GetEdges(unsortedVertex))
                        {
                            unsortedNeighbours[unsortedData.GetLocation(edge.Neighbour)] = edge.EdgeData;
                        }
                        Assert.AreEqual(neighbours.Count, unsortedNeighbours.Count);
                        foreach (var neighbour in neighbours)
                        {
                            Assert.IsTrue(unsortedNeighbours.ContainsKey(neighbour.Key));
                            var value = unsortedNeighbours[neighbour.Key];
                            Assert.AreEqual(neighbour.Value.Weight, value.Weight);
                            Assert.AreEqual(neighbour.Value.Forward, value.Forward);
                            Assert.AreEqual(neighbour.Value.RepresentsNeighbourRelations, value.RepresentsNeighbourRelations);
                            Assert.AreEqual(neighbour.Value.Tags, value.Tags);
                        }
                        break;
                    }
                }
            }
        }
    }
}