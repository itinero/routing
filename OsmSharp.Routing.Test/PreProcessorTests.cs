// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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
using OsmSharp.Routing.Graphs.PreProcessing;
using System.Linq;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Contains tests for the preprocessor.
    /// </summary>
    [TestFixture]
    public class PreProcessorTests
    {
        /// <summary>
        /// Tests a simple removal of one sparse vertex.
        /// from    1 - 2 - 3
        ///     
        /// to      1 - 3
        /// </summary>
        [Test]
        public void TestSparseRemoval1()
        {
            // use one edge definition everywhere.
            var edge = new Edge();
            edge.Forward = true;
            edge.Tags = 1;

            var graph = new Graph<Edge>();
            uint vertex1 = graph.AddVertex(0, 0);
            uint vertex2 = graph.AddVertex(1, 1);
            uint vertex3 = graph.AddVertex(2, 2);

            graph.AddEdge(vertex1, vertex2, edge, null);
            graph.AddEdge(vertex2, vertex1, edge, null);
            graph.AddEdge(vertex2, vertex3, edge, null);
            graph.AddEdge(vertex3, vertex2, edge, null);

            // execute pre-processor.
            var preProcessor = new GraphSimplificationPreprocessor(graph);
            preProcessor.Start();

            // test resulting graph.
            Assert.AreEqual(2, graph.VertexCount);
            Assert.AreEqual(1, graph.GetEdges(1).ToKeyValuePairs().Length);
            Assert.AreEqual(2, graph.GetEdges(1).ToKeyValuePairs()[0].Key);
            Assert.AreEqual(1, graph.GetEdges(2).ToKeyValuePairs().Length);
            Assert.AreEqual(1, graph.GetEdges(2).ToKeyValuePairs()[0].Key);
        }

        /// <summary>
        /// Tests a simple removal of one sparse vertex.
        /// from    1 - 2 - 3 - 4 - 5
        ///         3 - 6
        ///     
        /// to      1 - 2 - 3
        ///         2 - 4
        /// </summary>
        [Test]
        public void TestSparseRemoval2()
        {
            // use one edge definition everywhere.
            var edge = new Edge();
            edge.Forward = true;
            edge.Tags = 1;

            var graph = new Graph<Edge>();
            uint vertex1 = graph.AddVertex(0, 0);
            uint vertex2 = graph.AddVertex(1, 1);
            uint vertex3 = graph.AddVertex(2, 2);
            uint vertex4 = graph.AddVertex(3, 3);
            uint vertex5 = graph.AddVertex(4, 4);
            uint vertex6 = graph.AddVertex(5, 5);

            graph.AddEdge(vertex1, vertex2, edge, null); // 1 <-> 2
            graph.AddEdge(vertex2, vertex1, edge, null); // 1 <-> 2
            graph.AddEdge(vertex2, vertex3, edge, null); // 2 <-> 3
            graph.AddEdge(vertex3, vertex2, edge, null); // 2 <-> 3
            graph.AddEdge(vertex3, vertex4, edge, null); // 3 <-> 4
            graph.AddEdge(vertex4, vertex3, edge, null); // 3 <-> 4
            graph.AddEdge(vertex4, vertex5, edge, null); // 4 <-> 5
            graph.AddEdge(vertex5, vertex4, edge, null); // 4 <-> 5
            graph.AddEdge(vertex3, vertex6, edge, null); // 3 <-> 6
            graph.AddEdge(vertex6, vertex3, edge, null); // 3 <-> 6

            // execute pre-processor.
            var preProcessor = new GraphSimplificationPreprocessor(graph);
            preProcessor.Start();

            // test resulting graph.
            Assert.AreEqual(4, graph.VertexCount);

            Assert.AreEqual(1, graph.GetEdges(1).ToKeyValuePairs().Length);
            Assert.AreEqual(2, graph.GetEdges(1).ToKeyValuePairs()[0].Key);

            Assert.AreEqual(3, graph.GetEdges(2).ToKeyValuePairs().Length);
            Assert.IsTrue(graph.GetEdges(2).ToKeyValuePairs().Any(x => x.Key == 1));
            Assert.IsTrue(graph.GetEdges(2).ToKeyValuePairs().Any(x => x.Key == 3));
            Assert.IsTrue(graph.GetEdges(2).ToKeyValuePairs().Any(x => x.Key == 4));

            Assert.AreEqual(1, graph.GetEdges(3).ToKeyValuePairs().Length);
            Assert.AreEqual(2, graph.GetEdges(3).ToKeyValuePairs()[0].Key);

            Assert.AreEqual(1, graph.GetEdges(4).ToKeyValuePairs().Length);
            Assert.AreEqual(2, graph.GetEdges(4).ToKeyValuePairs()[0].Key);
        }
    }
}