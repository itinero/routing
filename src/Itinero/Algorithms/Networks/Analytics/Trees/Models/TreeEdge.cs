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

namespace Itinero.Algorithms.Networks.Analytics.Trees.Models
{
    /// <summary>
    /// Represents an edge in a tree.
    /// </summary>
    public class TreeEdge
    {
        /// <summary>
        /// Gets or sets the edge id.
        /// </summary>
        public uint EdgeId { get; set; }

        /// <summary>
        /// Gets or sets the previous edge id.
        /// </summary>
        public uint PreviousEdgeId { get; set; }

        /// <summary>
        /// Gets or sets the first weight.
        /// </summary>
        public float Weight1 { get; set; }

        /// <summary>
        /// Gets or sets the second weight.
        /// </summary>
        public float Weight2 { get; set; }

        /// <summary>
        /// Gets or sets the shape.
        /// </summary>
        public float[][] Shape { get; set; }
    }
}