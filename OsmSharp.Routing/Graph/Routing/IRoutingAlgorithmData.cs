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

using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph.Routing
{
    /// <summary>
    /// Abstracts a data source of a router that is a dynamic graph with an extra lookup function.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public interface IRoutingAlgorithmData<TEdgeData> : IGraphReadOnly<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Returns true if the given vehicle profile is supported by the the data in this data source.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        bool SupportsProfile(Vehicle vehicle);

        /// <summary>
        /// Returns true if the given profile is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        void AddSupportedProfile(Vehicle vehicle);

        /// <summary>
        /// Returns a list of edges and their vertices inside the given bounding box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        INeighbourEnumerator<TEdgeData> GetEdges(GeoCoordinateBox box);

        /// <summary>
        /// Returns all neighbours even the reverse edges in directed graph.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        /// <remarks>
        /// Only to be used for generating instructions or statistics about a route.
        /// WARNING: could potentially increase memory usage.
        /// </remarks>
        IEnumerable<Edge<TEdgeData>> GetDirectNeighbours(uint vertex);

        /// <summary>
        /// Returns the tags index.
        /// </summary>
        ITagsIndex TagsIndex
        {
            get;
        }


        /// <summary>
        /// Adds a restriction to this graph by prohibiting the given route.
        /// </summary>
        /// <param name="route"></param>
        void AddRestriction(uint[] route);

        /// <summary>
        /// Adds a restriction to this graph by prohibiting the given route for the given vehicle.
        /// </summary>
        /// <param name="vehicleType"></param>
        /// <param name="route"></param>
        void AddRestriction(string vehicleType, uint[] route);

        /// <summary>
        /// Returns all restricted routes that start in the given vertex.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
        bool TryGetRestrictionAsStart(Vehicle vehicle, uint vertex, out List<uint[]> routes);

        /// <summary>
        /// Returns true if there is a restriction that ends with the given vertex.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex"></param>
        /// <param name="routes"></param>
        /// <returns></returns>
        bool TryGetRestrictionAsEnd(Vehicle vehicle, uint vertex, out List<uint[]> routes);
    }
}