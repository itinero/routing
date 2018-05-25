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
            var point = new Coordinate(51.05349f, 3.731339f); // inside of line.

            var location1 = new Coordinate(51.053382873535156f, 3.7314085960388184f);
            var location2 = new Coordinate(51.05362319946289f, 3.7312211990356445f);
            var line = new Line(location1, location2);

            var projected = line.ProjectOn(point);
            var expectedProject = new Coordinate(51.053487627907394f, 3.7313255667686462f);

            Assert.IsTrue(projected.HasValue);
            Assert.AreEqual(expectedProject.Latitude, projected.Value.Latitude, 0.00001f);
            Assert.AreEqual(expectedProject.Longitude, projected.Value.Longitude, 0.00001f);

            point = new Coordinate(51.053216177542964f, 3.731534779071808f); // outside of line.
            projected = line.ProjectOn(point);
            Assert.IsFalse(projected.HasValue);

            point = new Coordinate(51.05382483111302f, 3.7310868501663204f); // outside of line.
            projected = line.ProjectOn(point);
            Assert.IsFalse(projected.HasValue);
        }
    }
}
