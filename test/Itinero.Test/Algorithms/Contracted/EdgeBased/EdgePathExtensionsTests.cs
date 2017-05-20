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
using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains tests for the edge path extensions.
    /// </summary>
    [TestFixture]
    public class EdgePathExtensionsTests
    {
        /// <summary>
        /// Tests getting the last sequence before the target vertex.
        /// </summary>
        [Test]
        public void TestGetSequence2()
        {
            // build graph.
            var graph = new DirectedDynamicGraph(ContractedEdgeDataSerializer.DynamicFixedSize);
            var e1 = graph.AddEdge(0, 1, 100, null);
            var e2 = graph.AddEdge(1, 2, 100, null);
            var e3 = graph.AddEdge(2, 6, 100, null, 4, 3, 5);
            var e4 = graph.AddEdge(6, 16, 100, null, 11, 7, 15);
            var e5 = graph.AddEdge(16, 17, 100, null);
            var e6 = graph.AddEdge(17, 18, 100, null);
            var enumerator = graph.GetEdgeEnumerator();

            // build and test getting sequences from paths.
            Assert.AreEqual(Constants.NO_VERTEX, (new EdgePath<float>(0)).GetSequence2(enumerator));
            Assert.AreEqual(0, (new EdgePath<float>(1, 100, new EdgePath<float>(0))).GetSequence2(enumerator));
            Assert.AreEqual(1, (new EdgePath<float>(2, 200, new DirectedEdgeId(e2, true), new EdgePath<float>(1, 100, new EdgePath<float>(0)))).GetSequence2(enumerator));
            Assert.AreEqual(5, (new EdgePath<float>(6, 300, new DirectedEdgeId(e3, true), new EdgePath<float>(2, 200, new DirectedEdgeId(e2, true), new EdgePath<float>(1, 100, new EdgePath<float>(0))))).GetSequence2(enumerator));
            Assert.AreEqual(15, (new EdgePath<float>(16, 400, new DirectedEdgeId(e4, true), new EdgePath<float>(6, 300, new DirectedEdgeId(e3, true), new EdgePath<float>(2, 200, new DirectedEdgeId(e2, true), new EdgePath<float>(1, 100, new EdgePath<float>(0)))))).GetSequence2(enumerator));
            Assert.AreEqual(16, (new EdgePath<float>(17, 500, new EdgePath<float>(16, 400, new DirectedEdgeId(e4, true), new EdgePath<float>(6, 300, new DirectedEdgeId(e3, true), new EdgePath<float>(2, 200, new DirectedEdgeId(e2, true), new EdgePath<float>(1, 100, new EdgePath<float>(0))))))).GetSequence2(enumerator));
            Assert.AreEqual(17, (new EdgePath<float>(18, 600, new EdgePath<float>(17, 500, new EdgePath<float>(16, 400, new DirectedEdgeId(e4, true), new EdgePath<float>(6, 300, new DirectedEdgeId(e3, true), new EdgePath<float>(2, 200, new DirectedEdgeId(e2, true), new EdgePath<float>(1, 100, new EdgePath<float>(0)))))))).GetSequence2(enumerator));
        }
    }
}