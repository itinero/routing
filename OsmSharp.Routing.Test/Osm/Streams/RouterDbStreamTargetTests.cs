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
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Osm;
using OsmSharp.Osm.Streams;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Osm.Vehicles;
using System.Linq;

namespace OsmSharp.Routing.Test.Osm.Streams
{
    /// <summary>
    /// Contains tests for the router db stream target.
    /// </summary>
    [TestFixture]
    class RouterDbStreamTargetTests
    {
        /// <summary>
        /// Tests loading one way.
        /// </summary>
        [Test]
        public void TestOneWay()
        {
            // build source stream.
            var location1 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2)
                }.ToOsmStreamSource();

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, 51.265016473294075f, 4.7835588455200195f);
            Assert.AreNotEqual(uint.MaxValue, vertex1);
            var vertex2 = this.FindVertex(routerDb, 51.265016473294075f, 4.7907257080078125f);
            Assert.AreNotEqual(uint.MaxValue, vertex2);

            // get edge-information.
            var edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            var data = edges.First().Data;
            var profile = routerDb.EdgeProfiles.Get(data.Profile);
            var meta = routerDb.EdgeMeta.Get(data.MetaId);

            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);
        }

        /// <summary>
        /// Tests loading one way that's oneway.
        /// </summary>
        [Test]
        public void TestOneWayOneway()
        {
            // build source stream.
            var location1 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential"),
                    Tag.Create("oneway", "yes")), 1, 2)
                }.ToOsmStreamSource();

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, 51.265016473294075f, 4.7835588455200195f);
            Assert.AreNotEqual(uint.MaxValue, vertex1);
            var vertex2 = this.FindVertex(routerDb, 51.265016473294075f, 4.7907257080078125f);
            Assert.AreNotEqual(uint.MaxValue, vertex2);

            // get edge-information.
            var edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            var data = edges.First().Data;
            var profile = routerDb.EdgeProfiles.Get(data.Profile);
            var meta = routerDb.EdgeMeta.Get(data.MetaId);

            Assert.IsFalse(edges.First().DataInverted);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential"),
                new Tag("oneway", "yes")), profile);
            Assert.AreEqual(new TagsCollection(), meta);
        }

        /// <summary>
        /// Tests loading one incomplete way.
        /// </summary>
        [Test]
        public void TestOneWayIncomplete()
        {
            // build source stream.
            var location1 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3)
                }.ToOsmStreamSource();

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(1, routerDb.Network.VertexCount);

            var vertex1 = this.FindVertex(routerDb, 51.265016473294075f, 4.7835588455200195f);
            Assert.AreNotEqual(uint.MaxValue, vertex1);

            // another version of the same incomplete way.
            source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 3, 2, 1)
                }.ToOsmStreamSource();

            // build db from stream.
            routerDb = new RouterDb();
            target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(0, routerDb.Network.VertexCount);
        }

        /// <summary>
        /// Tests loading two ways.
        /// </summary>
        [Test]
        public void TestTwoWays()
        {
            // build source stream.
            var location1 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var location3 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7978925704956050f };
            var location4 = new GeoCoordinateSimple() { Latitude = 51.269567822699510f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Node.Create(4, location4.Latitude, location4.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 2, 4)
                }.ToOsmStreamSource();

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex1);
            var vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex2);
            var vertex3 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex3);
            var vertex4 = this.FindVertex(routerDb, location4.Latitude, location4.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex4);

            // verify 1->2
            var edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            var edge = edges.FirstOrDefault();
            Assert.IsNotNull(edge);
            var data = edge.Data;
            var profile = routerDb.EdgeProfiles.Get(data.Profile);
            var meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(3, edges.Count());

            // verify 2->1
            edge = edges.FirstOrDefault(x => x.To == vertex1);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->3
            edge = edges.FirstOrDefault(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            edge = edges.FirstOrDefault();
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->4
            edge = edges.FirstOrDefault(x => x.To == vertex4);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location4, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            edges = routerDb.Network.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());

            // verify 3->2
            edge = edges.FirstOrDefault(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            edges = routerDb.Network.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());

            // verify 4->2
            edge = edges.FirstOrDefault(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location4, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);
        }

        /// <summary>
        /// Tests loading two ways with the same end-nodes.
        /// </summary>
        [Test]
        public void TestTwoWaysSameEndNodes()
        {
            // build source stream
            var location1 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var location3 = new GeoCoordinateSimple() { Latitude = 51.269567822699510f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 3),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3)
                }.ToOsmStreamSource();

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex1);
            var vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex2);
            var vertex3 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex3);

            // verify 1->2
            var edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            var edge = edges.First(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            var data = edge.Data;
            var profile = routerDb.EdgeProfiles.Get(data.Profile);
            var meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 1->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            edge = edges.First(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location3), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->1
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex1);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // build source stream
            source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 3)
                }.ToOsmStreamSource();

            // build db from stream.
            routerDb = new RouterDb();
            target = new RouterDbStreamTarget(
                routerDb, new Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex1);
            vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex2);
            vertex3 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex3);

            // verify 1->2
            edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            edge = edges.First(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 1->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            edge = edges.First(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location3), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->1
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex1);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);
        }

        /// <summary>
        /// Finds a vertex in the given router db.
        /// </summary>
        /// <returns></returns>
        private uint FindVertex(RouterDb db, float latitude, float longitude)
        {
            var e = 0.00001f;
            for(uint vertex = 0; vertex < db.Network.VertexCount; vertex++)
            {
                var location = db.Network.GetVertex(vertex);
                if(System.Math.Abs(location.Latitude - latitude) < e &&
                   System.Math.Abs(location.Longitude - longitude) < e)
                {
                    return vertex;
                }
            }
            return uint.MaxValue;
        }
    }
}
