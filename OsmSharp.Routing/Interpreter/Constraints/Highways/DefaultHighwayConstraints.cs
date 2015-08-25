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

using System.Collections.Generic;
using System.Text.RegularExpressions;
using OsmSharp.Routing.Constraints;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Collections.Tags;

namespace OsmSharp.Routing.Interpreter.Constraints.Highways
{
    /// <summary>
    /// Handles default highway constraints.
    /// </summary>
    public class DefaultHighwayConstraints : IRoutingConstraints
    {
        /// <summary>
        /// Holds the edge interpreter.
        /// </summary>
        private IEdgeInterpreter _edge_intepreter;

//        /// <summary>
//        /// Holds the local label.
//        /// </summary>
//        private RoutingLabel _local_label = 
//            new RoutingLabel('L', "OnlyLocalAccessible");

//        /// <summary>
//        /// Holds the general label.
//        /// </summary>
//        private RoutingLabel _general_label = 
//            new RoutingLabel('R', "GeneralAccessible");

        /// <summary>
        /// Creates a new highway constraint.
        /// </summary>
        /// <param name="edge_intepreter"></param>
        public DefaultHighwayConstraints(IEdgeInterpreter edge_intepreter)
        {
            _edge_intepreter = edge_intepreter;
        }

        /// <summary>
        /// Returns a label for different categories of highways.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public RoutingLabel GetLabelFor(TagsCollectionBase tags)
        {
            if (_edge_intepreter.IsOnlyLocalAccessible(tags))
            {
                return new RoutingLabel('L', "OnlyLocalAccessible"); // local
            }
            return new RoutingLabel('R', "GeneralAccessible"); // regular.
        }

        /// <summary>
        /// Returns true if the given sequence is allowed.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="latest"></param>
        /// <returns></returns>
        public bool ForwardSequenceAllowed(IList<RoutingLabel> sequence, RoutingLabel latest)
        {
            return Regex.IsMatch(sequence.CreateString(latest), "^L*R*L*$");
        }

        /// <summary>
        /// Returns true if the given sequence is allowed.
        /// </summary>
        /// <param name="sequence"></param>
        /// <param name="latest"></param>
        /// <returns></returns>
        public bool BackwardSequenceAllowed(IList<RoutingLabel> sequence, RoutingLabel latest)
        {
            return Regex.IsMatch(sequence.CreateString(latest), "^L*R*L*$");
        }
    }
}