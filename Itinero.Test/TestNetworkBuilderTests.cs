// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric;
using Itinero.Network;
using System.Linq;
using System.Reflection;

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
                Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
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
    }
}