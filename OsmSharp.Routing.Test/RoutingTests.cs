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

using System.Collections.Generic;
using NUnit.Framework;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Primitives;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;
using System;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Base class with tests around the Router object.
    /// </summary>
    public abstract class SimpleRoutingTests<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Builds the router.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="interpreter"></param>
        /// <param name="basicRouter"></param>
        /// <returns></returns>
        public abstract Router BuildRouter(IRoutingAlgorithmData<TEdgeData> data,
            IRoutingInterpreter interpreter, IRoutingAlgorithm<TEdgeData> basicRouter);

        /// <summary>
        /// Builds the basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public abstract IRoutingAlgorithm<TEdgeData> BuildBasicRouter(IRoutingAlgorithmData<TEdgeData> data);

        /// <summary>
        /// Builds the data.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="embeddedString"></param>
        /// <returns></returns>
        public abstract IRoutingAlgorithmData<TEdgeData> BuildData(IOsmRoutingInterpreter interpreter, 
            string embeddedString);

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortestDefault()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);
            var source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            var target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));

            var route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Segments.Length);

            // float latitude, longitude;
            // data.GetVertex(20, out latitude, out longitude);
            Assert.AreEqual(51.0578537, route.Segments[0].Latitude, 0.00001);
            Assert.AreEqual(3.71922278, route.Segments[0].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Start, route.Segments[0].Type);

            // data.GetVertex(21, out latitude, out longitude);
            Assert.AreEqual(51.0578537, route.Segments[1].Latitude, 0.00001);
            Assert.AreEqual(3.71956539, route.Segments[1].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[1].Type);

            // data.GetVertex(16, out latitude, out longitude);
            Assert.AreEqual(51.05773, route.Segments[2].Latitude, 0.00001);
            Assert.AreEqual(3.719745, route.Segments[2].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[2].Type);

            // data.GetVertex(22, out latitude, out longitude);
            Assert.AreEqual(51.05762, route.Segments[3].Latitude, 0.00001);
            Assert.AreEqual(3.71965814, route.Segments[3].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[3].Type);

            // data.GetVertex(23, out latitude, out longitude);
            Assert.AreEqual(51.05762, route.Segments[4].Latitude, 0.00001);
            Assert.AreEqual(3.71918, route.Segments[4].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Stop, route.Segments[4].Type);
        }

        /// <summary>
        /// Tests that a router preserves tags given to resolved points.
        /// </summary>
        protected void DoTestResolvedTags()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(
                data, interpreter, basicRouter);
            var source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            source.Tags.Add(new KeyValuePair<string, string>("name", "source"));
            var target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            target.Tags.Add(new KeyValuePair<string, string>("name", "target"));

            var route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Segments.Length);

            // float latitude, longitude;
            // data.GetVertex(20, out latitude, out longitude);
            Assert.AreEqual(51.0578537, route.Segments[0].Latitude, 0.00001);
            Assert.AreEqual(3.71922278, route.Segments[0].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Start, route.Segments[0].Type);
            Assert.IsNotNull(route.Segments[0].Points[0].Tags);
            Assert.AreEqual(1, route.Segments[0].Points[0].Tags.Length);
            Assert.AreEqual("source", route.Segments[0].Points[0].Tags[0].Value);

            // data.GetVertex(23, out latitude, out longitude);
            Assert.AreEqual(51.05762, route.Segments[4].Latitude, 0.00001);
            Assert.AreEqual(3.71918, route.Segments[4].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Stop, route.Segments[4].Type);
            Assert.IsNotNull(route.Segments[4].Points[0].Tags);
            Assert.AreEqual(1, route.Segments[4].Points[0].Tags.Length);
            Assert.AreEqual("target", route.Segments[4].Points[0].Tags[0].Value);
        }

        /// <summary>
        /// Tests that a router preserves tags that are located on ways/arcs in the route.
        /// </summary>
        protected void DoTestEdgeTags()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);
            var source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            source.Tags.Add(new KeyValuePair<string, string>("name", "source"));
            var target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            target.Tags.Add(new KeyValuePair<string, string>("name", "target"));

            var route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Segments.Length);

            Assert.AreEqual("highway", route.Segments[1].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[1].Tags[0].Value);

            Assert.AreEqual("highway", route.Segments[2].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[2].Tags[0].Value);

            Assert.AreEqual("highway", route.Segments[3].Tags[0].Key);
            Assert.AreEqual("residential", route.Segments[3].Tags[0].Value);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(
                data, interpreter, basicRouter);
            var source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            var target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));

            var route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Segments.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest2()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Segments.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest3()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Segments.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest4()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0579235, 3.7199811));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Segments.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortest5()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basic_router = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basic_router);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581001, 3.7200612));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(7, route.Segments.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortestResolved1()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578153, 3.7193937));
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0582408, 3.7194636));

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(10, route.Segments.Length);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        protected void DoTestShortestResolved2()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581843, 3.7201209)); // between 2 - 3
            RouterPoint target = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581484, 3.7194957)); // between 9 - 8

            Route route = router.Calculate(Vehicle.Car, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(5, route.Segments.Length);
        }

        /// <summary>
        /// Tests if the many-to-many weights are the same as the point-to-point weights.
        /// </summary>
        protected void DoTestManyToMany1()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(
                data, interpreter, basicRouter);

            var resolvedPoints = new RouterPoint[3];
            resolvedPoints[0] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            resolvedPoints[1] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            resolvedPoints[2] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581001, 3.7200612));

            var weights = router.CalculateManyToManyWeight(Vehicle.Car, resolvedPoints, resolvedPoints);

            for (int x = 0; x < weights.Length; x++)
            {
                for (int y = 0; y < weights.Length; y++)
                {
                    var manyToMany = weights[x][y];
                    var pointToPoint = router.CalculateWeight(Vehicle.Car, resolvedPoints[x], resolvedPoints[y]);

                    Assert.AreEqual(pointToPoint, manyToMany);
                }
            }
        }

        /// <summary>
        /// Test if the connectivity test succeed/fail.
        /// </summary>
        protected void DoTestConnectivity1()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            var resolvedPoints = new RouterPoint[3];
            resolvedPoints[0] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578532, 3.7192229));
            resolvedPoints[1] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            resolvedPoints[2] = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0581001, 3.7200612));

            // test connectivity succes.
            Assert.IsTrue(router.CheckConnectivity(Vehicle.Car, resolvedPoints[0], 5));
            //Assert.IsTrue(router.CheckConnectivity(VehicleEnum.Car, resolved_points[1], 5));
            Assert.IsTrue(router.CheckConnectivity(Vehicle.Car, resolvedPoints[2], 5));

            // test connectivity failiure.
            Assert.IsFalse(router.CheckConnectivity(Vehicle.Car, resolvedPoints[0], 1000));
            Assert.IsFalse(router.CheckConnectivity(Vehicle.Car, resolvedPoints[1], 1000));
            Assert.IsFalse(router.CheckConnectivity(Vehicle.Car, resolvedPoints[2], 1000));
        }

        /// <summary>
        /// Test if the resolving of nodes returns those same nodes.
        /// 
        /// (does not work on a lazy loading data source!)
        /// </summary>
        protected void DoTestResolveAllNodes()
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            for (int idx = 1; idx < data.VertexCount; idx++)
            {
                float latitude, longitude;
                if (data.GetVertex((uint)idx, out latitude, out longitude))
                {
                    var point = router.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude));
                    Assert.AreEqual(idx, (point as RouterPoint).Id);
                }
            }
        }

        /// <summary>
        /// Test if routes from a resolved node to itself is correctly calculated.
        /// 
        /// Regression Test: Routing to self with a resolved node returns a route to the nearest real node and back.
        /// </summary>
        protected void DoTestResolveBetweenRouteToSelf()
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);
            
            // first test a non-between node.
            var resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576193, 3.7191801));
            var route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Segments.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);

            resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0578761, 3.7193972)); //,-103,  -4,  -8
            route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Segments.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);


            resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576510, 3.7194124)); //,-104, -14, -12
            route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Segments.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);

            resolved = router.Resolve(Vehicle.Car, new GeoCoordinate(51.0576829, 3.7196791)); //,-105, -12, -10
            route = router.Calculate(Vehicle.Car, resolved, resolved);
            Assert.AreEqual(1, route.Segments.Length);
            Assert.AreEqual(0, route.TotalDistance);
            Assert.AreEqual(0, route.TotalTime);
        }

        /// <summary>
        /// Test if routes between two resolved nodes are correctly calculated.
        /// </summary>
        protected void DoTestResolveBetweenClose()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);
            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            for (double position1 = 0.1; position1 < 0.91; position1 = position1 + 0.1)
            {
                var point = vertex20 + ((vertex21 - vertex20) * position1);
                var vertex2021 = new GeoCoordinate(point[1], point[0]);
                for (double position2 = 0.1; position2 < 0.91; position2 = position2 + 0.1)
                {
                    point = vertex21 + ((vertex16 - vertex21) * position2);
                    var vertex2116 = new GeoCoordinate(point[1], point[0]);

                    // calculate route.
                    var basicRouter = this.BuildBasicRouter(data);
                    var router = this.BuildRouter(data, interpreter, basicRouter);

                    var route = router.Calculate(Vehicle.Car, 
                        router.Resolve(Vehicle.Car, vertex2021),
                        router.Resolve(Vehicle.Car, vertex2116));

                    Assert.AreEqual(3, route.Segments.Length);
                    Assert.AreEqual(vertex2021.Latitude, route.Segments[0].Latitude, 0.0001);
                    Assert.AreEqual(vertex2021.Longitude, route.Segments[0].Longitude, 0.0001);

                    Assert.AreEqual(vertex21.Latitude, route.Segments[1].Latitude, 0.0001);
                    Assert.AreEqual(vertex21.Longitude, route.Segments[1].Longitude, 0.0001);

                    Assert.AreEqual(vertex2116.Latitude, route.Segments[2].Latitude, 0.0001);
                    Assert.AreEqual(vertex2116.Longitude, route.Segments[2].Longitude, 0.0001);
                }
            }
        }

        /// <summary>
        /// Test if routes between two resolved nodes are correctly calculated.
        /// </summary>
        protected void DoTestResolveBetweenTwo()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);

            for (double position1 = 0.1; position1 < 0.91; position1 = position1 + 0.1)
            {
                PointF2D point = vertex20 + ((vertex21 - vertex20) * position1);
                var vertex2021 = new GeoCoordinate(point[1], point[0]);

                point = vertex21 + ((vertex20 - vertex21) * position1);
                var vertex2120 = new GeoCoordinate(point[1], point[0]);

                // calculate route.
                IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
                Router router = this.BuildRouter(data, interpreter, basicRouter);

                Route route = router.Calculate(Vehicle.Car,
                    router.Resolve(Vehicle.Car, vertex2021),
                    router.Resolve(Vehicle.Car, vertex2120));

                if (vertex2021.Latitude != vertex2120.Latitude &&
                    vertex2021.Longitude != vertex2120.Longitude)
                {
                    Assert.AreEqual(2, route.Segments.Length);
                    Assert.AreEqual(vertex2021.Latitude, route.Segments[0].Latitude, 0.0001);
                    Assert.AreEqual(vertex2021.Longitude, route.Segments[0].Longitude, 0.0001);

                    Assert.AreEqual(vertex2120.Latitude, route.Segments[1].Latitude, 0.0001);
                    Assert.AreEqual(vertex2120.Longitude, route.Segments[1].Longitude, 0.0001);
                }
            }
        }

        /// <summary>
        /// Test if routes between resolved nodes are correctly calculated.
        /// 
        /// 20----x----21----x----16
        /// </summary>
        protected void DoTestResolveCase1()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<TEdgeData> data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);
            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            PointF2D point = vertex20 + ((vertex21 - vertex20) * 0.5);
            var vertex2021 = new GeoCoordinate(point[1], point[0]);

            point = vertex21 + ((vertex16 - vertex21) * 0.5);
            var vertex2116 = new GeoCoordinate(point[1], point[0]);

            // calculate route.
            IRoutingAlgorithm<TEdgeData> basicRouter = this.BuildBasicRouter(data);
            Router router = this.BuildRouter(data, interpreter, basicRouter);

            Route route = router.Calculate(Vehicle.Car,
                router.Resolve(Vehicle.Car, vertex2021),
                router.Resolve(Vehicle.Car, vertex2116));

            Assert.AreEqual(3, route.Segments.Length);
            Assert.AreEqual(vertex2021.Latitude, route.Segments[0].Latitude, 0.0001);
            Assert.AreEqual(vertex2021.Longitude, route.Segments[0].Longitude, 0.0001);

            Assert.AreEqual(vertex21.Latitude, route.Segments[1].Latitude, 0.0001);
            Assert.AreEqual(vertex21.Longitude, route.Segments[1].Longitude, 0.0001);

            Assert.AreEqual(vertex2116.Latitude, route.Segments[2].Latitude, 0.0001);
            Assert.AreEqual(vertex2116.Longitude, route.Segments[2].Longitude, 0.0001);
        }

        /// <summary>
        /// Test if routes between resolved nodes are correctly calculated.
        /// 
        /// 20--x---x--21---------16
        /// </summary>
        protected void DoTestResolveCase2()
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");

            var vertex20 = new GeoCoordinate(51.0578532, 3.7192229);
            var vertex21 = new GeoCoordinate(51.0578518, 3.7195654);
