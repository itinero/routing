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
using Itinero.Algorithms.Networks.Islands;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.Profiles;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Networks.Islands
{
    /// <summary>
    /// Contains tests for the island detector.
    /// </summary>
    [TestFixture]
    public class EdgeBasedIslandDetectorTests
    {
        /// <summary>
        /// Tests island detection on network 1 with profile pedestrian.
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork01_PedestrianIsConnected()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            });
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);

            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(0, labels[0]);
        }
        
        /// <summary>
        /// Tests island detection on network 2 with profile pedestrian.
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork02_PedestrianIsConnected()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            });
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);

            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(3, labels.Count);
            Assert.AreEqual(0, labels[0]);
            Assert.AreEqual(0, labels[1]);
            Assert.AreEqual(0, labels[2]);
        }
        
        /// <summary>
        /// Tests island detection on network 3 with profile pedestrian.
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork03_PedestrianIsConnected()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network3.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            });
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);

            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(10, labels.Count);
            for (uint i = 0; i < labels.Count; i++)
            {
                Assert.AreEqual(0, labels[i]);
            }
        }
        
        /// <summary>
        /// Tests island detection on network 3 with profile car.
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork03_CarIsConnected_WithNoAccess()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network3.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            });
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);

            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(10, labels.Count);
            Assert.AreEqual(0, labels[0]);
            Assert.AreEqual(IslandLabels.NoAccess, labels[1]); 
            Assert.AreEqual(2, labels[2]);
            Assert.AreEqual(2, labels[3]);
            Assert.AreEqual(4, labels[4]);
            Assert.AreEqual(4, labels[5]);
            Assert.AreEqual(4, labels[6]);
            Assert.AreEqual(4, labels[7]);
            Assert.AreEqual(4, labels[8]);
            Assert.AreEqual(4, labels[9]);
        }
        
        /// <summary>
        /// Tests island detection on network 19 with profile car.
        ///
        /// In this network has two islands are linked together with two oneways. 
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork19_CarIsConnected()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network19.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            });
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);

            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(4, labels.Count);
            Assert.AreEqual(0, labels[0]);
            Assert.AreEqual(0, labels[1]);
            Assert.AreEqual(0, labels[2]);
            Assert.AreEqual(0, labels[3]);
        }     
        
        /// <summary>
        /// Tests island detection on network 5 with profile car.
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork05_CarIsConnected()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network5.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            });
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);
            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(routerDb.Network.EdgeCount, labels.Count);
            Assert.AreEqual(0, labels[0]);
            Assert.AreEqual(IslandLabels.NoAccess, labels[1]);
            Assert.AreEqual(2, labels[2]);
            Assert.AreEqual(2, labels[3]);
            for (uint i = 4; i < labels.Count; i++)
            {
                Assert.AreEqual(4, labels[i]);
            }
        }
        
        /// <summary>
        /// Tests island detection on network 18 with profile car.
        /// </summary>
        [Test]
        public void TestEdgeBasedIslandNetwork18_CarIsConnected()
        {
            var routerDb = new RouterDb();
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network18.geojson"));
            routerDb.Sort();
            routerDb.Compress();
            var profile = Itinero.Osm.Vehicles.Vehicle.Car.Fastest();

            var islandDetector = new EdgeBasedIslandDetector(routerDb.Network, (p) =>
            {
                var edgeProfile = routerDb.EdgeProfiles.Get(p);
                return profile.Factor(edgeProfile);
            }, routerDb.GetRestrictions(profile));
            islandDetector.Run();
            
            Assert.IsTrue(islandDetector.HasRun);
            Assert.IsTrue(islandDetector.HasSucceeded);
            var labels = islandDetector.IslandLabels;
            Assert.IsNotNull(labels);
            Assert.AreEqual(routerDb.Network.EdgeCount, labels.Count);
            for (uint i = 0; i < 7; i++)
            {
                Assert.AreEqual(0, labels[i]);
            }
            for (uint i = 7; i < labels.Count; i++)
            {
                Assert.AreEqual(7, labels[i]);
            }
        }
    }
}