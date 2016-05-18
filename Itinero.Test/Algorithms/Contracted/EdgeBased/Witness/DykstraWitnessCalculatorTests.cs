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
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(0, 1, 100f, 0);
            graph.AddEdge(1, 2, 100f, 0);
            graph.AddEdge(2, 3, 100f, 0);

            var witnessCalculator = new DykstraWitnessCalculator(graph, (p) => new Itinero.Profiles.Factor() { Direction = 0, Value = 1 }, (v) => Enumerable.Empty<uint[]>());
            Assert.IsTrue(witnessCalculator.Calculate(new uint[] { 0 }, new uint[] { 3 }, 100, 1000));
            Assert.IsFalse(witnessCalculator.Calculate(new uint[] { 0 }, new uint[] { 3 }, 100, 200));
        }

        /// <summary>
        /// Tests basic calculations using a target sequence.
        /// </summary>
        [Test]
        public void TestTargetSequence()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(0, 1, 100f, 0);
            graph.AddEdge(1, 2, 100f, 0);
            graph.AddEdge(2, 3, 100f, 0);

            var witnessCalculator = new DykstraWitnessCalculator(graph, (p) => new Itinero.Profiles.Factor() { Direction = 0, Value = 1 }, (v) => Enumerable.Empty<uint[]>());
            Assert.IsTrue(witnessCalculator.Calculate(new uint[] { 0 }, new uint[] { 2, 3 }, 100, 1000));
            Assert.IsFalse(witnessCalculator.Calculate(new uint[] { 0 }, new uint[] { 2, 3 }, 100, 201));
        }

        /// <summary>
        /// Tests basic calculations using a source sequence.
        /// </summary>
        [Test]
        public void TestSourceSequence()
        {
            var graph = new Graph(1);
            graph.AddVertex(0);
            graph.AddVertex(1);
            graph.AddVertex(2);
            graph.AddVertex(3);
            graph.AddEdge(0, 1, 100f, 0);
            graph.AddEdge(1, 2, 100f, 0);
            graph.AddEdge(2, 3, 100f, 0);

            var witnessCalculator = new DykstraWitnessCalculator(graph, (p) => new Itinero.Profiles.Factor() { Direction = 0, Value = 1 }, (v) => Enumerable.Empty<uint[]>());
            Assert.IsTrue(witnessCalculator.Calculate(new uint[] { 0, 1 }, new uint[] { 3 }, 100, 1000));
            Assert.IsFalse(witnessCalculator.Calculate(new uint[] { 0, 1 }, new uint[] { 3 }, 100, 201));
        }
    }
}