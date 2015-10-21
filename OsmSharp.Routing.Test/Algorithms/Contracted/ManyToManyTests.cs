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
using OsmSharp.Routing.Algorithms.Contracted;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms.Contracted
{
    /// <summary>
    /// Contains tests for the many-to-many algorithm.
    /// </summary>
    [TestFixture]
    public class ManyToManyTests
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(2, algorithm.Weights.Length);
            Assert.AreEqual(2, algorithm.Weights[0].Length);
            Assert.AreEqual(2, algorithm.Weights[1].Length);

            Assert.AreEqual(0, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(100, algorithm.Weights[1][0]);
            Assert.AreEqual(0, algorithm.Weights[1][1]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(100, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(200, algorithm.Weights[2][0]);
            Assert.AreEqual(100, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(100, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(200, algorithm.Weights[2][0]);
            Assert.AreEqual(100, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(100, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(200, algorithm.Weights[2][0]);
            Assert.AreEqual(100, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][0]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][0]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][0]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
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
            var algorithm = new OsmSharp.Routing.Algorithms.Contracted.ManyToManyBidirectionalDykstra(graph,
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) },
                        new Path[] { new Path(3) },
                        new Path[] { new Path(4) }}),
                new List<IEnumerable<Path>>(
                    new IEnumerable<Path>[] { 
                        new Path[] { new Path(0) },
                        new Path[] { new Path(1) },
                        new Path[] { new Path(2) },
                        new Path[] { new Path(3) },
                        new Path[] { new Path(4) }}));
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(5, algorithm.Weights.Length);

            Assert.AreEqual(5, algorithm.Weights[0].Length);
            Assert.AreEqual(5, algorithm.Weights[1].Length);
            Assert.AreEqual(5, algorithm.Weights[2].Length);
            Assert.AreEqual(5, algorithm.Weights[3].Length);
            Assert.AreEqual(5, algorithm.Weights[4].Length);

            Assert.AreEqual(000, algorithm.Weights[0][0]);
            Assert.AreEqual(100, algorithm.Weights[0][1]);
            Assert.AreEqual(200, algorithm.Weights[0][2]);
            Assert.AreEqual(200, algorithm.Weights[0][3]);
            Assert.AreEqual(100, algorithm.Weights[0][4]);

            Assert.AreEqual(100, algorithm.Weights[1][0]);
            Assert.AreEqual(000, algorithm.Weights[1][1]);
            Assert.AreEqual(100, algorithm.Weights[1][2]);
            Assert.AreEqual(200, algorithm.Weights[1][3]);
            Assert.AreEqual(200, algorithm.Weights[1][4]);

            Assert.AreEqual(200, algorithm.Weights[2][0]);
            Assert.AreEqual(100, algorithm.Weights[2][1]);
            Assert.AreEqual(000, algorithm.Weights[2][2]);
            Assert.AreEqual(100, algorithm.Weights[2][3]);
            Assert.AreEqual(200, algorithm.Weights[2][4]);
        }
    }
}