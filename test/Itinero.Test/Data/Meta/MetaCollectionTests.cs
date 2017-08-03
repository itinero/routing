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

using Itinero.Data;
using NUnit.Framework;
using System.IO;

namespace Itinero.Test.Data.Meta
{
    /// <summary>
    /// Contains tests for meta collections.
    /// </summary>
    [TestFixture]
    public class MetaCollectionTests
    {
        /// <summary>
        /// Tests creating a new collection.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            var collection = new MetaCollection<int>(1024);

            Assert.AreEqual(0, collection.Count);
        }

        /// <summary>
        /// Tests getting/setting data.
        /// </summary>
        [Test]
        public void TestGetSet()
        {
            var collection = new MetaCollection<int>(0);

            var size = 1024;
            for (uint i = 0; i < size; i++)
            {
                collection[i] = (int)(i * 2);
            }

            for (uint i = 0; i < size; i++)
            {
                Assert.AreEqual(i * 2, collection[i]);
            }
        }

        /// <summary>
        /// Tests serialize/deserialize.
        /// </summary>
        [Test]
        public void SerializeDeserialize()
        {
            var collection = new MetaCollection<int>(0);

            var size = 1024;
            for (uint i = 0; i < size; i++)
            {
                collection[i] = (int)(i * 2);
            }

            using (var stream = new MemoryStream())
            {
                collection.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);

                collection = MetaCollection.Deserialize(stream, null) as MetaCollection<int>;
            }

            for (uint i = 0; i < size; i++)
            {
                Assert.AreEqual(i * 2, collection[i]);
            }
        }
    }
}
