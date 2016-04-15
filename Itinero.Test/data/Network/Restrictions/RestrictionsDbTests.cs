// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Data.Network.Restrictions;
using NUnit.Framework;

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
            Assert.IsTrue(enumerator.MoveToFirst(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(3));
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
            Assert.IsTrue(enumerator.MoveToFirst(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(3));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            db.Add(10, 11, 12);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(12));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(11, enumerator[1]);
            Assert.AreEqual(12, enumerator[2]);

            db.Add(10, 111, 222);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(10));
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
            Assert.IsTrue(enumerator.MoveToLast(222));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(111, enumerator[1]);
            Assert.AreEqual(222, enumerator[2]);

            db.Add(12, 2, 3);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(12));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(12, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(3));
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
            Assert.IsTrue(enumerator.MoveToFirst(1));
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
            Assert.IsTrue(enumerator.MoveToFirst(1));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(1, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(3));
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
            db.Switch(2, 20);
            db.Switch(3, 30);
            db.Switch(4, 40);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(10));
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
            Assert.IsTrue(enumerator.MoveToFirst(10));
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

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(30));
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
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(30));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(10));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(30, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(10, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());
            
            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(40));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(10, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(30, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(30, enumerator[0]);
            Assert.AreEqual(20, enumerator[1]);
            Assert.AreEqual(10, enumerator[2]);
            Assert.AreEqual(40, enumerator[3]);
            Assert.IsFalse(enumerator.MoveNext());

            db = new RestrictionsDb(1024);
            db.Add(2, 1, 3);
            db.Switch(4, 1);
            db.Switch(4, 8);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(2));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(2, enumerator[0]);
            Assert.AreEqual(8, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToLast(3));
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
            Assert.IsTrue(enumerator.MoveToFirst(1));
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

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveToFirst(6));
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(5, enumerator.Count);
            Assert.AreEqual(6, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.AreEqual(5, enumerator[4]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(4, enumerator.Count);
            Assert.AreEqual(6, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.AreEqual(4, enumerator[3]);
            Assert.IsTrue(enumerator.MoveNext());
            Assert.AreEqual(3, enumerator.Count);
            Assert.AreEqual(6, enumerator[0]);
            Assert.AreEqual(2, enumerator[1]);
            Assert.AreEqual(3, enumerator[2]);
            Assert.IsFalse(enumerator.MoveNext());
        }
    }
}