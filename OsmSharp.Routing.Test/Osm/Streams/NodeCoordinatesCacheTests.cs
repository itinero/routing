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
using OsmSharp.Routing.Osm.Streams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OsmSharp.Routing.Test.Osm.Streams
{
    /// <summary>
    /// Contains tests for the router db stream target.
    /// </summary>
    [TestFixture]
    public class NodeCoordinatesCacheTests
    {
        /// <summary>
        /// Tests adding.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var cache = new NodeCoordinatesCache();
            cache.Add(0, 0, 0);
            Assert.AreEqual(1, cache.Count);
            cache.Add(1, 1, 10);
            Assert.AreEqual(2, cache.Count);
            cache.Add(2, 2, 20);
            Assert.AreEqual(3, cache.Count);
        }

        /// <summary>
        /// Tests getting data out.
        /// </summary>
        [Test]
        public void TestGet()
        {
            var cache = new NodeCoordinatesCache();
            cache.Add(0, 0, 0);
            cache.Add(1, 1, 10);
            cache.Add(2, 2, 20);

            float lat, lon;
            Assert.IsTrue(cache.TryGet(0, out lat, out lon));
            Assert.AreEqual(0, lat);
            Assert.AreEqual(0, lon);
            Assert.IsTrue(cache.TryGet(1, out lat, out lon));
            Assert.AreEqual(1, lat);
            Assert.AreEqual(10, lon);
            Assert.IsTrue(cache.TryGet(2, out lat, out lon));
            Assert.AreEqual(2, lat);
            Assert.AreEqual(20, lon);

            cache = new NodeCoordinatesCache();
            cache.Add(0, 0, 0);
            cache.Add(1, 1, 10);
            cache.Add(2, 2, 20);
            cache.Add(3, 3, 30);
            cache.Add(4, 4, 40);
            cache.Add(5, 5, 50);
            cache.Add(6, 6, 60);
            cache.Add(7, 7, 70);
            cache.Add(8, 8, 80);

            Assert.IsTrue(cache.TryGet(0, out lat, out lon));
            Assert.AreEqual(0, lat);
            Assert.AreEqual(0, lon);
            Assert.IsTrue(cache.TryGet(1, out lat, out lon));
            Assert.AreEqual(1, lat);
            Assert.AreEqual(10, lon);
            Assert.IsTrue(cache.TryGet(2, out lat, out lon));
            Assert.AreEqual(2, lat);
            Assert.AreEqual(20, lon);
            Assert.IsTrue(cache.TryGet(3, out lat, out lon));
            Assert.AreEqual(3, lat);
            Assert.AreEqual(30, lon);
            Assert.IsTrue(cache.TryGet(4, out lat, out lon));
            Assert.AreEqual(4, lat);
            Assert.AreEqual(40, lon);
            Assert.IsTrue(cache.TryGet(5, out lat, out lon));
            Assert.AreEqual(5, lat);
            Assert.AreEqual(50, lon);
            Assert.IsTrue(cache.TryGet(6, out lat, out lon));
            Assert.AreEqual(6, lat);
            Assert.AreEqual(60, lon);
            Assert.IsTrue(cache.TryGet(7, out lat, out lon));
            Assert.AreEqual(7, lat);
            Assert.AreEqual(70, lon);
            Assert.IsTrue(cache.TryGet(8, out lat, out lon));
            Assert.AreEqual(8, lat);
            Assert.AreEqual(80, lon);
            Assert.IsTrue(cache.TryGet(7, out lat, out lon));
            Assert.AreEqual(7, lat);
            Assert.AreEqual(70, lon);
            Assert.IsTrue(cache.TryGet(4, out lat, out lon));
            Assert.AreEqual(4, lat);
            Assert.AreEqual(40, lon);
            Assert.IsTrue(cache.TryGet(1, out lat, out lon));
            Assert.AreEqual(1, lat);
            Assert.AreEqual(10, lon);
            Assert.IsTrue(cache.TryGet(3, out lat, out lon));
            Assert.AreEqual(3, lat);
            Assert.AreEqual(30, lon);
        }
    }
}
