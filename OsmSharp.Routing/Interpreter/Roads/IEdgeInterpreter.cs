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

using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;

namespace OsmSharp.Routing.Interpreter.Roads
{
    /// <summary>
    /// Interpreter for edges in the routable data.
    /// </summary>
    public interface IEdgeInterpreter
    {
        /// <summary>
        /// Returns true if in some configuration this edge is traversable.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        bool IsRoutable(TagsCollectionBase tags);

        /// <summary>
        /// Returns true if the edge with given tags can be traversed by the given vehicle.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        bool CanBeTraversedBy(TagsCollectionBase tags, Vehicle vehicle);

        /// <summary>
        /// Returns true if the edge is only locally accessible.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        bool IsOnlyLocalAccessible(TagsCollectionBase tags);

        /// <summary>
        /// Returns the name of the edge represented by the tags.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        string GetName(TagsCollectionBase tags);

        /// <summary>
        /// Returns the names of the edge represented by the tags in each available language.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        Dictionary<string, string> GetNamesInAllLanguages(TagsCollectionBase tags);

        /// <summary>
        /// Returns true if the edge with given properties is a roundabout.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        bool IsRoundabout(TagsCollectionBase tags);
    }
}