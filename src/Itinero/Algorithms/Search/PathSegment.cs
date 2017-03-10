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

using System;
using System.Collections.Generic;
using System.Text;

namespace OsmSharp.Routing.Algorithms.Resolver
{
    /// <summary>
    /// Linked list of routed vertices.
    /// </summary>
    public class PathSegment<TIdType>
    {
        /// <summary>
        /// Creates a vertex not linked to any others.
        /// </summary>
        public PathSegment(TIdType vertexId)
        {
            this.VertexId = vertexId;
            this.Weight = 0;
            this.From = null;
        }

        /// <summary>
        /// Creates a new linked vertex.
        /// </summary>
        public PathSegment(TIdType vertexId, double weight, PathSegment<TIdType> from)
        {
            this.VertexId = vertexId;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// The id of this vertex.
        /// </summary>
        public TIdType VertexId { get; set; }

        /// <summary>
        /// The weight from the source vertex.
        /// </summary>
        public double Weight { get; set; }

        /// <summary>
        /// The vertex that came before this one.
        /// </summary>
        public PathSegment<TIdType> From { get; set; }

        /// <summary>
        /// Returns the reverse of this path segment.
        /// </summary>
        /// <returns></returns>
        public PathSegment<TIdType> Reverse()
        {
            var route = new PathSegment<TIdType>(this.VertexId);
            PathSegment<TIdType> next = this;
            while (next.From != null)
            {
                route = new PathSegment<TIdType>(next.From.VertexId,
                    (next.Weight - next.From.Weight) + route.Weight, route);
                next = next.From;
            }
            return route;
        }

        /// <summary>
        /// Returns the first vertex.
        /// </summary>
        /// <returns></returns>
        public PathSegment<TIdType> First()
        {
            PathSegment<TIdType> next = this;
            while (next.From != null)
            {
                next = next.From;
            }
            return next;
        }

        /// <summary>
        /// Returns the length.
        /// </summary>
        /// <returns></returns>
        public int Length()
        {
            int length = 1;
            PathSegment<TIdType> next = this;
            while (next.From != null)
            {
                length++;
                next = next.From;
            }
            return length;
        }

        /// <summary>
        /// Concatenates this path after the given path.
        /// </summary>
        /// <returns></returns>
        public PathSegment<TIdType> ConcatenateAfter(PathSegment<TIdType> path, Func<TIdType, TIdType, int> comparer)
        {
            var clone = this.Clone();
            var first = clone.First();
            var pathClone = path.Clone();

            var current = clone;
            current.Weight = path.Weight + current.Weight;
            while (current.From != null)
            {
                current.From.Weight = path.Weight + current.From.Weight;
                current = current.From;
            }

            if (comparer == null)
            { // use default equals.
                if (first.VertexId.Equals(path.VertexId))
                {
                    first.Weight = pathClone.Weight;
                    first.From = pathClone.From;
                    return clone;
                }
                throw new ArgumentException("Paths must share beginning and end vertices to concatenate!");
            }
            else 
            { // use custom comparer.
                if (comparer.Invoke(first.VertexId, path.VertexId) == 0)
                {
                    first.Weight = pathClone.Weight;
                    first.From = pathClone.From;
                    return clone;
                }
                throw new ArgumentException("Paths must share beginning and end vertices to concatenate!");
            }
        }

        /// <summary>
        /// Concatenates this path after the given path.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public PathSegment<TIdType> ConcatenateAfter(PathSegment<TIdType> path)
        {
            return this.ConcatenateAfter(path, null);
        }

        /// <summary>
        /// Returns an exact copy of this path segment.
        /// </summary>
        /// <returns></returns>
        public PathSegment<TIdType> Clone()
        {
            if (this.From == null)
            { // cloning this case is easy!
                return new PathSegment<TIdType>(this.VertexId);
            }
            else
            { // recursively clone the from segments.
                PathSegment<TIdType> from = this.From.Clone();
                return new PathSegment<TIdType>(this.VertexId, this.Weight, from);
            }
        }

        /// <summary>
        /// Returns a description of this path.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var builder = new StringBuilder();
            var next = this;
            while (next.From != null)
            {
                builder.Insert(0, string.Format("-> {0}[{1}]", next.VertexId, next.Weight));
                next = next.From;
            }
            builder.Insert(0, string.Format("{0}[{1}]", next.VertexId, next.Weight));
            return builder.ToString();
        }

        /// <summary>
        /// Returns all the vertices in an array.
        /// </summary>
        /// <returns></returns>
        public TIdType[] ToArray()
        {
            var vertices = new List<TIdType>();
            var next = this;
            while (next.From != null)
            {
                vertices.Add(next.VertexId);
                next = next.From;
            }
            vertices.Add(next.VertexId);
            vertices.Reverse();
            return vertices.ToArray();
        }

        /// <summary>
        /// Returns all the vertices in an array along with their respective weight.
        /// </summary>
        /// <returns></returns>
        public Tuple<TIdType, double>[] ToArrayWithWeight()
        {
            var vertices = new List<Tuple<TIdType, double>>();
            var next = this;
            while (next.From != null)
            {
                vertices.Add(new Tuple<TIdType, double>(next.VertexId, next.Weight));
                next = next.From;
            }
            vertices.Add(new Tuple<TIdType, double>(next.VertexId, next.Weight));
            vertices.Reverse();
            return vertices.ToArray();
        }

        /// <summary>
        /// Returns true if the path is the samen.
        /// </summary>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <returns></returns>
        public static bool operator ==(PathSegment<TIdType> segment1, PathSegment<TIdType> segment2)
        {
            if ((object)segment1 != null && (object)segment2 != null)
            {
                if (segment1.VertexId.Equals(segment2.VertexId) &&
                    segment1.Weight == segment2.Weight)
                {
                    if ((object)segment1.From != null && (object)segment2.From != null)
                    {
                        return segment1.From == segment2.From;
                    }
                    else
                    {
                        return (object)segment1.From == null && (object)segment2.From == null;
                    }
                }
                return false;
            }
            return (object)segment1 == null && (object)segment2 == null;
        }


        /// <summary>
        /// Returns true if the path is the samen.
        /// </summary>
        /// <param name="segment1"></param>
        /// <param name="segment2"></param>
        /// <returns></returns>
        public static bool operator !=(PathSegment<TIdType> segment1, PathSegment<TIdType> segment2)
        {
            return !(segment2 == segment1);
        }

        /// <summary>
        /// Returns true if the given object equals this one.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected bool Equals(PathSegment<TIdType> other)
        {
            return EqualityComparer<TIdType>.Default.Equals(VertexId, other.VertexId) && Weight.Equals(other.Weight) && Equals(From, other.From);
        }

        /// <summary>
        /// Returns true if the given object equals this one.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PathSegment<TIdType>)obj);
        }

        /// <summary>
        /// Returns the hashcode for this object.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = EqualityComparer<TIdType>.Default.GetHashCode(VertexId);
                hashCode = (hashCode * 397) ^ Weight.GetHashCode();
                hashCode = (hashCode * 397) ^ (From != null ? From.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
