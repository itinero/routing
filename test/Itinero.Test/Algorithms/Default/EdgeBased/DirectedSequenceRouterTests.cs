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

using Itinero.Algorithms.Default.EdgeBased;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// Contains tests for the directed sequence router.
    /// </summary>
    [TestFixture]
    public class DirectedSequenceRouterTests
    {
        /// <summary>
        /// The optimize turns function should take into account the fixed turns.
        /// </summary>
        [Test]
        public void OptimizeTurns_ShouldPreferNonTurns()
        {
            var weights = new []
            {
                new float[] {0, 1, 1, 2, 1, 1, 1, 1},
                new float[] {1, 0, 2, 1, 1, 1, 1, 1},
                new float[] {1, 1, 0, 1, 1, 2, 1, 1},
                new float[] {1, 1, 1, 0, 2, 1, 1, 1},
                new float[] {1, 1, 1, 1, 0, 1, 1, 2},
                new float[] {1, 1, 1, 1, 1, 0, 2, 1},
                new float[] {1, 1, 1, 1, 1, 1, 0, 1},
                new float[] {1, 1, 1, 1, 1, 1, 1, 0}
            };

            var turns = DirectedSequenceRouter.CalculateOptimimal(weights, 10, null);
            Assert.IsNotNull(turns);
            Assert.AreEqual(new[] {0, 4, 8, 12}, turns);
        }
        
        /// <summary>
        /// The optimize turns function should take into account broken connections.
        /// </summary>
        [Test]
        public void OptimizeTurns_ShouldLeaveOutBrokenConnections()
        {
            var weights = new []
            {
                new float[] {0, 1, float.MaxValue, float.MaxValue, 1, 1, 1, 1},
                new float[] {1, 0, float.MaxValue, float.MaxValue, 1, 1, 1, 1},
                new float[] {1, 1, 0, 1, 1, 2, 1, 1},
                new float[] {1, 1, 1, 0, 2, 1, 1, 1},
                new float[] {1, 1, 1, 1, 0, 1, 1, 2},
                new float[] {1, 1, 1, 1, 1, 0, 2, 1},
                new float[] {1, 1, 1, 1, 1, 1, 0, 1},
                new float[] {1, 1, 1, 1, 1, 1, 1, 0}
            };

            var turns = DirectedSequenceRouter.CalculateOptimimal(weights, 10, null);
            Assert.IsNotNull(turns);
            Assert.AreEqual(new[] {0, 8, 12}, turns);
        }
        
        /// <summary>
        /// The optimize turns function should use turns when it's cheaper after all.
        /// </summary>
        [Test]
        public void OptimizeTurns_ShouldUseTurnsWhenItsCheaper()
        {
            var weights = new []
            {
                new float[] {0, 1, 100,   2,   1,   1, 1, 1},
                new float[] {1, 0,   2, 100,   1,   1, 1, 1},
                new float[] {1, 1,   0,   1,   1, 200, 1, 1},
                new float[] {1, 1,   1,   0, 200,   1, 1, 1},
                new float[] {1, 1,   1,   1,   0,   1, 1, 2},
                new float[] {1, 1,   1,   1,   1,   0, 2, 1},
                new float[] {1, 1,   1,   1,   1,   1, 0, 1},
                new float[] {1, 1,   1,   1,   1,   1, 1, 0}
            };

            var turns = DirectedSequenceRouter.CalculateOptimimal(weights, 10, null);
            Assert.IsNotNull(turns);
            Assert.AreEqual(new[] {0, 7, 11, 15}, turns);
        }
        
        /// <summary>
        /// The optimize turns function should take into account the fixed turns.
        /// </summary>
        [Test]
        public void OptimizeTurns_ShouldTakeIntoAccountFixedTurns()
        {
            var weights = new float[][]
            {
                new float[] {0, 1, 2, 3, 4, 5, 6},
                new float[] {6, 0, 1, 2, 3, 4, 5},
                new float[] {5, 6, 0, 1, 2, 3, 4},
                new float[] {4, 5, 6, 0, 1, 2, 3},
                new float[] {3, 4, 5, 6, 0, 1, 2},
                new float[] {2, 3, 4, 5, 6, 0, 1},
                new float[] {1, 2, 3, 4, 5, 6, 0}
            };

            var turns = DirectedSequenceRouter.CalculateOptimimal(weights, 10, null);
            
        }
    }
}