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
using NUnit.Framework.Internal;
using OsmSharp.Geo;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graphs.Geometric.Shapes;
using Reminiscence.IO;
using System.Collections.Generic;
using System.IO;

namespace OsmSharp.Routing.Test.Graphs.Geometric.Shapes
{
    /// <summary>
    /// Contains tests for the shapes index.
    /// </summary>
    [TestFixture]
    public class ShapesArrayTests
    {
        /// <summary>
        /// Tests creating a shape index.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            var array = new ShapesArray(1024);

            Assert.IsNull(array[0]);
            Assert.IsNull(array[1000]);

            using (var map = new MemoryMapStream())
            {
                array = new ShapesArray(map, 1024);

                Assert.IsNull(array[0]);
                Assert.IsNull(array[1000]);
            }
        }

        /// <summary>
        /// Tests adding shapes.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new ShapesArray(map, 1024);
                array.Set(0, new GeoCoordinate(0, 0.1), 
                    new GeoCoordinate(1, 1.1));

                var shape = array[0];
                Assert.IsNotNull(shape);
                Assert.AreEqual(2, shape.Count);
                Assert.AreEqual(0, shape[0].Latitude, .00001);
                Assert.AreEqual(0.1, shape[0].Longitude, .00001);
                Assert.AreEqual(1, shape[1].Latitude, .00001);
                Assert.AreEqual(1.1, shape[1].Longitude, .00001);
            }

            using (var map = new MemoryMapStream())
            {
                var box = new GeoCoordinateBox(
                    new GeoCoordinate(-90, -180),
                    new GeoCoordinate(90, 180));
                var refArray = new ShapeBase[1024];
                var array = new ShapesArray(map, 1024);

                var rand = new Randomizer(1587341311);
                for (var i = 0; i < 1024; i++)
                {
                    var count = rand.Next(10);
                    var newShape = new List<ICoordinate>(count);
                    for(var j = 0; j < count; j++)
                    {
                        newShape.Add(box.GenerateRandomIn());
                    }

                    var shape = new ShapeEnumerable(newShape);
                    refArray[i] = shape;
                    array[i] = shape;
                }

                for(var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    var shape = array[i];
                    Assert.IsNotNull(shape);

                    for (var j = 0; j < shape.Count; j++)
                    {
                        Assert.AreEqual(refShape[j].Latitude, shape[j].Latitude);
                        Assert.AreEqual(refShape[j].Longitude, shape[j].Longitude);
                    }
                }
            }
        }

        /// <summary>
        /// Tests copy to.
        /// </summary>
        [Test]
        public void TestCopyTo()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new ShapesArray(map, 1);
                array.Set(0, new GeoCoordinate(0, 0.1), new GeoCoordinate(1, 1.1));

                using (var stream = new MemoryStream())
                {
                    Assert.AreEqual(16 + 8 + (4 * 4), array.CopyTo(stream));
                }
            }
        }

        /// <summary>
        /// Tests copy to and create from.
        /// </summary>
        [Test]

        public void TestCopyToCreateFrom()
        {
            var box = new GeoCoordinateBox(
                new GeoCoordinate(-90, -180),
                new GeoCoordinate(90, 180));
            var refArray = new ShapesArray(1024);

            var rand = OsmSharp.Math.Random.StaticRandomGenerator.Get();
            OsmSharp.Math.Random.StaticRandomGenerator.Set(46541577);
            var totalCoordinateCount = 0;
            for (var i = 0; i < 1024; i++)
            {
                var count = rand.Generate(10);
                totalCoordinateCount += count;
                var newShape = new List<ICoordinate>(count);
                for (var j = 0; j < count; j++)
                {
                    newShape.Add(box.GenerateRandomIn());
                }

                var shape = new ShapeEnumerable(newShape);
                refArray[i] = shape;
            }

            using(var stream = new MemoryStream())
            {
                Assert.AreEqual(16 + (1024 * 8) + (totalCoordinateCount * 8),
                    refArray.CopyTo(stream));
                stream.Seek(0, SeekOrigin.Begin);

                var array = ShapesArray.CreateFrom(stream, true);
                for (var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    var shape = array[i];
                    Assert.IsNotNull(shape);

                    for (var j = 0; j < shape.Count; j++)
                    {
                        Assert.AreEqual(refShape[j].Latitude, shape[j].Latitude);
                        Assert.AreEqual(refShape[j].Longitude, shape[j].Longitude);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                array = ShapesArray.CreateFrom(stream, false);
                for (var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    var shape = array[i];
                    Assert.IsNotNull(shape);

                    for (var j = 0; j < shape.Count; j++)
                    {
                        Assert.AreEqual(refShape[j].Latitude, shape[j].Latitude);
                        Assert.AreEqual(refShape[j].Longitude, shape[j].Longitude);
                    }
                }
            }
        }

        /// <summary>
        /// Tests copy to.
        /// </summary>
        [Test]
        public void TestSettingNull()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new ShapesArray(map, 10);
                array.Set(0, new GeoCoordinate(0, 0.1), new GeoCoordinate(1, 1.1));

                array[0] = null;

                Assert.IsNull(array[0]);
                Assert.IsNull(array[4]);
                Assert.IsNull(array[7]);
                Assert.IsNull(array[2]);
                Assert.IsNull(array[8]);
            }
        }
    }
}