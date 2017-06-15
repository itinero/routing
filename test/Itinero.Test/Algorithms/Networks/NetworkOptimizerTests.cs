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
using Itinero.Algorithms.Networks;
using Itinero.Data.Network;
using Itinero.Data.Network.Edges;
using System.Linq;
using System.Collections.Generic;
using Itinero.LocalGeo;

namespace Itinero.Test.Algorithms.Networks
{
    /// <summary>
    /// Contains tests for the network optimizer.
    /// </summary>
    [TestFixture]
    public class NetworkOptimizerTests
    {
        /// <summary>
        /// Default delegate to merge edges for these tests.
        /// </summary>
        static NetworkOptimizer.MergeDelegate MergeDelegate = (EdgeData edgeData1, bool inverted1,
                EdgeData edgeData2, bool inverted2, out EdgeData edgeData, out bool inverted) =>
            {
                if (inverted2)
                {
                    edgeData = new EdgeData()
                    {
                        Distance = edgeData1.Distance + edgeData2.Distance,
                        MetaId = edgeData2.MetaId,
                        Profile = edgeData2.Profile
                    };
                    inverted = true;
                }
                else
                {
                    edgeData = new EdgeData()
                    {
                        Distance = edgeData1.Distance + edgeData2.Distance,
                        MetaId = edgeData2.MetaId,
                        Profile = edgeData2.Profile
                    };
                    inverted = false;
                }
                return true;
            };

        /// <summary>
        /// Tests two identical edges.
        /// </summary>
        [Test]
        public void TestTwoIdenticalEdges()
        {
            // create graph with one vertex and start adding 2.
            var graph = new RoutingNetwork(new Itinero.Graphs.Geometric.GeometricGraph(1, 2));

            // make sure to add 1 and 2.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            graph.AddEdge(0, 1, new EdgeData() { Profile = 1, MetaId = 1, Distance = 10 }, null);
            graph.AddEdge(1, 2, new EdgeData() { Profile = 1, MetaId = 1, Distance = 20 }, null);

            // execute algorithm.
            var algorithm = new NetworkOptimizer(graph, (v) => false, MergeDelegate);
            algorithm.Run();

            // check result.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data.Profile);
            Assert.AreEqual(1, edges.First().Data.MetaId);
            Assert.AreEqual(30, edges.First().Data.Distance);
            var shape = edges.Shape;
            Assert.IsNotNull(shape);
            Assert.AreEqual(1, shape.Count);
            Assert.AreEqual(1, shape.First().Latitude);
            Assert.AreEqual(1, shape.First().Longitude);
        }

        /// <summary>
        /// Tests two identical edges but merging would overwrite an existing edge.
        /// </summary>
        [Test]
        public void TestThreeEdges()
        {
            var graph = new RoutingNetwork(new Itinero.Graphs.Geometric.GeometricGraph(1, 2));
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            graph.AddEdge(0, 1, new EdgeData() { Profile = 2 }, null);
            graph.AddEdge(0, 2, new EdgeData() { Profile = 1 }, null);
            graph.AddEdge(2, 1, new EdgeData() { Profile = 1 }, null);

            // execute algorithm.
            var algorithm = new NetworkOptimizer(graph, (v) => false, MergeDelegate);
            algorithm.Run();

            // check result.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(2, edges.First(x => x.To == 1).Data.Profile);
            Assert.AreEqual(1, edges.First(x => x.To == 2).Data.Profile);
            edges = graph.GetEdgeEnumerator(1);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(2, edges.First(x => x.To == 0).Data.Profile);
            Assert.AreEqual(1, edges.First(x => x.To == 2).Data.Profile);
            edges = graph.GetEdgeEnumerator(2);
            Assert.AreEqual(2, edges.Count());
            Assert.AreEqual(1, edges.First(x => x.To == 0).Data.Profile);
            Assert.AreEqual(1, edges.First(x => x.To == 1).Data.Profile);
        }

        /// <summary>
        /// Tests two identical edges.
        /// </summary>
        [Test]
        public void TestTwoIdenticalEdgesWithShapes()
        {
            var graph = new RoutingNetwork(new Itinero.Graphs.Geometric.GeometricGraph(1, 2));

            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            graph.AddEdge(0, 1, new EdgeData() { Profile = 1 }, new Coordinate(0.5f, 0.5f));
            graph.AddEdge(1, 2, new EdgeData() { Profile = 1 }, new Coordinate(1.5f, 1.5f));

            // execute algorithm.
            var algorithm = new NetworkOptimizer(graph, (v) => false, MergeDelegate, 0);
            algorithm.Run();

            // check result.
            var edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data.Profile);
            var shape = new List<Coordinate>(edges.Shape);
            Assert.AreEqual(3, shape.Count);
            Assert.AreEqual(0.5, shape[0].Latitude);
            Assert.AreEqual(0.5, shape[0].Longitude);
            Assert.AreEqual(1, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(1.5, shape[2].Latitude);
            Assert.AreEqual(1.5, shape[2].Longitude);

            graph = new RoutingNetwork(new Itinero.Graphs.Geometric.GeometricGraph(1, 2));

            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            graph.AddEdge(1, 0, new EdgeData() { Profile = 1 }, new Coordinate(0.5f, 0.5f));
            graph.AddEdge(1, 2, new EdgeData() { Profile = 1 }, new Coordinate(1.5f, 1.5f));

            // execute algorithm.
            algorithm = new NetworkOptimizer(graph, (v) => false, MergeDelegate, 0);
            algorithm.Run();

            // check result.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data.Profile);
            shape = new List<Coordinate>(edges.Shape);
            Assert.AreEqual(3, shape.Count);
            Assert.AreEqual(0.5, shape[0].Latitude);
            Assert.AreEqual(0.5, shape[0].Longitude);
            Assert.AreEqual(1, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(1.5, shape[2].Latitude);
            Assert.AreEqual(1.5, shape[2].Longitude);

            graph = new RoutingNetwork(new Itinero.Graphs.Geometric.GeometricGraph(1, 2));

            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            graph.AddEdge(1, 0, new EdgeData() { Profile = 1 }, new Coordinate(0.5f, 0.5f));
            graph.AddEdge(2, 1, new EdgeData() { Profile = 1 }, new Coordinate(1.5f, 1.5f));

            // execute algorithm.
            algorithm = new NetworkOptimizer(graph, (v) => false, MergeDelegate, 0);
            algorithm.Run();

            // check result.
            edges = graph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            Assert.AreEqual(1, edges.First().Data.Profile);
            Assert.AreEqual(true, edges.First().DataInverted);
            shape = new List<Coordinate>(edges.Shape);
            Assert.AreEqual(3, shape.Count);
            Assert.AreEqual(1.5, shape[0].Latitude);
            Assert.AreEqual(1.5, shape[0].Longitude);
            Assert.AreEqual(1, shape[1].Latitude);
            Assert.AreEqual(1, shape[1].Longitude);
            Assert.AreEqual(0.5, shape[2].Latitude);
            Assert.AreEqual(0.5, shape[2].Longitude);
        }
    }
}