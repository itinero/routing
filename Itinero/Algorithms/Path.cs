// Itinero - OpenStreetMap (OSM) SDK
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

using System;
using System.Collections.Generic;
using System.Text;

namespace Itinero.Algorithms
{    
    /// <summary>
    /// Represents a path along a set of vertices.
    /// </summary>
    public class Path
    {
        /// <summary>
        /// Creates a path consisting of one vertex.
        /// </summary>
        public Path(uint vertex)
        {
            this.Vertex = vertex;
            this.Weight = 0;
            this.From = null;
        }

        /// <summary>
        /// Creates a path by existing an existing path.
        /// </summary>
        public Path(uint vertex, float weight, Path from)
        {
            this.Vertex = vertex;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        public uint Vertex { get; set; }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        public float Weight { get; set; }

        /// <summary>
        /// Gets previous path.
        /// </summary>
        public Path From { get; set; }

        /// <summary>
        /// Returns the reverse of this path segment.
        /// </summary>
        /// <returns></returns>
        public Path Reverse()
        {
            var route = new Path(this.Vertex);
            var next = this;
            while (next.From != null)
            {
                route = new Path(next.From.Vertex,
                    (next.Weight - next.From.Weight) + route.Weight, route);
                next = next.From;
            }
            return route;
        }

        /// <summary>
        /// Returns the first vertex.
        /// </summary>
        /// <returns></returns>
        public Path First()
        {
            var next = this;
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
            var length = 1;
            var next = this;
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
        public Path ConcatenateAfter(Path path, Func<uint, uint, int> comparer)
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
                if (first.Vertex.Equals(path.Vertex))
                {
                    first.Weight = pathClone.Weight;
                    first.From = pathClone.From;
                    return clone;
                }
                throw new ArgumentException("Paths must share beginning and end vertices to concatenate!");
            }
            else
            { // use custom comparer.
                if (comparer.Invoke(first.Vertex, path.Vertex) == 0)
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
        /// <returns></returns>
        public Path ConcatenateAfter(Path path)
        {
            return this.ConcatenateAfter(path, null);
        }

        /// <summary>
        /// Returns an exact copy of this path segment.
        /// </summary>
        /// <returns></returns>
        public Path Clone()
        {
            if (this.From == null)
            { // cloning this case is easy!
                return new Path(this.Vertex);
            }
            else
            { // recursively clone the from segments.
                return new Path(this.Vertex, this.Weight, this.From.Clone());
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
                builder.Insert(0, string.Format("-> {0}[{1}]", next.Vertex, next.Weight));
                next = next.From;
            }
            builder.Insert(0, string.Format("{0}[{1}]", next.Vertex, next.Weight));
            return builder.ToString();
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public void AddToListReverse(List<uint> vertices)
        {
            var path = this;
            while (path != null)
            {
                vertices.Add(path.Vertex);
                path = path.From;
            }
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public void AddToList(List<uint> vertices)
        {
            var reversed = new List<uint>();
            this.AddToListReverse(reversed);
            for(var i = reversed.Count - 1; i >= 0; i--)
            {
                vertices.Add(reversed[i]);
            }
        }
    }
}