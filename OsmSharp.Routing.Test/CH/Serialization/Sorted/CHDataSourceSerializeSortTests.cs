//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2013 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using System.Reflection;
//using NUnit.Framework;
//using OsmSharp.Math.Geo;
//using OsmSharp.Osm.Xml.Streams;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Osm.Streams.Graphs;
//using OsmSharp.Routing;
//using OsmSharp.Routing.CH.Serialization.Sorted;
//using OsmSharp.Routing.CH;
//using OsmSharp.Routing.Graph;
//using OsmSharp.Routing.CH.Preprocessing;

//namespace OsmSharp.Test.Unittests.Routing.CH.Serialization.Sorted
//{
//    /// <summary>
//    /// Holds test for the toplogical sorting of a graph.
//    /// </summary>
//    [TestFixture]
//    public class CHDataSourceSerializeSortTests
//    {
//        /// <summary>
//        /// Tests a topological sorted datasource.
//        /// </summary>
//        [Test]
//        public void TestCHDataSourceTopologicalSortTest()
//        {
//            const string embeddedString = "OsmSharp.Routing.Test.data.test_network_real1.osm";

//            // creates a new interpreter.
//            var interpreter = new OsmRoutingInterpreter();

//            // do the data processing.
//            var original = CHEdgeGraphOsmStreamTarget.Preprocess(new XmlOsmStreamSource(
//                                                                   Assembly.GetExecutingAssembly()
//                                                                           .GetManifestResourceStream(embeddedString)),
//                                                               interpreter,
//                                                               Vehicle.Car);
//            CHEdgeDataDataSourceSerializer serializer = new CHEdgeDataDataSourceSerializer();
//            var sortedGraph = serializer.SortGraph(original);
//            original = CHEdgeGraphOsmStreamTarget.Preprocess(new XmlOsmStreamSource(
//                                                                   Assembly.GetExecutingAssembly()
//                                                                           .GetManifestResourceStream(embeddedString)),
//                                                               interpreter,
//                                                               Vehicle.Car);
//            var basicRouterOriginal = new CHRouter();
//            Router referenceRouter = Router.CreateCHFrom(
//                original, basicRouterOriginal, interpreter);

//            // try to do some routing on the deserialized version.
//            var basicRouter = new CHRouter();
//            Router router = Router.CreateCHFrom(
//                new RouterDataSource<CHEdgeData>(sortedGraph, original.TagsIndex), basicRouter, interpreter);

//            // loop over all nodes and resolve their locations.
//            var resolvedReference = new RouterPoint[original.VertexCount];
//            var resolved = new RouterPoint[original.VertexCount];
//            for (uint idx = 1; idx < original.VertexCount + 1; idx++)
//            { // resolve each vertex.
//                float latitude, longitude;
//                if (original.GetVertex(idx, out latitude, out longitude))
//                {
//                    resolvedReference[idx - 1] = referenceRouter.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude));
//                    resolved[idx - 1] = router.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude));
//                }

//                Assert.IsNotNull(resolvedReference[idx - 1]);
//                Assert.IsNotNull(resolved[idx - 1]);

//                //Assert.AreEqual(resolvedReference[idx - 1].Location.Latitude,
//                //    resolved[idx - 1].Location.Latitude, 0.0001);
//                //Assert.AreEqual(resolvedReference[idx - 1].Location.Longitude,
//                //    resolved[idx - 1].Location.Longitude, 0.0001);
//            }

//            //// check all the routes having the same weight(s).
//            //for (int fromIdx = 0; fromIdx < resolved.Length; fromIdx++)
//            //{
//            //    for (int toIdx = 0; toIdx < resolved.Length; toIdx++)
//            //    {
//            //        OsmSharpRoute referenceRoute = referenceRouter.Calculate(Vehicle.Car,
//            //            resolvedReference[fromIdx], resolvedReference[toIdx]);
//            //        OsmSharpRoute route = router.Calculate(Vehicle.Car,
//            //            resolved[fromIdx], resolved[toIdx]);

//            //        Assert.IsNotNull(referenceRoute);
//            //        Assert.IsNotNull(route);
//            //        //Assert.AreEqual(referenceRoute.TotalDistance, route.TotalDistance, 0.1);
//            //        // TODO: meta data is missing in some CH routing; see issue 
//            //        //Assert.AreEqual(reference_route.TotalTime, route.TotalTime, 0.0001);
//            //    }
//            //}
//        }
//    }
//}