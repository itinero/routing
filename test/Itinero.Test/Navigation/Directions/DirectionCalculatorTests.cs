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
using System.Collections.Generic;
using NUnit.Framework;
using Itinero.LocalGeo;
using Itinero.Navigation.Directions;
using NUnit.Framework.Internal.Builders;

namespace Itinero.Test.Navigation.Directions
{
    /// <summary>
    /// Contains tests for the tools.
    /// </summary>
    [TestFixture]
    class DirectionCalculatorTests
    {
        /// <summary>
        /// Tests angle calculations.
        /// </summary>
        [Test]
        public void TestAngle()
        {
            var offset = 0.001f;
            var center = new Coordinate(51.16917253319145f, 4.476456642150879f);
            var north = new Coordinate(center.Latitude + offset, center.Longitude);
            var northEast = new Coordinate(center.Latitude + offset, center.Longitude + offset);
            var east = new Coordinate(center.Latitude, center.Longitude + offset);
            var southEast = new Coordinate(center.Latitude - offset, center.Longitude + offset);
            var south = new Coordinate(center.Latitude - offset, center.Longitude);
            var southWest = new Coordinate(center.Latitude - offset, center.Longitude - offset);
            var west = new Coordinate(center.Latitude, center.Longitude - offset);
            var northWest = new Coordinate(center.Latitude + offset, center.Longitude - offset);

            var E = 1f;
            Assert.AreEqual(45, DirectionCalculator.Angle(south, center, southEast).ToDegrees(), E);
            Assert.AreEqual(90, DirectionCalculator.Angle(south, center, east).ToDegrees(), E);
            Assert.AreEqual(135, DirectionCalculator.Angle(south, center, northEast).ToDegrees(), E);
            Assert.AreEqual(180, DirectionCalculator.Angle(south, center, north).ToDegrees(), E);
            Assert.AreEqual(225, DirectionCalculator.Angle(south, center, northWest).ToDegrees(), E);
            Assert.AreEqual(270, DirectionCalculator.Angle(south, center, west).ToDegrees(), E);
            Assert.AreEqual(315, DirectionCalculator.Angle(south, center, southWest).ToDegrees(), E);

            Assert.AreEqual(180, DirectionCalculator.Angle(
                new Coordinate(50.84993f, 4.320437f),
                new Coordinate(50.85011f, 4.320471f),
                new Coordinate(50.85029f, 4.320505f)).ToDegrees(), E);
        }

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

        [Test]
        public void CalculateAngleSameLatitudesGivesAngle()
        {
            var shape = new Coordinate(50.71287f, 4.555358f); // slightly to the right/east
            var nextShape = new Coordinate(50.71287f, 4.55207f); // slightly to the left/west
            var bearingOut = DirectionCalculator.Angle(
                nextShape, // west
                shape, // center
                new Coordinate(shape.Latitude + 0.01f /* perfectly north of the shapepoint*/,
                    shape.Longitude));
            Assert.False(double.IsNaN(bearingOut));
            Assert.AreEqual((float) (3 * Math.PI / 2), bearingOut);
        }

        [Test]
        public void CalculateAngleSameLatitudesGivesAngleEast()
        {
            var shape = new Coordinate(50.71287f, 4.555358f);
            var nextShape = new Coordinate(50.71287f, 4.55807f);
            var bearingOut = DirectionCalculator.Angle(
                nextShape, // west
                shape, // center
                new Coordinate(shape.Latitude + 0.01f /* perfectly north of the shapepoint*/,
                    shape.Longitude));
            Assert.False(double.IsNaN(bearingOut));
            Assert.AreEqual((float) Math.PI / 2, bearingOut);
        }


        [Test]
        public void CalculateAngleStraightOn()
        {
            var a = new Coordinate(50.71287f, 4.55f);
            var b = new Coordinate(50.71287f, 4.56f);
            var c = new Coordinate(50.71287f, 4.57f);

            var bearingOut = DirectionCalculator.Angle(a, b, c);
            
            Assert.False(double.IsNaN(bearingOut));
            Assert.AreEqual((float) Math.PI, bearingOut);
        }
        
        [Test]
        public void CalculateAngleTurnAroundExpects360()
        {
            var a = new Coordinate(50.71287f, 4.55f);
            var b = new Coordinate(50.71287f, 4.56f);

            var bearingOut = DirectionCalculator.Angle(a, b, a);
            
            Assert.False(double.IsNaN(bearingOut));
            Assert.AreEqual((float) (2 * Math.PI), bearingOut);
        }

        [Test]
        public void CalculateAngleTurnAround()
        {
            var coordinate1 = new Coordinate(51.168175f,4.4764566f);
            var coordinate2 = new Coordinate(51.169174f,4.4764566f);
            var coordinate3 = new Coordinate(51.168175f,4.4764566f);

            var angle = DirectionCalculator.Angle(coordinate1, coordinate2, coordinate3);
            Assert.AreEqual((float) (Math.PI * 2), angle);
        }
    }
}