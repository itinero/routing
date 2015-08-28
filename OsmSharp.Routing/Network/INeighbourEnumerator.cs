// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using OsmSharp.Collections.Coordinates.Collections;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Represents an abstract neighbour enumerator, enumerable and neighbour.
    /// </summary>
    public interface INeighbourEnumerator<TEdgeData> : IEnumerable<Neighbour<TEdgeData>>, IEnumerator<Neighbour<TEdgeData>>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Returns the first vertex.
        /// </summary>
        uint Vertex1
        {
            get;
        }

        /// <summary>
        /// Returns the second vertex.
        /// </summary>
        uint Vertex2
        {
            get;
        }

        /// <summary>
        /// Returns the edge data.
        /// </summary>
        TEdgeData EdgeData
        {
            get;
        }

        /// <summary>
        /// Returns the intermediates.
        /// </summary>
        ICoordinateCollection Intermediates
        {
            get;
        }

        /// <summary>
        /// Returns true if the count is known without enumeration.
        /// </summary>
        bool HasCount
        {
            get;
        }

        /// <summary>
        /// Returns the count if known.
        /// </summary>
        int Count
        {
            get;
        }
    }

    /// <summary>
    /// Abstract representation of neighbours.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public class Neighbour<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Creates a new neighbour.
        /// </summary>
        public Neighbour()
        {

        }

        /// <summary>
        /// Creates a new neighbour by copying the given neighbour.
        /// </summary>
        /// <param name="edge"></param>
        public Neighbour(Neighbour<TEdgeData> edge)
        {
            this.Vertex1 = edge.Vertex1;
            this.Vertex2 = edge.Vertex2;
            this.EdgeData = edge.EdgeData;
            this.Intermediates = edge.Intermediates;
        }

        /// <summary>
        /// Creates a new neighbour keeping the current state of the given enumerator.
        /// </summary>
        /// <param name="enumerator"></param>
        public Neighbour(INeighbourEnumerator<TEdgeData> enumerator)
        {
            this.Vertex1 = enumerator.Vertex1;
            this.Vertex2 = enumerator.Vertex2;
            this.EdgeData = enumerator.EdgeData;
            this.Intermediates = enumerator.Intermediates;
        }

        /// <summary>
        /// Returns the first vertex.
        /// </summary>
        public uint Vertex1
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the second vertex.
        /// </summary>
        public uint Vertex2
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
        /// Returns the intermediates.
        /// </summary>
        public ICoordinateCollection Intermediates
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
            return string.Format("{0}->{1}  {2}",
                this.Vertex1, this.Vertex2,
                this.EdgeData.ToInvariantString());
        }
    }
}