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

using Itinero.Algorithms.Collections;
using NUnit.Framework;
using System.Collections.Generic;

namespace Itinero.Test.Algorithms.Collections
{
    /// <summary>
    /// Contains tests for the sparse bit array.
    /// </summary>
    [TestFixture]
    public class SparseBitArray32Tests
    {
        /// <summary>
        /// Tests getting and setting bits.
        /// </summary>
        [Test]
        public void TestGetSet()
        {
            var index = new SparseBitArray32(65536 * 32, 32);

            index[10] = true;
            index[100] = true;
            index[1000] = true;
            index[10000] = true;
            index[100000] = true;

            Assert.IsTrue(index[10]);
            Assert.IsTrue(index[100]);
            Assert.IsTrue(index[1000]);
            Assert.IsTrue(index[10000]);
            Assert.IsTrue(index[100000]);
        }

        /// <summary>
        /// Tests enumerating the flags.
        /// </summary>
        [Test]
        public void TestEnumeration()
        {
            var index = new SparseBitArray32(65536 * 32, 32);

            index[10] = true;
            index[100] = true;
            index[1000] = true;
            index[10000] = true;
            index[100000] = true;

            var list = new List<long>(index);
            Assert.AreEqual(5, list.Count);
            Assert.IsTrue(list.Contains(10));
            Assert.IsTrue(list.Contains(100));
            Assert.IsTrue(list.Contains(1000));
            Assert.IsTrue(list.Contains(10000));
            Assert.IsTrue(list.Contains(100000));
        }
    }
}
