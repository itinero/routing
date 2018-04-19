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

using System;
using Itinero.Data.Network;
using Itinero.LocalGeo;

namespace Itinero.Test
{
    /// <summary>
    /// Contains extension methods for tests.
    /// </summary>
    public static class TestExtensions
    {
        private static Random _random = new Random();

        /// <summary>
        /// Generates a random coordinate in the given box.
        /// </summary>
        public static Coordinate GenerateRandomIn(this Box box)
        {
            var xNext = (float) _random.NextDouble();
            var yNext = (float) _random.NextDouble();

            return new Coordinate(box.MinLat + (box.MaxLat - box.MinLat) * xNext,
                box.MinLon + (box.MaxLon - box.MinLon) * yNext);
        }

        /// <summary>
        /// Adds a test edge.
        /// </summary>
        public static uint AddTestEdge(this RouterDb routerDb, float latitude1, float longitude1,
            float latitude2, float longitude2)
        {
            var vertex1 = routerDb.Network.VertexCount;
            routerDb.Network.AddVertex(vertex1, latitude1, longitude1);
            var vertex2 = routerDb.Network.VertexCount;
            routerDb.Network.AddVertex(vertex2, latitude2, longitude2);

            return routerDb.Network.AddEdge(vertex1, vertex2, 
                new Itinero.Data.Network.Edges.EdgeData()
                {
                    Distance = Coordinate.DistanceEstimateInMeter(latitude1, longitude1, latitude2, longitude2),
                    Profile = 0,
                    MetaId = 0
                });
        }
    }
}