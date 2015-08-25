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

using OsmSharp.Math.Geo;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// A router data source baseclass that wraps a graph but also contains other meta-data for routing.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public abstract class RouterDataSourceBase<TEdgeData> : GraphBase<TEdgeData>, IRoutingAlgorithmData<TEdgeData>
        where TEdgeData : struct, IGraphEdgeData
    {
        /// <summary>
        /// Returns true if the given vehicle profile is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public abstract bool SupportsProfile(Vehicle vehicle);

        /// <summary>
        /// Adds one more supported profile.
        /// </summary>
        /// <param name="vehicle"></param>
        public abstract void AddSupportedProfile(Vehicle vehicle);

        /// <summary>
        /// Returns all supported profiles.
        /// </summary>
        public abstract IEnumerable<Vehicle> GetSupportedProfiles();

        /// <summary>
        /// Returns all edges inside the given bounding box.
        /// </summary>
        /// <returns></returns>
        public abstract INeighbourEnumerator<TEdgeData> GetEdges(GeoCoordinateBox box);

        /// <summary>
        /// Returns all neighbours even the reverse edges in directed graph.
        /// </summary>
        /// <returns></returns>
        public abstract IEnumerable<Edge<TEdgeData>> GetDirectNeighbours(uint vertex);

        /// <summary>
        /// Returns the tags index.
        /// </summary>
        public abstract Collections.Tags.Index.ITagsIndex TagsIndex
        {
            get;
        }

        /// <summary>
        /// Adds a restriction to this graph by prohibiting the given route.
        /// </summary>
        public abstract void AddRestriction(uint[] route);

        /// <summary>
        /// Adds a restriction to this graph by prohibiting the given route for the given vehicle.
        /// </summary>
        public abstract void AddRestriction(string vehicleType, uint[] route);

        /// <summary>
        /// Returns all restricted routes that start in the given vertex.
        /// </summary>
        /// <returns></returns>
        public abstract bool TryGetRestrictionAsStart(Vehicle vehicle, uint vertex, out System.Collections.Generic.List<uint[]> routes);

        /// <summary>
        /// Returns true if there is a restriction that ends with the given vertex.
        /// </summary>
        /// <returns></returns>
        public abstract bool TryGetRestrictionAsEnd(Vehicle vehicle, uint vertex, out System.Collections.Generic.List<uint[]> routes);

        /// <summary>
        /// Returns true if this graph is directed.
        /// </summary>
        public override abstract bool IsDirected
        {
            get;
        }

        /// <summary>
        /// Returns true if this graph can have duplicates.
        /// </summary>
        public override abstract bool CanHaveDuplicates
        {
            get;
        }

        /// <summary>
        /// Returns true if a given vertex is in the graph.
        /// </summary>
        /// <returns></returns>
        public override abstract bool GetVertex(uint id, out float latitude, out float longitude);

        /// <summary>
        /// Returns true if the given vertex has the given neighbour.
        /// </summary>
        /// <returns></returns>
        public override abstract bool ContainsEdges(uint vertexId, uint neighbour);

        /// <summary>
        /// Returns true if the given vertex has neighbour as a neighbour.
        /// </summary>
        /// <returns></returns>
        public override abstract bool ContainsEdge(uint vertexId, uint neighbour, TEdgeData data);

        /// <summary>
        /// Returns an edge enumerator.
        /// </summary>
        /// <returns></returns>
        public override abstract EdgeEnumerator<TEdgeData> GetEdgeEnumerator();

        /// <summary>
        /// Returns the edge enumerator for the given vertex.
        /// </summary>
        /// <returns></returns>
        public override abstract EdgeEnumerator<TEdgeData> GetEdges(uint vertexId);

        /// <summary>
        /// Returns true if the given vertex has the given neighbour.
        /// </summary>
        /// <returns></returns>
        public override abstract EdgeEnumerator<TEdgeData> GetEdges(uint vertex1, uint vertex2);

        /// <summary>
        /// Returns true if the given vertex has the given neighbour.
        /// </summary>
        /// <returns></returns>
        public override abstract bool GetEdge(uint vertex1, uint vertex2, out TEdgeData data);

        /// <summary>
        /// Returns true if the given vertex has the given neighbour.
        /// </summary>
        /// <returns></returns>
        public override abstract bool GetEdgeShape(uint vertex1, uint vertex2, out Collections.Coordinates.Collections.ICoordinateCollection shape);

        /// <summary>
        /// Returns the number of vertices in this graph.
        /// </summary>
        public override abstract uint VertexCount
        {
            get;
        }
    }
}