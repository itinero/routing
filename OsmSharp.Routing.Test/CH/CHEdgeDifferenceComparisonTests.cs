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
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.CH.Preprocessing.Ordering;
using OsmSharp.Routing.CH.Preprocessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;
using System.Reflection;

namespace OsmSharp.Test.Unittests.Routing.CH
{
    /// <summary>
    /// Tests the CH Sparse routing against a reference implementation.
    /// </summary>
    [TestFixture]
    public class CHEdgeDifferenceComparisonTests : RoutingComparisonTestsBase
    {
        /// <summary>
        /// Holds the data.
        /// </summary>
       private Dictionary<string, RouterDataSource<CHEdgeData>> _data = null;

        /// <summary>
        /// Returns a new router.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedName"></param>
        /// <returns></returns>
        public override Router BuildRouter(IOsmRoutingInterpreter interpreter, string embeddedName)
        {
            if (_data == null)
            {
                _data = new Dictionary<string, RouterDataSource<CHEdgeData>>();
            }
            RouterDataSource<CHEdgeData> data = null;
            if (!_data.TryGetValue(embeddedName, out data))
            {
                var tagsIndex = new TagsIndex();

                // do the data processing.
                data = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
                var targetData = new CHEdgeGraphOsmStreamTarget(
                    data, interpreter, tagsIndex, Vehicle.Car);
                var dataProcessorSource = new XmlOsmStreamSource(
                    Assembly.GetExecutingAssembly().GetManifestResourceStream(string.Format(
                    "OsmSharp.Test.Unittests.{0}", embeddedName)));
                var sorter = new OsmStreamFilterSort();
                sorter.RegisterSource(dataProcessorSource);
                targetData.RegisterSource(sorter);
                targetData.Pull();

                _data[embeddedName] = data;
            }
            return Router.CreateCHFrom(data, new CHRouter(), interpreter);
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceAgainstReference()
        {
            this.TestCompareAll("test_network.osm");
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceAgainstReferenceBig()
        {
            this.TestCompareAll("test_network_big.osm");
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceOneWayAgainstReference()
        {
            this.TestCompareAll("test_network_oneway.osm");
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceRegression1()
        {
            this.TestCompareAll("test_routing_regression1.osm");
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceBig()
        {
            this.TestCompareAll("test_network_big.osm");
        }

        /// <summary>
        /// Compares all routes possible against a reference implementation.
        /// </summary>
        [Test]
        public void TestCHEdgeDifferenceAgainstReferenceRealNetwork()
        {
            this.TestCompareAll("test_network_real1.osm");
        }
    }
}