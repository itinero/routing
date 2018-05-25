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
        private static QuickHull CreateTestSet0()
        {
            var lons = new[] {0f, 1f, 1f, 0f, 0.6f};
            var lats = new[] {0f, 1f, 0f, 1f, 0.5f};

            return new QuickHull(lats, lons);
        }

        private static List<Coordinate> TestPoints()
        {
            var lons = new[] {0f, 1f, 1f, 0f, 0.6f};
            var lats = new[] {0f, 1f, 0f, 1f, 0.5f};
            var coors = new List<Coordinate>();
            for (var i = 0; i < lons.Length; i++)
            {
                coors.Add(new Coordinate(lats[i], lons[i]));
            }

            return coors;
        }

        /// <summary>
        /// Tests the swap method.
        /// </summary>
        [Test]
        public void TestSwap()
        {
            var arr = new[] {0, 1, 2, 3, 4};
            QuickHull.Swap(arr, 1, 3);
            Assert.AreEqual(arr, new[] {0, 3, 2, 1, 4});


            arr = new[] {0, 1, 9, 8, 2, 3, 7, 6};
            QuickHull.Merge(arr, 2, 4, 6);
            Assert.AreEqual(arr, new[] {0, 1, 2, 3, 9, 8, 7, 6});
        }

        /// <summary>
        /// Runs the quickhull algorithm on the simple case.
        /// </summary>
        [Test]
        public void TestSimple()
        {
            var cv0 = CreateTestSet0();
            cv0.CalculateMinMaxX(out var min, out var max);

            Assert.AreEqual(min, 0);
            Assert.AreEqual(max, 1);

            var cutoff = cv0.PartitionLeftRight(0, 1, 2, 4);
            var pts = cv0.Points;

            Assert.AreEqual(cutoff, 3);
            Assert.AreEqual(pts, new[]{0,1,3,2,4});
            // In the right partition, search longest
            var furthestInd = cv0.LongestDistance(0, 1, cutoff, 5);

            Assert.AreEqual(furthestInd, 3);
            Assert.AreEqual(pts[furthestInd], 2);

            // Line = pt0 and pt1; start-index = 2 (ignore first two = pt0;pt1); end index = 3 (only consider
            cutoff = cv0.PartitionInTriangle(0, 1, 2, 2, 4);
            Assert.AreEqual(cutoff, 2);
            Assert.AreEqual(pts, new[]{0,1,2,3,4});
   
            cutoff = cv0.FindHull(0, 1, 2, 5);
            Assert.AreEqual(cutoff, 5);
            Assert.AreEqual(pts, new[]{0,1,2,3,4});
   
            
            cv0 = CreateTestSet0();
            cutoff = cv0.Quickhull();

            Assert.AreEqual(new[] {0, 3, 1, 2, 4}, cv0.Points);
            Assert.AreEqual(cutoff, 4);
        }

        /// <summary>
        /// Test updating a hull.
        /// </summary>
        [Test]
        public void TestUpdate()
        {
            var tp = TestPoints();
            var hull = QuickHull.Quickhull(tp);

            for (var i = 0; i < 5; i++)
            {
                // Adding already known points shouldn't have any effect
                var ind = QuickHull.UpdateHull(hull, tp[i]);
                Assert.AreEqual(-1, ind);
            }

            var index = QuickHull.UpdateHull(hull, new Coordinate(0.5f, -0.5f));
            Assert.AreEqual(1, index);
            
            
            index = QuickHull.UpdateHull(hull, new Coordinate(2f, 2f));
            Assert.AreEqual(3, index);
        }
    }
}