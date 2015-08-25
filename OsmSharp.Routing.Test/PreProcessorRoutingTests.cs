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

using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graphs.PreProcessing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Contains tests for the router after pre-processing using the pre processor tests.
    /// </summary>
    [TestFixture]
    public class PreProcessorRoutingTests
    {
        /// <summary>
        /// Tests a simple removal of one sparse vertex and then serveral different routing queries.
        /// from    1 - 2 - 3
        ///     
        /// to      1 - 2
        /// </summary>
        [Test]
        public void TestSparseRemoval1Routing()
        {
            // use one edge definition everywhere.
            var tagsIndex = new TagsIndex();
            var tags = new TagsCollection(new Tag("highway","residential"));
            var edge = new Edge();
            edge.Forward = true;
            edge.Tags = tagsIndex.Add(tags);

            var graph = new Graph<Edge>();
            uint vertex1 = graph.AddVertex(51.267797f, 4.8013623f);
            uint vertex2 = graph.AddVertex(51.267702f, 4.8013396f);
            uint vertex3 = graph.AddVertex(51.267592f, 4.8013024f);

            graph.AddEdge(vertex1, vertex2, edge, null);
            graph.AddEdge(vertex2, vertex3, edge, null);
            graph.AddEdge(vertex3, vertex2, edge, null);

            // save vertex coordinates for later use.
            float latitude, longitude;
            graph.GetVertex(vertex1, out latitude, out longitude);
            var vertex1Coordinate = new GeoCoordinate(latitude, longitude);
            graph.GetVertex(vertex2, out latitude, out longitude);
            var vertex2Coordinate = new GeoCoordinate(latitude, longitude);
            graph.GetVertex(vertex3, out latitude, out longitude);
            var vertex3Coordinate = new GeoCoordinate(latitude, longitude);

            // execute pre-processor.
            var preProcessor = new GraphSimplificationPreprocessor(graph);
            preProcessor.Start();

            // create router.
            var source = new RouterDataSource<Edge>(
                graph, tagsIndex);
            var router = Router.CreateFrom(source, new OsmRoutingInterpreter());

            // test some basic routing requests.
            // 1 -> 3: 1 -> 2 -> 3.
            var resolved1 = router.Resolve(Vehicle.Car, vertex1Coordinate);
            var resolved3 = router.Resolve(Vehicle.Car, vertex3Coordinate);
            var route = router.Calculate(Vehicle.Car, resolved1, resolved3);
            
            // verify the simple route result.
            Assert.IsNotNull(route);
            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(vertex1Coordinate.Latitude, route.Segments[0].Latitude);
            Assert.AreEqual(vertex1Coordinate.Longitude, route.Segments[0].Longitude);
            Assert.AreEqual(vertex2Coordinate.Latitude, route.Segments[1].Latitude);
            Assert.AreEqual(vertex2Coordinate.Longitude, route.Segments[1].Longitude);
            Assert.AreEqual(vertex3Coordinate.Latitude, route.Segments[2].Latitude);
            Assert.AreEqual(vertex3Coordinate.Longitude, route.Segments[2].Longitude);

            // 1 -> 2: 1 -> 2.
            router = Router.CreateFrom(source, new OsmRoutingInterpreter());
            resolved1 = router.Resolve(Vehicle.Car, vertex1Coordinate);
            var resolved2 = router.Resolve(Vehicle.Car, vertex2Coordinate);
            route = router.Calculate(Vehicle.Car, resolved1, resolved2);

            // verify the simple route result.
            Assert.IsNotNull(route);
            Assert.AreEqual(2, route.Segments.Length);
            Assert.AreEqual(vertex1Coordinate.Latitude, route.Segments[0].Latitude);
            Assert.AreEqual(vertex1Coordinate.Longitude, route.Segments[0].Longitude);
            Assert.AreEqual(vertex2Coordinate.Latitude, route.Segments[1].Latitude);
            Assert.AreEqual(vertex2Coordinate.Longitude, route.Segments[1].Longitude);
        }
    }
}