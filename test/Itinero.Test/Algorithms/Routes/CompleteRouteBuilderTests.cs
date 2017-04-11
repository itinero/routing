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
using Itinero.Algorithms.Routes;
using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Data.Network;
using Itinero.Test.Profiles;
using System.Collections.Generic;
using System.Linq;
using Itinero.Data.Network.Edges;
using Itinero.Profiles;

namespace Itinero.Test.Algorithms.Routes
{
    /// <summary>
    /// Contains tests for route builder.
    /// </summary>
    [TestFixture]
    public class CompleteRouteBuilderTests
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
            var edgeId = routerDb.Network.AddEdge(0, 1, new EdgeData(), null);

            // build route.
            var source = new RouterPoint(0, 0, edgeId, 0, new Attribute("type", "source"));
            var target = new RouterPoint(0, 0, edgeId, 0, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(1, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(1, route.ShapeMeta.Length);
            Assert.AreEqual(profile.FullName, route.ShapeMeta[0].Profile);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            Assert.IsNotNull(route.Stops.FirstOrDefault(x => x.Attributes.Contains("type", "source")));
            Assert.IsNotNull(route.Stops.FirstOrDefault(x => x.Attributes.Contains("type", "target")));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
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
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, null);

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Attribute("type", "source"));
            var target = new RouterPoint(1, 1, 0, ushort.MaxValue, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(2, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(1, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(2, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            var speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);

            // build route with similar path.
            routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(2, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(1, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(2, route.ShapeMeta.Length);
            meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);

            // build route with similar path.
            routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(2, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(1, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(2, route.ShapeMeta.Length);
            meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
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
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.25f, 0.25f),
               new Coordinate(0.50f, 0.50f),
               new Coordinate(0.75f, 0.75f));

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Attribute("type", "source"));
            var target = new RouterPoint(1, 1, 0, ushort.MaxValue, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            var speed = profile.Speed(new AttributeCollection(
                    new Attribute("highway", "residential")));

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(5, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(.25f, route.Shape[1].Latitude);
            Assert.AreEqual(.25f, route.Shape[1].Longitude);
            Assert.AreEqual(.5f, route.Shape[2].Latitude);
            Assert.AreEqual(.5f, route.Shape[2].Longitude);
            Assert.AreEqual(.75f, route.Shape[3].Latitude);
            Assert.AreEqual(.75f, route.Shape[3].Longitude);
            Assert.AreEqual(1, route.Shape[4].Latitude);
            Assert.AreEqual(1, route.Shape[4].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(2, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }

        /// <summary>
        /// Tests a route with two edges and shapes between.
        /// </summary>
        [Test]
        public void TestTwoEdgeWithShape()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.EdgeMeta.Add(new AttributeCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.25f, 0.25f),
               new Coordinate(0.5f, 0.5f),
               new Coordinate(0.75f, 0.75f));
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.75f, 1),
               new Coordinate(0.5f, 1),
               new Coordinate(0.25f, 1));

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Attribute("type", "source"));
            var target = new RouterPoint(0, 1, 1, ushort.MaxValue, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, 1, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;

            var speed = profile.Speed(new AttributeCollection(
                    new Attribute("highway", "residential")));

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(9, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(.25f, route.Shape[1].Latitude);
            Assert.AreEqual(.25f, route.Shape[1].Longitude);
            Assert.AreEqual(.5f, route.Shape[2].Latitude);
            Assert.AreEqual(.5f, route.Shape[2].Longitude);
            Assert.AreEqual(.75f, route.Shape[3].Latitude);
            Assert.AreEqual(.75f, route.Shape[3].Longitude);
            Assert.AreEqual(1, route.Shape[4].Latitude);
            Assert.AreEqual(1, route.Shape[4].Longitude);
            Assert.AreEqual(.75f, route.Shape[5].Latitude);
            Assert.AreEqual(1f, route.Shape[5].Longitude);
            Assert.AreEqual(.5f, route.Shape[6].Latitude);
            Assert.AreEqual(1f, route.Shape[6].Longitude);
            Assert.AreEqual(.25f, route.Shape[7].Latitude);
            Assert.AreEqual(1f, route.Shape[7].Longitude);
            Assert.AreEqual(0f, route.Shape[8].Latitude);
            Assert.AreEqual(1f, route.Shape[8].Longitude);
            
            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);

            // build route.
            routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1, 2 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(9, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(.25f, route.Shape[1].Latitude);
            Assert.AreEqual(.25f, route.Shape[1].Longitude);
            Assert.AreEqual(.5f, route.Shape[2].Latitude);
            Assert.AreEqual(.5f, route.Shape[2].Longitude);
            Assert.AreEqual(.75f, route.Shape[3].Latitude);
            Assert.AreEqual(.75f, route.Shape[3].Longitude);
            Assert.AreEqual(1, route.Shape[4].Latitude);
            Assert.AreEqual(1, route.Shape[4].Longitude);
            Assert.AreEqual(.75f, route.Shape[5].Latitude);
            Assert.AreEqual(1f, route.Shape[5].Longitude);
            Assert.AreEqual(.5f, route.Shape[6].Latitude);
            Assert.AreEqual(1f, route.Shape[6].Longitude);
            Assert.AreEqual(.25f, route.Shape[7].Latitude);
            Assert.AreEqual(1f, route.Shape[7].Longitude);
            Assert.AreEqual(0f, route.Shape[8].Latitude);
            Assert.AreEqual(1f, route.Shape[8].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(3, route.ShapeMeta.Length);
            meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }

        /// <summary>
        /// Tests a route with three edges and shapes between.
        /// </summary>
        [Test]
        public void TestThreeEdgeWithShape()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeMeta.Add(new AttributeCollection());
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddVertex(3, 0, 2);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.25f, 0.25f),
               new Coordinate(0.50f, 0.50f),
               new Coordinate(0.75f, 0.75f));
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0.75f, 1),
               new Coordinate(0.50f, 1),
               new Coordinate(0.25f, 1));
            routerDb.Network.AddEdge(2, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            }, new Coordinate(0, 1.25f),
               new Coordinate(0, 1.50f),
               new Coordinate(0, 1.75f));

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Attribute("type", "source"));
            var target = new RouterPoint(0, 2, 2, ushort.MaxValue, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, 1, 2, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(13, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(.25f, route.Shape[1].Latitude);
            Assert.AreEqual(.25f, route.Shape[1].Longitude);
            Assert.AreEqual(.5f, route.Shape[2].Latitude);
            Assert.AreEqual(.5f, route.Shape[2].Longitude);
            Assert.AreEqual(.75f, route.Shape[3].Latitude);
            Assert.AreEqual(.75f, route.Shape[3].Longitude);
            Assert.AreEqual(1, route.Shape[4].Latitude);
            Assert.AreEqual(1, route.Shape[4].Longitude);
            Assert.AreEqual(.75f, route.Shape[5].Latitude);
            Assert.AreEqual(1f, route.Shape[5].Longitude);
            Assert.AreEqual(.5f, route.Shape[6].Latitude);
            Assert.AreEqual(1f, route.Shape[6].Longitude);
            Assert.AreEqual(.25f, route.Shape[7].Latitude);
            Assert.AreEqual(1f, route.Shape[7].Longitude);
            Assert.AreEqual(0f, route.Shape[8].Latitude);
            Assert.AreEqual(1f, route.Shape[8].Longitude);
            Assert.AreEqual(0f, route.Shape[9].Latitude);
            Assert.AreEqual(1.25f, route.Shape[9].Longitude);
            Assert.AreEqual(0f, route.Shape[10].Latitude);
            Assert.AreEqual(1.5f, route.Shape[10].Longitude);
            Assert.AreEqual(0f, route.Shape[11].Latitude);
            Assert.AreEqual(1.75f, route.Shape[11].Longitude);
            Assert.AreEqual(0f, route.Shape[12].Latitude);
            Assert.AreEqual(2f, route.Shape[12].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            var speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.01);
            meta = route.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(3000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(3000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);

            // build route.
            routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1, 2, 3 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(13, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(.25f, route.Shape[1].Latitude);
            Assert.AreEqual(.25f, route.Shape[1].Longitude);
            Assert.AreEqual(.5f, route.Shape[2].Latitude);
            Assert.AreEqual(.5f, route.Shape[2].Longitude);
            Assert.AreEqual(.75f, route.Shape[3].Latitude);
            Assert.AreEqual(.75f, route.Shape[3].Longitude);
            Assert.AreEqual(1, route.Shape[4].Latitude);
            Assert.AreEqual(1, route.Shape[4].Longitude);
            Assert.AreEqual(.75f, route.Shape[5].Latitude);
            Assert.AreEqual(1f, route.Shape[5].Longitude);
            Assert.AreEqual(.5f, route.Shape[6].Latitude);
            Assert.AreEqual(1f, route.Shape[6].Longitude);
            Assert.AreEqual(.25f, route.Shape[7].Latitude);
            Assert.AreEqual(1f, route.Shape[7].Longitude);
            Assert.AreEqual(0f, route.Shape[8].Latitude);
            Assert.AreEqual(1f, route.Shape[8].Longitude);
            Assert.AreEqual(0f, route.Shape[9].Latitude);
            Assert.AreEqual(1.25f, route.Shape[9].Longitude);
            Assert.AreEqual(0f, route.Shape[10].Latitude);
            Assert.AreEqual(1.5f, route.Shape[10].Longitude);
            Assert.AreEqual(0f, route.Shape[11].Latitude);
            Assert.AreEqual(1.75f, route.Shape[11].Longitude);
            Assert.AreEqual(0f, route.Shape[12].Latitude);
            Assert.AreEqual(2f, route.Shape[12].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);
            meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.01);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.01);
            meta = route.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(3000, meta.Distance, 0.01);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(3000 / speed.Value, meta.Time, 0.01);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);

            // build route.
            source = new RouterPoint(0.4f, 0.4f, 0, ushort.MaxValue / 10 * 4, new Attribute("type", "source"));
            routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, 1, 2, 3 }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);

            var e = 0.0001f;
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(12, route.Shape.Length);
            Assert.AreEqual(0.4f, route.Shape[0].Latitude, e);
            Assert.AreEqual(0.4f, route.Shape[0].Longitude, e);
            Assert.AreEqual(.5f, route.Shape[1].Latitude);
            Assert.AreEqual(.5f, route.Shape[1].Longitude);
            Assert.AreEqual(.75f, route.Shape[2].Latitude);
            Assert.AreEqual(.75f, route.Shape[2].Longitude);
            Assert.AreEqual(1, route.Shape[3].Latitude);
            Assert.AreEqual(1, route.Shape[3].Longitude);
            Assert.AreEqual(.75f, route.Shape[4].Latitude);
            Assert.AreEqual(1f, route.Shape[4].Longitude);
            Assert.AreEqual(.5f, route.Shape[5].Latitude);
            Assert.AreEqual(1f, route.Shape[5].Longitude);
            Assert.AreEqual(.25f, route.Shape[6].Latitude);
            Assert.AreEqual(1f, route.Shape[6].Longitude);
            Assert.AreEqual(0f, route.Shape[7].Latitude);
            Assert.AreEqual(1f, route.Shape[7].Longitude);
            Assert.AreEqual(0f, route.Shape[8].Latitude);
            Assert.AreEqual(1.25f, route.Shape[8].Longitude);
            Assert.AreEqual(0f, route.Shape[9].Latitude);
            Assert.AreEqual(1.5f, route.Shape[9].Longitude);
            Assert.AreEqual(0f, route.Shape[10].Latitude);
            Assert.AreEqual(1.75f, route.Shape[10].Longitude);
            Assert.AreEqual(0f, route.Shape[11].Latitude);
            Assert.AreEqual(2f, route.Shape[11].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);
            meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(600, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(600 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1600, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1600 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2600, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2600 / speed.Value, meta.Time, 0.1);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);

            // build route.
            source = new RouterPoint(0, 0, 0, 0, new Attribute("type", "source"));
            target = new RouterPoint(0, 1.6f, 2, ushort.MaxValue / 10 * 6, new Attribute("type", "target"));
            routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 0, 1, 2, Constants.NO_VERTEX }));
            routeBuilder.Run();

            // check result.
            route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(12, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(.25f, route.Shape[1].Latitude);
            Assert.AreEqual(.25f, route.Shape[1].Longitude);
            Assert.AreEqual(.5f, route.Shape[2].Latitude);
            Assert.AreEqual(.5f, route.Shape[2].Longitude);
            Assert.AreEqual(.75f, route.Shape[3].Latitude);
            Assert.AreEqual(.75f, route.Shape[3].Longitude);
            Assert.AreEqual(1, route.Shape[4].Latitude);
            Assert.AreEqual(1, route.Shape[4].Longitude);
            Assert.AreEqual(.75f, route.Shape[5].Latitude);
            Assert.AreEqual(1f, route.Shape[5].Longitude);
            Assert.AreEqual(.5f, route.Shape[6].Latitude);
            Assert.AreEqual(1f, route.Shape[6].Longitude);
            Assert.AreEqual(.25f, route.Shape[7].Latitude);
            Assert.AreEqual(1f, route.Shape[7].Longitude);
            Assert.AreEqual(0f, route.Shape[8].Latitude);
            Assert.AreEqual(1f, route.Shape[8].Longitude);
            Assert.AreEqual(0f, route.Shape[9].Latitude);
            Assert.AreEqual(1.25f, route.Shape[9].Longitude);
            Assert.AreEqual(0f, route.Shape[10].Latitude);
            Assert.AreEqual(1.5f, route.Shape[10].Longitude);
            Assert.AreEqual(0f, route.Shape[11].Latitude);
            Assert.AreEqual(1.6f, route.Shape[11].Longitude, e);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);
            meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2600, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2600 / speed.Value, meta.Time, 0.1);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }

        /// <summary>
        /// Tests a route with three edges and the last point in the path is a vertex but the routepoint is on an edge not in the path.
        /// </summary>
        [Test]
        public void TestThreeEdgeLastVertexRouterPointDifferentEdge()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeMeta.Add(new AttributeCollection());
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddVertex(3, 0, 2);
            routerDb.Network.AddVertex(4, 1, 2);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(2, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(4, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });

            // build route.
            var source = new RouterPoint(0, 0, 0, 0, new Attribute("type", "source"));
            var target = new RouterPoint(0, 2, 3, ushort.MaxValue, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { Constants.NO_VERTEX, 1, 2, 3 }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(4, route.Shape.Length);
            Assert.AreEqual(0, route.Shape[0].Latitude);
            Assert.AreEqual(0, route.Shape[0].Longitude);
            Assert.AreEqual(1, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);
            Assert.AreEqual(0, route.Shape[2].Latitude);
            Assert.AreEqual(1, route.Shape[2].Longitude);
            Assert.AreEqual(0, route.Shape[3].Latitude);
            Assert.AreEqual(2, route.Shape[3].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.1);
            var speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(3000, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(3000 / speed.Value, meta.Time, 0.1);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }

        /// <summary>
        /// Tests a route with three edges and the first point in the path is a vertex but the routepoint is on an edge not in the path.
        /// </summary>
        [Test]
        public void TestThreeEdgeFirstVertexRouterPointDifferentEdge()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeMeta.Add(new AttributeCollection());
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddVertex(3, 0, 2);
            routerDb.Network.AddVertex(4, 1, 2);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(2, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(4, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });

            // build route.
            var source = new RouterPoint(1, 1, 0, ushort.MaxValue, new Attribute("type", "source"));
            var target = new RouterPoint(1, 2, 3, 0, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 1, 2, 3, 4 }));
            routeBuilder.Run();

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(4, route.Shape.Length);
            Assert.AreEqual(1, route.Shape[0].Latitude);
            Assert.AreEqual(1, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);
            Assert.AreEqual(0, route.Shape[2].Latitude);
            Assert.AreEqual(2, route.Shape[2].Longitude);
            Assert.AreEqual(1, route.Shape[3].Latitude);
            Assert.AreEqual(2, route.Shape[3].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(4, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.1);
            var speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(1000 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[2];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(2000, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(2000 / speed.Value, meta.Time, 0.1);
            meta = route.ShapeMeta[3];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(3000, meta.Distance, 0.1);
            speed = profile.Speed(meta.Attributes);
            Assert.AreEqual(3000 / speed.Value, meta.Time, 0.1);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }

        /// <summary>
        /// Tests a route with two edges and the last point in the path is a vertex but the routepoint is on an edge not in the path.
        /// </summary>
        [Test]
        public void TestTwoEdgeLastVertexRouterPointDifferentEdge()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeMeta.Add(new AttributeCollection());
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddVertex(3, 0, 2);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(2, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });

            // build route.
            var source = new RouterPoint(1, 1, 0, ushort.MaxValue, new Attribute("type", "source"));
            var target = new RouterPoint(0, 1, 2, 0, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 1, 2 }));
            routeBuilder.Run();

            Assert.IsTrue(routeBuilder.HasSucceeded);

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(2, route.Shape.Length);
            Assert.AreEqual(1, route.Shape[0].Latitude);
            Assert.AreEqual(1, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(2, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.1);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }

        /// <summary>
        /// Tests a route where the end is equal to resolved vertex.
        /// </summary>
        [Test]
        public void TestEndEqual()
        {
            // build router db.
            var routerDb = new RouterDb();
            routerDb.EdgeMeta.Add(new AttributeCollection());
            routerDb.EdgeProfiles.Add(new AttributeCollection());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 0, 1);
            routerDb.Network.AddVertex(3, 0, 2);
            routerDb.Network.AddEdge(0, 1, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(1, 2, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });
            routerDb.Network.AddEdge(2, 3, new EdgeData()
            {
                Distance = 1000,
                MetaId = routerDb.EdgeMeta.Add(new AttributeCollection(
                    new Attribute("name", "Abelshausen Blvd."))),
                Profile = (ushort)routerDb.EdgeProfiles.Add(new AttributeCollection(
                    new Attribute("highway", "residential")))
            });

            // build route.
            var source = new RouterPoint(1, 1, 0, ushort.MaxValue, new Attribute("type", "source"));
            var target = new RouterPoint(0, 1, 2, 0, new Attribute("type", "target"));
            var profile = VehicleMock.Car().Fastest();
            var routeBuilder = new CompleteRouteBuilder(routerDb, profile,
                source, target, new List<uint>(new uint[] { 1, 2, Constants.NO_VERTEX }));
            routeBuilder.Run();

            Assert.IsTrue(routeBuilder.HasSucceeded);

            // check result.
            var route = routeBuilder.Route;
            Assert.IsNotNull(route);

            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(2, route.Shape.Length);
            Assert.AreEqual(1, route.Shape[0].Latitude);
            Assert.AreEqual(1, route.Shape[0].Longitude);
            Assert.AreEqual(0, route.Shape[1].Latitude);
            Assert.AreEqual(1, route.Shape[1].Longitude);

            Assert.IsNotNull(route.ShapeMeta);
            Assert.AreEqual(2, route.ShapeMeta.Length);
            var meta = route.ShapeMeta[0];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(1, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            meta = route.ShapeMeta[1];
            Assert.IsNotNull(meta.Attributes);
            Assert.AreEqual(5, meta.Attributes.Count);
            Assert.IsTrue(meta.Attributes.Contains("highway", "residential"));
            Assert.IsTrue(meta.Attributes.Contains("name", "Abelshausen Blvd."));
            Assert.IsTrue(meta.Attributes.Contains("profile", profile.FullName));
            Assert.AreEqual(1000, meta.Distance, 0.1);

            Assert.IsNotNull(route.Stops);
            Assert.AreEqual(2, route.Stops.Length);
            var stop = route.Stops[0];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "source"));
            stop = route.Stops[1];
            Assert.IsNotNull(stop.Attributes);
            Assert.AreEqual(3, stop.Attributes.Count);
            Assert.IsTrue(stop.Attributes.Contains("type", "target"));

            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Distance, route.TotalDistance);
            Assert.AreEqual(route.ShapeMeta[route.ShapeMeta.Length - 1].Time, route.TotalTime);
        }
    }
}