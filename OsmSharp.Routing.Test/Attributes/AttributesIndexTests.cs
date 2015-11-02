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
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Attributes;
using Reminiscence.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsmSharp.Routing.Test.Attributes
{
    /// <summary>
    /// Contains tests for the attributes index.
    /// </summary>
    [TestFixture]
    public class AttributesIndexTests
    {
        /// <summary>
        /// Tests creating.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            var index = new AttributesIndex();
            Assert.IsFalse(index.IsReadonly);

            index = new AttributesIndex(true);
            Assert.IsTrue(index.IsReadonly);

            using (var map = new MemoryMapStream())
            {
                index = new AttributesIndex();
                Assert.IsFalse(index.IsReadonly);
            }
        }

        /// <summary>
        /// Tests adding attributes.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var index = new AttributesIndex();

            Assert.AreEqual(0, index.Add(null));
            Assert.AreEqual(1, index.Add(new TagsCollection()));
            Assert.AreEqual(2, index.Add(Tag.Create("key1", "value1"))); // adds 9 bytes to index's index, 1 byte for size and two 4-byte points to string-table.
            Assert.AreEqual(2, index.Add(Tag.Create("key1", "value1")));
            Assert.AreEqual(11, index.Add(Tag.Create("key2", "value1"))); // adds another 9 bytes to index's index, 1 byte for size and two 4-byte points to string-table.
            Assert.AreEqual(20, index.Add(Tag.Create("key2", "value2"))); // adds another 9 bytes to index's index, 1 byte for size and two 4-byte points to string-table.
        }

        /// <summary>
        /// Gets getting attributes.
        /// </summary>
        [Test]
        public void TestGet()
        {
            var index = new AttributesIndex();

            var id1 = index.Add(Tag.Create("key1", "value1"));
            var id2 = index.Add(Tag.Create("key2", "value1"));
            var id3 = index.Add(Tag.Create("key2", "value2"));

            var attributes = index.Get(id1);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key1", attributes.First<Tag>().Key);
            Assert.AreEqual("value1", attributes.First<Tag>().Value);
            attributes = index.Get(id2);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key2", attributes.First<Tag>().Key);
            Assert.AreEqual("value1", attributes.First<Tag>().Value);
            attributes = index.Get(id3);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key2", attributes.First<Tag>().Key);
            Assert.AreEqual("value2", attributes.First<Tag>().Value);

            OsmSharp.Math.Random.StaticRandomGenerator.Set(116542346);
            var keys = 100;
            var values = 100000;
            index = new AttributesIndex();
            var refIndex = new Dictionary<uint, TagsCollectionBase>();
            for(var i = 0; i < 1000; i++)
            {
                attributes = new TagsCollection();
                for (var j = 0; j < OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(8); j++)
                {
                    attributes.Add(Tag.Create(
                        string.Format("k{0}", OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(keys)),
                        string.Format("v{0}", OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(values))));
                }

                var id = index.Add(attributes);
                refIndex[id] = attributes;
            }

            foreach (var refAttribute in refIndex)
            {
                attributes = index.Get(refAttribute.Key);
                Assert.AreEqual(refAttribute.Value.Count, attributes.Count);

                foreach(var attribute in refAttribute.Value)
                {
                    Assert.IsTrue(attributes.ContainsKeyValue(attribute.Key, attribute.Value));
                }

                foreach (var attribute in attributes)
                {
                    Assert.IsTrue(refAttribute.Value.ContainsKeyValue(attribute.Key, attribute.Value));
                }
            }
        }

        /// <summary>
        /// Tests serialization.
        /// </summary>
        [Test]
        public void TestSerialize()
        {
            using(var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();

                Assert.AreEqual(16, index.Serialize(memoryStream));
                Assert.AreEqual(16, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();
                index.Add(Tag.Create("1", "2"));

                Assert.AreEqual(31, index.Serialize(memoryStream));
                Assert.AreEqual(31, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();
                index.Add(Tag.Create("1", "2"));
                index.Add(Tag.Create("1", "2"));
                index.Add(Tag.Create("1", "2"));

                Assert.AreEqual(31, index.Serialize(memoryStream));
                Assert.AreEqual(31, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();
                index.Add(Tag.Create("1", "2"));
                index.Add(Tag.Create("1", "2"));
                index.Add(Tag.Create("2", "1"));

                Assert.AreEqual(40, index.Serialize(memoryStream));
                Assert.AreEqual(40, memoryStream.Position);
            }
        }

        /// <summary>
        /// Tests deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var refIndex = new AttributesIndex();

            var id1 = refIndex.Add(Tag.Create("key1", "value1"));
            var id2 = refIndex.Add(Tag.Create("key2", "value1"));
            var id3 = refIndex.Add(Tag.Create("key2", "value2"));

            using (var memoryStream = new MemoryStream())
            {
                var size = refIndex.Serialize(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var index = AttributesIndex.Deserialize(memoryStream, false);
                Assert.AreEqual(size, memoryStream.Position);

                var attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First<Tag>().Key);
                Assert.AreEqual("value1", attributes.First<Tag>().Value);
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First<Tag>().Key);
                Assert.AreEqual("value1", attributes.First<Tag>().Value);
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First<Tag>().Key);
                Assert.AreEqual("value2", attributes.First<Tag>().Value);

                memoryStream.Seek(0, SeekOrigin.Begin);
                index = AttributesIndex.Deserialize(memoryStream, true);
                Assert.AreEqual(size, memoryStream.Position);

                attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First<Tag>().Key);
                Assert.AreEqual("value1", attributes.First<Tag>().Value);
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First<Tag>().Key);
                Assert.AreEqual("value1", attributes.First<Tag>().Value);
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First<Tag>().Key);
                Assert.AreEqual("value2", attributes.First<Tag>().Value);
            }

            OsmSharp.Math.Random.StaticRandomGenerator.Set(116542346);
            var keys = 100;
            var values = 100000;
            refIndex = new AttributesIndex();
            var refIndexRef = new Dictionary<uint, TagsCollectionBase>();
            for (var i = 0; i < 1000; i++)
            {
                var attributes = new TagsCollection();
                for (var j = 0; j < OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(8); j++)
                {
                    attributes.Add(Tag.Create(
                        string.Format("k{0}", OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(keys)),
                        string.Format("v{0}", OsmSharp.Math.Random.StaticRandomGenerator.Get().Generate(values))));
                }

                var id = refIndex.Add(attributes);
                refIndexRef[id] = attributes;
            }

            using (var memoryStream = new MemoryStream())
            {
                var size = refIndex.Serialize(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var index = AttributesIndex.Deserialize(memoryStream, false);
                Assert.AreEqual(size, memoryStream.Position);

                foreach (var refAttribute in refIndexRef)
                {
                    var attributes = index.Get(refAttribute.Key);
                    Assert.AreEqual(refAttribute.Value.Count, attributes.Count);

                    foreach (var attribute in refAttribute.Value)
                    {
                        Assert.IsTrue(attributes.ContainsKeyValue(attribute.Key, attribute.Value));
                    }

                    foreach (var attribute in attributes)
                    {
                        Assert.IsTrue(refAttribute.Value.ContainsKeyValue(attribute.Key, attribute.Value));
                    }
                }

                memoryStream.Seek(0, SeekOrigin.Begin);
                index = AttributesIndex.Deserialize(memoryStream, true);
                Assert.AreEqual(size, memoryStream.Position);

                foreach (var refAttribute in refIndexRef)
                {
                    var attributes = index.Get(refAttribute.Key);
                    Assert.AreEqual(refAttribute.Value.Count, attributes.Count);

                    foreach (var attribute in refAttribute.Value)
                    {
                        Assert.IsTrue(attributes.ContainsKeyValue(attribute.Key, attribute.Value));
                    }

                    foreach (var attribute in attributes)
                    {
                        Assert.IsTrue(refAttribute.Value.ContainsKeyValue(attribute.Key, attribute.Value));
                    }
                }
            }
        }
    }
}
