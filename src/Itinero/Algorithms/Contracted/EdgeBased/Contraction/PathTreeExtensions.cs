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
using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Contains extension methods for the path tree data structure.
    /// </summary>
    public static class PathTreeExtensions
    {
        /// <summary>
        /// Adds a new settled edge.
        /// </summary>
        public static uint AddSettledEdge(this PathTree tree, OriginalEdge edge1, OriginalEdge edge2, WeightAndDir<float> weightAndDir, uint hops, uint fromPointer)
        {
            var hopsAndDirection = hops * 4 + weightAndDir.Direction._val;
            return tree.Add(edge1.Vertex1, edge2.Vertex2, edge2.Vertex1, edge2.Vertex2, (uint)(weightAndDir.Weight * 10),
                hopsAndDirection, fromPointer);
        }

        /// <summary>
        /// Gets a settled edge.
        /// </summary>
        public static void GetSettledEdge(this PathTree tree, uint pointer, out OriginalEdge edge1, out OriginalEdge edge2,
            out WeightAndDir<float> weightAndDir, out uint hops, out uint previous)
        {
            uint data0, data1, data2, data3, data4, data5, data6;
            tree.Get(pointer, out data0, out data1, out data2, out data3, out data4, out data5, out data6);
            edge1 = new OriginalEdge(data0, data1);
            edge2 = new OriginalEdge(data2, data3);
            previous = data6;
            weightAndDir = new WeightAndDir<float>()
            {
                Weight = data4 / 10.0f,
                Direction = new Dir()
                {
                    _val = (byte)(data5 & 3)
                }
            };
            hops = data5 / 4;
        }
        
        /// <summary>
        /// Gets a settled edge weight.
        /// </summary>
        public static WeightAndDir<float> GetSettledEdgeWeight(this PathTree tree, uint pointer)
        {
            uint data0, data1, data2, data3, data4, data5;
            tree.Get(pointer, out data0, out data1, out data2, out data3, out data4, out data5);
            return new WeightAndDir<float> ()
            {
                Weight = data4 / 10.0f,
                Direction = new Dir()
                {
                    _val = (byte)(data5 & 3)
                }
            };
        }
    }
}