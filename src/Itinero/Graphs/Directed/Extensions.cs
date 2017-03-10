/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

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

        /// <summary>
        /// Gets the edge from vertex1 -> vertex2.
        /// </summary>
        public static DynamicEdge GetEdge(this DirectedDynamicGraph.EdgeEnumerator enumerator, uint vertex1, uint vertex2)
        {
            if(!enumerator.MoveTo(vertex1))
            {
                throw new Exception("Vexter does not exist.");
            }
            while (enumerator.MoveNext())
            {
                if (enumerator.Neighbour == vertex2)
                {
                    return enumerator.Current;
                }
            }
            throw new Exception(string.Format("Edge {0}->{1} not found.", vertex1, vertex2));
        }
    }
}