//            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            var point = vertex20 + ((vertex21 - vertex20) * 0.25);
            var vertex20211 = new GeoCoordinate(point[1], point[0]);

            point = vertex20 + ((vertex21 - vertex20) * 0.75);
            var vertex20212 = new GeoCoordinate(point[1], point[0]);

            // calculate route.
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            var vertex20211Resolved = router.Resolve(Vehicle.Car, vertex20211);
            var vertex20212Resolved = router.Resolve(Vehicle.Car, vertex20212);
            var route = router.Calculate(Vehicle.Car, vertex20211Resolved, vertex20212Resolved);

            Assert.AreEqual(2, route.Segments.Length);
            Assert.AreEqual(vertex20211.Latitude, route.Segments[0].Latitude, 0.0001);
            Assert.AreEqual(vertex20211.Longitude, route.Segments[0].Longitude, 0.0001);

            Assert.AreEqual(vertex20212.Latitude, route.Segments[1].Latitude, 0.0001);
            Assert.AreEqual(vertex20212.Longitude, route.Segments[1].Longitude, 0.0001);
        }

        /// <summary>
        /// Resolves coordinates at the same locations and checks tag preservation.
        /// </summary>
        protected void DoTestResolveSameLocation()
        {
            var vertex16 = new GeoCoordinate(51.0577299, 3.719745);

            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, "OsmSharp.Routing.Test.data.test_network.osm");

            // create router.
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            // define test tags.
            var tags1 = new Dictionary<string, string>();
            tags1.Add("test1", "yes");
            var tags2 = new Dictionary<string, string>();
            tags2.Add("test2", "yes");

            // resolve points.
            var point1 = router.Resolve(Vehicle.Car, vertex16);
            point1.Tags.Add(new KeyValuePair<string, string>("test1","yes"));

            // test presence of tags.
            Assert.AreEqual(1, point1.Tags.Count);
            Assert.AreEqual("test1", point1.Tags[0].Key);
            Assert.AreEqual("yes", point1.Tags[0].Value);

            // resolve point again.
            RouterPoint point2 = router.Resolve(Vehicle.Car, vertex16);

            // the tags should be here still!
            Assert.AreEqual(1, point2.Tags.Count);
            Assert.AreEqual("test1", point2.Tags[0].Key);
            Assert.AreEqual("yes", point2.Tags[0].Value);
        }

        /// <summary>
        /// Tests many-to-many routing.
        /// </summary>
        protected void DoTestManyToMany(string filename)
        {
            // initialize data.
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, string.Format("OsmSharp.Routing.Test.data.{0}", filename));

            // create router.
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            var resolved = new RouterPoint[data.VertexCount - 1];
            for (uint idx = 1; idx < data.VertexCount; idx++)
            { // resolve each vertex.
                float latitude, longitude;
                if (data.GetVertex(idx, out latitude, out longitude))
                {
                    resolved[idx - 1] = router.Resolve(Vehicle.Car, new GeoCoordinate(latitude, longitude).OffsetRandom(20), true);
                }

                // reference and resolved have to exist.
                Assert.IsNotNull(resolved[idx - 1]);
            }

            // limit tests to a fixed number.
            int pointSize = 100;
            int testEveryOther = resolved.Length / pointSize;
            testEveryOther = System.Math.Max(testEveryOther, 1);

            // check all the routes having the same weight(s).
            var points = new List<RouterPoint>();
            for (int idx = 0; idx < resolved.Length; idx++)
            {
                int testNumber = idx;
                if (testNumber % testEveryOther == 0)
                {
                    points.Add(resolved[idx]);
                }
            }

            // calculate many-to-many weights.
            var weights = router.CalculateManyToManyWeight(Vehicle.Car, points.ToArray(), points.ToArray());
            for(int fromIdx = 0; fromIdx < points.Count; fromIdx++)
            {
                for (int toIdx = 0; toIdx < points.Count; toIdx++)
                {
                    var weight = router.CalculateWeight(Vehicle.Car, points[fromIdx], points[toIdx]);
                    Assert.AreEqual(weight, weights[fromIdx][toIdx]);
                }
            }
        }

        /// <summary>
        /// Tests argument checks on router.
        /// </summary>
        protected void DoTestArgumentChecks(string filename)
        {
            var interpreter = new OsmRoutingInterpreter();
            var data = this.BuildData(interpreter, string.Format("OsmSharp.Routing.Test.data.{0}", filename));
            var basicRouter = this.BuildBasicRouter(data);
            var router = this.BuildRouter(data, interpreter, basicRouter);

            var anyCoordinate = new GeoCoordinate(0, 0);
            var anyRouterPoint = new RouterPoint(-1, Vehicle.Car, anyCoordinate);
            var anyRouterPointArray = new RouterPoint[] { anyRouterPoint };
            var anyVehicle = Vehicle.Car;

            Assert.Throws<ArgumentNullException>(() => router.Calculate(null, anyRouterPoint, anyRouterPoint));
            Assert.Throws<ArgumentNullException>(() => router.Calculate(anyVehicle, null, anyRouterPoint));
            Assert.Throws<ArgumentNullException>(() => router.Calculate(anyVehicle, anyRouterPoint, null));

            Assert.Throws<ArgumentNullException>(() => router.CalculateWeight(null, anyRouterPoint, anyRouterPoint));
            Assert.Throws<ArgumentNullException>(() => router.CalculateWeight(anyVehicle, null, anyRouterPoint));
            Assert.Throws<ArgumentNullException>(() => router.CalculateWeight(anyVehicle, anyRouterPoint, null));

            Assert.Throws<ArgumentNullException>(() => router.Calculate(null, anyRouterPoint, anyRouterPoint, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.Calculate(anyVehicle, null, anyRouterPoint, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.Calculate(anyVehicle, anyRouterPoint, null, float.MaxValue, false));

            Assert.Throws<ArgumentNullException>(() => router.CalculateToClosest(null, anyRouterPoint, anyRouterPointArray));
            Assert.Throws<ArgumentNullException>(() => router.CalculateToClosest(anyVehicle, null, anyRouterPointArray));
            Assert.Throws<ArgumentNullException>(() => router.CalculateToClosest(anyVehicle, anyRouterPoint, null));

            Assert.Throws<ArgumentNullException>(() => router.CalculateToClosest(null, anyRouterPoint, anyRouterPointArray,float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.CalculateToClosest(anyVehicle, null, anyRouterPointArray, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.CalculateToClosest(anyVehicle, anyRouterPoint, null, float.MaxValue, false));

            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToMany(null, anyRouterPoint, anyRouterPointArray, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToMany(anyVehicle, null, anyRouterPointArray, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToMany(anyVehicle, anyRouterPoint, null, float.MaxValue, false));

            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToManyWeight(null, anyRouterPoint, anyRouterPointArray));
            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToManyWeight(anyVehicle, null, anyRouterPointArray));
            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToManyWeight(anyVehicle, anyRouterPoint, null));
            Assert.Throws<ArgumentNullException>(() => router.CalculateOneToManyWeight(anyVehicle, anyRouterPoint, anyRouterPointArray, null));

            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToMany(null, anyRouterPointArray, anyRouterPointArray, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToMany(anyVehicle, null, anyRouterPointArray, float.MaxValue, false));
            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToMany(anyVehicle, anyRouterPointArray, null, float.MaxValue, false));

            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToManyWeight(null, anyRouterPointArray, anyRouterPointArray));
            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToManyWeight(anyVehicle, null, anyRouterPointArray));
            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToManyWeight(anyVehicle, anyRouterPointArray, null));
            Assert.Throws<ArgumentNullException>(() => router.CalculateManyToManyWeight(anyVehicle, anyRouterPointArray, anyRouterPointArray, null));

            Assert.Throws<ArgumentNullException>(() => router.CalculateRange(null, anyRouterPoint, 100));
            Assert.Throws<ArgumentNullException>(() => router.CalculateRange(anyVehicle, null, 100));

            Assert.Throws<ArgumentNullException>(() => router.CheckConnectivity(null, anyRouterPoint, 100));
            Assert.Throws<ArgumentNullException>(() => router.CheckConnectivity(anyVehicle, (RouterPoint)null, 100));

            Assert.Throws<ArgumentNullException>(() => router.CheckConnectivity(null, anyRouterPointArray, 100));
            Assert.Throws<ArgumentNullException>(() => router.CheckConnectivity(anyVehicle, (RouterPoint[])null, 100));

            Assert.Throws<ArgumentNullException>(() => router.Resolve(null, anyCoordinate));
            Assert.Throws<ArgumentNullException>(() => router.Resolve(anyVehicle, (GeoCoordinate)null));
        }
    }
}