// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Restrictions;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Restrictions
{
    /// <summary>
    /// Contains test for restriction extension methods.
    /// </summary>
    [TestFixture]
    public class RestrictionExtensionTests
    {
        /// <summary>
        /// Tests shrink for.
        /// </summary>
        [Test]
        public void TestShrinkFor()
        {
            var restriction = new uint[] { 0, 1, 2, 3 };
            var sequence = new uint[] { 0 };

            var shrunk = restriction.ShrinkFor(sequence);
            Assert.AreEqual(4, shrunk.Length);
            Assert.AreEqual(restriction, shrunk);

            sequence = new uint[] { 0, 1 };
            shrunk = restriction.ShrinkFor(sequence);
            Assert.AreEqual(3, shrunk.Length);
            Assert.AreEqual(new uint[] { 1, 2, 3 }, shrunk);
        }

        /// <summary>
        /// Tests shrink for part.
        /// </summary>
        [Test]
        public void TestShrinkForPart()
        {
            var restriction = new uint[] { 0, 1, 2, 3 };
            var sequence = new uint[] { 10, 0 };

            var shrunk = restriction.ShrinkForPart(sequence);
            Assert.AreEqual(4, shrunk.Length);
            Assert.AreEqual(restriction, shrunk);

            sequence = new uint[] { 10, 0, 1 };
            shrunk = restriction.ShrinkForPart(sequence);
            Assert.AreEqual(3, shrunk.Length);
            Assert.AreEqual(new uint[] { 1, 2, 3 }, shrunk);
        }
    }
}