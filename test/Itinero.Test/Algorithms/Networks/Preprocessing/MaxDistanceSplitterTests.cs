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
    /// Tests the max distance splitter.
    /// </summary>
    [TestFixture]
    public class MaxDistanceSplitterTests
    {
        /// <summary>
        /// Tests without the need to split anything.
        /// </summary>
        [Test]
        public void TestWithNoSplits()
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

            var splitter = new Itinero.Algorithms.Networks.Preprocessing.MaxDistanceSplitter(network, (x, y) => {});
            splitter.Run();

            Assert.AreEqual(2, network.VertexCount);
            Assert.AreEqual(1, network.EdgeCount);
        }

        /// <summary>
        /// Tests with a split.
        /// </summary>
        [Test]
        public void TestWithSplit()
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

            var splitter = new Itinero.Algorithms.Networks.Preprocessing.MaxDistanceSplitter(network, (x, y) => {}, 50, (v) => 
            {
                Assert.AreEqual(2, v);
            });
            splitter.Run();

            Assert.AreEqual(3, network.VertexCount);
            Assert.AreEqual(2, network.EdgeCount);
            Assert.AreEqual(1, network.GetEdges(0).Count);
            Assert.AreEqual(1, network.GetEdges(1).Count);
            Assert.AreEqual(2, network.GetEdges(2).Count);
        }

        /// <summary>
        /// Tests with a split and a shape points.
        /// </summary>
        [Test]
        public void TestWithSplitWithShape()
        {
            var E = 1f;

            var network = new RoutingNetwork();
            network.AddVertex(0, 51.26833269279416f, 4.800724983215332f);
            network.AddVertex(1, 51.26570121838145f, 4.801089763641357f);

            network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 314.32f,
                Profile = 1,
                MetaId = 10
            }, new Coordinate(51.26783594640662f,4.801433086395264f),
                new Coordinate(51.26672160389640f,4.801068305969238f));

            var splitter = new Itinero.Algorithms.Networks.Preprocessing.MaxDistanceSplitter(network, (x, y) => {}, 300);
            splitter.Run();

            Assert.AreEqual(3, network.VertexCount);
            Assert.AreEqual(2, network.EdgeCount);
            Assert.AreEqual(1, network.GetEdges(0).Count);
            Assert.AreEqual(314.32f / 2, network.GetEdges(0)[0].Data.Distance, E);
            Assert.AreEqual(1, network.GetEdges(1).Count);
            Assert.AreEqual(314.32f / 2, network.GetEdges(1)[0].Data.Distance, E);
            Assert.AreEqual(2, network.GetEdges(2).Count);
        }
    }
}