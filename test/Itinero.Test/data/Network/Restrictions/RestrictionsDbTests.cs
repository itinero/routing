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

using Itinero.Data.Network.Restrictions;
using NUnit.Framework;
using System.IO;

namespace Itinero.Test.Data.Network.Restrictions
{
    /// <summary>
    /// Contains tests for the restriction db.
    /// </summary>
    [TestFixture]
    public class RestrictionsDbTests
    {
        /// <summary>
        /// Tests adding a restriction.
        /// </summary>
        [Test]
        public void TestAddOne()
        {
            var db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);

            var enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests adding multiple restriction.
        /// </summary>
        [Test]
        public void TestAdd()
        {
            var db = new RestrictionsDb(1024);

            db.Add(1, 2, 3);

            var enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            db.Add(10, 11, 12);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(11));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(12));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);

            db.Add(10, 111, 222);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(111, enumerator[1]);
            Assert.AreEqual(222, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(222));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(111, enumerator[1]);
            Assert.AreEqual(222, enumerator[2]);

            db.Add(12, 2, 3);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(12));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            
            db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(1, 2, 3, 4);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests switching vertices.
        /// </summary>
        [Test]
        public void TestSwitch()
        {
            var db = new RestrictionsDb(1024);
            db.Add(2, 1, 3);
            db.Switch(2, 1);
            
            var enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(1, 2, 3, 4);
            db.Switch(1, 10);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);

            db.Switch(2, 20);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);

            db.Switch(3, 30);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);

            db.Switch(4, 40);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());

            db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(3, 2, 1);
            db.Add(3, 2, 1, 4);
            db.Add(1, 2, 3, 4);
            db.Switch(1, 10);
            db.Switch(2, 20);
            db.Switch(3, 30);
            db.Switch(4, 40);
            
            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(30, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(10, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(30, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(10, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(30));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(30, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(10, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(30, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(10, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());

            db = new RestrictionsDb(1024);
            db.Add(2, 1, 3);
            db.Switch(4, 1);
            db.Switch(4, 8);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(2));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(2, enumerator[0]);
            Assert.AreEqual(8, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(2, enumerator[0]);
            Assert.AreEqual(8, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests adding multiple restrictions with the same start.
        /// </summary>
        [Test]
        public void TestAddMultipleSameStart()
        {
            var db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(1, 2, 3, 4);
            db.Add(1, 2, 3, 4, 5);

            var enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(5, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.AreEqual(5, enumerator[4]);
            Assert.IsFalse(enumerator.MoveNext());

            db.Switch(6, 1);

            Assert.IsTrue(enumerator.MoveTo(6));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(6, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(6, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(5, enumerator.Count);
            Assert.AreEqual(6, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.AreEqual(5, enumerator[4]);
            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests serializing restrictions.
        /// </summary>
        [Test]
        public void TestSerializeDeserialize()
        {
            var db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(10, 11, 12);
            db.Add(10, 111, 222);
            db.Add(12, 2, 3);

            using (var stream = new MemoryStream())
            {
                var size = db.Serialize(stream);
                Assert.AreEqual(stream.Position, size);

                stream.Seek(0, SeekOrigin.Begin);
                db = RestrictionsDb.Deserialize(stream, null);
            }

            var enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(12));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(1, 2, 3, 4);

            using (var stream = new MemoryStream())
            {
                var size = db.Serialize(stream);
                Assert.AreEqual(stream.Position, size);

                stream.Seek(0, SeekOrigin.Begin);
                db = RestrictionsDb.Deserialize(stream, null);
            }

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());
        }

        /// <summary>
        /// Tests serializing restrictions.
        /// </summary>
        [Test]
        public void TestSerializeDeserializeNoCacheProfile()
        {
            var db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(10, 11, 12);
            db.Add(10, 111, 222);
            db.Add(12, 2, 3);

            var stream = new MemoryStream();
            var size = db.Serialize(stream);
            Assert.AreEqual(stream.Position, size);

            stream.Seek(0, SeekOrigin.Begin);
            db = RestrictionsDb.Deserialize(stream, RestrictionsDbProfile.NoCache);

            var enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(12));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            db = new RestrictionsDb(1024);
            db.Add(1, 2, 3);
            db.Add(1, 2, 3, 4);

            stream.Dispose();
            stream = new MemoryStream();
            size = db.Serialize(stream);
            Assert.AreEqual(stream.Position, size);

            stream.Seek(0, SeekOrigin.Begin);
            db = RestrictionsDb.Deserialize(stream, RestrictionsDbProfile.NoCache);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}