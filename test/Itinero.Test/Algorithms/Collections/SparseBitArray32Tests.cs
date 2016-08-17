// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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
