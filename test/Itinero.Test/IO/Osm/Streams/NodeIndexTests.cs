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

using Itinero.IO.Osm.Streams;
using NUnit.Framework;

namespace Itinero.Test.IO.Osm.Streams
{
    /// <summary>
    /// Contains tests for the node index.
    /// </summary>
    [TestFixture]
    public class NodeIndexTests
    {
        /// <summary>
        /// Tests negative id indexing.
        /// </summary>
        [Test]
        public void TestNegativeIds()
        {
            var index = new NodeIndex();
            index.AddId(-128510752);
            index.SortAndConvertIndex();
            index.Set(-128510752, 11);

            index.TryGetValue(-128510752, out var latitude, out var longitude, out var isCore, out var vertex);
            Assert.AreEqual(float.MaxValue, latitude);
            Assert.AreEqual(float.MaxValue, longitude);
            Assert.AreEqual(false, isCore);
            Assert.AreEqual(11, vertex);
        }

        /// <summary>
        /// A regression tests, the node index doesn't seem to store a node with the given id.
        ///
        /// Cause: ID jumped more than int.maxvalue compare to the previous ID.
        /// </summary>
        [Test]
        public void NodeIndex_Regression1()
        {
            var index = new NodeIndex();
            index.AddId(4444197607);
            
            index.SortAndConvertIndex();
            var i = index.TryGetIndex(4444197607);
            Assert.AreEqual(0, i);
        }
    }
}