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
using System;
using System.Diagnostics;

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Contains extension methods related to the vertex info data structure.
    /// </summary>
    public static class VertexInfoExtensions
    {
        /// <summary>
        /// Adds edges relevant for contraction to the given vertex info, assuming it's empty.
        /// </summary>
        public static void AddRelevantEdges<T>(this VertexInfo<T> vertexInfo, DirectedDynamicGraph.EdgeEnumerator enumerator)
            where T : struct
        {
            Debug.Assert(vertexInfo.Count == 0);

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
            var shortcuts = vertexinfo.Shortcuts.GetAccessor();
            var hasRestrictions = vertexinfo.HasRestrictions;
            var vertex = vertexinfo.Vertex;

            // loop over all edge-pairs once.
            for (var j = 1; j < vertexinfo.Count; j++)
            {
                var edge1 = vertexinfo[j];

                bool? edge1Direction;
                var edge1Weight = weightHandler.GetEdgeWeight(edge1, out edge1Direction);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                // define source.
                var source = default(OriginalEdge);
                if (edge1.IsOriginal())
                { // is an original edge, create equivalent path.
                    source = new OriginalEdge(edge1.Neighbour, vertex);
                }
                else
                { // is not an original edge, should always have a sequence.
                    var s2 = edge1.GetSequence2();
                    source = new OriginalEdge(edge1.Neighbour, s2[s2.Length - 1]);
                }

                // adds the source.
                shortcuts.AddSource(source);

                // figure out what witness paths to calculate.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = vertexinfo[k];

                    bool? edge2Direction;
                    var edge2Weight = weightHandler.GetEdgeWeight(edge2, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    if (!(edge1CanMoveBackward && edge2CanMoveForward) &&
                        !(edge1CanMoveForward && edge2CanMoveBackward))
                    { // impossible route, do nothing.
                        continue;
                    }

                    var target = default(OriginalEdge);
                    if (edge2.IsOriginal())
                    { // is an original edge just create a path.
                        target = new OriginalEdge(vertex, edge2.Neighbour);
                    }
                    else
                    { // not an original edge, should always have a sequence.
                        var s2 = edge2.GetSequence2();
                        target = new OriginalEdge(s2[s2.Length - 1], edge2.Neighbour);
                    }

                    // create the witness.
                    var witness = new Shortcut<T>()
                    {
                        Edge = target
                    };

                    if (!hasRestrictions)
                    { // no restrictions, max is just edge1 -> edge2.
                        witness.Update(weightHandler, edge1CanMoveForward && edge2CanMoveBackward,
                            edge1CanMoveBackward && edge2CanMoveForward,
                                weightHandler.Add(edge1Weight, edge2Weight));
                    }
                    else
                    { // TODO: some advanced mumbo-jumbo to calculate loops.
                        throw new NotImplementedException("Restriction handling!");
                    }

                    // add witness.
                    shortcuts.Add(witness);
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

            var accessor = vertexInfo.Shortcuts.GetAccessor();
            while (accessor.MoveNextSource())
            {
                while(accessor.MoveNextTarget())
                {
                    var shortcut = accessor.Current;

                    var forwardMetric = weightHandler.GetMetric(shortcut.Forward);
                    var backwardMetric = weightHandler.GetMetric(shortcut.Backward);

                    if (forwardMetric > 0 && backwardMetric > 0 &&
                        System.Math.Abs(backwardMetric - forwardMetric) < HierarchyBuilder<float>.E)
                    { // forward and backward and identical weights.
                        added++;
                    }
                    else
                    {
                        if (forwardMetric > 0)
                        {
                            added++;
                        }
                        if (backwardMetric > 0)
                        {
                            added++;
                        }
                    }
                }
            }
            
            return differenceFactor * (added - removed) + (depthFactor * vertexInfo.Depth) +
                (contractedFactor * vertexInfo.ContractedNeighbours);
        }
    }
}