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
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;
using Itinero.Test.Algorithms.Search;
using Itinero.Test.Profiles;
using Itinero;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using Itinero.Algorithms.Contracted;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Contracted;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the router.
    /// </summary>
    [TestFixture]
    public class RouterTests
    {
        /// <summary>
        /// Tests routing using and edge-based contracted network.
        /// </summary>
        [Test]
        public void TestEdgeBasedContractedNetwork3()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network3.geojson"));

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);

            routerDb.Network.Sort();
            routerDb.Network.Compress();

            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(profile.Parent);

            //// test vertex-based.
            //var weightHandler = pedestrian.DefaultWeightHandlerCached(routerDb);
            //var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size, weightHandler.MetaSize);
            //var directedGraphBuilder = new DirectedGraphBuilder<float>(routerDb.Network.GeometricGraph.Graph, contracted, weightHandler);
            //directedGraphBuilder.Run();

            //// contract the graph.
            //var hierarchyBuilder = new Itinero.Algorithms.Contracted.Dual.HierarchyBuilder<float>(contracted,
            //    new Itinero.Algorithms.Contracted.Dual.Witness.DykstraWitnessCalculator<float>(contracted.Graph, weightHandler, int.MaxValue),
            //        weightHandler);
            //hierarchyBuilder.DepthFactor = 0;
            //hierarchyBuilder.ContractedFactor = 0;
            //hierarchyBuilder.DifferenceFactor = 1;
            //hierarchyBuilder.Run();

            //var contractedDb = new ContractedDb(contracted, false);

            //// add the graph.
            //routerDb.AddContracted(pedestrian, contractedDb);

            var json = routerDb.GetGeoJson();

            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var location2 = new Coordinate(52.35476168070f, 6.66636669078f);
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);

            var resolved1 = new Coordinate(52.352949189200494f, 6.665348410606384f);
            var resolved2 = new Coordinate(52.354736518079150f, 6.666120886802673f);
            var resolved3 = new Coordinate(52.354974058638945f, 6.664675176143646f);
            var resolved4 = new Coordinate(52.353594667362720f, 6.664688587188721f);

            var vertex0id = routerDb.SearchVertexFor(vertex0.Latitude, vertex0.Longitude);
            var vertex1id = routerDb.SearchVertexFor(vertex1.Latitude, vertex1.Longitude);
            var vertex2id = routerDb.SearchVertexFor(vertex2.Latitude, vertex2.Longitude);
            var vertex3id = routerDb.SearchVertexFor(vertex3.Latitude, vertex3.Longitude);
            var vertex4id = routerDb.SearchVertexFor(vertex4.Latitude, vertex4.Longitude);
            var vertex5id = routerDb.SearchVertexFor(vertex5.Latitude, vertex5.Longitude);
            var vertex6id = routerDb.SearchVertexFor(vertex6.Latitude, vertex6.Longitude);
            var vertex7id = routerDb.SearchVertexFor(vertex7.Latitude, vertex7.Longitude);
            var vertex8id = routerDb.SearchVertexFor(vertex8.Latitude, vertex8.Longitude);

            //var route = router.Calculate(pedestrian, vertex0, vertex0);
            //route = router.Calculate(pedestrian, vertex0, vertex1);
            //route = router.Calculate(pedestrian, vertex0, vertex2);
            //route = router.Calculate(pedestrian, vertex0, vertex3);
            //route = router.Calculate(pedestrian, vertex0, vertex4);
            //route = router.Calculate(pedestrian, vertex0, vertex5);
            //route = router.Calculate(pedestrian, vertex0, vertex6);
            //route = router.Calculate(pedestrian, vertex0, vertex7);
            //route = router.Calculate(pedestrian, vertex0, vertex8);

            //route = router.Calculate(pedestrian, vertex1, vertex0);
            //route = router.Calculate(pedestrian, vertex1, vertex1);
            //route = router.Calculate(pedestrian, vertex1, vertex2);
            //route = router.Calculate(pedestrian, vertex1, vertex3);
            //route = router.Calculate(pedestrian, vertex1, vertex4);
            //route = router.Calculate(pedestrian, vertex1, vertex5);
            //route = router.Calculate(pedestrian, vertex1, vertex6);
            //route = router.Calculate(pedestrian, vertex1, vertex7);
            //route = router.Calculate(pedestrian, vertex1, vertex8);

            //route = router.Calculate(pedestrian, vertex2, vertex0);
            //route = router.Calculate(pedestrian, vertex2, vertex1);
            //route = router.Calculate(pedestrian, vertex2, vertex2);
            //route = router.Calculate(pedestrian, vertex2, vertex3);
            //route = router.Calculate(pedestrian, vertex2, vertex4);
            //route = router.Calculate(pedestrian, vertex2, vertex5);
            //route = router.Calculate(pedestrian, vertex2, vertex6);
            //route = router.Calculate(pedestrian, vertex2, vertex7);
            //route = router.Calculate(pedestrian, vertex2, vertex8);

            //route = router.Calculate(pedestrian, vertex3, vertex0);
            //route = router.Calculate(pedestrian, vertex3, vertex1);
            //route = router.Calculate(pedestrian, vertex3, vertex2);
            //route = router.Calculate(pedestrian, vertex3, vertex3);
            //route = router.Calculate(pedestrian, vertex3, vertex4);
            //route = router.Calculate(pedestrian, vertex3, vertex5);
            //route = router.Calculate(pedestrian, vertex3, vertex6);
            //route = router.Calculate(pedestrian, vertex3, vertex7);
            //route = router.Calculate(pedestrian, vertex3, vertex8);

            //route = router.Calculate(pedestrian, vertex4, vertex0);
            //route = router.Calculate(pedestrian, vertex4, vertex1);
            //route = router.Calculate(pedestrian, vertex4, vertex2);
            //route = router.Calculate(pedestrian, vertex4, vertex3);
            //route = router.Calculate(pedestrian, vertex4, vertex4);
            //route = router.Calculate(pedestrian, vertex4, vertex5);
            //route = router.Calculate(pedestrian, vertex4, vertex6);
            //route = router.Calculate(pedestrian, vertex4, vertex7);
            //route = router.Calculate(pedestrian, vertex4, vertex8);

            //route = router.Calculate(pedestrian, vertex5, vertex0);
            //route = router.Calculate(pedestrian, vertex5, vertex1);
            //route = router.Calculate(pedestrian, vertex5, vertex2);
            //route = router.Calculate(pedestrian, vertex5, vertex3);
            //route = router.Calculate(pedestrian, vertex5, vertex4);
            //route = router.Calculate(pedestrian, vertex5, vertex5);
            //route = router.Calculate(pedestrian, vertex5, vertex6);
            //route = router.Calculate(pedestrian, vertex5, vertex7);
            //route = router.Calculate(pedestrian, vertex5, vertex8);

            //route = router.Calculate(pedestrian, vertex6, vertex0);
            //route = router.Calculate(pedestrian, vertex6, vertex1);
            //route = router.Calculate(pedestrian, vertex6, vertex2);
            //route = router.Calculate(pedestrian, vertex6, vertex3);
            //route = router.Calculate(pedestrian, vertex6, vertex4);
            //route = router.Calculate(pedestrian, vertex6, vertex5);
            //route = router.Calculate(pedestrian, vertex6, vertex6);
            //route = router.Calculate(pedestrian, vertex6, vertex7);
            //route = router.Calculate(pedestrian, vertex6, vertex8);

            //route = router.Calculate(pedestrian, vertex7, vertex0);
            //route = router.Calculate(pedestrian, vertex7, vertex1);
            //route = router.Calculate(pedestrian, vertex7, vertex2);
            //route = router.Calculate(pedestrian, vertex7, vertex3);
            //route = router.Calculate(pedestrian, vertex7, vertex4);
            //route = router.Calculate(pedestrian, vertex7, vertex5);
            //route = router.Calculate(pedestrian, vertex7, vertex6);
            //route = router.Calculate(pedestrian, vertex7, vertex7);
            //route = router.Calculate(pedestrian, vertex7, vertex8);

            //route = router.Calculate(pedestrian, vertex8, vertex0);
            //route = router.Calculate(pedestrian, vertex8, vertex1);
            //route = router.Calculate(pedestrian, vertex8, vertex2);
            //route = router.Calculate(pedestrian, vertex8, vertex3);
            //route = router.Calculate(pedestrian, vertex8, vertex4);
            //route = router.Calculate(pedestrian, vertex8, vertex5);
            //route = router.Calculate(pedestrian, vertex8, vertex6);
            //route = router.Calculate(pedestrian, vertex8, vertex7);
            //route = router.Calculate(pedestrian, vertex8, vertex8);

            //route = router.Calculate(pedestrian, vertex0, vertex8);
            //route = router.Calculate(pedestrian, vertex3, vertex7);
            //route = router.Calculate(pedestrian, resolved4, resolved2);

            // test edge-based.
            routerDb.AddContracted(profile, true);

            var route = router.Calculate(profile, vertex0, vertex0);
            route = router.Calculate(profile, vertex0, vertex1);
            route = router.Calculate(profile, vertex0, vertex2);
            route = router.Calculate(profile, vertex0, vertex3);
            route = router.Calculate(profile, vertex0, vertex4);
            route = router.Calculate(profile, vertex0, vertex5);
            route = router.Calculate(profile, vertex0, vertex6);
            route = router.Calculate(profile, vertex0, vertex7);
            route = router.Calculate(profile, vertex0, vertex8);

            route = router.Calculate(profile, vertex1, vertex0);
            route = router.Calculate(profile, vertex1, vertex1);
            route = router.Calculate(profile, vertex1, vertex2);
            route = router.Calculate(profile, vertex1, vertex3);
            route = router.Calculate(profile, vertex1, vertex4);
            route = router.Calculate(profile, vertex1, vertex5);
            route = router.Calculate(profile, vertex1, vertex6);
            route = router.Calculate(profile, vertex1, vertex7);
            route = router.Calculate(profile, vertex1, vertex8);

            route = router.Calculate(profile, vertex2, vertex0);
            route = router.Calculate(profile, vertex2, vertex1);
            route = router.Calculate(profile, vertex2, vertex2);
            route = router.Calculate(profile, vertex2, vertex3);
            route = router.Calculate(profile, vertex2, vertex4);
            route = router.Calculate(profile, vertex2, vertex5);
            route = router.Calculate(profile, vertex2, vertex6);
            route = router.Calculate(profile, vertex2, vertex7);
            route = router.Calculate(profile, vertex2, vertex8);

            route = router.Calculate(profile, vertex3, vertex0);
            route = router.Calculate(profile, vertex3, vertex1);
            route = router.Calculate(profile, vertex3, vertex2);
            route = router.Calculate(profile, vertex3, vertex3);
            route = router.Calculate(profile, vertex3, vertex4);
            route = router.Calculate(profile, vertex3, vertex5);
            route = router.Calculate(profile, vertex3, vertex6);
            route = router.Calculate(profile, vertex3, vertex7);
            route = router.Calculate(profile, vertex3, vertex8);

            route = router.Calculate(profile, vertex4, vertex0);
            route = router.Calculate(profile, vertex4, vertex1);
            route = router.Calculate(profile, vertex4, vertex2);
            route = router.Calculate(profile, vertex4, vertex3);
            route = router.Calculate(profile, vertex4, vertex4);
            route = router.Calculate(profile, vertex4, vertex5);
            route = router.Calculate(profile, vertex4, vertex6);
            route = router.Calculate(profile, vertex4, vertex7);
            route = router.Calculate(profile, vertex4, vertex8);

            route = router.Calculate(profile, vertex5, vertex0);
            route = router.Calculate(profile, vertex5, vertex1);
            route = router.Calculate(profile, vertex5, vertex2);
            route = router.Calculate(profile, vertex5, vertex3);
            route = router.Calculate(profile, vertex5, vertex4);
            route = router.Calculate(profile, vertex5, vertex5);
            route = router.Calculate(profile, vertex5, vertex6);
            route = router.Calculate(profile, vertex5, vertex7);
            route = router.Calculate(profile, vertex5, vertex8);

            route = router.Calculate(profile, vertex6, vertex0);
            route = router.Calculate(profile, vertex6, vertex1);
            route = router.Calculate(profile, vertex6, vertex2);
            route = router.Calculate(profile, vertex6, vertex3);
            route = router.Calculate(profile, vertex6, vertex4);
            route = router.Calculate(profile, vertex6, vertex5);
            route = router.Calculate(profile, vertex6, vertex6);
            route = router.Calculate(profile, vertex6, vertex7);
            route = router.Calculate(profile, vertex6, vertex8);

            route = router.Calculate(profile, vertex7, vertex0);
            route = router.Calculate(profile, vertex7, vertex1);
            route = router.Calculate(profile, vertex7, vertex2);
            route = router.Calculate(profile, vertex7, vertex3);
            route = router.Calculate(profile, vertex7, vertex4);
            route = router.Calculate(profile, vertex7, vertex5);
            route = router.Calculate(profile, vertex7, vertex6);
            route = router.Calculate(profile, vertex7, vertex7);
            route = router.Calculate(profile, vertex7, vertex8);

            route = router.Calculate(profile, vertex8, vertex0);
            route = router.Calculate(profile, vertex8, vertex1);
            route = router.Calculate(profile, vertex8, vertex2);
            route = router.Calculate(profile, vertex8, vertex3);
            route = router.Calculate(profile, vertex8, vertex4);
            route = router.Calculate(profile, vertex8, vertex5);
            route = router.Calculate(profile, vertex8, vertex6);
            route = router.Calculate(profile, vertex8, vertex7);
            route = router.Calculate(profile, vertex8, vertex8);

            route = router.Calculate(profile, vertex0, vertex8);
            route = router.Calculate(profile, vertex3, vertex7);
            route = router.Calculate(profile, resolved4, resolved2);
        }

        /// <summary>
        /// Tests routing using and edge-based contracted network.
        /// </summary>
        [Test]
        public void TestEdgeBasedContractedNetwork5()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network5.geojson"));

            var pedestrian = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(pedestrian.Parent);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);
            var vertex8 = routerDb.Network.GetVertex(8);
            var vertex9 = routerDb.Network.GetVertex(9);
            var vertex10 = routerDb.Network.GetVertex(10);
            var vertex11 = routerDb.Network.GetVertex(11);
            var vertex12 = routerDb.Network.GetVertex(12);
            var vertex13 = routerDb.Network.GetVertex(13);
            var vertex14 = routerDb.Network.GetVertex(14);
            var vertex15 = routerDb.Network.GetVertex(15);
            var vertex16 = routerDb.Network.GetVertex(16);
            var vertex17 = routerDb.Network.GetVertex(17);

            routerDb.Network.Sort();
            routerDb.AddContracted(pedestrian, true);

            var vertex0sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex0.Latitude, vertex0.Longitude, 0.0001f, 0.0001f);
            var vertex1sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex1.Latitude, vertex1.Longitude, 0.0001f, 0.0001f);
            var vertex2sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex2.Latitude, vertex2.Longitude, 0.0001f, 0.0001f);
            var vertex3sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex3.Latitude, vertex3.Longitude, 0.0001f, 0.0001f);
            var vertex4sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex4.Latitude, vertex4.Longitude, 0.0001f, 0.0001f);
            var vertex5sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex5.Latitude, vertex5.Longitude, 0.0001f, 0.0001f);
            var vertex6sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex6.Latitude, vertex6.Longitude, 0.0001f, 0.0001f);
            var vertex7sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex7.Latitude, vertex7.Longitude, 0.0001f, 0.0001f);
            var vertex8sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex8.Latitude, vertex8.Longitude, 0.0001f, 0.0001f);
            var vertex9sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex9.Latitude, vertex9.Longitude, 0.0001f, 0.0001f);
            var vertex10sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex10.Latitude, vertex10.Longitude, 0.0001f, 0.0001f);
            var vertex11sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex11.Latitude, vertex11.Longitude, 0.0001f, 0.0001f);
            var vertex12sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex12.Latitude, vertex12.Longitude, 0.0001f, 0.0001f);
            var vertex13sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex13.Latitude, vertex13.Longitude, 0.0001f, 0.0001f);
            var vertex14sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex14.Latitude, vertex14.Longitude, 0.0001f, 0.0001f);
            var vertex15sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex15.Latitude, vertex15.Longitude, 0.0001f, 0.0001f);
            var vertex16sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex16.Latitude, vertex16.Longitude, 0.0001f, 0.0001f);
            var vertex17sorted = routerDb.Network.GeometricGraph.SearchClosest(vertex17.Latitude, vertex17.Longitude, 0.0001f, 0.0001f);

            var router = new Router(routerDb);

            var vertices = new Coordinate[] { vertex0, vertex1, vertex2, vertex3, vertex4, vertex5, vertex6, vertex7, vertex8, vertex9,
                vertex10, vertex11, vertex12, vertex13, vertex14, vertex15, vertex16, vertex17 };

            for (int f = 0; f < vertices.Length; f++)
            {
                for (int t = 0; t < vertices.Length; t++)
                {
                    var route = router.TryCalculate(pedestrian, vertices[f], vertices[t]);
                    Assert.IsFalse(route.IsError);
                }
            }
        }

        /// <summary>
        /// Tests routing using and edge-based contracted network.
        /// </summary>
        [Test]
        public void TestEdgeBasedContractedNetwork6()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network6.geojson"));

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);

            var vertex0 = routerDb.Network.GetVertex(0);
            var vertex1 = routerDb.Network.GetVertex(1);
            var vertex2 = routerDb.Network.GetVertex(2);
            var vertex3 = routerDb.Network.GetVertex(3);
            var vertex4 = routerDb.Network.GetVertex(4);
            var vertex5 = routerDb.Network.GetVertex(5);
            var vertex6 = routerDb.Network.GetVertex(6);
            var vertex7 = routerDb.Network.GetVertex(7);

            routerDb.Network.Sort();
            routerDb.AddContracted(car, true);

            var router = new Router(routerDb);

            var vertices = new Coordinate[] { vertex0, vertex1, vertex2, vertex3, vertex4, vertex5, vertex6, vertex7 };

            for (int f = 0; f < vertices.Length; f++)
            {
                for (int t = 0; t < vertices.Length; t++)
                {
                    var route = router.TryCalculate(car, vertices[f], vertices[t]);
                    Assert.IsFalse(route.IsError);
                }
            }
        }

        /// <summary>
        /// Tests routing on a network with a restriction.
        /// </summary>
        [Test]
        public void TestNetwork7()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network7.geojson"));

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);

            var vertices = new Coordinate[]
                {
                    routerDb.Network.GetVertex(0),
                    routerDb.Network.GetVertex(1),
                    routerDb.Network.GetVertex(2),
                    routerDb.Network.GetVertex(3),
                    routerDb.Network.GetVertex(4),
                    routerDb.Network.GetVertex(5),
                    routerDb.Network.GetVertex(6),
                    routerDb.Network.GetVertex(7)
                };

            routerDb.Sort();
            routerDb.AddContracted(car, true);

            var ids = new uint[vertices.Length];
            for (uint v = 0; v < ids.Length; v++)
            {
                ids[v] = routerDb.SearchVertexFor(vertices[v].Latitude, vertices[v].Longitude);
            }

            var router = new Router(routerDb);

            var route = router.Calculate(car, vertices[0], vertices[2]);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(3, route.Shape.Length);
            Assert.AreEqual(vertices[0].Latitude, route.Shape[0].Latitude);
            Assert.AreEqual(vertices[0].Longitude, route.Shape[0].Longitude);
            Assert.AreEqual(vertices[1].Latitude, route.Shape[1].Latitude);
            Assert.AreEqual(vertices[1].Longitude, route.Shape[1].Longitude);
            Assert.AreEqual(vertices[2].Latitude, route.Shape[2].Latitude);
            Assert.AreEqual(vertices[2].Longitude, route.Shape[2].Longitude);
            route = router.Calculate(car, vertices[2], vertices[0]);
            Assert.IsNotNull(route);
            Assert.IsNotNull(route.Shape);
            Assert.AreEqual(5, route.Shape.Length);
            Assert.AreEqual(vertices[2].Latitude, route.Shape[0].Latitude);
            Assert.AreEqual(vertices[2].Longitude, route.Shape[0].Longitude);
            Assert.AreEqual(vertices[5].Latitude, route.Shape[1].Latitude);
            Assert.AreEqual(vertices[5].Longitude, route.Shape[1].Longitude);
            Assert.AreEqual(vertices[4].Latitude, route.Shape[2].Latitude);
            Assert.AreEqual(vertices[4].Longitude, route.Shape[2].Longitude);
            Assert.AreEqual(vertices[1].Latitude, route.Shape[3].Latitude);
            Assert.AreEqual(vertices[1].Longitude, route.Shape[3].Longitude);
            Assert.AreEqual(vertices[0].Latitude, route.Shape[4].Latitude);
            Assert.AreEqual(vertices[0].Longitude, route.Shape[4].Longitude);
        }

        /// <summary>
        /// Tests an error causing a U-turn in the resulting route at the end of a dead-end.
        /// </summary>
        [Test]
        public void TestRegressionEdgeBasedRoute1()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            routerDb.Network.Sort();

            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            var router = new Router(routerDb);

            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var edge1 = router.Resolve(car, location1).EdgeId;
            var location4 = new Coordinate(52.35361232125f, 6.66458017720f);
            var edge4 = router.Resolve(car, location4).EdgeId;

            var networkEdge1 = routerDb.Network.GetEdge(edge1);
            var edge1VertexFrom = routerDb.Network.GetVertex(networkEdge1.From);
            var edge1VertexTo = routerDb.Network.GetVertex(networkEdge1.To);
            var networkEdge4 = routerDb.Network.GetEdge(edge4);
            var edge4VertexFrom = routerDb.Network.GetVertex(networkEdge4.From);
            var edge4VertexTo = routerDb.Network.GetVertex(networkEdge4.To);

            // There should be no route found here, we are searching in the dead-end direction of a dead-end street, by 
            // design no route should be returned.
            // [edge1 -> edge4]
            var routeResult = router.TryCalculateRaw(car, car.DefaultWeightHandler(router), edge1 + 1, edge4 + 1, null);
            Assert.AreEqual(true, routeResult.IsError);
        }

        /// <summary>
        /// Tests non-contracted directed edge based routing.
        /// </summary>
        [Test]
        public void TestUncontractedDirectedEdgeBasedRouting()
        {
            var e = 0.0001f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved1, true, resolved2, true);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[7], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[5]), e);
            route = router.TryCalculate(vehicle, resolved1, true, resolved2, false);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[7], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[5], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[2], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[5]), e);
            route = router.TryCalculate(vehicle, resolved1, false, resolved2, true);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(9, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[2], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[5], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[5]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[6]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[7]), e);
            route = router.TryCalculate(vehicle, resolved1, false, resolved2, false);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(3, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[1]), e);
        }

        /// <summary>
        /// Tests contracted directed edge based routing.
        /// </summary>
        [Test]
        public void TestContractedDirectedEdgeBasedRouting()
        {
            var e = 0.0001f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            // route between two locations for all 4 options.
            var location1 = new Coordinate(52.35286546406f, 6.66554092450f);
            var resolved1 = router.Resolve(vehicle, location1);
            var location2 = new Coordinate(52.3547616807f, 6.66636669078f);
            var resolved2 = router.Resolve(vehicle, location2);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved1, true, resolved2, true);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[7], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[5]), e);
            route = router.TryCalculate(vehicle, resolved1, true, resolved2, false);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(7, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[7], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[5], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[2], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[5]), e);
            route = router.TryCalculate(vehicle, resolved1, false, resolved2, true);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(9, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[2], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[5], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[5]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[6]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[7]), e);
            route = router.TryCalculate(vehicle, resolved1, false, resolved2, false);
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(3, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[1]), e);
        }

        /// <summary>
        /// Tests non-contracted directed edge based routing starting and stopping on the same edge.
        /// </summary>
        [Test]
        public void TestUncontractedDirectedEdgeBasedRoutingSameEdge()
        {
            var e = 0.0001f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            var router = new Router(routerDb);

            // test directed routes on the same edge.
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved3, true, resolved5, true); // should result in shortest route between 3 and 5
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(2, route.Value.Shape.Length);
            route = router.TryCalculate(vehicle, resolved3, true, resolved5, false); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(10, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[7]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[8]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved5, true); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(12, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[8]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[9]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[10]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved5, false); // should result in a route that's almost a closed loop
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(9, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[5], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[2], route.Value.Shape[5]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[6]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[7]), e);
        }

        /// <summary>
        /// Tests non-contracted directed edge based routing starting and stopping on the same edge.
        /// </summary>
        [Test]
        public void TestContractedDirectedEdgeBasedRoutingSameEdge()
        {
            var e = 0.0001f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            // test directed routes on the same edge.
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved3, true, resolved5, true); // should result in shortest route between 3 and 5
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(2, route.Value.Shape.Length);
            route = router.TryCalculate(vehicle, resolved3, true, resolved5, false); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(10, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[7]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[8]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved5, true); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(12, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[8]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[9]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[10]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved5, false); // should result in a route that's almost a closed loop
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(9, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[5], route.Value.Shape[4]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[2], route.Value.Shape[5]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[6]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[7]), e);
        }

        /// <summary>
        /// Tests non-contracted directed edge based routing starting and stopping at the exact same location.
        /// </summary>
        [Test]
        public void TestUncontractedDirectedEdgeBasedRoutingSameLocation()
        {
            var e = 0.0001f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            var router = new Router(routerDb);

            // test routes between identical location.
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved3, true, resolved3, true); // should result in a route of length '0'.
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(1, route.Value.Shape.Length);
            route = router.TryCalculate(vehicle, resolved3, true, resolved3, false); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(10, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[7]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[8]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved3, true); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(12, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[8]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[9]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[10]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved3, false); // should result in a route of length '0'.
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(1, route.Value.Shape.Length);
        }

        /// <summary>
        /// Tests contracted directed edge based routing starting and stopping at the exact same location.
        /// </summary>
        [Test]
        public void TestContractedDirectedEdgeBasedRoutingSameLocation()
        {
            var e = 0.0001f;

            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            // test routes between identical location.
            var location3 = new Coordinate(52.35502840541f, 6.66461193744f);
            var resolved3 = router.Resolve(vehicle, location3);
            var location5 = new Coordinate(52.35525746742958f, 6.665166020393372f);
            var resolved5 = router.Resolve(vehicle, location5);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved3, true, resolved3, true); // should result in a route of length '0'.
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(1, route.Value.Shape.Length);
            route = router.TryCalculate(vehicle, resolved3, true, resolved3, false); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(10, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[1], route.Value.Shape[7]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[0], route.Value.Shape[8]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved3, true); // should result in a route with a turn along the loop in the network
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(12, route.Value.Shape.Length);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[1]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[2]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[3]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[6], route.Value.Shape[8]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[4], route.Value.Shape[9]), e);
            Assert.AreEqual(0, Coordinate.DistanceEstimateInMeter(vertices[3], route.Value.Shape[10]), e);
            route = router.TryCalculate(vehicle, resolved3, false, resolved3, false); // should result in a route of length '0'.
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(1, route.Value.Shape.Length);
        }

        /// <summary>
        /// Tests non-contracted directed edge based routing starting and stopping on the same edge but that edge is oneway.
        /// </summary>
        [Test]
        public void TestUncontractedDirectedEdgeBasedRoutingSameEdgeOneWay()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            var router = new Router(routerDb);

            // test directed routes on the same edge.
            var location6 = new Coordinate(52.35383385230824f, 6.663897335529327f);
            var resolved6 = router.Resolve(vehicle, location6);
            var location7 = new Coordinate(52.35338333164195f, 6.66343331336975f);
            var resolved7 = router.Resolve(vehicle, location7);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved6, true, resolved7, true); // should result route within edge.
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(2, route.Value.Shape.Length);
            route = router.TryCalculate(vehicle, resolved6, true, resolved7, false); // should result in error.
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(vehicle, resolved6, false, resolved7, true); // should result in error.
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(vehicle, resolved6, false, resolved7, false); // should result in error.
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// Tests contracted directed edge based routing starting and stopping on the same edge but that edge is oneway.
        /// </summary>
        [Test]
        public void TestContractedDirectedEdgeBasedRoutingSameEdgeOneWay()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7)
            };

            // sort the network.
            routerDb.Network.Sort();

            // defined and add the supported profile.
            var vehicle = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            routerDb.AddSupportedVehicle(vehicle.Parent);
            routerDb.AddContracted(vehicle, true);
            var router = new Router(routerDb);

            // test directed routes on the same edge.
            var location6 = new Coordinate(52.35383385230824f, 6.663897335529327f);
            var resolved6 = router.Resolve(vehicle, location6);
            var location7 = new Coordinate(52.35338333164195f, 6.66343331336975f);
            var resolved7 = router.Resolve(vehicle, location7);

            // route and verify.
            var route = router.TryCalculate(vehicle, resolved6, true, resolved7, true); // should result route within edge.
            Assert.IsFalse(route.IsError);
            Assert.AreEqual(2, route.Value.Shape.Length);
            route = router.TryCalculate(vehicle, resolved6, true, resolved7, false); // should result in error.
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(vehicle, resolved6, false, resolved7, true); // should result in error.
            Assert.IsTrue(route.IsError);
            route = router.TryCalculate(vehicle, resolved6, false, resolved7, false); // should result in error.
            Assert.IsTrue(route.IsError);
        }

        /// <summary>
        /// Tests a simple restriction with a one-hop route.
        /// </summary>
        [Test]
        public void TestUncontractedSimpleRestrictionOneHopRoute()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network12.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7),
                routerDb.Network.GetVertex(8),
                routerDb.Network.GetVertex(9)
            };

            var location1 = new Coordinate(51.26568107896568f, 4.7888267040252686f);
            var location2 = new Coordinate(51.26647993569030f, 4.7911763191223145f);
            var location3 = new Coordinate(51.26551996332175f, 4.7935795783996580f);

            // sort the network.
            routerDb.Sort();

            // defined and add the supported profile.
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            var bicycle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            routerDb.AddSupportedVehicle(bicycle.Parent);
            var router = new Router(routerDb);

            // calculate routes.
            var route1 = router.TryCalculate(car, location1, location2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(car, location1, location3);
            Assert.IsTrue(route2.IsError);
            var route3 = router.TryCalculate(car, location2, location3);
            Assert.IsFalse(route3.IsError);
            Assert.AreEqual(715, route3.Value.TotalDistance, 10);
        }

        /// <summary>
        /// Tests a simple restriction with a one-hop route in a contracted network.
        /// </summary>
        [Test]
        public void TestContractedSimpleRestrictionOneHopRoute()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network12.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7),
                routerDb.Network.GetVertex(8),
                routerDb.Network.GetVertex(9)
            };

            var location1 = new Coordinate(51.26568107896568f, 4.7888267040252686f);
            var location2 = new Coordinate(51.26647993569030f, 4.7911763191223145f);
            var location3 = new Coordinate(51.26551996332175f, 4.7935795783996580f);

            // sort the network.
            routerDb.Sort();

            // defined and add the supported profile.
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            var bicycle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            routerDb.AddSupportedVehicle(bicycle.Parent);
            routerDb.AddContracted(car);
            var router = new Router(routerDb);

            // calculate routes.
            var route1 = router.TryCalculate(car, location1, location2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(car, location1, location3);
            Assert.IsTrue(route2.IsError);
            var route3 = router.TryCalculate(car, location2, location3);
            Assert.IsFalse(route3.IsError);
            Assert.AreEqual(715, route3.Value.TotalDistance, 10);
        }

        /// <summary>
        /// Tests a simple restriction with a one-hop route in a network with complex restrictions.
        /// </summary>
        [Test]
        public void TestUncontractedRestrictionOneHopRoute()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network13.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7),
                routerDb.Network.GetVertex(8),
                routerDb.Network.GetVertex(9)
            };

            var location1 = new Coordinate(51.26568107896568f, 4.7888267040252686f);
            var location2 = new Coordinate(51.26647993569030f, 4.7911763191223145f);
            var location3 = new Coordinate(51.26551996332175f, 4.7935795783996580f);

            // sort the network.
            routerDb.Sort();

            // defined and add the supported profile.
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            var bicycle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            routerDb.AddSupportedVehicle(bicycle.Parent);
            var router = new Router(routerDb);

            // calculate routes.
            var route1 = router.TryCalculate(car, location1, location2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(car, location1, location3);
            Assert.IsTrue(route2.IsError);
            var route3 = router.TryCalculate(car, location2, location3);
            Assert.IsFalse(route3.IsError);
            Assert.AreEqual(715, route3.Value.TotalDistance, 10);
        }

        /// <summary>
        /// Tests a simple restriction with a one-hop route in a network with complex restrictions.
        /// </summary>
        [Test]
        public void TestContractedRestrictionOneHopRoute()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network13.geojson"));

            // get the vertex locations to verify the resulting routes.
            var vertices = new Coordinate[]
            {
                routerDb.Network.GetVertex(0),
                routerDb.Network.GetVertex(1),
                routerDb.Network.GetVertex(2),
                routerDb.Network.GetVertex(3),
                routerDb.Network.GetVertex(4),
                routerDb.Network.GetVertex(5),
                routerDb.Network.GetVertex(6),
                routerDb.Network.GetVertex(7),
                routerDb.Network.GetVertex(8),
                routerDb.Network.GetVertex(9)
            };

            var location1 = new Coordinate(51.26568107896568f, 4.7888267040252686f);
            var location2 = new Coordinate(51.26647993569030f, 4.7911763191223145f);
            var location3 = new Coordinate(51.26551996332175f, 4.7935795783996580f);

            // sort the network.
            routerDb.Sort();

            // defined and add the supported profile.
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            var bicycle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            routerDb.AddSupportedVehicle(bicycle.Parent);
            routerDb.AddContracted(car);
            var router = new Router(routerDb);

            // calculate routes.
            var route1 = router.TryCalculate(car, location1, location2);
            Assert.IsTrue(route1.IsError);
            var route2 = router.TryCalculate(car, location1, location3);
            Assert.IsTrue(route2.IsError);
            var route3 = router.TryCalculate(car, location2, location3);
            Assert.IsFalse(route3.IsError);
            Assert.AreEqual(715, route3.Value.TotalDistance, 10);
        }

        /// <summary>
        /// Tests connectivity.
        /// </summary>
        [Test]
        public void TestCheckConnectivity()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network14.geojson"));

            var location1 = new Coordinate(51.265711288086045f, 4.798069596290588f);

            // sort the network.
            routerDb.Sort();
            
            // defined and add the supported profile.
            var car = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            var bicycle = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest();
            routerDb.AddSupportedVehicle(car.Parent);
            routerDb.AddSupportedVehicle(bicycle.Parent);
            var router = new Router(routerDb);

            var json = routerDb.GetGeoJson();

            //foreach(var db in routerDb.RestrictionDbs)
            //{
            //    var dbEnumerator = db.RestrictionsDb.GetEnumerator();
            //    while(dbEnumerator.MoveNext())
            //    {
            //        var c = new uint[dbEnumerator.Count];
            //        for (var i = 0; i < dbEnumerator.Count; i++)
            //        {
            //            c[i] = dbEnumerator[i];
            //        }
            //    }
            //}

            // check connectivity.
            var resolved = router.Resolve(car, location1);
            var connected = router.CheckConnectivity(car, resolved, 25);
            connected = router.CheckConnectivity(bicycle, resolved, 250);
        }
    }
}