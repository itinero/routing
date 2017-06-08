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

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// A structure representing a dykstra search source.
    /// </summary>
    public struct DirectedDykstraSource<T>
    {
        /// <summary>
        /// Creates a new source.
        /// </summary>
        public DirectedDykstraSource(DirectedEdgeId edge1)
        {
            this.Edge1 = edge1;
            this.Weight1 = default(T);
            this.Edge2 = DirectedEdgeId.NO_EDGE;
            this.Weight2 = default(T);
        }

        /// <summary>
        /// Creates a new source.
        /// </summary>
        public DirectedDykstraSource(DirectedEdgeId edge1, T weight1)
        {
            this.Edge1 = edge1;
            this.Weight1 = weight1;
            this.Edge2 = DirectedEdgeId.NO_EDGE;
            this.Weight2 = default(T);
        }

        /// <summary>
        /// Creates a new source.
        /// </summary>
        public DirectedDykstraSource(DirectedEdgeId edge1, T weight1, DirectedEdgeId edge2, T weight2)
        {
            this.Edge1 = edge1;
            this.Weight1 = weight1;
            this.Edge2 = edge2;
            this.Weight2 = weight2;
        }

        /// <summary>
        /// Gets or sets the edge1.
        /// </summary>
        public DirectedEdgeId Edge1 { get; set; }

        /// <summary>
        /// Gets or sets the weight1.
        /// </summary>
        public T Weight1 { get; set; }

        /// <summary>
        /// Gets or sets the edge2.
        /// </summary>
        public DirectedEdgeId Edge2 { get; set; }

        /// <summary>
        /// Gets or sets the weight2.
        /// </summary>
        public T Weight2 { get; set; }

        /// <summary>
        /// Returns a description of this dykstra source.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (!this.Edge2.IsNoEdge)
            {
                return string.Format("{0}@{1} {2}@{3}", this.Edge1, this.Weight1, this.Edge2, this.Weight2);
            }
            return string.Format("{0}@{1}", this.Edge1, this.Weight1);
        }
    }

    /// <summary>
    /// Contains extension methods related to dykstra sources.
    /// </summary>
    public static class DirectedDykstraSourceExtensions
    {
        /// <summary>
        /// Converts directed edge id's into an array of directed dykstra sources.
        /// </summary>
        public static DirectedDykstraSource<T>[] ToDykstraSources<T>(this DirectedEdgeId[] edges)
            where T : struct
        {
            var result = new DirectedDykstraSource<T>[edges.Length];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new DirectedDykstraSource<T>(edges[i]);
            }
            return result;
        }
    }
}