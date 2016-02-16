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

namespace Itinero.Test.Geo
{
    /// <summary>
    /// Contains tests for the tools.
    /// </summary>
    [TestFixture]
    public class ToolTests
    {
        /// <summary>
        /// Tests converting to radians.
        /// </summary>
        [Test]
        public void TestToRadians()
        {
            Assert.AreEqual(System.Math.PI * 0 / 2, 000.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 1 / 2, 090.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 2 / 2, 180.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 3 / 2, 270.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 4 / 2, 360.0f.ToRadians(), 0.00001);

            Assert.AreEqual(System.Math.PI * 0 / 2, 000.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 1 / 2, 090.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 2 / 2, 180.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 3 / 2, 270.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 4 / 2, 360.0.ToRadians(), 0.00001);
        }

        /// <summary>
        /// Tests converting to degrees.
        /// </summary>
        [Test]
        public void TestToDegrees()
        {
            Assert.AreEqual(000.0, (System.Math.PI * 0 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(090.0, (System.Math.PI * 1 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(180.0, (System.Math.PI * 2 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(270.0, (System.Math.PI * 3 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(360.0, (System.Math.PI * 4 / 2).ToDegrees(), 0.00001);

            Assert.AreEqual(000.0f, ((float)System.Math.PI * 0 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(090.0f, ((float)System.Math.PI * 1 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(180.0f, ((float)System.Math.PI * 2 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(270.0f, ((float)System.Math.PI * 3 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(360.0f, ((float)System.Math.PI * 4 / 2).ToDegrees(), 0.0001);
        }

        /// <summary>
        /// Tests normalizing degrees.
        /// </summary>
        [Test]
        public void TestNormalizeDegrees()
        {
            Assert.AreEqual(0, (0 + (1 * 360.0)).NormalizeDegrees(), 0.00001);
            Assert.AreEqual(36, (36 + (10 * 360.0)).NormalizeDegrees(), 0.00001);
            Assert.AreEqual(359, (359 + (11 * 360.0)).NormalizeDegrees(), 0.00001);
            
            Assert.AreEqual(360 - 36, (-36.0).NormalizeDegrees(), 0.00001);
            Assert.AreEqual(360 - 359, (-359.0).NormalizeDegrees(), 0.00001);
        }
    }
}