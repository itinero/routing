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
using Itinero.LocalGeo;

namespace Itinero.Test.Geo
{
    /// <summary>
    /// Contains tests for the line class.
    /// </summary>
    [TestFixture]
    public class LineTests
    {
        /// <summary>
        /// Tests project on.
        /// </summary>
        [Test]
        public void TestProjectOne()
        {
            var line = new Line(new Coordinate(0, 0), new Coordinate(0, 2));
            var coordinate = new Coordinate(1, 1);

            var result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value.Latitude, 0.001);
            Assert.AreEqual(1, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(2, 0));
            coordinate = new Coordinate(1, 1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Value.Latitude, 0.001);
            Assert.AreEqual(0, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(2, 2));
            coordinate = new Coordinate(1, 1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Value.Latitude, 0.001);
            Assert.AreEqual(1, result.Value.Longitude, 0.001);
            line = new Line(new Coordinate(0, 0), new Coordinate(0, -2));
            coordinate = new Coordinate(-1, -1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value.Latitude, 0.001);
            Assert.AreEqual(-1, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(-2, 0));
            coordinate = new Coordinate(-1, -1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result.Value.Latitude, 0.001);
            Assert.AreEqual(0, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(-2, -2));
            coordinate = new Coordinate(-1, -1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result.Value.Latitude, 0.001);
            Assert.AreEqual(-1, result.Value.Longitude, 0.001);
        }

        /// <summary>
        /// Tests project on.
        /// </summary>
        [Test]
        public void TestProjectOneRegression1()
        {
            var point = new Coordinate(51.05349f, 3.731339f);

            var location1 = new Coordinate(51.053382873535156f, 3.7314085960388184f);
            var location2 = new Coordinate(51.05362319946289f, 3.7312211990356445f);
            var line = new Line(location1, location2);

            var projected = line.ProjectOn(point);
            var expectedProject = new Coordinate(51.053487627907394f, 3.7313255667686462f);

            Assert.IsTrue(projected.HasValue);
            Assert.AreEqual(expectedProject.Latitude, projected.Value.Latitude, 0.00001f);
            Assert.AreEqual(expectedProject.Longitude, projected.Value.Longitude, 0.00001f);
        }
    }
}