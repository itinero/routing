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

using Itinero.Algorithms.Collections;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Contains extension methods for the path tree.
    /// </summary>
    public static class PathTreeExtensions
    {
        /// <summary>
        /// Adds a new settled edge.
        /// </summary>
        public static uint AddSettledEdge(this PathTree tree, OriginalEdge edge1, OriginalEdge edge2, float weight, uint edge, uint fromPointer)
        {
            return tree.Add(edge1.Vertex1, edge1.Vertex2, edge2.Vertex1, edge2.Vertex2, (uint)(weight * 10), edge, fromPointer);
        }

        /// <summary>
        /// Gets a settled edge.
        /// </summary>
        public static void GetSettledEdge(this PathTree tree, uint pointer, out OriginalEdge edge1, out OriginalEdge edge2,
            out float weight)
        {
            uint data0, data1, data2, data3, data4;
            tree.Get(pointer, out data0, out data1, out data2, out data3, out data4);
            edge1 = new OriginalEdge(data0, data1);
            edge2 = new OriginalEdge(data2, data3);
            weight = data4 / 10f;
        }

        /// <summary>
        /// Gets a settled edge.
        /// </summary>
        public static void GetSettledEdge(this PathTree tree, uint pointer, out OriginalEdge edge1, out OriginalEdge edge2,
            out float weight, out uint edge, out uint fromPrevious)
        {
            uint data0, data1, data2, data3, data4, data5, data6;
            tree.Get(pointer, out data0, out data1, out data2, out data3, out data4, out data5, out data6);
            edge1 = new OriginalEdge(data0, data1);
            edge2 = new OriginalEdge(data2, data3);
            weight = data4 / 10f;
            edge = data5;
            fromPrevious = data6;
        }

        /// <summary>
        /// Gets the edge-path fro the given pointer.
        /// </summary>
        public static EdgePath<float> GetEdgePath(this PathTree tree, uint pointer)
        {
            OriginalEdge edge1, edge2;
            float weight;
            uint edge, previous;
            tree.GetSettledEdge(pointer, out edge1, out edge2, out weight, out edge, out previous);
            if (previous == uint.MaxValue)
            {
                return new EdgePath<float>(edge2.Vertex2);
            }
            var path = new EdgePath<float>(edge2.Vertex2, weight, new DirectedEdgeId(edge, true), null);
            var first = path;
            while(true)
            {
                tree.GetSettledEdge(previous, out edge1, out edge2, out weight, out edge, out previous);
                if (previous == uint.MaxValue)
                {
                    path.From = new EdgePath<float>(edge2.Vertex2);
                    return first;
                }
                path.From = new EdgePath<float>(edge2.Vertex2, weight, new DirectedEdgeId(edge, true), null);
                path = path.From;
            }
        }
    }
}