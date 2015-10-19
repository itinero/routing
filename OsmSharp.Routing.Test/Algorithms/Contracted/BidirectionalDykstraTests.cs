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
using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms.Contracted
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(1) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            Path visit;
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
            graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(1) });
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
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

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = true,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = false,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = true,
                Weight = 100
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = true,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = false,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = false,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(0, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(2, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(3, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 2,
                Direction = null,
                Weight = 200
            }));
            graph.AddEdge(4, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 0,
                Direction = null,
                Weight = 200
            }));
            graph.AddEdge(4, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(1, algorithm.Best);
            Path visit;
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
            algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(1) }, new Path[] { new Path(3) });
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
            algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(3) }, new Path[] { new Path(1) });
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 3,
                Direction = null,
                Weight = 400
            }));
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
            graph.AddEdge(3, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 2,
                Direction = null,
                Weight = 300
            }));
            graph.AddEdge(3, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(4, algorithm.Best);
            Path visit;
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
            graph.AddEdge(3, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 2,
                Direction = null,
                Weight = 300
            }));
            graph.AddEdge(3, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(4, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 3,
                Direction = null,
                Weight = 400
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(0, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 2,
                Direction = null,
                Weight = 400
            }));
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
            graph.AddEdge(2, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 3,
                Direction = null,
                Weight = 200
            }));
            graph.AddEdge(3, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));
            graph.AddEdge(3, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 100
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(4, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 1,
                Direction = null,
                Weight = 30
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 10
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 20
            }));
            graph.AddEdge(2, 3, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 30
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(3) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(3, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(0, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 1,
                Direction = null,
                Weight = 30
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 10
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 20
            }));
            graph.AddEdge(2, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 3,
                Direction = null,
                Weight = 70
            }));
            graph.AddEdge(3, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 30
            }));
            graph.AddEdge(3, 4, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 40
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(4) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(4, algorithm.Best);
            Path visit;
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
            var graph = new DirectedGraph(ContractedEdgeDataSerializer.Size);
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = false,
                Weight = 100
            }));
            graph.AddEdge(1, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 3,
                Direction = true,
                Weight = 10
            }));
            graph.AddEdge(3, 0, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = true,
                Weight = 5
            }));
            graph.AddEdge(3, 1, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = false,
                Weight = 5
            }));
            graph.AddEdge(1, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = Constants.NO_VERTEX,
                Direction = null,
                Weight = 10
            }));
            graph.AddEdge(0, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 1,
                Direction = true,
                Weight = 110
            }));
            graph.AddEdge(0, 2, ContractedEdgeDataSerializer.Serialize(new ContractedEdgeData()
            {
                ContractedId = 1,
                Direction = false,
                Weight = 20
            }));

            // create algorithm and run.
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(0) }, new Path[] { new Path(2) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);

            Assert.AreEqual(new List<uint>(new uint[] { 0, 1, 2 }), algorithm.GetPath());

            // create algorithm and run.
            algorithm = new OsmSharp.Routing.Algorithms.Contracted.BidirectionalDykstra(graph,
                new Path[] { new Path(2) }, new Path[] { new Path(0) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);
            Assert.AreEqual(2, algorithm.Best);

            Assert.AreEqual(new List<uint>(new uint[] { 2, 1, 3, 0 }), algorithm.GetPath());
        }
    }
}