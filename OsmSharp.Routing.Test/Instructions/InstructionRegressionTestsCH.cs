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

using System.Reflection;
using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.CH.Preprocessing.Ordering;
using OsmSharp.Routing.CH.Preprocessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Math.Geo;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing.Instructions
{
    /// <summary>
    /// Holds regression tests based on dykstra routing.
    /// </summary>
    [TestFixture]
    public class InstructionRegressionTestsCH : InstructionRegressionTestsBase
    {
        /// <summary>
        /// Creates a router.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="manifestResourceName"></param>
        /// <returns></returns>
        protected override Router CreateRouter(IOsmRoutingInterpreter interpreter, string manifestResourceName)
        {
            TagsIndex tagsIndex = new TagsIndex();

            // do the data processing.
            var data = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            var targetData = new CHEdgeGraphOsmStreamTarget(
                data, interpreter, tagsIndex, Vehicle.Car);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(manifestResourceName));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            // do the pre-processing part.
            var witnessCalculator = new DykstraWitnessCalculator();
            var preProcessor = new CHPreprocessor(data,
                new EdgeDifferenceContractedSearchSpace(data, witnessCalculator), witnessCalculator);
            preProcessor.Start();

            return Router.CreateCHFrom(data, new CHRouter(), interpreter);
        }

        /// <summary>
        /// Issue with generating instructions.
        /// </summary>
        [Test]
        public void InstructionRegressionCHTest2()
        {
            this.DoInstructionComparisonTest("OsmSharp.Test.Unittests.test_routing_regression1.osm", 
                new GeoCoordinate(51.01257, 4.000753),
                new GeoCoordinate(51.01250, 4.000013));
        }

        /// <summary>
        /// Issue with generating instructions.
        /// </summary>
        [Test]
        public void InstructionRegressionCHTest3()
        {
            this.DoInstructionComparisonTest("OsmSharp.Test.Unittests.test_routing_regression1.osm",
                new GeoCoordinate(51.01177, 4.00249),
                new GeoCoordinate(51.01250, 4.000013));
        }

        /// <summary>
        /// Issue with generating instructions.
        /// </summary>
        [Test]
        public void InstructionRegressionCHTest4()
        {
            this.DoInstructionComparisonTest("OsmSharp.Test.Unittests.test_routing_regression1.osm",
                new GeoCoordinate(51.01173, 4.00246),
                new GeoCoordinate(51.01250, 4.000013));
        }
    }
}