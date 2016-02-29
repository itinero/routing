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

using Itinero.Network.Restrictions;
using NUnit.Framework;

namespace Itinero.Test.Network.Restrictions
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

            db.Add(10, 11, 12);

            enumerator = db.GetEnumerator();
            Assert.IsTrue(enumerator.MoveTo(10));
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
        }
    }
}