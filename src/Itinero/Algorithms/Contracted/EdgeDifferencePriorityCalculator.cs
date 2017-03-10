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
using Itinero.Algorithms.Contracted.Witness;
using Itinero.Graphs.Directed;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// A priority calculator.
    /// </summary>
    public class EdgeDifferencePriorityCalculator : IPriorityCalculator
    {
        private readonly DirectedMetaGraph _graph;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        private readonly IWitnessCalculator _witnessCalculator;

        /// <summary>
        /// Creates a new priority calculator.
        /// </summary>
        public EdgeDifferencePriorityCalculator(DirectedMetaGraph graph, IWitnessCalculator witnessCalculator)
        {
            _graph = graph;
            _witnessCalculator = witnessCalculator;
            _contractionCount = new Dictionary<uint, int>();
            _depth = new Dictionary<long, int>();

            this.DifferenceFactor = 1;
            this.DepthFactor = 2;
            this.ContractedFactor = 1;
        }

        /// <summary>
        /// Calculates the priority of the given vertex.
        /// </summary>
        public float Calculate(BitArray32 contractedFlags, uint vertex)
        {
            var removed = 0;
            var added = 0;
            
            // get and keep edges.
            var edges = new List<Edge>(_graph.Graph.GetEdgeEnumerator(vertex));

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < edges.Count)
            {
                var edgeEnumerator = _graph.GetEdgeEnumerator(edges[i].Neighbour);
                edgeEnumerator.Reset();
                while(edgeEnumerator.MoveNext())
                {
                    if(edgeEnumerator.Neighbour == vertex)
                    {
                        removed++;
                    }
                }

                if (contractedFlags[edges[i].Neighbour])
                { // neighbour was already contracted, remove 'downward' edge and exclude it.
                    edgeEnumerator.MoveTo(vertex);
                    edgeEnumerator.Reset();
                    while (edgeEnumerator.MoveNext())
                    {
                        if (edgeEnumerator.Neighbour == edges[i].Neighbour)
                        {
                            removed++;
                        }
                    }
                    edges.RemoveAt(i);
                }
                else
                { // move to next edge.
                    i++;
                }
            }

            // loop over all edge-pairs once.
            for (var j = 1; j < edges.Count; j++)
            {
                var edge1 = edges[j];

                float edge1Weight;
                bool? edge1Direction;
                Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge1.Data[0],
                    out edge1Weight, out edge1Direction);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                // figure out what witness paths to calculate.
                var forwardWitnesses = new bool[j];
                var backwardWitnesses = new bool[j];
                var targets = new List<uint>(j);
                var targetWeights = new List<float>(j);
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    float edge2Weight;
                    bool? edge2Direction;
                    Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge2.Data[0],
                        out edge2Weight, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    forwardWitnesses[k] = !(edge1CanMoveBackward && edge2CanMoveForward);
                    backwardWitnesses[k] = !(edge1CanMoveForward && edge2CanMoveBackward); 
                    targets.Add(edge2.Neighbour);
                    targetWeights.Add(edge1Weight + edge2Weight);
                }

                // calculate all witness paths.
                _witnessCalculator.Calculate(_graph.Graph, edge1.Neighbour, targets, targetWeights, 
                    ref forwardWitnesses, ref backwardWitnesses, vertex);

                // add contracted edges if needed.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    var removedLocal = 0;
                    var addedLocal = 0;
                    if (!forwardWitnesses[k] && !backwardWitnesses[k])
                    { // add bidirectional edge.
                        _graph.TryAddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            targetWeights[k], null, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                        _graph.TryAddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            targetWeights[k], null, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                    }
                    else if (!forwardWitnesses[k])
                    { // add forward edge.
                        _graph.TryAddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            targetWeights[k], true, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                        _graph.TryAddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            targetWeights[k], false, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                    }
                    else if (!backwardWitnesses[k])
                    { // add forward edge.
                        _graph.TryAddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            targetWeights[k], false, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                        _graph.TryAddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            targetWeights[k], true, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                    }
                }
            }

            var contracted = 0;
            _contractionCount.TryGetValue(vertex, out contracted);
            var depth = 0;
            _depth.TryGetValue(vertex, out depth);
            return this.DifferenceFactor * (added - removed) + (this.DepthFactor * depth) + 
                (this.ContractedFactor * contracted);
        }

        /// <summary>
        /// Gets or sets the difference factor.
        /// </summary>
        public int DifferenceFactor { get; set; }

        /// <summary>
        /// Gets or sets the depth factor.
        /// </summary>
        public int DepthFactor { get; set; }

        /// <summary>
        /// Gets or sets the contracted factor.
        /// </summary>
        public int ContractedFactor { get; set; }

        /// <summary>
        /// Notifies this calculator that the given vertex was contracted.
        /// </summary>
        public void NotifyContracted(uint vertex)
        {
            // removes the contractions count.
            _contractionCount.Remove(vertex);

            // loop over all neighbours.
            var edgeEnumerator = _graph.GetEdgeEnumerator(vertex);
            edgeEnumerator.Reset();
            while(edgeEnumerator.MoveNext())
            {
                var neighbour = edgeEnumerator.Neighbour;
                int count;
                if (!_contractionCount.TryGetValue(neighbour, out count))
                {
                    _contractionCount[neighbour] = 1;
                }
                else
                {
                    _contractionCount[neighbour] = count++;
                }
            }

            int vertexDepth = 0;
            _depth.TryGetValue(vertex, out vertexDepth);
            _depth.Remove(vertex);
            vertexDepth++;

            // store the depth.
            edgeEnumerator.Reset();
            while(edgeEnumerator.MoveNext())
            {
                var neighbour = edgeEnumerator.Neighbour;

                int depth = 0;
                _depth.TryGetValue(neighbour, out depth);
                if (vertexDepth >= depth)
                {
                    _depth[neighbour] = vertexDepth;
                }
            }
        }
    }
}