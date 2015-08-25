// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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
using OsmSharp.Routing.Graph.Routing;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// Does tests on the path segment class.
    /// </summary>
    [TestFixture]
    public class PathSegmentTests
    {
        /// <summary>
        /// Tests path segment equality.
        /// </summary>
        [Test]
        public void TestPathSegmentEqualityOperator()
        {
            PathSegment<long> segment1 = new PathSegment<long>(1);
            PathSegment<long> segment1_clone = new PathSegment<long>(1);

            Assert.IsTrue(segment1 == segment1_clone);
            Assert.IsFalse(segment1 != segment1_clone);

            PathSegment<long> segment2 = new PathSegment<long>(2, 10, segment1);
            PathSegment<long> segment2_clone = new PathSegment<long>(2, 10, segment1_clone);

            Assert.IsTrue(segment2 == segment2_clone);
            Assert.IsFalse(segment2 != segment2_clone);

            PathSegment<long> segment2_different_weight = new PathSegment<long>(2, 11, segment1_clone);

            Assert.IsFalse(segment2 == segment2_different_weight);
            Assert.IsTrue(segment2 != segment2_different_weight);

            PathSegment<long> segment2_different = new PathSegment<long>(2);

            Assert.IsFalse(segment2 == segment2_different);
            Assert.IsTrue(segment2 != segment2_different);
        }

        /// <summary>
        /// Tests concatenation of a path segment.
        /// 
        /// Regression Test: Concatenation returns weight of the second route only not their sum.
        /// </summary>
        [Test]
        public void TestPathSegmentConcatenation()
        {
            PathSegment<long> segment12 = new PathSegment<long>(2, 10, new PathSegment<long>(1)); // 1 -> 2
            PathSegment<long> segment23 = new PathSegment<long>(3, 20, new PathSegment<long>(2)); // 2 -> 3

            PathSegment<long> segment_123 = segment23.ConcatenateAfter(segment12);
            Assert.AreEqual(3, segment_123.VertexId);
            Assert.AreEqual(2, segment_123.From.VertexId);
            Assert.AreEqual(1, segment_123.From.From.VertexId);
            Assert.AreEqual(30, segment_123.Weight);

            PathSegment<long> segment123 = new PathSegment<long>(3, 22, new PathSegment<long>(2, 10, new PathSegment<long>(1))); // 1 -> 2 -> 3
            PathSegment<long> segment345 = new PathSegment<long>(5, 23, new PathSegment<long>(4, 20, new PathSegment<long>(3))); // 3 -> 4 -> 5

            PathSegment<long> segment_12345 = segment345.ConcatenateAfter(segment123);
            Assert.AreEqual(5, segment_12345.VertexId);
            Assert.AreEqual(4, segment_12345.From.VertexId);
            Assert.AreEqual(3, segment_12345.From.From.VertexId);
            Assert.AreEqual(2, segment_12345.From.From.From.VertexId);
            Assert.AreEqual(1, segment_12345.From.From.From.From.VertexId);
            Assert.AreEqual(45, segment_12345.Weight);
        }

        /// <summary>
        /// Tests reversing the path segment.
        /// </summary>
        [Test]
        public void TestPathSegmentReverse()
        {
            // define segment 4 -(10)> 3 -(10)> 2 -(10)> 1
            var segment = new PathSegment<long>(1, 30,
                new PathSegment<long>(2, 20,
                    new PathSegment<long>(3, 10, new PathSegment<long>(4))));

            // reverse segment to 1 -(10)> 2 -(20)> 3 -(30)> 4
            var reverse = segment.Reverse();
            Assert.AreEqual(segment.Weight, reverse.Weight);
            Assert.AreEqual(4, reverse.VertexId);

            Assert.AreEqual(3, reverse.From.VertexId);
            Assert.AreEqual(20, reverse.From.Weight);

            Assert.AreEqual(2, reverse.From.From.VertexId);
            Assert.AreEqual(10, reverse.From.From.Weight);

            Assert.AreEqual(1, reverse.From.From.From.VertexId);
            Assert.AreEqual(0, reverse.From.From.From.Weight);
        }
    }
}