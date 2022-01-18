using Itinero.Algorithms.Default.EdgeBased;
using Itinero.Data.Network;
using Itinero.Test.Profiles;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Default.EdgeBased
{
    [TestFixture]
    public class OneHopRouterTests
    {
        [Test]
        public void OneHopRouter_OneEdge_FromToOnEdge_ForwardForward_OneHopPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), true, 
                new RouterPoint(1, 1, 0, ushort.MaxValue), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(1, algorithm.Result.Vertex);
            Assert.AreEqual(1, algorithm.Result.Edge);
            Assert.NotNull(algorithm.Result.From);
            Assert.AreEqual(0, algorithm.Result.From.Vertex);
            Assert.Null(algorithm.Result.From.From);
        }
        
        [Test]
        public void OneHopRouter_OneEdge_FromToOnEdge_ForwardBackward_NoPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), true, 
                new RouterPoint(1, 1, 0, ushort.MaxValue), false);
            algorithm.Run();
            
            Assert.Null(algorithm.Result);
        }
        
        [Test]
        public void OneHopRouter_OneEdge_FromToOnEdge_BackwardForward_NoPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), false, 
                new RouterPoint(1, 1, 0, ushort.MaxValue), true);
            algorithm.Run();
            
            Assert.Null(algorithm.Result);
        }
        
        [Test]
        public void OneHopRouter_OneEdge_FromToOnEdge_BackwardBackward_NoPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), false, 
                new RouterPoint(1, 1, 0, ushort.MaxValue), false);
            algorithm.Run();
            
            Assert.Null(algorithm.Result);
        }
        
        [Test]
        public void OneHopRouter_OneEdge_FromAndFrom_SameEdge_ForwardForward_OneVertexPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), true, 
                new RouterPoint(0, 0, 0, 0), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(0, algorithm.Result.Vertex);
            Assert.Null(algorithm.Result.From);
        }
        
        [Test]
        public void OneHopRouter_OneEdge_FromAndFrom_SameEdge_ForwardBackward_NoPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), true, 
                new RouterPoint(0, 0, 0, 0), false);
            algorithm.Run();
            
            Assert.Null(algorithm.Result);
        }
        
        [Test]
        public void OneHopRouter_ThreeEdges_FromAndFrom_DifferentEdge_ForwardForward_OneVertexPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, edge0, 0), true, 
                new RouterPoint(0, 0, edge1, 0), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(0, algorithm.Result.Vertex);
            Assert.Null(algorithm.Result.From);
        }
        
        [Test]
        public void OneHopRouter_ThreeEdges_FromAndFrom_DifferentEdge_ForwardBackward_NoPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, edge0, 0), true, 
                new RouterPoint(0, 0, edge1, 0), false);
            algorithm.Run();
            
            Assert.Null(algorithm.Result);
        }
        
        [Test]
        public void OneHopRouter_ThreeEdges_FromToOnEdge_ForwardForward_OneHopPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, 0, 0), true,
                new RouterPoint(1, 1, 0, ushort.MaxValue), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(1, algorithm.Result.Vertex);
            Assert.AreEqual(1, algorithm.Result.Edge);
            Assert.NotNull(algorithm.Result.From);
            Assert.AreEqual(0, algorithm.Result.From.Vertex);
            Assert.Null(algorithm.Result.From.From);
        }
        
        [Test]
        public void OneHopRouter_ThreeEdges_FromToOnEdgeReverse_BackwardBackward_OneHopPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(1, 1, 0, ushort.MaxValue), true,
                new RouterPoint(0, 0, 0, 0), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(0, algorithm.Result.Vertex);
            Assert.AreEqual(-1, algorithm.Result.Edge);
            Assert.NotNull(algorithm.Result.From);
            Assert.AreEqual(1, algorithm.Result.From.Vertex);
            Assert.Null(algorithm.Result.From.From);
        }
        
        [Test]
        public void OneHopRouter_ThreeEdges_FromToNotOnEdge_ForwardForward_OneHopPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, edge1, 0), true,
                new RouterPoint(1, 1, edge2, 0), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(1, algorithm.Result.Vertex);
            Assert.AreEqual(1, algorithm.Result.Edge);
            Assert.NotNull(algorithm.Result.From);
            Assert.AreEqual(0, algorithm.Result.From.Vertex);
            Assert.Null(algorithm.Result.From.From);
        }
        
        [Test]
        public void OneHopRouter_ThreeEdges_FromNotOnEdge_ToNotVertexe_ForwardForwar_OneHopPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, edge1, 0), true,
                new RouterPoint(1, 1, edge0, ushort.MaxValue / 2), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(Constants.NO_VERTEX, algorithm.Result.Vertex);
            Assert.AreEqual(1, algorithm.Result.Edge);
            Assert.NotNull(algorithm.Result.From);
            Assert.AreEqual(0, algorithm.Result.From.Vertex);
            Assert.Null(algorithm.Result.From.From);
        }
        
        /// <summary>
        /// Tests shortest path calculations on just one edge with start half way on the edge and target a vertex resolved on another edge.
        /// </summary>
        /// <remarks>
        /// Situation:
        ///  (2)          (3)
        ///   |            |
        ///  (0)---100m---(1) @ 100km/h
        /// </remarks>
        [Test]
        public void OneHopRouter_ThreeEdges_FromNotVertex_ToNotOnEdgee_ForwardForwar_OneHopPath()
        {
            // build graph.
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(VehicleMock.Car());
            routerDb.Network.AddVertex(0, 51.268211863085995f, 4.793214797973633f);
            routerDb.Network.AddVertex(1, 51.268602880297650f, 4.794494211673737f);
            routerDb.Network.AddVertex(2, 51.269416789574144f, 4.793858528137207f);
            routerDb.Network.AddVertex(3, 51.269022422962060f, 4.792589843273162f);
                
            var edge0 = routerDb.Network.AddEdge(0, 1, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge1 = routerDb.Network.AddEdge(0, 2, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });    
            var edge2 = routerDb.Network.AddEdge(1, 3, new Itinero.Data.Network.Edges.EdgeData()
            {
                Distance = 100,
                Profile = 0,
                MetaId = 0
            });

            // run algorithm.
            var router = new Router(routerDb);
            var profile = VehicleMock.Car().Fastest();
            var weightHandler = router.GetDefaultWeightHandler(profile);
            var algorithm = new OneHopRouter<float>(routerDb, profile, weightHandler, 
                new RouterPoint(0, 0, edge0, ushort.MaxValue / 2), true,
                new RouterPoint(1, 1, edge2, 0), true);
            algorithm.Run();
            
            Assert.NotNull(algorithm.Result);
            Assert.AreEqual(1, algorithm.Result.Vertex);
            Assert.AreEqual(1, algorithm.Result.Edge);
            Assert.NotNull(algorithm.Result.From);
            Assert.AreEqual(Constants.NO_VERTEX, algorithm.Result.From.Vertex);
            Assert.Null(algorithm.Result.From.From);
        }
    }
}