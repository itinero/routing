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

using OsmSharp.Collections.Arrays;

namespace OsmSharp.Routing.Graph.Directed
{
    /// <summary>
    /// Abstract representation of edge data that can be memory-mapped.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public interface IMappedEdgeData<TEdgeData> : IEdgeData
        where TEdgeData : struct, IEdgeData
    {
        /// <summary>
        /// Returns the map-from delegate.
        /// </summary>
        MappedHugeArray<TEdgeData, uint>.MapFrom MapFromDelegate
        {
            get;
        }

        /// <summary>
        /// Returns the map-to delegate.
        /// </summary>
        MappedHugeArray<TEdgeData, uint>.MapTo MapToDelegate
        {
            get;
        }

        /// <summary>
        /// The size in uint's onces mapped.
        /// </summary>
        int MappedSize
        {
            get;
        }
    }
}