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
using OsmSharp.Routing.Constraints;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Osm;
using OsmSharp.Collections.Tags;

namespace OsmSharp.Routing.Interpreter
{
    /// <summary>
    /// Interprets routing data abstracting the type of data.
    /// </summary>
    public interface IRoutingInterpreter
    {
        /// <summary>
        /// Returns true if the given tag is relevant for this interpreter, false otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsRelevant(string key);

        /// <summary>
        /// Returns true if the given key-value pair is relevant for this interpreter, false otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        bool IsRelevant(string key, string value);

        /// <summary>
        /// Returns true if the given tag is relevant for this interpreter for routing, false otherwise.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsRelevantRouting(string key);

        /// <summary>
        /// Returns true if the given vertices can be traversed in the given order.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="along"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        bool CanBeTraversed(long from, long along, long to);

        /// <summary>
        /// Returns the edge interpreter.
        /// </summary>
        IEdgeInterpreter EdgeInterpreter
        {
            get;
        }

        /// <summary>
        /// Returns the routing constraints.
        /// </summary>
        IRoutingConstraints Constraints
        {
            get;
        }
    }
}