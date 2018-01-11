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
using NUnit.Framework.Internal;
using Itinero.LocalGeo;
using Itinero.Graphs.Geometric.Shapes;
using Reminiscence.IO;
using System.Collections.Generic;
using System.IO;

namespace Itinero.Test.Graphs.Geometric.Shapes
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
                array.Set(0, new Coordinate(0, 0.1f),
                    new Coordinate(1, 1.1f));

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
                var box = new Box(
                    new Coordinate(-90, -180),
                    new Coordinate(90, 180));
                var refArray = new ShapeBase[1024];
                var array = new ShapesArray(map, 1024);

                var rand = new Randomizer(1587341311);
                for (var i = 0; i < 1024; i++)
                {
                    var count = rand.Next(10);
                    var newShape = new List<Coordinate>(count);
                    for (var j = 0; j < count; j++)
                    {
                        newShape.Add(box.GenerateRandomIn());
                    }

                    var shape = new ShapeEnumerable(newShape);
                    refArray[i] = shape;
                    array[i] = shape;
                }

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
        public void TestCopyTo()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new ShapesArray(map, 1);
                array.Set(0, new Coordinate(0, 0.1f), new Coordinate(1, 1.1f));

                using (var stream = new MemoryStream())
                {
                    Assert.AreEqual(16 + 8 + (4 * 4), array.CopyTo(stream));
                }
            }
        }

        /// <summary>
        /// Tests copy to.
        /// </summary>
        [Test]
        public void TestCopyToWithElevation()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new ShapesArray(map, 1);
                array.Set(0, new Coordinate(0, 0.1f, 10), new Coordinate(1, 1.1f, 11));

                using (var stream = new MemoryStream())
                {
                    Assert.AreEqual(16 + 8 + (4 * 4) + 2 * 2, array.CopyTo(stream));
                }
            }
        }

        /// <summary>
        /// Tests copy to and create from.
        /// </summary>
        [Test]

        public void TestCopyToCreateFrom()
        {
            var box = new Box(
                new Coordinate(-90, -180),
                new Coordinate(90, 180));
            var refArray = new ShapesArray(1024);

            var rand = new System.Random(46541577);
            var totalCoordinateCount = 0;
            for (var i = 0; i < 1024; i++)
            {
                var count = rand.Next(10);
                totalCoordinateCount += count;
                var newShape = new List<Coordinate>(count);
                for (var j = 0; j < count; j++)
                {
                    newShape.Add(box.GenerateRandomIn());
                }

                var shape = new ShapeEnumerable(newShape);
                refArray[i] = shape;
            }

            using (var stream = new MemoryStream())
            {
                Assert.AreEqual(16 + (1024 * 8) + (totalCoordinateCount * 8),
                    refArray.CopyTo(stream));
                stream.Seek(0, SeekOrigin.Begin);

                var array = ShapesArray.CreateFrom(stream, true);
                for (var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    if (refShape.Count == 0)
                    {
                        continue;
                    }
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
                    if (refShape.Count == 0)
                    {
                        continue;
                    }
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
        /// Tests copy to and create from.
        /// </summary>
        [Test]

        public void TestCopyToCreateFromWithElevation()
        {
            var box = new Box(
                new Coordinate(-90, -180),
                new Coordinate(90, 180));
            var refArray = new ShapesArray(1024);

            var rand = new System.Random(46541577);
            var totalCoordinateCount = 0;
            for (var i = 0; i < 1024; i++)
            {
                var count = rand.Next(10);
                totalCoordinateCount += count;
                var newShape = new List<Coordinate>(count);
                for (var j = 0; j < count; j++)
                {
                    var c = box.GenerateRandomIn();
                    c.Elevation = (short)(short.MaxValue * (rand.NextDouble()));
                    newShape.Add(c);
                }

                var shape = new ShapeEnumerable(newShape);
                refArray[i] = shape;
            }

            using (var stream = new MemoryStream())
            {
                Assert.AreEqual(16 + (1024 * 8) + (totalCoordinateCount * 8) + (totalCoordinateCount * 2),
                    refArray.CopyTo(stream));
                stream.Seek(0, SeekOrigin.Begin);

                var array = ShapesArray.CreateFrom(stream, true, true);
                for (var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    if (refShape.Count == 0)
                    {
                        continue;
                    }
                    var shape = array[i];
                    Assert.IsNotNull(shape);

                    for (var j = 0; j < shape.Count; j++)
                    {
                        Assert.AreEqual(refShape[j].Latitude, shape[j].Latitude);
                        Assert.AreEqual(refShape[j].Longitude, shape[j].Longitude);
                        Assert.AreEqual(refShape[j].Elevation, shape[j].Elevation);
                    }
                }

                stream.Seek(0, SeekOrigin.Begin);
                array = ShapesArray.CreateFrom(stream, false, true);
                for (var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    if (refShape.Count == 0)
                    {
                        continue;
                    }
                    var shape = array[i];
                    Assert.IsNotNull(shape);

                    for (var j = 0; j < shape.Count; j++)
                    {
                        Assert.AreEqual(refShape[j].Latitude, shape[j].Latitude);
                        Assert.AreEqual(refShape[j].Longitude, shape[j].Longitude);
                        Assert.AreEqual(refShape[j].Elevation, shape[j].Elevation);
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
                array.Set(0, new Coordinate(0.0f, 0.1f), new Coordinate(1.0f, 1.1f));

                array[0] = null;

                Assert.IsNull(array[0]);
                Assert.IsNull(array[4]);
                Assert.IsNull(array[7]);
                Assert.IsNull(array[2]);
                Assert.IsNull(array[8]);
            }
        }
        
        /// <summary>
        /// Tests adding shapes with elevation.
        /// </summary>
        [Test]
        public void TestAddWithElevation()
        {
            using (var map = new MemoryMapStream())
            {
                var array = new ShapesArray(map, 1024);
                array.Set(0, new Coordinate(0, 0.1f, 10),
                    new Coordinate(1, 1.1f, 11));

                var shape = array[0];
                Assert.IsNotNull(shape);
                Assert.AreEqual(2, shape.Count);
                Assert.AreEqual(0, shape[0].Latitude, .00001);
                Assert.AreEqual(0.1, shape[0].Longitude, .00001);
                Assert.AreEqual(10, shape[0].Elevation);
                Assert.AreEqual(1, shape[1].Latitude, .00001);
                Assert.AreEqual(1.1, shape[1].Longitude, .00001);
                Assert.AreEqual(11, shape[1].Elevation);
            }

            using (var map = new MemoryMapStream())
            {
                var box = new Box(
                    new Coordinate(-90, -180),
                    new Coordinate(90, 180));
                var refArray = new ShapeBase[1024];
                var array = new ShapesArray(map, 1024);

                var rand = new Randomizer(1587341311);
                for (var i = 0; i < 1024; i++)
                {
                    var count = rand.Next(10);
                    var newShape = new List<Coordinate>(count);
                    for (var j = 0; j < count; j++)
                    {
                        newShape.Add(box.GenerateRandomIn());
                    }

                    var shape = new ShapeEnumerable(newShape);
                    refArray[i] = shape;
                    array[i] = shape;
                }

                for (var i = 0; i < refArray.Length; i++)
                {
                    var refShape = refArray[i];
                    var shape = array[i];
                    Assert.IsNotNull(shape);

                    for (var j = 0; j < shape.Count; j++)
                    {
                        Assert.AreEqual(refShape[j].Latitude, shape[j].Latitude);
                        Assert.AreEqual(refShape[j].Longitude, shape[j].Longitude);
                        Assert.AreEqual(refShape[j].Elevation, shape[j].Elevation);
                    }
                }
            }
        }
    }
}