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

using Itinero.Algorithms;
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the hierarchy builder.
    /// </summary>
    [TestFixture]
    public class BidirectionalDykstraTests
    {
        /// <summary>
        /// Tests a small network with a restricted turn.
        /// </summary>
        [Test]
        public void TestRestrictedTurn()
        {
            var directedGraph = new DirectedDynamicGraph(5, 1);
            var edge01 = directedGraph.AddEdge(0, 1, 100, null);
            var edge12 = directedGraph.AddEdge(1, 2, 100, null);
            var edge13 = directedGraph.AddEdge(1, 3, 100, null);
            var edge14 = directedGraph.AddEdge(1, 4, 100, null);
            var edge23 = directedGraph.AddEdge(2, 3, 100, null);

            // contract graph.
            Func<uint, IEnumerable<uint[]>> getRestrictions = (v) =>
            {
                if (v == 0)
                {
                    return new uint[][] {
                        new uint[] { 0, 1, 4 }
                    };
                }
                if (v == 4)
                {
                    return new uint[][] {
                        new uint[] { 4, 1, 0 }
                    };
                }
                return Enumerable.Empty<uint[]>();
            };
            Func<ushort, Factor> getFactor = (p) => new Factor() { Direction = 0, Value = 1 };

            var dykstra = new BidirectionalDykstra(directedGraph,
                new Path[] { new Path(1, 100, new Path(0)), new Path(0) },
                new Path[] { new Path(1, 100, new Path(2)), new Path(2) });
            dykstra.Run();

            var path = dykstra.GetPath();
            Assert.AreEqual(3, path.Count);
            Assert.AreEqual(1, path[0]);
            Assert.AreEqual(2, path[1]);
            Assert.AreEqual(3, path[2]);
        }
    }
}