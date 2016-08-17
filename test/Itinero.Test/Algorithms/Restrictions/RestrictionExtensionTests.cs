// Itinero - Routing for .NET
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

using Itinero.Algorithms;
using Itinero.Algorithms.Restrictions;
using NUnit.Framework;
using System.Collections.Generic;

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

        /// <summary>
        /// Tests the contains method.
        /// </summary>
        [Test]
        public void TestContains()
        {
            var s1 = new uint[] { 0, 1, 2, 3 };
            var s2 = new uint[] { 10, 0 };

            int start;
            Assert.IsFalse(s1.Contains(s2, out start));
            Assert.IsFalse(s2.Contains(s1, out start));

            s1 = new uint[] { 0, 1, 2, 3 };
            s2 = new uint[] { 1, 0 };
            
            Assert.IsFalse(s1.Contains(s2, out start));
            Assert.IsFalse(s2.Contains(s1, out start));

            s1 = new uint[] { 0, 1, 2, 3 };
            s2 = new uint[] { 0, 1 };

            Assert.IsTrue(s1.Contains(s2, out start));
            Assert.AreEqual(0, start);
            Assert.IsFalse(s2.Contains(s1, out start));

            s1 = new uint[] { 0, 1, 2, 3 };
            s2 = new uint[] { 2, 3 };

            Assert.IsTrue(s1.Contains(s2, out start));
            Assert.AreEqual(2, start);
            Assert.IsFalse(s2.Contains(s1, out start));

            s1 = new uint[] { 2, 3, 2, 3 };
            s2 = new uint[] { 2, 3 };

            Assert.IsTrue(s1.Contains(s2, out start));
            Assert.AreEqual(0, start);
            Assert.IsFalse(s2.Contains(s1, out start));

            var s1l = new List<uint>(new uint[] { 0, 1, 2, 3 });
            var s2l = new List<uint>(new uint[] { 10, 0 });
            
            Assert.IsFalse(s1l.ToArray().Contains(s2l, out start));
            Assert.IsFalse(s2l.ToArray().Contains(s1l, out start));

            s1l = new List<uint>(new uint[] { 0, 1, 2, 3 });
            s2l = new List<uint>(new uint[] { 1, 0 });

            Assert.IsFalse(s1l.ToArray().Contains(s2l, out start));
            Assert.IsFalse(s2l.ToArray().Contains(s1l, out start));

            s1l = new List<uint>(new uint[] { 0, 1, 2, 3 });
            s2l = new List<uint>(new uint[] { 0, 1 });

            Assert.IsTrue(s1l.ToArray().Contains(s2l, out start));
            Assert.AreEqual(0, start);
            Assert.IsFalse(s2l.ToArray().Contains(s1l, out start));

            s1l = new List<uint>(new uint[] { 0, 1, 2, 3 });
            s2l = new List<uint>(new uint[] { 2, 3 });

            Assert.IsTrue(s1l.ToArray().Contains(s2l, out start));
            Assert.AreEqual(2, start);
            Assert.IsFalse(s2l.ToArray().Contains(s1l, out start));

            s1l = new List<uint>(new uint[] { 2, 3, 2, 3 });
            s2l = new List<uint>(new uint[] { 2, 3 });

            Assert.IsTrue(s1l.ToArray().Contains(s2l, out start));
            Assert.AreEqual(0, start);
            Assert.IsFalse(s2l.ToArray().Contains(s1l, out start));
        }

        /// <summary>
        /// Tests is sequence allowed methods.
        /// </summary>
        [Test]
        public void TestIsSequenceAllowed()
        {
            var restriction = new uint[] { 1, 2, 3 };
            var restrictions = new uint[][] {
                new uint[] { 1, 2, 3 },
                new uint[] { 3, 2, 1 } };

            var s = new uint[] { 10, 20 };            
            Assert.IsTrue(restriction.IsSequenceAllowed(s));
            Assert.IsTrue(restrictions.IsSequenceAllowed(s));

            s = new uint[] { 1, 2 };
            Assert.IsTrue(restriction.IsSequenceAllowed(s));
            Assert.IsTrue(restrictions.IsSequenceAllowed(s));

            s = new uint[] { 1, 2, 3 };
            Assert.IsFalse(restriction.IsSequenceAllowed(s));
            Assert.IsFalse(restrictions.IsSequenceAllowed(s));

            s = new uint[] { 1, 2, 3, 4 };
            Assert.IsFalse(restriction.IsSequenceAllowed(s));
            Assert.IsFalse(restrictions.IsSequenceAllowed(s));

            s = new uint[] { 0, 1, 2, 3 };
            Assert.IsFalse(restriction.IsSequenceAllowed(s));
            Assert.IsFalse(restrictions.IsSequenceAllowed(s));

            s = new uint[] { 3, 2, 1, 0 };
            Assert.IsTrue(restriction.IsSequenceAllowed(s));
            Assert.IsFalse(restrictions.IsSequenceAllowed(s));
        }
    }
}