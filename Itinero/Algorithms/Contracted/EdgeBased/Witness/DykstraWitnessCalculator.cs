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
using Itinero.Graphs.Directed;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Restrictions;

namespace Itinero.Algorithms.Contracted.EdgeBased.Witness
{
    /// <summary>
    /// A dykstra-based witness calculator.
    /// </summary>
    public class DykstraWitnessCalculator : IWitnessCalculator
    {
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;
        private readonly BinaryHeap<EdgePath> _heap;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(Func<uint, IEnumerable<uint[]>> getRestrictions)
        {
            _getRestrictions = getRestrictions;

            _heap = new BinaryHeap<EdgePath>();
        }
        
        private Dictionary<EdgePath, LinkedRestriction> _edgeRestrictions;
        private Dictionary<long, EdgePath> _visits;

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public bool Calculate(DirectedDynamicGraph graph, uint[] sourceVertices, uint[] targetVertices, uint vertexToSkip, float maxWeight)
        {
            _edgeRestrictions = new Dictionary<EdgePath, LinkedRestriction>();
            _visits = new Dictionary<long, EdgePath>();
            _heap.Clear();

            // initialize.
            this.QueueSource(graph, sourceVertices);

            // prepare target.
            var targetWeight = graph.GetOriginalWeight(targetVertices);
            if (targetWeight > maxWeight)
            {
                return false;
            }

            // step until no longer possible.
            var enumerator = graph.GetEdgeEnumerator();
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
                if (!enumerator.MoveToEdge(current.Edge))
                {
                    continue;
                }
                var currentVertex = enumerator.Neighbour;
                if (currentVertex == vertexToSkip)
                {
                    continue;
                }
                var contractedId = enumerator.GetContracted();

                if (!enumerator.MoveTo(currentVertex))
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
                        var sequenceToTarget = this.GetPathAtTarget(graph, current, sourceVertices);
                        if (sequenceToTarget == null ||
                            sequenceToTarget.Count == 0)
                        { // no sequence, no restriction possible.
                            if (current.Weight < maxWeight - targetWeight)
                            {
                                return true;
                            }
                        }
                        else
                        {
                            sequenceToTarget.RemoveAt(sequenceToTarget.Count - 1);
                            sequenceToTarget.AddRange(targetVertices);

                            if (!restrictions.Restricts(sequenceToTarget.ToArray()))
                            {
                                if (current.Weight < maxWeight - targetWeight)
                                { // no sequence, no restriction possible.
                                    return true;
                                }
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
                    float weight;
                    bool? direction;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0,
                        out weight, out direction);
                    if (direction != null && !direction.Value)
                    { // only backwards.
                        continue;
                    }
                    uint[] sequence = null;
                    if (restrictions != null)
                    { // there are restrictions registered at this edge, check the first sequence of this edge.
                        sequence = edge.GetSequence1WithSource(currentVertex);
                        if (restrictions.Restricts(sequence))
                        { // this movement is restricted.
                            continue;
                        }
                    }

                    // check for u-turns.
                    if (direction == null)
                    { // only u-turns on bidirectional edges.
                        if (current.SourceVertex == edge.Neighbour &&
                            contractedId == edge.GetContracted())
                        { // same vertex again and shortcut for the same vertex of both original, this is a u-turn.
                            continue;
                        }
                    }


                    // queue path.
                    var neighbourPath = new EdgePath(currentVertex, enumerator.Id, weight + current.Weight, current);
                    if (neighbourPath.Weight > maxWeight - targetWeight)
                    { // exceeded maximum weight, just don't queue and stop here.
                        continue;
                    }
                    _heap.Push(neighbourPath, neighbourPath.Weight);

                    // get restrictions at neighbour.
                    var neighbourRestrictions = _getRestrictions(edge.Neighbour);
                    if (neighbourRestrictions != null)
                    { // check this restriction against the reverse sequence to the neighbour.
                        var sequenceToNeighbour = this.GetPathAtTarget(graph, neighbourPath, sourceVertices);
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
        private void QueueSource(DirectedDynamicGraph graph, uint[] sourceVertices)
        {
            var enumerator = graph.GetEdgeEnumerator();
            var source = sourceVertices[sourceVertices.Length - 1];
            var sourceWeight = graph.GetOriginalWeight(sourceVertices);
            if (!enumerator.MoveTo(source))
            {
                return;
            }
            var neighbourRestrictions = _getRestrictions(source);
            while(enumerator.MoveNext())
            {
                // get weight, check direction.
                float weight;
                bool? direction;
                ContractedEdgeDataSerializer.Deserialize(enumerator.Data0,
                    out weight, out direction);
                if (direction != null && !direction.Value)
                { // only backwards.
                    continue;
                }

                var neighbourPath = new EdgePath(source, enumerator.Id, weight + sourceWeight, new EdgePath());

                // check if restricted.
                LinkedRestriction newRestrictions = null;
                if (neighbourRestrictions != null)
                {
                    var sequenceToNeighbour = new List<uint>(sourceVertices);
                    sequenceToNeighbour.Add(enumerator.Neighbour);
                    var restricted = false;
                    foreach (var neighbourRestriction in neighbourRestrictions)
                    {
                        var shrunk = neighbourRestriction.ShrinkForPart(sequenceToNeighbour);
                        if (shrunk.Length <= 1)
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
        /// Gets the last couple of relevant vertices for the given path.
        /// </summary>
        private List<uint> GetPathAtTarget(DirectedDynamicGraph graph, EdgePath path, uint[] sourceVertices)
        {
            var enumerator = graph.GetEdgeEnumerator();
            if(!enumerator.MoveToEdge(path.Edge))
            {
                return null;
            }
            if (enumerator.IsOriginal())
            {
                if (path.From != null &&
                    path.From.Edge != Constants.NO_EDGE)
                {
                    var neighbour = enumerator.Neighbour;
                    var from = this.GetPathAtTarget(graph, path.From, sourceVertices);
                    from.Add(enumerator.Neighbour);
                    return from;
                }
                var sequence = new List<uint>(sourceVertices);
                sequence.Add(enumerator.Neighbour);
                return sequence;
            }
            else
            {
                var dynamicData = enumerator.DynamicData;
                if (dynamicData == null || dynamicData.Length < 1)
                {
                    throw new ArgumentException("The given edge is not part of a contracted edge-based graph.");
                }
                if (dynamicData.Length < 2)
                {
                    return new List<uint>();
                }
                var size = dynamicData[2];
                var sequence = new List<uint>();
                for (var i = 0; i < dynamicData.Length - 2 - size; i++)
                {
                    sequence.Add(dynamicData[size + 2]);
                }
                sequence.Add(enumerator.Neighbour);
                return sequence;
            }
        }
        
        private class EdgePath
        {
            private readonly uint _sourceVertex;
            private readonly uint _edge;
            private readonly float _weight;
            private readonly EdgePath _from;
            
            public EdgePath()
            {
                _sourceVertex = Constants.NO_VERTEX;
                _edge = Constants.NO_EDGE;
                _weight = 0;

                _from = null;
            }

            public EdgePath(uint sourceVertex, uint edge, float weight, EdgePath from)
            {
                _sourceVertex = sourceVertex;
                _edge = edge;
                _weight = weight;

                _from = from;
            }

            public uint SourceVertex
            {
                get
                {
                    return _sourceVertex;
                }
            }

            public uint Edge
            {
                get
                {
                    return _edge;
                }
            }

            public float Weight
            {
                get
                {
                    return _weight;
                }
            }

            public EdgePath From
            {
                get
                {
                    return _from;
                }
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
                if (sequence.Length < this.Restriction.Length)
                {
                    restricts = true;
                    for (var i = 0; i < Restriction.Length; i++)
                    {
                        if (Restriction[i] != sequence.Length)
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
        }
    }
}