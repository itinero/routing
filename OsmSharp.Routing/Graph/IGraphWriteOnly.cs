// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Abstracts a graph implementation that is write-only.
    /// </summary>
    public interface IGraphWriteOnly<TEdgeData> : IGraphReadOnly<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Adds a vertex.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        uint AddVertex(float latitude, float longitude);

        /// <summary>
        /// Sets the vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        void SetVertex(uint vertex, float latitude, float longitude);

        /// <summary>
        /// Adds an edge with associated data and no shapes.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        void AddEdge(uint from, uint to, TEdgeData data);

        /// <summary>
        /// Adds an edge with associated data.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <param name="coordinates"></param>
        void AddEdge(uint from, uint to, TEdgeData data, ICoordinateCollection coordinates);

        /// <summary>
        /// Compresses all the data in this graph.
        /// </summary>
        /// <remarks>Does not trim or resize the internal data structures. Use Trim()</remarks>
        void Compress();

        /// <summary>
        /// Trims all internal datastructures to their smallest possible size.
        /// </summary>
        /// <remarks>Be careful when doing this, could need up to 2x the current memory usage.</remarks>
        void Trim();

        /// <summary>
        /// Resizes the internal data structures of the graph to handle the number of vertices/edges estimated.
        /// </summary>
        /// <param name="vertexEstimate"></param>
        /// <param name="edgeEstimate"></param>
        void Resize(long vertexEstimate, long edgeEstimate);
    }
}