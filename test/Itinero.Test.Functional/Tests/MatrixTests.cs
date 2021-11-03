using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Algorithms.Search;
using Itinero.LocalGeo;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests calculating route matrices.
    /// </summary>
    public static class MatrixTests
    { 
        /// <summary>
        /// Runs matrix calculation tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);
            
            TestMatrix(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
            TestMatrix(router, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            TestMatrix(router, Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest());
            TestMatrix(router, Itinero.Osm.Vehicles.Vehicle.BigTruck.Fastest());
        }

        public static void TestMatrix(Router router, Profiles.Profile profile)
        {
            for (var count = 20; count <= 20; count += 20)
            {
                GetTestMatrix(router, profile, count)
                    .TestPerf($"Testing {profile.FullName} " +
                              $"({(router.Db.HasContractedFor(profile) ? "contracted" : "uncontracted")}) {count}x{count} matrix");
            }
        }

        /// <summary>
        /// Tests calculating a weight matrix.
        /// </summary>
        public static Action GetTestMatrix(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random(145171654);
            var vertices = new HashSet<uint>();
            var locations = new List<Coordinate>();
            while (locations.Count < count)
            {
                var v = (uint)random.Next((int)router.Db.Network.VertexCount);
                if (!vertices.Contains(v))
                {
                    vertices.Add(v);
                    locations.Add(router.Db.Network.GetVertex(v));
                }
            }

            var locationsArray = locations.ToArray();
            var massResolver = new MassResolvingAlgorithm(router, new Profiles.IProfileInstance[] { profile }, locationsArray);
            massResolver.Run();
            
            // remove all unconnected routerpoints, we are only testing the matrix calculation.
            var connectedPoints = massResolver.RouterPoints.Where(x => 
                router.CheckConnectivity(profile, x)).ToArray();
            
            var resolved = connectedPoints;
            return () =>
            {
                var routes = router.TryCalculate(profile, resolved, resolved);

                if (routes.IsError) throw new Exception("Test ran incorrectly");
            };
        }
    }
}