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
    /// Represents different profile metrics.
    /// </summary>
    public enum ProfileMetric
    {
        /// <summary>
        /// A profile that uses time in seconds.
        /// </summary>
        /// <remarks>Means that Factor() = 1/Speed().</remarks>
        TimeInSeconds,
        /// <summary>
        /// A profile that uses distance in meters.
        /// </summary>
        /// <remarks>Means that Factor() is constant, Speed() returns the actual speed.</remarks>
        DistanceInMeters,
        /// <summary>
        /// A profile that uses a custom metric.
        /// </summary>
        /// <remarks>Means that Factor() can be anything, Speed() returns the actual speed.</remarks>
        Custom
    }
}