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

using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.Dual;
using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Contracted.Dual
{
    /// <summary>
    /// Contains tests for the dykstra algorithm.
    /// </summary>
    [TestFixture]
    public class DykstraTests
    {
        /// <summary>
        /// Tests routing a graph with one edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            var weightHandler = new DefaultWeightHandler(null);
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                weightHandler.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);

            var dykstra = new Itinero.Algorithms.Contracted.Dual.Dykstra<float>(graph, weightHandler, 0, false, float.MaxValue);
            dykstra.WasFound += (p, v, w) =>
            { 
                 if (v == 0)
                 {
                     Assert.AreEqual(0, w);
                 }
                 else if(v == 1)
                 {
                     Assert.AreEqual(100, w);
                 }

                 return false;
            };
            dykstra.Run();

            Assert.AreEqual(true, dykstra.HasRun);
            Assert.AreEqual(true, dykstra.HasSucceeded);
        }

        /// <summary>
        /// Tests routing a graph with two edges.
        /// </summary>
        [Test]
        public void TestOneHopTwoEdges()
        {
            var weightHandler = new DefaultWeightHandler(null);
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                weightHandler.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);
            graph.AddEdge(0, 2, 200, true, Constants.NO_VERTEX);

            var dykstra = new Itinero.Algorithms.Contracted.Dual.Dykstra<float>(graph, weightHandler, 0, false, float.MaxValue);
            dykstra.WasFound += (p, v, w) =>
            {
                if (v == 0)
                {
                    Assert.AreEqual(0, w);
                }
                else if (v == 1)
                {
                    Assert.AreEqual(100, w);
                }
                else if(v == 2)
                {
                    Assert.AreEqual(200, w);
                }

                return false;
            };
            dykstra.Run();

            Assert.AreEqual(true, dykstra.HasRun);
            Assert.AreEqual(true, dykstra.HasSucceeded);
        }

        /// <summary>
        /// Tests routing a graph with two edges.
        /// </summary>
        [Test]
        public void TestTwoHopTwoEdges()
        {
            var weightHandler = new DefaultWeightHandler(null);
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                weightHandler.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 200, true, Constants.NO_VERTEX);

            var dykstra = new Itinero.Algorithms.Contracted.Dual.Dykstra<float>(graph, weightHandler, 0, false, float.MaxValue);
            dykstra.WasFound += (p, v, w) =>
            {
                if (v == 0)
                {
                    Assert.AreEqual(0, w);
                }
                else if (v == 1)
                {
                    Assert.AreEqual(100, w);
                }
                else if (v == 2)
                {
                    Assert.AreEqual(300, w);
                }

                return false;
            };
            dykstra.Run();

            Assert.AreEqual(true, dykstra.HasRun);
            Assert.AreEqual(true, dykstra.HasSucceeded);
        }
    }
}
