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

using OsmSharp.Collections.Coordinates.Collections;

namespace OsmSharp.Routing.Graph.Geometric
{
    /// <summary>
    /// A geometric edge.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public class GeometricEdge<TEdgeData>
        where TEdgeData : struct, IEdgeData
    {
        /// <summary>
        /// Creates a new geometric edge.
        /// </summary>
        public GeometricEdge(uint id, uint from, uint to, TEdgeData edgeData, bool edgeDataInverted, 
            ICoordinateCollection shape)
        {
            this.Id = id;
            this.From = from;
            this.To = to;
            this.EdgeData = edgeData;
            this.EdgeDataInverted = edgeDataInverted;
            this.Shape = shape;
        }

        /// <summary>
        /// Creates a new geometric edge.
        /// </summary>
        /// <param name="enumerator"></param>
        internal GeometricEdge(GeometricGraph<TEdgeData>.EdgeEnumerator enumerator)
        {
            this.Id = enumerator.Id;
            this.From = enumerator.From;
            this.To = enumerator.To;
            this.EdgeData = enumerator.EdgeData;
            this.EdgeDataInverted = enumerator.EdgeDataInverted;
            this.Shape = enumerator.Shape;
        }

        /// <summary>
        /// Gets the edge id.
        /// </summary>
        public uint Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vertex at the beginning of this edge.
        /// </summary>
        public uint From
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vertex at the end of this edge.
        /// </summary>
        public uint To
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the edge data is inverted relative to the direction of this edge.
        /// </summary>
        public bool EdgeDataInverted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the edge data.
        /// </summary>
        public TEdgeData EdgeData
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the directed edge data.
        /// </summary>
        /// <returns></returns>
        public TEdgeData GetDirectedEdgeData()
        {
            if (this.EdgeDataInverted)
            {
                return (TEdgeData)this.EdgeData.Reverse();
            }
            return this.EdgeData;
        }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public ICoordinateCollection Shape
        {
            get;
            private set;
        }
    }
}