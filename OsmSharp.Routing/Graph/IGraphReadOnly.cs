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
using OsmSharp.Math.Geo;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Abstracts a graph implementation. 
    /// </summary>
    public interface IGraphReadOnly<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Returns true if an edge is only as an outgoing edge. When false, edges are added both from and to.
        /// </summary>
        bool IsDirected { get; }

        /// <summary>
        /// Returns true if there can be multiple edges 
        /// </summary>
        bool CanHaveDuplicates { get; }

        /// <summary>
        /// Gets an existing vertex.
        /// </summary>
        /// <param name="vertex">The vertex.</param>
        /// <param name="latitude">The latitude.</param>
        /// <param name="longitude">The longitude.</param>
        bool GetVertex(uint vertex, out float latitude, out float longitude);

        /// <summary>
        /// Returns true if the given edge exists.
        /// </summary>
        /// <param name="vertexId"></param>
        /// <param name="neighbour"></param>
        /// <returns></returns>
        bool ContainsEdges(uint vertexId, uint neighbour);

        /// <summary>
        /// Returns true if the given edge exists.
        /// </summary>
        /// <param name="vertexId"></param>
        /// <param name="neighbour"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool ContainsEdge(uint vertexId, uint neighbour, TEdgeData data);

        /// <summary>
        /// Returns an empty enumerator.
        /// </summary>
        /// <returns></returns>
        EdgeEnumerator<TEdgeData> GetEdgeEnumerator();

        /// <summary>
        /// Returns all edges adjacent to the given vertex.
        /// </summary>
        /// <param name="vertexId"></param>
        /// <returns></returns>
        EdgeEnumerator<TEdgeData> GetEdges(uint vertexId);

        /// <summary>
        /// Returns all edges between the given vertices.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        EdgeEnumerator<TEdgeData> GetEdges(uint vertex1, uint vertex2);

        /// <summary>
        /// Returns the edge between the two given vertices. Throws exception if this graph allows duplicate edges, use GetEdges in that case..
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        bool GetEdge(uint vertex1, uint vertex2, out TEdgeData data);

        /// <summary>
        /// Returns the edge shape between the two given vertices. Throws exception if this graph allows duplicate edges, use GetEdges in that case.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="shape"></param>
        /// <returns></returns>
        bool GetEdgeShape(uint vertex1, uint vertex2, out ICoordinateCollection shape);

        /// <summary>
        /// Returns the total number of vertices.
        /// </summary>
        uint VertexCount
        {
            get;
        }
    }
}