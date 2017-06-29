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

using Itinero.Attributes;
using NUnit.Framework;
using System.IO;

namespace Itinero.Test.Attributes
{
    /// <summary>
    /// Contains tests for the mapped attributes index.
    /// </summary>
    [TestFixture]
    public class MappedAttributesIndexTests
    {
        /// <summary>
        /// Tests adding data.
        /// </summary>
        [Test]
        public void TestSetGet()
        {
            var map = new MappedAttributesIndex();

            map[123] = new AttributeCollection(
                new Attribute("highway", "residential"));
            map[1234] = new AttributeCollection(
                new Attribute("highway", "residential"));
            map[12345] = new AttributeCollection(
                new Attribute("highway", "primary"));

            Assert.AreEqual(map[123], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[1234], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[12345], new AttributeCollection(
                new Attribute("highway", "primary")));
        }

        /// <summary>
        /// Tests optimize.
        /// </summary>
        [Test]
        public void TestOptimize()
        {
            var map = new MappedAttributesIndex();

            map[12345] = new AttributeCollection(
                new Attribute("highway", "primary"));
            map[123] = new AttributeCollection(
                new Attribute("highway", "residential"));
            map[1234] = new AttributeCollection(
                new Attribute("highway", "residential"));
            map[12] = new AttributeCollection(
                new Attribute("highway", "secondary"));
            map[1] = new AttributeCollection(
                new Attribute("highway", "secondary"));

            Assert.AreEqual(map[1], new AttributeCollection(
                new Attribute("highway", "secondary")));
            Assert.AreEqual(map[12], new AttributeCollection(
                new Attribute("highway", "secondary")));
            Assert.AreEqual(map[123], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[1234], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[12345], new AttributeCollection(
                new Attribute("highway", "primary")));

            map.Optimize();

            Assert.IsTrue(map.IsOptimized);

            Assert.AreEqual(map[1], new AttributeCollection(
                new Attribute("highway", "secondary")));
            Assert.AreEqual(map[12], new AttributeCollection(
                new Attribute("highway", "secondary")));
            Assert.AreEqual(map[123], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[1234], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[12345], new AttributeCollection(
                new Attribute("highway", "primary")));
        }

        /// <summary>
        /// Tests serializing/deserializing.
        /// </summary>
        [Test]
        public void TestSerializeDeserialize()
        {
            var map = new MappedAttributesIndex();

            map[12345] = new AttributeCollection(
                new Attribute("highway", "primary"));
            map[123] = new AttributeCollection(
                new Attribute("highway", "residential"));
            map[1234] = new AttributeCollection(
                new Attribute("highway", "residential"));
            map[12] = new AttributeCollection(
                new Attribute("highway", "secondary"));
            map[1] = new AttributeCollection(
                new Attribute("highway", "secondary"));

            using (var stream = new MemoryStream())
            {
                map.Serialize(stream);
                Assert.IsTrue(map.IsOptimized);

                Assert.AreEqual(map[1], new AttributeCollection(
                    new Attribute("highway", "secondary")));
                Assert.AreEqual(map[12], new AttributeCollection(
                    new Attribute("highway", "secondary")));
                Assert.AreEqual(map[123], new AttributeCollection(
                    new Attribute("highway", "residential")));
                Assert.AreEqual(map[1234], new AttributeCollection(
                    new Attribute("highway", "residential")));
                Assert.AreEqual(map[12345], new AttributeCollection(
                    new Attribute("highway", "primary")));

                stream.Seek(0, SeekOrigin.Begin);

                map = MappedAttributesIndex.Deserialize(stream, null);
            }

            Assert.AreEqual(map[1], new AttributeCollection(
                new Attribute("highway", "secondary")));
            Assert.AreEqual(map[12], new AttributeCollection(
                new Attribute("highway", "secondary")));
            Assert.AreEqual(map[123], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[1234], new AttributeCollection(
                new Attribute("highway", "residential")));
            Assert.AreEqual(map[12345], new AttributeCollection(
                new Attribute("highway", "primary")));
        }
    }
}