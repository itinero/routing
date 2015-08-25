// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using NUnit.Framework;
using OsmSharp.Routing.CH.Preprocessing;

namespace OsmSharp.Test.Unittests.Routing.CH
{
    /// <summary>
    /// Contains tests for the contracted edge class.
    /// </summary>
    [TestFixture]
    public class CHEdgeDataTests
    {
        /// <summary>
        /// Tests the uncontracted version.
        /// </summary>
        [Test]
        public void TestUncontracted()
        {
            var edge = new CHEdgeData(123, true, true, true, 123.45f);

            Assert.AreEqual(7, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(123, edge.Tags);
            Assert.AreEqual(true, edge.Forward);
            Assert.AreEqual(true, edge.CanMoveBackward);
            Assert.AreEqual(true, edge.CanMoveForward);
            Assert.AreEqual(uint.MaxValue, edge.ContractedId);

            Assert.IsFalse(edge.IsContracted);
            Assert.IsTrue(edge.RepresentsNeighbourRelations);

            edge = new CHEdgeData(123, false, true, true, 123.45f);

            Assert.AreEqual(3, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(123, edge.Tags);
            Assert.AreEqual(false, edge.Forward);
            Assert.AreEqual(true, edge.CanMoveForward);
            Assert.AreEqual(true, edge.CanMoveBackward);
            Assert.AreEqual(uint.MaxValue, edge.ContractedId);

            Assert.IsFalse(edge.IsContracted);
            Assert.IsTrue(edge.RepresentsNeighbourRelations);

            edge = new CHEdgeData(123, true, false, true, 123.45f);

            Assert.AreEqual(6, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(123, edge.Tags);
            Assert.AreEqual(true, edge.Forward);
            Assert.AreEqual(false, edge.CanMoveForward);
            Assert.AreEqual(true, edge.CanMoveBackward);
            Assert.AreEqual(uint.MaxValue, edge.ContractedId);

            Assert.IsFalse(edge.IsContracted);
            Assert.IsTrue(edge.RepresentsNeighbourRelations);

            edge = new CHEdgeData(123, true, true, false, 123.45f);

            Assert.AreEqual(5, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(123, edge.Tags);
            Assert.AreEqual(true, edge.Forward);
            Assert.AreEqual(true, edge.CanMoveForward);
            Assert.AreEqual(false, edge.CanMoveBackward);
            Assert.AreEqual(uint.MaxValue, edge.ContractedId);

            Assert.IsFalse(edge.IsContracted);
            Assert.IsTrue(edge.RepresentsNeighbourRelations);
        }

        /// <summary>
        /// Tests the uncontracted version.
        /// </summary>
        [Test]
        public void TestContracted()
        {
            var edge = new CHEdgeData(123, true, true, 123.45f);

            Assert.AreEqual(11, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(uint.MaxValue, edge.Tags);
            Assert.AreEqual(false, edge.Forward);
            Assert.AreEqual(true, edge.CanMoveBackward);
            Assert.AreEqual(true, edge.CanMoveForward);
            Assert.AreEqual(123, edge.ContractedId);

            Assert.IsTrue(edge.IsContracted);
            Assert.IsFalse(edge.RepresentsNeighbourRelations);

            edge = new CHEdgeData(123, false, true, 123.45f);

            Assert.AreEqual(10, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(uint.MaxValue, edge.Tags);
            Assert.AreEqual(false, edge.Forward);
            Assert.AreEqual(false, edge.CanMoveForward);
            Assert.AreEqual(true, edge.CanMoveBackward);
            Assert.AreEqual(123, edge.ContractedId);

            Assert.IsTrue(edge.IsContracted);
            Assert.IsFalse(edge.RepresentsNeighbourRelations);

            edge = new CHEdgeData(123, true, false, 123.45f);

            Assert.AreEqual(9, edge.Meta);
            Assert.AreEqual(123.45f, edge.Weight);
            Assert.AreEqual(123, edge.Value);

            Assert.AreEqual(uint.MaxValue, edge.Tags);
            Assert.AreEqual(false, edge.Forward);
            Assert.AreEqual(true, edge.CanMoveForward);
            Assert.AreEqual(false, edge.CanMoveBackward);
            Assert.AreEqual(123, edge.ContractedId);

            Assert.IsTrue(edge.IsContracted);
            Assert.IsFalse(edge.RepresentsNeighbourRelations);
        }
    }
}