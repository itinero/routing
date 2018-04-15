/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using NUnit.Framework;
using Itinero.Algorithms;
using Itinero.Algorithms.Default;
using Itinero.Data;
using Itinero.Graphs;
using Itinero.Profiles;
using System;
using Itinero.Data.Edges;
using System.Collections.Generic;

namespace Itinero.Test.Algorithms
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class DykstraTests
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
            var algorithm = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(0) }, 
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
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
            var algorithm = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(0) },
                (100 / speed) / 2, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
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
            var algorithm = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(0) },
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
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
            algorithm = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(0) },
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
            var algorithm = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { 
                new EdgePath<float>(0, 10 / speed, new EdgePath<float>(uint.MaxValue)),
                new EdgePath<float>(1, 90 / speed, new EdgePath<float>(uint.MaxValue))},
                float.MaxValue, false);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
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

        /// <summary>
        /// Tests edge visit reporting.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)----100m----(1)
        ///   \             /
        ///    \           /           
        ///     \         /
        ///     100m    100m
        ///       \     /
        ///        \   /
        ///         (2)
        /// </remarks>
        [Test]
        public void TestEdgeVisits()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            var e01 = graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            })) + 1;
            var e12 = graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            })) + 1;
            var e02 = graph.AddEdge(0, 2, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            })) + 1;

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
            var reportedEdges = new HashSet<long>();
            var algorithm = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(0) }, float.MaxValue, false);
            algorithm.Visit += (path) =>
            {
                if (path.From == null)
                {
                    return false;
                }

                var v1 = path.From.Vertex;
                var v2 = path.Vertex;
                var w1 = path.From.Weight;
                var w2 = path.Weight;
                var e = path.Edge;
                var edge = graph.GetEdge(e);
                float l;
                ushort p;
                Itinero.Data.Edges.EdgeDataSerializer.Deserialize(edge.Data[0], out l, out p);

                if (v1 == 0 && v2 == 1)
                {
                    Assert.AreEqual(100, l);
                    Assert.AreEqual(e01, e);
                    Assert.AreEqual(0, w1);
                    Assert.AreEqual(100 / speed, w2);
                    reportedEdges.Add(e01);
                }
                else if (v1 == 1 && v2 == 0)
                {
                    Assert.AreEqual(100, l);
                    Assert.AreEqual(-e01, e);
                    Assert.AreEqual(100 / speed, w1);
                    Assert.AreEqual(200 / speed, w2);
                    reportedEdges.Add(-e01);
                }
                else if (v1 == 0 && v2 == 2)
                {
                    Assert.AreEqual(100, l);
                    Assert.AreEqual(e02, e);
                    Assert.AreEqual(0, w1);
                    Assert.AreEqual(100 / speed, w2);
                    reportedEdges.Add(e02);
                }
                else if (v1 == 2 && v2 == 0)
                {
                    Assert.AreEqual(100, l);
                    Assert.AreEqual(-e02, e);
                    Assert.AreEqual(100 / speed, w1);
                    Assert.AreEqual(200 / speed, w2);
                    reportedEdges.Add(-e02);
                }
                else if (v1 == 1 && v2 == 2)
                {
                    Assert.AreEqual(100, l);
                    Assert.AreEqual(e12, e);
                    Assert.AreEqual(100 / speed, w1);
                    Assert.AreEqual(200 / speed, w2);
                    reportedEdges.Add(e12);
                }
                else if (v1 == 2 && v2 == 1)
                {
                    Assert.AreEqual(100, l);
                    Assert.AreEqual(-e12, e);
                    Assert.AreEqual(100 / speed, w1);
                    Assert.AreEqual(200 / speed, w2);
                    reportedEdges.Add(-e12);
                }
                return false;
            };
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(reportedEdges.Contains(e01));
            Assert.IsTrue(reportedEdges.Contains(e02));
        }
    }
}