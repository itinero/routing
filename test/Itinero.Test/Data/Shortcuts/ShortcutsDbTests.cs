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
using Itinero.Data.Shortcuts;
using Itinero.Osm.Vehicles;
using NUnit.Framework;

namespace Itinero.Test.Data.Shortcuts
{
    /// <summary>
    /// Contains tests for the shortcuts db.
    /// </summary>
    [TestFixture]
    public class ShortcutsDbTests
    {
        /// <summary>
        /// Tests adding stops.
        /// </summary>
        [Test]
        public void TestAddStops()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest().FullName);

            db.AddStop(10, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(11, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(12, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
            db.AddStop(13, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
        }

        /// <summary>
        /// Tests getting stops.
        /// </summary>
        [Test]
        public void TestGetStop()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest().FullName);

            db.AddStop(10, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(11, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(12, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
            db.AddStop(13, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            var a = db.GetStop(10);
            Assert.IsNotNull(a);
            Assert.AreEqual(1, a.Count);
        }

        /// <summary>
        /// Tests adding shortcuts.
        /// </summary>
        [Test]
        public void TestAddShortcuts()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest().FullName);

            db.AddStop(10, null);
            db.AddStop(11, null);
            db.AddStop(12, null);
            db.AddStop(13, null);

            db.Add(new uint[] { 10, 100, 101, 102, 103, 11 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            db.Add(new uint[] { 12, 110, 111, 112, 113, 13 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
        }

        /// <summary>
        /// Tests getting shortcuts.
        /// </summary>
        [Test]
        public void TestGetShortcut()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest().FullName);

            db.AddStop(10, null);
            db.AddStop(11, null);
            db.AddStop(12, null);
            db.AddStop(13, null);

            var s1 = db.Add(new uint[] { 10, 100, 101, 102, 103, 11 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            var s2 = db.Add(new uint[] { 12, 110, 111, 112, 113, 13 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            IAttributeCollection meta;
            var s = db.Get(s1, out meta);
            Assert.IsNotNull(s);
            Assert.AreEqual(6, s.Length);
            Assert.IsNotNull(meta);
            Assert.AreEqual(1, meta.Count);
        }

        /// <summary>
        /// Tests getting shortcuts by vertices.
        /// </summary>
        [Test]
        public void TestGetShortcutByVertices()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest().FullName);

            db.AddStop(10, null);
            db.AddStop(11, null);
            db.AddStop(12, null);
            db.AddStop(13, null);

            var s1 = db.Add(new uint[] { 10, 100, 101, 102, 103, 11 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            var s2 = db.Add(new uint[] { 12, 110, 111, 112, 113, 13 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            IAttributeCollection meta;
            var s = db.Get(10, 11, out meta);
            Assert.IsNotNull(s);
            Assert.AreEqual(6, s.Length);
            Assert.IsNotNull(meta);
            Assert.AreEqual(1, meta.Count);

            s = db.Get(12, 13, out meta);
            Assert.IsNotNull(s);
            Assert.AreEqual(6, s.Length);
            Assert.IsNotNull(meta);
            Assert.AreEqual(1, meta.Count);
        }
    }
}