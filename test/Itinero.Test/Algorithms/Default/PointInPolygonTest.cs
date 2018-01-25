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
using Itinero.Algorithms.Default;
using Itinero.Test.Profiles;
using Itinero.Data.Network;
using Itinero.LocalGeo;
using System;
using System.Collections.Generic;


namespace Itinero.Test.Algorithms.Default
{
    /// <summary>
    /// Executes tests
    /// </summary>
    [TestFixture]
    class PointInPolygonTest
    {
        [Test]
        public void TestPointInPolygon()
        {
            Console.WriteLine("Starting Point in Polygon Test");
            Coordinate c1 = new Coordinate((float)51.2157978, (float)3.2200444);
            Coordinate c2 = new Coordinate((float)51.2157465, (float)3.2200733);
            Coordinate c3 = new Coordinate((float)51.2157229, (float)3.2199664);
            Coordinate c4 = new Coordinate((float)51.2157743, (float)3.2199375);

            Polygon p = new Polygon();
            p.ExteriorRing.Add(c1);
            p.ExteriorRing.Add(c2);
            p.ExteriorRing.Add(c3);
            p.ExteriorRing.Add(c4);


            // In the middle of the polygon
            Coordinate testPoint0 = new Coordinate((float)51.2157635, (float)3.2200099);
            Assert.IsTrue(p.PointLiesWithin(testPoint0));
            // just outside of the polygon, but within the bounding box
            Coordinate testPoint1 = new Coordinate((float)51.2157898, (float)3.2200648);
            Assert.IsFalse(p.PointLiesWithin(testPoint1));

            // way outside of the poly
            Coordinate testPoint2 = new Coordinate((float)51.2158183, (float)3.2201524);
            Assert.IsFalse(p.PointLiesWithin(testPoint2));

        }
    }
}