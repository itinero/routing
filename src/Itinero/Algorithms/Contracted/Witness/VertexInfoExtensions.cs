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
using System.Collections.Generic;
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
        public static float Priority<T>(this VertexInfo<T> vertexInfo, DirectedMetaGraph graph, WeightHandler<T> weightHandler, float differenceFactor, float contractedFactor, 
            float depthFactor, float weightDiffFactor = 1)
            where T : struct
        {
            var vertex = vertexInfo.Vertex;

            var removed = vertexInfo.Count;
            var added = 0;

            //var removedWeight = 0f;
            //var addedWeight = 0f;

            foreach (var shortcut in vertexInfo.Shortcuts)
            {
                var shortcutForward = weightHandler.GetMetric(shortcut.Value.Forward);
                var shortcutBackward = weightHandler.GetMetric(shortcut.Value.Backward);

                int localAdded, localRemoved;
                if (shortcutForward > 0 && shortcutForward < float.MaxValue &&
                    shortcutBackward > 0 && shortcutBackward < float.MaxValue &&
                    System.Math.Abs(shortcutForward - shortcutBackward) < FastHierarchyBuilder<float>.E)
                { // add two bidirectional edges.
                    graph.TryAddOrUpdateEdge(shortcut.Key.Vertex1, shortcut.Key.Vertex2, shortcutForward, null, vertex, 
                        out localAdded, out localRemoved);
                    added += localAdded;
                    removed += localRemoved;
                    graph.TryAddOrUpdateEdge(shortcut.Key.Vertex2, shortcut.Key.Vertex1, shortcutForward, null, vertex,
                        out localAdded, out localRemoved);
                    added += localAdded;
                    removed += localRemoved;

                    //added += 2;
                    //addedWeight += shortcutForward;
                    //addedWeight += shortcutBackward;
                }
                else
                {
                    if (shortcutForward > 0 && shortcutForward < float.MaxValue)
                    {
                        graph.TryAddOrUpdateEdge(shortcut.Key.Vertex1, shortcut.Key.Vertex2, shortcutForward, true, vertex,
                            out localAdded, out localRemoved);
                        added += localAdded;
                        removed += localRemoved;
                        graph.TryAddOrUpdateEdge(shortcut.Key.Vertex2, shortcut.Key.Vertex1, shortcutForward, false, vertex,
                            out localAdded, out localRemoved);
                        added += localAdded;
                        removed += localRemoved;

                        //added += 2;
                        //addedWeight += shortcutForward;
                        //addedWeight += shortcutForward;
                    }
                    if (shortcutBackward > 0 && shortcutBackward < float.MaxValue)
                    {
                        graph.TryAddOrUpdateEdge(shortcut.Key.Vertex1, shortcut.Key.Vertex2, shortcutBackward, false, vertex,
                            out localAdded, out localRemoved);
                        added += localAdded;
                        removed += localRemoved;
                        graph.TryAddOrUpdateEdge(shortcut.Key.Vertex2, shortcut.Key.Vertex1, shortcutBackward, true, vertex,
                            out localAdded, out localRemoved);
                        added += localAdded;
                        removed += localRemoved;

                        //added += 2;
                        //addedWeight += shortcutBackward;
                        //addedWeight += shortcutBackward;
                    }
                }
            }

            //for (var e = 0; e < vertexInfo.Count; e++)
            //{
            //    var w = weightHandler.GetEdgeWeight(vertexInfo[e]);
            //    var wMetric = weightHandler.GetMetric(w.Weight);

            //    if (w.Direction.F)
            //    {
            //        removedWeight += wMetric;
            //    }
            //    if (w.Direction.B)
            //    {
            //        removedWeight += wMetric;
            //    }
            //}

            var weigthDiff = 1f;
            //if (removedWeight != 0)
            //{
            //    weigthDiff = removedWeight;
            //}
            
            return (differenceFactor * (added - removed) + (depthFactor * vertexInfo.Depth) +
                 (contractedFactor * vertexInfo.ContractedNeighbours)) * (weigthDiff * weightDiffFactor);
            //return ((differenceFactor * (2 * added - 4 * removed)) / 2 + (depthFactor * vertexInfo.Depth) +
             //   (contractedFactor * vertexInfo.ContractedNeighbours));// * (weigthDiff * weightDiffFactor);
        }
        
        public static bool RemoveShortcuts<T>(this VertexInfo<T> vertexInfo, DirectedGraph witnessGraph, WeightHandler<T> weightHandler)
            where T : struct
        {
            var witnessGraphEnumerator = witnessGraph.GetEdgeEnumerator();
            
            List<Tuple<OriginalEdge, Shortcut<T>>> toUpdate = null;
            foreach (var pair in vertexInfo.Shortcuts)
            {
                var edge = pair.Key;

                if (edge.Vertex1 >= witnessGraph.VertexCount ||
                    edge.Vertex2 >= witnessGraph.VertexCount)
                {
                    continue;
                }

                var shortcut = pair.Value;

                var shortcutForward = weightHandler.GetMetric(shortcut.Forward);
                var shortcutBackward = weightHandler.GetMetric(shortcut.Backward);

                var witnessedForward = float.MaxValue;
                var witnessedBackward = float.MaxValue;
                if (edge.Vertex1 < edge.Vertex2)
                {
                    witnessGraphEnumerator.MoveTo(edge.Vertex1);
                    while (witnessGraphEnumerator.MoveNext())
                    {
                        if (witnessGraphEnumerator.Neighbour == edge.Vertex2)
                        {
                            witnessedForward = DirectedGraphExtensions.FromData(witnessGraphEnumerator.Data0);
                            witnessedBackward = DirectedGraphExtensions.FromData(witnessGraphEnumerator.Data1);
                            break;
                        }
                    }
                }
                else
                {
                    witnessGraphEnumerator.MoveTo(edge.Vertex2);
                    while (witnessGraphEnumerator.MoveNext())
                    {
                        if (witnessGraphEnumerator.Neighbour == edge.Vertex1)
                        {
                            witnessedBackward = DirectedGraphExtensions.FromData(witnessGraphEnumerator.Data0);
                            witnessedForward = DirectedGraphExtensions.FromData(witnessGraphEnumerator.Data1);
                            break;
                        }
                    }
                }

                // check forward.
                var hasUpdates = false;
                if (shortcutForward > 0 && shortcutForward < float.MaxValue &&
                    shortcutForward - FastHierarchyBuilder<float>.E > witnessedForward)
                {
                    shortcut.Forward = weightHandler.Infinite;
                    hasUpdates = true;
                }
                if (shortcutBackward > 0 && shortcutBackward < float.MaxValue &&
                    shortcutBackward - FastHierarchyBuilder<float>.E > witnessedBackward)
                {
                    shortcut.Backward = weightHandler.Infinite;
                    hasUpdates = true;
                }

                if (hasUpdates)
                {
                    if (toUpdate == null)
                    {
                        toUpdate = new List<Tuple<OriginalEdge, Shortcut<T>>>();
                    }
                    toUpdate.Add(new Tuple<OriginalEdge, Shortcut<T>>(edge, shortcut));
                }
            }         

            if (toUpdate != null)
            {
                foreach (var update in toUpdate)
                {
                    vertexInfo.Shortcuts[update.Item1] = update.Item2;
                }
                return true;
            }   
            return false;
        }
    }
}