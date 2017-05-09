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

using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Represents details associated with a (potential) shortcut.
    /// </summary>
    public struct Shortcut<T>
            where T : struct
    {
        /// <summary>
        /// Gets or sets the target edge.
        /// </summary>
        public OriginalEdge Edge { get; set; }

        /// <summary>
        /// Gets or sets the maximum forward weight.
        /// </summary>
        public T Forward { get; set; }

        /// <summary>
        /// Gets or sets the maximum backward weight.
        /// </summary>
        public T Backward { get; set; }

        /// <summary>
        /// Updates the weights.
        /// </summary>
        public void Update(WeightHandler<T> weightHandler, bool forward, bool backward, T weight)
        {
            if (forward && (
                weightHandler.GetMetric(this.Forward) == 0 || 
                weightHandler.IsLargerThan(this.Forward, weight)))
            {
                this.Forward = weight;
            }
            if (backward && (
                weightHandler.GetMetric(this.Backward) == 0 || 
                weightHandler.IsLargerThan(this.Backward, weight)))
            {
                this.Backward = weight;
            }
        }
    }
}