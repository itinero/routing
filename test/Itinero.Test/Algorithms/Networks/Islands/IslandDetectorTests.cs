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

using Itinero.Algorithms.Networks;
using Itinero.Graphs;
using NUnit.Framework;
using System;

namespace Itinero.Test.Algorithms.Networks.Islands
{
    /// <summary>
    /// Contains tests for the island detector.
    /// </summary>
    [TestFixture]
    public class IslandDetectorTests
    {
        /// <summary>
        /// Tests island detection on one edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            // build routerdb.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 1
            }, null);

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Itinero.Profiles.Factor> getFactor = (x) =>
            {
                return new Itinero.Profiles.Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // start island detector.
            var islandDetector = new IslandDetector(routerDb, new Func<ushort, Itinero.Profiles.Factor>[] { getFactor });
            islandDetector.Run();

            // verify the islands.
            var islands = islandDetector.Islands;
            Assert.IsNotNull(islands);
            Assert.AreEqual(2, islands.Length);
            Assert.AreEqual(1, islands[0]);
            Assert.AreEqual(1, islands[1]);
        }

        /// <summary>
        /// Tests island detection on two distinct edges.
        /// </summary>
        [Test]
        public void TestTwoDistinctEdges()
        {
            // build routerdb.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 1
            }, null);
            routerDb.Network.AddVertex(2, 0, 0);
            routerDb.Network.AddVertex(3, 1, 1);
            routerDb.Network.AddEdge(2, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 1
            }, null);

            // build speed profile function.
            var speed = 100f / 3.6f;
            Func<ushort, Itinero.Profiles.Factor> getFactor = (x) =>
            {
                return new Itinero.Profiles.Factor()
                {
                    Direction = 0,
                    Value = 1.0f / speed
                };
            };

            // start island detector.
            var islandDetector = new IslandDetector(routerDb, new Func<ushort, Itinero.Profiles.Factor>[] { getFactor });
            islandDetector.Run();

            // verify the islands.
            var islands = islandDetector.Islands;
            Assert.IsNotNull(islands);
            Assert.AreEqual(4, islands.Length);
            Assert.AreEqual(1, islands[0]);
            Assert.AreEqual(1, islands[1]);
            Assert.AreEqual(2, islands[2]);
            Assert.AreEqual(2, islands[3]);
        }
    }
}