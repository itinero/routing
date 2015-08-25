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

using OsmSharp.Math.Geo.Simple;
using System;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Abstracts edge information.
    /// </summary>
    public interface IGraphEdgeData : IEquatable<IGraphEdgeData>
    {
        /// <summary>
        /// Returns the forward flag relative to the tags.
        /// </summary>
        bool Forward { get; }

        /// <summary>
        /// Returns true if this edge represents a neighbour relation.
        /// </summary>
        bool RepresentsNeighbourRelations { get; }

        /// <summary>
        /// Returns true if the shape of this edge is within the bounding box formed by it's two vertices.
        /// </summary>
        /// <remarks>False by default, only true when explicitly checked.</remarks>
        bool ShapeInBox { get; }

        /// <summary>
        /// Returns the tag id.
        /// </summary>
        uint Tags
        {
            get;
        }

        /// <summary>
        /// Returns the exact reverse edge.
        /// </summary>
        /// <returns></returns>
        IGraphEdgeData Reverse();
    }
}