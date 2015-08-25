// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System.Reflection;
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.CH.Preprocessing.Ordering;
using OsmSharp.Routing.CH.Preprocessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.CH;

namespace OsmSharp.Test.Unittests.Routing.CH
{
    /// <summary>
    /// Tests the priority calculations.
    /// </summary>
    [TestFixture]
    public class CHPriorityCalculationTests
    {
        /// <summary>
        /// Tests the priority calculations in a very simple network.
        /// </summary>
        /// <remarks>
        /// Network: 
        /// 
        ///              (1)        (2)
        ///                \        /
        ///                10s    10s
        ///                  \    /
        ///                   \  /
        ///                   (3)
        ///                   
        /// To test: Priority parameters.
        ///          newEdges removedEdges contracted depth
        ///     (1):        0            1          0     0
        ///     (2):        0            1          0     0
        ///     (3):        2            2          0     0
        /// </remarks>
        [Test]
        public void TestPriorityCalculation1NoWitnesses()
        {
            var graph = new DirectedGraph<CHEdgeData>();
            var vertex1 = graph.AddVertex(1, 0);
            var vertex2 = graph.AddVertex(2, 0);
            var vertex3 = graph.AddVertex(3, 0);

            graph.AddEdge(vertex1, vertex3, new CHEdgeData(1, true, true, true, 10));
            graph.AddEdge(vertex3, vertex1, new CHEdgeData(1, false, true, true, 10));
            graph.AddEdge(vertex2, vertex3, new CHEdgeData(1, true, true, true, 10));
            graph.AddEdge(vertex3, vertex2, new CHEdgeData(1, false, true, true, 10));

            var witnessCalculator = new DykstraWitnessCalculator(int.MaxValue);
            var priorityCalculator = new EdgeDifferenceContractedSearchSpace(graph, witnessCalculator);

            int newEdges, removedEdges, contracted, depth;
            priorityCalculator.Calculate(vertex1, out newEdges, out removedEdges, out depth, out contracted);
            Assert.AreEqual(0, newEdges);
            Assert.AreEqual(1, removedEdges);
            Assert.AreEqual(0, depth);
            Assert.AreEqual(0, contracted);
            priorityCalculator.Calculate(vertex2, out newEdges, out removedEdges, out depth, out contracted);
            Assert.AreEqual(0, newEdges);
            Assert.AreEqual(1, removedEdges);
            Assert.AreEqual(0, depth);
            Assert.AreEqual(0, contracted);
            priorityCalculator.Calculate(vertex3, out newEdges, out removedEdges, out depth, out contracted);
            Assert.AreEqual(2, newEdges);
            Assert.AreEqual(2, removedEdges);
            Assert.AreEqual(0, depth);
            Assert.AreEqual(0, contracted);
        }
    }
}