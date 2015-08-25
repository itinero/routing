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

using OsmSharp.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmSharp.Test.Unittests.Routing
{
    /// <summary>
    /// A reference stream target to build a reference network.
    /// </summary>
    public class ReferenceGraphTarget : GraphOsmStreamTarget
    {
        /// <summary>
        /// Creates a reference stream target.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="tagsIndex"></param>
        /// <param name="vehicles"></param>
        public ReferenceGraphTarget(RouterDataSourceBase<Edge> graph, IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex, IEnumerable<Vehicle> vehicles)
            : base(graph, interpreter, tagsIndex, vehicles, false)
        {

        }
    }
}
