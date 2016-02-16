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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing.Interpreter;
using OsmSharp;
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Interface used to match a coordinate to a configurable routable position.
    /// </summary>
    public interface IEdgeMatcher
    {
        /// <summary>
        /// Returns true if the edge is a suitable candidate as a target for a point to be resolved on.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="pointTags"></param>
        /// <param name="edgeTags"></param>
        /// <returns></returns>
        bool MatchWithEdge(Vehicle vehicle, TagsCollectionBase pointTags, TagsCollectionBase edgeTags);
    }

    /// <summary>
    /// A default implementation of the edge matcher.
    /// </summary>
    public class DefaultEdgeMatcher : IEdgeMatcher
    {
        /// <summary>
        /// Returns true if the edge is a suitable candidate as a target for a point to be resolved on.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="pointTags"></param>
        /// <param name="edgeTags"></param>
        /// <returns></returns>
        public bool MatchWithEdge(Vehicle vehicle,
            TagsCollectionBase pointTags, TagsCollectionBase edgeTags)
        {
            if (pointTags == null || pointTags.Count == 0)
            { // when the point has no tags it has no requirements.
                return true;
            }

            if (edgeTags == null || edgeTags.Count == 0)
            { // when the edge has no tags, no way to verify.
                return false;
            }

            string pointName, edgeName;
            if (pointTags.TryGetValue("name", out pointName) &&
                edgeTags.TryGetValue("name", out edgeName))
            { // both have names.
                return (pointName == edgeName);
            }
            return false;
        }
    }

    /// <summary>
    /// A Levenshtein matching implementation of the edge matcher.
    /// </summary>
    public class LevenshteinEdgeMatcher : IEdgeMatcher
    {
        /// <summary>
        /// Returns true if the edge is a suitable candidate as a target for a point to be resolved on.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="pointTags"></param>
        /// <param name="edgeTags"></param>
        /// <returns></returns>
        public bool MatchWithEdge(Vehicle vehicle,
            TagsCollectionBase pointTags, TagsCollectionBase edgeTags)
        {
            if (pointTags == null || pointTags.Count == 0)
            { // when the point has no tags it has no requirements.
                return true;
            }

            if (edgeTags == null || edgeTags.Count == 0)
            { // when the edge has no tags, no way to verify.
                return false;
            }

            string pointName, edgeName;
            if (pointTags.TryGetValue("name", out pointName) &&
                edgeTags.TryGetValue("name", out edgeName))
            { // both have names.
                return (pointName.LevenshteinMatch(edgeName, 90));
            }
            return false;
        }
    }
}