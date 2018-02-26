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

using NUnit.Framework;
using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Data.Network;
using System.Linq;
using System.Reflection;
using Itinero.Data.Network.Restrictions;
using Itinero.Data;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the test network builder.
    /// </summary>
    [TestFixture]
    public class TestNetworkBuilderTests
    {
        /// <summary>
        /// Tests building network 1.
        /// </summary>
        [Test]
        public void TestNetwork1()
        {
            var routerDb = new RouterDb(new RoutingNetwork(new GeometricGraph(1)),
                new AttributesIndex(),
                new AttributesIndex(),
                new AttributeCollection(),
                Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));

            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.22965768754021, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463152885437011, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.22961737711890, vertex1.Latitude, 0.00001);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 0.2);
            var edgeProfile = routerDb.EdgeProfiles.Get(
                edge.Data.Profile);
            Assert.IsTrue(edgeProfile.Contains("highway", "residential"));
            var edgeMeta = routerDb.EdgeMeta.Get(
                edge.Data.MetaId);
            Assert.IsTrue(edgeMeta.Contains("name", "Abelshausen Blvd."));

            MetaCollection<long> nodeIds;
            Assert.IsTrue(routerDb.VertexData.TryGet("node_id", out nodeIds));
            Assert.AreEqual(2, nodeIds.Count);
        }

        /// <summary>
        /// Tests building network 2.
        /// </summary>
        [Test]
        public void TestNetwork2()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

            var vertex2 = routerDb.Network.GetVertex(2);
            Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
            Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

            var vertex3 = routerDb.Network.GetVertex(3);
            Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
            Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

            var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

            var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

            var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
        }

        /// <summary>
        /// Tests building network 3.
        /// </summary>
        [Test]
        public void TestNetwork3()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network3.geojson"));

            Assert.AreEqual(9, routerDb.Network.VertexCount);
            Assert.AreEqual(10, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);
            var vertex9 = routerDb.Network.GetVertex(9);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 1);
            var profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 7);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex7), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(7).First(x => x.To == 6);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex7, vertex6), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(6).First(x => x.To == 4);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex6, vertex4), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(4).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex4, vertex3), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(3).First(x => x.To == 0);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex3, vertex0), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(5).First(x => x.To == 6);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex6, vertex5), edge.Data.Distance, 1);
            Assert.AreEqual(false, edge.DataInverted);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(2, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.IsTrue(profile.Contains("oneway", "yes"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 5);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex5, vertex2), edge.Data.Distance, 1);
            Assert.AreEqual(false, edge.DataInverted);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(2, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.IsTrue(profile.Contains("oneway", "yes"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex2, vertex1), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "pedestrian"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 8);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex2, vertex8), edge.Data.Distance, 1);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));
        }

        /// <summary>
        /// Tests building network 5.
        /// </summary>
        [Test]
        public void TestNetwork5()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network5.geojson"));

            Assert.AreEqual(18, routerDb.Network.VertexCount);
            Assert.AreEqual(21, routerDb.Network.EdgeCount);

            string name;
            var meta = routerDb.VertexMeta[1];
            Assert.IsNotNull(meta);
            Assert.IsTrue(meta.TryGetValue("name", out name));
            Assert.AreEqual("meta-value1", name);
            meta = routerDb.VertexMeta[3];
            Assert.IsNotNull(meta);
            Assert.IsTrue(meta.TryGetValue("name", out name));
            Assert.AreEqual("meta-value3", name);
            meta = routerDb.VertexMeta[8];
            Assert.IsNotNull(meta);
            Assert.IsTrue(meta.TryGetValue("name", out name));
            Assert.AreEqual("meta-value8", name);
            meta = routerDb.VertexMeta[14];
            Assert.IsNotNull(meta);
            Assert.IsTrue(meta.TryGetValue("name", out name));
            Assert.AreEqual("meta-value14", name);
        }

        /// <summary>
        /// Tests building network 7 (with a 3-vertex restriction).
        /// </summary>
        [Test]
        public void TestNetwork7()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network7.geojson"));

            Assert.AreEqual(8, routerDb.Network.VertexCount);
            Assert.AreEqual(8, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);
            var vertex9 = routerDb.Network.GetVertex(9);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 5);
            var profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 4);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex4), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex2, vertex3), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 5);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex2, vertex5), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(4).First(x => x.To == 5);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex4, vertex5), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(4).First(x => x.To == 6);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex4, vertex6), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(5).First(x => x.To == 7);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex5, vertex7), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            RestrictionsDb restrictions = null;
            Assert.IsTrue(routerDb.TryGetRestrictions(string.Empty, out restrictions));
            var enumerator = restrictions.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(0));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(2, enumerator[0]);
            Assert.AreEqual(1, enumerator[1]);
            Assert.AreEqual(0, enumerator[2]);
        }

        /// <summary>
        /// Tests building network 12 (with a 1-vertex restriction).
        /// </summary>
        [Test]
        public void TestNetwork12()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network12.geojson"));

            Assert.AreEqual(8, routerDb.Network.VertexCount);
            Assert.AreEqual(8, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);
            var vertex9 = routerDb.Network.GetVertex(9);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 5);
            var profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 4);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex4), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex2, vertex3), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(2).First(x => x.To == 5);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex2, vertex5), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(4).First(x => x.To == 5);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex4, vertex5), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(4).First(x => x.To == 6);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex4, vertex6), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            edge = routerDb.Network.GetEdgeEnumerator(5).First(x => x.To == 7);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex5, vertex7), edge.Data.Distance, 5);
            profile = routerDb.EdgeProfiles.Get(edge.Data.Profile);
            Assert.AreEqual(1, profile.Count);
            Assert.IsTrue(profile.Contains("highway", "residential"));

            RestrictionsDb restrictions = null;
            Assert.IsTrue(routerDb.TryGetRestrictions("motorcar", out restrictions));
            var enumerator = restrictions.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(1, enumerator[0]);
        }
    }
}