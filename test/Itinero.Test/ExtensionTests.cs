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
using Itinero.Test.Mocks;
using Reminiscence.Arrays;
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

        /// <summary>
        /// Tests <see cref="Extensions.EnsureMinimumSize{T}"/>.
        /// </summary>
        [Test]
        public void TestEnsureMinimumSize()
        {
            const long InitialLength = 16;
            Guid initVal = Guid.NewGuid();
            Guid fillVal = Guid.NewGuid();

            var array = new MemoryArray<Guid>(0);

            // resizing should work if it starts at 0.
            array.EnsureMinimumSize(InitialLength, initVal);
            Assert.LessOrEqual(InitialLength, array.Length, "EnsureMinimumSize should be able to resize up from 0 to at least a big enough size.");
            Assert.AreEqual(initVal, array[0], "EnsureMinimumSize with a 'fill' parameter should fill the new slots with the given value.");
            Assert.AreEqual(initVal, array[array.Length - 1], "EnsureMinimumSize with a 'fill' parameter should fill the new slots with the given value.");

            // now that we've gone up from 0, set the size directly.
            array.Resize(InitialLength);

            // resizing smaller should do nothing.
            array.EnsureMinimumSize(array.Length - 5, fillVal);
            array.EnsureMinimumSize(array.Length - 5);
            Assert.AreEqual(InitialLength, array.Length, "EnsureMinimumSize should leave the array alone when the array is bigger than needed.");

            // resizing equal should do nothing.
            array.EnsureMinimumSize(InitialLength, fillVal);
            array.EnsureMinimumSize(InitialLength);
            Assert.AreEqual(InitialLength, array.Length, "EnsureMinimumSize should leave the array alone when the array is exactly as big as needed.");

            // first resize goes straight up to 1024.
            array.EnsureMinimumSize(InitialLength + 1, fillVal);
            Assert.AreEqual(1024, array.Length, "EnsureMinimumSize should have a floor of 1024 elements when resizing.");
            Assert.AreEqual(initVal, array[0], "EnsureMinimumSize should not change any data that's already in the array when resizing.");
            Assert.AreEqual(initVal, array[InitialLength - 1], "EnsureMinimumSize should not change any data that's already in the array when resizing.");
            Assert.AreEqual(fillVal, array[InitialLength], "EnsureMinimumSize with a 'fill' parameter should fill the new slots with the given value.");
            Assert.AreEqual(fillVal, array[array.Length - 1], "EnsureMinimumSize with a 'fill' parameter should fill the new slots with the given value.");

            // resizes above the current size should double whatever it was.
            array.Resize(1050);
            array.EnsureMinimumSize(1051);
            Assert.AreEqual(2100, array.Length, "EnsureMinimumSize should double current size, even if it wasn't a power of two before.");
            Assert.AreEqual(Guid.Empty, array[array.Length - 1], "EnsureMinimumSize without a 'fill' parameter should fill with default.");
        }
    }
}