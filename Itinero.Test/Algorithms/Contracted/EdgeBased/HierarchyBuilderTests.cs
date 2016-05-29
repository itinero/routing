// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Graphs.Directed;
using Itinero.Algorithms.Contracted.EdgeBased;
using NUnit.Framework;
using System.Linq;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using System.Collections.Generic;
using System;
using Itinero.Graphs;
using Itinero.Algorithms.Default;
using Itinero.Profiles;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the hierarchy builder.
    /// </summary>
    [TestFixture]
    public class HierarchyBuilderTests
    {
        /// <summary>
        /// Tests contracting a graph with 2 vertices.
        /// </summary>
        [Test]
        public void Test2Vertices()
        {
            // build graph.
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, 100f, 0);
            var directedGraph = new DirectedDynamicGraph(2, 1);
            directedGraph.AddEdge(0, 1, 100, null);
            directedGraph.AddEdge(1, 0, 100, null);

            // contract graph.
            Func<uint, IEnumerable<uint[]>> getRestrictions = (v) => Enumerable.Empty<uint[]>();
            Func<ushort, Factor> getFactor = (p) => new Factor() { Direction = 0, Value = 1 };
            var witnessCalculator = new DykstraWitnessCalculator(graph, getFactor, getRestrictions);
            var hierarchyBuilder = new HierarchyBuilder(directedGraph, new EdgeDifferencePriorityCalculator(directedGraph,
                witnessCalculator), witnessCalculator, getRestrictions);
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = directedGraph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edge10 = directedGraph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge10);
        }

        /// <summary>
        /// Tests contracting a graph with 3 vertices.
        /// </summary>
        [Test]
        public void Test3Vertices()
        {
            // build graph.
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, 100f, 0);
            graph.AddEdge(1, 2, 100f, 0);
            var directedGraph = new DirectedDynamicGraph(3, 1);
            directedGraph.AddEdge(0, 1, 100, null);
            directedGraph.AddEdge(1, 0, 100, null);
            directedGraph.AddEdge(1, 2, 100, null);
            directedGraph.AddEdge(2, 1, 100, null);

            // contract graph.
            Func<uint, IEnumerable<uint[]>> getRestrictions = (v) => Enumerable.Empty<uint[]>();
            Func<ushort, Factor> getFactor = (p) => new Factor() { Direction = 0, Value = 1 };
            var witnessCalculator = new DykstraWitnessCalculator(graph, getFactor, getRestrictions);
            var hierarchyBuilder = new HierarchyBuilder(directedGraph, new EdgeDifferencePriorityCalculator(directedGraph,
                witnessCalculator), witnessCalculator, getRestrictions);
            hierarchyBuilder.Run();

            // check edges.
            var edges01 = directedGraph.GetEdgeEnumerator(0).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNotNull(edges01);
            var edge10 = directedGraph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 0);
            Assert.IsNull(edge10);
            var edge12 = directedGraph.GetEdgeEnumerator(1).FirstOrDefault(x => x.Neighbour == 2);
            Assert.IsNotNull(edge12);
            var edges21 = directedGraph.GetEdgeEnumerator(2).FirstOrDefault(x => x.Neighbour == 1);
            Assert.IsNull(edges21);
        }

        /// <summary>
        /// Tests contracting a graph with 5 vertices and a simple restriction.
        /// </summary>
        [Test]
        public void Test5VerticesWithRestriction()
        {
            // build graph.
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            var edge01 = graph.AddEdge(0, 1, 100f, 0);
            var edge12 = graph.AddEdge(1, 2, 100f, 0);
            var edge13 = graph.AddEdge(1, 3, 100f, 0);
            var edge14 = graph.AddEdge(1, 4, 100f, 0);
            var edge23 = graph.AddEdge(2, 3, 100f, 0);
            var directedGraph = new DirectedDynamicGraph(5, 1);
            directedGraph.AddEdge(0, 1, 100, null);
            directedGraph.AddEdge(1, 0, 100, null);
            directedGraph.AddEdge(1, 2, 100, null);
            directedGraph.AddEdge(2, 1, 100, null);
            directedGraph.AddEdge(1, 3, 100, null);
            directedGraph.AddEdge(3, 1, 100, null);
            directedGraph.AddEdge(1, 4, 100, null);
            directedGraph.AddEdge(4, 1, 100, null);
            directedGraph.AddEdge(2, 3, 100, null);
            directedGraph.AddEdge(3, 2, 100, null);

            // contract graph.
            Func<uint, IEnumerable<uint[]>> getRestrictions = (v) =>
            {
                if (v == 0)
                {
                    return new uint[][] {
                        new uint[] { 0, 1, 4 }
                    };
                }
                if (v == 4)
                {
                    return new uint[][] {
                        new uint[] { 4, 1, 0 }
                    };
                }
                return Enumerable.Empty<uint[]>();
            };
            Func<ushort, Factor> getFactor = (p) => new Factor() { Direction = 0, Value = 1 };
            var witnessCalculator = new DykstraWitnessCalculator(graph, getFactor, getRestrictions);
            var hierarchyBuilder = new HierarchyBuilder(directedGraph, new EdgeDifferencePriorityCalculator(directedGraph,
                witnessCalculator), witnessCalculator, getRestrictions);
            hierarchyBuilder.Run();

            // check edges.

            // verify all edges for 0.
            var edges = directedGraph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Neighbour);
            Assert.AreEqual(null, edges.First().Direction());
            Assert.AreEqual(100, edges.First().Weight());

            // verify all edges for 1.
            edges = directedGraph.GetEdgeEnumerator(1);
            Assert.AreEqual(0, edges.Count());

            // verify all edges for 2.
            edges = directedGraph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            var edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(100, edge.Weight());
            edge = edges.First(e => e.Neighbour == 3);
            Assert.AreEqual(3, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(100, edge.Weight());

            // verify all edges for 3.
            edges = directedGraph.GetEdgeEnumerator(3);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(100, edge.Weight());

            // verify all edges for 4.
            edges = directedGraph.GetEdgeEnumerator(4);
            Assert.AreEqual(1, edges.Count());
            edge = edges.First(e => e.Neighbour == 1);
            Assert.AreEqual(1, edge.Neighbour);
            Assert.AreEqual(null, edge.Direction());
            Assert.AreEqual(100, edge.Weight());
        }
    }
}
