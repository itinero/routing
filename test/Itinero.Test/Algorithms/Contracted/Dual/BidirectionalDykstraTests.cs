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
using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.Dual;
using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Contracted.Dual
{
    /// <summary>
    /// Contains 
    /// </summary>
    [TestFixture]
    public class BidirectionalDykstraTests
    {
        /// <summary>
        /// Tests one edge routes.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            var weightHandler = new DefaultWeightHandler(null);
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                weightHandler.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);

            var bidirectionalDykstra = new Itinero.Algorithms.Contracted.Dual.BidirectionalDykstra<float>(graph,
                weightHandler, 0, 1);
            bidirectionalDykstra.Run();

            Assert.AreEqual(true, bidirectionalDykstra.HasRun);
            Assert.AreEqual(true, bidirectionalDykstra.HasSucceeded);

            EdgePath<float> path;
            if (bidirectionalDykstra.TryGetForwardVisit(0, out path))
            {
                Assert.AreEqual(0, path.Vertex);
                Assert.AreEqual(0, path.Weight);
                Assert.AreEqual(null, path.From);
            }
            if (bidirectionalDykstra.TryGetForwardVisit(1, out path))
            {
                Assert.AreEqual(1, path.Vertex);
                Assert.AreEqual(100, path.Weight);
                Assert.IsNotNull(path.From);
                Assert.AreEqual(0, path.From.Vertex);
            }
            if (bidirectionalDykstra.TryGetBackwardVisit(1, out path))
            {
                Assert.AreEqual(1, path.Vertex);
                Assert.AreEqual(0, path.Weight);
                Assert.AreEqual(null, path.From);
            }
            if (bidirectionalDykstra.TryGetBackwardVisit(0, out path))
            {
                Assert.AreEqual(0, path.Vertex);
                Assert.AreEqual(100, path.Weight);
                Assert.IsNotNull(path.From);
                Assert.AreEqual(1, path.From.Vertex);
            }
        }

        /// <summary>
        /// Tests one hop with two edges.
        /// </summary>
        [Test]
        public void TestTwoEdgeOneHop()
        {
            var weightHandler = new DefaultWeightHandler(null);
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                weightHandler.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);
            graph.AddEdge(0, 2, 200, true, Constants.NO_VERTEX);

            var bidirectionalDykstra = new Itinero.Algorithms.Contracted.Dual.BidirectionalDykstra<float>(graph,
                weightHandler, 0, 1);
            bidirectionalDykstra.Run();

            Assert.AreEqual(true, bidirectionalDykstra.HasRun);
            Assert.AreEqual(true, bidirectionalDykstra.HasSucceeded);

            EdgePath<float> path;
            if (bidirectionalDykstra.TryGetForwardVisit(0, out path))
            {
                Assert.AreEqual(0, path.Vertex);
                Assert.AreEqual(0, path.Weight);
                Assert.AreEqual(null, path.From);
            }
            if (bidirectionalDykstra.TryGetForwardVisit(1, out path))
            {
                Assert.AreEqual(1, path.Vertex);
                Assert.AreEqual(100, path.Weight);
                Assert.IsNotNull(path.From);
                Assert.AreEqual(0, path.From.Vertex);
            }
            if (bidirectionalDykstra.TryGetForwardVisit(2, out path))
            {
                Assert.AreEqual(2, path.Vertex);
                Assert.AreEqual(200, path.Weight);
                Assert.IsNotNull(path.From);
                Assert.AreEqual(0, path.From.Vertex);
            }
            if (bidirectionalDykstra.TryGetBackwardVisit(1, out path))
            {
                Assert.AreEqual(1, path.Vertex);
                Assert.AreEqual(0, path.Weight);
                Assert.AreEqual(null, path.From);
            }
            if (bidirectionalDykstra.TryGetBackwardVisit(0, out path))
            {
                Assert.AreEqual(0, path.Vertex);
                Assert.AreEqual(100, path.Weight);
                Assert.IsNotNull(path.From);
                Assert.AreEqual(1, path.From.Vertex);
            }
        }
    }
}