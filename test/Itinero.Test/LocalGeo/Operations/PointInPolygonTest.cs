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

using System.Collections.Generic;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test.LocalGeo.Operations
{
    /// <summary>
    /// Contains tests related to the point in polygon algorithm.
    /// </summary>
    [TestFixture]
    class PointInPolygonTest
    {
        /// <summary>
        /// Tests basics of point in polygon.
        /// </summary>
        [Test]
        public void TestPointInPolygon()
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

            // In the middle of the polygon
            var testPoint0 = new Coordinate(51.2157635f, 3.2200099f);
            Assert.IsTrue(p.PointIn(testPoint0));
            // just outside of the polygon, but within the bounding box
            var testPoint1 = new Coordinate(51.2157898f, 3.2200648f);
            Assert.IsFalse(p.PointIn(testPoint1));

            // way outside of the poly
            var testPoint2 = new Coordinate(51.2158183f, 3.2201524f);
            Assert.IsFalse(p.PointIn(testPoint2));
        }


        /// <summary>
        /// Tests PIP with a polygon with a hole.
        /// </summary>
        [Test]
        public void TestPointInPolygonWithHole()
        { // http://geojson.io/#id=gist:anonymous/3df900982f5685b17529376e87c7e037&map=13/51.1932/4.4385
            var p = new Polygon();
            p.ExteriorRing.Add(new Coordinate(51.1725629311492f, 4.383201599121094f));
            p.ExteriorRing.Add(new Coordinate(51.1408018208278f, 4.466800689697266f));
            p.ExteriorRing.Add(new Coordinate(51.22860288655629f, 4.470577239990234f));
            p.ExteriorRing.Add(new Coordinate(51.17256293114924f, 4.383201599121094f)); // this is closed manually.
            
            Assert.IsTrue(p.PointIn(new Coordinate(51.210647407041904f, 4.456501007080078f)));
            Assert.IsTrue(p.PointIn(new Coordinate(51.16728887106285f, 4.425601959228516f)));
            
            // this should be 'in the hole'.
            Assert.IsTrue(p.PointIn(new Coordinate(51.184508061068165f, 4.440021514892578f)));

            p.InteriorRings.Add(new List<Coordinate>());
            p.InteriorRings[0].Add(new Coordinate(51.19752580320046f, 4.446544647216797f));
            p.InteriorRings[0].Add(new Coordinate(51.18300163866561f, 4.424400329589844f));
            p.InteriorRings[0].Add(new Coordinate(51.17406969467605f, 4.448089599609375f));
            p.InteriorRings[0].Add(new Coordinate(51.19752580320046f, 4.446544647216797f)); // this is closed manually.

            Assert.IsTrue(p.PointIn(new Coordinate(51.210647407041904f, 4.456501007080078f)));
            Assert.IsTrue(p.PointIn(new Coordinate(51.16728887106285f, 4.425601959228516f)));

            // this should be 'in the hole'.
            Assert.IsFalse(p.PointIn(new Coordinate(51.184508061068165f, 4.440021514892578f)));
        }

        /// <summary>
        /// Tests PIP when the coordinates are around 180°
        /// </summary>
        [Test]
        public void TestPointInPolygonAround180()
        {
            var p = new Polygon();
            p.ExteriorRing.Add(new Coordinate(1, -178));
            p.ExteriorRing.Add(new Coordinate(1, 178));
            p.ExteriorRing.Add(new Coordinate(3, 178));
            p.ExteriorRing.Add(new Coordinate(3, -178));

            Assert.IsTrue(p.PointIn(new Coordinate(2, 179)));
            Assert.IsTrue(p.PointIn(new Coordinate(2, 180)));
            Assert.IsTrue(p.PointIn(new Coordinate(2, -180)));
            Assert.IsTrue(p.PointIn(new Coordinate(2, -179)));

            Assert.IsFalse(p.PointIn(new Coordinate(2, 177)));
            Assert.IsFalse(p.PointIn(new Coordinate(2, -177)));

            p = new Polygon();
            p.ExteriorRing.Add(new Coordinate(3, -178));
            p.ExteriorRing.Add(new Coordinate(3, 178));
            p.ExteriorRing.Add(new Coordinate(1, 178));
            p.ExteriorRing.Add(new Coordinate(1, -178));

            Assert.IsTrue(p.PointIn(new Coordinate(2, 179)));
            Assert.IsTrue(p.PointIn(new Coordinate(2, 180)));
            Assert.IsTrue(p.PointIn(new Coordinate(2, -180)));
            Assert.IsTrue(p.PointIn(new Coordinate(2, -179)));

            Assert.IsFalse(p.PointIn(new Coordinate(2, 177)));
            Assert.IsFalse(p.PointIn(new Coordinate(2, -177)));
        }
    }
}