//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
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
//using OsmSharp.Collections.Tags;
//using OsmSharp.Osm.Xml.Streams;
//using OsmSharp.Routing;
//using OsmSharp.Routing.CH;
//using OsmSharp.Routing.CH.Preprocessing;
//using OsmSharp.Routing.Graph.Routing;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Osm.Streams.Graphs;
//using System;
//using System.Diagnostics;
//using System.IO;
//using System.Reflection;

//namespace OsmSharp.Test.Unittests.Routing.CH.Serialization.Sorted
//{
//    /// <summary>
//    /// Tests comparing routing before/after serilization.
//    /// </summary>
//    [TestFixture]
//    public class CHRoutingSerializationComparizonTests : RoutingComparisonTests
//    {
//        /// <summary>
//        /// Tests serializing/deserializing RoutingSerializationRoutingComparisonTest using the 
//        /// V3 routing serializer.
//        /// </summary>
//        [Test]
//        public void RoutingSerializationV2CHRoutingV2ComparisonTestNetwork()
//        {
//            const string embeddedString = "OsmSharp.Test.Unittests.test_network.osm";

//            this.DoRoutingSerializationV2CHRoutingV2ComparisonTest(embeddedString);
//        }

//        /// <summary>
//        /// Tests serializing/deserializing RoutingSerializationRoutingComparisonTest using the 
//        /// V3 routing serializer.
//        /// </summary>
//        [Test]
//        public void RoutingSerializationV2CHRoutingV2ComparisonTestRealNetwork1()
//        {
//            const string embeddedString = "OsmSharp.Test.Unittests.test_network_real1.osm";

//            this.DoRoutingSerializationV2CHRoutingV2ComparisonTest(embeddedString);
//        }

//        /// <summary>
//        /// Tests serializing/deserializing RoutingSerializationRoutingComparisonTest using the V3 routing serializer.
//        /// </summary>
//        [Test]
//        public void RoutingSerializationV2CHRoutingV2ComparisonTestNetworkBig()
//        {
//            const string embeddedString = "OsmSharp.Test.Unittests.test_network_big.osm";

//            this.DoRoutingSerializationV2CHRoutingV2ComparisonTest(embeddedString);
//        }

//        /// <summary>
//        /// Does the actual testing.
//        /// </summary>
//        /// <param name="embeddedString"></param>
//        private void DoRoutingSerializationV2CHRoutingV2ComparisonTest(string embeddedString)
//        {
//            // creates a new interpreter.
//            var interpreter = new OsmRoutingInterpreter();

//            // do the data processing.
//            var original = CHEdgeGraphOsmStreamTarget.Preprocess(new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString)),
//                    interpreter, Vehicle.Car);

//            // create serializer.
//            var routingSerializer = new OsmSharp.Routing.CH.Serialization.Sorted.CHEdgeDataDataSourceSerializer(false);

//            // serialize/deserialize.
//            TagsCollectionBase metaData = new TagsCollection();
//            metaData.Add("some_key", "some_value");
//            byte[] byteArray;
//            using (var stream = new MemoryStream())
//            {
//                try
//                {
//                    routingSerializer.Serialize(stream, original, metaData);
//                    byteArray = stream.ToArray();
//                }
//                catch (Exception)
//                {
//                    if (Debugger.IsAttached)
//                    {
//                        Debugger.Break();
//                    }
//                    throw;
//                }
//            }

//            var deserializedVersion = routingSerializer.Deserialize(new MemoryStream(byteArray), out metaData);
//            Assert.AreEqual(original.TagsIndex.Get(0), deserializedVersion.TagsIndex.Get(0));
//            Assert.IsTrue(deserializedVersion.SupportsProfile(Vehicle.Car));
//            Assert.IsFalse(deserializedVersion.SupportsProfile(Vehicle.Bicycle));

//            // create reference router.
//            original = CHEdgeGraphOsmStreamTarget.Preprocess(new XmlOsmStreamSource(
//                Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedString)),
//                    interpreter, Vehicle.Car);
//            var basicRouterOriginal = new CHRouter();
//            var referenceRouter = Router.CreateCHFrom(original, basicRouterOriginal, interpreter);

//            // try to do some routing on the deserialized version.
//            var basicRouter = new CHRouter();
//            var router = Router.CreateCHFrom(deserializedVersion, basicRouter, interpreter);

//            this.TestCompareAll(original, referenceRouter, router);
//        }

//        /// <summary>
//        /// Not needed here.
//        /// </summary>
//        /// <param name="interpreter"></param>
//        /// <param name="embeddedName"></param>
//        /// <param name="contract"></param>
//        /// <returns></returns>
//        public override Router BuildRouter(IOsmRoutingInterpreter interpreter, string embeddedName, bool contract)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
