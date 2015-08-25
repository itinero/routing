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

using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using OsmSharp.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.Streams.Filters;
using OsmSharp.Osm.Xml.Streams;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Holds some regression tests of previously detected routing issues.
    /// </summary>
    [TestFixture]
    public class RoutingRegressionTests
    {
        /// <summary>
        /// Issue with resolved points and incorrect routing.
        /// Some routes will pass through the same point twice.
        /// </summary>
        [Test]
        public void RoutingRegressionTest1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(memoryData, interpreter, tagsIndex);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression1.osm"));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            var basicRouter = new Dykstra();
            var router = Router.CreateFrom(memoryData, basicRouter, interpreter);

            // resolve the three points in question.
            var point35 = new GeoCoordinate(51.01257, 4.000753);
            var point35resolved = router.Resolve(Vehicle.Car, point35);
            var point45 = new GeoCoordinate(51.01315, 3.999588);
            var point45resolved = router.Resolve(Vehicle.Car, point45);

            // route between 35 and 45.
            var routebefore = router.Calculate(Vehicle.Car, point35resolved, point45resolved);

            // route between 35 and 45.
            var routeafter = router.Calculate(Vehicle.Car, point35resolved, point45resolved);
            Assert.AreEqual(routebefore.TotalDistance, routeafter.TotalDistance);
        }

        /// <summary>
        /// Issue with high-density resolved points. Routes are incorrectly skipping nodes.
        /// Some routes are not on known roads as a result.
        /// </summary>
        [Test]
        public void RoutingRegressionTest2()
        {
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(memoryData, interpreter, tagsIndex);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_network.osm"));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            var basicRouter = new Dykstra();
            var router = Router.CreateFrom(memoryData, basicRouter, interpreter);

            // build coordinates list of resolved points.
            var testPoints = new List<GeoCoordinate>();
            testPoints.Add(new GeoCoordinate(51.0582204, 3.7193524));
            testPoints.Add(new GeoCoordinate(51.0582199, 3.7194002));

            testPoints.Add(new GeoCoordinate(51.0581727, 3.7195833));
            testPoints.Add(new GeoCoordinate(51.0581483, 3.7195553));

            testPoints.Add(new GeoCoordinate(51.0581883, 3.7196617));
            testPoints.Add(new GeoCoordinate(51.0581628, 3.7196889));

            // build a matrix of routes between all points.
            var referenceRoutes = new Route[testPoints.Count][];
            var permuationArray = new int[testPoints.Count];
            for (int fromIdx = 0; fromIdx < testPoints.Count; fromIdx++)
            {
                permuationArray[fromIdx] = fromIdx;
                referenceRoutes[fromIdx] = new Route[testPoints.Count];
                for (int toIdx = 0; toIdx < testPoints.Count; toIdx++)
                {
                    // create router from scratch.
                    router = Router.CreateFrom(
                        memoryData, basicRouter, interpreter);

                    // resolve points.
                    var from = router.Resolve(Vehicle.Car, testPoints[fromIdx]);
                    var to = router.Resolve(Vehicle.Car, testPoints[toIdx]);

                    // calculate route.
                    referenceRoutes[fromIdx][toIdx] = router.Calculate(Vehicle.Car, from, to);
                }
            }

            // resolve points in some order and compare the resulting routes.
            // they should be identical in length except for some numerical rounding errors.
            var enumerator = new PermutationEnumerable<int>(permuationArray);
            foreach (int[] permutation in enumerator)
            {
                // create router from scratch.
                router = Router.CreateFrom(memoryData, basicRouter, interpreter);

                // resolve in the order of the permutation.
                var resolvedPoints = new RouterPoint[permutation.Length];
                for (int idx = 0; idx < permutation.Length; idx++)
                {
                    resolvedPoints[permutation[idx]] = router.Resolve(Vehicle.Car, testPoints[permutation[idx]]);
                }

                for (int fromIdx = 0; fromIdx < testPoints.Count; fromIdx++)
                {
                    for (int toIdx = 0; toIdx < testPoints.Count; toIdx++)
                    {
                        // calculate route.
                        var route = router.Calculate(Vehicle.Car, resolvedPoints[fromIdx], resolvedPoints[toIdx]);

                        // TODO: changed the resolve accuracy to .5m. Make sure this is more accurate in the future.
                        Assert.AreEqual(referenceRoutes[fromIdx][toIdx].TotalDistance, route.TotalDistance, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Issue with high-density resolved points. Routes are incorrectly skipping nodes.
        /// Some routes are not on known roads as a result.
        /// </summary>
        [Test]
        public void RoutingRegressionTest3()
        {
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(memoryData, interpreter, tagsIndex);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_network.osm"));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            var basicRouter = new Dykstra();
            var router = Router.CreateFrom(memoryData, basicRouter, interpreter);

            // build coordinates list of resolved points.
            var testPoints = new List<GeoCoordinate>();
            testPoints.Add(new GeoCoordinate(51.0581719, 3.7201622));
            testPoints.Add(new GeoCoordinate(51.0580439, 3.7202134));
            testPoints.Add(new GeoCoordinate(51.0580573, 3.7204378));
            testPoints.Add(new GeoCoordinate(51.0581862, 3.7203758));

            // build a matrix of routes between all points.
            var referenceRoutes = new Route[testPoints.Count][];
            var permuationArray = new int[testPoints.Count];
            for (int fromIdx = 0; fromIdx < testPoints.Count; fromIdx++)
            {
                permuationArray[fromIdx] = fromIdx;
                referenceRoutes[fromIdx] = new Route[testPoints.Count];
                for (int toIdx = 0; toIdx < testPoints.Count; toIdx++)
                {
                    // create router from scratch.
                    router = Router.CreateFrom(
                        memoryData, basicRouter, interpreter);

                    // resolve points.
                    var from = router.Resolve(Vehicle.Car, testPoints[fromIdx]);
                    var to = router.Resolve(Vehicle.Car, testPoints[toIdx]);

                    // calculate route.
                    referenceRoutes[fromIdx][toIdx] = router.Calculate(Vehicle.Car, from, to);
                }
            }

            // resolve points in some order and compare the resulting routes.
            // they should be identical in length except for some numerical rounding errors.
            var enumerator = new PermutationEnumerable<int>(
                permuationArray);
            foreach (int[] permutation in enumerator)
            {
                // create router from scratch.
                router = Router.CreateFrom(memoryData, basicRouter, interpreter);

                // resolve in the order of the permutation.
                var resolvedPoints = new RouterPoint[permutation.Length];
                for (int idx = 0; idx < permutation.Length; idx++)
                {
                    resolvedPoints[permutation[idx]] = router.Resolve(Vehicle.Car, testPoints[permutation[idx]]);
                }

                for (int fromIdx = 0; fromIdx < testPoints.Count; fromIdx++)
                {
                    for (int toIdx = 0; toIdx < testPoints.Count; toIdx++)
                    {
                        // calculate route.
                        var route = router.Calculate(Vehicle.Car, resolvedPoints[fromIdx], resolvedPoints[toIdx]);

                        Assert.AreEqual(referenceRoutes[fromIdx][toIdx].TotalDistance, route.TotalDistance, 0.1);
                    }
                }
            }
        }

        /// <summary>
        /// Issue with resolved points with different routing profiles.
        /// https://github.com/OsmSharp/OsmSharp/issues/194
        /// </summary>
        [Test]
        public void RoutingRegressionTest4()
        {
            var interpreter = new OsmRoutingInterpreter();
            var tagsIndex = new TagsIndex();

            // do the data processing.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(memoryData, interpreter, tagsIndex);
            var dataProcessorSource = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression1.osm"));
            var sorter = new OsmStreamFilterSort();
            sorter.RegisterSource(dataProcessorSource);
            targetData.RegisterSource(sorter);
            targetData.Pull();

            var basicRouter = new Dykstra();
            var router = Router.CreateFrom(memoryData, basicRouter, interpreter);

            // resolve the three points in question.
            var point35 = new GeoCoordinate(51.01257, 4.000753);
            var point35ResolvedCar = router.Resolve(Vehicle.Car, point35);
            var point35ResolvedBicycle = router.Resolve(Vehicle.Bicycle, point35);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape.
        /// </summary>
        [Test]
        public void RoutingRegressionTest5()
        {
            double e = 0.1; // 10 cm

            // network:
            //        x(1)------x(2)
            //       /           \
            // x(3)--x(4)---------x(5)--x(6)

            // 1: 50.98508962508, 4.82958530756
            // 2: 50.98509255957, 4.83009340615
            // 3: 50.98496931078, 4.82889075077
            // 4: 50.98496931078, 4.82939884936
            // 5: 50.98496931078, 4.83025189562
            // 6: 50.98496931078, 4.83079728585

            var vertex1 = new GeoCoordinate(50.98508962508, 4.82958530756);
            var vertex2 = new GeoCoordinate(50.98509255957, 4.83009340615);
            var vertex3 = new GeoCoordinate(50.98496931078, 4.82889075077);
            var vertex4 = new GeoCoordinate(50.98496931078, 4.82939884936);
            var vertex5 = new GeoCoordinate(50.98496931078, 4.83025189562);
            var vertex6 = new GeoCoordinate(50.98496931078, 4.83079728585);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression3.osm"));
            var router = Router.CreateFrom(source, new OsmRoutingInterpreter());

            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);

            var route = router.Calculate(Vehicle.Car, resolved3, resolved6);
            var points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex3).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex6).Value, e);

            route = router.Calculate(Vehicle.Car, resolved6, resolved3);
            points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex6).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex3).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape.
        /// </summary>
        [Test]
        public void RoutingRegressionTest5Contracted()
        {
            double e = 0.1; // 10 cm

            // network:
            //        x(1)------x(2)
            //       /           \
            // x(3)--x(4)---------x(5)--x(6)

            // 1: 50.98508962508, 4.82958530756
            // 2: 50.98509255957, 4.83009340615
            // 3: 50.98496931078, 4.82889075077
            // 4: 50.98496931078, 4.82939884936
            // 5: 50.98496931078, 4.83025189562
            // 6: 50.98496931078, 4.83079728585

            var vertex1 = new GeoCoordinate(50.98508962508, 4.82958530756);
            var vertex2 = new GeoCoordinate(50.98509255957, 4.83009340615);
            var vertex3 = new GeoCoordinate(50.98496931078, 4.82889075077);
            var vertex4 = new GeoCoordinate(50.98496931078, 4.82939884936);
            var vertex5 = new GeoCoordinate(50.98496931078, 4.83025189562);
            var vertex6 = new GeoCoordinate(50.98496931078, 4.83079728585);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression3.osm"));
            var router = Router.CreateCHFrom(source, new OsmRoutingInterpreter(), Vehicle.Car);

            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);

            var route = router.Calculate(Vehicle.Car, resolved3, resolved6);
            var points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex3).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex6).Value, e);

            route = router.Calculate(Vehicle.Car, resolved6, resolved3);
            points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex6).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex3).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape but was sorted differently in source.
        /// </summary>
        [Test]
        public void RoutingRegressionTest6()
        {
            double e = 0.1; // 10 cm

            // network:
            //        x(1)------x(2)
            //       /           \
            // x(3)--x(4)---------x(5)--x(6)

            // 1: 50.98508962508, 4.82958530756
            // 2: 50.98509255957, 4.83009340615
            // 3: 50.98496931078, 4.82889075077
            // 4: 50.98496931078, 4.82939884936
            // 5: 50.98496931078, 4.83025189562
            // 6: 50.98496931078, 4.83079728585

            var vertex1 = new GeoCoordinate(50.98508962508, 4.82958530756);
            var vertex2 = new GeoCoordinate(50.98509255957, 4.83009340615);
            var vertex3 = new GeoCoordinate(50.98496931078, 4.82889075077);
            var vertex4 = new GeoCoordinate(50.98496931078, 4.82939884936);
            var vertex5 = new GeoCoordinate(50.98496931078, 4.83025189562);
            var vertex6 = new GeoCoordinate(50.98496931078, 4.83079728585);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression4.osm"));
            var router = Router.CreateFrom(source, new OsmRoutingInterpreter());

            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);

            var route = router.Calculate(Vehicle.Car, resolved3, resolved6);
            var points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex3).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex6).Value, e);

            route = router.Calculate(Vehicle.Car, resolved6, resolved3);
            points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex6).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex3).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape but was sorted differently in source.
        /// </summary>
        [Test]
        public void RoutingRegressionTest6Contracted()
        {
            double e = 0.1; // 10 cm

            // network:
            //        x(1)------x(2)
            //       /           \
            // x(3)--x(4)---------x(5)--x(6)

            // 1: 50.98508962508, 4.82958530756
            // 2: 50.98509255957, 4.83009340615
            // 3: 50.98496931078, 4.82889075077
            // 4: 50.98496931078, 4.82939884936
            // 5: 50.98496931078, 4.83025189562
            // 6: 50.98496931078, 4.83079728585

            var vertex1 = new GeoCoordinate(50.98508962508, 4.82958530756);
            var vertex2 = new GeoCoordinate(50.98509255957, 4.83009340615);
            var vertex3 = new GeoCoordinate(50.98496931078, 4.82889075077);
            var vertex4 = new GeoCoordinate(50.98496931078, 4.82939884936);
            var vertex5 = new GeoCoordinate(50.98496931078, 4.83025189562);
            var vertex6 = new GeoCoordinate(50.98496931078, 4.83079728585);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression4.osm"));
            var router = Router.CreateCHFrom(source, new OsmRoutingInterpreter(), Vehicle.Car);

            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);

            var route = router.Calculate(Vehicle.Car, resolved3, resolved6);
            var points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex3).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex6).Value, e);

            route = router.Calculate(Vehicle.Car, resolved6, resolved3);
            points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(4, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex6).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex5).Value, e);
            Assert.AreEqual(0, points[2].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex3).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape but was sorted differently in source.
        /// </summary>
        [Test]
        public void RoutingRegressionTest7()
        {
            double e = 0.2; // 10 cm

            // network:
            //        x(6)-----------x(4)---x(5)
            //        /              /
            // x(1)--x(2)---------x(3)

            var vertex1 = new GeoCoordinate(50.984988723456084, 4.828170772332617);
            var vertex2 = new GeoCoordinate(50.984987035397620, 4.828346366914291);
            var vertex3 = new GeoCoordinate(50.984985823202960, 4.828728258663133);
            var vertex4 = new GeoCoordinate(50.985013303511700, 4.828792771883094);
            var vertex5 = new GeoCoordinate(50.985012880931120, 4.828949650265730);
            var vertex6 = new GeoCoordinate(50.985017593072016, 4.828410262241598);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression5.osm"));
            var router = Router.CreateFrom(source, new OsmRoutingInterpreter());

            var resolved1 = router.Resolve(Vehicle.Car, vertex1, true);
            Assert.AreEqual(0, resolved1.Location.DistanceReal(vertex1).Value, e);
            var resolved2 = router.Resolve(Vehicle.Car, vertex2, true);
            Assert.AreEqual(0, resolved2.Location.DistanceReal(vertex2).Value, e);
            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            Assert.AreEqual(0, resolved3.Location.DistanceReal(vertex3).Value, e);
            var resolved4 = router.Resolve(Vehicle.Car, vertex4, true);
            Assert.AreEqual(0, resolved4.Location.DistanceReal(vertex4).Value, e);
            var resolved5 = router.Resolve(Vehicle.Car, vertex5, true);
            Assert.AreEqual(0, resolved5.Location.DistanceReal(vertex5).Value, e);

            var route = router.Calculate(Vehicle.Car, resolved1, resolved5);
            var points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(5, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex1).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex2).Value, e);
            Assert.AreEqual(0, System.Math.Min(points[2].DistanceReal(vertex6).Value,points[2].DistanceReal(vertex3).Value), e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[4].DistanceReal(vertex5).Value, e);

            route = router.Calculate(Vehicle.Car, resolved5, resolved1);
            points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(5, points.Count);
            Assert.AreEqual(0, points[4].DistanceReal(vertex1).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex2).Value, e);
            Assert.AreEqual(0, System.Math.Min(points[2].DistanceReal(vertex6).Value, points[2].DistanceReal(vertex3).Value), e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[0].DistanceReal(vertex5).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape but was sorted differently in source.
        /// </summary>
        /// <remarks>Contracted version.</remarks>
        [Test]
        public void RoutingRegressionTest7Contracted()
        {
            double e = 0.2; // 10 cm

            // network:
            //        x(6)-----------x(4)---x(5)
            //        /              /
            // x(1)--x(2)---------x(3)

            var vertex1 = new GeoCoordinate(50.984988723456084, 4.828170772332617);
            var vertex2 = new GeoCoordinate(50.984987035397620, 4.828346366914291);
            var vertex3 = new GeoCoordinate(50.984985823202960, 4.828728258663133);
            var vertex4 = new GeoCoordinate(50.985013303511700, 4.828792771883094);
            var vertex5 = new GeoCoordinate(50.985012880931120, 4.828949650265730);
            var vertex6 = new GeoCoordinate(50.985017593072016, 4.828410262241598);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression5.osm"));
            var router = Router.CreateCHFrom(source, new OsmRoutingInterpreter(), Vehicle.Car);

            var resolved1 = router.Resolve(Vehicle.Car, vertex1, true);
            Assert.AreEqual(0, resolved1.Location.DistanceReal(vertex1).Value, e);
            var resolved2 = router.Resolve(Vehicle.Car, vertex2, true);
            Assert.AreEqual(0, resolved2.Location.DistanceReal(vertex2).Value, e);
            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            Assert.AreEqual(0, resolved3.Location.DistanceReal(vertex3).Value, e);
            var resolved4 = router.Resolve(Vehicle.Car, vertex4, true);
            Assert.AreEqual(0, resolved4.Location.DistanceReal(vertex4).Value, e);
            var resolved5 = router.Resolve(Vehicle.Car, vertex5, true);
            Assert.AreEqual(0, resolved5.Location.DistanceReal(vertex5).Value, e);

            var route = router.Calculate(Vehicle.Car, resolved1, resolved5);
            var points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(5, points.Count);
            Assert.AreEqual(0, points[0].DistanceReal(vertex1).Value, e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex2).Value, e);
            Assert.AreEqual(0, System.Math.Min(points[2].DistanceReal(vertex6).Value, points[2].DistanceReal(vertex3).Value), e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[4].DistanceReal(vertex5).Value, e);

            route = router.Calculate(Vehicle.Car, resolved5, resolved1);
            points = new List<GeoCoordinate>(route.GetPoints());

            Assert.AreEqual(5, points.Count);
            Assert.AreEqual(0, points[4].DistanceReal(vertex1).Value, e);
            Assert.AreEqual(0, points[3].DistanceReal(vertex2).Value, e);
            Assert.AreEqual(0, System.Math.Min(points[2].DistanceReal(vertex6).Value, points[2].DistanceReal(vertex3).Value), e);
            Assert.AreEqual(0, points[1].DistanceReal(vertex4).Value, e);
            Assert.AreEqual(0, points[0].DistanceReal(vertex5).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape but was sorted differently in source.
        /// </summary>
        [Test]
        public void RoutingRegressionTest8()
        {
            double e = 0.2; // 10 cm

            // network:
            //            x(6)--------x(8)-x(4)---x(5)
            //           /                /
            // x(1)--x(2)-x(7)--------x(3)

            var vertex1 = new GeoCoordinate(50.984988723456084, 4.828170772332617);
            var vertex2 = new GeoCoordinate(50.984987035397620, 4.828346366914291);
            var vertex3 = new GeoCoordinate(50.984985823202960, 4.828728258663133);
            var vertex4 = new GeoCoordinate(50.985013303511700, 4.828792771883094);
            var vertex5 = new GeoCoordinate(50.985012880931120, 4.828949650265730);
            var vertex6 = new GeoCoordinate(50.985017593072016, 4.828410262241598);
            var vertex7 = new GeoCoordinate(50.984986416884716, 4.828409529435097);
            var vertex8 = new GeoCoordinate(50.985013738270930 ,4.828723851247985);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression6.osm"));
            var router = Router.CreateFrom(source, new OsmRoutingInterpreter());

            var resolved1 = router.Resolve(Vehicle.Car, vertex1, true);
            Assert.AreEqual(0, resolved1.Location.DistanceReal(vertex1).Value, e);
            var resolved2 = router.Resolve(Vehicle.Car, vertex2, true);
            Assert.AreEqual(0, resolved2.Location.DistanceReal(vertex2).Value, e);
            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            Assert.AreEqual(0, resolved3.Location.DistanceReal(vertex3).Value, e);
            var resolved4 = router.Resolve(Vehicle.Car, vertex4, true);
            Assert.AreEqual(0, resolved4.Location.DistanceReal(vertex4).Value, e);
            var resolved5 = router.Resolve(Vehicle.Car, vertex5, true);
            Assert.AreEqual(0, resolved5.Location.DistanceReal(vertex5).Value, e);
            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);
            Assert.AreEqual(0, resolved6.Location.DistanceReal(vertex6).Value, e);
            var resolved7 = router.Resolve(Vehicle.Car, vertex7, true);
            Assert.AreEqual(0, resolved7.Location.DistanceReal(vertex7).Value, e);
            var resolved8 = router.Resolve(Vehicle.Car, vertex8, true);
            Assert.AreEqual(0, resolved8.Location.DistanceReal(vertex8).Value, e);
        }

        /// <summary>
        /// Issue with multiple edges between the same two vertices and with a different shape but was sorted differently in source.
        /// </summary>
        [Test]
        public void RoutingRegressionTest8Contracted()
        {
            double e = 0.2; // 10 cm

            // network: (a/b) a=vertexX, b=contractedVertexId
            //                x(6/5)--------x(8/x)-x(4/2)---x(5/3)
            //               /                    /
            // x(1/4)--x(2/1)-x(7/6)--------x(3/x)

            // x(7/5) is one of the shapepoints converted to an actual vertex to prevent two edges 1<->2.
            // this regression test has everything to do with x(7/5).

            var vertex1 = new GeoCoordinate(50.984988723456084, 4.828170772332617);
            var vertex2 = new GeoCoordinate(50.984987035397620, 4.828346366914291);
            var vertex3 = new GeoCoordinate(50.984985823202960, 4.828728258663133);
            var vertex4 = new GeoCoordinate(50.985013303511700, 4.828792771883094);
            var vertex5 = new GeoCoordinate(50.985012880931120, 4.828949650265730);
            var vertex6 = new GeoCoordinate(50.985017593072016, 4.828410262241598);
            var vertex7 = new GeoCoordinate(50.984986416884716, 4.828409529435097);
            var vertex8 = new GeoCoordinate(50.985013738270930, 4.828723851247985);

            var source = new XmlOsmStreamSource(
                Assembly.GetExecutingAssembly().GetManifestResourceStream("OsmSharp.Test.Unittests.test_routing_regression6.osm"));
            var router = Router.CreateCHFrom(source, new OsmRoutingInterpreter(), Vehicle.Car);

            var resolved1 = router.Resolve(Vehicle.Car, vertex1, true);
            Assert.AreEqual(0, resolved1.Location.DistanceReal(vertex1).Value, e);
            var resolved2 = router.Resolve(Vehicle.Car, vertex2, true);
            Assert.AreEqual(0, resolved2.Location.DistanceReal(vertex2).Value, e);
            var resolved3 = router.Resolve(Vehicle.Car, vertex3, true);
            Assert.AreEqual(0, resolved3.Location.DistanceReal(vertex3).Value, e);
            var resolved4 = router.Resolve(Vehicle.Car, vertex4, true);
            Assert.AreEqual(0, resolved4.Location.DistanceReal(vertex4).Value, e);
            var resolved5 = router.Resolve(Vehicle.Car, vertex5, true);
            Assert.AreEqual(0, resolved5.Location.DistanceReal(vertex5).Value, e);
            var resolved6 = router.Resolve(Vehicle.Car, vertex6, true);
            Assert.AreEqual(0, resolved6.Location.DistanceReal(vertex6).Value, e);
            var resolved7 = router.Resolve(Vehicle.Car, vertex7, true);
            Assert.AreEqual(0, resolved7.Location.DistanceReal(vertex7).Value, e);
            var resolved8 = router.Resolve(Vehicle.Car, vertex8, true);
            Assert.AreEqual(0, resolved8.Location.DistanceReal(vertex8).Value, e);
        }

        /// <summary>
        /// Issue with resolving and search for close edges and oneway, directed edges.
        /// </summary>
        [Test]
        public void RoutingRegressionTest9ResolvingReverse()
        {
            // build a graph to encode from.
            var tags = new TagsIndex();
            var graphDataSource = new RouterDataSource<Edge>(new Graph<Edge>(), tags);
            var vertex1 = graphDataSource.AddVertex(51.05849821468899f, 3.7240000000000000f);
            var vertex2 = graphDataSource.AddVertex(51.05849821468899f, 3.7254400000000000f);
            var vertex3 = graphDataSource.AddVertex(51.05849821468899f, 3.7225627899169926f);
            var edge = new Edge() // all edges are identical.
            {
                Distance = 100,
                Forward = true,
                Tags = tags.Add(new TagsCollection(
                    Tag.Create("highway", "tertiary"),
                    Tag.Create("oneway", "yes")))
            };
            graphDataSource.AddEdge(vertex1, vertex2, edge, null);
            graphDataSource.AddEdge(vertex3, vertex1, edge, null);

            // {RectF:[(3,71326552867889,51,048498214689),(3,73326552867889,51,068498214689)]}
            var edges =  graphDataSource.GetEdges(new GeoCoordinateBox(
                new GeoCoordinate(51.068498214689, 3.73326552867889),
                new GeoCoordinate(51.048498214689, 3.71326552867889)));

            while(edges.MoveNext())
            {
                if(edges.Vertex1 == 1 &&
                    edges.Vertex2 == 2)
                {
                    Assert.IsTrue(edges.EdgeData.Forward);
                }
                else if(edges.Vertex1 == 2 &&
                    edges.Vertex2 == 1)
                {
                    Assert.IsFalse(edges.EdgeData.Forward);
                }
                if (edges.Vertex1 == 1 &&
                    edges.Vertex2 == 3)
                {
                    Assert.IsFalse(edges.EdgeData.Forward);
                }
                else if (edges.Vertex1 == 3 &&
                    edges.Vertex2 == 1)
                {
                    Assert.IsTrue(edges.EdgeData.Forward);
                }
            }
        }

        /// <summary>
        /// Issue with resolving and search for close edges and oneway, directed edges and their shapes.
        /// </summary>
        [Test]
        public void RoutingRegressionTest10ResolvingReverse()
        {
            // build a graph to encode from.
            var tags = new TagsIndex();
            var graphDataSource = new RouterDataSource<Edge>(new Graph<Edge>(), tags);
            var vertex1 = graphDataSource.AddVertex(51.05849821468899f, 3.7240000000000000f);
            var vertex2 = graphDataSource.AddVertex(51.05849821468899f, 3.7254400000000000f);
            var vertex3 = graphDataSource.AddVertex(51.05849821468899f, 3.7225627899169926f);
            var edge = new Edge() // all edges are identical.
            {
                Distance = 100,
                Forward = true,
                Tags = tags.Add(new TagsCollection(
                    Tag.Create("highway", "tertiary"),
                    Tag.Create("oneway", "yes")))
            };
            var shape1 = new CoordinateArrayCollection<OsmSharp.Math.Geo.Simple.GeoCoordinateSimple>(
                new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple[] {
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 1,
                        Longitude = 1
                    },
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 2,
                        Longitude = 2
                    }
                });
            var shape2 = new CoordinateArrayCollection<OsmSharp.Math.Geo.Simple.GeoCoordinateSimple>(
                new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple[] {
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 3,
                        Longitude = 3
                    },
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 4,
                        Longitude = 4
                    }
                });
            graphDataSource.AddEdge(vertex1, vertex2, edge, shape1);
            graphDataSource.AddEdge(vertex3, vertex1, edge, shape2);

            // {RectF:[(3,71326552867889,51,048498214689),(3,73326552867889,51,068498214689)]}
            var edges =  graphDataSource.GetEdges(new GeoCoordinateBox(
                new GeoCoordinate(51.068498214689, 3.73326552867889),
                new GeoCoordinate(51.048498214689, 3.71326552867889)));

            while (edges.MoveNext())
            {
                if (edges.Vertex1 == 1 &&
                    edges.Vertex2 == 2)
                {
                    Assert.IsTrue(edges.EdgeData.Forward);
                    var shapes = edges.Intermediates.ToSimpleArray();
                    Assert.AreEqual(2, shapes.Length);
                    Assert.AreEqual(1, shapes[0].Latitude);
                    Assert.AreEqual(1, shapes[0].Longitude);
                    Assert.AreEqual(2, shapes[1].Latitude);
                    Assert.AreEqual(2, shapes[1].Longitude);
                }
                else if (edges.Vertex1 == 2 &&
                    edges.Vertex2 == 1)
                {
                    Assert.IsFalse(edges.EdgeData.Forward);
                    var shapes = edges.Intermediates.ToSimpleArray();
                    Assert.AreEqual(2, shapes.Length);
                    Assert.AreEqual(2, shapes[0].Latitude);
                    Assert.AreEqual(2, shapes[0].Longitude);
                    Assert.AreEqual(1, shapes[1].Latitude);
                    Assert.AreEqual(1, shapes[1].Longitude);
                }
                if (edges.Vertex1 == 1 &&
                    edges.Vertex2 == 3)
                {
                    Assert.IsFalse(edges.EdgeData.Forward);
                    var shapes = edges.Intermediates.ToSimpleArray();
                    Assert.AreEqual(2, shapes.Length);
                    Assert.AreEqual(4, shapes[0].Latitude);
                    Assert.AreEqual(4, shapes[0].Longitude);
                    Assert.AreEqual(3, shapes[1].Latitude);
                    Assert.AreEqual(3, shapes[1].Longitude);
                }
                else if (edges.Vertex1 == 3 &&
                    edges.Vertex2 == 1)
                {
                    Assert.IsTrue(edges.EdgeData.Forward);
                    var shapes = edges.Intermediates.ToSimpleArray();
                    Assert.AreEqual(2, shapes.Length);
                    Assert.AreEqual(3, shapes[0].Latitude);
                    Assert.AreEqual(3, shapes[0].Longitude);
                    Assert.AreEqual(4, shapes[1].Latitude);
                    Assert.AreEqual(4, shapes[1].Longitude);
                }
            }
        }

        /// <summary>
        /// Issue with returning the correct edges.
        /// </summary>
        [Test]
        public void RoutingRegressionTest11ResolvingReverse()
        {
            // build a graph to encode from.
            var tags = new TagsIndex();
            var graphDataSource = new RouterDataSource<Edge>(new Graph<Edge>(), tags);
            var vertex1 = graphDataSource.AddVertex(51.05849821468899f, 3.7240000000000000f);
            var vertex2 = graphDataSource.AddVertex(51.05849821468899f, 3.7254400000000000f);
            var vertex3 = graphDataSource.AddVertex(51.05849821468899f, 3.7225627899169926f);
            var edgeData = new Edge() // all edges are identical.
            {
                Distance = 100,
                Forward = true,
                Tags = tags.Add(new TagsCollection(
                    Tag.Create("highway", "tertiary"),
                    Tag.Create("oneway", "yes")))
            };
            var shape1 = new CoordinateArrayCollection<OsmSharp.Math.Geo.Simple.GeoCoordinateSimple>(
                new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple[] {
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 1,
                        Longitude = 1
                    },
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 2,
                        Longitude = 2
                    }
                });
            var shape2 = new CoordinateArrayCollection<OsmSharp.Math.Geo.Simple.GeoCoordinateSimple>(
                new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple[] {
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 3,
                        Longitude = 3
                    },
                    new OsmSharp.Math.Geo.Simple.GeoCoordinateSimple()
                    {
                        Latitude = 4,
                        Longitude = 4
                    }
                });
            graphDataSource.AddEdge(vertex1, vertex2, edgeData, shape1);
            graphDataSource.AddEdge(vertex3, vertex1, edgeData, shape2);

            var edges =  new List<Edge<Edge>>(graphDataSource.GetEdges(1));
            Assert.AreEqual(2, edges.Count);
            foreach(var edge in edges)
            {
                if (edge.Neighbour == 2)
                {
                    Assert.AreEqual(true, edge.EdgeData.Forward);
                }
                else if(edge.Neighbour == 3)
                {
                    Assert.AreEqual(false, edge.EdgeData.Forward);
                }
            }

            edges = new List<Edge<Edge>>(graphDataSource.GetEdges(2));
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual(false, edges[0].EdgeData.Forward);

            edges = new List<Edge<Edge>>(graphDataSource.GetEdges(3));
            Assert.AreEqual(1, edges.Count);
            Assert.AreEqual(true, edges[0].EdgeData.Forward);
        }
    }
}