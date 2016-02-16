// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Algorithms;
using Itinero.Algorithms.Default;
using Itinero.Data;
using Itinero.Graphs;
using Itinero.Profiles;
using System;

namespace Itinero.Test.Algorithms
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class OneToAllDykstraTests
    {
        /// <summary>
        /// Tests shortest path calculations on just one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m----(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
                {
                    Distance = 100,
                    Profile = 1
                }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Itinero.Profiles.Factor> getFactor = (x) =>
            {
                return new Itinero.Profiles.Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // run algorithm.
            var algorithm = new Dykstra(graph, getFactor, new Path[] { new Path(0) }, 
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(100 / speed, visit.Weight);
        }

        /// <summary>
        /// Tests shortest path calculations with a max value.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m---(1) @ 100km/h
        /// Result:
        ///  Only settle 0 because search is limited to 100m/100km/h
        /// </remarks>
        [Test]
        public void TestMaxValue()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
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

            // run algorithm.
            var algorithm = new Dykstra(graph, getFactor, new Path[] { new Path(0) },
                (100 / speed) / 2, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
        }

        /// <summary>
        /// Tests shortest path calculations on just one edge but with a direction.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)----100m----(1) @ 100km/h
        /// Result:
        ///  - Settle 0 and 1 when direction forward.
        ///  - Only settle 0 when direction backward.
        /// </remarks>
        [Test]
        public void TestOneEdgeOneway()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
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
                    Direction = 1,
                    Value = 1.0f / speed
                };
            };

            // run algorithm.
            var algorithm = new Dykstra(graph, getFactor, new Path[] { new Path(0) },
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(100 / speed, visit.Weight);

            // invert direction.
            getFactor = (x) =>
            {
                return new Factor()
                {
                    Direction = 2,
                    Value = 1.0f / speed
                };
            };

            // run algorithm.
            algorithm = new Dykstra(graph, getFactor, new Path[] { new Path(0) },
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(null, visit.From);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(0, visit.Weight);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
        }

        /// <summary>
        /// Tests shortest path calculations using a given source.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)-X--100m----(1)
        ///   \             /
        ///    \           /           
        ///     \         /
        ///     100m    100m
        ///       \     /
        ///        \   /
        ///         (2)
        /// With x the starting point at 10m from 0 and 90m from 1.
        /// 
        /// Result:
        ///  - Settle 0@10m, 1@90m and 2@110m.
        /// </remarks>
        [Test]
        public void TestSourceBetween()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));
            graph.AddEdge(0, 2, EdgeDataSerializer.Serialize(new EdgeData()
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
                    Direction = 1,
                    Value = 1.0f / speed
                };
            };

            // run algorithm.
            var algorithm = new Dykstra(graph, getFactor, new Path[] { 
                new Path(0, 10 / speed, new Path(uint.MaxValue)),
                new Path(1, 90 / speed, new Path(uint.MaxValue))},
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Path visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(uint.MaxValue, visit.From.Vertex);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(10 / speed, visit.Weight);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(uint.MaxValue, visit.From.Vertex);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(90 / speed, visit.Weight);
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(110 / speed, visit.Weight);
        }
    }
}