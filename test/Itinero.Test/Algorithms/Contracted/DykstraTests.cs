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
using Itinero.Algorithms.Contracted;
using Itinero.Data.Contracted;
using Itinero.Graphs.Directed;
using System.Collections.Generic;
using Itinero.Data.Contracted.Edges;

namespace Itinero.Test.Algorithms.Contracted
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph, new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph, new EdgePath<float>[] { new EdgePath<float>(1) }, false);
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, true);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, true);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, true);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100, false, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(2) }, true);
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, true, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, false, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100, false, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, false);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(1) }, false);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(2) }, false);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(0) }, true);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(1) }, true);
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
            algorithm = new Itinero.Algorithms.Contracted.Dykstra(graph,
                new EdgePath<float>[] { new EdgePath<float>(2) }, true);
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
    }
}