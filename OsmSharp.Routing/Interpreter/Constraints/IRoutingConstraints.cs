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
using OsmSharp.Collections.Tags;
using OsmSharp.Math;

namespace OsmSharp.Routing.Constraints
{
    /// <summary>
    /// Represents some routing constraints.
    /// </summary>
    /// <remarks>Objects of this type can be used to constrain routes that will be found to certain criteria.</remarks>
    public interface IRoutingConstraints
    {
        /// <summary>
        /// Translates some tags into some routing label.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        RoutingLabel GetLabelFor(TagsCollectionBase tags);

        /// <summary>
        /// Returns true if a given (forward) sequence of labels is allowed under these constraints.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="latest"></param>
        /// <returns></returns>
        bool ForwardSequenceAllowed(IList<RoutingLabel> sequence, RoutingLabel latest);

        /// <summary>
        /// Returns true if a given (backward) sequence of labels is allowed under these constraints.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="latest"></param>
        /// <returns></returns>
        bool BackwardSequenceAllowed(IList<RoutingLabel> sequence, RoutingLabel latest);
    }
}