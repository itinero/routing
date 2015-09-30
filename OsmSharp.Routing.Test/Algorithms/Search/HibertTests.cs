// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using NUnit.Framework;
using OsmSharp.Math.Algorithms;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Graphs.Geometric;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test.Algorithms.Search
{
    /// <summary>
    /// Contains tests for the hilbert sort/search algorithms.
    /// </summary>
    [TestFixture]
    class HibertTests
    {
        /// <summary>
        /// Tests the sort hilbert function with order #4.
        /// </summary>
        [Test]
        public void SortHilbertTestSteps4()
        {
            var n = 4;

            // build locations.
            var locations = new List<GeoCoordinate>();
            locations.Add(new GeoCoordinate(-90, -180));
            locations.Add(new GeoCoordinate(-90, -60));
            locations.Add(new GeoCoordinate(-90, 60));
            locations.Add(new GeoCoordinate(-90, 180));
            locations.Add(new GeoCoordinate(-30, -180));
            locations.Add(new GeoCoordinate(-30, -60));
            locations.Add(new GeoCoordinate(-30, 60));
            locations.Add(new GeoCoordinate(-30, 180));
            locations.Add(new GeoCoordinate(30, -180));
            locations.Add(new GeoCoordinate(30, -60));
            locations.Add(new GeoCoordinate(30, 60));
            locations.Add(new GeoCoordinate(30, 180));
            locations.Add(new GeoCoordinate(90, -180));
            locations.Add(new GeoCoordinate(90, -60));
            locations.Add(new GeoCoordinate(90, 60));
            locations.Add(new GeoCoordinate(90, 180));

            // build graph.
            var graph = new GeometricGraph(1);
            for (var vertex = 0; vertex < locations.Count; vertex++)
            {
                graph.AddVertex((uint)vertex, (float)locations[vertex].Latitude,
                    (float)locations[vertex].Longitude);
            }

            // build a sorted version in-place.
            graph.Sort(n);

            // test if sorted.
            for (uint vertex = 1; vertex < graph.VertexCount - 1; vertex++)
            {
                Assert.IsTrue(
                    graph.Distance(n, vertex) <=
                    graph.Distance(n, vertex + 1));
            }

            // sort locations.
            locations.Sort((x, y) =>
            {
                return HilbertCurve.HilbertDistance((float)x.Latitude, (float)x.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance((float)y.Latitude, (float)y.Longitude, n));
            });

            // confirm sort.
            for (uint vertex = 0; vertex < graph.VertexCount; vertex++)
            {
                float latitude, longitude;
                graph.GetVertex(vertex, out latitude, out longitude);
                Assert.AreEqual(latitude, locations[(int)vertex].Latitude);
                Assert.AreEqual(longitude, locations[(int)vertex].Longitude);
            }
        }

        /// <summary>
        /// Tests searching the closest vertex.
        /// </summary>
        [Test]
        public void SearchClosestVertexTest()
        {
            var graph = new GeometricGraph(1);
            graph.AddVertex(0, 1, 1);
            graph.AddVertex(1, 2, 2);

            graph.Sort();

            Assert.AreEqual(0, graph.SearchClosest(1, 1, 1));
            Assert.AreEqual(1, graph.SearchClosest(2, 2, 1));
            Assert.AreEqual(Constants.NO_VERTEX, graph.SearchClosest(3, 3, .5f));
        }
    }
}
