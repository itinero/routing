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

namespace Itinero.Algorithms
{
    /// <summary>
    /// Represents a directed edge id.
    /// </summary>
    public struct DirectedEdgeId
    {
        /// <summary>
        /// The raw value, not be confused with an undirected id!
        /// </summary>
        public uint Raw;

        /// <summary>
        /// Creates a new directed id.
        /// </summary>
        public DirectedEdgeId(long directedEdgeId)
        {
            uint edgeId = Constants.NO_EDGE;
            if (directedEdgeId > 0)
            {
                edgeId = (uint)(directedEdgeId - 1);
            }
            else
            {
                edgeId = (uint)(-directedEdgeId - 1);
            }

            Raw = edgeId * 2;
            if (directedEdgeId < 0)
            {
                Raw++;
            }
        }

        /// <summary>
        /// Creates a new directed id.
        /// </summary>
        public DirectedEdgeId(uint edgeId, bool forward)
        {
            Raw = edgeId * 2;
            if (!forward)
            {
                Raw++;
            }
        }

        /// <summary>
        /// Creates a directed edge id structure.
        /// </summary>
        public static DirectedEdgeId FromRaw(uint rawDirectedEdgeId)
        {
            return new DirectedEdgeId()
            {
                Raw = rawDirectedEdgeId
            };
        }

        /// <summary>
        /// Gets a direcred edge id that represents no edge.
        /// </summary>
        public static DirectedEdgeId NO_EDGE = new DirectedEdgeId()
        {
            Raw = Constants.NO_EDGE
        };

        /// <summary>
        /// Returns the equivalent signed directed id.
        /// </summary>
        public long SignedDirectedId
        {
            get
            {
                if (this.Forward)
                {
                    return this.EdgeId + 1;
                }
                return -(this.EdgeId + 1);
            }
        }

        /// <summary>
        /// Gets the undirected edge id.
        /// </summary>
        public uint EdgeId
        {
            get
            {
                return Raw / 2;
            }
        }

        /// <summary>
        /// Returns true if this edge is forward.
        /// </summary>
        public bool Forward
        {
            get
            {
                return (Raw & 1) == 0;
            }
        }

        /// <summary>
        /// Gets the reverse edge id.
        /// </summary>
        /// <returns></returns>
        public DirectedEdgeId Reverse => new DirectedEdgeId(this.EdgeId, !this.Forward);

        /// <summary>
        /// Reverse the raw id.
        /// </summary>
        public static uint ReverseRaw(uint id)
        {
            var a = new DirectedEdgeId()
            {
                Raw = id
            };
            a = a.Reverse;
            return a.Raw;
        }

        /// <summary>
        /// Returns true if this edge represents no edge..
        /// </summary>
        public bool IsNoEdge
        {
            get
            {
                return Raw == Constants.NO_EDGE;
            }
        }
        /// <summary>
        /// Returns true if the given object represents the same edge.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is DirectedEdgeId)
            {
                return this.Equals((DirectedEdgeId)obj);
            }
            return false;
        }

        /// <summary>
        /// Overloads the == operator to avoid boxing/unboxing equals.
        /// </summary>
        public static bool operator ==(DirectedEdgeId edge1, DirectedEdgeId edge2)
        {
            return edge1.Raw == edge2.Raw;
        }

        /// <summary>
        /// Overloads the != operator to avoid boxing/unboxing equals.
        /// </summary>
        public static bool operator !=(DirectedEdgeId edge1, DirectedEdgeId edge2)
        {
            return edge1.Raw != edge2.Raw;
        }

        /// <summary>
        /// Returns true if the given object represents the same edge.
        /// </summary>
        public bool Equals(DirectedEdgeId obj)
        {
            return obj.Raw == this.Raw;
        }

        /// <summary>
        /// Gets a hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return this.Raw.GetHashCode();
        }

        /// <summary>
        /// Gets a description of this id.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.Forward)
            {
                return string.Format("{1}({0}(F))",
                    this.EdgeId, this.Raw);
            }
            return string.Format("{1}({0}(B))",
                this.EdgeId, this.Raw);
        }
    }
}