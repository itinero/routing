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

using System;
using System.Collections.Generic;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Restrictions;
using Itinero.Graphs;
using Itinero.Algorithms.Default;
using Itinero.Profiles;

namespace Itinero.Algorithms.Contracted.EdgeBased.Witness
{
    /// <summary>
    /// A dykstra-based witness calculator.
    /// </summary>
    public class DykstraWitnessCalculator : IWitnessCalculator
    {
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly BinaryHeap<EdgePath> _heap;
        private readonly Graph _graph;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(Graph graph, Func<ushort, Factor> getFactor, Func<uint, IEnumerable<uint[]>> getRestrictions)
        {
            _getRestrictions = getRestrictions;
            _getFactor = getFactor;
            _graph = graph;

            _heap = new BinaryHeap<EdgePath>();
        }
        
        private Dictionary<EdgePath, LinkedRestriction> _edgeRestrictions;
        private Dictionary<long, EdgePath> _visits;

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public bool Calculate(uint[] sourceVertices, uint[] targetVertices, uint vertexToSkip, float maxWeight)
        {
            _edgeRestrictions = new Dictionary<EdgePath, LinkedRestriction>();
            _visits = new Dictionary<long, EdgePath>();
            _heap.Clear();

            // initialize.
            this.QueueSource(sourceVertices);

            // prepare target.
            var targetWeight = _graph.GetWeight(_getFactor, targetVertices);
            if (targetWeight > maxWeight)
            {
                return false;
            }

            // step until no longer possible.
            var enumerator = _graph.GetEdgeEnumerator();
            EdgePath current = null;
            while (true)
            {
                current = null;
                if (_heap.Count > 0)
                {
                    current = _heap.Pop();
                }

                if (current == null)
                {
                    break;
                }

                // move to the current edge and after to it's target.
                enumerator.MoveToTargetVertex(current.DirectedEdge);
                var currentVertex = enumerator.From;
                if (currentVertex == vertexToSkip)
                {
                    continue;
                }

                LinkedRestriction restrictions;
                if (_edgeRestrictions.TryGetValue(current, out restrictions))
                {
                    _edgeRestrictions.Remove(current);
                }
                var targetVertexRestriction = _getRestrictions(currentVertex);
                if (targetVertexRestriction != null)
                {
                    foreach (var restriction in targetVertexRestriction)
                    {
                        if (restriction != null &&
                            restriction.Length > 0)
                        {
                            if (restriction.Length == 1)
                            { // a simple restriction, restricted vertex, no need to check outgoing edges.
                                return true;
                            }
                            else
                            { // a complex restriction.
                                restrictions = new LinkedRestriction()
                                {
                                    Restriction = restriction,
                                    Next = restrictions
                                };
                            }
                        }
                    }
                }
                
                if (currentVertex == targetVertices[targetVertices.Length - 1])
                { // check if we arrived without restriction.
                    if (restrictions == null)
                    { // unrestricted path was found.
                        if (current.Weight < maxWeight - targetWeight)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!restrictions.Restricts(targetVertices))
                        {
                            if (current.Weight < maxWeight - targetWeight)
                            { // no sequence, no restriction possible.
                                return true;
                            }
                        }
                    }
                }

                while (enumerator.MoveNext())
                {
                    var edge = enumerator;
                    var edgeId = enumerator.Id;

                    // impossible to do a don't go back here.

                    // don't double-visit.
                    if (_visits.ContainsKey(edgeId))
                    {
                        continue;
                    }

                    // get details about edge.
                    float distance;
                    ushort profileId;
                    Data.Edges.EdgeDataSerializer.Deserialize(enumerator.Data0,
                        out distance, out profileId);
                    var factor = _getFactor(profileId);
                    if (factor.Direction == 2 || factor.Value == 0)
                    { // only backwards.
                        continue;
                    }
                    var weight = factor.Value * distance;
                    if (restrictions != null)
                    { // there are restrictions registered at this edge, check the first sequence of this edge.
                        if (restrictions.Restricts(currentVertex, enumerator.To))
                        { // this movement is restricted.
                            continue;
                        }
                    }

                    // check for u-turns.
                    if (factor.Direction == 0)
                    { // only u-turns on bidirectional edges.
                        if (current.DirectedEdge == -edge.IdDirected())
                        { // this is a u-turn.
                            continue;
                        }
                    }

                    // queue path.
                    var neighbourPath = new EdgePath(enumerator.IdDirected(), weight + current.Weight, current);
                    if (neighbourPath.Weight > maxWeight - targetWeight)
                    { // exceeded maximum weight, just don't queue and stop here.
                        continue;
                    }
                    _heap.Push(neighbourPath, neighbourPath.Weight);

                    // get restrictions at neighbour.
                    var neighbourRestrictions = _getRestrictions(edge.To);
                    if (neighbourRestrictions != null)
                    { // check this restriction against the reverse sequence to the neighbour.
                        var sequenceToNeighbour = new List<uint>();
                        sequenceToNeighbour.Add(currentVertex);
                        sequenceToNeighbour.Add(enumerator.To);
                        LinkedRestriction newRestrictions = null;
                        foreach (var neighbourRestriction in neighbourRestrictions)
                        {
                            var shrink = neighbourRestriction.ShrinkFor(sequenceToNeighbour);
                            if (shrink.Length > 1)
                            {
                                newRestrictions = new LinkedRestriction()
                                {
                                    Next = newRestrictions,
                                    Restriction = shrink
                                };
                            }
                        }
                        _edgeRestrictions[neighbourPath] = newRestrictions;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Queues all restrictions that start right after the source-path as if we got this path via routing.
        /// </summary>
        private void QueueSource(uint[] sourceVertices)
        {
            var enumerator = _graph.GetEdgeEnumerator();
            var source = sourceVertices[sourceVertices.Length - 1];
            var sourceWeight = enumerator.GetWeight(_getFactor, sourceVertices);
            if (!enumerator.MoveTo(source))
            {
                return;
            }
            var neighbourRestrictions = _getRestrictions(source);
            while(enumerator.MoveNext())
            {
                // get weight, check direction.
                float distance;
                ushort profileId;
                Data.Edges.EdgeDataSerializer.Deserialize(enumerator.Data0,
                    out distance, out profileId);
                var factor = _getFactor(profileId);
                if (factor.Direction == 2 || factor.Value == 0)
                { // only backwards.
                    continue;
                }
                var weight = factor.Value * distance;

                var neighbourPath = new EdgePath(enumerator.IdDirected(), sourceWeight + weight, new EdgePath(Constants.NO_EDGE));

                // check if restricted.
                LinkedRestriction newRestrictions = null;
                if (neighbourRestrictions != null)
                {
                    var sequenceToNeighbour = new List<uint>(sourceVertices);
                    sequenceToNeighbour.Add(enumerator.To);
                    var restricted = false;
                    foreach (var neighbourRestriction in neighbourRestrictions)
                    {
                        var shrunk = neighbourRestriction.ShrinkForPart(sequenceToNeighbour);
                        if (shrunk.Length > 0)
                        {
                            if (shrunk.Length == 1) // is restricted right here.
                            {
                                restricted = true;
                                break;
                            }
                            else
                            {
                                newRestrictions = new LinkedRestriction()
                                {
                                    Next = newRestrictions,
                                    Restriction = shrunk
                                };
                            }
                        }
                    }
                    if (restricted)
                    { // this edge is impossible, is restricted, don't consider it.
                        continue;
                    }
                    if (newRestrictions != null)
                    {
                        _edgeRestrictions[neighbourPath] = newRestrictions;
                    }
                }
                _heap.Push(neighbourPath, neighbourPath.Weight);
            }
        }

        /// <summary>
        /// A linked restriction.
        /// </summary>
        private class LinkedRestriction
        {
            /// <summary>
            /// Gets the restriction.
            /// </summary>
            public uint[] Restriction { get; set; }

            /// <summary>
            /// Gets the next linked restriction.
            /// </summary>
            public LinkedRestriction Next { get; set; }

            /// <summary>
            /// Returns true if any restriction exists with a given length.
            /// </summary>
            public bool ContainsAnyLength(int length)
            {
                var cur = this;
                while (cur != null)
                {
                    if (cur.Restriction != null &&
                        cur.Restriction.Length == length)
                    {
                        return true;
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns true if this restriction or any restriction following this restrictions restricts the given sequence.
            /// </summary>
            public bool Restricts(uint[] sequence)
            {
                var restricts = false;
                if (sequence.Length <= this.Restriction.Length)
                {
                    restricts = true;
                    for (var i = 0; i < Restriction.Length; i++)
                    {
                        if (Restriction[i] != sequence[i])
                        {
                            restricts = false;
                        }
                    }
                }
                if (restricts)
                {
                    return true;
                }
                if (this.Next != null)
                {
                    return this.Next.Restricts(sequence);
                }
                return false;
            }

            /// <summary>
            /// Returns true if this restriction or any restriction following this restrictions restricts the given sequence.
            /// </summary>
            public bool Restricts(uint vertex1, uint vertex2)
            {
                var restricts = false;
                if (2 < this.Restriction.Length)
                {
                    if (this.Restriction.Length == 1)
                    {
                        restricts = this.Restriction[0] == vertex1;
                    }
                    else if (this.Restriction.Length == 2)
                    {
                        restricts = this.Restriction[0] == vertex1 &&
                            this.Restriction[1] == vertex2;
                    }
                }
                if (restricts)
                {
                    return true;
                }
                if (this.Next != null)
                {
                    return this.Next.Restricts(vertex1, vertex2);
                }
                return false;
            }
        }
    }
}