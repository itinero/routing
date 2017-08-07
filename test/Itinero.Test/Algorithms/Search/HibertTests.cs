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
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Test.Algorithms.Search
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
            var locations = new List<Coordinate>();
            locations.Add(new Coordinate(-90, -180));
            locations.Add(new Coordinate(-90, -60));
            locations.Add(new Coordinate(-90, 60));
            locations.Add(new Coordinate(-90, 180));
            locations.Add(new Coordinate(-30, -180));
            locations.Add(new Coordinate(-30, -60));
            locations.Add(new Coordinate(-30, 60));
            locations.Add(new Coordinate(-30, 180));
            locations.Add(new Coordinate(30, -180));
            locations.Add(new Coordinate(30, -60));
            locations.Add(new Coordinate(30, 60));
            locations.Add(new Coordinate(30, 180));
            locations.Add(new Coordinate(90, -180));
            locations.Add(new Coordinate(90, -60));
            locations.Add(new Coordinate(90, 60));
            locations.Add(new Coordinate(90, 180));

            // build graph.
            var graph = new GeometricGraph(1);
            for (var vertex = 0; vertex < locations.Count; vertex++)
            {
                graph.AddVertex((uint)vertex, locations[vertex].Latitude,
                    locations[vertex].Longitude);
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
                return HilbertCurve.HilbertDistance(x.Latitude, x.Longitude, n).CompareTo(
                     HilbertCurve.HilbertDistance(y.Latitude, y.Longitude, n));
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

            Assert.AreEqual(0, graph.SearchClosest(1, 1, 1, 1));
            Assert.AreEqual(1, graph.SearchClosest(2, 2, 1, 1));
            Assert.AreEqual(Constants.NO_VERTEX, graph.SearchClosest(3, 3, .5f, .5f));
        }

        /// <summary>
        /// Tests searching the vertices.
        /// </summary>
        [Test]
        public void SearchVerticesTest()
        {
            var graph = new GeometricGraph(1);
            graph.AddVertex(0, .00f, .00f);
            graph.AddVertex(1, .02f, .00f);
            graph.AddVertex(2, .04f, .00f);
            graph.AddVertex(3, .06f, .00f);
            graph.AddVertex(4, .08f, .00f);
            graph.AddVertex(5, .00f, .02f);
            graph.AddVertex(6, .02f, .02f);
            graph.AddVertex(7, .04f, .02f);
            graph.AddVertex(8, .06f, .02f);
            graph.AddVertex(9, .08f, .02f);
            graph.AddVertex(10, .00f, .04f);
            graph.AddVertex(11, .02f, .04f);
            graph.AddVertex(12, .04f, .04f);
            graph.AddVertex(13, .06f, .04f);
            graph.AddVertex(14, .08f, .04f);
            graph.AddVertex(15, .00f, .06f);
            graph.AddVertex(16, .02f, .06f);
            graph.AddVertex(17, .04f, .06f);
            graph.AddVertex(18, .06f, .06f);
            graph.AddVertex(19, .08f, .06f);
            graph.AddVertex(20, .00f, .08f);
            graph.AddVertex(21, .02f, .08f);
            graph.AddVertex(22, .04f, .08f);
            graph.AddVertex(23, .06f, .08f);
            graph.AddVertex(24, .08f, .08f);

            graph.Sort();

            // test 0.009, 0.009, 0.019, 0.019
            var vertices = graph.Search(0.009f, 0.009f, 0.029f, 0.029f);
            Assert.AreEqual(1, vertices.Count);
            Assert.IsTrue(vertices.Contains(3));
            var location = graph.GetVertex(3);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);

            // test 0.009, 0.009, 0.099, 0.019
            vertices = graph.Search(0.009f, 0.009f, 0.099f, 0.029f);
            Assert.AreEqual(4, vertices.Count);
            Assert.IsTrue(vertices.Contains(3));
            location = graph.GetVertex(3);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(13));
            location = graph.GetVertex(13);
            Assert.AreEqual(.04f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(16));
            location = graph.GetVertex(16);
            Assert.AreEqual(.06f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(19));
            location = graph.GetVertex(19);
            Assert.AreEqual(.08f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);

            // test -0.001, -0.001, 0.09, 0.09
            vertices = graph.Search(-0.001f, -0.001f, 0.09f, 0.09f);
            Assert.AreEqual(25, vertices.Count);
            Assert.IsTrue(vertices.Contains(0));
            location = graph.GetVertex(0);
            Assert.AreEqual(.0f, location.Latitude);
            Assert.AreEqual(.0f, location.Longitude);
            Assert.IsTrue(vertices.Contains(1));
            location = graph.GetVertex(1);
            Assert.AreEqual(.0f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(2));
            location = graph.GetVertex(2);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.0f, location.Longitude);
            Assert.IsTrue(vertices.Contains(3));
            location = graph.GetVertex(3);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(4));
            location = graph.GetVertex(4);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.04f, location.Longitude);
            Assert.IsTrue(vertices.Contains(5));
            location = graph.GetVertex(5);
            Assert.AreEqual(.00f, location.Latitude);
            Assert.AreEqual(.04f, location.Longitude);
            Assert.IsTrue(vertices.Contains(6));
            location = graph.GetVertex(6);
            Assert.AreEqual(.00f, location.Latitude);
            Assert.AreEqual(.06f, location.Longitude);
            Assert.IsTrue(vertices.Contains(7));
            location = graph.GetVertex(7);
            Assert.AreEqual(.00f, location.Latitude);
            Assert.AreEqual(.08f, location.Longitude);
            Assert.IsTrue(vertices.Contains(8));
            location = graph.GetVertex(8);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.08f, location.Longitude);
            Assert.IsTrue(vertices.Contains(9));
            location = graph.GetVertex(9);
            Assert.AreEqual(.02f, location.Latitude);
            Assert.AreEqual(.06f, location.Longitude);
            Assert.IsTrue(vertices.Contains(10));
            location = graph.GetVertex(10);
            Assert.AreEqual(.04f, location.Latitude);
            Assert.AreEqual(.08f, location.Longitude);
            Assert.IsTrue(vertices.Contains(11));
            location = graph.GetVertex(11);
            Assert.AreEqual(.04f, location.Latitude);
            Assert.AreEqual(.06f, location.Longitude);
            Assert.IsTrue(vertices.Contains(12));
            location = graph.GetVertex(12);
            Assert.AreEqual(.04f, location.Latitude);
            Assert.AreEqual(.04f, location.Longitude);
            Assert.IsTrue(vertices.Contains(13));
            location = graph.GetVertex(13);
            Assert.AreEqual(.04f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(14));
            location = graph.GetVertex(14);
            Assert.AreEqual(.04f, location.Latitude);
            Assert.AreEqual(.00f, location.Longitude);
            Assert.IsTrue(vertices.Contains(15));
            location = graph.GetVertex(15);
            Assert.AreEqual(.06f, location.Latitude);
            Assert.AreEqual(.04f, location.Longitude);
            Assert.IsTrue(vertices.Contains(16));
            location = graph.GetVertex(16);
            Assert.AreEqual(.06f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(17));
            location = graph.GetVertex(17);
            Assert.AreEqual(.06f, location.Latitude);
            Assert.AreEqual(.00f, location.Longitude);
            Assert.IsTrue(vertices.Contains(18));
            location = graph.GetVertex(18);
            Assert.AreEqual(.08f, location.Latitude);
            Assert.AreEqual(.00f, location.Longitude);
            Assert.IsTrue(vertices.Contains(19));
            location = graph.GetVertex(19);
            Assert.AreEqual(.08f, location.Latitude);
            Assert.AreEqual(.02f, location.Longitude);
            Assert.IsTrue(vertices.Contains(20));
            location = graph.GetVertex(20);
            Assert.AreEqual(.08f, location.Latitude);
            Assert.AreEqual(.04f, location.Longitude);
            Assert.IsTrue(vertices.Contains(21));
            location = graph.GetVertex(21);
            Assert.AreEqual(.08f, location.Latitude);
            Assert.AreEqual(.06f, location.Longitude);
            Assert.IsTrue(vertices.Contains(22));
            location = graph.GetVertex(22);
            Assert.AreEqual(.08f, location.Latitude);
            Assert.AreEqual(.08f, location.Longitude);
            Assert.IsTrue(vertices.Contains(23));
            location = graph.GetVertex(23);
            Assert.AreEqual(.06f, location.Latitude);
            Assert.AreEqual(.08f, location.Longitude);
            Assert.IsTrue(vertices.Contains(24));
            location = graph.GetVertex(24);
            Assert.AreEqual(.06f, location.Latitude);
            Assert.AreEqual(.06f, location.Longitude);
        }

        /// <summary>
        /// Tests a geometric search fix.
        /// </summary>
        [Test]
        public void GeometricSearchRegressionTest()
        {
            // setup a routing network to test against.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network7.geojson"));
            routerDb.Sort();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            var vertex7 = routerDb.Network.GetVertex(7);
            var vertices = routerDb.Network.GeometricGraph.Search(vertex7.Latitude, vertex7.Longitude, 0.001f);
            Assert.AreEqual(1, vertices.Count);
            Assert.AreEqual(7, vertices.First());
        }

        /// <summary>
        /// Regression tests for issue: https://github.com/itinero/routing/issues/117
        /// </summary>
        [Test]
        public void SearchCloseVerticesTest()
        {
            var DefaultHilbertSteps = (int)System.Math.Pow(2, 15);

            // build locations.
            var locations = new List<Coordinate>();
            locations.Add(new Coordinate(53.06905f, 29.40519f));
            locations.Add(new Coordinate(53.1183f, 29.31932f));
            locations.Add(new Coordinate(53.08611f, 29.37143f));
            locations.Add(new Coordinate(53.05757f, 29.44653f));

            // build graph.
            var graph = new GeometricGraph(1);
            int vertex = 0;
            for (vertex = 0; vertex < locations.Count; vertex++)
            {
                graph.AddVertex((uint)vertex, locations[vertex].Latitude,
                    locations[vertex].Longitude);
            }

            // build a sorted version in-place.
            graph.Sort(DefaultHilbertSteps);
            var closelist1 = graph.Search(53.0695f, 29.40594f, 0.1f);
            Assert.AreEqual(4, closelist1.Count);
            
            // add another vertex.
            graph.AddVertex((uint)vertex, 52.7362f, 29.4935f);
            graph.Sort(DefaultHilbertSteps);

            // build a sorted version in-place.
            var closelist2 = graph.Search(53.0695f, 29.40594f, 0.1f);
            Assert.IsTrue(closelist2.Count >= 3, "some close vertex missed");
        }
    }
}