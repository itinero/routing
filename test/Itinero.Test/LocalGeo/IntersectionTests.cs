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
using Itinero.Test.Profiles;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Itinero.Test.LocalGeo
{
    /// <summary>
    /// Contains tests for the intersections code.
    /// </summary>
    [TestFixture]
    public class IntersectionTests
    {
        /// <summary>
        /// Tests an intersection with a line segment that has one point inside and one point outside.
        /// </summary>
        [Test]
        public void TestInsideToOutside()
        {            
            var c1 = new Coordinate(51.2157978f, 3.2200444f);
            var c2 = new Coordinate(51.2157465f, 3.2200733f);
            var c3 = new Coordinate(51.2157229f, 3.2199664f);
            var c4 = new Coordinate(51.2157743f, 3.2199375f);

            var p = new Polygon();
            p.ExteriorRing.Add(c1);
            p.ExteriorRing.Add(c2);
            p.ExteriorRing.Add(c3);
            p.ExteriorRing.Add(c4);
            p.ExteriorRing.Add(c1);

            // In the middle of the polygon
            var testPoint0 = new Coordinate(51.2157635f, 3.2200099f);
            // just outside of the polygon, but within the bounding box
            var testPoint1 = new Coordinate(51.2157898f, 3.2200648f);

            var intersections = p.Intersect(testPoint0.Latitude, testPoint0.Longitude,
                testPoint1.Latitude, testPoint1.Longitude);
            Assert.IsNotNull(intersections);
            Assert.AreEqual(1, intersections.Count());
        }

        /// <summary>
        /// Tests an intersection with a line segment that has one point inside and one point outside.
        /// </summary>
        [Test]
        public void TestTwoOutside()
        {
            var c1 = new Coordinate(51.2157978f, 3.2200444f);
            var c2 = new Coordinate(51.2157465f, 3.2200733f);
            var c3 = new Coordinate(51.2157229f, 3.2199664f);
            var c4 = new Coordinate(51.2157743f, 3.2199375f);

            var p = new Polygon();
            p.ExteriorRing.Add(c1);
            p.ExteriorRing.Add(c2);
            p.ExteriorRing.Add(c3);
            p.ExteriorRing.Add(c4);
            p.ExteriorRing.Add(c1);

            // both outside of the polygon.
            var testPoint0 = new Coordinate(51.21581906125835f, 3.219967782497406f);
            var testPoint1 = new Coordinate(51.215703133854038f, 3.220040202140808f);

            var intersections = p.Intersect(testPoint0.Latitude, testPoint0.Longitude,
                testPoint1.Latitude, testPoint1.Longitude);
            Assert.IsNotNull(intersections);
            Assert.AreEqual(2, intersections.Count());
        }
    }
}