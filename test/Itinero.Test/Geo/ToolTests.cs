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