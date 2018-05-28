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
using OsmSharp.Streams;
using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.IO.Osm.Streams;
using Itinero.Osm.Vehicles;
using OsmSharp.Tags;
using System.Linq;
using OsmSharp;
using Itinero.Data.Network.Restrictions;
using Itinero.IO.Osm;
using Itinero.Data;
using Itinero.Data.Network;
using Itinero.Algorithms.Networks;

namespace Itinero.Test.IO.Osm.Streams
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
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
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

            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);
        } 
        
        /// <summary>
        /// Tests loading one way.
        /// </summary>
        [Test]
        public void TestOneWayWithAShapePoint()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 49.8355328612026f, Longitude = 5.754808187484741f };
            var location2 = new Coordinate() { Latitude = 49.83543598213665f, Longitude = 5.755451917648315f };
            var location3 = new Coordinate() { Latitude = 49.83556054090007f, Longitude = 5.756278038024902f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex1);
            var vertex2 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);
            Assert.AreNotEqual(uint.MaxValue, vertex2);

            // get edge-information.
            var edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            var data = edges.First().Data;
            var profile = routerDb.EdgeProfiles.Get(data.Profile);
            var meta = routerDb.EdgeMeta.Get(data.MetaId);
            var shape = edges.First().Shape;

            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2) +
                Coordinate.DistanceEstimateInMeter(location2, location3), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);
            Assert.IsNotNull(shape);
            Assert.AreEqual(1, shape.Count);
            Assert.AreEqual(location2.Latitude, shape[0].Latitude, 0.00001f);
            Assert.AreEqual(location2.Longitude, shape[0].Longitude, 0.00001f);
        }

        /// <summary>
        /// Tests loading one way that's oneway.
        /// </summary>
        [Test]
        public void TestOneWayOneway()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential"),
                        new Tag("oneway", "yes")),
                    Nodes = new long[] { 1, 2 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
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
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.IsTrue(profile.Contains("oneway", "yes"));
            Assert.AreEqual(new AttributeCollection(), meta);
        }

        /// <summary>
        /// Tests loading one incomplete way.
        /// </summary>
        [Test]
        public void TestOneWayIncomplete()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
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
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 3, 2, 1 }
                }};

            // build db from stream.
            routerDb = new RouterDb();
            target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
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
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var location3 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7978925704956050f };
            var location4 = new Coordinate() { Latitude = 51.269567822699510f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3 }
                },
                new Way()
                {
                    Id = 2,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 4 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
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
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            Assert.AreEqual(3, edges.Count());

            // verify 2->1
            edge = edges.FirstOrDefault(x => x.To == vertex1);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            // verify 2->3
            edge = edges.FirstOrDefault(x => x.To == vertex3);
            Assert.IsNotNull(edge);
            edge = edges.FirstOrDefault();
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            // verify 2->4
            edge = edges.FirstOrDefault(x => x.To == vertex4);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location4, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            edges = routerDb.Network.GetEdgeEnumerator(vertex3);
            Assert.AreEqual(1, edges.Count());

            // verify 3->2
            edge = edges.FirstOrDefault(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            edges = routerDb.Network.GetEdgeEnumerator(vertex4);
            Assert.AreEqual(1, edges.Count());

            // verify 4->2
            edge = edges.FirstOrDefault(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location4, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);
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
            //   1-2, 2-2 (along 3-4-5)
            
            var location1 = new Coordinate() { Latitude = 51.26118473347939f, Longitude = 4.796192049980164f };
            var location2 = new Coordinate() { Latitude = 51.26137943317470f, Longitude = 4.796060621738434f };
            var location3 = new Coordinate() { Latitude = 51.26142810796969f, Longitude = 4.796184003353119f };
            var location4 = new Coordinate() { Latitude = 51.26152881427841f, Longitude = 4.795939922332763f };
            var location5 = new Coordinate() { Latitude = 51.26134754276387f, Longitude = 4.795937240123749f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Node()
                {
                    Id = 5,
                    Latitude = location5.Latitude,
                    Longitude = location5.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3, 4, 5, 2 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            routerDb.Network.GeometricGraph.Graph.MarkAsMulti();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(2, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            var vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);

            // verify 1->2
            var edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            var edge = edges.First(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            var data = edge.Data;
            var profile = routerDb.EdgeProfiles.Get(data.Profile);
            var meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            // verify 2->2
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location2, location3) +
                Coordinate.DistanceEstimateInMeter(location3, location4) +
                Coordinate.DistanceEstimateInMeter(location4, location5) +
                Coordinate.DistanceEstimateInMeter(location5, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

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
            //   1-2, 2-2 (along 3)

            location1 = new Coordinate() { Latitude = 51.26118473347939f, Longitude = 4.796192049980164f };
            location2 = new Coordinate() { Latitude = 51.26137943317470f, Longitude = 4.796060621738434f };
            location3 = new Coordinate() { Latitude = 51.26142810796969f, Longitude = 4.796184003353119f };
            source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3, 2 }
                }};

            // build db from stream.
            routerDb = new RouterDb();
            routerDb.Network.GeometricGraph.Graph.MarkAsMulti();
            target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                });
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(2, routerDb.Network.EdgeCount);

            vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);

            // verify 1->2
            edges = routerDb.Network.GetEdgeEnumerator(vertex1);
            edge = edges.First(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location1, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);

            // verify 2->3
            edges = routerDb.Network.GetEdgeEnumerator(vertex2);
            edge = edges.First(x => x.To == vertex2);
            Assert.IsNotNull(edge);
            data = edge.Data;
            profile = routerDb.EdgeProfiles.Get(data.Profile);
            meta = routerDb.EdgeMeta.Get(data.MetaId);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(location2, location3) +
                Coordinate.DistanceEstimateInMeter(location3, location2), data.Distance, 0.1);
            Assert.IsTrue(profile.Contains("highway", "residential"));
            Assert.AreEqual(new AttributeCollection(), meta);
        }

        /// <summary>
        /// Tests processing a perfect restriction.
        /// </summary>
        [Test]
        public void TestPerfectRestriction()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var location3 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7978925704956050f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2 }
                },
                new Way()
                {
                    Id = 2,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 3 }
                },
                new Relation()
                {
                    Id = 1,
                    Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "via",
                            Type = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "from",
                            Type = OsmGeoType.Way
                        },
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "to",
                            Type = OsmGeoType.Way
                        }
                    },
                    Tags = new TagsCollection(
                        new Tag("type", "restriction"),
                        new Tag("restriction", "no_left_turn"))
                }};
            
            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                }, processRestrictions: true);
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(2, routerDb.Network.EdgeCount);

            // check for a restriction.
            RestrictionsDb restrictions;
            Assert.IsTrue(routerDb.TryGetRestrictions("motor_vehicle", out restrictions));
        }

        /// <summary>
        /// Tests processing a positive restriction.
        ///        (4)
        ///         |
        /// (1)----(2)----(3)
        ///         |
        ///        (5)
        ///        
        /// With restriction: only_straight_on (1)->(2)->(3)
        /// </summary>
        [Test]
        public void TestPositiveRestriction()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var location3 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7978925704956050f };
            var location4 = new Coordinate() { Latitude = 51.265106473294075f, Longitude = 4.7907257080078125f };
            var location5 = new Coordinate() { Latitude = 51.264916473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Node()
                {
                    Id = 5,
                    Latitude = location5.Latitude,
                    Longitude = location5.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2 }
                },
                new Way()
                {
                    Id = 2,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 3 }
                },
                new Way()
                {
                    Id = 3,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 4 }
                },
                new Way()
                {
                    Id = 4,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 5 }
                },
                new Relation()
                {
                    Id = 1,
                    Members = new RelationMember[]
                    {
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "via",
                            Type = OsmGeoType.Node
                        },
                        new RelationMember()
                        {
                            Id = 1,
                            Role = "from",
                            Type = OsmGeoType.Way
                        },
                        new RelationMember()
                        {
                            Id = 2,
                            Role = "to",
                            Type = OsmGeoType.Way
                        }
                    },
                    Tags = new TagsCollection(
                        new Tag("type", "restriction"),
                        new Tag("restriction", "only_straight_on"))
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                }, processRestrictions: true);
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(5, routerDb.Network.VertexCount);
            Assert.AreEqual(4, routerDb.Network.EdgeCount);

            // check for a restriction.
            RestrictionsDb restrictions;
            Assert.IsTrue(routerDb.TryGetRestrictions("motor_vehicle", out restrictions));
            Assert.AreEqual(2, restrictions.Count);
        }

        /// <summary>
        /// Tests processing vertex meta related to cycle networks.
        /// </summary>
        [Test]
        public void TestVertexMeta()
        {
            // build source stream.
            var location1 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7835588455200195f };
            var location2 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7907257080078125f };
            var location3 = new Coordinate() { Latitude = 51.265016473294075f, Longitude = 4.7978925704956050f };
            var location4 = new Coordinate() { Latitude = 51.265106473294075f, Longitude = 4.7907257080078125f };
            var location5 = new Coordinate() { Latitude = 51.264916473294075f, Longitude = 4.7907257080078125f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude,
                    Tags = new TagsCollection(
                        new Tag("rcn_ref", "12")),
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Node()
                {
                    Id = 5,
                    Latitude = location5.Latitude,
                    Longitude = location5.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2 }
                },
                new Way()
                {
                    Id = 2,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 3 }
                },
                new Way()
                {
                    Id = 3,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 4 }
                },
                new Way()
                {
                    Id = 4,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 2, 5 }
                }
            };

            var routerDb = new RouterDb();
            routerDb.LoadOsmData(source, Itinero.Osm.Vehicles.Vehicle.Bicycle);

            var vertexNode3 = Itinero.Algorithms.Search.Hilbert.HilbertExtensions.SearchClosest(routerDb.Network.GeometricGraph,
                location3.Latitude, location3.Longitude, 0.01f, 0.01f);
            var meta = routerDb.VertexMeta[vertexNode3];
            Assert.AreEqual(meta, new AttributeCollection(
                new Attribute("rcn_ref", "12")));
        }

        /// <summary>
        /// Tests loading data the keeps the node id's.
        /// </summary>
        [Test]
        public void TestKeepingNodeIds()
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
            //   1-2, 2-2 (along 3-4-5)

            var location1 = new Coordinate() { Latitude = 51.26118473347939f, Longitude = 4.796192049980164f };
            var location2 = new Coordinate() { Latitude = 51.26137943317470f, Longitude = 4.796060621738434f };
            var location3 = new Coordinate() { Latitude = 51.26142810796969f, Longitude = 4.796184003353119f };
            var location4 = new Coordinate() { Latitude = 51.26152881427841f, Longitude = 4.795939922332763f };
            var location5 = new Coordinate() { Latitude = 51.26134754276387f, Longitude = 4.795937240123749f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Node()
                {
                    Id = 5,
                    Latitude = location5.Latitude,
                    Longitude = location5.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3, 4, 5, 2 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            routerDb.Network.GeometricGraph.Graph.MarkAsMulti();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                });
            target.KeepNodeIds = true;
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(2, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            var vertex2 = this.FindVertex(routerDb, location2.Latitude, location2.Longitude);
            
            var nodeIds = routerDb.VertexData.Get<long>("node_id");
            Assert.AreEqual(1, nodeIds[vertex1]);
            Assert.AreEqual(2, nodeIds[vertex2]);
        }

        /// <summary>
        /// Tests loading data the keeps the way id's.
        /// </summary>
        [Test]
        public void TestKeepingWayIds()
        {
            // build source stream with two ways:
            //         
            //         (1)
            //          |
            //         (2)
            //          |
            //    (4)--(3)--(5)
            //        
            // this should result in edges:
            //   1-3, 4-3, 3-5

            var location1 = new Coordinate() { Latitude = 51.26118473347939f, Longitude = 4.796192049980164f };
            var location2 = new Coordinate() { Latitude = 51.26137943317470f, Longitude = 4.796060621738434f };
            var location3 = new Coordinate() { Latitude = 51.26142810796969f, Longitude = 4.796184003353119f };
            var location4 = new Coordinate() { Latitude = 51.26152881427841f, Longitude = 4.795939922332763f };
            var location5 = new Coordinate() { Latitude = 51.26134754276387f, Longitude = 4.795937240123749f };
            var source = new OsmGeo[] {
                new Node()
                {
                    Id = 1,
                    Latitude = location1.Latitude,
                    Longitude = location1.Longitude
                },
                new Node()
                {
                    Id = 2,
                    Latitude = location2.Latitude,
                    Longitude = location2.Longitude
                },
                new Node()
                {
                    Id = 3,
                    Latitude = location3.Latitude,
                    Longitude = location3.Longitude
                },
                new Node()
                {
                    Id = 4,
                    Latitude = location4.Latitude,
                    Longitude = location4.Longitude
                },
                new Node()
                {
                    Id = 5,
                    Latitude = location5.Latitude,
                    Longitude = location5.Longitude
                },
                new Way()
                {
                    Id = 1,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 1, 2, 3 }
                },
                new Way()
                {
                    Id = 2,
                    Tags = new TagsCollection(
                        new Tag("highway", "residential")),
                    Nodes = new long[] { 4, 3, 5 }
                }};

            // build db from stream.
            var routerDb = new RouterDb();
            var target = new RouterDbStreamTarget(
                routerDb, new Itinero.Profiles.Vehicle[] {
                    Vehicle.Car
                });
            target.KeepWayIds = true;
            target.RegisterSource(source);
            target.Initialize();
            target.Pull();

            // check result.
            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex1 = this.FindVertex(routerDb, location1.Latitude, location1.Longitude);
            var vertex3 = this.FindVertex(routerDb, location3.Latitude, location3.Longitude);
            var vertex4 = this.FindVertex(routerDb, location4.Latitude, location4.Longitude);
            var vertex5 = this.FindVertex(routerDb, location5.Latitude, location5.Longitude);
            
            var wayIds = routerDb.EdgeData.Get<long>("way_id");
            Assert.IsNotNull(wayIds);
            var wayNodeIndices = routerDb.EdgeData.Get<ushort>("way_node_idx");
            Assert.IsNotNull(wayNodeIndices);

            var edge = routerDb.Network.GetEdges(vertex1).First(x => x.To == vertex3).Id;
            Assert.AreEqual(1, wayIds[edge]);
            Assert.AreEqual(0, wayNodeIndices[edge]);
            edge = routerDb.Network.GetEdges(vertex4).First(x => x.To == vertex3).Id;
            Assert.AreEqual(2, wayIds[edge]);
            Assert.AreEqual(0, wayNodeIndices[edge]);
            edge = routerDb.Network.GetEdges(vertex3).First(x => x.To == vertex5).Id;
            Assert.AreEqual(2, wayIds[edge]);
            Assert.AreEqual(1, wayNodeIndices[edge]);
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
