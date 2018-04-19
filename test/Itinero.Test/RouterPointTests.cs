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
using Itinero.Data.Network;
using Itinero.Data.Network.Edges;
using Itinero.Profiles;
using Itinero.Test.Profiles;
using System.Linq;
using Itinero.Algorithms.Weights;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for router points.
    /// </summary>
    [TestFixture]
    public class RouterPointTests
    {
        /// <summary>
        /// Tests creating new router points.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            var point = new RouterPoint(0, 1, 2, 3);

            Assert.AreEqual(0, point.Latitude);
            Assert.AreEqual(1, point.Longitude);
            Assert.AreEqual(2, point.EdgeId);
            Assert.AreEqual(3, point.Offset);

            point = new RouterPoint(0, 1, 2, 3, new Attribute("key", "value"));

            Assert.AreEqual(0, point.Latitude);
            Assert.AreEqual(1, point.Longitude);
            Assert.AreEqual(2, point.EdgeId);
            Assert.AreEqual(3, point.Offset);
            Assert.AreEqual(1, point.Attributes.Count);
            Assert.IsTrue(point.Attributes.Contains("key", "value"));
        }

        /// <summary>
        /// Tests creating paths out of a router point.
        /// </summary>
        [Test]
        public void TestToEdgePaths()
        {
            var distance = Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.1f, 0.1f));

            // build router db.
            var routerDb = new RouterDb(Itinero.Data.Edges.EdgeDataSerializer.MAX_DISTANCE);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // mock profile.
            var profile = VehicleMock.Car().Fastest();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));
            var paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);

            var factor = profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential")));
            var weight0 = Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.04f, 0.04f)) * factor.Value;
            var weight1 = Coordinate.DistanceEstimateInMeter(new Coordinate(.1f, .1f),
                new Coordinate(0.04f, 0.04f)) * factor.Value;
            Assert.IsNotNull(paths);
            Assert.AreEqual(2, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(weight0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 0).From.Vertex);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(weight1, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 1).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).From.Vertex);

            point = new RouterPoint(.1f, .1f, 0, ushort.MaxValue);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(1, paths.First(x => x.Vertex == 0).From.Vertex);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);
        }

        /// <summary>
        /// Tests creating paths out of a router point.
        /// </summary>
        [Test]
        public void TestToEdgePathsOneway()
        {
            var distance = (float)Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.1f, 0.1f));

            // build router db.
            var routerDb = new RouterDb(Itinero.Data.Edges.EdgeDataSerializer.MAX_DISTANCE);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // mock profile.
            var profile = VehicleMock.Mock("OnwayMock", x =>
                {
                    return new Itinero.Profiles.FactorAndSpeed()
                    {
                        Direction = 1,
                        Value = 1 / 50f / 3.6f,
                        SpeedFactor = 1 / 50f / 3.6f
                    };
                }).Fastest();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);
            var factor = profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential")));
            var weight0 = Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.04f, 0.04f)) * factor.Value;
            var weight1 = Coordinate.DistanceEstimateInMeter(new Coordinate(.1f, .1f),
                new Coordinate(0.04f, 0.04f)) * factor.Value;
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(weight1, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 1).From.Vertex);

            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);
            factor = profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential")));
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(weight0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 0).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);
            Assert.IsNotNull(paths);
            Assert.AreEqual(2, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);
            Assert.IsNotNull(paths);
            Assert.AreEqual(2, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(1, paths.First(x => x.Vertex == 0).From.Vertex);
        }

        /// <summary>
        /// Tests creating paths out of a router point.
        /// </summary>
        [Test]
        public void TestToEdgePathsOnewayReverse()
        {
            var distance = Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.1f, 0.1f));

            // build router db.
            var routerDb = new RouterDb(Itinero.Data.Edges.EdgeDataSerializer.MAX_DISTANCE);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // mock profile.
            var profile = VehicleMock.Mock("OnwayMock", x =>
            {
                return new Itinero.Profiles.FactorAndSpeed()
                {
                    Direction = 2,
                    Value = 1 / 50f / 3.6f,
                    SpeedFactor = 1 / 50f / 3.6f
                };
            }).Fastest();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);
            var factor = profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential")));
            var weight0 = Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.04f, 0.04f)) * factor.Value;
            var weight1 = Coordinate.DistanceEstimateInMeter(new Coordinate(.1f, .1f),
                new Coordinate(0.04f, 0.04f)) * factor.Value;
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(weight1, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 1).From.Vertex);

            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);
            factor = profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential")));
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(weight0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 0).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);
            Assert.IsNotNull(paths);
            Assert.AreEqual(2, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), false);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToEdgePaths(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), true);
            Assert.IsNotNull(paths);
            Assert.AreEqual(2, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(1, paths.First(x => x.Vertex == 0).From.Vertex);
        }

        /// <summary>
        /// Tests calculating distance from router point to it's neighbours.
        /// </summary>
        [Test]
        public void TestDistanceTo()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // mock profile.
            var profile = VehicleMock.Car().Fastest();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var distance = point.DistanceTo(routerDb, 0);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.04f, 0.04f)), distance, 0.001);

            distance = point.DistanceTo(routerDb, 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(new Coordinate(.1f, .1f),
                new Coordinate(0.04f, 0.04f)), distance, 0.001);
        }

        /// <summary>
        /// Tests getting the shape points from router point to it's neighbours.
        /// </summary>
        [Test]
        public void TestShapePointsTo()
        {
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // mock profile.
            var profile = VehicleMock.Car().Fastest();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var shape = point.ShapePointsTo(routerDb, 0);
            Assert.IsNotNull(shape);
            Assert.AreEqual(1, shape.Count);
            Assert.AreEqual(0.025, shape[0].Latitude, 0.001);
            Assert.AreEqual(0.025, shape[0].Longitude, 0.001);

            shape = point.ShapePointsTo(routerDb, 1);
            Assert.IsNotNull(shape);
            Assert.AreEqual(2, shape.Count);
            Assert.AreEqual(0.050, shape[0].Latitude, 0.001);
            Assert.AreEqual(0.050, shape[0].Longitude, 0.001);
            Assert.AreEqual(0.075, shape[1].Latitude, 0.001);
            Assert.AreEqual(0.075, shape[1].Longitude, 0.001);

            var point1 = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));
            var point2 = new RouterPoint(0.08f, 0.08f, 0, (ushort)(0.8 * ushort.MaxValue));

            shape = point1.ShapePointsTo(routerDb, point2);
            Assert.IsNotNull(shape);
            Assert.AreEqual(2, shape.Count);
            Assert.AreEqual(0.050, shape[0].Latitude, 0.001);
            Assert.AreEqual(0.050, shape[0].Longitude, 0.001);
            Assert.AreEqual(0.075, shape[1].Latitude, 0.001);
            Assert.AreEqual(0.075, shape[1].Longitude, 0.001);

            shape = point2.ShapePointsTo(routerDb, point1);
            Assert.IsNotNull(shape);
            Assert.AreEqual(2, shape.Count);
            Assert.AreEqual(0.075, shape[0].Latitude, 0.001);
            Assert.AreEqual(0.075, shape[0].Longitude, 0.001);
            Assert.AreEqual(0.050, shape[1].Latitude, 0.001);
            Assert.AreEqual(0.050, shape[1].Longitude, 0.001);
        }

        /// <summary>
        /// Tests calculating path between two router points.
        /// </summary>
        [Test]
        public void TestEdgePathTo()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            var edgeId = routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // mock profile.
            var profile = VehicleMock.Car().Fastest();

            var point1 = new RouterPoint(0.01f, 0.01f, 0,
                (ushort)(0.1 * ushort.MaxValue));
            var point2 = new RouterPoint(0.09f, 0.09f, 0,
                (ushort)(0.9 * ushort.MaxValue));

            var path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential"))).Value, path.Weight, 0.001f);
            Assert.AreEqual(edgeId + 1, path.Edge);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential"))).Value, path.Weight, 0.001f);
            Assert.AreEqual(-edgeId - 1, path.Edge);

            path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            // mock profile and force oneway forward.
            profile = VehicleMock.Car(x => new FactorAndSpeed()
            {
                Value = 1 / 50f / 3.6f,
                SpeedFactor = 1 / 50f / 3.6f,
                Direction = 1
            }).Fastest();

            path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential"))).Value, path.Weight, 0.001f);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNull(path);

            path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            // mock profile and force oneway backward.
            profile = VehicleMock.Car(x => new FactorAndSpeed()
            {
                Value = 1 / 50f / 3.6f,
                SpeedFactor = 1 / 50f / 3.6f,
                Direction = 2
            }).Fastest();

            path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNull(path);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new AttributeCollection(
                    new Attribute("highway", "residential"))).Value, path.Weight, 0.001f);

            path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            // test the full edge.
            profile = VehicleMock.Car().Fastest();
            point1 = new RouterPoint(0f, 0f, 0, 0);
            point2 = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);

            path = point1.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(1000 * profile.Factor(null).Value, path.Weight, 0.001f);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = point2.EdgePathTo(routerDb, profile.DefaultWeightHandler(new Router(routerDb)), point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(1000 * profile.Factor(null).Value, path.Weight, 0.001f);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNull(path);
        }

        /// <summary>
        /// Tests getting location of the routerpoint on the network.
        /// </summary>
        [Test]
        public void TestLocationOnNetwork()
        {
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, -.1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, -0.025f),
                new Coordinate(0.050f, -0.050f),
                new Coordinate(0.075f, -0.075f));

            // mock profile.
            var profile = VehicleMock.Car().Fastest();

            var point = new RouterPoint(0.04f, -0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var location = point.LocationOnNetwork(routerDb);
            Assert.AreEqual(0.04f, location.Latitude, 0.001f);
            Assert.AreEqual(-0.04f, location.Longitude, 0.001f);

            point = new RouterPoint(0.08f, -0.08f, 0, (ushort)(0.8 * ushort.MaxValue));

            location = point.LocationOnNetwork(routerDb);
            Assert.AreEqual(0.08f, location.Latitude, 0.001f);
            Assert.AreEqual(-0.08f, location.Longitude, 0.001f);

            point = new RouterPoint(0, 0, 0, 0);

            location = point.LocationOnNetwork(routerDb);
            Assert.AreEqual(0, location.Latitude, 0.001f);
            Assert.AreEqual(0, location.Longitude, 0.001f);

            point = new RouterPoint(.1f, -.1f, 0, ushort.MaxValue);

            location = point.LocationOnNetwork(routerDb);
            Assert.AreEqual(.1f, location.Latitude, 0.001f);
            Assert.AreEqual(-.1f, location.Longitude, 0.001f);
        }

        /// <summary>
        /// Tests adding a router point as a vertex.
        /// </summary>
        [Test]
        public void TestAddAsVertex()
        {
            var distance = Coordinate.DistanceEstimateInMeter(new Coordinate(0, 0),
                new Coordinate(0.1f, 0.1f));

            // build router db.
            var routerDb = new RouterDb(Itinero.Data.Edges.EdgeDataSerializer.MAX_DISTANCE);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            var edge = routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.025f, 0.025f),
                new Coordinate(0.050f, 0.050f),
                new Coordinate(0.075f, 0.075f));

            // create router point.
            var point = new RouterPoint(0.040f, 0.04f, edge, (ushort)(ushort.MaxValue * 0.4));

            // add as vertex.
            var v = routerDb.AddAsVertex(point);
            Assert.AreEqual(1, v);
            Assert.AreEqual(3, routerDb.Network.VertexCount);
            Assert.AreEqual(2, routerDb.Network.EdgeCount);
        }

        /// <summary>
        /// Tests calculating an angle at a routerpoint's location.
        /// </summary>
        [Test]
        public void TestAngle()
        {
            var routerDb = new RouterDb(Itinero.Data.Edges.EdgeDataSerializer.MAX_DISTANCE);

            // add a vertical test edge bottom to top.
            var edge = routerDb.AddTestEdge(
                51.26737947201908f, 4.793193340301514f, 
                51.26737947201908f + 0.001f, 4.793193340301514f);
            var middle = new RouterPoint(0, 0, edge, ushort.MaxValue / 2);
            var angle = middle.Angle(routerDb.Network);
            Assert.AreEqual(0, angle, 0.1f);
            var start = new RouterPoint(0, 0, edge, 0);
            angle = start.Angle(routerDb.Network);
            Assert.AreEqual(0, angle, 0.1f);
            var end = new RouterPoint(0, 0, edge, ushort.MaxValue);
            angle = end.Angle(routerDb.Network);
            Assert.AreEqual(0, angle, 0.1f);

            // add an horizontal test edge left to right.
            edge = routerDb.AddTestEdge(
                51.26737947201908f, 4.793193340301514f, 
                51.26737947201908f, 4.793193340301514f + 0.001f);
            middle = new RouterPoint(0, 0, edge, ushort.MaxValue / 2);
            angle = middle.Angle(routerDb.Network);
            Assert.AreEqual(90, angle);
            start = new RouterPoint(0, 0, edge, 0);
            angle = start.Angle(routerDb.Network);
            Assert.AreEqual(90, angle, 0.1f);
            end = new RouterPoint(0, 0, edge, ushort.MaxValue);
            angle = end.Angle(routerDb.Network);
            Assert.AreEqual(90, angle, 0.1f);

            // add a vertical test edge top to bottom.
            edge = routerDb.AddTestEdge(
                51.26737947201908f + 0.001f, 4.793193340301514f, 
                51.26737947201908f, 4.793193340301514f);
            middle = new RouterPoint(0, 0, edge, ushort.MaxValue / 2);
            angle = middle.Angle(routerDb.Network);
            Assert.AreEqual(180, angle);
            start = new RouterPoint(0, 0, edge, 0);
            angle = start.Angle(routerDb.Network);
            Assert.AreEqual(180, angle, 0.1f);
            end = new RouterPoint(0, 0, edge, ushort.MaxValue);
            angle = end.Angle(routerDb.Network);
            Assert.AreEqual(180, angle, 0.1f);

            // add an horizontal test edge right to left.
            edge = routerDb.AddTestEdge(
                51.26737947201908f, 4.793193340301514f + 0.001f, 
                51.26737947201908f, 4.793193340301514f);
            middle = new RouterPoint(0, 0, edge, ushort.MaxValue / 2);
            angle = middle.Angle(routerDb.Network);
            Assert.AreEqual(270, angle);
            start = new RouterPoint(0, 0, edge, 0);
            angle = start.Angle(routerDb.Network);
            Assert.AreEqual(270, angle, 0.1f);
            end = new RouterPoint(0, 0, edge, ushort.MaxValue);
            angle = end.Angle(routerDb.Network);
            Assert.AreEqual(270, angle, 0.1f);
        }
    }
}