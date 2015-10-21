// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test
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
    }
}