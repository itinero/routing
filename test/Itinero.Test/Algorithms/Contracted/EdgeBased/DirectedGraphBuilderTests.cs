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
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Linq;
using Itinero.Data.Edges;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Weights;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Containts tests for the directed graph builder.
    /// </summary>
    [TestFixture]
    public class DirectedGraphBuilderTests
    {
        /// <summary>
        /// Tests converting a graph with one edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var graph = new Itinero.Graphs.Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Factor> getFactor = (x) =>
            {
                return new Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // convert graph.
            var directedGraph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            var algorithm = new DirectedGraphBuilder(graph, directedGraph, getFactor);
            algorithm.Run();

            // check result.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            directedGraph.Compress();
            Assert.AreEqual(2, directedGraph.VertexCount);
            Assert.AreEqual(2, directedGraph.EdgeCount);

            // verify all edges.
            var edges = directedGraph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            var data = ContractedEdgeDataSerializer.Serialize(100 * getFactor(1).Value, null);
            Assert.AreEqual(data, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);
            
            edges = directedGraph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            data = ContractedEdgeDataSerializer.Serialize(100 * getFactor(1).Value, null);
            Assert.AreEqual(data, edges.First().Data[0]);
            Assert.AreEqual(0, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests converting a graph with one oneway edge.
        /// </summary>
        [Test]
        public void TestOneOnewayEdge()
        {
            // build graph.
            var graph = new Itinero.Graphs.Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Factor> getFactor = (x) =>
            {
                return new Factor()
                {
                    Direction = 1,
                    Value = 1.0f / speed
                };
            };

            // convert graph.
            var directedGraph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            var algorithm = new DirectedGraphBuilder(graph, directedGraph, getFactor);
            algorithm.Run();

            // check result.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            directedGraph.Compress();
            Assert.AreEqual(2, directedGraph.VertexCount);
            Assert.AreEqual(2, directedGraph.EdgeCount);

            // verify all edges.
            var edges = directedGraph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            var data = ContractedEdgeDataSerializer.Serialize(100 * getFactor(1).Value, true);
            Assert.AreEqual(data, edges.First().Data[0]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = directedGraph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            data = ContractedEdgeDataSerializer.Serialize(100 * getFactor(1).Value, false);
            Assert.AreEqual(data, edges.First().Data[0]);
            Assert.AreEqual(0, edges.First().Neighbour);
        }

        /// <summary>
        /// Tests converting a graph with one edge.
        /// </summary>
        [Test]
        public void TestOneEdgeAugmented()
        {
            // build graph.
            var graph = new Itinero.Graphs.Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, FactorAndSpeed> getFactor = (x) =>
            {
                return new FactorAndSpeed()
                {
                    Direction = 0,
                    SpeedFactor = 1.0f / speed,
                    Value = 1.0f / speed
                };
            };

            // convert graph.
            var directedGraph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicAugmentedFixedSize);
            var algorithm = new DirectedGraphBuilder<Weight>(graph, directedGraph, new WeightHandler(getFactor));
            algorithm.Run();

            // check result.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            directedGraph.Compress();
            Assert.AreEqual(2, directedGraph.VertexCount);
            Assert.AreEqual(2, directedGraph.EdgeCount);

            // verify all edges.
            var edges = directedGraph.GetEdgeEnumerator(0);
            Assert.AreEqual(1, edges.Count());
            var data = ContractedEdgeDataSerializer.SerializeDynamicAugmented(100 * getFactor(1).Value, null, 100, 100 * getFactor(1).Value);
            Assert.AreEqual(data[0], edges.First().Data[0]);
            Assert.AreEqual(data[1], edges.First().Data[1]);
            Assert.AreEqual(data[2], edges.First().Data[2]);
            Assert.AreEqual(1, edges.First().Neighbour);

            edges = directedGraph.GetEdgeEnumerator(1);
            Assert.AreEqual(1, edges.Count());
            data = ContractedEdgeDataSerializer.SerializeDynamicAugmented(100 * getFactor(1).Value, null, 100, 100 * getFactor(1).Value);
            Assert.AreEqual(data[0], edges.First().Data[0]);
            Assert.AreEqual(data[1], edges.First().Data[1]);
            Assert.AreEqual(data[2], edges.First().Data[2]);
            Assert.AreEqual(0, edges.First().Neighbour);
        }
    }
}