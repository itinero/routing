// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using OsmSharp.Routing.Geo;
using OsmSharp.Routing.Navigation.Directions;

namespace OsmSharp.Routing.Test.Navigation.Directions
{
    /// <summary>
    /// Contains tests for the tools.
    /// </summary>
    [TestFixture]
    class DirectionCalculatorTests
    {
        /// <summary>
        /// Tests calculating relative directions.
        /// </summary>
        [Test]
        public void RelativeDirectionTest()
        {
            var direction = DirectionCalculator.Calculate(new Coordinate(0, 1), 
                new Coordinate(0, 0), new Coordinate(1, 0));
            Assert.AreEqual(90, direction.Angle, 0.0001);
            Assert.AreEqual(RelativeDirectionEnum.Left, direction.Direction);
            direction = DirectionCalculator.Calculate(new Coordinate(1, 0),
                 new Coordinate(0, 0), new Coordinate(0, 1));
            Assert.AreEqual(270, direction.Angle, 0.0001);
            Assert.AreEqual(RelativeDirectionEnum.Right, direction.Direction);
        }

        /// <summary>
        /// Tests calculate direction.
        /// </summary>
        [Test]
        public void DirectionTest()
        {
            Assert.AreEqual(DirectionEnum.North, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(1, 0)));
            Assert.AreEqual(DirectionEnum.NorthWest, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(1, 1)));
            Assert.AreEqual(DirectionEnum.West, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(0, 1)));
            Assert.AreEqual(DirectionEnum.SouthWest, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(-1, 1)));
            Assert.AreEqual(DirectionEnum.South, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(-1, 0)));
            Assert.AreEqual(DirectionEnum.SouthEast, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(-1, -1)));
            Assert.AreEqual(DirectionEnum.East, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(0, -1)));
            Assert.AreEqual(DirectionEnum.NorthEast, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(1, -1)));
        }
    }
}