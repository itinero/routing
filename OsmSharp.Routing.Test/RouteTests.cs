// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using OsmSharp.Units.Time;
using System.Collections.Generic;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Does some unittesting on the OsmSharpRoute data format.
    /// </summary>
    [TestFixture]
    public class RouteTests
    {
        /// <summary>
        /// Tests the presence of tags in a calculated route.
        /// </summary>
        [Test]
        public void RouteConcatenateTagsTest()
        {
            var route1 = new Route();
            route1.Vehicle = Vehicle.Car.UniqueName;
            var route1segment1 = new RouteSegment();
            route1segment1.Distance = 10;
            route1segment1.Latitude = -1;
            route1segment1.Longitude = -1;
            route1segment1.Metrics = null;
            route1segment1.Points = new RoutePoint[1];
            route1segment1.Points[0] = new RoutePoint();
            route1segment1.Points[0].Name = "TestPoint1";
            route1segment1.Points[0].Tags = new RouteTags[1];
            route1segment1.Points[0].Tags[0] = new RouteTags();
            route1segment1.Points[0].Tags[0].Value = "TestValue1";
            route1segment1.Points[0].Tags[0].Key = "TestKey1";
            route1segment1.SideStreets = null;
            route1segment1.Tags = new RouteTags[1];
            route1segment1.Tags[0] = new RouteTags();
            route1segment1.Tags[0].Key = "highway";
            route1segment1.Tags[0].Value = "residential";
            route1segment1.Time = 10;
            route1segment1.Type = RouteSegmentType.Start;
            route1segment1.Name = string.Empty;
            route1segment1.Names = null;

            RouteSegment route1segment2 = new RouteSegment();
            route1segment2.Distance = 10;
            route1segment2.Latitude = -1;
            route1segment2.Longitude = -1;
            route1segment2.Metrics = null;
            route1segment2.Points = new RoutePoint[1];
            route1segment2.Points[0] = new RoutePoint();
            route1segment2.Points[0].Name = "TestPoint2";
            route1segment2.Points[0].Tags = new RouteTags[1];
            route1segment2.Points[0].Tags[0] = new RouteTags();
            route1segment2.Points[0].Tags[0].Value = "TestValue2";
            route1segment1.Points[0].Tags[0].Key = "TestKey2";
            route1segment2.SideStreets = null;
            route1segment2.Tags = new RouteTags[1];
            route1segment2.Tags[0] = new RouteTags();
            route1segment2.Tags[0].Key = "highway";
            route1segment2.Tags[0].Value = "residential";
            route1segment2.Time = 10;
            route1segment2.Type = RouteSegmentType.Start;
            route1segment2.Name = string.Empty;
            route1segment2.Names = null;

            route1.Segments = new RouteSegment[2];
            route1.Segments[0] = route1segment1;
            route1.Segments[1] = route1segment2;


            var route2 = new Route();
            route2.Vehicle = Vehicle.Car.UniqueName;
            var route2segment1 = new RouteSegment();
            route2segment1.Distance = 10;
            route2segment1.Latitude = -1;
            route2segment1.Longitude = -1;
            route2segment1.Metrics = null;
            route2segment1.Points = new RoutePoint[1];
            route2segment1.Points[0] = new RoutePoint();
            route2segment1.Points[0].Name = "TestPoint3";
            route2segment1.Points[0].Tags = new RouteTags[1];
            route2segment1.Points[0].Tags[0] = new RouteTags();
            route2segment1.Points[0].Tags[0].Value = "TestValue3";
            route2segment1.Points[0].Tags[0].Key = "TestKey3";
            route2segment1.SideStreets = null;
            route2segment1.Tags = new RouteTags[1];
            route2segment1.Tags[0] = new RouteTags();
            route2segment1.Tags[0].Key = "highway";
            route2segment1.Tags[0].Value = "residential";
            route2segment1.Time = 10;
            route2segment1.Type = RouteSegmentType.Start;
            route2segment1.Name = string.Empty;
            route2segment1.Names = null;

            RouteSegment route2segment2 = new RouteSegment();
            route2segment2.Distance = 10;
            route2segment2.Latitude = -1;
            route2segment2.Longitude = -1;
            route2segment2.Metrics = null;
            route2segment2.Points = new RoutePoint[1];
            route2segment2.Points[0] = new RoutePoint();
            route2segment2.Points[0].Name = "TestPoint4";
            route2segment2.Points[0].Tags = new RouteTags[1];
            route2segment2.Points[0].Tags[0] = new RouteTags();
            route2segment2.Points[0].Tags[0].Value = "TestValue4";
            route2segment1.Points[0].Tags[0].Key = "TestKey4";
            route2segment2.SideStreets = null;
            route2segment2.Tags = new RouteTags[1];
            route2segment2.Tags[0] = new RouteTags();
            route2segment2.Tags[0].Key = "highway";
            route2segment2.Tags[0].Value = "residential";
            route2segment2.Time = 10;
            route2segment2.Type = RouteSegmentType.Start;
            route2segment2.Name = string.Empty;
            route2segment2.Names = null;

            route2.Segments = new RouteSegment[2];
            route2.Segments[0] = route2segment1;
            route2.Segments[1] = route2segment2;

            Route concatenated = Route.Concatenate(route1, route2);

            // test the result.
            Assert.IsNotNull(concatenated);
            Assert.IsNotNull(concatenated.Segments);
            Assert.AreEqual(route1.Vehicle, concatenated.Vehicle);
            Assert.AreEqual(3, concatenated.Segments.Length);
            Assert.AreEqual("TestPoint1", concatenated.Segments[0].Points[0].Name);
            Assert.AreEqual("TestPoint2", concatenated.Segments[1].Points[0].Name);
            Assert.AreEqual("TestPoint3", concatenated.Segments[1].Points[1].Name);
            Assert.AreEqual("TestPoint4", concatenated.Segments[2].Points[0].Name);
        }

        /// <summary>
        /// Tests the presence of identical tags in a calculated route.
        /// </summary>
        [Test]
        public void RouteConcatenateTagsIdenticalTest()
        {
            Route route1 = new Route();
            RouteSegment route1segment1 = new RouteSegment();
            route1segment1.Distance = 10;
            route1segment1.Latitude = -1;
            route1segment1.Longitude = -1;
            route1segment1.Metrics = null;
            route1segment1.Points = new RoutePoint[1];
            route1segment1.Points[0] = new RoutePoint();
            route1segment1.Points[0].Name = "TestPoint1";
            route1segment1.Points[0].Tags = new RouteTags[1];
            route1segment1.Points[0].Tags[0] = new RouteTags();
            route1segment1.Points[0].Tags[0].Value = "TestValue1";
            route1segment1.Points[0].Tags[0].Key = "TestKey1";
            route1segment1.SideStreets = null;
            route1segment1.Tags = new RouteTags[1];
            route1segment1.Tags[0] = new RouteTags();
            route1segment1.Tags[0].Key = "highway";
            route1segment1.Tags[0].Value = "residential";
            route1segment1.Time = 10;
            route1segment1.Type = RouteSegmentType.Start;
            route1segment1.Name = string.Empty;
            route1segment1.Names = null;

            RouteSegment route1segment2 = new RouteSegment();
            route1segment2.Distance = 10;
            route1segment2.Latitude = -1;
            route1segment2.Longitude = -1;
            route1segment2.Metrics = null;
            route1segment2.Points = new RoutePoint[1];
            route1segment2.Points[0] = new RoutePoint();
            route1segment2.Points[0].Name = "TestPoint2";
            route1segment2.Points[0].Tags = new RouteTags[1];
            route1segment2.Points[0].Tags[0] = new RouteTags();
            route1segment2.Points[0].Tags[0].Value = "TestValue2";
            route1segment2.Points[0].Tags[0].Key = "TestKey2";
            route1segment2.SideStreets = null;
            route1segment2.Tags = new RouteTags[1];
            route1segment2.Tags[0] = new RouteTags();
            route1segment2.Tags[0].Key = "highway";
            route1segment2.Tags[0].Value = "residential";
            route1segment2.Time = 10;
            route1segment2.Type = RouteSegmentType.Start;
            route1segment2.Name = string.Empty;
            route1segment2.Names = null;

            route1.Segments = new RouteSegment[2];
            route1.Segments[0] = route1segment1;
            route1.Segments[1] = route1segment2;


            Route route2 = new Route();
            RouteSegment route2segment1 = new RouteSegment();
            route2segment1.Distance = 10;
            route2segment1.Latitude = -1;
            route2segment1.Longitude = -1;
            route2segment1.Metrics = null;
            route2segment1.Points = new RoutePoint[1];
            route2segment1.Points[0] = new RoutePoint();
            route2segment1.Points[0].Name = "TestPoint2";
            route2segment1.Points[0].Tags = new RouteTags[1];
            route2segment1.Points[0].Tags[0] = new RouteTags();
            route2segment1.Points[0].Tags[0].Value = "TestValue2";
            route2segment1.Points[0].Tags[0].Key = "TestKey2";
            route2segment1.SideStreets = null;
            route2segment1.Tags = new RouteTags[1];
            route2segment1.Tags[0] = new RouteTags();
            route2segment1.Tags[0].Key = "highway";
            route2segment1.Tags[0].Value = "residential";
            route2segment1.Time = 10;
            route2segment1.Type = RouteSegmentType.Start;
            route2segment1.Name = string.Empty;
            route2segment1.Names = null;

            RouteSegment route2segment2 = new RouteSegment();
            route2segment2.Distance = 10;
            route2segment2.Latitude = -1;
            route2segment2.Longitude = -1;
            route2segment2.Metrics = null;
            route2segment2.Points = new RoutePoint[1];
            route2segment2.Points[0] = new RoutePoint();
            route2segment2.Points[0].Name = "TestPoint4";
            route2segment2.Points[0].Tags = new RouteTags[1];
            route2segment2.Points[0].Tags[0] = new RouteTags();
            route2segment2.Points[0].Tags[0].Value = "TestValue4";
            route2segment2.Points[0].Tags[0].Key = "TestKey4";
            route2segment2.SideStreets = null;
            route2segment2.Tags = new RouteTags[1];
            route2segment2.Tags[0] = new RouteTags();
            route2segment2.Tags[0].Key = "highway";
            route2segment2.Tags[0].Value = "residential";
            route2segment2.Time = 10;
            route2segment2.Type = RouteSegmentType.Start;
            route2segment2.Name = string.Empty;
            route2segment2.Names = null;

            route2.Segments = new RouteSegment[2];
            route2.Segments[0] = route2segment1;
            route2.Segments[1] = route2segment2;

            Route concatenated = Route.Concatenate(route1, route2);

            // test the result.
            Assert.IsNotNull(concatenated);
            Assert.IsNotNull(concatenated.Segments);
            Assert.AreEqual(3, concatenated.Segments.Length);
            Assert.AreEqual("TestPoint1", concatenated.Segments[0].Points[0].Name);
            Assert.AreEqual("TestPoint2", concatenated.Segments[1].Points[0].Name);
            Assert.AreEqual(1, concatenated.Segments[1].Points.Length);
            Assert.AreEqual("TestPoint4", concatenated.Segments[2].Points[0].Name);
        }

        /// <summary>
        /// Tests the concatenation of segments with vehicles in a calculated route.
        /// </summary>
        [Test]
        public void RouteConcatenateVehiclesTest()
        {
            var route1 = new Route();
            route1.Vehicle = Vehicle.Car.UniqueName;
            var route1segment1 = new RouteSegment();
            route1segment1.Distance = 10;
            route1segment1.Latitude = -1;
            route1segment1.Longitude = -1;
            route1segment1.Metrics = null;
            route1segment1.Points = new RoutePoint[1];
            route1segment1.Points[0] = new RoutePoint();
            route1segment1.Points[0].Name = "TestPoint1";
            route1segment1.Points[0].Tags = new RouteTags[1];
            route1segment1.Points[0].Tags[0] = new RouteTags();
            route1segment1.Points[0].Tags[0].Value = "TestValue1";
            route1segment1.Points[0].Tags[0].Key = "TestKey1";
            route1segment1.SideStreets = null;
            route1segment1.Tags = new RouteTags[1];
            route1segment1.Tags[0] = new RouteTags();
            route1segment1.Tags[0].Key = "highway";
            route1segment1.Tags[0].Value = "residential";
            route1segment1.Time = 10;
            route1segment1.Type = RouteSegmentType.Start;
            route1segment1.Name = string.Empty;
            route1segment1.Names = null;
            route1segment1.Vehicle = Vehicle.Car.UniqueName;

            var route1segment2 = new RouteSegment();
            route1segment2.Distance = 10;
            route1segment2.Latitude = -1;
            route1segment2.Longitude = -1;
            route1segment2.Metrics = null;
            route1segment2.Points = new RoutePoint[1];
            route1segment2.Points[0] = new RoutePoint();
            route1segment2.Points[0].Name = "TestPoint2";
            route1segment2.Points[0].Tags = new RouteTags[1];
            route1segment2.Points[0].Tags[0] = new RouteTags();
            route1segment2.Points[0].Tags[0].Value = "TestValue2";
            route1segment1.Points[0].Tags[0].Key = "TestKey2";
            route1segment2.SideStreets = null;
            route1segment2.Tags = new RouteTags[1];
            route1segment2.Tags[0] = new RouteTags();
            route1segment2.Tags[0].Key = "highway";
            route1segment2.Tags[0].Value = "residential";
            route1segment2.Time = 10;
            route1segment2.Type = RouteSegmentType.Start;
            route1segment2.Name = string.Empty;
            route1segment2.Names = null;
            route1segment2.Vehicle = Vehicle.Car.UniqueName;

            route1.Segments = new RouteSegment[2];
            route1.Segments[0] = route1segment1;
            route1.Segments[1] = route1segment2;

            var route2 = new Route();
            route2.Vehicle = Vehicle.Pedestrian.UniqueName;
            var route2segment1 = new RouteSegment();
            route2segment1.Distance = 10;
            route2segment1.Latitude = -1;
            route2segment1.Longitude = -1;
            route2segment1.Metrics = null;
            route2segment1.Points = new RoutePoint[1];
            route2segment1.Points[0] = new RoutePoint();
            route2segment1.Points[0].Name = "TestPoint3";
            route2segment1.Points[0].Tags = new RouteTags[1];
            route2segment1.Points[0].Tags[0] = new RouteTags();
            route2segment1.Points[0].Tags[0].Value = "TestValue3";
            route2segment1.Points[0].Tags[0].Key = "TestKey3";
            route2segment1.SideStreets = null;
            route2segment1.Tags = new RouteTags[1];
            route2segment1.Tags[0] = new RouteTags();
            route2segment1.Tags[0].Key = "highway";
            route2segment1.Tags[0].Value = "residential";
            route2segment1.Time = 10;
            route2segment1.Type = RouteSegmentType.Start;
            route2segment1.Name = string.Empty;
            route2segment1.Names = null;
            route2segment1.Vehicle = Vehicle.Pedestrian.UniqueName;

            var route2segment2 = new RouteSegment();
            route2segment2.Distance = 10;
            route2segment2.Latitude = -1;
            route2segment2.Longitude = -1;
            route2segment2.Metrics = null;
            route2segment2.Points = new RoutePoint[1];
            route2segment2.Points[0] = new RoutePoint();
            route2segment2.Points[0].Name = "TestPoint4";
            route2segment2.Points[0].Tags = new RouteTags[1];
            route2segment2.Points[0].Tags[0] = new RouteTags();
            route2segment2.Points[0].Tags[0].Value = "TestValue4";
            route2segment1.Points[0].Tags[0].Key = "TestKey4";
            route2segment2.SideStreets = null;
            route2segment2.Tags = new RouteTags[1];
            route2segment2.Tags[0] = new RouteTags();
            route2segment2.Tags[0].Key = "highway";
            route2segment2.Tags[0].Value = "residential";
            route2segment2.Time = 10;
            route2segment2.Type = RouteSegmentType.Start;
            route2segment2.Name = string.Empty;
            route2segment2.Names = null;
            route2segment2.Vehicle = Vehicle.Pedestrian.UniqueName;

            route2.Segments = new RouteSegment[2];
            route2.Segments[0] = route2segment1;
            route2.Segments[1] = route2segment2;

            var concatenated = Route.Concatenate(route1, route2);

            // test the result.
            Assert.IsNotNull(concatenated);
            Assert.IsNotNull(concatenated.Segments);
            Assert.AreEqual(null, concatenated.Vehicle);
            Assert.AreEqual(3, concatenated.Segments.Length);
            Assert.AreEqual(Vehicle.Car.UniqueName, concatenated.Segments[0].Vehicle);
            Assert.AreEqual(Vehicle.Car.UniqueName, concatenated.Segments[1].Vehicle);
            Assert.AreEqual(Vehicle.Pedestrian.UniqueName, concatenated.Segments[2].Vehicle);
        }

        /// <summary>
        /// Tests the position after of route.
        /// </summary>
        [Test]
        public void RoutePositionAfterRegressionTest1()
        {
            var delta = .00001;

            // a route of approx 30 m along the following coordinates:
            // 50.98624687752063, 2.902620979360633
            // 50.98624687752063, 2.9027639004471673 (10m)
            // 50.986156907620895, 2.9027639004471673  (20m)
            // 50.9861564788317, 2.902620884621392 (30m)

            Route route1 = new Route();
            route1.Vehicle = Vehicle.Car.UniqueName;
            RouteSegment route1segment1 = new RouteSegment();
            route1segment1.Distance = -1;
            route1segment1.Latitude = 50.98624687752063f;
            route1segment1.Longitude = 2.902620979360633f;
            route1segment1.Metrics = null;
            route1segment1.Points = new RoutePoint[1];
            route1segment1.Points[0] = new RoutePoint();
            route1segment1.Points[0].Name = "TestPoint1";
            route1segment1.Points[0].Tags = new RouteTags[1];
            route1segment1.Points[0].Tags[0] = new RouteTags();
            route1segment1.Points[0].Tags[0].Value = "TestValue1";
            route1segment1.Points[0].Tags[0].Key = "TestKey1";
            route1segment1.SideStreets = null;
            route1segment1.Tags = new RouteTags[1];
            route1segment1.Tags[0] = new RouteTags();
            route1segment1.Tags[0].Key = "highway";
            route1segment1.Tags[0].Value = "residential";
            route1segment1.Time = 10;
            route1segment1.Type = RouteSegmentType.Start;
            route1segment1.Name = string.Empty;
            route1segment1.Names = null;

            RouteSegment route1segment2 = new RouteSegment();
            route1segment2.Distance = -1;
            route1segment2.Latitude = 50.98624687752063f;
            route1segment2.Longitude = 2.9027639004471673f;
            route1segment2.Metrics = null;
            route1segment2.Points = new RoutePoint[1];
            route1segment2.Points[0] = new RoutePoint();
            route1segment2.Points[0].Name = "TestPoint2";
            route1segment2.Points[0].Tags = new RouteTags[1];
            route1segment2.Points[0].Tags[0] = new RouteTags();
            route1segment2.Points[0].Tags[0].Value = "TestValue2";
            route1segment1.Points[0].Tags[0].Key = "TestKey2";
            route1segment2.SideStreets = null;
            route1segment2.Tags = new RouteTags[1];
            route1segment2.Tags[0] = new RouteTags();
            route1segment2.Tags[0].Key = "highway";
            route1segment2.Tags[0].Value = "residential";
            route1segment2.Time = 10;
            route1segment2.Type = RouteSegmentType.Start;
            route1segment2.Name = string.Empty;
            route1segment2.Names = null;

            RouteSegment route1segment3 = new RouteSegment();
            route1segment3.Distance = -1;
            route1segment3.Latitude = 50.986156907620895f;
            route1segment3.Longitude = 2.9027639004471673f;
            route1segment3.Metrics = null;
            route1segment3.Points = new RoutePoint[1];
            route1segment3.Points[0] = new RoutePoint();
            route1segment3.Points[0].Name = "TestPoint3";
            route1segment3.Points[0].Tags = new RouteTags[1];
            route1segment3.Points[0].Tags[0] = new RouteTags();
            route1segment3.Points[0].Tags[0].Value = "TestValue3";
            route1segment1.Points[0].Tags[0].Key = "TestKey3";
            route1segment3.SideStreets = null;
            route1segment3.Tags = new RouteTags[1];
            route1segment3.Tags[0] = new RouteTags();
            route1segment3.Tags[0].Key = "highway";
            route1segment3.Tags[0].Value = "residential";
            route1segment3.Time = 10;
            route1segment3.Type = RouteSegmentType.Start;
            route1segment3.Name = string.Empty;
            route1segment3.Names = null;

            RouteSegment route1segment4 = new RouteSegment();
            route1segment4.Distance = -1;
            route1segment4.Latitude = 50.9861564788317f;
            route1segment4.Longitude = 2.902620884621392f;
            route1segment4.Metrics = null;
            route1segment4.Points = new RoutePoint[1];
            route1segment4.Points[0] = new RoutePoint();
            route1segment4.Points[0].Name = "TestPoint4";
            route1segment4.Points[0].Tags = new RouteTags[1];
            route1segment4.Points[0].Tags[0] = new RouteTags();
            route1segment4.Points[0].Tags[0].Value = "TestValue4";
            route1segment1.Points[0].Tags[0].Key = "TestKey4";
            route1segment4.SideStreets = null;
            route1segment4.Tags = new RouteTags[1];
            route1segment4.Tags[0] = new RouteTags();
            route1segment4.Tags[0].Key = "highway";
            route1segment4.Tags[0].Value = "residential";
            route1segment4.Time = 10;
            route1segment4.Type = RouteSegmentType.Start;
            route1segment4.Name = string.Empty;
            route1segment4.Names = null;

            route1.Segments = new RouteSegment[4];
            route1.Segments[0] = route1segment1;
            route1.Segments[1] = route1segment2;
            route1.Segments[2] = route1segment3;
            route1.Segments[3] = route1segment4;

            // first test position after.
            var positionAfter = route1.PositionAfter(5);
            Assert.IsNotNull(positionAfter);
            Assert.AreEqual(5.0, positionAfter.DistanceReal(new GeoCoordinate(route1.Segments[0].Latitude, route1.Segments[0].Longitude)).Value, .0001);
            positionAfter = route1.PositionAfter(15);
            Assert.IsNotNull(positionAfter);
            Assert.AreEqual(15.0, 
                new GeoCoordinate(route1.Segments[0].Latitude, route1.Segments[0].Longitude).DistanceReal(new GeoCoordinate(route1.Segments[1].Latitude, route1.Segments[1].Longitude)).Value +
                positionAfter.DistanceReal(new GeoCoordinate(route1.Segments[1].Latitude, route1.Segments[1].Longitude)).Value, .0001);
            positionAfter = route1.PositionAfter(25);
            Assert.IsNotNull(positionAfter);
            Assert.AreEqual(25.0,
                new GeoCoordinate(route1.Segments[0].Latitude, route1.Segments[0].Longitude).DistanceReal(new GeoCoordinate(route1.Segments[1].Latitude, route1.Segments[1].Longitude)).Value +
                new GeoCoordinate(route1.Segments[1].Latitude, route1.Segments[1].Longitude).DistanceReal(new GeoCoordinate(route1.Segments[2].Latitude, route1.Segments[2].Longitude)).Value +
                positionAfter.DistanceReal(new GeoCoordinate(route1.Segments[2].Latitude, route1.Segments[2].Longitude)).Value, .0001);

            // use position after to test project on.
            int segmentIdx;
            GeoCoordinate projected;
            Meter distanceFromStart;
            Second timeFromStart;

            var distance = 5.0;
            var location = route1.PositionAfter(distance);
            Assert.IsTrue(route1.ProjectOn(location, out projected, out segmentIdx, out distanceFromStart, out timeFromStart));
            Assert.AreEqual(distance, distanceFromStart.Value, delta);
            Assert.AreEqual(location.Latitude, projected.Latitude, delta);
            Assert.AreEqual(location.Longitude, projected.Longitude, delta);
            Assert.AreEqual(0, segmentIdx);

            location = route1.PositionAfter(distanceFromStart);
            Assert.AreEqual(location.Latitude, projected.Latitude, delta);
            Assert.AreEqual(location.Longitude, projected.Longitude, delta);

            distance = 15.0;
            location = route1.PositionAfter(distance);
            Assert.IsTrue(route1.ProjectOn(location, out projected, out segmentIdx, out distanceFromStart, out timeFromStart));
            Assert.AreEqual(distance, distanceFromStart.Value, delta);
            Assert.AreEqual(location.Latitude, projected.Latitude, delta);
            Assert.AreEqual(location.Longitude, projected.Longitude, delta);
            Assert.AreEqual(1, segmentIdx);

            location = route1.PositionAfter(distanceFromStart);
            Assert.AreEqual(location.Latitude, projected.Latitude, delta);
            Assert.AreEqual(location.Longitude, projected.Longitude, delta);

            distance = 25;
            location = route1.PositionAfter(distance);
            Assert.IsTrue(route1.ProjectOn(location, out projected, out segmentIdx, out distanceFromStart, out timeFromStart));
            Assert.AreEqual(distance, distanceFromStart.Value, delta);
            Assert.AreEqual(location.Latitude, projected.Latitude, delta);
            Assert.AreEqual(location.Longitude, projected.Longitude, delta);
            Assert.AreEqual(2, segmentIdx);

            location = route1.PositionAfter(distanceFromStart);
            Assert.AreEqual(location.Latitude, projected.Latitude, delta);
            Assert.AreEqual(location.Longitude, projected.Longitude, delta);
        }

        /// <summary>
        /// Tests the conversion of a route to a feature collection.
        /// </summary>
        [Test]
        public void RouteToFeatureCollection()
        {
            // build a test route.
            var route = new Route();
            route.Vehicle = Vehicle.Car.UniqueName;
            var route1segment1 = new RouteSegment();
            route1segment1.Distance = 10;
            route1segment1.Latitude = -1;
            route1segment1.Longitude = -1;
            route1segment1.Metrics = null;
            route1segment1.Points = new RoutePoint[1];
            route1segment1.Points[0] = new RoutePoint();
            route1segment1.Points[0].Name = "TestPoint1";
            route1segment1.Points[0].Tags = new RouteTags[1];
            route1segment1.Points[0].Tags[0] = new RouteTags();
            route1segment1.Points[0].Tags[0].Value = "TestValue1";
            route1segment1.Points[0].Tags[0].Key = "TestKey1";
            route1segment1.SideStreets = null;
            route1segment1.Tags = new RouteTags[1];
            route1segment1.Tags[0] = new RouteTags();
            route1segment1.Tags[0].Key = "highway";
            route1segment1.Tags[0].Value = "residential";
            route1segment1.Time = 10;
            route1segment1.Type = RouteSegmentType.Start;
            route1segment1.Name = string.Empty;
            route1segment1.Names = null;

            var route1segment2 = new RouteSegment();
            route1segment2.Distance = 10;
            route1segment2.Latitude = -1;
            route1segment2.Longitude = -1;
            route1segment2.Metrics = null;
            route1segment2.Points = new RoutePoint[1];
            route1segment2.Points[0] = new RoutePoint();
            route1segment2.Points[0].Name = "TestPoint2";
            route1segment2.Points[0].Tags = new RouteTags[1];
            route1segment2.Points[0].Tags[0] = new RouteTags();
            route1segment2.Points[0].Tags[0].Value = "TestValue2";
            route1segment2.Points[0].Tags[0].Key = "TestKey2";
            route1segment2.SideStreets = null;
            route1segment2.Tags = new RouteTags[1];
            route1segment2.Tags[0] = new RouteTags();
            route1segment2.Tags[0].Key = "highway";
            route1segment2.Tags[0].Value = "residential";
            route1segment2.Time = 10;
            route1segment2.Type = RouteSegmentType.Start;
            route1segment2.Name = string.Empty;
            route1segment2.Names = null;

            route.Segments = new RouteSegment[2];
            route.Segments[0] = route1segment1;
            route.Segments[1] = route1segment2;

            // execute the conversion.
            var features = route.ToFeatureCollection();

            // check result, two points, one linestring.
            Assert.IsNotNull(features);
            Assert.AreEqual(3, features.Count); 
            var featuresList = new List<Feature>(features);
            Assert.IsInstanceOf<Point>(featuresList[0].Geometry);
            Assert.IsInstanceOf<LineString>(featuresList[1].Geometry);
            Assert.IsInstanceOf<Point>(featuresList[2].Geometry);
        }

        /// <summary>
        /// Tests the conversion of a route to GeoJson.
        /// </summary>
        [Test]
        public void RouteToGeoJson()
        {
            // build a test route.
            var route = new Route();
            route.Vehicle = Vehicle.Car.UniqueName;
            var route1segment1 = new RouteSegment();
            route1segment1.Distance = 0;
            route1segment1.Latitude = 1;
            route1segment1.Longitude = 1;
            route1segment1.Metrics = null;
            route1segment1.Points = new RoutePoint[1];
            route1segment1.Points[0] = new RoutePoint();
            route1segment1.Points[0].Name = "TestPoint1";
            route1segment1.Points[0].Tags = new RouteTags[1];
            route1segment1.Points[0].Tags[0] = new RouteTags();
            route1segment1.Points[0].Tags[0].Value = "TestValue1";
            route1segment1.Points[0].Tags[0].Key = "TestKey1";
            route1segment1.SideStreets = null;
            route1segment1.Tags = new RouteTags[1];
            route1segment1.Tags[0] = new RouteTags();
            route1segment1.Tags[0].Key = "highway";
            route1segment1.Tags[0].Value = "residential";
            route1segment1.Time = 0;
            route1segment1.Type = RouteSegmentType.Start;
            route1segment1.Name = string.Empty;
            route1segment1.Names = null;

            var route1segment2 = new RouteSegment();
            route1segment2.Distance = 10;
            route1segment2.Latitude = 2;
            route1segment2.Longitude = 2;
            route1segment2.Metrics = null;
            route1segment2.Points = new RoutePoint[1];
            route1segment2.Points[0] = new RoutePoint();
            route1segment2.Points[0].Name = "TestPoint2";
            route1segment2.Points[0].Tags = new RouteTags[1];
            route1segment2.Points[0].Tags[0] = new RouteTags();
            route1segment2.Points[0].Tags[0].Value = "TestValue2";
            route1segment2.Points[0].Tags[0].Key = "TestKey2";
            route1segment2.SideStreets = null;
            route1segment2.Tags = new RouteTags[1];
            route1segment2.Tags[0] = new RouteTags();
            route1segment2.Tags[0].Key = "highway";
            route1segment2.Tags[0].Value = "residential";
            route1segment2.Time = 20;
            route1segment2.Type = RouteSegmentType.Start;
            route1segment2.Name = string.Empty;
            route1segment2.Names = null;

            route.Segments = new RouteSegment[2];
            route.Segments[0] = route1segment1;
            route.Segments[1] = route1segment2;

            // execute the conversion.
            var geojson = route.ToGeoJson();
            geojson = geojson.RemoveWhitespace();

            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"properties\":{\"TestKey1\":\"TestValue1\"},\"geometry\":{\"type\":\"Point\",\"coordinates\":[0.0,0.0]}},{\"type\":\"Feature\",\"properties\":{\"highway\":\"residential\",\"time\":20.0,\"distance\":10.0},\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[2.0,2.0]]}},{\"type\":\"Feature\",\"properties\":{\"TestKey2\":\"TestValue2\"},\"geometry\":{\"type\":\"Point\",\"coordinates\":[0.0,0.0]}}]}",
                geojson);
        }
    }
}
