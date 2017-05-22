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

using Itinero.Algorithms.Restrictions;
using System;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Represents a turn.
    /// </summary>
    public struct Turn
    {
        /// <summary>
        /// Creates a new turn.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="vertex3"></param>
        public Turn(uint vertex1, uint vertex2, uint vertex3)
        {
            this.Vertex1 = vertex1;
            this.Vertex2 = vertex2;
            this.Vertex3 = vertex3;
        }

        /// <summary>
        /// Creates a turn from an original and a third vertex.
        /// </summary>
        public Turn(OriginalEdge edge, uint to)
        {
            this.Vertex1 = edge.Vertex1;
            this.Vertex2 = edge.Vertex2;
            this.Vertex3 = to;
        }

        /// <summary>
        /// First vertex.
        /// </summary>
        public uint Vertex1 { get; set; }

        /// <summary>
        /// Second vertex.
        /// </summary>
        public uint Vertex2 { get; set; }

        /// <summary>
        /// Third vertex.
        /// </summary>
        public uint Vertex3 { get; set; }

        /// <summary>
        /// Gets the n-th vertex in this turn.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public uint this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0:
                        return this.Vertex1;
                    case 1:
                        return this.Vertex2;
                    case 2:
                        return this.Vertex3;
                }
                throw new IndexOutOfRangeException();
            }
        }

        /// <summary>
        /// Reverses this turn.
        /// </summary>
        /// <returns></returns>
        public void Reverse()
        {
            var t = this.Vertex3;
            this.Vertex3 = this.Vertex1;
            this.Vertex1 = t;
        }

        /// <summary>
        /// Returns true if this turn is restricted by one of the restrictions in the given collection.
        /// </summary>
        public bool IsRestrictedBy(RestrictionCollection restrictions)
        {
            for (var r = 0; r < restrictions.Count; r++)
            {
                if (IsRestrictedBy(restrictions[r]))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this turn is restricted by the given restriction.
        /// </summary>
        public bool IsRestrictedBy(Restriction restriction)
        {
            if (restriction.Vertex3 == Constants.NO_VERTEX)
            {
                if (restriction.Vertex2 == Constants.NO_VERTEX)
                {
                    return restriction.Vertex1 == this.Vertex1 ||
                        restriction.Vertex1 == this.Vertex2 ||
                        restriction.Vertex1 == this.Vertex3;
                }
                return (restriction.Vertex1 == this.Vertex1 &&
                    restriction.Vertex2 == this.Vertex2) ||
                    (restriction.Vertex1 == this.Vertex2 &&
                    restriction.Vertex2 == this.Vertex3);
            }
            return restriction.Vertex1 == this.Vertex1 &&
                restriction.Vertex2 == this.Vertex2 &&
                restriction.Vertex3 == this.Vertex3;
        }

        /// <summary>
        /// Gets the length of this turn.
        /// </summary>
        public int Length
        {
            get
            {
                return 3;
            }
        }

        /// <summary>
        /// Returns true if this turn is a u-turn.
        /// </summary>
        public bool IsUTurn
        {
            get
            {
                return this.Vertex1 == this.Vertex3;
            }
        }

        /// <summary>
        /// Returns a description of this turn.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0}->{1}->{2}", this.Vertex1, this.Vertex2, this.Vertex3);
        }
    }
}