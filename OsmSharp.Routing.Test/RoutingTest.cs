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
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Tests the sparse node ordering CH.
    /// </summary>
    [TestFixture]
    public class RoutingTest : SimpleRoutingTests<Edge>
    {
        /// <summary>
        /// Returns a new router.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="interpreter"></param>
        /// <param name="basicRouter"></param>
        /// <returns></returns>
        public override Router BuildRouter(IRoutingAlgorithmData<Edge> data,
            IRoutingInterpreter interpreter, IRoutingAlgorithm<Edge> basicRouter)
        {
            return Router.CreateFrom(data, basicRouter, interpreter);
        }

        /// <summary>
        /// Returns a basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public override IRoutingAlgorithm<Edge> BuildBasicRouter(IRoutingAlgorithmData<Edge> data)
        {
            return new Dykstra();
        }

        /// <summary>
        /// Builds the data.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedString"></param>
        /// <returns></returns>
        public override IRoutingAlgorithmData<Edge> BuildData(IOsmRoutingInterpreter interpreter,
            string embeddedString)
        {
            var key = string.Format("Routing.IRoutingAlgorithmData<Edge>.OSM.{0}",
                embeddedString);

            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(
                memoryData, interpreter, tagsIndex, null, false);
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
        public void TestShortedDefault()
        {
            this.DoTestShortestDefault();
        }

        /// <summary>
        /// Tests if the raw router preserves tags.
        /// </summary>
        [Test]
        public void TestResolvedTags()
        {
            this.DoTestResolvedTags();
        }

        /// <summary>
        /// Tests if the raw router preserves tags on arcs/ways.
        /// </summary>
        [Test]
        public void TestArcTags()
        {
            this.DoTestEdgeTags();
        }

        /// <summary>
        /// Tests one shortest route calculation.
        /// </summary>
        [Test]
        public void TestShortest1()
        {
            this.DoTestShortest1();
        }

        /// <summary>
        /// Tests another shortest route calculation.
        /// </summary>
        [Test]
        public void TestShortest2()
        {
            this.DoTestShortest2();
        }

        /// <summary>
        /// Test is the CH router can calculate another route.
        /// </summary>
        [Test]
        public void TestShortest3()
        {
            this.DoTestShortest3();
        }

        /// <summary>
        /// Test is the CH router can calculate another route.
        /// </summary>
        [Test]
        public void TestShortest4()
        {
            this.DoTestShortest4();
        }

        /// <summary>
        /// Test is the CH router can calculate another route.
        /// </summary>
        [Test]
        public void TestShortest5()
        {
            this.DoTestShortest5();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestResolvedShortest1()
        {
            this.DoTestShortestResolved1();
        }

        /// <summary>
        /// Test is the raw router can calculate another route.
        /// </summary>
        [Test]
        public void TestResolvedShortest2()
        {
            this.DoTestShortestResolved2();
        }

        /// <summary>
        /// Test if the ch router many-to-many weights correspond to the point-to-point weights.
        /// </summary>
        [Test]
        public void TestManyToMany1()
        {
            this.DoTestManyToMany1();
        }

        /// <summary>
        /// Tests a simple shortest route calculation.
        /// </summary>
        [Test]
        public void TestResolveAllNodes()
        {
            this.DoTestResolveAllNodes();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestResolveBetweenClose()
        {
            this.DoTestResolveBetweenClose();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestResolveBetweenTwo()
        {
            this.DoTestResolveBetweenTwo();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestResolveCase1()
        {
            this.DoTestResolveCase1();
        }

        /// <summary>
        /// Tests routing when resolving points.
        /// </summary>
        [Test]
        public void TestResolveCase2()
        {
            this.DoTestResolveCase2();
        }

        /// <summary>
        /// Tests argument checks on router.
        /// </summary>
        [Test]
        public void TestArgumentNullExceptions()
        {
            this.DoTestArgumentChecks("test_network.osm");
        }
    }
}