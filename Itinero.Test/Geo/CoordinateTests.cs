// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using Itinero.Geo;
using Itinero.Navigation.Directions;

namespace Itinero.Test.Geo
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