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

namespace Itinero.Profiles
{
    /// <summary>
    /// A factor returned by a routing profile to influence routing augmented with the speed.
    /// </summary>
    public struct FactorAndSpeed
    {
        /// <summary>
        /// Gets or sets the actual factor.
        /// </summary>
        public float Value { get; set; }

        /// <summary>
        /// Gets or sets the speed (1/m/s).
        /// </summary>
        public float SpeedFactor { get; set; }

        /// <summary>
        /// Gets or sets the direction.
        /// </summary>
        /// 0=bidirectional, 1=forward, 2=backward.
        /// 3=bidirectional, 4=forward, 5=backward but without stopping abilities.
        public short Direction { get; set; }

        /// <summary>
        /// Gets or sets the constraint values.
        /// </summary>
        public float[] Constraints { get; set; }
        
        /// <summary>
        /// Returns a non-value.
        /// </summary>
        public static FactorAndSpeed NoFactor { get { return new FactorAndSpeed() { Direction = 0, Value = 0, SpeedFactor = 0, Constraints = null }; } }
    }
}