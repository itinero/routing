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
using Itinero.Algorithms.Search;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;

namespace Itinero.Test.Algorithms.Search
{
    /// <summary>
    /// Contains tests for the resolve algorithm.
    /// </summary>
    [TestFixture]
    class ResolveAlgorithmTests
    {
        /// <summary>
        /// Tests resolving on an edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, 
                Constants.DefaultMaxEdgeDistance, 50f,
                    (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve right in between.
            location = new Coordinate((vertex0.Latitude + vertex1.Latitude) / 2,
                (vertex0.Longitude + vertex1.Longitude) / 2);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on an edge with a shape.
        /// </summary>
        [Test]
        public void TestOneEdgeWithShape()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var shape0 = new Coordinate(51.26763791763357f, 4.801728129386902f);
            var shape1 = new Coordinate(51.267070677950585f, 4.801749587059021f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, vertex0.Latitude, vertex0.Longitude);
            graph.AddVertex(1, vertex1.Latitude, vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, shape0, shape1);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape0.
            location = shape0;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape1.
            location = shape1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape0 a bit to the top-right.
            location = new Coordinate(51.26771847181371f, 4.801915884017944f);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape1 a bit to the bottom-right.
            location = new Coordinate(51.266986766160414f, 4.8019373416900635f);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on an edge with an isBetter function.
        /// </summary>
        [Test]
        public void TestOneEdgeWithIsBetter()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve right in between.
            location = new Coordinate((vertex0.Latitude + vertex1.Latitude) / 2,
                (vertex0.Longitude + vertex1.Longitude) / 2);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on two edges with an isBetter function.
        /// </summary>
        [Test]
        public void TestTwoEdgesWithIsBetter()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);
            var vertex2 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, vertex0.Latitude, vertex0.Longitude);
            graph.AddVertex(1, vertex1.Latitude, vertex1.Longitude);
            graph.AddVertex(2, vertex2.Latitude, vertex2.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);
            var expectedEdgeId = graph.AddEdge(0, 2, new uint[] { 1 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve right in between.
            location = new Coordinate((vertex0.Latitude + vertex1.Latitude) / 2,
                (vertex0.Longitude + vertex1.Longitude) / 2);
            resolver = new ResolveAlgorithm(graph, location.Latitude, location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);
        }

        /// <summary>
        /// Tests resolving on an edge with a shape.
        /// </summary>
        [Test]
        public void TestTwoEdgesWithShapeWithIsBetter()
        {
            var vertex0 = new Coordinate(51.26779566943717f, 4.801347255706787f);
            var shape0 = new Coordinate(51.26763791763357f, 4.801728129386902f);
            var shape1 = new Coordinate(51.267070677950585f, 4.801749587059021f);
            var vertex1 = new Coordinate(51.26689735000000f, 4.801347255706787f);
            var vertex2 = new Coordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddVertex(2, (float)vertex2.Latitude, (float)vertex2.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, shape0, shape1);
            var expectedEdgeId = graph.AddEdge(0, 2, new uint[] { 1 }, shape0, shape1);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape0.
            location = shape0;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape1.
            location = shape1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape0 a bit to the top-right.
            location = new Coordinate(51.26771847181371f, 4.801915884017944f);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);

            // resolve on shape1 a bit to the bottom-right.
            location = new Coordinate(51.266986766160414f, 4.8019373416900635f);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, Constants.DefaultMaxEdgeDistance, 50f,
                (edge) => { return true; }, (edge) => { return edge.Data[0] == 1; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
            Assert.AreEqual(expectedEdgeId, result.EdgeId);
        }
    }
}