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
using Itinero.Algorithms.Default;
using Itinero.Graphs;
using Itinero.Profiles;
using System;
using Itinero.Data.Edges;

namespace Itinero.Test.Algorithms.Default
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class BidirectionalDykstraTests
    {
        /// <summary>
        /// Tests shortest path calculations over two edges.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m---(1)---100m---(2) @ 100km/h
        /// </remarks>
        [Test]
        public void TestTwoEdges()
        {
            // build graph.
            var graph = new Graph(EdgeDataSerializer.Size);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddEdge(0, 1, EdgeDataSerializer.Serialize(new EdgeData()
            {
                Distance = 100,
                Profile = 1
            }));
            graph.AddEdge(1, 2, EdgeDataSerializer.Serialize(new EdgeData()
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

            // run algorithm.
            var sourceSearch = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(0) },
                150 * 1 / speed, false);
            var targetSearch = new Dykstra(graph, getFactor, null, new EdgePath<float>[] { new EdgePath<float>(2) },
                150 * 1 / speed, true);
            var algorithm = new BidirectionalDykstra(sourceSearch, targetSearch, getFactor);
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.AreEqual(1, algorithm.BestVertex);
            Assert.AreEqual(new uint[] { 0, 1, 2 }, algorithm.GetPath().ToListAsVertices().ToArray());
        }
    }
}