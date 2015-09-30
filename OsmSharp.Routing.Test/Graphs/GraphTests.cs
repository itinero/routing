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
using OsmSharp.Routing.Graphs;
using System;
using System.Linq;

namespace OsmSharp.Routing.Test.Graphs
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
        public void TestArgumentExceptions()
        {
            // create graph with one vertex and start adding vertex2.
            var graph = new Graph(1, 2);
            uint vertex0 = 0;
            uint vertex1 = 1;
            uint vertex2 = 2;

            // make sure to add vertex1 and vertex2.
            Assert.Catch<ArgumentException>(() =>
            {
                graph.AddEdge(vertex0, vertex1, 1);
            });
            
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(vertex0, vertex1, 1);
            Assert.Catch<ArgumentOutOfRangeException>(() =>
            {
                graph.GetEdgeEnumerator(vertex2);
            });
        }

        /// <summary>
        /// Tests adding a vertex.
        /// </summary>
        [Test]
        public void TestAddVertex()
        {
            var graph = new Graph(1, 2);

            graph.AddVertex(0);
            Assert.IsTrue(graph.HasVertex(0));

            graph.AddVertex(1);
            Assert.IsTrue(graph.HasVertex(1));

            Assert.IsFalse(graph.HasVertex(1000000));
        }

        /// <summary>
        /// Tests removing a vertex.
        /// </summary>
        [Test]
        public void TestRemoveVertex()
        {
            var graph = new Graph(1, 2);

            graph.AddVertex(0);
            Assert.IsTrue(graph.RemoveVertex(0));
            Assert.IsFalse(graph.HasVertex(0));
            graph.AddVertex(1);
            Assert.IsTrue(graph.RemoveVertex(1));
            Assert.IsFalse(graph.HasVertex(1));

            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);
            Assert.IsTrue(graph.RemoveVertex(0));
            Assert.IsFalse(graph.HasVertex(0));
            Assert.IsTrue(graph.HasVertex(1));

            Assert.IsFalse(graph.RemoveVertex(1000000));

            graph = new Graph(1, 5);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(4);

            Assert.IsTrue(graph.RemoveVertex(4));
            Assert.AreEqual(2, graph.VertexCount);

            Assert.IsTrue(graph.RemoveVertex(0));
            Assert.IsTrue(graph.RemoveVertex(1));
            Assert.AreEqual(0, graph.VertexCount);
        }

        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestAddEdge()
        {
            var graph = new Graph(1, 2);
            uint vertex0 = 0;
            uint vertex1 = 1;

            // add edge.
            graph.AddVertex(vertex0);
            graph.AddVertex(vertex1);
            var edgeId1 = graph.AddEdge(vertex0, vertex1, 1);
            Assert.AreEqual(0, edgeId1);

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex0, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex1, edges.First().From);
            Assert.AreEqual(vertex0, edges.First().To);

            // add another edge.
            uint vertex2 = 2;
            graph.AddVertex(vertex2);
            var edgeId2 = graph.AddEdge(vertex1, vertex2, 2);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex0, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex0));
            Assert.AreEqual(true, edges.First(x => x.To == vertex0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == vertex0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == vertex0).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex0).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex2));
            Assert.AreEqual(2, edges.First(x => x.To == vertex2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == vertex2).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex2).From);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(2, edges.First().Data[0]);
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(vertex2, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            // add another edge.
            uint vertex3 = 3;
            graph.AddVertex(vertex3);
            var edgeId3 = graph.AddEdge(vertex1, vertex3, 3);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex0, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex0));
            Assert.AreEqual(true, edges.First(x => x.To == vertex0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == vertex0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == vertex0).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex0).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex2));
            Assert.AreEqual(2, edges.First(x => x.To == vertex2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == vertex2).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex2).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex3));
            Assert.AreEqual(3, edges.First(x => x.To == vertex3).Data[0]);
            Assert.AreEqual(edgeId3, edges.First(x => x.To == vertex3).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex3).From);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(2, edges.First().Data[0]);
            Assert.AreEqual(vertex2, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(3, edges.First().Data[0]);
            Assert.AreEqual(edgeId3, edges.First().Id);
            Assert.AreEqual(vertex3, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            // overwrite another edge but in reverse.
            var edgeId4 = graph.AddEdge(vertex3, vertex1, 3);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex0, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex0));
            Assert.AreEqual(true, edges.First(x => x.To == vertex0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == vertex0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == vertex0).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex0).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex2));
            Assert.AreEqual(2, edges.First(x => x.To == vertex2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == vertex2).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex2).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex3));
            Assert.AreEqual(true, edges.First(x => x.To == vertex3).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == vertex3).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == vertex3).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex3).From);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(2, edges.First().Data[0]);
            Assert.AreEqual(vertex2, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().Data[0]);
            Assert.AreEqual(edgeId4, edges.First().Id);
            Assert.AreEqual(vertex3, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            // add another edge and start a new island.
            uint vertex4 = 4;
            uint vertex5 = 5;
            graph.AddVertex(vertex4);
            graph.AddVertex(vertex5);
            var edge5Id = graph.AddEdge(vertex4, vertex5, 4);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex0, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex0));
            Assert.AreEqual(true, edges.First(x => x.To == vertex0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == vertex0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == vertex0).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex0).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex2));
            Assert.AreEqual(2, edges.First(x => x.To == vertex2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == vertex2).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex2).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex3));
            Assert.AreEqual(true, edges.First(x => x.To == vertex3).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == vertex3).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == vertex3).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex3).From);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(2, edges.First().Data[0]);
            Assert.AreEqual(vertex2, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().Data[0]);
            Assert.AreEqual(edgeId4, edges.First().Id);
            Assert.AreEqual(vertex3, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(4, edges.First().Data[0]);
            Assert.AreEqual(edge5Id, edges.First().Id);
            Assert.AreEqual(vertex4, edges.First().From);
            Assert.AreEqual(vertex5, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(4, edges.First().Data[0]);
            Assert.AreEqual(edge5Id, edges.First().Id);
            Assert.AreEqual(vertex5, edges.First().From);
            Assert.AreEqual(vertex4, edges.First().To);

            // connect the islands.
            var edgeId5 = graph.AddEdge(vertex5, vertex3, 5);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(edgeId1, edges.First().Id);
            Assert.AreEqual(vertex0, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(3, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex0));
            Assert.AreEqual(true, edges.First(x => x.To == vertex0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == vertex0).Data[0]);
            Assert.AreEqual(edgeId1, edges.First(x => x.To == vertex0).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex0).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex2));
            Assert.AreEqual(2, edges.First(x => x.To == vertex2).Data[0]);
            Assert.AreEqual(edgeId2, edges.First(x => x.To == vertex2).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex2).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex3));
            Assert.AreEqual(true, edges.First(x => x.To == vertex3).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == vertex3).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == vertex3).Id);
            Assert.AreEqual(vertex1, edges.First(x => x.To == vertex3).From);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(edgeId2, edges.First().Id);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(2, edges.First().Data[0]);
            Assert.AreEqual(vertex2, edges.First().From);
            Assert.AreEqual(vertex1, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex1));
            Assert.AreEqual(3, edges.First(x => x.To == vertex1).Data[0]);
            Assert.AreEqual(edgeId4, edges.First(x => x.To == vertex1).Id);
            Assert.AreEqual(vertex3, edges.First(x => x.To == vertex1).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex5));
            Assert.AreEqual(true, edges.First(x => x.To == vertex5).DataInverted);
            Assert.AreEqual(5, edges.First(x => x.To == vertex5).Data[0]);
            Assert.AreEqual(edgeId5, edges.First(x => x.To == vertex5).Id);
            Assert.AreEqual(vertex3, edges.First(x => x.To == vertex5).From);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(4, edges.First().Data[0]);
            Assert.AreEqual(edge5Id, edges.First().Id);
            Assert.AreEqual(vertex4, edges.First().From);
            Assert.AreEqual(vertex5, edges.First().To);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == vertex4));
            Assert.AreEqual(true, edges.First(x => x.To == vertex4).DataInverted);
            Assert.AreEqual(4, edges.First(x => x.To == vertex4).Data[0]);
            Assert.AreEqual(edge5Id, edges.First(x => x.To == vertex4).Id);
            Assert.AreEqual(vertex5, edges.First(x => x.To == vertex4).From);
            Assert.IsTrue(edges.Any(x => x.To == vertex3));
            Assert.AreEqual(5, edges.First(x => x.To == vertex3).Data[0]);
            Assert.AreEqual(edgeId5, edges.First(x => x.To == vertex3).Id);
            Assert.AreEqual(vertex5, edges.First(x => x.To == vertex3).From);
        }

        /// <summary>
        /// Tests get edge enumerator.
        /// </summary>
        [Test]
        public void TestGetEdgeEnumerator()
        {
            var graph = new Graph(1, 10);

            // add edges.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddVertex(5);
            var edge1 = graph.AddEdge(0, 1, 1);
            var edge2 = graph.AddEdge(1, 2, 2);
            var edge3 = graph.AddEdge(1, 3, 3);
            var edge4 = graph.AddEdge(3, 4, 4);
            var edge5 = graph.AddEdge(4, 1, 5);
            var edge6 = graph.AddEdge(5, 1, 6);

            // get empty edge enumerator.
            var edges = graph.GetEdgeEnumerator();
            Assert.IsFalse(edges.HasData);

            // move to vertices and test result.
            Assert.IsTrue(edges.MoveTo(0));
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().To);

            Assert.IsTrue(edges.MoveTo(1));
            Assert.AreEqual(5, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 0));
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(edge1, edges.First(x => x.To == 0).Id);
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(2, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(edge2, edges.First(x => x.To == 2).Id);
            Assert.IsTrue(edges.Any(x => x.To == 3));
            Assert.AreEqual(3, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(edge3, edges.First(x => x.To == 3).Id);
            Assert.IsTrue(edges.Any(x => x.To == 4));
            Assert.AreEqual(true, edges.First(x => x.To == 4).DataInverted);
            Assert.AreEqual(5, edges.First(x => x.To == 4).Data[0]);
            Assert.AreEqual(edge5, edges.First(x => x.To == 4).Id);
            Assert.IsTrue(edges.Any(x => x.To == 5));
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(6, edges.First(x => x.To == 5).Data[0]);
            Assert.AreEqual(edge6, edges.First(x => x.To == 5).Id);

            Assert.IsTrue(edges.MoveTo(2));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(2, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge2, edges.First(x => x.To == 1).Id);

            Assert.IsTrue(edges.MoveTo(3));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge3, edges.First(x => x.To == 1).Id);
            Assert.IsTrue(edges.Any(x => x.To == 4));
            Assert.AreEqual(4, edges.First(x => x.To == 4).Data[0]);
            Assert.AreEqual(edge4, edges.First(x => x.To == 4).Id);

            Assert.IsTrue(edges.MoveTo(4));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 3));
            Assert.AreEqual(true, edges.First(x => x.To == 3).DataInverted);
            Assert.AreEqual(4, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(edge4, edges.First(x => x.To == 3).Id);
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(5, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge5, edges.First(x => x.To == 1).Id);

            Assert.IsTrue(edges.MoveTo(5));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 1));
            Assert.AreEqual(6, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(edge6, edges.First(x => x.To == 1).Id);
        }

        /// <summary>
        /// Tests get edge.
        /// </summary>
        [Test]
        public void TestGetEdge()
        {
            var graph = new Graph(1, 10);

            // add edges.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddVertex(5);
            var edge1 = graph.AddEdge(0, 1, 1);
            var edge2 = graph.AddEdge(1, 2, 2);
            var edge3 = graph.AddEdge(1, 3, 3);
            var edge4 = graph.AddEdge(3, 4, 4);
            var edge5 = graph.AddEdge(4, 1, 5);
            var edge6 = graph.AddEdge(5, 1, 6);

            // get edges one by one.
            var edge1Details = graph.GetEdge(edge1);
            Assert.AreEqual(edge1, edge1Details.Id);
            Assert.AreEqual(0, edge1Details.From);
            Assert.AreEqual(1, edge1Details.To);
            Assert.AreEqual(1, edge1Details.Data[0]);
            var edge2Details = graph.GetEdge(edge2);
            Assert.AreEqual(edge2, edge2Details.Id);
            Assert.AreEqual(1, edge2Details.From);
            Assert.AreEqual(2, edge2Details.To);
            Assert.AreEqual(2, edge2Details.Data[0]);
            var edge3Details = graph.GetEdge(edge3);
            Assert.AreEqual(edge3, edge3Details.Id);
            Assert.AreEqual(1, edge3Details.From);
            Assert.AreEqual(3, edge3Details.To);
            Assert.AreEqual(3, edge3Details.Data[0]);
            var edge4Details = graph.GetEdge(edge4);
            Assert.AreEqual(edge4, edge4Details.Id);
            Assert.AreEqual(3, edge4Details.From);
            Assert.AreEqual(4, edge4Details.To);
            Assert.AreEqual(4, edge4Details.Data[0]);
            var edge5Details = graph.GetEdge(edge5);
            Assert.AreEqual(edge5, edge5Details.Id);
            Assert.AreEqual(4, edge5Details.From);
            Assert.AreEqual(1, edge5Details.To);
            Assert.AreEqual(5, edge5Details.Data[0]);
            var edge6Details = graph.GetEdge(edge6);
            Assert.AreEqual(edge6, edge6Details.Id);
            Assert.AreEqual(5, edge6Details.From);
            Assert.AreEqual(1, edge6Details.To);
            Assert.AreEqual(6, edge6Details.Data[0]);
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdge()
        {
            var graph = new Graph(1, 10);

            // add and remove edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);
            Assert.IsTrue(graph.RemoveEdge(0, 1));

            graph = new Graph(1, 10);

            // add and remove edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            var edge = graph.AddEdge(0, 1, 1);
            Assert.IsTrue(graph.RemoveEdge(edge));
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdges()
        {
            var graph = new Graph(1, 10);

            // add and remove edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);
            Assert.AreEqual(1, graph.RemoveEdges(0));
            Assert.AreEqual(0, graph.RemoveEdges(1));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            graph = new Graph(1, 10);

            // add and remove edges.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 2, 1);
            Assert.AreEqual(2, graph.RemoveEdges(0));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(0, edges.Count());

            graph = new Graph(1, 10);

            // add and remove edges.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 2, 2);
            graph.AddEdge(1, 2, 3);
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
            var graph = new Graph(1, 10);

            // add edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);

            // trim.
            graph.Trim();
            Assert.AreEqual(2, graph.VertexCount);

            graph = new Graph(1, 10);

            // add edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(11001);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 11001, 1);

            // trim.
            graph.Trim();
            Assert.AreEqual(11002, graph.VertexCount);

            graph = new Graph(1, 10);

            // trim.
            graph.Trim(); // keep minimum one vertex.
            Assert.AreEqual(0, graph.VertexCount);
        }

        /// <summary>
        /// Tests the edge count.
        /// </summary>
        [Test]
        public void TestEdgeCount()
        {
            var graph = new Graph(1, 10);

            // add edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);
            Assert.AreEqual(1, graph.EdgeCount);

            graph = new Graph(1, 10);

            // add edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(11001);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 11001, 1);
            Assert.AreEqual(2, graph.EdgeCount);

            graph.AddEdge(0, 11001, 2);
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
            var graph = new Graph(1, 10);

            // add and compress.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);
            graph.Compress();

            Assert.AreEqual(2, graph.VertexCount);
            Assert.AreEqual(1, graph.EdgeCount);

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(0, edges.First().To);

            graph = new Graph(1, 10);

            // add and compress.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(1, 2, 2);
            graph.AddEdge(2, 3, 3);
            graph.AddEdge(3, 4, 4);
            graph.RemoveEdge(1, 2);
            graph.Compress();

            Assert.AreEqual(5, graph.VertexCount);
            Assert.AreEqual(3, graph.EdgeCount);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().To);

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(1, edges.First().Data[0]);
            Assert.AreEqual(0, edges.First().To);

            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().Data[0]);
            Assert.AreEqual(3, edges.First().To);

            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.To == 2));
            Assert.AreEqual(true, edges.First(x => x.To == 2).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == 2).Data[0]);
            Assert.IsTrue(edges.Any(x => x.To == 4));
            Assert.AreEqual(4, edges.First(x => x.To == 4).Data[0]);

            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(4, edges.First().Data[0]);
            Assert.AreEqual(3, edges.First().To);
        }

        /// <summary>
        /// Tests the serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            var graph = new Graph(1, 10);

            // add one edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);

            // serialize.
            graph.Compress();
            var expectedSize = 8 + 8 + 4 + 4 + // the header: two longs representing vertex and edge count and one int for edge size and one for vertex size.
                graph.VertexCount * 4 + // the bytes for the vertex-index: 2 vertices, pointing to 0.
                graph.EdgeCount * 4 * (4 + 1); // the bytes for the one edge: one edge = 4 uints + edge data size.
            using (var stream = new System.IO.MemoryStream())
            {
                Assert.AreEqual(expectedSize, graph.Serialize(stream));
            }

            graph = new Graph(1, 10);

            // add one edge.
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddVertex(5);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 2, 2);
            graph.AddEdge(0, 3, 3);
            graph.AddEdge(0, 4, 4);
            graph.AddEdge(5, 1, 5);
            graph.AddEdge(5, 2, 6);
            graph.AddEdge(5, 3, 7);
            graph.AddEdge(5, 4, 8);

            // serialize.
            graph.Compress();
            expectedSize = 8 + 8 + 4 + 4 + // the header: two longs representing vertex and edge count and one int for edge size and one for vertex size.
                graph.VertexCount * 4 + // the bytes for the vertex-index: 2 vertices, pointing to 0.
                graph.EdgeCount * 4 * (4 + 1); // the bytes for the one edge: one edge = 4 uints and edge data size.
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
            var graph = new Graph(1, 10);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);

            // serialize.
            using (var stream = new System.IO.MemoryStream())
            {
                graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = Graph.Deserialize(stream, false);

                Assert.AreEqual(2, deserializedGraph.VertexCount);
                Assert.AreEqual(1, deserializedGraph.EdgeCount);

                // verify all edges.
                var edges = deserializedGraph.GetEdgeEnumerator(0);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(1, edges.First().Data[0]);
                Assert.AreEqual(1, edges.First().To);

                edges = deserializedGraph.GetEdgeEnumerator(1);
                Assert.AreEqual(1, edges.Count());
                Assert.AreEqual(true, edges.First().DataInverted);
                Assert.AreEqual(1, edges.First().Data[0]);
                Assert.AreEqual(0, edges.First().To);
            }

            graph = new Graph(1, 10);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddVertex(5);
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
                graph.Serialize(stream);

                stream.Seek(0, System.IO.SeekOrigin.Begin);

                var deserializedGraph = Graph.Deserialize(stream, false);

                Assert.AreEqual(6, deserializedGraph.VertexCount);
                Assert.AreEqual(8, deserializedGraph.EdgeCount);
            }
        }

        /// <summary>
        /// Tests switching two vertices.
        /// </summary>
        [Test]
        public void TestSwitch()
        {
            var graph = new Graph(1, 10);

            Assert.Catch<ArgumentException>(() => 
            {
                graph.Switch(0, 1);
            });

            graph = new Graph(1, 10);
            graph.AddVertex(0);
            graph.AddVertex(1);

            graph.Switch(0, 1);

            graph.HasVertex(0);
            graph.HasVertex(1);

            graph = new Graph(1, 10);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 1);

            graph.Switch(0, 1);

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().To);
            Assert.AreEqual(true, edges.First().DataInverted);
            Assert.AreEqual(1, edges.First().Data[0]);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(0, edges.First().To);
            Assert.AreEqual(1, edges.First().Data[0]);

            graph = new Graph(1, 10);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddVertex(5);
            graph.AddEdge(0, 1, 1);
            graph.AddEdge(0, 2, 2);
            graph.AddEdge(0, 3, 3);
            graph.AddEdge(0, 4, 4);
            graph.AddEdge(5, 1, 5);
            graph.AddEdge(5, 2, 6);
            graph.AddEdge(5, 3, 7);
            graph.AddEdge(5, 4, 8);

            graph.Switch(0, 1);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(5, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(4, edges.Count());
            Assert.AreEqual(1, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(2, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(3, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(4, edges.First(x => x.To == 4).Data[0]);
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(2, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(6, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(7, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 1).DataInverted);
            Assert.AreEqual(4, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(8, edges.First(x => x.To == 5).Data[0]);

            graph.Switch(0, 1);

            // verify all edges.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(4, edges.Count());
            Assert.AreEqual(1, edges.First(x => x.To == 1).Data[0]);
            Assert.AreEqual(2, edges.First(x => x.To == 2).Data[0]);
            Assert.AreEqual(3, edges.First(x => x.To == 3).Data[0]);
            Assert.AreEqual(4, edges.First(x => x.To == 4).Data[0]);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(1, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(5, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(2, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(6, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(3);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(3, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(7, edges.First(x => x.To == 5).Data[0]);
            edges = graph.GetEdgeEnumerator(4);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(true, edges.First(x => x.To == 0).DataInverted);
            Assert.AreEqual(4, edges.First(x => x.To == 0).Data[0]);
            Assert.AreEqual(true, edges.First(x => x.To == 5).DataInverted);
            Assert.AreEqual(8, edges.First(x => x.To == 5).Data[0]);
        }
    }
}