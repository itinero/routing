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
using Itinero.Algorithms.Networks;

namespace Itinero.Test.Algorithms.Networks
{
    /// <summary>
    /// Contains tests for the network routerdb extensions.
    /// </summary>
    [TestFixture]
    public class RouterDbExtensionTests
    {
        /// <summary>
        /// Tests optimizing network 9.
        /// </summary>
        [Test]
        public void TestOptimizeNetwork()
        {
            // build and load network.
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network9.geojson"));

            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            // optimize.
            Assert.AreEqual(10, routerDb.Network.EdgeCount);
            routerDb.OptimizeNetwork();
            routerDb.Network.Compress();
            Assert.AreEqual(6, routerDb.Network.EdgeCount);
        }
    }
}