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
            
            TestWeightMatrix(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
            TestWeightMatrix(router, Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());
            TestWeightMatrix(router, Itinero.Osm.Vehicles.Vehicle.Bicycle.Fastest(), 20, 20);
            TestWeightMatrix(router, Itinero.Osm.Vehicles.Vehicle.BigTruck.Fastest(), 20, 20);
        }
        
        public static void TestWeightMatrix(Router router, Profiles.Profile profile, int step = 250, int max = 1000)
        {
            for (var count = step; count <= max; count += step)
            {
                GetTestWeightMatrix(router, profile, count)
                    .TestPerf($"Testing {profile.FullName} " +
                              $"({(router.Db.HasContractedFor(profile) ? "contracted" : "uncontracted")}) {count}x{count} weight matrix");
                GetTestDirectedWeightMatrix(router, profile, count)
                    .TestPerf($"Testing {profile.FullName} " +
                              $"({(router.Db.HasContractedFor(profile) ? "contracted" : "uncontracted")}) {count}x{count}x4 directed weight matrix");
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
        public static Action GetTestDirectedWeightMatrix(Router router, Profiles.Profile profile, int count)
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