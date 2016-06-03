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
using Itinero.Graphs.Directed;
using Itinero.Data.Contracted.Edges;

namespace Itinero.Algorithms.Contracted.EdgeBased.Witness
{
    /// <summary>
    /// A dykstra-based witness calculator.
    /// </summary>
    public class DykstraWitnessCalculator : IWitnessCalculator
    {
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(Func<uint, IEnumerable<uint[]>> getRestrictions)
        {
            _getRestrictions = getRestrictions;
            _heap = new BinaryHeap<EdgePath>();
        }

        private BinaryHeap<EdgePath> _heap;
        private Dictionary<EdgePath, LinkedRestriction> _edgeRestrictions;
        private Dictionary<long, EdgePath> _visits;

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public EdgePath Calculate(DirectedDynamicGraph graph, uint[] sourceVertices, uint[] targetVertices)
        {
            var enumerator = graph.GetEdgeEnumerator();
            EdgePath best = null;

            _edgeRestrictions = new Dictionary<EdgePath, LinkedRestriction>();
            _visits = new Dictionary<long, EdgePath>();
            _heap.Clear();

            // initialize.
            var current = enumerator.BuildPath(sourceVertices);
            _heap.Push(current, current.Weight);

            // prepare target.
            var targetPath = enumerator.BuildPath(targetVertices, true, true);

            // step until no longer possible.
            while (true)
            {
                // pop next edge.
                current = null;
                if (_heap.Count > 0)
                {
                    current = _heap.Pop();
                }

                if (current == null)
                {
                    break;
                }

                // check if already visited.
                if (current.Edge != Constants.NO_EDGE)
                {
                    if (_visits.ContainsKey(current.Edge))
                    {
                        continue;
                    }
                    _visits.Add(current.Edge, current);
                }

                // check for the vertex to skip.
                var fromVertex = Constants.NO_VERTEX;
                var fromEdge = (long)Constants.NO_EDGE;
                if (current.From != null)
                {
                    fromVertex = current.From.Vertex;
                    fromEdge = current.From.Edge;
                }

                // move to edge and get end sequence.
                var sequence2 = new uint[] { fromVertex };
                var currentOriginal = true;
                if (current.Edge != Constants.NO_EDGE)
                {
                    enumerator.MoveToEdge(current.Edge);
                    if (!enumerator.IsOriginal())
                    {
                        currentOriginal = false;
                        sequence2 = enumerator.GetSequence2();
                    }
                }

                //    //LinkedRestriction restrictions;
                //    //if (_edgeRestrictions.TryGetValue(current, out restrictions))
                //    //{
                //    //    _edgeRestrictions.Remove(current);
                //    //}
                //    //var targetVertexRestriction = _getRestrictions(currentVertex);
                //    //if (targetVertexRestriction != null)
                //    //{
                //    //    foreach (var restriction in targetVertexRestriction)
                //    //    {
                //    //        if (restriction != null &&
                //    //            restriction.Length > 0)
                //    //        {
                //    //            if (restriction.Length == 1)
                //    //            { // a simple restriction, restricted vertex, no need to check outgoing edges.
                //    //                return true;
                //    //            }
                //    //            else
                //    //            { // a complex restriction.
                //    //                restrictions = new LinkedRestriction()
                //    //                {
                //    //                    Restriction = restriction,
                //    //                    Next = restrictions
                //    //                };
                //    //            }
                //    //        }
                //    //    }
                //    //}

                var target = -1;
                for (var i = 0; i < targetVertices.Length; i++)
                {
                    if (targetVertices[i] == current.Vertex)
                    {
                        target = i;
                        break;
                    }
                }
                if (target != -1)
                {
                    var ok = true;
                    for (var j = 0; j < sequence2.Length; j++)
                    {
                        if (sequence2[j] != targetVertices[j])
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok)
                    {
                        var shortendedTargetPath = targetPath;
                        if (target != 0)
                        {
                            shortendedTargetPath = enumerator.BuildPath(targetVertices.SubArray(target, targetVertices.Length - target),
                                true, true);
                        }

                        if (best == null || current.Weight + shortendedTargetPath.Weight < best.Weight)
                        { // TODO: check restrictions!
                            best = current.Append(shortendedTargetPath);
                        }
                    }
                }

                // move to the current edge's target.
                enumerator.MoveTo(current.Vertex);
                while (enumerator.MoveNext())
                {
                    var edge = enumerator;
                    var edgeId = enumerator.IdDirected();

                    if (_visits.ContainsKey(edgeId))
                    { // this edge was already visited.
                        continue;
                    }

                    // don't double-visit.
                    if (_visits.ContainsKey(edgeId))
                    {
                        continue;
                    }

                    // get edge-details.
                    float weight;
                    bool? direction;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0, out weight, out direction);

                    // check for u-turns.
                    if (direction == null && current.Edge != Constants.NO_EDGE)
                    { // u-turns only possible when direction is bidirectional.
                        if (enumerator.IsOriginal())
                        {
                            if (sequence2[sequence2.Length - 1] == enumerator.Neighbour)
                            { // this is a u-turn.
                                continue;
                            }
                        }
                        else
                        {
                            var sequence1 = enumerator.GetSequence1();
                            if (sequence2[sequence2.Length - 1] == sequence1[0])
                            { // this is a u-turn.
                                continue;
                            }
                        }
                    }

                    // queue path.
                    var neighbourPath = new EdgePath(enumerator.Neighbour, weight + current.Weight, enumerator.IdDirected(), current);
                    if (best != null && neighbourPath.Weight > best.Weight)
                    { // exceeded maximum weight, just don't queue and stop here.
                        continue;
                    }
                    _heap.Push(neighbourPath, neighbourPath.Weight);

                    //        //// get restrictions at neighbour.
                    //        //var neighbourRestrictions = _getRestrictions(edge.To);
                    //        //if (neighbourRestrictions != null)
                    //        //{ // check this restriction against the reverse sequence to the neighbour.
                    //        //    var sequenceToNeighbour = new List<uint>();
                    //        //    sequenceToNeighbour.Add(currentVertex);
                    //        //    sequenceToNeighbour.Add(enumerator.To);
                    //        //    LinkedRestriction newRestrictions = null;
                    //        //    foreach (var neighbourRestriction in neighbourRestrictions)
                    //        //    {
                    //        //        var shrink = neighbourRestriction.ShrinkFor(sequenceToNeighbour);
                    //        //        if (shrink.Length > 1)
                    //        //        {
                    //        //            newRestrictions = new LinkedRestriction()
                    //        //            {
                    //        //                Next = newRestrictions,
                    //        //                Restriction = shrink
                    //        //            };
                    //        //        }
                    //        //    }
                    //        //    _edgeRestrictions[neighbourPath] = newRestrictions;
                    //        //}
                    //    }
                }
            }

