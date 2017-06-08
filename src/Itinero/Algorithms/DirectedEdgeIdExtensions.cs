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

using Itinero.Graphs;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Contains extension methods related to the directed edge id.
    /// </summary>
    public static class DirectedEdgeIdExtensions
    {
        /// <summary>
        /// Moves the enumerator to the edge represented by the given directed edge id.
        /// </summary>
        public static void MoveToEdge(this Graph.EdgeEnumerator enumerator, DirectedEdgeId edgeId)
        {
            enumerator.MoveToEdge(edgeId.EdgeId);
        }
        
        /// <summary>
        /// Returns an array of directed edges id's for a given array of routerpoints and forward flags.
        /// </summary>
        public static DirectedEdgeId[] ToDirectedEdgeIds(this RouterPoint[] points, bool[] forwards)
        {
            var directedIds = new DirectedEdgeId[points.Length];
            for(var i = 0; i < points.Length; i++)
            {
                directedIds[i] = new DirectedEdgeId(points[i].EdgeId, forwards[i]);
            }
            return directedIds;
        }
    }
}