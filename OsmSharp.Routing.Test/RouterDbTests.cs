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
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Network;
using System.IO;
using System.Linq;

namespace OsmSharp.Routing.Test
{
    /// <summary>
    /// Contains tests for the router db.
    /// </summary>
    [TestFixture]
    public class RouterDbTests
    {
        /// <summary>
        /// Tests saving and then loading test network1.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork1()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedProfile(OsmSharp.Routing.Osm.Vehicles.Vehicle.Car.Fastest());
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "OsmSharp.Routing.Test.test_data.networks.network1.geojson"));

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream);
            }

            // check serialized.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.22965768754021, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463152885437011, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.22961737711890, vertex1.Latitude, 0.00001);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 0.2);
            var edgeProfile = routerDb.EdgeProfiles.Get(
                edge.Data.Profile);
            Assert.IsTrue(edgeProfile.ContainsKeyValue("highway", "residential"));
            var edgeMeta = routerDb.EdgeMeta.Get(
                edge.Data.MetaId);
            Assert.IsTrue(edgeMeta.ContainsKeyValue("name", "Abelshausen Blvd."));
        }
    }
}