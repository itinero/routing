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
using System.Linq;
using Itinero.Data.Network.Edges;

namespace Itinero.Test.Algorithms.Networks
{
    /// <summary>
    /// Contains tests for the network routerdb extensions.
    /// </summary>
    [TestFixture]
    public class RouterDbExtensionTests
    {
        /// <summary>
        /// Tests optimizing network 9.
        /// </summary>
        [Test]
        public void TestOptimizeNetwork9()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            // optimize but not possible because of edge meta.
            Assert.AreEqual(10, routerDb.Network.EdgeCount);
            routerDb.OptimizeNetwork();
            routerDb.Network.Compress();
            Assert.AreEqual(10, routerDb.Network.EdgeCount);
        }
        
        /// <summary>
        /// Tests optimizing network 9.
        /// </summary>
        [Test]
        public void TestOptimizeNetwork9WithoutMeta()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.EdgeData.Clear();

            // optimize but not possible because of edge meta.
            Assert.AreEqual(10, routerDb.Network.EdgeCount);
            routerDb.OptimizeNetwork();
            routerDb.Network.Compress();
            Assert.AreEqual(6, routerDb.Network.EdgeCount);
        }
        
        /// <summary>
        /// Tests two identical edges.
        /// </summary>
        [Test]
        public void TestTwoIdenticalEdges()
        {
            // create graph with one vertex and start adding 2.
            var routerDb = new RouterDb();
            var graph = routerDb.Network;

            // make sure to add 1 and 2.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            graph.AddEdge(0, 1, new EdgeData() { Profile = 1, MetaId = 1, Distance = 10 }, null);
            graph.AddEdge(1, 2, new EdgeData() { Profile = 1, MetaId = 1, Distance = 20 }, null);

            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            // optimize.
            routerDb.OptimizeNetwork();
            routerDb.Network.Compress();
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

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
        /// Tests two identical edges with different edge meta (can't merge them).
        /// </summary>
        [Test]
        public void TestTwoIdenticalEdgesWithDifferentEdgeMEta()
        {
            // create graph with one vertex and start adding 2.
            var routerDb = new RouterDb();
            var graph = routerDb.Network;

            // make sure to add 1 and 2.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            var e1 = graph.AddEdge(0, 1, new EdgeData() { Profile = 1, MetaId = 1, Distance = 10 }, null);
            var e2 = graph.AddEdge(1, 2, new EdgeData() { Profile = 1, MetaId = 1, Distance = 20 }, null);
            var someData = routerDb.EdgeData.AddInt16("some-data");
            someData[e1] = 0;
            someData[e2] = 1;

            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            // optimize.
            routerDb.OptimizeNetwork();
            routerDb.Network.Compress();
            Assert.AreEqual(2, routerDb.Network.EdgeCount);
        }
        
        /// <summary>
        /// Tests two identical edges with identical edge meta (they should merge fine).
        /// </summary>
        [Test]
        public void TestTwoIdenticalEdgesWithIdenticalEdgeMEta()
        {
            // create graph with one vertex and start adding 2.
            var routerDb = new RouterDb();
            var graph = routerDb.Network;

            // make sure to add 1 and 2.
            graph.AddVertex(0, 0, 0);
            graph.AddVertex(1, 1, 1);
            graph.AddVertex(2, 2, 2);
            var e1 = graph.AddEdge(0, 1, new EdgeData() { Profile = 1, MetaId = 1, Distance = 10 }, null);
            var e2 = graph.AddEdge(1, 2, new EdgeData() { Profile = 1, MetaId = 1, Distance = 20 }, null);
            var someData = routerDb.EdgeData.AddInt16("some-data");
            someData[e1] = 10;
            someData[e2] = 10;

            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            // optimize.
            routerDb.OptimizeNetwork();
            routerDb.Network.Compress();
            Assert.AreEqual(1, routerDb.Network.EdgeCount);
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
            
            Assert.AreEqual(10, routerDb.EdgeData.Get<short>("some-data")[edges.First().Id]);
        }
    }
}