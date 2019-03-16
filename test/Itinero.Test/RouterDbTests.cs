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
using Itinero.LocalGeo;
using Itinero.Graphs.Directed;
using System.IO;
using Itinero.Attributes;
using System.Linq;
using Itinero.Data.Network.Restrictions;
using Itinero.Data.Contracted;
using System.Collections.Generic;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.Data;
using System;
using Itinero.Elevation;
using Itinero.LocalGeo.Elevation;
using Edge = NetTopologySuite.Planargraph.Edge;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the router db.
    /// </summary>
    [TestFixture]
    public class RouterDbTests
    {
        /// <summary>
        /// Tests testing for a supportest profile.
        /// </summary>
        [Test]
        public void TestSupportsProfile()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);

            Assert.IsTrue(routerDb.SupportProfile("car"));
            Assert.IsTrue(routerDb.SupportProfile("car.shortest"));
        }

        /// <summary>
        /// Tests saving and then loading test network1.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork1()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            // check serialized.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.22965768754021, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463152885437011, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.22961737711890, vertex1.Latitude, 0.00001);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 0.2);
            var edgeProfile = routerDb.EdgeProfiles.Get(
                edge.Data.Profile);
            Assert.IsTrue(edgeProfile.Contains("highway", "residential"));
            var edgeMeta = routerDb.EdgeMeta.Get(
                edge.Data.MetaId);
            Assert.IsTrue(edgeMeta.Contains("name", "Abelshausen Blvd."));
        }

        /// <summary>
        /// Tests saving and then loading test network2.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

            var vertex2 = routerDb.Network.GetVertex(2);
            Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
            Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

            var vertex3 = routerDb.Network.GetVertex(3);
            Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
            Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

            var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

            var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

            var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
        }

        /// <summary>
        /// Tests saving and then loading test network2 with elevation.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2WithElevation()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add elevation.
            ElevationHandler.GetElevation = (lat, lon) =>
            {
                return (short)(((lat * lon) % 2000) - 100);
            };
            routerDb.AddElevation();

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);
            Assert.AreEqual(ElevationHandler.GetElevation(vertex0.Latitude, vertex0.Longitude),
                vertex0.Elevation);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);
            Assert.AreEqual(ElevationHandler.GetElevation(vertex1.Latitude, vertex1.Longitude),
                vertex1.Elevation);

            var vertex2 = routerDb.Network.GetVertex(2);
            Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
            Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);
            Assert.AreEqual(ElevationHandler.GetElevation(vertex2.Latitude, vertex2.Longitude),
                vertex2.Elevation);

            var vertex3 = routerDb.Network.GetVertex(3);
            Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
            Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);
            Assert.AreEqual(ElevationHandler.GetElevation(vertex3.Latitude, vertex3.Longitude),
                vertex3.Elevation);

            var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

            var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

            var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, RouterDbProfile.NoCache);

                Assert.AreEqual(4, routerDb.Network.VertexCount);
                Assert.AreEqual(3, routerDb.Network.EdgeCount);

                vertex0 = routerDb.Network.GetVertex(0);
                Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
                Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);
                Assert.AreEqual(ElevationHandler.GetElevation(vertex0.Latitude, vertex0.Longitude),
                    vertex0.Elevation);

                vertex1 = routerDb.Network.GetVertex(1);
                Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
                Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);
                Assert.AreEqual(ElevationHandler.GetElevation(vertex1.Latitude, vertex1.Longitude),
                    vertex1.Elevation);

                vertex2 = routerDb.Network.GetVertex(2);
                Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
                Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);
                Assert.AreEqual(ElevationHandler.GetElevation(vertex2.Latitude, vertex2.Longitude),
                    vertex2.Elevation);

                vertex3 = routerDb.Network.GetVertex(3);
                Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
                Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);
                Assert.AreEqual(ElevationHandler.GetElevation(vertex3.Latitude, vertex3.Longitude),
                    vertex3.Elevation);

                edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

                edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

                edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
            }
        }

        /// <summary>
        /// Tests saving and then loading test network1 with a contracted graph.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork1ContractedCar()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network1.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            // check serialized.
            Assert.AreEqual(2, routerDb.Network.VertexCount);
            Assert.AreEqual(1, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.22965768754021, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463152885437011, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.22961737711890, vertex1.Latitude, 0.00001);

            var edge = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge.Data.Distance, 0.2);
            var edgeProfile = routerDb.EdgeProfiles.Get(
                edge.Data.Profile);
            Assert.IsTrue(edgeProfile.Contains("highway", "residential"));
            var edgeMeta = routerDb.EdgeMeta.Get(
                edge.Data.MetaId);
            Assert.IsTrue(edgeMeta.Contains("name", "Abelshausen Blvd."));

            ContractedDb contracted;
            Assert.IsTrue(routerDb.TryGetContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), out contracted));
        }

        /// <summary>
        /// Tests saving and then loading test network1 with a contracted graph.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2ContractedCar()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

            var vertex2 = routerDb.Network.GetVertex(2);
            Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
            Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

            var vertex3 = routerDb.Network.GetVertex(3);
            Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
            Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

            var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

            var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

            var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
        }

        /// <summary>
        /// Tests saving and then loading test network2 with two contracted graphs.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2ContractedMultiple()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Pedestrian);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

            var vertex2 = routerDb.Network.GetVertex(2);
            Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
            Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

            var vertex3 = routerDb.Network.GetVertex(3);
            Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
            Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

            var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

            var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

            var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
        }

        /// <summary>
        /// Tests saving and then loading test network2 with two contracted graphs using the default profile.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2ContractedMultipleDefaultProfile()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Pedestrian);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, RouterDbProfile.Default);

                Assert.AreEqual(4, routerDb.Network.VertexCount);
                Assert.AreEqual(3, routerDb.Network.EdgeCount);

                var vertex0 = routerDb.Network.GetVertex(0);
                Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
                Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

                var vertex1 = routerDb.Network.GetVertex(1);
                Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
                Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

                var vertex2 = routerDb.Network.GetVertex(2);
                Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
                Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

                var vertex3 = routerDb.Network.GetVertex(3);
                Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
                Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

                var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

                var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

                var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
            }
        }

        /// <summary>
        /// Tests saving and then loading test network2 with two contracted graphs using the default profile.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2ContractedMultipleNoCacheProfile()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Pedestrian);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, RouterDbProfile.NoCache);

                Assert.AreEqual(4, routerDb.Network.VertexCount);
                Assert.AreEqual(3, routerDb.Network.EdgeCount);

                var vertex0 = routerDb.Network.GetVertex(0);
                Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
                Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

                var vertex1 = routerDb.Network.GetVertex(1);
                Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
                Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

                var vertex2 = routerDb.Network.GetVertex(2);
                Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
                Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

                var vertex3 = routerDb.Network.GetVertex(3);
                Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
                Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

                var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);

                var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);

                var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
                Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
            }
        }

        /// <summary>
        /// Tests saving and then loading test network2 with some tags describing what the network is about.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2Meta()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Pedestrian);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Pedestrian.Fastest());

            // add meta-data.
            routerDb.Meta.AddOrReplace("name", "test-network-2");
            routerDb.Meta.AddOrReplace("date", "30-11-2015");

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, RouterDbProfile.NoCache);

                Assert.IsTrue(routerDb.Meta.Contains("name", "test-network-2"));
                Assert.IsTrue(routerDb.Meta.Contains("date", "30-11-2015"));
            }
        }

        /// <summary>
        /// Tests saving and then loading a contracted network only.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork2ContractedCarOnly()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network2.geojson"));

            // add contracted version.
            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());

            using (var stream = new MemoryStream())
            {
                routerDb.SerializeContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), stream);
                routerDb.RemoveContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest());
                Assert.IsFalse(routerDb.HasContractedFor(Itinero.Osm.Vehicles.Vehicle.Car.Fastest()));
                stream.Seek(0, SeekOrigin.Begin);
                routerDb.DeserializeAndAddContracted(stream);
                Assert.IsTrue(routerDb.HasContractedFor(Itinero.Osm.Vehicles.Vehicle.Car.Fastest()));

                // create a new db.
                routerDb = new RouterDb();
                routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
                routerDb.LoadTestNetwork(
                    System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                        "Itinero.Test.test_data.networks.network2.geojson"));

                stream.Seek(0, SeekOrigin.Begin);
                Assert.Catch(() =>
                    {
                        routerDb.DeserializeAndAddContracted(stream);
                    });
            }
        }

        /// <summary>
        /// Tests saving and then loading test network4.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork4()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network4.geojson"));

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            var vertex0 = routerDb.Network.GetVertex(0);
            Assert.AreEqual(4.460974931716918, vertex0.Longitude, 0.00001);
            Assert.AreEqual(51.2296492895387, vertex0.Latitude, 0.00001);

            var vertex1 = routerDb.Network.GetVertex(1);
            Assert.AreEqual(4.463168978691101, vertex1.Longitude, 0.00001);
            Assert.AreEqual(51.2296224159235, vertex1.Latitude, 0.00001);

            var vertex2 = routerDb.Network.GetVertex(2);
            Assert.AreEqual(4.465247690677643, vertex2.Longitude, 0.00001);
            Assert.AreEqual(51.22962073632204, vertex2.Latitude, 0.00001);

            var vertex3 = routerDb.Network.GetVertex(3);
            Assert.AreEqual(4.46317434310913, vertex3.Longitude, 0.00001);
            Assert.AreEqual(51.23092072952097, vertex3.Latitude, 0.00001);

            var edge1 = routerDb.Network.GetEdgeEnumerator(0).First(x => x.To == 1);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex0, vertex1), edge1.Data.Distance, 1);
            Assert.IsNotNull(edge1.Shape);
            var shape = new List<Coordinate>(edge1.Shape);
            Assert.AreEqual(1, shape.Count);
            Assert.AreEqual(4.462069272994995, shape[0].Longitude, 0.00001);
            Assert.AreEqual(51.22964425073703, shape[0].Latitude, 0.00001);

            var edge2 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 2);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex2), edge2.Data.Distance, 1);
            Assert.IsNotNull(edge2.Shape);
            shape = new List<Coordinate>(edge2.Shape);
            Assert.AreEqual(1, shape.Count);
            Assert.AreEqual(4.46420431137085, shape[0].Longitude, 0.00001);
            Assert.AreEqual(51.229610658711934, shape[0].Latitude, 0.00001);

            var edge3 = routerDb.Network.GetEdgeEnumerator(1).First(x => x.To == 3);
            Assert.AreEqual(Coordinate.DistanceEstimateInMeter(vertex1, vertex3), edge3.Data.Distance, 1);
            Assert.IsNotNull(edge3.Shape);
            shape = new List<Coordinate>(edge3.Shape);
            Assert.AreEqual(1, shape.Count);
            Assert.AreEqual(4.46317434310913, shape[0].Longitude, 0.00001);
            Assert.AreEqual(51.23026905793433, shape[0].Latitude, 0.00001);
        }

        /// <summary>
        /// Tests adding a restriction db.
        /// </summary>
        [Test]
        public void TestAddRestrictionDb()
        {
            var routerDb = new RouterDb();
            var restrictions = new RestrictionsDb();
            routerDb.AddRestrictions("vehicle", restrictions);

            Assert.IsTrue(routerDb.TryGetRestrictions("vehicle", out restrictions));
        }

        /// <summary>
        /// Tests saving and then loading test network6.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork6()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network6.geojson"));
            routerDb.Sort();

            var json = routerDb.GetGeoJsonAround(1, 10, false, true, false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.791842,51.26816]},\"properties\":{\"id\":1,\"node_id\":6}}]}",
                json);

            json = routerDb.GetGeoJsonAround(4, 10, true, true, false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.790533,51.26566]},\"properties\":{\"id\":4,\"node_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.787357,51.26565],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":0,\"vertex1\":7,\"vertex2\":4,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.790533,51.26566],[4.791756,51.26695]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":2,\"vertex1\":4,\"vertex2\":3,\"edge_id\":5}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}}]}", 
                json);
        }
        
        /// <summary>
        /// Tests saving and then loading test network4.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork4AndVertexData()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network4.geojson"));

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            MetaCollection<long> nodeIds;
            Assert.IsTrue(routerDb.VertexData.TryGet("node_id", out nodeIds));
            Assert.AreEqual(4, nodeIds.Count);
        }
        
        /// <summary>
        /// Tests saving and then loading test network4.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork4AndEdgeData()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network4.geojson"));

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }

            Assert.AreEqual(4, routerDb.Network.VertexCount);
            Assert.AreEqual(3, routerDb.Network.EdgeCount);

            MetaCollection<long> edgeIds;
            Assert.IsTrue(routerDb.EdgeData.TryGet("edge_id", out edgeIds));
            Assert.AreEqual(3, edgeIds.Count);
        }

        /// <summary>
        /// Tests saving and then loading test network6 and then checking if it's writable.
        /// </summary>
        [Test]
        public void TestSaveLoadNetwork6WritableAfterSerialization()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network6.geojson"));
            routerDb.Sort();

            using (var stream = new MemoryStream())
            {
                routerDb.Serialize(stream);
                stream.Seek(0, SeekOrigin.Begin);
                routerDb = RouterDb.Deserialize(stream, null);
            }
            
            // try adding things that already exist.
            for (uint i = 1; i < routerDb.EdgeProfiles.Count; i++)
            {
                var attributes = routerDb.EdgeProfiles.Get(i);
                var profileId = routerDb.EdgeProfiles.Add(attributes);
                Assert.AreEqual(i, profileId);
                var profile = routerDb.EdgeProfiles.Get(profileId);
                Assert.IsNotNull(profile);
                Assert.AreEqual(attributes, profile);
            }

            // try adding new ones.
            for (var i = 0; i < 4096; i++)
            {
                var attributes = new AttributeCollection(
                    new Itinero.Attributes.Attribute("highway", $"unknown_{i}"));
                var profileId = routerDb.EdgeProfiles.Add(attributes);
                var profile = routerDb.EdgeProfiles.Get(profileId);
                Assert.IsNotNull(profile);
                Assert.AreEqual(attributes, profile);
            }

            // try adding them a second time.
            for (var i = 0; i < 4096; i++)
            {
                var attributes = new AttributeCollection(
                    new Itinero.Attributes.Attribute("highway", $"unknown_{i}"));
                var profileId = routerDb.EdgeProfiles.Add(attributes);
                var profile = routerDb.EdgeProfiles.Get(profileId);
                Assert.IsNotNull(profile);
                Assert.AreEqual(attributes, profile);
            }

            // try adding new ones.
            for (var i = 0; i < 4096; i++)
            {
                var attributes = new AttributeCollection(
                    new Itinero.Attributes.Attribute("name", $"Some roadname {i}, never ever used before!"));
                var metaId = routerDb.EdgeMeta.Add(attributes);
                var meta = routerDb.EdgeMeta.Get(metaId);
                Assert.IsNotNull(meta);
                Assert.AreEqual(attributes, meta);
            }
            
            // try adding them a second time.
            for (var i = 0; i < 4096; i++)
            {
                var attributes = new AttributeCollection(
                    new Itinero.Attributes.Attribute("name", $"Some roadname {i}, never ever used before!"));
                var metaId = routerDb.EdgeMeta.Add(attributes);
                var meta = routerDb.EdgeMeta.Get(metaId);
                Assert.IsNotNull(meta);
                Assert.AreEqual(attributes, meta);
            }
        }

        /// <summary>
        /// Tests getting geojson for edges.
        /// </summary>
        [Test]
        public void TestGetGeoJsonEdges()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network6.geojson"));
            routerDb.Sort();
            
            var router = new Router(routerDb);
            var resolved = router.Resolve(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 51.26568779210525f, 4.793654680252075f);

            var json = routerDb.GetGeoJsonEdges(edgeIds: resolved.EdgeId, includeProfileDetails: false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.796669,51.2657]},\"properties\":{\"id\":5,\"node_id\":2}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.790533,51.26566]},\"properties\":{\"id\":4,\"node_id\":1}}]}",
                json);
            
            json = routerDb.GetGeoJsonEdges(edgeIds: resolved.EdgeId, includeVertices: false, includeProfileDetails: false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}}]}",
                json);
            
            json = routerDb.GetGeoJsonEdges(edgeIds: resolved.EdgeId, neighbours: true, includeProfileDetails: false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.79948,51.2657]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":1,\"vertex1\":5,\"vertex2\":6,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.795725,51.26694],[4.796669,51.2657]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":4,\"vertex1\":2,\"vertex2\":5,\"edge_id\":7}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.787357,51.26565],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":0,\"vertex1\":7,\"vertex2\":4,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.790533,51.26566],[4.791756,51.26695]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":2,\"vertex1\":4,\"vertex2\":3,\"edge_id\":5}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.796669,51.2657]},\"properties\":{\"id\":5,\"node_id\":2}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.790533,51.26566]},\"properties\":{\"id\":4,\"node_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.79948,51.2657]},\"properties\":{\"id\":6,\"node_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.795725,51.26694]},\"properties\":{\"id\":2,\"node_id\":5}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.787357,51.26565]},\"properties\":{\"id\":7,\"node_id\":0}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.791756,51.26695]},\"properties\":{\"id\":3,\"node_id\":4}}]}",
                json);
            
            json = routerDb.GetGeoJsonEdges(edgeIds: resolved.EdgeId, neighbours: true, includeVertices: false, includeProfileDetails: false);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.79948,51.2657]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":1,\"vertex1\":5,\"vertex2\":6,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.795725,51.26694],[4.796669,51.2657]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":4,\"vertex1\":2,\"vertex2\":5,\"edge_id\":7}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.787357,51.26565],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":0,\"vertex1\":7,\"vertex2\":4,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.790533,51.26566],[4.791756,51.26695]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":2,\"vertex1\":4,\"vertex2\":3,\"edge_id\":5}}]}", 
                json);
        }

        /// <summary>
        /// Tests getting geojson for vertices.
        /// </summary>
        [Test]
        public void TestGetGeoJsonVertices()
        {
            var routerDb = new RouterDb();
            routerDb.AddSupportedVehicle(Itinero.Osm.Vehicles.Vehicle.Car);
            routerDb.LoadTestNetwork(
                System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.networks.network6.geojson"));
            routerDb.Sort();
            
            var router = new Router(routerDb);
            var resolved = router.Resolve(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 51.26568779210525f, 4.793654680252075f);
            var edge = routerDb.Network.GetEdge(resolved.EdgeId);

            var json = routerDb.GetGeoJsonVertices(vertexIds: edge.From);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.796669,51.2657]},\"properties\":{\"id\":5,\"node_id\":2}}]}", 
                json);
            
            json = routerDb.GetGeoJsonVertices(vertexIds: new [] { edge.From, edge.To });
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.796669,51.2657]},\"properties\":{\"id\":5,\"node_id\":2}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.790533,51.26566]},\"properties\":{\"id\":4,\"node_id\":1}}]}", 
                json);
            
            json = routerDb.GetGeoJsonVertices(vertexIds: new [] { edge.From, edge.To }, neighbours: true);
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.79948,51.2657]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":1,\"vertex1\":5,\"vertex2\":6,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.795725,51.26694],[4.796669,51.2657]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":4,\"vertex1\":2,\"vertex2\":5,\"edge_id\":7}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.796669,51.2657],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":5,\"vertex1\":5,\"vertex2\":4,\"edge_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.787357,51.26565],[4.790533,51.26566]]},\"properties\":{\"highway\":\"residential\",\"edgeid\":0,\"vertex1\":7,\"vertex2\":4,\"edge_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"LineString\",\"coordinates\":[[4.790533,51.26566],[4.791756,51.26695]]},\"properties\":{\"highway\":\"residential\",\"oneway\":\"yes\",\"edgeid\":2,\"vertex1\":4,\"vertex2\":3,\"edge_id\":5}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.796669,51.2657]},\"properties\":{\"id\":5,\"node_id\":2}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.79948,51.2657]},\"properties\":{\"id\":6,\"node_id\":3}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.795725,51.26694]},\"properties\":{\"id\":2,\"node_id\":5}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.790533,51.26566]},\"properties\":{\"id\":4,\"node_id\":1}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.787357,51.26565]},\"properties\":{\"id\":7,\"node_id\":0}},{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[4.791756,51.26695]},\"properties\":{\"id\":3,\"node_id\":4}}]}",
                json);
        }
    }
}