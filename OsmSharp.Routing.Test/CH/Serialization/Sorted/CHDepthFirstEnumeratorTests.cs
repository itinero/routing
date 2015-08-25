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

//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using NUnit.Framework;
//using OsmSharp.Osm.Xml.Streams;
//using OsmSharp.Routing;
//using OsmSharp.Routing.CH.Preprocessing;
//using OsmSharp.Routing.CH.Serialization.Sorted;
//using OsmSharp.Routing.Osm.Interpreter;
//using OsmSharp.Routing.Osm.Streams.Graphs;

//namespace OsmSharp.Test.Unittests.Routing.CH.Serialization.Sorted
//{
//    /// <summary>
//    /// Tests for the CH depth first enumerator.
//    /// </summary>
//    [TestFixture]
//    public class CHDepthFirstEnumeratorTests
//    {
//        /// <summary>
//        /// Tests the CH depth first enumerator.
//        /// </summary>
//        [Test]
//        public void TestCHDepthFirstEnumerator()
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

//            //// add the downward edges.
//            //original.AddDownwardEdges();

//            // enumerate using depth-first search.
//            var enumerator =
//                new CHDepthFirstEnumerator(original);
//            HashSet<uint> vertices = new HashSet<uint>(
//                enumerator.Select(x => x.VertexId));
//            for (uint vertexId = 1; vertexId < original.VertexCount + 1; vertexId++)
//            {
//                float latitude, longitude;
//                if (original.GetVertex(vertexId, out latitude, out longitude))
//                {
//                    Assert.IsTrue(vertices.Contains(vertexId));
//                }
//            }
//        }
//    }
//}