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

using Itinero.Algorithms;
using Itinero.Algorithms.Search;
using Itinero.LocalGeo;
using System;
using Itinero;
using System.Collections.Generic;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains tests calculating weight matrices.
    /// </summary>
    public static class WeightMatrixTests
    {
        /// <summary>
        /// Runs weight matrix calculation tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);

            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();
            var max = 7200;
            for (var count = 250; count <= 1000; count += 250)
            {
                var random = new System.Random(145171654);
                var vertices = new HashSet<uint>();
                var locations = new List<Coordinate>();
                while (locations.Count < count)
                {
                    var v = (uint) random.Next((int) router.Db.Network.VertexCount);
                    if (!vertices.Contains(v))
                    {
                        vertices.Add(v);
                        locations.Add(router.Db.Network.GetVertex(v));
                    }
                }

                var locationsArray = locations.ToArray();
                var massResolver =
                    new MassResolvingAlgorithm(router, new Profiles.IProfileInstance[] {profile}, locationsArray);
                massResolver.Run();

                GetTestWeightMatrixAlgorithm(router, profile, massResolver, max)
                    .TestPerf($"Testing {profile.FullName} {count}x{count} matrix");
                GetTestDirectedWeightMatrix(router, profile, massResolver, max)
                    .TestPerf($"Testing {profile.FullName} {count}x{count} directed matrix");
            }

            profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();
            for (var count = 250; count <= 1000; count += 250)
            {
                var random = new System.Random(145171654);
                var vertices = new HashSet<uint>();
                var locations = new List<Coordinate>();
                while (locations.Count < count)
                {
                    var v = (uint) random.Next((int) router.Db.Network.VertexCount);
                    if (!vertices.Contains(v))
                    {
                        vertices.Add(v);
                        locations.Add(router.Db.Network.GetVertex(v));
                    }
                }

                var locationsArray = locations.ToArray();
                var massResolver =
                    new MassResolvingAlgorithm(router, new Profiles.IProfileInstance[] {profile}, locationsArray);
                massResolver.Run();

                GetTestWeightMatrixAlgorithm(router, profile, massResolver, max)
                    .TestPerf($"Testing {profile.FullName} {count}x{count} matrix");
            }

            profile = Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest();
            for (var count = 50; count <= 200; count += 50)
            {
                var random = new System.Random(145171654);
                var vertices = new HashSet<uint>();
                var locations = new List<Coordinate>();
                while (locations.Count < count)
                {
                    var v = (uint) random.Next((int) router.Db.Network.VertexCount);
                    if (!vertices.Contains(v))
                    {
                        vertices.Add(v);
                        locations.Add(router.Db.Network.GetVertex(v));
                    }
                }
            
                var locationsArray = locations.ToArray();
                var massResolver =
                    new MassResolvingAlgorithm(router, new Profiles.IProfileInstance[] {profile}, locationsArray);
                massResolver.Run();
            
                GetTestWeightMatrixAlgorithm(router, profile, massResolver, max)
                    .TestPerf($"Testing {profile.FullName} (uncontracted) {count}x{count} matrix");
            }

            profile = Itinero.Osm.Vehicles.Vehicle.BigTruck.Fastest();
            for (var count = 50; count <= 200; count += 50)
            {
                var random = new System.Random(145171654);
                var vertices = new HashSet<uint>();
                var locations = new List<Coordinate>();
                while (locations.Count < count)
                {
                    var v = (uint) random.Next((int) router.Db.Network.VertexCount);
                    if (!vertices.Contains(v))
                    {
                        vertices.Add(v);
                        locations.Add(router.Db.Network.GetVertex(v));
                    }
                }

                var locationsArray = locations.ToArray();
                var massResolver =
                    new MassResolvingAlgorithm(router, new Profiles.IProfileInstance[] {profile}, locationsArray);
                massResolver.Run();

                GetTestWeightMatrixAlgorithm(router, profile, massResolver, max)
                    .TestPerf($"Testing {profile.FullName} (uncontracted) {count}x{count} matrix");
            }
        }

        /// <summary>
        /// Tests calculating a weight matrix.
        /// </summary>
        public static Action GetTestWeightMatrix(Router router, Profiles.Profile profile, int count, float max = 7200)
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

            var settings = new RoutingSettings<float>()
            {
                //Cache = cache
            };
            settings.SetMaxSearch(profile.FullName, max);
            return () =>
            {
                var invalids = new HashSet<int>();
                var weights = router.CalculateWeight(profile, massResolver.RouterPoints.ToArray(), invalids,
                    settings);
            };
        }

        /// <summary>
        /// Tests calculating a weight matrix.
        /// </summary>
        public static Action GetTestWeightMatrix(Router router, Profiles.Profile profile, MassResolvingAlgorithm massResolver, float max = 7200)
        {
            var settings = new RoutingSettings<float>()
            {
                //Cache = cache
            };
            settings.SetMaxSearch(profile.FullName, max);
            return () =>
            {
                var invalids = new HashSet<int>();
                var weights = router.CalculateWeight(profile, massResolver.RouterPoints.ToArray(), invalids,
                    settings);
            };
        }
        
        /// <summary>
        /// Tests calculating a weight matrix.
        /// </summary>
        public static Action GetTestWeightMatrixAlgorithm(Router router, Profiles.Profile profile, MassResolvingAlgorithm massResolver, float max = 7200)
        {
            var settings = new RoutingSettings<float>()
            {
                //Cache = cache
            };
            settings.SetMaxSearch(profile.FullName, max);
            return () =>
            {
                var matrix = new Itinero.Algorithms.Matrices.WeightMatrixAlgorithm(router, profile, massResolver, settings);
                matrix.Run();
                Assert.IsTrue(matrix.HasSucceeded);
            };
        }

        /// <summary>
        /// Tests calculating a weight matrix.
        /// </summary>
        public static Action GetTestDirectedWeightMatrix(Router router, Profiles.Profile profile, MassResolvingAlgorithm massResolver, int count)
        {
            var edges = new List<DirectedEdgeId>();
            foreach(var resolved in massResolver.RouterPoints)
            {
                edges.Add(new DirectedEdgeId(resolved.EdgeId, true));
                edges.Add(new DirectedEdgeId(resolved.EdgeId, false));
            }

            return () =>
            {
                var result = router.TryCalculateWeight(profile, edges.ToArray(), edges.ToArray(), null);
            };
        }
    }
}