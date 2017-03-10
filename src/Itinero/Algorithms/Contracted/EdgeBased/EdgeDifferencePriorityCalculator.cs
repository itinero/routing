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
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// A priority calculator.
    /// </summary>
    public class EdgeDifferencePriorityCalculator<T> : IPriorityCalculator
        where T : struct
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        private readonly IWitnessCalculator<T> _witnessCalculator;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new priority calculator.
        /// </summary>
        public EdgeDifferencePriorityCalculator(DirectedDynamicGraph graph, WeightHandler<T> weightHandler, IWitnessCalculator<T> witnessCalculator)
        {
            _graph = graph;
            _witnessCalculator = witnessCalculator;
            _contractionCount = new Dictionary<uint, int>();
            _depth = new Dictionary<long, int>();
            _weightHandler = weightHandler;

            this.DifferenceFactor = 1;
            this.DepthFactor = 2;
            this.ContractedFactor = 1;
        }

        /// <summary>
        /// Calculates the priority of the given vertex.
        /// </summary>
        public float Calculate(BitArray32 contractedFlags, Func<uint, IEnumerable<uint[]>> getRestrictions, uint vertex)
        {
            var removed = 0;
            var added = 0;

            // get and keep edges.
            var edges = new List<DynamicEdge>(_graph.GetEdgeEnumerator(vertex));

            // check if this vertex has a potential restrictions.
            var restrictions = getRestrictions(vertex);
            var hasRestrictions = restrictions != null && restrictions.Any();

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < edges.Count)
            {
                var edgeEnumerator = _graph.GetEdgeEnumerator(edges[i].Neighbour);
                edgeEnumerator.Reset();
                while (edgeEnumerator.MoveNext())
                {
                    if (edgeEnumerator.Neighbour == vertex)
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
                
                bool? edge1Direction;
                var edge1Weight = _weightHandler.GetEdgeWeight(edge1, out edge1Direction);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                // figure out what witness paths to calculate.
                var forwardWitnesses = new EdgePath<T>[j];
                var backwardWitnesses = new EdgePath<T>[j];
                var targets = new List<uint>(j);
                var targetWeights = new List<T>(j);
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];
                    
                    bool? edge2Direction;
                    var edge2Weight = _weightHandler.GetEdgeWeight(edge2, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    if (!(edge1CanMoveBackward && edge2CanMoveForward))
                    {
                        forwardWitnesses[k] = new EdgePath<T>();
                    }
                    if (!(edge1CanMoveForward && edge2CanMoveBackward))
                    {
                        backwardWitnesses[k] = new EdgePath<T>();
                    }
                    targets.Add(edge2.Neighbour);
                    if (hasRestrictions)
                    { // weight can potentially be bigger.                        
                        targetWeights.Add(_weightHandler.Infinite);
                    }
                    else
                    { // weight can max be the sum of the two edges.
                        targetWeights.Add(_weightHandler.Add(edge1Weight, edge2Weight));
                    }
                }

                // calculate all witness paths.
                _witnessCalculator.Calculate(_graph, getRestrictions, edge1.Neighbour, targets, targetWeights,
                    ref forwardWitnesses, ref backwardWitnesses, Constants.NO_VERTEX);

                // add contracted edges if needed.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    var removedLocal = 0;
                    var addedLocal = 0;
                    if (forwardWitnesses[k].HasVertex(vertex) && backwardWitnesses[k].HasVertex(vertex))
                    { // add bidirectional edge.
                        _graph.TryAddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            _weightHandler.GetMetric(targetWeights[k]), null, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                        _graph.TryAddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            _weightHandler.GetMetric(targetWeights[k]), null, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                    }
                    else if (forwardWitnesses[k].HasVertex(vertex))
                    { // add forward edge.
                        _graph.TryAddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            _weightHandler.GetMetric(targetWeights[k]), true, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                        _graph.TryAddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            _weightHandler.GetMetric(targetWeights[k]), false, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                    }
                    else if (backwardWitnesses[k].HasVertex(vertex))
                    { // add forward edge.
                        _graph.TryAddOrUpdateEdge(edge1.Neighbour, edge2.Neighbour,
                            _weightHandler.GetMetric(targetWeights[k]), false, vertex, out addedLocal, out removedLocal);
                        added += addedLocal;
                        removed += removedLocal;
                        _graph.TryAddOrUpdateEdge(edge2.Neighbour, edge1.Neighbour,
                            _weightHandler.GetMetric(targetWeights[k]), true, vertex, out addedLocal, out removedLocal);
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
            while (edgeEnumerator.MoveNext())
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
            while (edgeEnumerator.MoveNext())
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

    /// <summary>
    /// A priority calculator.
    /// </summary>
    public sealed class EdgeDifferencePriorityCalculator : EdgeDifferencePriorityCalculator<float>
    {
        /// <summary>
        /// Creates a new priority calculator.
        /// </summary>
        public EdgeDifferencePriorityCalculator(DirectedDynamicGraph graph, IWitnessCalculator<float> witnessCalculator)
            : base (graph, new DefaultWeightHandler(null), witnessCalculator)
        {

        }
    }
}