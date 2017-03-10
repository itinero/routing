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
using Itinero.Navigation.Directions;

namespace Itinero.Test.Navigation.Directions
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
            Assert.AreEqual(RelativeDirectionEnum.Right, direction.Direction);
            direction = DirectionCalculator.Calculate(new Coordinate(1, 0),
                 new Coordinate(0, 0), new Coordinate(0, 1));
            Assert.AreEqual(270, direction.Angle, 0.0001);
            Assert.AreEqual(RelativeDirectionEnum.Left, direction.Direction);
        }

        /// <summary>
        /// Tests calculate direction.
        /// </summary>
        [Test]
        public void DirectionTest()
        {
            Assert.AreEqual(DirectionEnum.North, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(1, 0)));
            Assert.AreEqual(DirectionEnum.NorthEast, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(1, 1)));
            Assert.AreEqual(DirectionEnum.East, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(0, 1)));
            Assert.AreEqual(DirectionEnum.SouthEast, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(-1, 1)));
            Assert.AreEqual(DirectionEnum.South, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(-1, 0)));
            Assert.AreEqual(DirectionEnum.SouthWest, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(-1, -1)));
            Assert.AreEqual(DirectionEnum.West, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(0, -1)));
            Assert.AreEqual(DirectionEnum.NorthWest, DirectionCalculator.Calculate(new Coordinate(0, 0),
                new Coordinate(1, -1)));
        }
    }
}