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
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Base class with tests around IRouter objects.
    /// </summary>
    public abstract class RoutingConditionalAccessTests<EdgeData>
        where EdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Builds the router;
        /// </summary>
        /// <param name="data"></param>
        /// <param name="interpreter"></param>
        /// <param name="basicRouter"></param>
        /// <returns></returns>
        public abstract Router BuildRouter(IRoutingAlgorithmData<EdgeData> data,
            IRoutingInterpreter interpreter, IRoutingAlgorithm<EdgeData> basicRouter);

        /// <summary>
        /// Builds the basic router.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public abstract IRoutingAlgorithm<EdgeData> BuildBasicRouter(IRoutingAlgorithmData<EdgeData> data, 
            Vehicle vehicle);

        /// <summary>
        /// Builds the data.
        /// </summary>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="accessTags"></param>
        /// <returns></returns>
        public abstract IRoutingAlgorithmData<EdgeData> BuildData(IRoutingInterpreter interpreter, 
            Vehicle vehicle, List<KeyValuePair<string, string>> accessTags);

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="accessTags"></param>
        protected void DoTestShortestWithAccess(Vehicle vehicle, List<KeyValuePair<string, string>> accessTags)
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<EdgeData> data = this.BuildData(interpreter, vehicle, accessTags);
            IRoutingAlgorithm<EdgeData> basicRouter = this.BuildBasicRouter(data, vehicle);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(vehicle, new GeoCoordinate(51.0582205, 3.7192647)); // -52
            RouterPoint target = router.Resolve(vehicle, new GeoCoordinate(51.0579530, 3.7196168)); // -56

            Route route = router.Calculate(vehicle, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(4, route.Segments.Length);

            float latitude, longitude;
            data.GetVertex(19, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[0].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[0].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Start, route.Segments[0].Type);

            data.GetVertex(8, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[1].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[1].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[1].Type);

            data.GetVertex(9, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[2].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[2].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[2].Type);

            data.GetVertex(10, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[3].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[3].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Stop, route.Segments[3].Type);
        }

        /// <summary>
        /// Tests that a router actually finds the shortest route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="access_tags"></param>
        protected void DoTestShortestWithoutAccess(Vehicle vehicle, List<KeyValuePair<string, string>> access_tags)
        {
            var interpreter = new OsmRoutingInterpreter();
            IRoutingAlgorithmData<EdgeData> data = this.BuildData(interpreter, vehicle, access_tags);
            IRoutingAlgorithm<EdgeData> basicRouter = this.BuildBasicRouter(data, vehicle);
            Router router = this.BuildRouter(
                data, interpreter, basicRouter);
            RouterPoint source = router.Resolve(vehicle, new GeoCoordinate(51.0579530, 3.7196168)); // -56
            RouterPoint target = router.Resolve(vehicle, new GeoCoordinate(51.0582205, 3.7192647)); // -52

            Route route = router.Calculate(vehicle, source, target);
            Assert.IsNotNull(route);
            Assert.AreEqual(6, route.Segments.Length);

            float latitude, longitude;
            data.GetVertex(10, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[0].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[0].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Start, route.Segments[0].Type);

            data.GetVertex(12, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[1].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[1].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[1].Type);

            data.GetVertex(13, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[2].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[2].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[2].Type);

            data.GetVertex(14, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[3].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[3].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[3].Type);

            data.GetVertex(15, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[4].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[4].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Along, route.Segments[4].Type);

            data.GetVertex(19, out latitude, out longitude);
            Assert.AreEqual(latitude, route.Segments[5].Latitude, 0.00001);
            Assert.AreEqual(longitude, route.Segments[5].Longitude, 0.00001);
            Assert.AreEqual(RouteSegmentType.Stop, route.Segments[5].Type);
        }

        ///// <summary>
        ///// Tests if the many-to-many weights are the same as the point-to-point weights.
        ///// </summary>
        //protected void DoTestManyToMany1()
        //{
        //    OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();
        //    IRoutingAlgorithmData<EdgeData> data = this.BuildData(interpreter);
        //    IRoutingAlgorithm<EdgeData> basic_router = this.BuildBasicRouter(data);
        //    IRouter<ResolvedType> router = this.BuildRouter(
        //        data, interpreter, basic_router);
        //    ResolvedType[] resolved_points = new ResolvedType[3];
        //    resolved_points[0] = router.Resolve(VehicleEnum.Car, new GeoCoordinate(51.0578532, 3.7192229));
        //    resolved_points[1] = router.Resolve(VehicleEnum.Car, new GeoCoordinate(51.0576193, 3.7191801));
        //    resolved_points[2] = router.Resolve(VehicleEnum.Car, new GeoCoordinate(51.0581001, 3.7200612));

        //    double[][] weights = router.CalculateManyToManyWeight(VehicleEnum.Car, resolved_points, resolved_points);

        //    for (int x = 0; x < weights.Length; x++)
        //    {
        //        for (int y = 0; y < weights.Length; y++)
        //        {
        //            double many_to_many = weights[x][y];
        //            double point_to_point = router.CalculateWeight(VehicleEnum.Car, resolved_points[x], resolved_points[y]);

        //            Assert.AreEqual(point_to_point, many_to_many);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Test if the connectivity test succeed/fail.
        ///// </summary>
        //protected void DoTestConnectivity1()
        //{
        //    OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();
        //    IRoutingAlgorithmData<EdgeData> data = this.BuildData(interpreter);
        //    IRoutingAlgorithm<EdgeData> basic_router = this.BuildBasicRouter(data);
        //    IRouter<ResolvedType> router = this.BuildRouter(
        //        data, interpreter, basic_router);
        //    ResolvedType[] resolved_points = new ResolvedType[3];
        //    resolved_points[0] = router.Resolve(VehicleEnum.Car, new GeoCoordinate(51.0578532, 3.7192229));
        //    resolved_points[1] = router.Resolve(VehicleEnum.Car, new GeoCoordinate(51.0576193, 3.7191801));
        //    resolved_points[2] = router.Resolve(VehicleEnum.Car, new GeoCoordinate(51.0581001, 3.7200612));

        //    // test connectivity succes.
        //    Assert.IsTrue(router.CheckConnectivity(VehicleEnum.Car, resolved_points[0], 5));
        //    Assert.IsTrue(router.CheckConnectivity(VehicleEnum.Car, resolved_points[1], 5));
        //    Assert.IsTrue(router.CheckConnectivity(VehicleEnum.Car, resolved_points[2], 5));

        //    // test connectivity failiure.
        //    Assert.IsFalse(router.CheckConnectivity(VehicleEnum.Car, resolved_points[0], 1000));
        //    Assert.IsFalse(router.CheckConnectivity(VehicleEnum.Car, resolved_points[1], 1000));
        //    Assert.IsFalse(router.CheckConnectivity(VehicleEnum.Car, resolved_points[2], 1000));
        //}

        ///// <summary>
        ///// Test if the resolving of nodes returns those same nodes.
        ///// 
        ///// (does not work on a lazy loading data source!)
        ///// </summary>
        //protected void DoTestResolveAllNodes()
        //{
        //    OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();
        //    IRoutingAlgorithmData<EdgeData> data = this.BuildData(interpreter);
        //    IRoutingAlgorithm<EdgeData> basic_router = this.BuildBasicRouter(data);
        //    IRouter<ResolvedType> router = this.BuildRouter(
        //        data, interpreter, basic_router);
        //    for (int idx = 1; idx < data.VertexCount; idx++)
        //    {
        //        float latitude, longitude;
        //        if (data.GetVertex((uint)idx, out latitude, out longitude))
        //        {
        //            ResolvedType point = router.Resolve(VehicleEnum.Car, new GeoCoordinate(latitude, longitude));
        //            Assert.AreEqual(idx, (point as RouterPoint).Id);
        //        }
        //    }
        //}

        ///// <summary>
        ///// Test resolving all nodes.
        ///// </summary>
        //protected void DoTestResolveBetweenNodes()
        //{
        //    OsmRoutingInterpreter interpreter = new OsmRoutingInterpreter();
        //    IRoutingAlgorithmData<EdgeData> data = this.BuildData(interpreter);
        //    IRoutingAlgorithm<EdgeData> basic_router = this.BuildBasicRouter(data);

        //    float delta = 0.001f;
        //    SearchClosestResult result;
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0578761, 3.7193972), delta, null, null); //,-103,  -4,  -8
        //    Assert.IsTrue((result.Vertex1 == 20 && result.Vertex2 == 21) ||
        //        (result.Vertex1 == 21 && result.Vertex2 == 20));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0576510, 3.7194124), delta, null, null); //,-104, -14, -12
        //    Assert.IsTrue((result.Vertex1 == 22 && result.Vertex2 == 23) ||
        //        (result.Vertex1 == 23 && result.Vertex2 == 22));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0576829, 3.7196791), delta, null, null); //,-105, -12, -10
        //    Assert.IsTrue((result.Vertex1 == 22 && result.Vertex2 == 16) ||
        //        (result.Vertex1 == 16 && result.Vertex2 == 22));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0577819, 3.7196308), delta, null, null); //,-106, -10,  -8
        //    Assert.IsTrue((result.Vertex1 == 21 && result.Vertex2 == 16) ||
        //        (result.Vertex1 == 16 && result.Vertex2 == 21));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0577516, 3.7198975), delta, null, null); //,-107, -10, -18
        //    Assert.IsTrue((result.Vertex1 == 17 && result.Vertex2 == 16) ||
        //        (result.Vertex1 == 16 && result.Vertex2 == 17));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0578218, 3.7200626), delta, null, null); //,-108, -18, -20
        //    Assert.IsTrue((result.Vertex1 == 17 && result.Vertex2 == 7) ||
        //        (result.Vertex1 == 7 && result.Vertex2 == 17));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0578170, 3.7202480), delta, null, null); //,-109, -20, -76
        //    Assert.IsTrue((result.Vertex1 == 6 && result.Vertex2 == 7) ||
        //        (result.Vertex1 == 7 && result.Vertex2 == 6));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0577580, 3.7204004), delta, null, null); //,-110, -76, -74
        //    Assert.IsTrue((result.Vertex1 == 5 && result.Vertex2 == 6) ||
        //        (result.Vertex1 == 6 && result.Vertex2 == 5));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0579032, 3.7204258), delta, null, null); //,-111, -74, -72
        //    Assert.IsTrue((result.Vertex1 == 1 && result.Vertex2 == 5) ||
        //        (result.Vertex1 == 5 && result.Vertex2 == 1));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0580453, 3.7204614), delta, null, null); //,-112, -72, -70
        //    Assert.IsTrue((result.Vertex1 == 4 && result.Vertex2 == 1) ||
        //        (result.Vertex1 == 1 && result.Vertex2 == 4));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0581938, 3.7203953), delta, null, null); //,-113, -70, -68
        //    Assert.IsTrue((result.Vertex1 == 3 && result.Vertex2 == 4) ||
        //        (result.Vertex1 == 4 && result.Vertex2 == 3));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0581826, 3.7201413), delta, null, null); //,-114, -46, -68
        //    Assert.IsTrue((result.Vertex1 == 3 && result.Vertex2 == 2) ||
        //        (result.Vertex1 == 2 && result.Vertex2 == 3));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0580310, 3.7201998), delta, null, null); //,-115, -46, -72
        //    Assert.IsTrue((result.Vertex1 == 2 && result.Vertex2 == 1) ||
        //        (result.Vertex1 == 1 && result.Vertex2 == 2));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0579208, 3.7200525), delta, null, null); //,-116, -20, -22
        //    Assert.IsTrue((result.Vertex1 == 11 && result.Vertex2 == 7) ||
        //        (result.Vertex1 == 7 && result.Vertex2 == 11));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0580134, 3.7199966), delta, null, null); //,-117, -46, -22
        //    Assert.IsTrue((result.Vertex1 == 2 && result.Vertex2 == 11) ||
        //        (result.Vertex1 == 11 && result.Vertex2 == 2));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0581251, 3.7198950), delta, null, null); //,-118, -46, -48
        //    Assert.IsTrue((result.Vertex1 == 18 && result.Vertex2 == 2) ||
        //        (result.Vertex1 == 2 && result.Vertex2 == 18));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0579479, 3.7197985), delta, null, null); //,-119, -22, -56
        //    Assert.IsTrue((result.Vertex1 == 10 && result.Vertex2 == 11) ||
        //        (result.Vertex1 == 11 && result.Vertex2 == 10));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0580166, 3.7195496), delta, null, null); //,-120, -56, -65
        //    Assert.IsTrue((result.Vertex1 == 10 && result.Vertex2 == 9) ||
        //        (result.Vertex1 == 9 && result.Vertex2 == 10));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0581299, 3.7195673), delta, null, null); //,-121, -65, -50
        //    Assert.IsTrue((result.Vertex1 == 8 && result.Vertex2 == 9) ||
        //        (result.Vertex1 == 9 && result.Vertex2 == 8));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0581651, 3.7196664), delta, null, null); //,-122, -50, -48
        //    Assert.IsTrue((result.Vertex1 == 8 && result.Vertex2 == 18) ||
        //        (result.Vertex1 == 18 && result.Vertex2 == 8));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0582050, 3.7194505), delta, null, null); //,-123, -50, -52
        //    Assert.IsTrue((result.Vertex1 == 19 && result.Vertex2 == 8) ||
        //        (result.Vertex1 == 8 && result.Vertex2 == 19));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0582082, 3.7191330), delta, null, null); //,-124, -52, -54
        //    Assert.IsTrue((result.Vertex1 == 15 && result.Vertex2 == 19) ||
        //        (result.Vertex1 == 19 && result.Vertex2 == 15));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0581651, 3.7189628), delta, null, null); //,-125, -54, -62
        //    Assert.IsTrue((result.Vertex1 == 15 && result.Vertex2 == 14) ||
        //        (result.Vertex1 == 14 && result.Vertex2 == 15));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0580725, 3.7189781), delta, null, null); //,-126, -62, -60
        //    Assert.IsTrue((result.Vertex1 == 14 && result.Vertex2 == 13) ||
        //        (result.Vertex1 == 13 && result.Vertex2 == 14));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0580006, 3.7191305), delta, null, null); //,-127, -60, -58
        //    Assert.IsTrue((result.Vertex1 == 13 && result.Vertex2 == 12) ||
        //        (result.Vertex1 == 12 && result.Vertex2 == 13));
        //    result = basic_router.SearchClosest(data, interpreter, VehicleEnum.Car, new GeoCoordinate(51.0579783, 3.7194149), delta, null, null); //,-128, -58, -56
        //    Assert.IsTrue((result.Vertex1 == 10 && result.Vertex2 == 12) ||
        //        (result.Vertex1 == 12 && result.Vertex2 == 10));
        //}
    }
}