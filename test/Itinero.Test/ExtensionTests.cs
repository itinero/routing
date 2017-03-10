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
using OsmSharp.IO;
using Itinero.Test.Mocks;
using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for extensions.
    /// </summary>
    [TestFixture]
    public class ExtensionTests
    {
        /// <summary>
        /// Tests the try get value or default extension.
        /// </summary>
        [Test]
        public void TestTryGetValueOrDefault()
        {
            var dic = new Dictionary<int, Tuple<int>>();
            dic.Add(0, new Tuple<int>(10));
            dic.Add(1, new Tuple<int>(11));

            Assert.AreEqual(10, dic.TryGetValueOrDefault(0).Item1);
            Assert.AreEqual(11, dic.TryGetValueOrDefault(1).Item1);
            Assert.AreEqual(null, dic.TryGetValueOrDefault(2));
        }

        /// <summary>
        /// Tests the next power of 2.
        /// </summary>
        [Test]
        public void TestNextPowerOf2()
        {
            Assert.AreEqual(0, Extensions.NextPowerOfTwo(0));
            Assert.AreEqual(1, Extensions.NextPowerOfTwo(1));
            Assert.AreEqual(2, Extensions.NextPowerOfTwo(2));
            Assert.AreEqual(4, Extensions.NextPowerOfTwo(3));
            Assert.AreEqual(4, Extensions.NextPowerOfTwo(4));
            Assert.AreEqual(8, Extensions.NextPowerOfTwo(5));
            Assert.AreEqual(8, Extensions.NextPowerOfTwo(6));
            Assert.AreEqual(8, Extensions.NextPowerOfTwo(7));
            Assert.AreEqual(8, Extensions.NextPowerOfTwo(8));
            Assert.AreEqual(16, Extensions.NextPowerOfTwo(12));
            Assert.AreEqual(32, Extensions.NextPowerOfTwo(28));
            Assert.AreEqual(64, Extensions.NextPowerOfTwo(45));
            Assert.AreEqual(512, Extensions.NextPowerOfTwo(413));
            Assert.AreEqual(65536, Extensions.NextPowerOfTwo(41465));
            Assert.AreEqual(131072, Extensions.NextPowerOfTwo(130072));
            Assert.AreEqual(524288, Extensions.NextPowerOfTwo(514288));
        }

        /// <summary>
        /// Test seek begin.
        /// </summary>
        [Test]
        public void TestSeekBegin()
        {
            var stream = new BinaryWriter(new StreamMock());

            var position = (long)int.MaxValue + 10;
            Assert.AreEqual(position, stream.SeekBegin(position));

            position = 1000;
            Assert.AreEqual(position, stream.SeekBegin(position));

            position = (long)int.MaxValue * 32;
            Assert.AreEqual(position, stream.SeekBegin(position));
        }
    }
}