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
using Itinero.Attributes;
using Reminiscence.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Itinero.Test.Attributes
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

            using (var map = new MemoryMapStream())
            {
                index = new AttributesIndex(map);
                Assert.IsFalse(index.IsReadonly);

                index = new AttributesIndex(map, AttributesIndexMode.IncreaseOne);
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
            Assert.AreEqual(1, index.Add(new AttributeCollection()));
            Assert.AreEqual(2, index.Add(new Attribute("key1", "value1"))); // adds 9 bytes to index's index, 1 byte for size and two 4-byte points to string-table.
            Assert.AreEqual(2, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(11, index.Add(new Attribute("key2", "value1"))); // adds another 9 bytes to index's index, 1 byte for size and two 4-byte points to string-table.
            Assert.AreEqual(20, index.Add(new Attribute("key2", "value2"))); // adds another 9 bytes to index's index, 1 byte for size and two 4-byte points to string-table.

            index = new AttributesIndex(AttributesIndexMode.IncreaseOne | AttributesIndexMode.ReverseStringIndex | 
                AttributesIndexMode.ReverseCollectionIndex);

            Assert.AreEqual(0, index.Add(null));
            Assert.AreEqual(1, index.Add(new AttributeCollection()));
            Assert.AreEqual(2, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(2, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(3, index.Add(new Attribute("key2", "value1")));
            Assert.AreEqual(4, index.Add(new Attribute("key2", "value2")));

            index = new AttributesIndex(AttributesIndexMode.ReverseStringIndexKeysOnly);

            Assert.AreEqual(0, index.Add(null));
            Assert.AreEqual(1, index.Add(new AttributeCollection()));
            Assert.AreEqual(2, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(11, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(20, index.Add(new Attribute("key2", "value1")));
            Assert.AreEqual(29, index.Add(new Attribute("key2", "value2")));

            index = new AttributesIndex(AttributesIndexMode.None);

            Assert.AreEqual(0, index.Add(null));
            Assert.AreEqual(1, index.Add(new AttributeCollection()));
            Assert.AreEqual(2, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(11, index.Add(new Attribute("key1", "value1")));
            Assert.AreEqual(20, index.Add(new Attribute("key2", "value1")));
            Assert.AreEqual(29, index.Add(new Attribute("key2", "value2")));
        }

        /// <summary>
        /// Gets getting attributes.
        /// </summary>
        [Test]
        public void TestGet()
        {
            var index = new AttributesIndex();

            var id1 = index.Add(new Attribute("key1", "value1"));
            var id2 = index.Add(new Attribute("key2", "value1"));
            var id3 = index.Add(new Attribute("key2", "value2"));

            var attributes = index.Get(id1);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key1", attributes.First().Key);
            Assert.AreEqual("value1", attributes.First().Value);
            attributes = index.Get(id2);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key2", attributes.First().Key);
            Assert.AreEqual("value1", attributes.First().Value);
            attributes = index.Get(id3);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key2", attributes.First().Key);
            Assert.AreEqual("value2", attributes.First().Value);

            var random = new System.Random(116542346);
            var keys = 100;
            var values = 100000;
            index = new AttributesIndex();
            var refIndex = new Dictionary<uint, IAttributeCollection>();
            for(var i = 0; i < 1000; i++)
            {
                attributes = new AttributeCollection();
                for (var j = 0; j < random.Next(8); j++)
                {
                    attributes.AddOrReplace(new Attribute(
                        string.Format("k{0}", random.Next(keys)),
                        string.Format("v{0}", random.Next(values))));
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
                    Assert.IsTrue(attributes.Contains(attribute.Key, attribute.Value));
                }

                foreach (var attribute in attributes)
                {
                    Assert.IsTrue(refAttribute.Value.Contains(attribute.Key, attribute.Value));
                }
            }

            index = new AttributesIndex(AttributesIndexMode.IncreaseOne);

            id1 = index.Add(new Attribute("key1", "value1"));
            id2 = index.Add(new Attribute("key2", "value1"));
            id3 = index.Add(new Attribute("key2", "value2"));

            attributes = index.Get(id1);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key1", attributes.First().Key);
            Assert.AreEqual("value1", attributes.First().Value);
            attributes = index.Get(id2);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key2", attributes.First().Key);
            Assert.AreEqual("value1", attributes.First().Value);
            attributes = index.Get(id3);
            Assert.IsNotNull(attributes);
            Assert.AreEqual(1, attributes.Count);
            Assert.AreEqual("key2", attributes.First().Key);
            Assert.AreEqual("value2", attributes.First().Value);

            random = new System.Random(116542346);
            keys = 100;
            values = 100000;
            index = new AttributesIndex(AttributesIndexMode.IncreaseOne);
            refIndex = new Dictionary<uint, IAttributeCollection>();
            for (var i = 0; i < 1000; i++)
            {
                attributes = new AttributeCollection();
                for (var j = 0; j < random.Next(8); j++)
                {
                    attributes.AddOrReplace(new Attribute(
                        string.Format("k{0}", random.Next(keys)),
                        string.Format("v{0}", random.Next(values))));
                }

                var id = index.Add(attributes);
                refIndex[id] = attributes;
            }

            foreach (var refAttribute in refIndex)
            {
                attributes = index.Get(refAttribute.Key);
                Assert.AreEqual(refAttribute.Value.Count, attributes.Count);

                foreach (var attribute in refAttribute.Value)
                {
                    Assert.IsTrue(attributes.Contains(attribute.Key, attribute.Value));
                }

                foreach (var attribute in attributes)
                {
                    Assert.IsTrue(refAttribute.Value.Contains(attribute.Key, attribute.Value));
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

                Assert.AreEqual(19, index.Serialize(memoryStream));
                Assert.AreEqual(19, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();
                index.Add(new Attribute("1", "2"));

                Assert.AreEqual(34, index.Serialize(memoryStream));
                Assert.AreEqual(34, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("1", "2"));

                Assert.AreEqual(34, index.Serialize(memoryStream));
                Assert.AreEqual(34, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex();
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("2", "1"));

                Assert.AreEqual(43, index.Serialize(memoryStream));
                Assert.AreEqual(43, memoryStream.Position);
            }


            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex(AttributesIndexMode.IncreaseOne);

                Assert.AreEqual(16 + 9 + 2, index.Serialize(memoryStream));
                Assert.AreEqual(16 + 9 + 2, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex(AttributesIndexMode.IncreaseOne);
                index.Add(new Attribute("1", "2"));

                Assert.AreEqual(31 + 3 + 8 + 4, index.Serialize(memoryStream));
                Assert.AreEqual(31 + 3 + 8 + 4, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex(AttributesIndexMode.IncreaseOne | AttributesIndexMode.ReverseAll);
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("1", "2"));

                Assert.AreEqual(31 + 3 + 8 + 4, index.Serialize(memoryStream));
                Assert.AreEqual(31 + 3 + 8 + 4, memoryStream.Position);
            }
            using (var memoryStream = new MemoryStream())
            {
                var index = new AttributesIndex(AttributesIndexMode.IncreaseOne | AttributesIndexMode.ReverseAll);
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("1", "2"));
                index.Add(new Attribute("2", "1"));

                Assert.AreEqual(40 + 3 + 8 + 4 * 2, index.Serialize(memoryStream));
                Assert.AreEqual(40 + 3 + 8 + 4 * 2, memoryStream.Position);
            }
        }

        /// <summary>
        /// Tests deserialization.
        /// </summary>
        [Test]
        public void TestDeserialize()
        {
            var refIndex = new AttributesIndex();

            var id1 = refIndex.Add(new Attribute("key1", "value1"));
            var id2 = refIndex.Add(new Attribute("key2", "value1"));
            var id3 = refIndex.Add(new Attribute("key2", "value2"));

            using (var memoryStream = new MemoryStream())
            {
                var size = refIndex.Serialize(memoryStream);
                Assert.AreEqual(size, memoryStream.Position);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var index = AttributesIndex.Deserialize(memoryStream, false);
                Assert.AreEqual(size, memoryStream.Position);

                var attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value2", attributes.First().Value);

                memoryStream.Seek(0, SeekOrigin.Begin);
                index = AttributesIndex.Deserialize(memoryStream, true);
                Assert.AreEqual(size, memoryStream.Position);

                attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value2", attributes.First().Value);
            }

            var random = new System.Random(116542346);
            var keys = 100;
            var values = 100000;
            refIndex = new AttributesIndex();
            var refIndexRef = new Dictionary<uint, IAttributeCollection>();
            for (var i = 0; i < 1000; i++)
            {
                var attributes = new AttributeCollection();
                for (var j = 0; j < random.Next(8); j++)
                {
                    attributes.AddOrReplace(new Attribute(
                        string.Format("k{0}", random.Next(keys)),
                        string.Format("v{0}", random.Next(values))));
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
                        Assert.IsTrue(attributes.Contains(attribute.Key, attribute.Value));
                    }

                    foreach (var attribute in attributes)
                    {
                        Assert.IsTrue(refAttribute.Value.Contains(attribute.Key, attribute.Value));
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
                        Assert.IsTrue(attributes.Contains(attribute.Key, attribute.Value));
                    }

                    foreach (var attribute in attributes)
                    {
                        Assert.IsTrue(refAttribute.Value.Contains(attribute.Key, attribute.Value));
                    }
                }
            }

            refIndex = new AttributesIndex(AttributesIndexMode.IncreaseOne);

            id1 = refIndex.Add(new Attribute("key1", "value1"));
            id2 = refIndex.Add(new Attribute("key2", "value1"));
            id3 = refIndex.Add(new Attribute("key2", "value2"));

            using (var memoryStream = new MemoryStream())
            {
                var size = refIndex.Serialize(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var index = AttributesIndex.Deserialize(memoryStream, false);
                Assert.AreEqual(size, memoryStream.Position);

                var attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value2", attributes.First().Value);

                memoryStream.Seek(0, SeekOrigin.Begin);
                index = AttributesIndex.Deserialize(memoryStream, true);
                Assert.AreEqual(size, memoryStream.Position);

                attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value2", attributes.First().Value);
            }

            random = new System.Random(116542346);
            keys = 100;
            values = 100000;
            refIndex = new AttributesIndex(AttributesIndexMode.IncreaseOne);
            refIndexRef = new Dictionary<uint, IAttributeCollection>();
            for (var i = 0; i < 1000; i++)
            {
                var attributes = new AttributeCollection();
                for (var j = 0; j < random.Next(8); j++)
                {
                    attributes.AddOrReplace(new Attribute(
                        string.Format("k{0}", random.Next(keys)),
                        string.Format("v{0}", random.Next(values))));
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
                        Assert.IsTrue(attributes.Contains(attribute.Key, attribute.Value));
                    }

                    foreach (var attribute in attributes)
                    {
                        Assert.IsTrue(refAttribute.Value.Contains(attribute.Key, attribute.Value));
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
                        Assert.IsTrue(attributes.Contains(attribute.Key, attribute.Value));
                    }

                    foreach (var attribute in attributes)
                    {
                        Assert.IsTrue(refAttribute.Value.Contains(attribute.Key, attribute.Value));
                    }
                }
            }
        }

        /// <summary>
        /// Tests attributes index being writable after deserialization.
        /// </summary>
        [Test]
        public void TestWritableAfterDeserialization()
        {
            var refIndex = new AttributesIndex();

            var id1 = refIndex.Add(new Attribute("key1", "value1"));
            var id2 = refIndex.Add(new Attribute("key2", "value1"));
            var id3 = refIndex.Add(new Attribute("key2", "value2"));

            using (var memoryStream = new MemoryStream())
            {
                var size = refIndex.Serialize(memoryStream);

                memoryStream.Seek(0, SeekOrigin.Begin);
                var index = AttributesIndex.Deserialize(memoryStream, false);

                var id4 = index.Add(new Attribute("key3", "value4"));

                var attributes = index.Get(id1);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key1", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                
                attributes = index.Get(id2);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value1", attributes.First().Value);
                
                attributes = index.Get(id3);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key2", attributes.First().Key);
                Assert.AreEqual("value2", attributes.First().Value);
                
                attributes = index.Get(id4);
                Assert.IsNotNull(attributes);
                Assert.AreEqual(1, attributes.Count);
                Assert.AreEqual("key3", attributes.First().Key);
                Assert.AreEqual("value4", attributes.First().Value);
            }
        }
    }
}
