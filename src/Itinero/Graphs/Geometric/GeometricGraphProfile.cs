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

using Reminiscence.Arrays;

namespace Itinero.Graphs.Geometric
{
    /// <summary>
    /// A profile with settings for a memory-mapped geometric graph.
    /// </summary>
    public class GeometricGraphProfile
    {
        /// <summary>
        /// Gets or sets the graph profile.
        /// </summary>
        public GraphProfile GraphProfile { get; set; }

        /// <summary>
        /// Gets or sets the coordinates profile.
        /// </summary>
        public ArrayProfile CoordinatesProfile { get; set; }

        /// <summary>
        /// A profile that tells the graph to use no caching.
        /// </summary>
        public static GeometricGraphProfile NoCache = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.NoCache,
            GraphProfile = GraphProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static GeometricGraphProfile OneBuffer = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.OneBuffer,
            GraphProfile = GraphProfile.OneBuffer
        };

        /// <summary>
        /// An profile that aggressively caches data with potenally 32Kb of cached data.
        /// </summary>
        public static GeometricGraphProfile Aggressive32 = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.Aggressive8,
            GraphProfile = GraphProfile.Aggressive24
        };

        /// <summary>
        /// A default profile that use no caching for coordinates but aggressive for graph.
        /// </summary>
        public static GeometricGraphProfile Default = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.NoCache,
            GraphProfile = GraphProfile.Aggressive24
        };
    }
}
