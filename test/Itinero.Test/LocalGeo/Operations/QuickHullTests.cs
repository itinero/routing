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
using Itinero.LocalGeo.Operations;
using NUnit.Framework;

namespace Itinero.Test.LocalGeo.Operations
{
    /// <summary>
    /// Contains tests related to the quickhull algorithm.
    /// </summary>
    [TestFixture]
    public class QuickHullTests
    {
        private static List<Coordinate> asList(float[] lat, float[] lon)
        {
            var coors = new List<Coordinate>();
            for (var i = 0; i < lat.Length; i++)
            {
                coors.Add(new Coordinate(lat[i], lon[i]));
            }

            return coors;
        }

        private static List<Coordinate> CreateTestSet0()
        {
            var lons = new[] {0f, 1f, 1f, 0f, 0.6f};
            var lats = new[] {0f, 1f, 0f, 1f, 0.5f};
            return asList(lats, lons);
        }

        private static List<Coordinate> CreateTestSet1()
        {
            var lons = new[] {7.03125f, 7.03125f, 8.7890625f, 13.359375f, 13.0078125f};
            var lats = new[]
                {47.517200697839414f, 51.17934297928927f, 47.989921667414194f, 48.45835188280866f, 48.22467264956519f};
            return asList(lats, lons);
        }


        private static List<Coordinate> CreateTestSet2()
        {
            var lons = new[] {7.03125f, 7.03125f, 8.7890625f, 13.359375f, 13.0078125f, 9.6385f};
            var lats = new[]
            {
                47.517200697839414f, 51.17934297928927f, 47.989921667414194f, 48.45835188280866f, 48.22467264956519f,
                49.686f
            };
            return asList(lats, lons);
        }


        private static List<Coordinate> SelectPoints(List<Coordinate> points, int[] indices)
        {
            var result = new List<Coordinate>();

            for (var i = 0; i < indices.Length; i++)
            {
                result.Add(points[indices[i]]);
            }

            return result;
        }

        [Test]
        public void Test0()
        {
            var points = CreateTestSet0();
            var hull = points.Convexhull();

            var expected = SelectPoints(points, new[] {0, 3, 1, 2, 0});
            Assert.AreEqual(expected, hull);
        }


        [Test]
        public void Test1()
        {
            var points = CreateTestSet1();
            var hull = points.Convexhull();

            var expected = SelectPoints(points, new[] {0, 1, 3, 4, 0});
            Assert.AreEqual(expected, hull);
        }

        [Test]
        public void Test2()
        {
            var points = CreateTestSet2();
            var hull = points.Convexhull();

            var expected = SelectPoints(points, new[] {0, 1, 3, 4, 0});
            Assert.AreEqual(expected, hull);
        }


        /// <summary>
        /// Test updating a hull.
        /// </summary>
        [Test]
        public void TestUpdate()
        {
            var tp = CreateTestSet0();
            var hull = tp.Convexhull();

            for (var i = 0; i < 5; i++)
            {
                // Adding already known points shouldn't have any effect
                var ind = hull.UpdateHull(tp[i]);
                Assert.AreEqual(-1, ind);
            }

            var index = hull.UpdateHull(new Coordinate(0.5f, -0.5f));
            Assert.AreEqual(1, index);


            index = hull.UpdateHull(new Coordinate(2f, 2f));
            Assert.AreEqual(3, index);
        }
    }
}