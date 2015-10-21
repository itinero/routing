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
using System.Linq;
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // expand edge.
            Assert.Catch<System.Exception>(() =>
                {
                    graph.ExpandEdge(1, 2, new List<uint>(), true, true);
                });
        }

        /// <summary>
        /// Tests expanding an uncontracted edge.
        /// </summary>
        [Test]
        public void TestExpandEdgeNotContracted()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // expand edge.
            var vertices = new List<uint>();
            graph.ExpandEdge(0, 1, vertices, true, true);

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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(0, 2, 100, null, 1);

            // expand edge.
            var vertices = new List<uint>();
            graph.ExpandEdge(0, 2, vertices, true, true);

            // check result.
            Assert.AreEqual(1, vertices.Count);
            Assert.AreEqual(1, vertices[0]);

            // expand edge.
            vertices = new List<uint>();
            graph.ExpandEdge(0, 2, vertices, false, true);

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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 0, 200, null, 1);
            graph.AddEdge(2, 3, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(0, 3, 300, null, 2);

            // expand edge.
            var vertices = new List<uint>();
            graph.ExpandEdge(0, 3, vertices, true, true);

            // check result.
            Assert.AreEqual(2, vertices.Count);
            Assert.AreEqual(1, vertices[0]);
            Assert.AreEqual(2, vertices[1]);

            // expand edge.
            vertices = new List<uint>();
            graph.ExpandEdge(0, 3, vertices, false, true);

            // check result.
            Assert.AreEqual(2, vertices.Count);
            Assert.AreEqual(2, vertices[0]);
            Assert.AreEqual(1, vertices[1]);
        }

        /// <summary>
        /// Test adding or updating an edge.
        /// </summary>
        [Test]
        public void TestAddOrUpdateEdge()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 99, null, Constants.NO_VERTEX);

            // check result.
            var edges = new List<MetaEdge>(graph.GetEdgeEnumerator(0).Where(x => x.Neighbour == 1));
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual(1, edges[0].Neighbour);
            var edgeData = ContractedEdgeDataSerializer.Deserialize(edges[0].Data[0], edges[0].MetaData[0]);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(99, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 101, null, Constants.NO_VERTEX);

            // check result.
            edges = new List<MetaEdge>(graph.GetEdgeEnumerator(0).Where(x => x.Neighbour == 1));
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual(1, edges[0].Neighbour);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edges[0].Data[0], edges[0].MetaData[0]);
            Assert.AreEqual(null, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(100, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 99, true, Constants.NO_VERTEX);

            // check result.
            edges = new List<MetaEdge>(graph.GetEdgeEnumerator(0).Where(x => x.Neighbour == 1));
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual(1, edges[0].Neighbour);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edges[0].Data[0], edges[0].MetaData[0]);
            Assert.AreEqual(true, edgeData.Direction);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(99, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 99, true, Constants.NO_VERTEX);

            // check result.
            var edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 && 
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == false);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(100, edgeData.Weight); 

            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 &&
                 ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == true);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(99, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, false, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 99, true, Constants.NO_VERTEX);

            // check result.
            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 &&
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == false);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(100, edgeData.Weight);

            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 &&
                 ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == true);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(99, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, false, Constants.NO_VERTEX);
            graph.AddEdge(0, 1, 99, true, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 98, null, Constants.NO_VERTEX);

            // check result.
            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 &&
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == null);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(98, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, false, Constants.NO_VERTEX);
            graph.AddEdge(0, 1, 98, true, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 1, 99, null, Constants.NO_VERTEX);

            // check result.
            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 &&
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == false);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(99, edgeData.Weight);

            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 1 &&
                 ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == true);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(98, edgeData.Weight);

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // update.
            graph.AddOrUpdateEdge(0, 2, 99, null, Constants.NO_VERTEX);

            // check result.
            edge = graph.GetEdgeEnumerator(0).First(x => x.Neighbour == 2 &&
                ContractedEdgeDataSerializer.Deserialize(x.Data[0], x.MetaData[0]).Direction == null);
            Assert.IsNotNull(edge);
            edgeData = ContractedEdgeDataSerializer.Deserialize(edge.Data[0], edge.MetaData[0]);
            Assert.AreEqual(Constants.NO_VERTEX, edgeData.ContractedId);
            Assert.AreEqual(99, edgeData.Weight);
        }
    }
}