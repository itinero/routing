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

using Itinero.LocalGeo;
using Itinero.Navigation.Directions;
using NUnit.Framework;

namespace Itinero.Test.LocalGeo
{
    /// <summary>
    /// Contains tests for the local simple coordinate class.
    /// </summary>
    [TestFixture]
    public class CoordinateTests
    {
        /// <summary>
        /// Tests the offset with direction.
        /// </summary>
        [Test]
        public void TestOffsetWithDirection()
        {
            var distance = 1000; // 1km
            var start = new Coordinate(53.32056f, 1.72972f);

            var offset = start.OffsetWithDirection(distance, DirectionEnum.North);
            Assert.AreEqual(53.32950, offset.Latitude, 0.0001);
            Assert.AreEqual(1.72970, offset.Longitude, 0.0001);

            offset = start.OffsetWithDirection(distance, DirectionEnum.NorthEast);
            Assert.AreEqual(53.32690, offset.Latitude, 0.0001);
            Assert.AreEqual(1.74040, offset.Longitude, 0.0001);

            start = new Coordinate(0, 0);

            offset = start.OffsetWithDirection(distance, DirectionEnum.West);
            Assert.AreEqual(0, offset.Latitude, 0.0001);
            Assert.AreEqual(-0.008984, offset.Longitude, 0.0001);

            offset = start.OffsetWithDirection(distance, DirectionEnum.East);
            Assert.AreEqual(0, offset.Latitude, 0.0001);
            Assert.AreEqual(0.008984, offset.Longitude, 0.0001);

            offset = start.OffsetWithDirection(distance, DirectionEnum.North);
            Assert.AreEqual(0.008896, offset.Latitude, 0.0001);
            Assert.AreEqual(0, offset.Longitude, 0.0001);

            offset = start.OffsetWithDirection(distance, DirectionEnum.South);
            Assert.AreEqual(-0.008896, offset.Latitude, 0.0001);
            Assert.AreEqual(0, offset.Longitude, 0.0001);
        }

        /// <summary>
        /// Tests the offset distance estimate.
        /// </summary>
        [Test]
        public void TestGeoCoordinateOffsetEstimate()
        {
            var coordinate = new Coordinate(51, 4.8f);

            var distance = 1000;

            var offset = coordinate.OffsetWithDistances(distance);
            var offsetLat = new Coordinate(offset.Latitude, coordinate.Longitude);
            var offsetlon = new Coordinate(coordinate.Latitude, offset.Longitude);

            var distanceLat = Coordinate.DistanceEstimateInMeter(offsetLat, coordinate);
            var distanceLon = Coordinate.DistanceEstimateInMeter(offsetlon, coordinate);

            Assert.AreEqual(distance, distanceLat, 0.3);
            Assert.AreEqual(distance, distanceLon, 0.3);
        }
    }
}