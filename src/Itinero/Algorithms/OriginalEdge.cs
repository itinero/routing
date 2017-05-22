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

using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Graphs.Directed;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Represents an original edge consisting of vertex1 -> vertex2.
    /// </summary>
    public struct OriginalEdge
    {
        /// <summary>
        /// Creates an original edge.
        /// </summary>
        public OriginalEdge(uint vertex1, uint vertex2)
        {
            this.Vertex1 = vertex1;
            this.Vertex2 = vertex2;
        }

        /// <summary>
        /// Gets or sets vertex1.
        /// </summary>
        public uint Vertex1 { get; set; }

        /// <summary>
        /// Gets or sets vertex2.
        /// </summary>
        public uint Vertex2 { get; set; }

        /// <summary>
        /// Converts this original edge to an edge path.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public EdgePath<T> ToEdgePath<T>(T weight = default(T))
        {
            return new EdgePath<T>(this.Vertex2, weight, new EdgePath<T>(this.Vertex1));
        }

        /// <summary>
        /// Gets a hash.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Vertex1.GetHashCode() ^
                this.Vertex2.GetHashCode();
        }

        /// <summary>
        /// Returns true if the given object represents the same edge.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is OriginalEdge)
            {
                return this.Equals((OriginalEdge)obj);
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given object represents the same edge.
        /// </summary>
        public bool Equals(OriginalEdge obj)
        {
            return obj.Vertex1 == this.Vertex1 &&
                obj.Vertex2 == this.Vertex2;
        }

        /// <summary>
        /// Reverses this edge.
        /// </summary>
        /// <returns></returns>
        public OriginalEdge Reverse()
        {
            return new OriginalEdge(this.Vertex2, this.Vertex1);
        }

        /// <summary>
        /// Returns a description.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}->{1}", this.Vertex1, this.Vertex2);
        }
    }

    /// <summary>
    /// Contains origin edge related extension methods.
    /// </summary>
    public static class OriginalEdgeExtensions
    {

    }
}