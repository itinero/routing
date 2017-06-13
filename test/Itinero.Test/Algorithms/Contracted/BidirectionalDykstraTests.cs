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
    /// Contains tests for the bidirectional dykstra algorithm.
    /// </summary>
    [TestFixture]
    public class BidirectionalDykstraTests
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(1) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNull(visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1 }), algorithm.GetPath());

            // build graph.
            graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(1) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNull(visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(0, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);
            Assert.AreEqual(new List<uint>(new uint[] { 0, 1 }), algorithm.GetPath());
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetForwardVisit(2, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(0, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetForwardVisit(2, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());
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
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(0, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(1, visit.From.Vertex);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests some routes on a pentagon.
        /// </summary>
        [Test]
        public void TestPentagon()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(0, 4, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 3, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(3, 1, 200, null, 2);
            graph.AddEdge(4, 1, 200, null, 0);
            graph.AddEdge(4, 3, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(2, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(1) }, new EdgePath<float>[] { new EdgePath<float>(3) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);

            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(3, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(3, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(3, visit.Vertex);
            Assert.IsNull(visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 1, 2, 3 }), algorithm.GetPath());

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(3) }, new EdgePath<float>[] { new EdgePath<float>(1) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);

            Assert.IsTrue(algorithm.TryGetForwardVisit(1, out visit));
            Assert.AreEqual(200, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(3, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetForwardVisit(3, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(3, visit.Vertex);
            Assert.IsNull(visit.From);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(1, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(1, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 3, 2, 1 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests routing with multiple levels of hierarchy.
        /// </summary>
        [Test]
        public void TestMultipleLevelHiearchy1()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 4, 400, null, 3);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 0, 200, null, 1);
            graph.AddEdge(2, 3, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(3, 0, 300, null, 2);
            graph.AddEdge(3, 4, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(4, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(4, out visit));
            Assert.AreEqual(400, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(4, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2, 3, 4 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests routing with multiple levels of hierarchy.
        /// </summary>
        [Test]
        public void TestMultipleLevelHiearchy2()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 0, 200, null, 1);
            graph.AddEdge(2, 3, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(3, 0, 300, null, 2);
            graph.AddEdge(3, 4, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(4, 0, 400, null, 3);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(4, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.IsTrue(algorithm.TryGetBackwardVisit(0, out visit));
            Assert.AreEqual(400, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(4, visit.From.Vertex);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2, 3, 4 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests routing with multiple levels of hierarchy.
        /// </summary>
        [Test]
        public void TestMultipleLevelHiearchy3()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 4, 400, null, 2);
            graph.AddEdge(1, 0, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 0, 200, null, 1);
            graph.AddEdge(2, 4, 200, null, 3);
            graph.AddEdge(3, 2, 100, null, Constants.NO_VERTEX);
            graph.AddEdge(3, 4, 100, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(4, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(4, out visit));
            Assert.AreEqual(400, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(4, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2, 3, 4 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests routing with multiple levels of hierarchy.
        /// </summary>
        [Test]
        public void TestMultipleLevelHiearchy4()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 2, 30, null, 1);
            graph.AddEdge(1, 0, 10, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 20, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 3, 30, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(3) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(3, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(2, out visit));
            Assert.AreEqual(30, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetForwardVisit(3, out visit));
            Assert.AreEqual(60, visit.Weight);
            Assert.AreEqual(3, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(3, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(3, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2, 3 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests routing with multiple levels of hierarchy.
        /// </summary>
        [Test]
        public void TestMultipleLevelHiearchy5()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 2, 30, null, 1);
            graph.AddEdge(1, 0, 10, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 20, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 4, 70, null, 3);
            graph.AddEdge(3, 2, 30, null, Constants.NO_VERTEX);
            graph.AddEdge(3, 4, 40, null, Constants.NO_VERTEX);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(4, algorithm.Best);
            EdgePath<float> visit;
            Assert.IsTrue(algorithm.TryGetForwardVisit(0, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(0, visit.Vertex);
            Assert.AreEqual(null, visit.From);
            Assert.IsTrue(algorithm.TryGetForwardVisit(2, out visit));
            Assert.AreEqual(30, visit.Weight);
            Assert.AreEqual(2, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(0, visit.From.Vertex);
            Assert.IsTrue(algorithm.TryGetForwardVisit(4, out visit));
            Assert.AreEqual(100, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.IsNotNull(visit.From);
            Assert.AreEqual(2, visit.From.Vertex);

            Assert.IsTrue(algorithm.TryGetBackwardVisit(4, out visit));
            Assert.AreEqual(0, visit.Weight);
            Assert.AreEqual(4, visit.Vertex);
            Assert.AreEqual(null, visit.From);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2, 3, 4 }), algorithm.GetPath());
        }

        /// <summary>
        /// Tests routing with part of the route shorter but in opposite direction.
        /// </summary>
        [Test]
        public void TestShorterEdgeOpposite()
        {
            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100, false, Constants.NO_VERTEX);
            graph.AddEdge(1, 0, 10, true, 3);
            graph.AddEdge(3, 0, 5, true, Constants.NO_VERTEX);
            graph.AddEdge(3, 1, 5, false, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 10, null, Constants.NO_VERTEX);
            graph.AddEdge(0, 2, 110, true, 1);
            graph.AddEdge(0, 2, 20, false, 1);

            // create algorithm and run.
            var algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(0) }, new EdgePath<float>[] { new EdgePath<float>(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());

            // create algorithm and run.
            algorithm = new Itinero.Algorithms.Contracted.BidirectionalDykstra(graph, null,
                new EdgePath<float>[] { new EdgePath<float>(2) }, new EdgePath<float>[] { new EdgePath<float>(0) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);

            Assert.AreEqual(new List<uint>(new uint[] { 2, 1, 3, 0 }), algorithm.GetPath());
        }
    }
}