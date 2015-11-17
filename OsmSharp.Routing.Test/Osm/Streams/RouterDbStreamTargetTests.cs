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
        /// Tests loading a way with one part of the way a closed path.
        /// </summary>
        [Test]
        public void TestOneWayWithClosedPath()
        {
            // build source stream with one way:
            //         
            //         (1)
            //          |
            //          |
            //    (3)--(2)--(5)
            //      \       /   
            //       \     /
            //        \   /
            //         (4)
            //        
            // this should result in edges:
            //   1-2, 2-3, 2-5, 3-(4)-5

            var location1 = new GeoCoordinateSimple() { Latitude = 51.26118473347939f, Longitude = 4.796192049980164f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.26137943317470f, Longitude = 4.796060621738434f };
            var location3 = new GeoCoordinateSimple() { Latitude = 51.26142810796969f, Longitude = 4.796184003353119f };
            var location4 = new GeoCoordinateSimple() { Latitude = 51.26152881427841f, Longitude = 4.795939922332763f };
            var location5 = new GeoCoordinateSimple() { Latitude = 51.26134754276387f, Longitude = 4.795937240123749f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Node.Create(4, location4.Latitude, location4.Longitude),
                Node.Create(5, location5.Latitude, location5.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3, 4, 5, 2)
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
            Assert.AreEqual(4, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            var vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);
            var vertex3 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);
            var vertex5 = this.FindVertex(routerDb, location5.Latitude, location5.Longitude);

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

            // verify 2->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location2, location3), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 2->5
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex5);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location2, location5), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // verify 3->5
            edges = routerDb.Network.GetEdgeEnumerator(vertex3);
            edge = edges.First(x => x.To == vertex5);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location3, location4) + GeoCoordinate.DistanceEstimateInMeter(location4, location5), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);

            // build source stream with one way:
            //         
            //         (1)
            //          |
            //          |
            //         (2)
            //          |
            //          |
            //         (3)
            // the the way contains the following sequence of nodes:
            //  1, 2, 3, 2
            //        
            // this should result in edges:
            //   1-2, 2-3

            location1 = new GeoCoordinateSimple() { Latitude = 51.26118473347939f, Longitude = 4.796192049980164f };
            location2 = new GeoCoordinateSimple() { Latitude = 51.26137943317470f, Longitude = 4.796060621738434f };
            location3 = new GeoCoordinateSimple() { Latitude = 51.26142810796969f, Longitude = 4.796184003353119f };
            source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3, 2)
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
            Assert.AreEqual(2, routerDb.Network.EdgeCount);

            vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);
            vertex3 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);

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

            // verify 2->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location2, location3), data.Distance, 0.1);
            Assert.AreEqual(new TagsCollection(new Tag("highway", "residential")), profile);
            Assert.AreEqual(new TagsCollection(), meta);
        }

        /// <summary>
        /// Tests adding a very long edge.
        /// </summary>
        [Test]
        public void TestLongEdge()
        {
            // build source stream with one way:
            //         
            //         (1)
            //          |
            //          |
            //         (2)
            //        

            var location1 = new GeoCoordinateSimple() { Latitude = 51.32717923968566f, Longitude = 4.5867919921875f };
            var location2 = new GeoCoordinateSimple() { Latitude = 51.19282276127831f, Longitude = 4.5867919921875f };
            var location3 = new GeoCoordinateSimple() { Latitude = 51.05807338112719f, Longitude = 4.5867919921875f };
            var source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
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

            Assert.AreEqual(2, routerDb.Network.EdgeCount);
            var edge1 = routerDb.Network.GetEdge(0);
            var edge2 = routerDb.Network.GetEdge(1);
            Assert.IsNull(edge1.Shape);
            Assert.IsNull(edge2.Shape);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location3),
                edge1.Data.Distance + edge2.Data.Distance, 0.2);
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(location1.Latitude, routerDb.Network.GetVertex(0).Latitude);
            Assert.AreEqual(location1.Longitude, routerDb.Network.GetVertex(0).Longitude);
            Assert.AreEqual(location2.Latitude, routerDb.Network.GetVertex(2).Latitude);
            Assert.AreEqual(location2.Longitude, routerDb.Network.GetVertex(2).Longitude);
            Assert.AreEqual(location3.Latitude, routerDb.Network.GetVertex(1).Latitude);
            Assert.AreEqual(location3.Longitude, routerDb.Network.GetVertex(1).Longitude);

            source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
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

            Assert.AreEqual(2, routerDb.Network.EdgeCount);
            edge1 = routerDb.Network.GetEdge(0);
            edge2 = routerDb.Network.GetEdge(1);
            Assert.IsNull(edge1.Shape);
            Assert.IsNull(edge2.Shape);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location3),
                edge1.Data.Distance + edge2.Data.Distance, 0.2);
            var middle = new GeoCoordinateSimple()
            {
                Latitude = (float)(((double)location1.Latitude +
                    (double)location3.Latitude) / 2.0),
                Longitude = (float)(((double)location1.Longitude +
                    (double)location3.Longitude) / 2.0),
            };
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(location1.Latitude, routerDb.Network.GetVertex(0).Latitude);
            Assert.AreEqual(location1.Longitude, routerDb.Network.GetVertex(0).Longitude);
            Assert.AreEqual(location3.Latitude, routerDb.Network.GetVertex(1).Latitude);
            Assert.AreEqual(location3.Longitude, routerDb.Network.GetVertex(1).Longitude);
            Assert.AreEqual(middle.Latitude, routerDb.Network.GetVertex(2).Latitude);
            Assert.AreEqual(middle.Longitude, routerDb.Network.GetVertex(2).Longitude);

            location1 = new GeoCoordinateSimple() { Latitude = 51.32717923968566f, Longitude = 4.5867919921875f };
            location2 = new GeoCoordinateSimple() { Latitude = 51.26005008781385f, Longitude = 4.5867919921875f };
            location3 = new GeoCoordinateSimple() { Latitude = 51.19282276127831f, Longitude = 4.5867919921875f };
            var location4 = new GeoCoordinateSimple() { Latitude = 51.12549720918989f, Longitude = 4.5867919921875f };
            var location5 = new GeoCoordinateSimple() { Latitude = 51.05807338112719f, Longitude = 4.5867919921875f };

            source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Node.Create(4, location4.Latitude, location4.Longitude),
                Node.Create(5, location5.Latitude, location5.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3, 4, 5)
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

            Assert.AreEqual(2, routerDb.Network.EdgeCount);
            edge1 = routerDb.Network.GetEdge(0);
            edge2 = routerDb.Network.GetEdge(1);
            Assert.AreEqual(2, edge1.Shape.Count);
            Assert.AreEqual(edge1.Shape[0].Latitude, location2.Latitude);
            Assert.AreEqual(edge1.Shape[0].Longitude, location2.Longitude);
            Assert.AreEqual(edge1.Shape[1].Latitude, location3.Latitude);
            Assert.AreEqual(edge1.Shape[1].Longitude, location3.Longitude);
            Assert.IsTrue(edge2.Shape == null || edge2.Shape.Count == 0);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location5),
                edge1.Data.Distance + edge2.Data.Distance, 0.2);
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(location1.Latitude, routerDb.Network.GetVertex(0).Latitude);
            Assert.AreEqual(location1.Longitude, routerDb.Network.GetVertex(0).Longitude);
            Assert.AreEqual(location5.Latitude, routerDb.Network.GetVertex(1).Latitude);
            Assert.AreEqual(location5.Longitude, routerDb.Network.GetVertex(1).Longitude);
            Assert.AreEqual(location4.Latitude, routerDb.Network.GetVertex(2).Latitude);
            Assert.AreEqual(location4.Longitude, routerDb.Network.GetVertex(2).Longitude);

            var location6 = new GeoCoordinateSimple() { Latitude = 50.98004704630210f, Longitude = 4.5867919921875f };
            var location7 = new GeoCoordinateSimple() { Latitude = 50.77902363244571f, Longitude = 4.5867919921875f };

            source = new OsmGeo[] {
                Node.Create(1, location1.Latitude, location1.Longitude),
                Node.Create(2, location2.Latitude, location2.Longitude),
                Node.Create(3, location3.Latitude, location3.Longitude),
                Node.Create(4, location4.Latitude, location4.Longitude),
                Node.Create(5, location5.Latitude, location5.Longitude),
                Node.Create(6, location6.Latitude, location6.Longitude),
                Node.Create(7, location7.Latitude, location7.Longitude),
                Way.Create(1, new TagsCollection(
                    Tag.Create("highway", "residential")), 1, 2, 3, 4, 5, 6, 7)
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

            Assert.AreEqual(3, routerDb.Network.EdgeCount);
            edge1 = routerDb.Network.GetEdge(0);
            edge2 = routerDb.Network.GetEdge(1);
            var edge3 = routerDb.Network.GetEdge(2);
            Assert.AreEqual(2, edge1.Shape.Count);
            Assert.AreEqual(edge1.Shape[0].Latitude, location2.Latitude);
            Assert.AreEqual(edge1.Shape[0].Longitude, location2.Longitude);
            Assert.AreEqual(edge1.Shape[1].Latitude, location3.Latitude);
            Assert.AreEqual(edge1.Shape[1].Longitude, location3.Longitude);
            Assert.AreEqual(1, edge2.Shape.Count);
            Assert.AreEqual(edge2.Shape[0].Latitude, location5.Latitude);
            Assert.AreEqual(edge2.Shape[0].Longitude, location5.Longitude);
            Assert.IsTrue(edge3.Shape == null || edge3.Shape.Count == 0);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(location1, location7),
                edge1.Data.Distance + edge2.Data.Distance + edge3.Data.Distance, 0.2);
            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(location1.Latitude, routerDb.Network.GetVertex(0).Latitude);
            Assert.AreEqual(location1.Longitude, routerDb.Network.GetVertex(0).Longitude);
            Assert.AreEqual(location7.Latitude, routerDb.Network.GetVertex(1).Latitude);
            Assert.AreEqual(location7.Longitude, routerDb.Network.GetVertex(1).Longitude);
            Assert.AreEqual(location4.Latitude, routerDb.Network.GetVertex(2).Latitude);
            Assert.AreEqual(location4.Longitude, routerDb.Network.GetVertex(2).Longitude);
            Assert.AreEqual(location6.Latitude, routerDb.Network.GetVertex(3).Latitude);
            Assert.AreEqual(location6.Longitude, routerDb.Network.GetVertex(3).Longitude);
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
