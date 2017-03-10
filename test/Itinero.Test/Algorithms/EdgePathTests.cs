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
