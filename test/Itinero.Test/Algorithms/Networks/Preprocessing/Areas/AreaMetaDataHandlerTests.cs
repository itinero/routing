/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Algorithms.Networks.Preprocessing.Areas;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using System.Collections.Generic;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Networks.Preprocessing.Areas
{
    /// <summary>
    /// Contains tests for the area meta data handler.
    /// </summary>
    [TestFixture]
    public class AreaMetaDataHandlerTests
    {
        /// <summary>
        /// Tests the case where one edge is not included.
        /// </summary>
        [Test]
        public void TestOneEdgeExcluded()
        {
            var routerDb = new RouterDb();
            var network = routerDb.Network;
            network.AddVertex(0, 51.154800936865406f, 4.2070770263671875f);
            network.AddVertex(1, 51.159968767920475f, 4.2235565185546875f);
            var edgeId = network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 57.69f,
                Profile = 1,
                MetaId = 10
            });

            var area = new PolygonArea(new Polygon()
            {
                ExteriorRing = new List<Coordinate>(new Coordinate[]
                {
                    new Coordinate(51.27391736369659f, 4.799566268920898f),
                    new Coordinate(51.258531028690626f, 4.78879451751709f),
                    new Coordinate(51.2665873439388f, 4.817891120910644f),
                    new Coordinate(51.27391736369659f, 4.799566268920898f)
                })
            });

            var handler = new AreaMetaDataHandler(routerDb, area);
            handler.NewVertex += id => { Assert.Fail("This should not be called!"); };
            handler.NewEdge += (oldEdgeId, newEdgeId) => { Assert.Fail("This should not be called!"); };
            handler.EdgeInside += id => { Assert.Fail("This should not be called!"); };

            handler.Run();
            Assert.IsTrue(handler.HasSucceeded);
        }
        
        /// <summary>
        /// Tests the case where one edge is included.
        /// </summary>
        [Test]
        public void TestOneEdgeIncluded()
        {
            var routerDb = new RouterDb();
            var network = routerDb.Network;
            network.AddVertex(0, 51.268305f, 4.8014116287231445f);
            network.AddVertex(1, 51.2643048646482f, 4.797978401184082f);
            var edgeId = network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 57.69f,
                Profile = 1,
                MetaId = 10
            });

            var area = new PolygonArea(new Polygon()
            {
                ExteriorRing = new List<Coordinate>(new Coordinate[]
                {
                    new Coordinate(51.27391736369659f, 4.799566268920898f),
                    new Coordinate(51.258531028690626f, 4.78879451751709f),
                    new Coordinate(51.2665873439388f, 4.817891120910644f),
                    new Coordinate(51.27391736369659f, 4.799566268920898f)
                })
            });

            var handler = new AreaMetaDataHandler(routerDb, area);
            handler.NewVertex += id => { Assert.Fail("This should not be called!"); };
            handler.NewEdge += (oldEdgeId, newEdgeId) => { Assert.Fail("This should not be called!"); };
            handler.EdgeInside += id => { Assert.AreEqual(edgeId, id); };

            handler.Run();
            Assert.IsTrue(handler.HasSucceeded);
        }
        
        /// <summary>
        /// Tests the case where one edge is split in two.
        /// </summary>
        [Test]
        public void TestOneEdgeSplitInTwo()
        {
            var routerDb = new RouterDb();
            var network = routerDb.Network;
            network.AddVertex(0, 51.26830584177533f, 4.8014116287231445f);
            network.AddVertex(1, 51.27440062061442f, 4.810037612915039f);
            var edgeId = network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 906.00f,
                Profile = 1,
                MetaId = 10
            });
            var futureNewVertexId = network.VertexCount;

            var area = new PolygonArea(new Polygon()
            {
                ExteriorRing = new List<Coordinate>(new Coordinate[]
                {
                    new Coordinate(51.27391736369659f, 4.799566268920898f),
                    new Coordinate(51.258531028690626f, 4.78879451751709f),
                    new Coordinate(51.2665873439388f, 4.817891120910644f),
                    new Coordinate(51.27391736369659f, 4.799566268920898f)
                })
            });

            var handler = new AreaMetaDataHandler(routerDb, area);
            var newEdgeCalled = 0;
            var newEdges = new HashSet<uint>();
            handler.NewVertex += id => { Assert.AreEqual(futureNewVertexId, id); };
            handler.NewEdge += (oldEdgeId, newEdgeId) =>
            { // this should be called twice.
                newEdgeCalled++; 
                Assert.AreEqual(edgeId, oldEdgeId);
                newEdges.Add(newEdgeId);
            };
            var edgeInsideCalled = 0;
            handler.EdgeInside += id =>
            { // this should be called only once.
                Assert.AreNotEqual(edgeId, id);
                Assert.IsTrue(newEdges.Contains(id));
                edgeInsideCalled++;
                Assert.AreEqual(1, edgeInsideCalled);
            };

            handler.Run();
            Assert.IsTrue(handler.HasSucceeded);
            Assert.AreEqual(2, newEdgeCalled);
        }
    }
}