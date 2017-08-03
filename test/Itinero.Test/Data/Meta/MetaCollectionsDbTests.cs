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
using Itinero.Data.Network.Edges;
using NUnit.Framework;
using System.IO;

namespace Itinero.Test.Data.Meta
{
    /// <summary>
    /// Contains tests for the meta collections db.
    /// </summary>
    [TestFixture]
    public class MetaCollectionsDbTests
    {
        /// <summary>
        /// Tests creating a db.
        /// </summary>
        [Test]
        public void TestCreate()
        {
            var db = new MetaCollectionDb();
            
            MetaCollection<int> col;
            Assert.IsFalse(db.TryGet<int>("not-there", out col));
        }

        /// <summary>
        /// Tests add get collections.
        /// </summary>
        [Test]
        public void TestAddGet()
        {
            var db = new MetaCollectionDb();

            var intCol = db.AddInt32("int");
            intCol[1023] = 1;
            var dblCol = db.AddDouble("double");
            dblCol[1023] = 1;

            Assert.IsTrue(db.TryGet<int>("int", out intCol));
            Assert.IsTrue(db.TryGet<double>("double", out dblCol));
        }

        /// <summary>
        /// Tests serialize/deserialize.
        /// </summary>
        [Test]
        public void TestSerializeDeserialize()
        {
            var db = new MetaCollectionDb();

            var intCol = db.AddInt32("int");
            var dblCol = db.AddDouble("double");

            var size = 15213;
            for (uint i = 0; i < size; i++)
            {
                intCol[i] = (int)(i * 2);
            }
            for (uint i = 0; i < size; i++)
            {
                dblCol[i] = i * .1;
            }

            using (var stream = new MemoryStream())
            {
                db.Serialize(stream);

                stream.Seek(0, SeekOrigin.Begin);

                db = MetaCollectionDb.Deserialize(stream, null);
            }

            Assert.IsTrue(db.TryGet<int>("int", out intCol));
            Assert.IsTrue(db.TryGet<double>("double", out dblCol));

            for (uint i = 0; i < intCol.Count; i++)
            {
                Assert.AreEqual((int)i * 2, intCol[i]);
            }
            for (uint i = 0; i < dblCol.Count; i++)
            {
                Assert.AreEqual(i * .1, dblCol[i]);
            }
        }
    }
}