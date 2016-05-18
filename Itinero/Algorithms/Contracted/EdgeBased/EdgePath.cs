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

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Represents a path along a set of edges.
    /// </summary>
    public class EdgePath
    {
        /// <summary>
        /// Creates a edge path consisting of one edge.
        /// </summary>
        public EdgePath(uint edge)
        {
            this.Edge = edge;
            this.Weight = 0;
            this.From = null;
        }

        /// <summary>
        /// Creates a edge path consisting of one edge.
        /// </summary>
        public EdgePath(uint edge, float weight)
        {
            this.Edge = edge;
            this.Weight = weight;
            this.From = null;
        }

        /// <summary>
        /// Creates a path by existing an existing path.
        /// </summary>
        public EdgePath(uint edge, float weight, EdgePath from)
        {
            this.Edge = edge;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Gets the edge.
        /// </summary>
        public uint Edge { get; private set; }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        public float Weight { get; private set; }

        /// <summary>
        /// Gets previous path.
        /// </summary>
        public EdgePath From { get; private set; }

        /// <summary>
        /// Returns the reverse of this path segment.
        /// </summary>
        public EdgePath Reverse()
        {
            var route = new EdgePath(this.Edge);
            var next = this;
            while (next.From != null)
            {
                route = new EdgePath(next.From.Edge,
                    (next.Weight - next.From.Weight) + route.Weight, route);
                next = next.From;
            }
            return route;
        }

        /// <summary>
        /// Returns the first edge.
        /// </summary>
        /// <returns></returns>
        public EdgePath First()
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
        public EdgePath ConcatenateAfter(EdgePath path, Func<long, long, int> comparer)
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
                if (first.Edge.Equals(path.Edge))
                {
                    first.Weight = pathClone.Weight;
                    first.From = pathClone.From;
                    return clone;
                }
                throw new ArgumentException("Paths must share beginning and end edges to concatenate!");
            }
            else
            { // use custom comparer.
                if (comparer.Invoke(first.Edge, path.Edge) == 0)
                {
                    first.Weight = pathClone.Weight;
                    first.From = pathClone.From;
                    return clone;
                }
                throw new ArgumentException("Paths must share beginning and end edges to concatenate!");
            }
        }

        /// <summary>
        /// Concatenates this path after the given path.
        /// </summary>
        public EdgePath ConcatenateAfter(EdgePath path)
        {
            return this.ConcatenateAfter(path, null);
        }

        /// <summary>
        /// Returns an exact copy of this path segment.
        /// </summary>
        public EdgePath Clone()
        {
            if (this.From == null)
            { // cloning this case is easy!
                return new EdgePath(this.Edge);
            }
            else
            { // recursively clone the from segments.
                return new EdgePath(this.Edge, this.Weight, this.From.Clone());
            }
        }

        /// <summary>
        /// Returns a description of this path.
        /// </summary>
        public override string ToString()
        {
            var builder = new StringBuilder();
            var next = this;
            while (next.From != null)
            {
                builder.Insert(0, string.Format("-> {0}[{1}]", next.Edge, next.Weight));
                next = next.From;
            }
            builder.Insert(0, string.Format("{0}[{1}]", next.Edge, next.Weight));
            return builder.ToString();
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public void AddToListReverse(List<long> edges)
        {
            var path = this;
            while (path != null)
            {
                if (edges.Count == 0 ||
                    edges[edges.Count - 1] != path.Edge)
                {
                    edges.Add(path.Edge);
                }
                path = path.From;
            }
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public void AddToList(List<long> edges)
        {
            var reversed = new List<long>();
            this.AddToListReverse(reversed);
            for (var i = reversed.Count - 1; i >= 0; i--)
            {
                edges.Add(reversed[i]);
            }
        }
    }
}
