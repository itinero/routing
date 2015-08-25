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
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Graph.Routing;
using System.Reflection;
using OsmSharp.Collections.Tags;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Does some raw routing tests.
    /// </summary>
    [TestFixture]
    public class DykstraAccessTests : RoutingAccessTests<Edge>
    {
        /// <summary>
        /// Builds a router.
        /// </summary>
        /// <returns></returns>
        public override Router BuildRouter(IRoutingAlgorithmData<Edge> data,
            IOsmRoutingInterpreter interpreter,
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
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public override IRoutingAlgorithmData<Edge> BuildData(IOsmRoutingInterpreter interpreter,
                                                                            string embeddedString, Vehicle vehicle)
        {
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData =
                new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(
                memoryData, interpreter, tagsIndex);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            return memoryData;
        }

        /// <summary>
        /// Test access restrictions.
        /// </summary>
        [Test]
        public void TestDykstraAccessHighways()
        {
            this.DoAccessTestsHighways();
        }
    }
}