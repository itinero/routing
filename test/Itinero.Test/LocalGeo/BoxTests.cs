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
    /// Contains tests for the box.
    /// </summary>
    [TestFixture]
    public class BoxTests
    {
        /// <summary>
        /// Tests overlaps with box.
        /// </summary>
        [Test]
        public void TestOverlapsWithBox()
        {
            var box = new Box(0, 0, 2, 2);
            
            Assert.IsTrue(box.Overlaps(box));
            Assert.IsTrue(box.Overlaps(new Box(0, 0, 1, 1)));
            Assert.IsTrue(box.Overlaps(new Box(-1, -1, 3, 3)));
            Assert.IsFalse(box.Overlaps(new Box(10, 10, 30, 30)));
        }

        /// <summary>
        /// Tests expand with.
        /// </summary>
        [Test]
        public void TestExpandWith()
        {
            var box = new Box(0, 0, 1, 1);
            var expanded = box.ExpandWith(0.5f, 0.5f);

            Assert.AreEqual(0, expanded.MinLat);
            Assert.AreEqual(0, expanded.MinLon);
            Assert.AreEqual(1, expanded.MaxLat);
            Assert.AreEqual(1, expanded.MaxLon);

            expanded = box.ExpandWith(0.5f, 1.5f);

            Assert.AreEqual(0, expanded.MinLat);
            Assert.AreEqual(0, expanded.MinLon);
            Assert.AreEqual(1, expanded.MaxLat);
            Assert.AreEqual(1.5f, expanded.MaxLon);

            expanded = box.ExpandWith(1.5f, 0.5f);

            Assert.AreEqual(0, expanded.MinLat);
            Assert.AreEqual(0, expanded.MinLon);
            Assert.AreEqual(1.5f, expanded.MaxLat);
            Assert.AreEqual(1f, expanded.MaxLon);

            expanded = box.ExpandWith(1.5f, 1.5f);

            Assert.AreEqual(0, expanded.MinLat);
            Assert.AreEqual(0, expanded.MinLon);
            Assert.AreEqual(1.5f, expanded.MaxLat);
            Assert.AreEqual(1.5f, expanded.MaxLon);

            expanded = box.ExpandWith(-0.5f, 0.5f);

            Assert.AreEqual(-0.5f, expanded.MinLat);
            Assert.AreEqual(0, expanded.MinLon);
            Assert.AreEqual(1, expanded.MaxLat);
            Assert.AreEqual(1, expanded.MaxLon);

            expanded = box.ExpandWith(0.5f, -0.5f);

            Assert.AreEqual(0, expanded.MinLat);
            Assert.AreEqual(-0.5f, expanded.MinLon);
            Assert.AreEqual(1f, expanded.MaxLat);
            Assert.AreEqual(1f, expanded.MaxLon);

            expanded = box.ExpandWith(-0.5f, -0.5f);

            Assert.AreEqual(-0.5f, expanded.MinLat);
            Assert.AreEqual(-0.5f, expanded.MinLon);
            Assert.AreEqual(1f, expanded.MaxLat);
            Assert.AreEqual(1f, expanded.MaxLon);
        }
    }
}