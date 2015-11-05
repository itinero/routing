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
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Network.Data;
using OsmSharp.Routing.Profiles;
using OsmSharp.Routing.Test.Profiles;
using System.Linq;

namespace OsmSharp.Routing.Test
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

            point = new RouterPoint(0, 1, 2, 3, new Tag("key", "value"));

            Assert.AreEqual(0, point.Latitude);
            Assert.AreEqual(1, point.Longitude);
            Assert.AreEqual(2, point.EdgeId);
            Assert.AreEqual(3, point.Offset);
            Assert.AreEqual(1, point.Tags.Count);
            Assert.IsTrue(point.Tags.ContainsKeyValue("key", "value"));
        }

        /// <summary>
        /// Tests creating paths out of a router point.
        /// </summary>
        [Test]
        public void TestToPaths()
        {
            var distance = (float)GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.1, 0.1));

            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new GeoCoordinate(0.025, 0.025),
                new GeoCoordinate(0.050, 0.050),
                new GeoCoordinate(0.075, 0.075));

            // mock profile.
            var profile = MockProfile.CarMock();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));
            var paths = point.ToPaths(routerDb, profile, false);

            var factor = profile.Factor(new TagsCollection(
                    new Tag("highway", "residential")));
            var weight0 = GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.04, 0.04)) * factor.Value;
            var weight1 = GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(.1, .1),
                new GeoCoordinate(0.04, 0.04)) * factor.Value;
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
            paths = point.ToPaths(routerDb, profile, true);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(distance * profile.Factor(null).Value, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).From.Vertex);

            point = new RouterPoint(.1f, .1f, 0, ushort.MaxValue);
            paths = point.ToPaths(routerDb, profile, true);

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
        public void TestToPathsOneway()
        {
            var distance = (float)GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.1, 0.1));

            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new GeoCoordinate(0.025, 0.025),
                new GeoCoordinate(0.050, 0.050),
                new GeoCoordinate(0.075, 0.075));

            // mock profile.
            var profile = MockProfile.Mock("OnwayMock", x =>
                {
                    return new Routing.Profiles.Speed()
                    {
                        Direction = 1,
                        Value = 50f / 3.6f
                    };
                });

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var paths = point.ToPaths(routerDb, profile, true);
            var factor = profile.Factor(new TagsCollection(
                    new Tag("highway", "residential")));
            var weight0 = GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.04, 0.04)) * factor.Value;
            var weight1 = GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(.1, .1),
                new GeoCoordinate(0.04, 0.04)) * factor.Value;
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(weight1, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 1).From.Vertex);

            paths = point.ToPaths(routerDb, profile, false);
            factor = profile.Factor(new TagsCollection(
                    new Tag("highway", "residential")));
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(weight0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 0).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToPaths(routerDb, profile, true);
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
            paths = point.ToPaths(routerDb, profile, false);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToPaths(routerDb, profile, true);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToPaths(routerDb, profile, false);
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
        public void TestToPathsOnewayReverse()
        {
            var distance = (float)GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.1, 0.1));

            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = distance,
                MetaId = routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new GeoCoordinate(0.025, 0.025),
                new GeoCoordinate(0.050, 0.050),
                new GeoCoordinate(0.075, 0.075));

            // mock profile.
            var profile = MockProfile.Mock("OnwayMock", x =>
            {
                return new Routing.Profiles.Speed()
                {
                    Direction = 2,
                    Value = 50f / 3.6f
                };
            });

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var paths = point.ToPaths(routerDb, profile, false);
            var factor = profile.Factor(new TagsCollection(
                    new Tag("highway", "residential")));
            var weight0 = GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.04, 0.04)) * factor.Value;
            var weight1 = GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(.1, .1),
                new GeoCoordinate(0.04, 0.04)) * factor.Value;
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(weight1, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 1).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 1).From.Vertex);

            paths = point.ToPaths(routerDb, profile, true);
            factor = profile.Factor(new TagsCollection(
                    new Tag("highway", "residential")));
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(weight0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNotNull(paths.First(x => x.Vertex == 0).From);
            Assert.AreEqual(Constants.NO_VERTEX, paths.First(x => x.Vertex == 0).From.Vertex);

            point = new RouterPoint(0, 0, 0, 0);
            paths = point.ToPaths(routerDb, profile, false);
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
            paths = point.ToPaths(routerDb, profile, true);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 0));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 0).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 0).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToPaths(routerDb, profile, false);
            Assert.IsNotNull(paths);
            Assert.AreEqual(1, paths.Length);

            Assert.IsNotNull(paths.First(x => x.Vertex == 1));
            Assert.AreEqual(0, paths.First(x => x.Vertex == 1).Weight, 0.01);
            Assert.IsNull(paths.First(x => x.Vertex == 1).From);

            point = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);
            paths = point.ToPaths(routerDb, profile, true);
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
                MetaId = routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new GeoCoordinate(0.025, 0.025),
                new GeoCoordinate(0.050, 0.050),
                new GeoCoordinate(0.075, 0.075));

            // mock profile.
            var profile = MockProfile.CarMock();

            var point = new RouterPoint(0.04f, 0.04f, 0, (ushort)(0.4 * ushort.MaxValue));

            var distance = point.DistanceTo(routerDb, 0);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(0, 0),
                new GeoCoordinate(0.04, 0.04)), distance, 0.001);

            distance = point.DistanceTo(routerDb, 1);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(.1, .1),
                new GeoCoordinate(0.04, 0.04)), distance, 0.001);
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
                MetaId = routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new GeoCoordinate(0.025, 0.025),
                new GeoCoordinate(0.050, 0.050),
                new GeoCoordinate(0.075, 0.075));

            // mock profile.
            var profile = MockProfile.CarMock();

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
        public void TestPathTo()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, .1f, .1f);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new GeoCoordinate(0.025, 0.025),
                new GeoCoordinate(0.050, 0.050),
                new GeoCoordinate(0.075, 0.075));

            // mock profile.
            var profile = MockProfile.CarMock();

            var point1 = new RouterPoint(0.01f, 0.01f, 0,
                (ushort)(0.1 * ushort.MaxValue));
            var point2 = new RouterPoint(0.09f, 0.09f, 0,
                (ushort)(0.9 * ushort.MaxValue));

            var path = point1.PathTo(routerDb, profile, point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new TagsCollection(
                    new Tag("highway", "residential"))).Value, path.Weight, 0.001f);

            path = point2.PathTo(routerDb, profile, point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new TagsCollection(
                    new Tag("highway", "residential"))).Value, path.Weight, 0.001f);

            path = point1.PathTo(routerDb, profile, point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            path = point2.PathTo(routerDb, profile, point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            // mock profile and force oneway forward.
            profile = MockProfile.CarMock(x => new Speed()
            {
                Value = 50f / 3.6f,
                Direction = 1
            });

            path = point1.PathTo(routerDb, profile, point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new TagsCollection(
                    new Tag("highway", "residential"))).Value, path.Weight, 0.001f);

            path = point2.PathTo(routerDb, profile, point1);
            Assert.IsNull(path);

            path = point1.PathTo(routerDb, profile, point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            path = point2.PathTo(routerDb, profile, point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            // mock profile and force oneway backward.
            profile = MockProfile.CarMock(x => new Speed()
            {
                Value = 50f / 3.6f,
                Direction = 2
            });

            path = point1.PathTo(routerDb, profile, point2);
            Assert.IsNull(path);

            path = point2.PathTo(routerDb, profile, point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(800 * profile.Factor(new TagsCollection(
                    new Tag("highway", "residential"))).Value, path.Weight, 0.001f);

            path = point1.PathTo(routerDb, profile, point1);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            path = point2.PathTo(routerDb, profile, point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);

            // test the full edge.
            profile = MockProfile.CarMock();
            point1 = new RouterPoint(0f, 0f, 0, 0);
            point2 = new RouterPoint(0.1f, 0.1f, 0, ushort.MaxValue);

            path = point1.PathTo(routerDb, profile, point2);
            Assert.IsNotNull(path);
            Assert.AreEqual(1000 * profile.Factor(null).Value, path.Weight, 0.001f);
            Assert.AreEqual(1, path.Vertex);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Weight, 0.001f);
            Assert.AreEqual(0, path.Vertex);
            path = path.From;
            Assert.IsNull(path);

            path = point2.PathTo(routerDb, profile, point1);
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
    }
}