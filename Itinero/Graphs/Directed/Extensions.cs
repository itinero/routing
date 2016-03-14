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

using System;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// Contains extension methods for the directed graphs and related.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Returns the number of elements in this enumerator.
        /// </summary>
        public static int Count(this DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            var c = 0;
            enumerator.Reset();

            while(enumerator.MoveNext())
            {
                c++;
            }
            return c;
        }

        /// <summary>
        /// Moves the numerator to the first element.
        /// </summary>
        public static DirectedDynamicGraph.EdgeEnumerator First(this DirectedDynamicGraph.EdgeEnumerator enumerator)
        {
            return enumerator.First(x => true);
        }

        /// <summary>
        /// Moves the enumerator until the given condition is true or throws an exception if the condition is never true.
        /// </summary>
        public static DirectedDynamicGraph.EdgeEnumerator First(this DirectedDynamicGraph.EdgeEnumerator enumerator, 
            Func<DirectedDynamicGraph.EdgeEnumerator, bool> stop)
        {
            enumerator.Reset();

            while(enumerator.MoveNext())
            {
                if (stop(enumerator))
                {
                    return enumerator;
                }
            }
            throw new Exception("No edge found that satisfies the given condition.");
        }

        /// <summary>
        /// Returns enumerator for the given vertex. Throws an exception if the vertex is not in the graph.
        /// </summary>
        public static DirectedDynamicGraph.EdgeEnumerator GetEdgeEnumerator(this DirectedDynamicGraph graph, uint vertex)
        {
            var enumerator = graph.GetEdgeEnumerator();
            if (!enumerator.MoveTo(vertex))
            {
                throw new Exception("Vertex does not exist.");
            }
            return enumerator;
        }
    }
}
