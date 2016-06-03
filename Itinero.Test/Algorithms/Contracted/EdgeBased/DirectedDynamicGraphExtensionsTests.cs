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

using Itinero.Graphs.Directed;
using NUnit.Framework;
using Itinero.Algorithms.Contracted.EdgeBased;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Tests extension methods related to the directed dynamic graph and the edge-layouts.
    /// </summary>
    [TestFixture]
    public class DirectedDynamicGraphExtensionsTests
    {
        /// <summary>
        /// Tests is original.
        /// </summary>
        [Test]
        public void TestIsOriginal()
        {
            // build a graph with one of each possible edge-types.
            var graph = new DirectedDynamicGraph(1);
            var edge1 = graph.AddEdge(0, 1, 10); // add weight/direction field only.
            var edge2 = graph.AddEdge(1, 2, 10, 3); // add a weight/direction field and a contracted id. In this case startsequence is { 3 } and endsequence is { 3 }.
            var edge3 = graph.AddEdge(2, 3, 10, 4, 1, 2); // add a weight/direction field, a contracted id and a startsequence { 2 }.
            var edge4 = graph.AddEdge(3, 4, 10, 5, 0, 2); // add a weight/direction field, a contracted id and an endsequence { 2 }.
            var edge5 = graph.AddEdge(4, 5, 10, 6, 1, 4, 5); // add a weight/direction field, a contracted id, a startsequence { 4 } and an endsequence { 5 }.

            var enumerator = graph.GetEdgeEnumerator();

            enumerator.MoveToEdge(edge1);
            Assert.IsTrue(enumerator.IsOriginal());
            Assert.IsTrue(enumerator.Current.IsOriginal());

            enumerator.MoveToEdge(edge2);
            Assert.IsFalse(enumerator.IsOriginal());
            Assert.IsFalse(enumerator.Current.IsOriginal());

            enumerator.MoveToEdge(edge3);
            Assert.IsFalse(enumerator.IsOriginal());
            Assert.IsFalse(enumerator.Current.IsOriginal());

            enumerator.MoveToEdge(edge4);
            Assert.IsFalse(enumerator.IsOriginal());
            Assert.IsFalse(enumerator.Current.IsOriginal());

            enumerator.MoveToEdge(edge5);
            Assert.IsFalse(enumerator.IsOriginal());
            Assert.IsFalse(enumerator.Current.IsOriginal());
        }

        /// <summary>
        /// Tests get contracted.
        /// </summary>
        [Test]
        public void TestGetContracted()
        {
            // build a graph with one of each possible edge-types.
            var graph = new DirectedDynamicGraph(1);
            var edge1 = graph.AddEdge(0, 1, 10); // add weight/direction field only.
            var edge2 = graph.AddEdge(1, 2, 10, 3); // add a weight/direction field and a contracted id, and edge with start- or endsequence.
            var edge3 = graph.AddEdge(2, 3, 10, 4, 1, 2); // add a weight/direction field, a contracted id and a startsequence { 2 }.
            var edge4 = graph.AddEdge(3, 4, 10, 5, 0, 2); // add a weight/direction field, a contracted id and an endsequence { 2 }.
            var edge5 = graph.AddEdge(4, 5, 10, 6, 1, 4, 5); // add a weight/direction field, a contracted id, a startsequence { 4 } and an endsequence { 5 }.

            var enumerator = graph.GetEdgeEnumerator();

            enumerator.MoveToEdge(edge1);
            Assert.AreEqual(null, enumerator.GetContracted());
            enumerator.MoveToEdge(edge2);
            Assert.AreEqual(3, enumerator.GetContracted());
            enumerator.MoveToEdge(edge3);
            Assert.AreEqual(4, enumerator.GetContracted());
            enumerator.MoveToEdge(edge4);
            Assert.AreEqual(5, enumerator.GetContracted());
            enumerator.MoveToEdge(edge5);
            Assert.AreEqual(6, enumerator.GetContracted());
        }

        /// <summary>
        /// Tests getting the source sequence.
        /// </summary>
        [Test]
        public void GetSequence1()
        {
            // build a graph with one of each possible edge-types.
            var graph = new DirectedDynamicGraph(1);
            var edge1 = graph.AddEdge(0, 1, 10f, null); // add weight/direction field only.
            var edge2 = graph.AddEdge(1, 2, 20f, null, 3, null, null); // add a weight/direction field and a contracted id, and edge with start- or endsequence.
            var edge3 = graph.AddEdge(2, 3, 30f, null, 4, new uint[] { 2 }, null); // add a weight/direction field, a contracted id and a startsequence { 2 }.
            var edge4 = graph.AddEdge(3, 4, 40f, null, 5, null, new uint[] { 2 }); // add a weight/direction field, a contracted id and an endsequence { 2 }.
            var edge5 = graph.AddEdge(4, 5, 50f, null, 6, new uint[] { 4 }, new uint[] { 5 }); // add a weight/direction field, a contracted id, a startsequence { 4 } and an endsequence { 5 }.

            var enumerator = graph.GetEdgeEnumerator();

            enumerator.MoveToEdge(edge1);
            var seq = enumerator.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[0], seq);
            seq = enumerator.Current.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[0], seq);

            enumerator.MoveToEdge(edge2);
            seq = enumerator.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 3 }, seq);
            seq = enumerator.Current.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 3 }, seq);

            enumerator.MoveToEdge(edge3);
            seq = enumerator.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 2 }, seq);
            seq = enumerator.Current.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 2 }, seq);

            enumerator.MoveToEdge(edge4);
            seq = enumerator.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 5 }, seq);
            seq = enumerator.Current.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 5 }, seq);

            enumerator.MoveToEdge(edge5);
            seq = enumerator.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 4 }, seq);
            seq = enumerator.Current.GetSequence1();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 4 }, seq);
        }

        /// <summary>
        /// Tests getting the target sequence.
        /// </summary>
        [Test]
        public void GetSequence2()
        {
            // build a graph with one of each possible edge-types.
            var graph = new DirectedDynamicGraph(1);
            var edge1 = graph.AddEdge(0, 1, 10f, null); // add weight/direction field only.
            var edge2 = graph.AddEdge(1, 2, 20f, null, 3, null, null); // add a weight/direction field and a contracted id, and edge with start- or endsequence.
            var edge3 = graph.AddEdge(2, 3, 30f, null, 4, new uint[] { 2 }, null); // add a weight/direction field, a contracted id and a startsequence { 2 }.
            var edge4 = graph.AddEdge(3, 4, 40f, null, 5, null, new uint[] { 2 }); // add a weight/direction field, a contracted id and an endsequence { 2 }.
            var edge5 = graph.AddEdge(4, 5, 50f, null, 6, new uint[] { 4 }, new uint[] { 5 }); // add a weight/direction field, a contracted id, a startsequence { 4 } and an endsequence { 5 }.

            var enumerator = graph.GetEdgeEnumerator();

            enumerator.MoveToEdge(edge1);
            var seq = enumerator.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[0], seq);
            seq = enumerator.Current.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[0], seq);

            enumerator.MoveToEdge(edge2);
            seq = enumerator.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 3 }, seq);
            seq = enumerator.Current.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 3 }, seq);

            enumerator.MoveToEdge(edge3);
            seq = enumerator.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 4 }, seq);
            seq = enumerator.Current.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 4 }, seq);

            enumerator.MoveToEdge(edge4);
            seq = enumerator.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 2 }, seq);
            seq = enumerator.Current.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 2 }, seq);

            enumerator.MoveToEdge(edge5);
            seq = enumerator.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 5 }, seq);
            seq = enumerator.Current.GetSequence2();
            Assert.IsNotNull(seq);
            Assert.AreEqual(new uint[] { 5 }, seq);
        }

        /// <summary>
        /// Tests adding or update edge.
        /// </summary>
        [Test]
        public void AddOrUpdateEdge()
        {
            // build a graph with one of each possible edge-types.
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100, null);
            graph.AddEdge(1, 2, 200, null, 1, null, null);
            graph.AddEdge(2, 3, 300, false, 2, null, null);
            graph.AddEdge(3, 4, 400, true, 3, null, null);

            graph.AddEdgeOrUpdate(0, 1, 110, null, 10, new uint[] { 10 }, new uint[] { 10 });
            graph.AddEdgeOrUpdate(1, 2, 190, null, 11, new uint[] { 11 }, new uint[] { 11 });
            graph.AddEdgeOrUpdate(1, 2, 180, true, 12, new uint[] { 12 }, new uint[] { 12 });
        }
    }
}