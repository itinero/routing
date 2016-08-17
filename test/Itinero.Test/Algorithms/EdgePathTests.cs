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
using NUnit.Framework;


namespace Itinero.Test.Algorithms
{
    /// <summary>
    /// Tests for the edge path and edgepath extensions.
    /// </summary>
    [TestFixture]
    public class EdgePathTests
    {
        /// <summary>
        /// Tests appending edge paths.
        /// </summary>
        [Test]
        public void TestAppend()
        {
            var forwardPath = new EdgePath<float>(2, 10, 20, new EdgePath<float>(1, 5, 10, new EdgePath<float>(0)));
            var backwardPath = new EdgePath<float>(2, 10, -30, new EdgePath<float>(3, 5, -40, new EdgePath<float>(4)));

            var path = forwardPath.Append(backwardPath);
            Assert.IsNotNull(path);
            Assert.AreEqual(4, path.Vertex);
            Assert.AreEqual(20, path.Weight);
            Assert.AreEqual(40, path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(3, path.Vertex);
            Assert.AreEqual(15, path.Weight);
            Assert.AreEqual(30, path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(2, path.Vertex);
            Assert.AreEqual(10, path.Weight);
            Assert.AreEqual(20, path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(1, path.Vertex);
            Assert.AreEqual(5, path.Weight);
            Assert.AreEqual(10, path.Edge);
            path = path.From;
            Assert.IsNotNull(path);
            Assert.AreEqual(0, path.Vertex);
            Assert.AreEqual(0, path.Weight);
            Assert.AreEqual(Constants.NO_EDGE, path.Edge);
            Assert.IsNull(path.From);
        }

        /// <summary>
        /// Tests has vertex.
        /// </summary>
        [Test]
        public void TestHasVertex()
        {
            var path = new EdgePath<float>(2, 10, 20, new EdgePath<float>(1, 5, 10, new EdgePath<float>(0)));

            Assert.IsTrue(path.HasVertex(2));
            Assert.IsTrue(path.HasVertex(1));
            Assert.IsTrue(path.HasVertex(0));
            Assert.IsFalse(path.HasVertex(10));
        }
    }
}
