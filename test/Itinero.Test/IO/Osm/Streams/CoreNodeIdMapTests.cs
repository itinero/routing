//// Itinero - Routing for .NET
//// Copyright (C) 2016 Abelshausen Ben
//// 
//// This file is part of Itinero.
//// 
//// Itinero is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// Itinero is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

//using Itinero.IO.Osm.Streams;
//using NUnit.Framework;
//using System.Collections.Generic;

//namespace Itinero.Test.IO.Osm.Streams
//{
//    /// <summary>
//    /// Contains tests for the core node id map.
//    /// </summary>
//    [TestFixture]
//    public class CoreNodeIdMapTests
//    {
//        /// <summary>
//        /// Tests storing a single pair.
//        /// </summary>
//        [Test]
//        public void TestOnePair()
//        {
//            var map = new CoreNodeIdMap();
//            map.Add(1, 10);

//            Assert.AreEqual(1, map.MaxVerticePerNode());
//            var result = new uint[10];
//            Assert.AreEqual(1, map.Get(1, ref result));
//            Assert.AreEqual(10, result[0]);
//        }

//        /// <summary>
//        /// Tests storing some pairs with some duplicates.
//        /// </summary>
//        [Test]
//        public void TestSomePairs()
//        {
//            var map = new CoreNodeIdMap();
//            map.Add(1, 10);
//            map.Add(1, 11);
//            map.Add(2, 20);
//            map.Add(3, 30);
//            map.Add(3, 31);
//            map.Add(3, 32);
//            map.Add(4, 40);

//            //Assert.AreEqual(3, map.MaxVerticePerNode());
//            var result = new uint[10];
//            Assert.AreEqual(2, map.Get(1, ref result));
//            Assert.AreEqual(10, result[0]);
//            Assert.AreEqual(11, result[1]);
//            Assert.AreEqual(1, map.Get(2, ref result));
//            Assert.AreEqual(20, result[0]);
//            Assert.AreEqual(3, map.Get(3, ref result));
//            Assert.AreEqual(30, result[0]);
//            Assert.IsTrue(result[1] == 31 || result[1] == 32);
//            Assert.IsTrue(result[2] == 31 || result[2] == 32);
//            Assert.AreEqual(1, map.Get(4, ref result));
//            Assert.AreEqual(40, result[0]);
//        }

//        /// <summary>
//        /// Tests enumerable with all nodes.
//        /// </summary>
//        [Test]
//        public void TestNodesEnumerable()
//        {
//            var map = new CoreNodeIdMap();
//            map.Add(1, 10);
//            map.Add(1, 11);
//            map.Add(2, 20);
//            map.Add(3, 30);
//            map.Add(3, 31);
//            map.Add(3, 32);
//            map.Add(4, 40);

//            var nodes = new List<long>(map.Nodes);
//            Assert.AreEqual(4, nodes.Count);
//            Assert.IsTrue(nodes.Contains(1));
//            Assert.IsTrue(nodes.Contains(2));
//            Assert.IsTrue(nodes.Contains(3));
//            Assert.IsTrue(nodes.Contains(4));
//        }
//    }
//}