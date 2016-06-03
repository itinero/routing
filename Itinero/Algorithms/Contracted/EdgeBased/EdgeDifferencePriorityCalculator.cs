// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.


using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Graphs.Directed;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// A priority calculator.
    /// </summary>
    public class EdgeDifferencePriorityCalculator : IPriorityCalculator
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly Dictionary<uint, int> _contractionCount;
        private readonly Dictionary<long, int> _depth;
        private readonly IWitnessCalculator _witnessCalculator;
        private const float E = 0.1f;

        /// <summary>
        /// Creates a new priority calculator.
        /// </summary>
        public EdgeDifferencePriorityCalculator(DirectedDynamicGraph graph, IWitnessCalculator witnessCalculator)
        {
            _graph = graph;
            _witnessCalculator = witnessCalculator;
            _contractionCount = new Dictionary<uint, int>();
            _depth = new Dictionary<long, int>();

            this.DifferenceFactor = 2;
            this.DepthFactor = 1;
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
            var edges = new List<DynamicEdge>(_graph.GetEdgeEnumerator(vertex));

            // remove 'downward' edge to vertex.
            removed = edges.Count;
            //var i = 0;
            //while (i < edges.Count)
            //{
            //    removed++;
            //    i
            //}

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
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    float edge2Weight;
                    bool? edge2Direction;
                    Data.Contracted.Edges.ContractedEdgeDataSerializer.Deserialize(edge2.Data[0],
                        out edge2Weight, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    // check if there are any restrictions restriction edge1->vertex->edge2
                    //var vertexRestrictions = (IEnumerable<uint[]>)null; // _getRestriction(vertex);
                    var sequence = new List<uint>();
                    //if (vertexRestrictions != null)
                    //{
                    //    // get sequence at vertex and check restrictions.
                    //    sequence.AddRange(edge1.GetSequence2().Reverse());
                    //    sequence.Add(vertex);
                    //    sequence.AddRange(edge2.GetSequence1());

                    //    if (!vertexRestrictions.IsSequenceAllowed(sequence))
                    //    { // there is restriction the prohibits this move.
                    //        continue;
                    //    }
                    //}

                    // figure out how much of the path needs to be saved at the source vertex.
                    uint[] sequence1 = null;
                    if (edge1.IsOriginal())
                    {
                        sequence1 = new uint[] { vertex };
                    }
                    else
                    {
                        sequence1 = edge1.GetSequence2();
                        sequence1.Reverse();
                    }
                    //if (restrictions1 != null)
                    //{
                    //    sequence = GetSequence(edge1, vertex, edge2);
                    //    var m = sequence.MatchAny(restrictions1);
                    //    if (m > 1)
                    //    { // there is a match that is non-trivial, make sure to add this.
                    //        sequence1 = new uint[m];
                    //        sequence.CopyTo(0, sequence1, 0, m);
                    //    }
                    //}
                    sequence1 = (new uint[] { edge1.Neighbour }).Append(sequence1);
                    uint[] sequence2 = null;
                    if (edge2.IsOriginal())
                    {
                        sequence2 = new uint[] { vertex };
                    }
                    else
                    {
                        sequence2 = edge2.GetSequence2();
                    }
                    sequence2 = sequence2.Append(edge2.Neighbour);
                    //if (restrictions2 != null)
                    //{
                    //    sequence = GetSequence(edge2, vertex, edge1);
                    //    var m = sequence.MatchAnyReverse(restrictions1);
                    //    if (m > 1)
                    //    { // there is a match that is non-trivial, make sure to add this.
                    //        sequence2 = new uint[m];
                    //        sequence.CopyTo(0, sequence2, 0, m);
                    //    }
                    //}

                    var forwardWeight = float.MaxValue;
                    if (edge1CanMoveBackward && edge2CanMoveForward)
                    {
                        var forwardPath = _witnessCalculator.Calculate(_graph, sequence1, sequence2);
                        if (forwardPath.HasVertex(vertex))
                        { // not a witness path.
                            forwardWeight = forwardPath.Weight;
                        }
                    }

                    var backwardWeight = float.MaxValue;
                    if (edge1CanMoveForward && edge2CanMoveBackward)
                    {
                        var backwardPath = _witnessCalculator.Calculate(_graph, sequence2.ReverseAndCopy(), sequence1.ReverseAndCopy());
                        if (backwardPath.HasVertex(vertex))
                        { // not a witness path.
                            backwardWeight = backwardPath.Weight;
                        }
                    }
                    
                    // build shortcut sequences.
                    if (sequence1 != null && sequence1.Length > 0)
                    {
                        sequence1 = sequence1.SubArray(1, sequence1.Length - 1);
                    }
                    if (sequence2 != null && sequence2.Length > 0)
                    {
                        sequence2 = sequence2.SubArray(0, sequence2.Length - 1);
                    }

                    if (forwardWeight == float.MaxValue && backwardWeight == float.MaxValue)
                    { // not shortcuts should be added.
                        continue;
                    }
                    if (System.Math.Abs(forwardWeight - backwardWeight) < E)
                    { // both forward and backward path have the same weight, add the as the same edge.
                        added += _graph.TryAddEdgeOrUpdate(edge1.Neighbour, edge2.Neighbour, forwardWeight, null, vertex, sequence1, sequence2);
                        added += _graph.TryAddEdgeOrUpdate(edge2.Neighbour, edge1.Neighbour, forwardWeight, null, vertex, sequence2.ReverseAndCopy(), sequence1.ReverseAndCopy());
                    }
                    else
                    {
                        if (forwardWeight != float.MaxValue)
                        { // there is a shortcut in forward direction.
                            added += _graph.TryAddEdgeOrUpdate(edge1.Neighbour, edge2.Neighbour, forwardWeight, true, vertex, sequence1, sequence2);
                            added += _graph.TryAddEdgeOrUpdate(edge2.Neighbour, edge1.Neighbour, forwardWeight, false, vertex, sequence2.ReverseAndCopy(), sequence1.ReverseAndCopy());
                        }
                        if (backwardWeight != float.MaxValue)
                        { // there is a shortcut in backward direction.
                            added += _graph.TryAddEdgeOrUpdate(edge1.Neighbour, edge2.Neighbour, backwardWeight, false, vertex, sequence1, sequence2);
                            added += _graph.TryAddEdgeOrUpdate(edge2.Neighbour, edge1.Neighbour, backwardWeight, true, vertex, sequence2.ReverseAndCopy(), sequence1.ReverseAndCopy());
                        }
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
}