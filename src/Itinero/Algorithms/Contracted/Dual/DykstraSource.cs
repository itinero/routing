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

using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// A structure representing a dykstra search source.
    /// </summary>
    public struct DykstraSource<T>
    {
        /// <summary>
        /// Creates a new source.
        /// </summary>
        public DykstraSource(uint vertex1)
        {
            this.Vertex1 = vertex1;
            this.Weight1 = default(T);
            this.Vertex2 = Constants.NO_VERTEX;
            this.Weight2 = default(T);
        }

        /// <summary>
        /// Creates a new source.
        /// </summary>
        public DykstraSource(uint vertex1, T weight1)
        {
            this.Vertex1 = vertex1;
            this.Weight1 = weight1;
            this.Vertex2 = Constants.NO_VERTEX;
            this.Weight2 = default(T);
        }

        /// <summary>
        /// Creates a new source.
        /// </summary>
        public DykstraSource(uint vertex1, T weight1, uint vertex2, T weight2)
        {
            this.Vertex1 = vertex1;
            this.Weight1 = weight1;
            this.Vertex2 = vertex2;
            this.Weight2 = weight2;
        }

        /// <summary>
        /// Gets or sets the vertex1.
        /// </summary>
        public uint Vertex1 { get; set; }

        /// <summary>
        /// Gets or sets the weight1.
        /// </summary>
        public T Weight1 { get; set; }

        /// <summary>
        /// Gets or sets the vertex2.
        /// </summary>
        public uint Vertex2 { get; set; }

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
            if (this.Vertex2 != Constants.NO_VERTEX)
            {
                return string.Format("{0}@{1} {2}@{3}", this.Vertex1, this.Weight1, this.Vertex2, this.Weight2);
            }
            return string.Format("{0}@{1}", this.Vertex1, this.Weight1);
        }
    }

    /// <summary>
    /// Contains extension methods related to dykstra sources.
    /// </summary>
    public static class DykstraSourceExtensions
    {
        /// <summary>
        /// Converts directed edge id's into an array of dykstra sources.
        /// </summary>
        public static DykstraSource<T>[] ToDykstraSources<T>(this DirectedEdgeId[] edges)
            where T : struct
        {
            var result = new DykstraSource<T>[edges.Length];
            for(var i = 0; i < result.Length; i++)
            {
                result[i] = new DykstraSource<T>(edges[i].Raw);
            }
            return result;
        }

        /// <summary>
        /// Converts directed edge id's into an array of dykstra sources.
        /// </summary>
        public static DykstraSource<T>[] ToDykstraSources<T>(this List<DirectedEdgeId> edges)
            where T : struct
        {
            var result = new DykstraSource<T>[edges.Count];
            for (var i = 0; i < result.Length; i++)
            {
                result[i] = new DykstraSource<T>(edges[i].Raw);
            }
            return result;
        }
    }
}