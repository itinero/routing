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

using Itinero.Algorithms;
using Itinero.Algorithms.Default.Edge;
using Itinero.Data;
using Itinero.Data.Edges;
using Itinero.Graphs;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Default.Edge
{
    /// <summary>
    /// Contains tests for the edge-based dykstra algorithm.
    /// </summary>
    [TestFixture]
    public class DykstraTests
    {
        /// <summary>
        /// Tests a simple one-hop.
        /// </summary>
        [Test]
        public void TestOneHop()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new Dykstra(graph, (profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }, null,
            new EdgePath[]
            {
                new EdgePath(0, 50, 1, new EdgePath()),
                new EdgePath(1, 50, -1, new EdgePath())
            }, float.MaxValue, false);
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
            EdgePath visit;
            Assert.IsTrue(dykstra.TryGetVisit(-1, out visit));
            Assert.AreEqual(-1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(1, out visit));
            Assert.AreEqual(1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(2, out visit));
            Assert.AreEqual(2, visit.Edge);
            Assert.AreEqual(150, visit.Weight);
            Assert.IsFalse(dykstra.TryGetVisit(-2, out visit));
        }

        /// <summary>
        /// Tests a simple two-hop.
        /// </summary>
        [Test]
        public void TestTwoHops()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(2, 3, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new Dykstra(graph, (profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }, null, 
            new EdgePath[]
            {
                new EdgePath(0, 50, 1, new EdgePath()),
                new EdgePath(1, 50, -1, new EdgePath())
            }, float.MaxValue, false);
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
            EdgePath visit;
            Assert.IsTrue(dykstra.TryGetVisit(-1, out visit));
            Assert.AreEqual(-1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(1, out visit));
            Assert.AreEqual(1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(2, out visit));
            Assert.AreEqual(2, visit.Edge);
            Assert.AreEqual(150, visit.Weight);
            Assert.IsFalse(dykstra.TryGetVisit(-2, out visit));
            Assert.IsTrue(dykstra.TryGetVisit(3, out visit));
            Assert.AreEqual(3, visit.Edge);
            Assert.AreEqual(250, visit.Weight);
            Assert.IsFalse(dykstra.TryGetVisit(-3, out visit));
        }

        /// <summary>
        /// Tests a simple triangular network.
        /// </summary>
        [Test]
        public void TestTriangle()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(2, 0, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new Dykstra(graph, (profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }, null, 
            new EdgePath[]
            {
                new EdgePath(0, 50, 1, new EdgePath()),
                new EdgePath(1, 50, -1, new EdgePath())
            }, float.MaxValue, false);
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
            EdgePath visit;
            Assert.IsTrue(dykstra.TryGetVisit(-1, out visit));
            Assert.AreEqual(-1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(1, out visit));
            Assert.AreEqual(1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(2, out visit));
            Assert.AreEqual(2, visit.Edge);
            Assert.AreEqual(150, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(-2, out visit));
            Assert.AreEqual(-2, visit.Edge);
            Assert.AreEqual(250, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(3, out visit));
            Assert.AreEqual(3, visit.Edge);
            Assert.AreEqual(250, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(-3, out visit));
            Assert.AreEqual(-3, visit.Edge);
            Assert.AreEqual(150, visit.Weight);
        }

        /// <summary>
        /// Tests a simple one-hop but with a restriction.
        /// </summary>
        [Test]
        public void TestOneHopRestricted()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new Dykstra(graph, (profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }, (vertex) =>
            {
                if(vertex == 1)
                {
                    return new uint [][] { new uint[] { 1 } };
                }
                return null;
            },
            new EdgePath[]
            {
                new EdgePath(0, 50, 1, new EdgePath()),
                new EdgePath(1, 50, -1, new EdgePath())
            }, float.MaxValue, false);
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
            EdgePath visit;
            Assert.IsTrue(dykstra.TryGetVisit(-1, out visit));
            Assert.AreEqual(-1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(1, out visit));
            Assert.AreEqual(1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsFalse(dykstra.TryGetVisit(2, out visit));
            Assert.IsFalse(dykstra.TryGetVisit(-2, out visit));
        }

        /// <summary>
        /// Tests a simple one-hop but with a restriction.
        /// </summary>
        /// <remarks>
        /// (0)--1--(1)--2--(2)
        ///                R |
        ///                  3
        ///                  |
        ///                 (3)
        ///                 
        /// 1->2->3 is restricted, edge 3, -3 and -2 should never be settled.
        /// </remarks>
        [Test]
        public void TestTwoHopRestricted()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(2, 3, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new Dykstra(graph, (profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }, (vertex) =>
            {
                if (vertex == 1)
                {
                    return new uint[][] { new uint[] { 1, 2, 3 } };
                }
                return null;
            },
            new EdgePath[]
            {
                new EdgePath(0, 50, 1, new EdgePath()),
                new EdgePath(1, 50, -1, new EdgePath())
            }, float.MaxValue, false);
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
            EdgePath visit;
            Assert.IsTrue(dykstra.TryGetVisit(-1, out visit));
            Assert.AreEqual(-1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(1, out visit));
            Assert.AreEqual(1, visit.Edge);
            Assert.AreEqual(50, visit.Weight);
            Assert.IsTrue(dykstra.TryGetVisit(2, out visit));
            Assert.AreEqual(2, visit.Edge);
            Assert.AreEqual(150, visit.Weight);
            Assert.IsFalse(dykstra.TryGetVisit(-2, out visit));
            Assert.IsFalse(dykstra.TryGetVisit(3, out visit));
            Assert.IsFalse(dykstra.TryGetVisit(-2, out visit));
        }

        /// <summary>
        /// Tests routing a small network with a turn restriction and a loop.
        /// </summary>
        /// <remarks>
        ///         (3)--- 
        ///          |    \
        ///          5     4
        ///          |      \
        /// (4)--2--(1)--3--(2)
        ///        R |
        ///          1
        ///          |
        ///         (0)
        ///         
        /// Turn 0->1->4 is restricted; route 0->4 should be 0->1->(2->3|3->2)->1->4
        /// </remarks>
        [Test]
        public void TestLoopRestricted()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddVertex(4);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 4, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(2, 3, EdgeDataSerializer.Serialize(100, 1));
            graph.AddEdge(3, 1, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new Dykstra(graph, (profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }, (vertex) =>
            {
                if (vertex == 0)
                {
                    return new uint[][] { new uint[] { 0, 1, 4 } };
                }
                return null;
            },
            new EdgePath[]
            {
                new EdgePath(1, 50, 1, new EdgePath()),
                new EdgePath(0, 50, -1, new EdgePath())
            }, float.MaxValue, false);
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
            EdgePath visit;
            Assert.IsTrue(dykstra.TryGetVisit(-1, out visit) && visit.Weight == 50);
            Assert.IsTrue(dykstra.TryGetVisit(1, out visit) && visit.Weight == 50);
            Assert.IsFalse(dykstra.TryGetVisit(-2, out visit));
            Assert.IsTrue(dykstra.TryGetVisit(2, out visit) && visit.Weight == 450);
            Assert.IsTrue(dykstra.TryGetVisit(-3, out visit) && visit.Weight == 350);
            Assert.IsTrue(dykstra.TryGetVisit(3, out visit) && visit.Weight == 150);
            Assert.IsTrue(dykstra.TryGetVisit(-4, out visit) && visit.Weight == 250);
            Assert.IsTrue(dykstra.TryGetVisit(4, out visit) && visit.Weight == 250);
            Assert.IsTrue(dykstra.TryGetVisit(-5, out visit) && visit.Weight == 150);
            Assert.IsTrue(dykstra.TryGetVisit(4, out visit) && visit.Weight == 250);
        }
    }
}