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
using OsmSharp.Collections;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams;
using System.Collections.Generic;
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Does some raw routing tests.
    /// </summary>
    [TestFixture]
    public class DykstraRoutingTests : SimpleRoutingTests<Edge>
    {
        /// <summary>
        /// Builds a router.
        /// </summary>
        /// <returns></returns>
        public override Router BuildRouter(IRoutingAlgorithmData<Edge> data, IRoutingInterpreter interpreter,
            IRoutingAlgorithm<Edge> basicRouter)
        {
            // initialize the router.
            return Router.CreateFrom(data, basicRouter, interpreter);
        }

        /// <summary>
        /// Builds a basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override IRoutingAlgorithm<Edge> BuildBasicRouter(IRoutingAlgorithmData<Edge> data)
        {
            return new Dykstra();
        }

        /// <summary>
        /// Builds data source.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedString"></param>
        /// <returns></returns>
        public override IRoutingAlgorithmData<Edge> BuildData(IOsmRoutingInterpreter interpreter,
            string embeddedString)
        {
            var key = string.Format("Dykstra.Routing.IRoutingAlgorithmData<SimpleWeighedEdge>.OSM.{0}",
                embeddedString);

            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(memoryData, interpreter, tagsIndex, null, false);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            return memoryData;
        }

        /// <summary>
        /// Tests a simple shortest route calculation.
        /// </summary>
        [Test]
        public void TestDykstraShortedDefault()
        {
            this.DoTestShortestDefault();
        }

        /// <summary>
        /// Tests if the raw router preserves tags.
        /// </summary>
        [Test]
        public void TestDykstraResolvedTags()
        {
            this.DoTestResolvedTags();
        }

        /// <summary>
        /// Tests if the raw router preserves tags on arcs/ways.
        /// </summary>
        [Test]
        public void TestDykstraEdgeTags()
        {
            this.DoTestEdgeTags();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortest1()
        {
            this.DoTestShortest1();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortest2()
        {
            this.DoTestShortest2();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortest3()
        {
            this.DoTestShortest3();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortest4()
        {
            this.DoTestShortest4();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortest5()
        {
            this.DoTestShortest5();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortestResolved1()
        {
            this.DoTestShortestResolved1();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestDykstraShortestResolved2()
        {
            this.DoTestShortestResolved2();
        }

        /// <summary>
        /// Test if the raw router many-to-many weights correspond to the point-to-point weights.
        /// </summary>
        [Test]
        public void TestDykstraManyToMany1()
        {
            this.DoTestManyToMany1();
        }

        /// <summary>
        /// Test if the raw router handles connectivity questions correctly.
        /// </summary>
        [Test]
        public void TestDykstraConnectivity1()
        {
            this.DoTestConnectivity1();
        }

        /// <summary>
        /// Tests a simple shortest route calculation.
        /// </summary>
        [Test]
        public void TestDykstraResolveAllNodes()
        {
            this.DoTestResolveAllNodes();
        }

        /// <summary>
        /// Regression test on routing resolved nodes.
        /// </summary>
        [Test]
        public void TestDykstraResolveBetweenRouteToSelf()
        {
            this.DoTestResolveBetweenRouteToSelf();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestDykstraResolveBetweenClose()
        {
            this.DoTestResolveBetweenClose();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestDykstraResolveCase1()
        {
            this.DoTestResolveCase1();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestDykstraResolveCase2()
        {
            this.DoTestResolveCase2();
        }

        /// <summary>
        /// Tests many to many routing.
        /// </summary>
        [Test]
        public void TestManyToManyBigNetwork()
        {
            this.DoTestManyToMany("test_network_big.osm");
        }

        ///// <summary>
        ///// Tests many to many routing.
        ///// </summary>
        //[Test]
        //public void TestManyToManyTSPReal()
        //{
        //    this.DoTestManyToMany("tsp_real.osm");
        //}
    }
}