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
using NUnit.Framework;

namespace Itinero.Test.LocalGeo
{
    /// <summary>
    /// Contains tests for the line.
    /// </summary>
    [TestFixture]
    public class LineTests
    {
        /// <summary>
        /// Tests intersections.
        /// </summary>
        [Test]
        public void TestIntersection()
        {
            var line1 = new Line(new Coordinate(0.001f, 0.000f), new Coordinate(-0.001f, 0.000f));
            var line2 = new Line(new Coordinate(0.000f, 0.001f), new Coordinate(0.000f, -0.001f));

            var intersection = line1.Intersect(line2);

            Assert.AreEqual(0, intersection.Value.Latitude);
            Assert.AreEqual(0, intersection.Value.Longitude);
            Assert.AreEqual(null, intersection.Value.Elevation);
        }

        /// <summary>
        /// Tests intersection with elevation.
        /// </summary>
        [Test]
        public void TestIntersectionWithElevation()
        {
            var line1 = new Line(new Coordinate(0.001f, 0.000f, 0), new Coordinate(-0.001f, 0.000f, 100));
            var line2 = new Line(new Coordinate(0.000f, 0.001f, 0), new Coordinate(0.000f, -0.001f, 100));

            var intersection = line1.Intersect(line2);

            Assert.AreEqual(0, intersection.Value.Latitude);
            Assert.AreEqual(0, intersection.Value.Longitude);
            Assert.AreEqual(50, intersection.Value.Elevation);

            intersection = line2.Intersect(line1);

            Assert.AreEqual(0, intersection.Value.Latitude);
            Assert.AreEqual(0, intersection.Value.Longitude);
            Assert.AreEqual(50, intersection.Value.Elevation);
        }
    }
}
