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
        public static uint AddSettledEdge(this PathTree tree, OriginalEdge edge, WeightAndDir<float> weightAndDir, uint hops, uint fromPointer)
        {
            var hopsAndDirection = hops * 4 + weightAndDir.Direction._val;
            return tree.Add(edge, (uint)(weightAndDir.Weight * 10), hopsAndDirection, fromPointer);
        }

        /// <summary>
        /// Gets a settled edge.
        /// </summary>
        public static OriginalEdge GetSettledEdge(this PathTree tree, uint pointer, out WeightAndDir<float> weightAndDir, out uint hops)
        {
            uint data0, data1;
            var edge = tree.Get(pointer, out data0, out data1);
            weightAndDir = new WeightAndDir<float>()
            {
                Weight = data0 / 10.0f,
                Direction = new Dir()
                {
                    _val = (byte)(data1 & 3)
                }
            };
            hops = data1 / 4;
            return edge;
        }
    }
}