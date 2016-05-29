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

using System.Text;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Represents a path along a set of edges/vertices.
    /// </summary>
    public class EdgePath
    {
        /// <summary>
        /// Creates a path source.
        /// </summary>
        public EdgePath(uint vertex = Constants.NO_VERTEX)
        {
            this.Vertex = vertex;
            this.Edge = Constants.NO_EDGE;
            this.Weight = 0;
            this.From = null;
        }

        /// <summary>
        /// Creates a path to the given vertex with the given weight.
        /// </summary>
        public EdgePath(uint vertex, float weight, EdgePath from)
        {
            this.Vertex = vertex;
            this.Edge = Constants.NO_EDGE;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Creates a path to the given vertex with the given weight along the given edge.
        /// </summary>
        public EdgePath(uint vertex, float weight, long edge, EdgePath from)
        {
            this.Vertex = vertex;
            this.Edge = edge;
            this.Weight = weight;
            this.From = from;
        }

        /// <summary>
        /// Gets the edge right before the vertex.
        /// </summary>
        public long Edge { get; private set; }

        /// <summary>
        /// Gets the vertex.
        /// </summary>
        public uint Vertex { get; private set; }

        /// <summary>
        /// Gets the weight at the vertex.
        /// </summary>
        public float Weight { get; private set; }

        /// <summary>
        /// Gets previous path.
        /// </summary>
        public EdgePath From { get; private set; }

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
    }
}
