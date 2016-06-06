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
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the edge path extensions.
    /// </summary>
    [TestFixture]
    public class EdgePathExtensionsTests
    {
        /// <summary>
        /// Tests getting the original sequences.
        /// </summary>
        [Test]
        public void TestGetSequence()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(1);
            var e1 = graph.AddEdge(0, 1, 100, null);
            var e2 = graph.AddEdge(1, 2, 100, null);
            var e3 = graph.AddEdge(2, 6, 100, null, 4, new uint[] { 3 }, new uint[] { 5 });
            var e4 = graph.AddEdge(6, 16, 100, null, 11, new uint[] { 7, 8, 9, 10 }, new uint[] { 12, 13, 14, 15 });
            var enumerator = graph.GetEdgeEnumerator();

            // build and test getting sequences from paths.
            var path = new EdgePath(0);
            var s = path.GetSequence(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(1, s.Length);
            Assert.AreEqual(0, s[0]);

            path = new EdgePath(1, 100, new EdgePath(0));
            s = path.GetSequence(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(2, s.Length);
            Assert.AreEqual(0, s[0]);
            Assert.AreEqual(1, s[1]);

            path = new EdgePath(2, 200, e2 + 1, new EdgePath(1, 100, new EdgePath(0)));
            s = path.GetSequence(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(3, s.Length);
            Assert.AreEqual(0, s[0]);
            Assert.AreEqual(1, s[1]);
            Assert.AreEqual(2, s[2]);

            path = new EdgePath(6, 300, e3 + 1, new EdgePath(2, 200, e2 + 1, new EdgePath(1, 100, new EdgePath(0))));
            s = path.GetSequence(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(2, s.Length);
            Assert.AreEqual(5, s[0]);
            Assert.AreEqual(6, s[1]);

            path = new EdgePath(16, 400, e4 + 1, new EdgePath(6, 300, e3 + 1, new EdgePath(2, 200, e2 + 1, new EdgePath(1, 100, new EdgePath(0)))));
            s = path.GetSequence(enumerator);
            Assert.IsNotNull(s);
            Assert.AreEqual(5, s.Length);
            Assert.AreEqual(12, s[0]);
            Assert.AreEqual(13, s[1]);
            Assert.AreEqual(14, s[2]);
            Assert.AreEqual(15, s[3]);
            Assert.AreEqual(16, s[4]);
        }
    }
}