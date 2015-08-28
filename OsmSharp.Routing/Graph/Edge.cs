// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Abstract representation of an edge.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public class Edge<TEdgeData>
        where TEdgeData : struct, IEdgeData
    {
        /// <summary>
        /// Creates a new edge.
        /// </summary>
        internal Edge()
        {

        }

        /// <summary>
        /// Creates a new edge.
        /// </summary>
        internal Edge(uint neighbour, TEdgeData edgeData)
        {
            this.Neighbour = neighbour;
            this.EdgeData = edgeData;
        }

        /// <summary>
        /// Creates a new edge by copying the given edge.
        /// </summary>
        internal Edge(Edge<TEdgeData> edge)
        {
            this.Neighbour = edge.Neighbour;
            this.EdgeData = edge.EdgeData;
        }

        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal Edge(Graph<TEdgeData>.GraphEdgeEnumerator enumerator)
        {
            this.Neighbour = enumerator.Neighbour;
            this.EdgeData = enumerator.EdgeData;
        }

        /// <summary>
        /// Returns the current neighbour.
        /// </summary>
        public uint Neighbour
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the edge data.
        /// </summary>
        public TEdgeData EdgeData
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string representing this edge.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}",
                this.Neighbour,
                this.EdgeData.ToInvariantString());
        }
    }
}