// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Weights;
using System.Text;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Represents a path along a set of edges/vertices.
    /// </summary>
    public class EdgePath<T>
    {
        /// <summary>
        /// Creates a path source.
        /// </summary>
        public EdgePath(uint vertex = Constants.NO_VERTEX)
        {
            this.Vertex = vertex;
            this.Edge = Constants.NO_EDGE;
            this.Weight = default(T);
            this.From = null;
        }

        /// <summary>
        /// Creates a path to the given vertex with the given weight.
        /// </summary>
        public EdgePath(uint vertex, T weight, EdgePath<T> from)
        {
            this.Vertex = vertex;
            this.Edge = Constants.NO_EDGE;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Creates a path to the given vertex with the given weight along the given edge.
        /// </summary>
        public EdgePath(uint vertex, T weight, long edge, EdgePath<T> from)
        {
            this.Vertex = vertex;
            this.Edge = edge;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Gets the edge right before the vertex.
        /// </summary>
        public long Edge { get; set; }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        public uint Vertex { get; set; }
        
        /// <summary>
        /// Gets the weight at the vertex.
        /// </summary>
        public T Weight { get; set; }

        /// <summary>
        /// Gets previous path.
        /// </summary>
        public EdgePath<T> From { get; set; }

        /// <summary>
        /// Returns a description of this path.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            var next = this;
            while (next != null)
            {
                if (next.From != null)
                {
                    builder.Insert(0, string.Format("->{2}->{0}[{1}]", next.Vertex, next.Weight, next.Edge));
                }
                else
                {
                    builder.Insert(0, string.Format("{0}[{1}]", next.Vertex, next.Weight));
                }
                next = next.From;
            }
            return builder.ToString();
        }

        /// <summary>
        /// Returns true if the given object represents the same edge/vertex.
        /// </summary>
        public override bool Equals(object obj)
        {
            var other = obj as EdgePath<T>;
            if (other == null)
            {
                return false;
            }
            return other.Edge == this.Edge &&
                other.Vertex == this.Vertex;
        }

        /// <summary>
        /// Serves as a hashfunction for this type.
        /// </summary>
        public override int GetHashCode()
        {
            return this.Edge.GetHashCode() ^
                this.Vertex.GetHashCode();
        }
    }
    
    /// <summary>
    /// A linked list of edge paths.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LinkedEdgePath<T>
        where T : struct
    {
        /// <summary>
        /// Gets the path.
        /// </summary>
        public EdgePath<T> Path { get; set; }

        /// <summary>
        /// Gets the next path.
        /// </summary>
        public LinkedEdgePath<T> Next { get; set; }

        /// <summary>
        /// Gets the best path in this linked list.
        /// </summary>
        public EdgePath<T> Best(WeightHandler<T> weightHandler)
        {
            var best = this.Path;
            var current = this.Next;
            while (current != null)
            {
                if (weightHandler.IsSmallerThan(current.Path.Weight, best.Weight))
                {
                    best = current.Path;
                }
                current = current.Next;
            }
            return best;
        }

        /// <summary>
        /// Returns true if this list contains the given path.
        /// </summary>
        public bool HasPath(EdgePath<T> path)
        {
            var current = this;
            while (current != null)
            {
                if (current.Path.Edge == path.Edge)
                {
                    return true;
                }
                current = current.Next;
            }
            return false;
        }
    }
}