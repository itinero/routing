using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Itinero.Test.Functional.Tests
{
    public static class InstructionTests
    {
        /// <summary>
        /// Executes generating instructions on the given routerdb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);

            GetTestInstructionGenerationParallel(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 25).TestPerf("Routing instructions parallel");
        }
        
        /// <summary>
        /// Tests calculating a number of routes.
        /// </summary>
        public static Action GetTestInstructionGenerationParallel(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();

            var routes = new List<Route>();
            while(routes.Count < count)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var v2 = (uint)random.Next((int)router.Db.Network.VertexCount - 1);
                if (v1 == v2)
                {
                    v2++;
                }

                var f1 = router.Db.Network.GetVertex(v1);
                var f2 = router.Db.Network.GetVertex(v2);

                var route = router.TryCalculate(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), f1, f2);
                if (!route.IsError)
                {
                    routes.Add(route.Value);
                }
            }

            return () =>
            {
                System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
                {
                    var instructions = routes[x].GenerateInstructions();
                });
            };
        }
    }
}
