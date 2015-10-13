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
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms.Contracted
{
    /// <summary>
    /// Contains test for the directed graph extensions.
    /// </summary>
    [TestFixture]
    public class DirectedGraphExtensionsTests
    {
        /// <summary>
        /// Tests expanding an edge that doesn't exists.
        /// </summary>
        [Test]
        public void TestExpandNonExistingEdge()
        {
            // build graph.
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // expand edge.
            Assert.Catch<System.Exception>(() =>
                {
                    graph.ExpandEdge(1, 2, new List<uint>(), true);
                });
        }

        /// <summary>
        /// Tests expanding an uncontracted edge.
        /// </summary>
        [Test]
        public void TestExpandEdgeNotContracted()
        {
            // build graph.
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // expand edge.
            var vertices = new List<uint>();
            graph.ExpandEdge(0, 1, vertices, true);

            // check result.
            Assert.AreEqual(0, vertices.Count);
        }

        /// <summary>
        /// Tests expanding an contracted edge.
        /// </summary>
        [Test]
        public void TestExpandEdgeContracted()
        {
            // build graph.
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(0, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 1,
                Direction = null,
                Weight = 100
            }));

            // expand edge.
            var vertices = new List<uint>();
            graph.ExpandEdge(0, 2, vertices, true);

            // check result.
            Assert.AreEqual(1, vertices.Count);
            Assert.AreEqual(1, vertices[0]);

            // expand edge.
            vertices = new List<uint>();
            graph.ExpandEdge(0, 2, vertices, false);

            // check result.
            Assert.AreEqual(1, vertices.Count);
            Assert.AreEqual(1, vertices[0]);
        }

        /// <summary>
        /// Tests expanding a nested contracted edge.
        /// </summary>
        [Test]
        public void TestExpandNestedEdgeContracted()
        {
            // build graph.
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 1,
                Direction = null,
                Weight = 200
            }));
            graph.AddEdge(2, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(0, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 2,
                Direction = null,
                Weight = 300
            }));

            // expand edge.
            var vertices = new List<uint>();
            graph.ExpandEdge(0, 3, vertices, true);

            // check result.
            Assert.AreEqual(2, vertices.Count);
            Assert.AreEqual(1, vertices[0]);
            Assert.AreEqual(2, vertices[1]);

            // expand edge.
            vertices = new List<uint>();
            graph.ExpandEdge(0, 3, vertices, false);

            // check result.
            Assert.AreEqual(2, vertices.Count);
            Assert.AreEqual(2, vertices[0]);
            Assert.AreEqual(1, vertices[1]);
        }
    }
}