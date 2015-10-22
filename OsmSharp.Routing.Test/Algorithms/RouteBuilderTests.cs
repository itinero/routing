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
using OsmSharp.Routing.Algorithms;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Network.Data;
using OsmSharp.Routing.Test.Profiles;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Test.Algorithms
{
    /// <summary>
    /// Contains tests for route builder.
    /// </summary>
    [TestFixture]
    public class RouteBuilderTests
    {
        /// <summary>
        /// Tests a route with one vertex.
        /// </summary>
        [Test]
        public void TestOneVertex()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddEdge(0, 1, new EdgeData(), null);

            // build route.
            var source = new RouterPoint(0, 0, 1, 0, new Tag("type", "source"));
            var target = new RouterPoint(0, 0, 1, 0, new Tag("type", "target"));
            var profile = MockProfile.CarMock();
            var routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(1, route.Segments.Count);
            Assert.AreEqual(0, route.Segments[0].Latitude);
            Assert.AreEqual(0, route.Segments[0].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[0].Profile);
            Assert.IsNotNull(route.Segments[0].Points);
            Assert.AreEqual(2, route.Segments[0].Points.Length);
            Assert.IsNotNull(route.Segments[0].Points.FirstOrDefault(x => x.Tags.FirstOrDefault(y => y.Value == "source") != null));
            Assert.IsNotNull(route.Segments[0].Points.FirstOrDefault(x => x.Tags.FirstOrDefault(y => y.Value == "target") != null));
        }

        /// <summary>
        /// Tests a route with one edge.
        /// </summary>
        [Test]
        public void TestOneEdge()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, null);

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Tag("type", "source"));
            var target = new RouterPoint(1, 1, 0, ushort.MaxValue, new Tag("type", "target"));
            var profile = MockProfile.CarMock();
            var routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(2, route.Segments.Count);

            Assert.AreEqual(0, route.Segments[0].Latitude);
            Assert.AreEqual(0, route.Segments[0].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[0].Profile);
            Assert.IsNotNull(route.Segments[0].Points);
            Assert.AreEqual(1, route.Segments[0].Points.Length);
            Assert.AreEqual(1, route.Segments[0].Points[0].Tags.Length);
            Assert.AreEqual("type", route.Segments[0].Points[0].Tags[0].Key);
            Assert.AreEqual("source", route.Segments[0].Points[0].Tags[0].Value);

            Assert.AreEqual(1, route.Segments[1].Latitude);
            Assert.AreEqual(1, route.Segments[1].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[1].Profile);
            Assert.IsNotNull(route.Segments[1].Points);
            Assert.AreEqual(1, route.Segments[1].Points.Length);
            Assert.AreEqual(1, route.Segments[1].Points[0].Tags.Length);
            Assert.AreEqual("type", route.Segments[1].Points[0].Tags[0].Key);
            Assert.AreEqual("target", route.Segments[1].Points[0].Tags[0].Value);
            Assert.AreEqual("residential", route.Segments[1].Tags.First(y => y.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", route.Segments[1].Tags.First(y => y.Key == "name").Value);

            // build route with similar path.
            routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(2, route.Segments.Count);

            Assert.AreEqual(0, route.Segments[0].Latitude);
            Assert.AreEqual(0, route.Segments[0].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[0].Profile);
            Assert.IsNotNull(route.Segments[0].Points);
            Assert.AreEqual(1, route.Segments[0].Points.Length);
            Assert.AreEqual(1, route.Segments[0].Points[0].Tags.Length);
            Assert.AreEqual("type", route.Segments[0].Points[0].Tags[0].Key);
            Assert.AreEqual("source", route.Segments[0].Points[0].Tags[0].Value);

            Assert.AreEqual(1, route.Segments[1].Latitude);
            Assert.AreEqual(1, route.Segments[1].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[1].Profile);
            Assert.IsNotNull(route.Segments[1].Points);
            Assert.AreEqual(1, route.Segments[1].Points.Length);
            Assert.AreEqual(1, route.Segments[1].Points[0].Tags.Length);
            Assert.AreEqual("type", route.Segments[1].Points[0].Tags[0].Key);
            Assert.AreEqual("target", route.Segments[1].Points[0].Tags[0].Value);
            Assert.AreEqual("residential", route.Segments[1].Tags.First(y => y.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", route.Segments[1].Tags.First(y => y.Key == "name").Value);

            // build route with similar path.
            routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(2, route.Segments.Count);

            Assert.AreEqual(0, route.Segments[0].Latitude);
            Assert.AreEqual(0, route.Segments[0].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[0].Profile);
            Assert.IsNotNull(route.Segments[0].Points);
            Assert.AreEqual(1, route.Segments[0].Points.Length);
            Assert.AreEqual(1, route.Segments[0].Points[0].Tags.Length);
            Assert.AreEqual("type", route.Segments[0].Points[0].Tags[0].Key);
            Assert.AreEqual("source", route.Segments[0].Points[0].Tags[0].Value);

            Assert.AreEqual(1, route.Segments[1].Latitude);
            Assert.AreEqual(1, route.Segments[1].Longitude);
            Assert.AreEqual(profile.Name, route.Segments[1].Profile);
            Assert.IsNotNull(route.Segments[1].Points);
            Assert.AreEqual(1, route.Segments[1].Points.Length);
            Assert.AreEqual(1, route.Segments[1].Points[0].Tags.Length);
            Assert.AreEqual("type", route.Segments[1].Points[0].Tags[0].Key);
            Assert.AreEqual("target", route.Segments[1].Points[0].Tags[0].Value);
            Assert.AreEqual("residential", route.Segments[1].Tags.First(y => y.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", route.Segments[1].Tags.First(y => y.Key == "name").Value);
        }

        /// <summary>
        /// Tests a route with one edge and a shape.
        /// </summary>
        [Test]
        public void TestOneEdgeWithShape()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                new GeoCoordinate(0.25, 0.25),
                new GeoCoordinate(0.5, 0.5),
                new GeoCoordinate(0.75, 0.75)
            }));

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Tag("type", "source"));
            var target = new RouterPoint(1, 1, 0, ushort.MaxValue, new Tag("type", "target"));
            var profile = MockProfile.CarMock();
            var routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(5, route.Segments.Count);

            var speed = profile.Speed(new TagsCollection(
                    new Tag("highway", "residential")));
            var segment = route.Segments[0];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(0, segment.Longitude);
            Assert.AreEqual(0, segment.Time);
            Assert.AreEqual(0, segment.Distance);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Points);
            Assert.AreEqual(1, segment.Points.Length);
            Assert.AreEqual(1, segment.Points[0].Tags.Length);
            Assert.AreEqual("type", segment.Points[0].Tags[0].Key);
            Assert.AreEqual("source", segment.Points[0].Tags[0].Value);

            var previous = segment;
            segment = route.Segments[1];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(0.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[2];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(0.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[3];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(0.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[4];
            Assert.AreEqual(1, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);
        }

        /// <summary>
        /// Tests a route with two edges and shapes between.
        /// </summary>
        [Test]
        public void TestTwoEdgeWithShape()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeProfiles.Add(new TagsCollection());
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                new GeoCoordinate(0.25, 0.25),
                new GeoCoordinate(0.5, 0.5),
                new GeoCoordinate(0.75, 0.75)
            }));
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                new GeoCoordinate(0.75, 1),
                new GeoCoordinate(0.5, 1),
                new GeoCoordinate(0.25, 1)
            }));

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Tag("type", "source"));
            var target = new RouterPoint(0, 1, 1, ushort.MaxValue, new Tag("type", "target"));
            var profile = MockProfile.CarMock();
            var routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, 1, Constants.NO_VERTEX })); 
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(9, route.Segments.Count);

            var speed = profile.Speed(new TagsCollection(
                    new Tag("highway", "residential")));
            var segment = route.Segments[0];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(0, segment.Longitude);
            Assert.AreEqual(0, segment.Time);
            Assert.AreEqual(0, segment.Distance);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Points);
            Assert.AreEqual(1, segment.Points.Length);
            Assert.AreEqual(1, segment.Points[0].Tags.Length);
            Assert.AreEqual("type", segment.Points[0].Tags[0].Key);
            Assert.AreEqual("source", segment.Points[0].Tags[0].Value);

            var previous = segment;
            segment = route.Segments[1];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(0.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[2];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(0.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[3];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(0.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[4];
            Assert.AreEqual(1, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[5];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[6];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[7];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[8];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            // build route.
            routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1, 2 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(9, route.Segments.Count);

            speed = profile.Speed(new TagsCollection(
                    new Tag("highway", "residential")));
            segment = route.Segments[0];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(0, segment.Longitude);
            Assert.AreEqual(0, segment.Time);
            Assert.AreEqual(0, segment.Distance);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Points);
            Assert.AreEqual(1, segment.Points.Length);
            Assert.AreEqual(1, segment.Points[0].Tags.Length);
            Assert.AreEqual("type", segment.Points[0].Tags[0].Key);
            Assert.AreEqual("source", segment.Points[0].Tags[0].Value);

            previous = segment;
            segment = route.Segments[1];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(0.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[2];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(0.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[3];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(0.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[4];
            Assert.AreEqual(1, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[5];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[6];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[7];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[8];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);
        }

        /// <summary>
        /// Tests a route with three edges and shapes between.
        /// </summary>
        [Test]
        public void TestThreeEdgeWithShape()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeMeta.Add(new TagsCollection());
            routerDb.EdgeProfiles.Add(new TagsCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddVertex(3, 0, 2);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                new GeoCoordinate(0.25, 0.25),
                new GeoCoordinate(0.5, 0.5),
                new GeoCoordinate(0.75, 0.75)
            }));
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                new GeoCoordinate(0.75, 1),
                new GeoCoordinate(0.5, 1),
                new GeoCoordinate(0.25, 1)
            }));
            routerDb.Network.AddEdge(2, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new TagsCollection(
                    new Tag("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new TagsCollection(
                    new Tag("highway", "residential")))
            }, new CoordinateArrayCollection<ICoordinate>(new GeoCoordinate[] {
                new GeoCoordinate(0, 1.25),
                new GeoCoordinate(0, 1.5),
                new GeoCoordinate(0, 1.75)
            }));

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Tag("type", "source"));
            var target = new RouterPoint(0, 2, 2, ushort.MaxValue, new Tag("type", "target"));
            var profile = MockProfile.CarMock();
            var routeBuilder = new RouteBuilder(routerDb, profile,
                source, target,  new List<uint>(new uint[] { Constants.NO_VERTEX, 1, 2, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(13, route.Segments.Count);

            var speed = profile.Speed(new TagsCollection(
                    new Tag("highway", "residential")));
            var segment = route.Segments[0];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(0, segment.Longitude);
            Assert.AreEqual(0, segment.Time);
            Assert.AreEqual(0, segment.Distance);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Points);
            Assert.AreEqual(1, segment.Points.Length);
            Assert.AreEqual(1, segment.Points[0].Tags.Length);
            Assert.AreEqual("type", segment.Points[0].Tags[0].Key);
            Assert.AreEqual("source", segment.Points[0].Tags[0].Value);

            var previous = segment;
            segment = route.Segments[1];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(0.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[2];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(0.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[3];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(0.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[4];
            Assert.AreEqual(1, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[5];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[6];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[7];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[8];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[9];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[10];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[11];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[12];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(2, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            // build route.
            routeBuilder = new RouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1, 2, 3 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Segments);
            Assert.AreEqual(13, route.Segments.Count);

            speed = profile.Speed(new TagsCollection(
                    new Tag("highway", "residential")));
            segment = route.Segments[0];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(0, segment.Longitude);
            Assert.AreEqual(0, segment.Time);
            Assert.AreEqual(0, segment.Distance);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Points);
            Assert.AreEqual(1, segment.Points.Length);
            Assert.AreEqual(1, segment.Points[0].Tags.Length);
            Assert.AreEqual("type", segment.Points[0].Tags[0].Key);
            Assert.AreEqual("source", segment.Points[0].Tags[0].Value);

            previous = segment;
            segment = route.Segments[1];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(0.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[2];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(0.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[3];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(0.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[4];
            Assert.AreEqual(1, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[5];
            Assert.AreEqual(0.75, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[6];
            Assert.AreEqual(0.5, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[7];
            Assert.AreEqual(0.25, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[8];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[9];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1.25, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[10];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1.5, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[11];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(1.75, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);

            previous = segment;
            segment = route.Segments[12];
            Assert.AreEqual(0, segment.Latitude);
            Assert.AreEqual(2, segment.Longitude);
            Assert.AreEqual(GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance, segment.Distance, 0.001);
            Assert.AreEqual((GeoCoordinate.DistanceEstimateInMeter(new GeoCoordinate(previous.Latitude, previous.Longitude),
                new GeoCoordinate(segment.Latitude, segment.Longitude)) + previous.Distance) / speed.Value, segment.Time, 0.001);
            Assert.AreEqual(profile.Name, segment.Profile);
            Assert.IsNotNull(segment.Tags);
            Assert.AreEqual(2, segment.Tags.Length);
            Assert.AreEqual("residential", segment.Tags.First(x => x.Key == "highway").Value);
            Assert.AreEqual("Abelshausen Blvd.", segment.Tags.First(x => x.Key == "name").Value);
        }
    }
}