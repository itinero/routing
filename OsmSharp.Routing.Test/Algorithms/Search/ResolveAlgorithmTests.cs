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
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Algorithms.Search;
using OsmSharp.Routing.Graphs.Geometric;

namespace OsmSharp.Routing.Test.Algorithms.Search
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
        public void TestOnEdge()
        {
            var vertex0 = new GeoCoordinate(51.26779566943717f, 4.801347255706787f);
            var vertex1 = new GeoCoordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, null);

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve right in between.
            location = (vertex0 + vertex1) / 2;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }

        /// <summary>
        /// Tests resolving on an edge with a shape.
        /// </summary>
        [Test]
        public void TestOnEdgeWithShape()
        {
            var vertex0 = new GeoCoordinate(51.26779566943717f, 4.801347255706787f);
            var shape0 = new GeoCoordinate(51.26763791763357f, 4.801728129386902f);
            var shape1 = new GeoCoordinate(51.267070677950585f, 4.801749587059021f);
            var vertex1 = new GeoCoordinate(51.26689735000000f, 4.801347255706787f);

            var graph = new GeometricGraph(1);
            graph.AddVertex(0, (float)vertex0.Latitude, (float)vertex0.Longitude);
            graph.AddVertex(1, (float)vertex1.Latitude, (float)vertex1.Longitude);
            graph.AddEdge(0, 1, new uint[] { 0 }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                    shape0,
                    shape1
                }));

            // resolve on vertex0.
            var location = vertex0;
            var resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            var result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on vertex1.
            location = vertex1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape0.
            location = shape0;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape1.
            location = shape1;
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape0 a bit to the top-right.
            location = new GeoCoordinate(51.26771847181371f, 4.801915884017944f);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);

            // resolve on shape1 a bit to the bottom-right.
            location = new GeoCoordinate(51.266986766160414f, 4.8019373416900635f);
            resolver = new ResolveAlgorithm(graph, (float)location.Latitude, (float)location.Longitude, .01f, 50f,
                (edge) => { return true; });
            resolver.Run();

            result = resolver.Result;
            Assert.IsNotNull(result);
        }
    }
}