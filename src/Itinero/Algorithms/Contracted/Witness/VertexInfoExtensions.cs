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

using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;

namespace Itinero.Algorithms.Contracted.Witness
{
    /// <summary>
    /// Contains extension methods related to the vertex info data structure.
    /// </summary>
    public static class VertexInfoExtensions
    {
        /// <summary>
        /// Adds edges relevant for contraction to the given vertex info, assuming it's empty.
        /// </summary>
        public static void AddRelevantEdges<T>(this VertexInfo<T> vertexInfo, DirectedMetaGraph.EdgeEnumerator enumerator)
            where T : struct
        {
            System.Diagnostics.Debug.Assert(vertexInfo.Count == 0);

            var vertex = vertexInfo.Vertex;

            enumerator.MoveTo(vertex);
            while(enumerator.MoveNext())
            {
                if (enumerator.Neighbour == vertex)
                {
                    continue;
                }

                vertexInfo.Add(enumerator.Current);
            }
        }

        /// <summary>
        /// Builds the potential shortcuts.
        /// </summary>
        public static void BuildShortcuts<T>(this VertexInfo<T> vertexinfo, WeightHandler<T> weightHandler)
            where T : struct
        {
            var vertex = vertexinfo.Vertex;
            var shortcuts = vertexinfo.Shortcuts;

            // loop over all edge-pairs once.
            var shortcut = new Shortcut<T>()
            {
                Backward = weightHandler.Infinite,
                Forward = weightHandler.Infinite
            };
            var shortcutEdge = new OriginalEdge();
            for (var j = 1; j < vertexinfo.Count; j++)
            {
                var edge1 = vertexinfo[j];
                var edge1Weight = weightHandler.GetEdgeWeight(edge1);
                shortcutEdge.Vertex1 = edge1.Neighbour;

                // figure out what witness paths to calculate.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = vertexinfo[k];
                    var edge2Weight = weightHandler.GetEdgeWeight(edge2);
                    shortcutEdge.Vertex2 = edge2.Neighbour;

                    if (!(edge1Weight.Direction.B && edge2Weight.Direction.F) &&
                        !(edge1Weight.Direction.F && edge2Weight.Direction.B))
                    { // impossible route, do nothing.
                        continue;
                    }

                    shortcut.Backward = weightHandler.Infinite;
                    shortcut.Forward = weightHandler.Infinite;

                    if (edge1Weight.Direction.B && edge2Weight.Direction.F)
                    {
                        shortcut.Forward = weightHandler.Add(edge1Weight.Weight, edge2Weight.Weight);
                    }
                    if (edge1Weight.Direction.F && edge2Weight.Direction.B)
                    {
                        shortcut.Backward = weightHandler.Add(edge1Weight.Weight, edge2Weight.Weight);
                    }

                    shortcuts.AddOrUpdate(shortcutEdge, shortcut, weightHandler);
                }
            }
        }

        /// <summary>
        /// Calculates the priority of this vertex.
        /// </summary>
        public static float Priority<T>(this VertexInfo<T> vertexInfo, WeightHandler<T> weightHandler, float differenceFactor, float contractedFactor, float depthFactor)
            where T : struct
        {
            var vertex = vertexInfo.Vertex;

            var removed = 0;
            var added = 0;

            foreach (var shortcut in vertexInfo.Shortcuts)
            {
                var shortcutForward = weightHandler.GetMetric(shortcut.Value.Forward);
                var shortcutBackward = weightHandler.GetMetric(shortcut.Value.Backward);

                if (shortcutForward > 0 && shortcutBackward > 0 &&
                    System.Math.Abs(shortcutForward - shortcutBackward) < HierarchyBuilder.E)
                { // add two bidirectional edges.
                        added += 2;
                }
                else
                {
                    if(shortcutForward > 0)
                    {
                        added += 2;
                    }
                    if (shortcutBackward > 0)
                    {
                        added += 2;
                    }
                }
            }

            removed = vertexInfo.Count;
            
            return differenceFactor * (added - removed) + (depthFactor * vertexInfo.Depth) +
                (contractedFactor * vertexInfo.ContractedNeighbours);
        }
    }
}
