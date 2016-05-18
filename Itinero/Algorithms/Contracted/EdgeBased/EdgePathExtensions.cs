// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Graphs.Directed;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Extension method for the edge path.
    /// </summary>
    public static class EdgePathExtensions
    {
        /// <summary>
        /// Returns the equivalent path.
        /// </summary>
        public static Path ToPath(this EdgePath edgePath, DirectedDynamicGraph graph, Dictionary<EdgePath, Path> paths, bool reversed)
        {
            var enumerator = graph.GetEdgeEnumerator();
            if (edgePath.Edge == Constants.NO_EDGE)
            {
                Path vertexPath;
                if (paths.TryGetValue(edgePath, out vertexPath))
                {
                    return vertexPath;
                }
            }
            enumerator.MoveToEdge(edgePath.Edge);
            var path = new Path(enumerator.Neighbour);
            var original = path;
            if (!reversed)
            {
                var c = edgePath.From;
                while (c != null)
                {
                    if (c.Edge == Constants.NO_EDGE)
                    {
                        Path vertexPath;
                        if (paths.TryGetValue(c, out vertexPath))
                        {
                            path.From = vertexPath;
                            path.Weight = c.Weight + path.Weight;
                        }
                        break;
                    }
                    else
                    {
                        enumerator.MoveToEdge(c.Edge);
                        path.From = new Path(enumerator.Neighbour);
                        path.Weight = c.Weight;
                        path = path.From;
                    }
                    c = c.From;
                }
                return original;
            }
            else
            {
                var c = edgePath.From;
                while (c != null)
                {
                    if (c.Edge == Constants.NO_EDGE)
                    {
                        break;
                    }
                    else
                    {
                        enumerator.MoveToEdge(c.Edge);
                        path = new Path(enumerator.Neighbour, c.Weight, path);
                    }
                    c = c.From;
                }
            }
            return path;
        }
    }
}
