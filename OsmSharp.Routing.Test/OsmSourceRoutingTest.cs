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

//using NUnit.Framework;
//using OsmSharp.Osm.Xml.OsmSource;
//using OsmSharp.Test.Unittests.Routing;
//using OsmSharp.Routing;
//using System.Reflection;
//using OsmSharp.Routing.Interpreter;
//using OsmSharp.Routing.Graph.Routing;
//using OsmSharp.Routing.Graph.Routing.Dykstra;
//using OsmSharp.Routing.Osm.Graphs;
//using OsmSharp.Collections.Tags;

//namespace OsmSharp.Test.Unittests.Routing.Dykstra
//{
//    /// <summary>
//    /// Does some tests on an OsmSource routing implementation.
//    /// </summary>
//    [TestFixture]
//    public class OsmSourceRoutingTests : SimpleRoutingTests<Edge>
//    {
//        /// <summary>
//        /// Builds a router.
//        /// </summary>
//        /// <returns></returns>
//        public override Router BuildRouter(IRoutingAlgorithmData<Edge> data,
//            IRoutingInterpreter interpreter, IRoutingAlgorithm<Edge> basicRouter)
//        {
//            // initialize the router.
//            return Router.CreateFrom(data, basicRouter, interpreter);
//        }

//        /// <summary>
//        /// Builds a basic router.
//        /// </summary>
//        /// <param name="data"></param>
//        /// <returns></returns>
//        public override IRoutingAlgorithm<Edge> BuildBasicRouter(IRoutingAlgorithmData<Edge> data)
//        {
//            return new Dykstra(data.TagsIndex);
//        }

//        /// <summary>
//        /// Builds data source.
//        /// </summary>
//        /// <param name="interpreter"></param>
//        /// <param name="embeddedString"></param>
//        /// <returns></returns>
//        public override IRoutingAlgorithmData<Edge> BuildData(IRoutingInterpreter interpreter,
//            string embeddedString)
//        {
//            var tagsIndex = new SimpleTagsIndex();
            
//            // do the data processing.
//            var source = new OsmDataSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString)); // "OsmSharp.Test.Unittests.test_network.osm"));
//            return new OsmSourceRouterDataSource(interpreter, tagsIndex, source);
//        }

//        /// <summary>
//        /// Tests a simple shortest route calculation.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortedDefault()
//        {
//            this.DoTestShortestDefault();
//        }

//        /// <summary>
//        /// Tests if the raw router preserves tags.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceResolvedTags()
//        {
//            this.DoTestResolvedTags();
//        }

//        /// <summary>
//        /// Tests if the raw router preserves tags on arcs/ways.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceArcTags()
//        {
//            this.DoTestArcTags();
//        }

//        /// <summary>
//        /// Test is the raw router can calculate another route.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortest1()
//        {
//            this.DoTestShortest1();
//        }

//        /// <summary>
//        /// Test is the raw router can calculate another route.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortest2()
//        {
//            this.DoTestShortest2();
//        }

//        /// <summary>
//        /// Test is the raw router can calculate another route.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortest3()
//        {
//            this.DoTestShortest3();
//        }

//        /// <summary>
//        /// Test is the raw router can calculate another route.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortest4()
//        {
//            this.DoTestShortest4();
//        }

//        /// <summary>
//        /// Test is the raw router can calculate another route.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortest5()
//        {
//            this.DoTestShortest5();
//        }

//        /// <summary>
//        /// Test is the raw router can calculate another route.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceShortestResolved1()
//        {
//            this.DoTestShortestResolved1();
//        }

//        /// <summary>
//        /// Test if the raw router many-to-many weights correspond to the point-to-point weights.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceManyToMany1()
//        {
//            this.DoTestManyToMany1();
//        }

//        /// <summary>
//        /// Test if the raw router handles connectivity questions correctly.
//        /// </summary>
//        [Test]
//        public void TestOsmSourceConnectivity1()
//        {
//            this.DoTestConnectivity1();
//        }
//    }
//}