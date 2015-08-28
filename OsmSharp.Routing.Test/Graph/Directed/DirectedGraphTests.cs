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
using OsmSharp.Routing.Graph.Directed;
using System.Linq;

namespace OsmSharp.Routing.Test.Graph.Directed
{
    /// <summary>
    /// Tests the directed graph implementation.
    /// </summary>
    [TestFixture]
    public class DirectedGraphTests
    {
        /// <summary>
        /// Tests adding an edge.
        /// </summary>
        [Test]
        public void TestAddEdge()
        {
            var graph = new DirectedGraph<EdgeDataMock>();
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
            Assert.AreEqual(0, edges.Count());

            // add another edge.
            uint vertex2 = 2;
            graph.AddEdge(vertex1, vertex2, new EdgeDataMock(2));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(2, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex2, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(0, edges.Count());

            // add another edge.
            uint vertex3 = 3;
            graph.AddEdge(vertex1, vertex3, new EdgeDataMock(3));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(0, edges.Count());

            // add another edge but in reverse.
            graph.AddEdge(vertex3, vertex1, new EdgeDataMock(3));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(0, edges.Count());

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
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(3, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(4, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex5, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex5);
            Assert.AreEqual(0, edges.Count());

            // connect the islands.
            graph.AddEdge(vertex5, vertex3, new EdgeDataMock(5));

            // verify all edges.
            edges = graph.GetEdgeEnumerator(vertex0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex1, edges.First().Neighbour);

            edges = graph.GetEdgeEnumerator(vertex1);
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == vertex2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == vertex3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == vertex3).EdgeData.Id);

            edges = graph.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(0, edges.Count());

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
            Assert.AreEqual(5, edges.First().EdgeData.Id);
            Assert.AreEqual(vertex3, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests get edge enumerator.
        /// </summary>
        [Test]
        public void TestGetEdgeEnumerator()
        {
            var graph = new DirectedGraph<EdgeDataMock>();

            // add edges.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(1, 2, new EdgeDataMock(2));
            graph.AddEdge(1, 3, new EdgeDataMock(3));
            graph.AddEdge(3, 4, new EdgeDataMock(4));
            graph.AddEdge(4, 1, new EdgeDataMock(5));
            graph.AddEdge(5, 1, new EdgeDataMock(6));

            // get empty edge enumerator.
            var edges = graph.GetEdgeEnumerator();

            // move to vertices and test result.
            Assert.IsTrue(edges.MoveTo(0));
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().EdgeData.Id);
            Assert.AreEqual(1, edges.First().Neighbour);

            Assert.IsTrue(edges.MoveTo(1));
            Assert.AreEqual(2, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 2));
            Assert.AreEqual(2, edges.First(x => x.Neighbour == 2).EdgeData.Id);
            Assert.IsTrue(edges.Any(x => x.Neighbour == 3));
            Assert.AreEqual(3, edges.First(x => x.Neighbour == 3).EdgeData.Id);

            Assert.IsFalse(edges.MoveTo(2));

            Assert.IsTrue(edges.MoveTo(3));
            Assert.AreEqual(1, edges.Count());
            Assert.IsTrue(edges.Any(x => x.Neighbour == 4));
            Assert.AreEqual(4, edges.First(x => x.Neighbour == 4).EdgeData.Id);

            Assert.IsTrue(edges.MoveTo(4));
            Assert.AreEqual(1, edges.Count());
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
            var graph = new DirectedGraph<EdgeDataMock>();

            // add and remove edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            Assert.AreEqual(0, graph.RemoveEdge(1, 0));
            Assert.AreEqual(1, graph.RemoveEdge(0, 1));

            graph = new DirectedGraph<EdgeDataMock>();

            // add and remove edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 1, new EdgeDataMock(2));
            Assert.AreEqual(2, graph.GetEdgeEnumerator(0).Count);
            Assert.AreEqual(0, graph.RemoveEdge(1, 0));
            Assert.IsTrue(graph.RemoveEdge(0, 1, new EdgeDataMock(1)));
            Assert.AreEqual(1, graph.GetEdgeEnumerator(0).Count);

            graph = new DirectedGraph<EdgeDataMock>();

            // add and remove edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            graph.AddEdge(0, 1, new EdgeDataMock(2));
            Assert.AreEqual(2, graph.GetEdgeEnumerator(0).Count);
            Assert.AreEqual(0, graph.RemoveEdge(1, 0));
            Assert.AreEqual(2, graph.RemoveEdge(0, 1));
            Assert.AreEqual(0, graph.GetEdgeEnumerator(0).Count);
        }

        /// <summary>
        /// Tests remove edges.
        /// </summary>
        [Test]
        public void TestRemoveEdges()
        {
            var graph = new DirectedGraph<EdgeDataMock>();

            // add and remove edge.
            graph.AddEdge(0, 1, new EdgeDataMock(1));
            Assert.AreEqual(0, graph.RemoveEdges(1));
            Assert.AreEqual(1, graph.RemoveEdges(0));

            // verify all edges.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(0, edges.Count());

            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            graph = new DirectedGraph<EdgeDataMock>();

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

            graph = new DirectedGraph<EdgeDataMock>();

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
            Assert.AreEqual(0, edges.Count());
        }
    }
}