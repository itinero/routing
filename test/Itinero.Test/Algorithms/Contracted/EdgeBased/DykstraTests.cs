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
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Graphs.Directed;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Network;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the dykstra algorithm.
    /// </summary>
    [TestFixture]
    public class DykstraTests
    {
        /// <summary>
        /// Tests routing on a graph with one edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph, new EdgePath<float>[] { new EdgePath<float>(0) },
                (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);

            // build graph.
            graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(1, 0, 100, null);

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph, new EdgePath<float>[] { new EdgePath<float>(1) },
                (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(1, visit.From.Vertex);
        }

        /// <summary>
        /// Tests routing on a graph with two edges where the middle is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesMiddleHighest()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(2, 1, 100, null);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));
        }

        /// <summary>
        /// Tests routing on a graph with two edges where the right is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesRightHighest()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 2, 100, null);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);
        }

        /// <summary>
        /// Tests routing on a graph with two edges where the left is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesLeftHighest()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(1, 0, 100, null);
            graph.AddEdge(2, 1, 100, null);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));
        }

        /// <summary>
        /// Tests routing on a graph with two oneway edges where the middle is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesDirectedMiddleHighest()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, true);
            graph.AddEdge(2, 1, 100, false);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(2) },
                    (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsFalse(algorithm.TryGetVisit(0, out visit));
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(2, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
        }

        /// <summary>
        /// Tests routing on a graph with two oneway edges where the right is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesRightMiddleHighest()
        {
            // build graph.            
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(0, 1, 100, true);
            graph.AddEdge(1, 2, 100, true);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);
        }

        /// <summary>
        /// Tests routing on a graph with two oneway edges where the left is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesLeftMiddleHighest()
        {
            // build graph.            
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            graph.AddEdge(1, 0, 100, false);
            graph.AddEdge(2, 1, 100, false);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(1) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsFalse(algorithm.TryGetVisit(0, out visit));
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(2) },
                    (v) => null, false);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsFalse(algorithm.TryGetVisit(0, out visit));
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) },
                    (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsFalse(algorithm.TryGetVisit(1, out visit));
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(1) },
                        (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(1, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsFalse(algorithm.TryGetVisit(2, out visit));

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(2) },
                    (v) => null, true);
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsTrue(algorithm.TryGetVisit(0, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(1, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(2, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
        }

        /// <summary>
        /// Tests routing for u-turns.
        /// </summary>
        /// <remarks>
        /// Network:
        ///  (0)----100m----(1)
        ///   \             /
        ///    \           /           
        ///     \         /
        ///     100m    100m
        ///       \     /
        ///        \   /
        ///         (2)
        [Test]
        public void TestUTurn()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Profiles.VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(2, 0, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.AddEdgeBasedContractedForTesting(Profiles.VehicleMock.Car().Fastest());
            Itinero.Data.Contracted.ContractedDb contractedDb;
            routerDb.TryGetContracted(Profiles.VehicleMock.Car().Fastest(), out contractedDb);
            
            var dykstra = new Itinero.Algorithms.Contracted.EdgeBased.Dykstra(contractedDb.EdgeBasedGraph,
                new EdgePath<float>[] { new EdgePath<float>(0, 100 * Profiles.VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, -1, new EdgePath<float>(1)) }, 
                    (i) => null, false);
            dykstra.Run();

            EdgePath<float> path;
            Assert.IsTrue(dykstra.TryGetVisit(0, out path));
            Assert.AreEqual(0, path.Vertex);
            Assert.AreEqual(1, path.From.Vertex);

            Assert.IsTrue(dykstra.TryGetVisit(1, out path));
            Assert.AreEqual(1, path.Vertex);
            Assert.AreEqual(2, path.From.Vertex);
            Assert.AreEqual(0, path.From.From.Vertex);
            Assert.AreEqual(1, path.From.From.From.Vertex);

            Assert.IsTrue(dykstra.TryGetVisit(2, out path));
            Assert.AreEqual(2, path.Vertex);
            Assert.AreEqual(0, path.From.Vertex);
            Assert.AreEqual(1, path.From.From.Vertex);
        }
    }
}