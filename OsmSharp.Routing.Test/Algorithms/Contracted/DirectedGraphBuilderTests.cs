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
using OsmSharp.Routing.Algorithms.Contracted;
using OsmSharp.Routing.Data;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using OsmSharp.Routing.Profiles;
using System;
using System.Linq;

namespace OsmSharp.Routing.Test.Algorithms.Contracted
{
    /// <summary>
    /// Containts tests for the directed graph builder.
    /// </summary>
    [TestFixture]
    public class DirectedGraphBuilderTests
    {
        /// <summary>
        /// Tests converting a graph with one edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var graph = new OsmSharp.Routing.Graphs.Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Factor> getFactor = (x) =>
            {
                return new Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // convert graph.
            var directedGraph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            var algorithm = new DirectedGraphBuilder(graph, directedGraph, getFactor);
            algorithm.Run();

            // check result.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            directedGraph.Compress();
            Assert.AreEqual(2, directedGraph.VertexCount);
            Assert.AreEqual(2, directedGraph.EdgeCount);

            // verify all edges.
            var edges = directedGraph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count);
            var data = ContractedEdgeDataSerializer.Serialize(100 * getFactor(1).Value, null, Constants.NO_VERTEX);
            Assert.AreEqual(data, edges.First().Data);
            Assert.AreEqual(1, edges.First().Neighbour);
            
            edges = directedGraph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count);
            data = ContractedEdgeDataSerializer.Serialize(100 * getFactor(1).Value, null, Constants.NO_VERTEX);
            Assert.AreEqual(data, edges.First().Data);
            Assert.AreEqual(0, edges.First().Neighbour);
        }
    }
}