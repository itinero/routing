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
using Itinero.Algorithms.Contracted;
using Itinero.Data.Contracted;
using Itinero.Data.Network;
using Itinero.Graphs.Directed;
using Itinero.Test.Profiles;
using Itinero.Profiles;
using Itinero.Data.Contracted.Edges;
using Itinero;

namespace Itinero.Test.Algorithms.Contracted
{
    /// <summary>
    /// Contains tests for the many-to-many algorithm.
    /// </summary>
    [TestFixture]
    public class ManyToManyWeightsTests
    {
        private static float SecondsTolerance = 0.25f;

        /// <summary>
        /// Tests many-to-many path calculations on just one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m----(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestOneEdge()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0, 0);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.AddContracted(VehicleMock.Car().Fastest());

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { new RouterPoint(0, 0, 0, 0) }, 
                new RouterPoint[] { new RouterPoint(1, 1, 0, ushort.MaxValue) });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(1, algorithm.Weights.Length);
            Assert.AreEqual(1, algorithm.Weights[0].Length);
            Assert.AreEqual(VehicleMock.Car().Fastest().FactorAndSpeed(null).Value * 100, algorithm.Weights[0][0], SecondsTolerance);
        }

        /// <summary>
        /// Tests many-to-many path calculations on within one edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)---100m---(1) @ 100km/h
        /// </remarks>
        [Test]
        public void TestWithinOneEdge()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 0, 0);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.AddContracted(VehicleMock.Car().Fastest());

            // run algorithm.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { new RouterPoint(0, 0, 0, ushort.MaxValue / 10) },
                new RouterPoint[] { new RouterPoint(1, 1, 0, ushort.MaxValue / 10 * 9) });
            algorithm.Run();

            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(1, algorithm.Weights.Length);
            Assert.AreEqual(1, algorithm.Weights[0].Length);
            Assert.AreEqual(VehicleMock.Car().Fastest().FactorAndSpeed(null).Value * 80, algorithm.Weights[0][0], SecondsTolerance);
        }

        /// <summary>
        /// Tests many to many calculations between vertices on a triangle.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (0)----100m----(1)
        ///   \             /
        ///    \           /           
        ///     \         /
        ///     100m    100m
        ///       \     /
        ///        \   /
        ///         (2)
        /// 
        /// Result:
        /// 
        ///     [  0,100,100]
        ///     [100,  0,100]
        ///     [100,100,  0]
        ///     
        /// </remarks>
        [Test]
        public void TestThreeEdges()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(2, 0, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.AddContracted(VehicleMock.Car().Fastest());

            // run algorithm (0, 1, 2)->(0, 1, 2).
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run(); Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            var weights = algorithm.Weights;
            Assert.IsNotNull(weights);
            Assert.AreEqual(3, weights.Length);
            Assert.AreEqual(3, weights[0].Length);
            Assert.AreEqual(0, weights[0][0], 0.001);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, weights[0][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, weights[0][2], SecondsTolerance);
            Assert.AreEqual(3, weights[1].Length);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, weights[1][0], SecondsTolerance);
            Assert.AreEqual(0, weights[1][1], 0.001);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, weights[1][2], SecondsTolerance);
            Assert.AreEqual(3, weights[2].Length);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, weights[2][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, weights[2][1], SecondsTolerance);
            Assert.AreEqual(0, weights[2][2], 0.001);
        }

        /// <summary>
        /// Tests routing on a graph with two edges where the middle is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesMiddleHighest()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][0], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][1], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
        }

        /// <summary>
        /// Tests routing on a graph with two edges where the right is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesRightHighest()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][0], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][1], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
        }

        /// <summary>
        /// Tests routing on a graph with two edges where the left is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesLeftHighest()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][0], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][1], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
        }

        /// <summary>
        /// Tests routing on a graph with two oneway edges where the middle is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesDirectedMiddleHighest()
        {
            // build graph.
            var oneway = VehicleMock.Car(t => new FactorAndSpeed()
                {
                    Value = VehicleMock.Car().Fastest().FactorAndSpeed(null).Value,
                    SpeedFactor = VehicleMock.Car().Fastest().FactorAndSpeed(null).SpeedFactor,
                    Direction = 1
                }).Fastest();
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(oneway.Parent);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });


            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100, true, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100, false, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), oneway,
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[1][0]);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][0]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][1]);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
        }

        /// <summary>
        /// Tests routing on a graph with two oneway edges where the right is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesRightMiddleHighest()
        {
            // build graph.
            var oneway = VehicleMock.Car(t => new FactorAndSpeed()
            {
                Value = VehicleMock.Car().Fastest().FactorAndSpeed(null).Value,
                SpeedFactor = VehicleMock.Car().Fastest().FactorAndSpeed(null).Value,
                Direction = 1
            }).Fastest();
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(oneway.Parent);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, true, Constants.NO_VERTEX);
            graph.AddEdge(1, 2, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, true, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), oneway,
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[1][0]);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][0]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][1]);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
        }

        /// <summary>
        /// Tests routing on a graph with two oneway edges where the left is highest.
        /// </summary>
        [Test]
        public void TestTwoEdgesLeftMiddleHighest()
        {
            // build graph.
            var oneway = VehicleMock.Car(t => new FactorAndSpeed()
            {
                Value = VehicleMock.Car().Fastest().FactorAndSpeed(null).Value,
                SpeedFactor = VehicleMock.Car().Fastest().FactorAndSpeed(null).SpeedFactor,
                Direction = 1
            }).Fastest();
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(oneway.Parent);
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(1, 0, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, false, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, false, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), oneway,
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(3, algorithm.Weights.Length);
            Assert.AreEqual(3, algorithm.Weights[0].Length);
            Assert.AreEqual(3, algorithm.Weights[1].Length);
            Assert.AreEqual(3, algorithm.Weights[2].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[1][0]);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][0]);
            Assert.AreEqual(float.MaxValue, algorithm.Weights[2][1]);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
        }

        /// <summary>
        /// Tests some routes on a pentagon.
        /// </summary>
        [Test]
        public void TestPentagon()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 0, 0);
            routerDb.Network.AddVertex(1, 1, 1);
            routerDb.Network.AddVertex(2, 2, 2);
            routerDb.Network.AddVertex(3, 3, 3);
            routerDb.Network.AddVertex(4, 4, 4);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(1, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(2, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(3, 4, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });
            routerDb.Network.AddEdge(4, 0, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // build graph.
            var graph = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                ContractedEdgeDataSerializer.MetaSize);
            graph.AddEdge(0, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(0, 4, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 1, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(2, 3, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            graph.AddEdge(3, 1, 200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, 2);
            graph.AddEdge(4, 1, 200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, 0);
            graph.AddEdge(4, 3, 100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, null, Constants.NO_VERTEX);
            routerDb.AddContracted(VehicleMock.Car().Fastest(), new ContractedDb(graph, false));

            // create algorithm and run.
            var algorithm = new ManyToManyWeightsBidirectionalDykstra(new Router(routerDb), VehicleMock.Car().Fastest(),
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2),
                    routerDb.Network.CreateRouterPointForVertex(3),
                    routerDb.Network.CreateRouterPointForVertex(4)
                },
                new RouterPoint[] { 
                    routerDb.Network.CreateRouterPointForVertex(0),
                    routerDb.Network.CreateRouterPointForVertex(1),
                    routerDb.Network.CreateRouterPointForVertex(2),
                    routerDb.Network.CreateRouterPointForVertex(3),
                    routerDb.Network.CreateRouterPointForVertex(4)
                });
            algorithm.Run();

            // check results.
            Assert.IsTrue(algorithm.HasRun);
            Assert.IsTrue(algorithm.HasSucceeded);

            Assert.IsNotNull(algorithm.Weights);
            Assert.AreEqual(5, algorithm.Weights.Length);

            Assert.AreEqual(5, algorithm.Weights[0].Length);
            Assert.AreEqual(5, algorithm.Weights[1].Length);
            Assert.AreEqual(5, algorithm.Weights[2].Length);
            Assert.AreEqual(5, algorithm.Weights[3].Length);
            Assert.AreEqual(5, algorithm.Weights[4].Length);

            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][1], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][2], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][3], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[0][4], SecondsTolerance);

            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][0], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][1], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][2], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][3], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[1][4], SecondsTolerance);

            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][0], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][1], SecondsTolerance);
            Assert.AreEqual(000 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][2], SecondsTolerance);
            Assert.AreEqual(100 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][3], SecondsTolerance);
            Assert.AreEqual(200 * VehicleMock.Car().Fastest().FactorAndSpeed(null).Value, algorithm.Weights[2][4], SecondsTolerance);
        }
    }
}