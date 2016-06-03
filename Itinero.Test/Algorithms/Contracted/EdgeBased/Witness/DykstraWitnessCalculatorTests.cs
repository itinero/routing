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
using Itinero.Algorithms.Contracted.EdgeBased;
using NUnit.Framework;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using System.Linq;
using Itinero.Graphs;
using Itinero.Algorithms.Default;
using Itinero.Algorithms;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased.Witness
{
    /// <summary>
    /// Contains tests for the dykstra witness calculator.
    /// </summary>
    [TestFixture]
    public class DykstraWitnessCalculatorTests
    {
        /// <summary>
        /// Tests basic calculations using no sequences.
        /// </summary>
        [Test]
        public void TestNoSequences()
        {
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100f, null);
            graph.AddEdge(1, 0, 100f, null);
            graph.AddEdge(1, 2, 100f, null);
            graph.AddEdge(2, 1, 100f, null);
            graph.AddEdge(2, 3, 100f, null);
            graph.AddEdge(3, 2, 100f, null);

            var witnessCalculator = new DykstraWitnessCalculator((v) => Enumerable.Empty<uint[]>());
            var path = witnessCalculator.Calculate(graph, new uint[] { 0 }, new uint[] { 3 });
            Assert.IsTrue(path.HasVertex(2));
        }

        /// <summary>
        /// Tests basic calculations using a target sequence.
        /// </summary>
        [Test]
        public void TestTargetSequence()
        {
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100f, null);
            graph.AddEdge(1, 2, 100f, null);
            graph.AddEdge(2, 3, 100f, null);
            graph.AddEdge(1, 0, 100f, null);
            graph.AddEdge(2, 1, 100f, null);
            graph.AddEdge(3, 2, 100f, null);

            var witnessCalculator = new DykstraWitnessCalculator((v) => Enumerable.Empty<uint[]>());
            var path = witnessCalculator.Calculate(graph, new uint[] { 0 }, new uint[] { 2, 3 });
            Assert.IsTrue(path.HasVertex(1));
        }

        /// <summary>
        /// Tests basic calculations using a source sequence.
        /// </summary>
        [Test]
        public void TestSourceSequence()
        {
            var graph = new DirectedDynamicGraph(1);
            graph.AddEdge(0, 1, 100f, null);
            graph.AddEdge(1, 0, 100f, null);
            graph.AddEdge(1, 2, 100f, null);
            graph.AddEdge(2, 1, 100f, null);
            graph.AddEdge(2, 3, 100f, null);
            graph.AddEdge(3, 2, 100f, null);

            var witnessCalculator = new DykstraWitnessCalculator((v) => Enumerable.Empty<uint[]>());
            var path = witnessCalculator.Calculate(graph, new uint[] { 0, 1 }, new uint[] { 3 });
            Assert.IsTrue(path.HasVertex(2));
        }
    }
}