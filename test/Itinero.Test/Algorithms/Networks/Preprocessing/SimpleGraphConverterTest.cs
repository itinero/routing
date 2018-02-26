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

using Itinero.Data.Network;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Networks.Preprocessing
{
    /// <summary>
    /// Contains tests for the simple graph converter.
    /// </summary>
    [TestFixture]
    public class SimpleGraphConverterTest
    {
        /// <summary>
        /// Test on a small network where no simplifcation is needed.
        /// </summary>
        [Test]
        public void TestNoSimplifcationNeeded()
        {
            var network = new RoutingNetwork();
            network.AddVertex(0, 51.268064181900094f, 4.800832271575927f);
            network.AddVertex(1, 51.2676479869138f, 4.801325798034667f);
            network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 57.69f,
                Profile = 1,
                MetaId = 10
            });

            var converter = new Itinero.Algorithms.Networks.Preprocessing.SimpleGraphConverter(network, (x, y) => {});
            converter.Run();

            Assert.AreEqual(2, network.VertexCount);
            Assert.AreEqual(1, network.EdgeCount);
        }
        
        /// <summary>
        /// Test on a small network with one duplicate edge.
        /// </summary>
        [Test]
        public void TestOneDuplicateEdge()
        {
            var network = new RoutingNetwork();
            network.GeometricGraph.Graph.MarkAsMulti();
            network.AddVertex(0, 51.268064181900094f, 4.800832271575927f);
            network.AddVertex(1, 51.2676479869138f, 4.801325798034667f);
            network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 57.69f,
                Profile = 1,
                MetaId = 10
            });
            network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 57.69f,
                Profile = 1,
                MetaId = 10
            });

            var converter = new Itinero.Algorithms.Networks.Preprocessing.SimpleGraphConverter(network, (x, y) => {}, (v) => 
            {
                Assert.AreEqual(2, v);
            });
            converter.Run();

            Assert.AreEqual(3, network.VertexCount);
            Assert.AreEqual(3, network.EdgeCount);
            Assert.IsTrue(network.GeometricGraph.Graph.IsSimple);
        }
        
        /// <summary>
        /// Test on a small network with one duplicate edge with a shape.
        /// </summary>
        [Test]
        public void TestOneDuplicateEdgeWithShape()
        {
            var network = new RoutingNetwork();
            network.GeometricGraph.Graph.MarkAsMulti();
            network.AddVertex(0, 51.26833269279416f, 4.800724983215332f);
            network.AddVertex(1, 51.26570121838145f, 4.801089763641357f);
            network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 214.32f,
                Profile = 1,
                MetaId = 10
            });
            network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 314.32f,
                Profile = 1,
                MetaId = 10
            }, new Coordinate(51.26783594640662f,4.801433086395264f),
                new Coordinate(51.26672160389640f,4.801068305969238f));
            Assert.IsFalse(network.GeometricGraph.Graph.MarkAsSimple());

            var converter = new Itinero.Algorithms.Networks.Preprocessing.SimpleGraphConverter(network, (x, y) => {}, (v) => 
            {
                Assert.AreEqual(2, v);
            });
            converter.Run();

            Assert.AreEqual(3, network.VertexCount);
            Assert.AreEqual(3, network.EdgeCount);
            Assert.IsTrue(network.GeometricGraph.Graph.IsSimple);
            Assert.IsTrue(network.GeometricGraph.Graph.MarkAsSimple());
        }

        /// <summary>
        /// Test on a small network with one looped edge with a shape.
        /// </summary>
        [Test]
        public void TestOneLoopedEdgeWithShape()
        {
            var network = new RoutingNetwork();
            network.GeometricGraph.Graph.MarkAsMulti();
            network.AddVertex(0, 51.26833269279416f, 4.800724983215332f);
            network.AddEdge(0, 0, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 314.32f,
                Profile = 1,
                MetaId = 10
            }, new Coordinate(51.26783594640662f,4.801433086395264f),
                new Coordinate(51.26672160389640f,4.801068305969238f));
            Assert.IsFalse(network.GeometricGraph.Graph.MarkAsSimple());

            var converter = new Itinero.Algorithms.Networks.Preprocessing.SimpleGraphConverter(network, (x, y) => {}, (v) => 
            {
                if (v != 1 && v != 2)
                {
                    Assert.Fail("An unexpected vertex was reportedly added.");
                }
            });
            converter.Run();

            Assert.AreEqual(3, network.VertexCount);
            Assert.AreEqual(3, network.EdgeCount);
            Assert.IsTrue(network.GeometricGraph.Graph.IsSimple);
            Assert.IsTrue(network.GeometricGraph.Graph.MarkAsSimple());
        }
    }
}