            return best;
        }

        /// <summary>
        /// Queues all restrictions that start right after the source-path as if we got this path via routing.
        /// </summary>
        private void QueueSource(DirectedDynamicGraph graph, uint[] sourceVertices, uint vertexToSkip, float maxWeight)
        {
            //var enumerator = graph.GetEdgeEnumerator();
            //var source = sourceVertices[sourceVertices.Length - 1];
            //var sourceWeight = enumerator.GetWeight(sourceVertices);
            //if (!enumerator.MoveTo(source))
            //{
            //    return;
            //}
            //var neighbourRestrictions = _getRestrictions(source);
            //while(enumerator.MoveNext())
            //{
            //    // get weight, check direction.
            //    float distance;
            //    ushort profileId;
            //    Data.Edges.EdgeDataSerializer.Deserialize(enumerator.Data0,
            //        out distance, out profileId);
            //    var factor = _getFactor(profileId);
            //    if ((enumerator.DataInverted && factor.Direction == 1) ||
            //        (!enumerator.DataInverted && factor.Direction == 2) ||
            //        factor.Value == 0)
            //    { // edge is only backwards or factor is zero.
            //        continue;
            //    }

            //    var weight = factor.Value * distance;

            //    if (sourceWeight + weight > maxWeight)
            //    {
            //        continue;
            //    }

            //    if (enumerator.To == vertexToSkip)
            //    {
            //        continue;
            //    }

            //    var neighbourPath = new DirectedEdgePath(enumerator.IdDirected(), sourceWeight + weight, new DirectedEdgePath(Constants.NO_EDGE));

            //    //// check if restricted.
            //    //LinkedRestriction newRestrictions = null;
            //    //if (neighbourRestrictions != null)
            //    //{
            //    //    var sequenceToNeighbour = new List<uint>(sourceVertices);
            //    //    sequenceToNeighbour.Add(enumerator.To);
            //    //    var restricted = false;
            //    //    foreach (var neighbourRestriction in neighbourRestrictions)
            //    //    {
            //    //        var shrunk = neighbourRestriction.ShrinkForPart(sequenceToNeighbour);
            //    //        if (shrunk.Length > 0)
            //    //        {
            //    //            if (shrunk.Length == 1) // is restricted right here.
            //    //            {
            //    //                restricted = true;
            //    //                break;
            //    //            }
            //    //            else
            //    //            {
            //    //                newRestrictions = new LinkedRestriction()
            //    //                {
            //    //                    Next = newRestrictions,
            //    //                    Restriction = shrunk
            //    //                };
            //    //            }
            //    //        }
            //    //    }
            //    //    if (restricted)
            //    //    { // this edge is impossible, is restricted, don't consider it.
            //    //        continue;
            //    //    }
            //    //    if (newRestrictions != null)
            //    //    {
            //    //        _edgeRestrictions[neighbourPath] = newRestrictions;
            //    //    }
            //    //}
            //    _heap.Push(neighbourPath, neighbourPath.Weight);
            //}
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
                if (sequence.Length >= this.Restriction.Length)
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