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

using Itinero.Algorithms;
using Itinero.Algorithms.Default.EdgeBased;
using Itinero.Algorithms.Weights;
using Itinero.Data.Edges;
using Itinero.Graphs;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// Contains tests for the directed edge-based dykstra algorithm.
    /// </summary>
    [TestFixture]
    public class DirectedDykstraTests
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
            var edge1 = graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            var edge2 = graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new DirectedDykstra<float>(graph, new DefaultWeightHandler((profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }), new Itinero.Algorithms.Restrictions.RestrictionCollection((c, v) => false), new DirectedEdgeId(edge1, true), float.MaxValue, false);
            dykstra.WasFound = (p, e, w) =>
            {
                if (e.EdgeId == edge1)
                {
                    Assert.AreEqual(true, e.Forward);
                    Assert.AreEqual(0, w);
                }
                else if(e.EdgeId == edge2)
                {
                    Assert.AreEqual(true, e.Forward);
                    Assert.AreEqual(100, w);
                }
                return false;
            };
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
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
            var edge1 = graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(100, 1));
            var edge2 = graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(100, 1));
            var edge3 = graph.AddEdge(2, 3, EdgeDataSerializer.Serialize(100, 1));

            var dykstra = new DirectedDykstra<float>(graph, new DefaultWeightHandler((profile) => new Itinero.Profiles.Factor()
            {
                Direction = 0,
                Value = 1
            }), new Itinero.Algorithms.Restrictions.RestrictionCollection((c, v) => false), new DirectedEdgeId(edge1, true), float.MaxValue, false);
            dykstra.WasFound = (p, e, w) =>
            {
                if (e.EdgeId == edge1)
                {
                    Assert.AreEqual(true, e.Forward);
                    Assert.AreEqual(0, w);
                }
                else if (e.EdgeId == edge2)
                {
                    Assert.AreEqual(true, e.Forward);
                    Assert.AreEqual(100, w);
                }
                else if (e.EdgeId == edge3)
                {
                    Assert.AreEqual(true, e.Forward);
                    Assert.AreEqual(200, w);
                }
                return false;
            };
            dykstra.Run();

            Assert.IsTrue(dykstra.HasRun);
            Assert.IsTrue(dykstra.HasSucceeded);
        }
    }
}