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
using Itinero.Graphs;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Represents a path along a set of edges.
    /// </summary>
    public class DirectedEdgePath
    {
        /// <summary>
        /// Creates a edge path consisting of one edge.
        /// </summary>
        public DirectedEdgePath(long edge)
        {
            this.DirectedEdge = edge;
            this.Weight = 0;
            this.From = null;
        }

        /// <summary>
        /// Creates a path by existing an existing path.
        /// </summary>
        public DirectedEdgePath(long edge, float weight, DirectedEdgePath from)
        {
            this.DirectedEdge = edge;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Gets the edge.
        /// </summary>
        public long DirectedEdge { get; private set; }

        /// <summary>
        /// Gets the weight.
        /// </summary>
        public float Weight { get; private set; }

        /// <summary>
        /// Gets previous path.
        /// </summary>
        public DirectedEdgePath From { get; private set; }

        /// <summary>
        /// Returns the reverse of this path segment.
        /// </summary>
        public DirectedEdgePath Reverse()
        {
            var route = new DirectedEdgePath(this.DirectedEdge);
            var next = this;
            while (next.From != null)
            {
                route = new DirectedEdgePath(next.From.DirectedEdge,
                    (next.Weight - next.From.Weight) + route.Weight, route);
                next = next.From;
            }
            return route;
        }

        /// <summary>
        /// Returns the first edge.
        /// </summary>
        /// <returns></returns>
        public DirectedEdgePath First()
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
        public DirectedEdgePath ConcatenateAfter(DirectedEdgePath path, Func<long, long, int> comparer)
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
                if (first.DirectedEdge.Equals(path.DirectedEdge))
                {
                    first.Weight = pathClone.Weight;
                    first.From = pathClone.From;
                    return clone;
                }
                throw new ArgumentException("Paths must share beginning and end edges to concatenate!");
            }
            else
            { // use custom comparer.
                if (comparer.Invoke(first.DirectedEdge, path.DirectedEdge) == 0)
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
        public DirectedEdgePath ConcatenateAfter(DirectedEdgePath path)
        {
            return this.ConcatenateAfter(path, null);
        }

        /// <summary>
        /// Returns an exact copy of this path segment.
        /// </summary>
        public DirectedEdgePath Clone()
        {
            if (this.From == null)
            { // cloning this case is easy!
                return new DirectedEdgePath(this.DirectedEdge);
            }
            else
            { // recursively clone the from segments.
                return new DirectedEdgePath(this.DirectedEdge, this.Weight, this.From.Clone());
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
                builder.Insert(0, string.Format("-> {0}[{1}]", next.DirectedEdge, next.Weight));
                next = next.From;
            }
            builder.Insert(0, string.Format("{0}[{1}]", next.DirectedEdge, next.Weight));
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
                    edges[edges.Count - 1] != path.DirectedEdge)
                {
                    edges.Add(path.DirectedEdge);
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

        /// <summary>
        /// Converts this edge path to a graph.
        /// </summary>
        public Path ToPath(Graph graph)
        {
            var edgeEnumerator = graph.GetEdgeEnumerator();
            Path first = null;
            if (this.DirectedEdge == Constants.NO_EDGE)
            {
                first = new Path(Constants.NO_VERTEX, this.Weight, null);
            }
            else
            {
                var target = edgeEnumerator.GetTargetVertex(this.DirectedEdge);
                first = new Path(target, this.Weight, null);
            }

            var current = this.From;
            var currentPath = first;
            while(current != null)
            {
                Path next = null;
                if (current.DirectedEdge == Constants.NO_EDGE)
                {
                    next = new Path(Constants.NO_VERTEX, current.Weight, null);
                }
                else
                {
                    var target = edgeEnumerator.GetTargetVertex(current.DirectedEdge);
                    next = new Path(target, current.Weight, null);
                }
                currentPath.From = next;
                currentPath = next;

                current = current.From;
            }
            return first;  
        }
    }
}