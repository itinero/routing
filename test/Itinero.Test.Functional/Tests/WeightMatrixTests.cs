// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Search;
using Itinero.LocalGeo;
using NUnit.Framework;
using System;
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

            GetTestWeightMatrix(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 200).TestPerf("Testing weight matrix");
            GetTestDirectedWeightMatrix(router, Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 100).TestPerf("Testing directed weight matrix");
        }

        /// <summary>
        /// Tests calculating a weight matrix.
        /// </summary>
        public static Action GetTestWeightMatrix(Router router, Profiles.Profile profile, int count)
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

            return () =>
            {
                var matrix = new Itinero.Algorithms.Matrices.WeightMatrixAlgorithm(router, profile, massResolver);
                matrix.Run();
                Assert.IsTrue(matrix.HasSucceeded);
            };
        }

        /// <summary>
        /// Tests calculating a directed weight matrix.
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

            return () =>
            {
                var matrix = new Itinero.Algorithms.Matrices.DirectedWeightMatrixAlgorithm(router, profile, massResolver);
                matrix.Run();
                Assert.IsTrue(matrix.HasSucceeded);
            };
        }
    }
}