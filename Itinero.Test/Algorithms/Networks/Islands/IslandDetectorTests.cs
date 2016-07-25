// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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