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

using System.Collections.Generic;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs.Directed;
using Itinero.Data.Contracted.Edges;
using System;
using System.Linq;
using Itinero.Algorithms.Restrictions;

namespace Itinero.Algorithms.Contracted.EdgeBased.Witness
{
    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public class DykstraWitnessCalculator : IWitnessCalculator
    {
        private readonly BinaryHeap<SettledEdge> _heap;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit)
            : this(hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(int hopLimit, int maxSettles)
        {
            _hopLimit = hopLimit;

            _heap = new BinaryHeap<SettledEdge>();
            _maxSettles = maxSettles;
        }

        private int _hopLimit;
        private int _maxSettles;

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public void Calculate(DirectedDynamicGraph graph, Func<uint, IEnumerable<uint[]>> getRestrictions, uint source, List<uint> targets, List<float> weights,
            ref bool[] forwardWitness, ref bool[] backwardWitness, uint vertexToSkip)
        {
            if (_hopLimit == 1)
            {
                this.ExistsOneHop(graph, source, targets, weights, ref forwardWitness, ref backwardWitness);
                return;
            }

            // creates the settled list.
            var backwardSettled = new HashSet<EdgePath>();
            var forwardSettled = new HashSet<EdgePath>();
            var backwardTargets = new HashSet<uint>();
            var forwardTargets = new HashSet<uint>();
            float forwardMaxWeight = 0, backwardMaxWeight = 0;
            for (int idx = 0; idx < weights.Count; idx++)
            {
                if (!forwardWitness[idx])
                {
                    forwardTargets.Add(targets[idx]);
                    if (forwardMaxWeight < weights[idx])
                    {
                        forwardMaxWeight = weights[idx];
                    }
                }
                if (!backwardWitness[idx])
                {
                    backwardTargets.Add(targets[idx]);
                    if (backwardMaxWeight < weights[idx])
                    {
                        backwardMaxWeight = weights[idx];
                    }
                }
            }
            if (forwardMaxWeight == 0 && backwardMaxWeight == 0)
            { // no need to search!
                return;
            }

            // creates the priorty queue.
            var forwardMinWeight = new Dictionary<EdgePath, float>();
            var backwardMinWeight = new Dictionary<EdgePath, float>();
            _heap.Clear();
            _heap.Push(new SettledEdge(new EdgePath(source), 0, forwardMaxWeight > 0, backwardMaxWeight > 0), 0);

            // keep looping until the queue is empty or the target is found!
            var edgeEnumerator = graph.GetEdgeEnumerator();
            while (_heap.Count > 0)
            { // pop the first customer.
                var current = _heap.Pop();
                if (current.Hops + 1 < _hopLimit)
                {
                    if (current.Edge.Vertex == vertexToSkip)
                    { // this is the vertex being contracted.
                        continue;
                    }
                    var forwardWasSettled = forwardSettled.Contains(current.Edge);
                    var backwardWasSettled = backwardSettled.Contains(current.Edge);
                    if (forwardWasSettled && backwardWasSettled)
                    { // both are already settled.
                        continue;
                    }

                    if (current.Forward)
                    { // this is a forward settle.
                        forwardSettled.Add(current.Edge);
                        forwardMinWeight.Remove(current.Edge);
                        if (forwardTargets.Contains(current.Edge.Vertex))
                        {
                            for (var i = 0; i < targets.Count; i++)
                            {
                                if (targets[i] == current.Edge.Vertex)
                                {
                                    forwardWitness[i] = current.Edge.Weight < weights[i];
                                    forwardTargets.Remove(current.Edge.Vertex);
                                }
                            }
                        }
                    }
                    if (current.Backward)
                    { // this is a backward settle.
                        backwardSettled.Add(current.Edge);
                        backwardMinWeight.Remove(current.Edge);
                        if (backwardTargets.Contains(current.Edge.Vertex))
                        {
                            for (var i = 0; i < targets.Count; i++)
                            {
                                if (targets[i] == current.Edge.Vertex)
                                {
                                    backwardWitness[i] = current.Edge.Weight < weights[i];
                                    backwardTargets.Remove(current.Edge.Vertex);
                                }
                            }
                        }
                    }

                    if (forwardTargets.Count == 0 &&
                        backwardTargets.Count == 0)
                    { // there is nothing left to check.
                        break;
                    }

                    if (forwardSettled.Count >= _maxSettles &&
                        backwardSettled.Count >= _maxSettles)
                    { // do not continue searching.
                        break;
                    }

                    var doForward = current.Forward && forwardTargets.Count > 0 && !forwardWasSettled;
                    var doBackward = current.Backward && backwardTargets.Count > 0 && !backwardWasSettled;
                    if (doForward || doBackward)
                    { // get the neighbours.

                        // check for a restriction and if need build the original sequence.
                        var restrictions = getRestrictions(current.Edge.Vertex);
                        var sequence = Constants.EMPTY; 
                        if (restrictions != null && restrictions.Any())
                        {
                            sequence = current.Edge.GetSequence(edgeEnumerator);
                        }

                        // move to the current vertex.
                        edgeEnumerator.MoveTo(current.Edge.Vertex);
                        while (edgeEnumerator.MoveNext())
                        { // move next.
                            var neighbour = edgeEnumerator.Neighbour;

                            float neighbourWeight;
                            bool? neighbourDirection;
                            ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                                out neighbourWeight, out neighbourDirection);
                            var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                            var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                            var neighbourPath = new EdgePath(neighbour, current.Edge.Weight + neighbourWeight, edgeEnumerator.IdDirected(), current.Edge);
                            
                            var totalNeighbourWeight = current.Edge.Weight + neighbourWeight;
                            var doNeighbourForward = doForward && neighbourCanMoveForward && totalNeighbourWeight < forwardMaxWeight &&
                                !forwardSettled.Contains(neighbourPath);
                            var doNeighbourBackward = doBackward && neighbourCanMoveBackward && totalNeighbourWeight < backwardMaxWeight &&
                                !backwardSettled.Contains(neighbourPath);
                            if (doNeighbourBackward || doNeighbourForward)
                            {
                                float existingWeight;
                                uint[] sequenceAlongNeighbour = null;

                                if ((doNeighbourBackward || doNeighbourForward) && sequence.Length > 0)
                                {
                                    if (edgeEnumerator.IsOriginal())
                                    {
                                        sequenceAlongNeighbour = sequence.Append(neighbour);
                                    }
                                    else
                                    {
                                        sequenceAlongNeighbour = sequence.Append(edgeEnumerator.GetSequence1());
                                    }
                                }

                                if (doNeighbourForward)
                                {
                                    if (sequence.Length == 0 || restrictions.IsSequenceAllowed(sequenceAlongNeighbour))
                                    { // restrictions ok.
                                        if (forwardMinWeight.TryGetValue(neighbourPath, out existingWeight))
                                        {
                                            if (existingWeight <= totalNeighbourWeight)
                                            {
                                                doNeighbourForward = false;
                                            }
                                            else
                                            {
                                                forwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                            }
                                        }
                                        else
                                        {
                                            forwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        doNeighbourForward = false;
                                    }
                                }
                                if (doNeighbourBackward)
                                {
                                    if (sequenceAlongNeighbour != null)
                                    {
                                        sequenceAlongNeighbour.Reverse();
                                    }
                                    if (sequence.Length == 0 || restrictions.IsSequenceAllowed(sequenceAlongNeighbour))
                                    { // restrictions ok.
                                        if (backwardMinWeight.TryGetValue(neighbourPath, out existingWeight))
                                        {
                                            if (existingWeight <= totalNeighbourWeight)
                                            {
                                                doNeighbourBackward = false;
                                            }
                                            else
                                            {
                                                backwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                            }
                                        }
                                        else
                                        {
                                            backwardMinWeight[neighbourPath] = totalNeighbourWeight;
                                        }
                                    }
                                    else
                                    {
                                        doNeighbourBackward = false;
                                    }
                                }

                                if (doNeighbourBackward || doNeighbourForward)
                                { // add to heap.
                                    var newSettle = new SettledEdge(neighbourPath, current.Hops + 1, doNeighbourForward, doNeighbourBackward);
                                    _heap.Push(newSettle, neighbourPath.Weight);
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates witness paths with just one hop.
        /// </summary>
        public void ExistsOneHop(DirectedDynamicGraph graph, uint source, List<uint> targets, List<float> weights,
            ref bool[] forwardExists, ref bool[] backwardExists)
        {
            var targetsToCalculate = new HashSet<uint>();
            var maxWeight = 0.0f;
            for (int idx = 0; idx < weights.Count; idx++)
            {
                if (!forwardExists[idx] || !backwardExists[idx])
                {
                    targetsToCalculate.Add(targets[idx]);
                    if (maxWeight < weights[idx])
                    {
                        maxWeight = weights[idx];
                    }
                }
            }

            if (targetsToCalculate.Count > 0)
            {
                var edgeEnumerator = graph.GetEdgeEnumerator(source);
                while (edgeEnumerator.MoveNext())
                {
                    var neighbour = edgeEnumerator.Neighbour;
                    if (targetsToCalculate.Contains(neighbour))
                    { // ok, this is a to-edge.
                        var index = targets.IndexOf(neighbour);
                        targetsToCalculate.Remove(neighbour);

                        float neighbourWeight;
                        bool? neighbourDirection;
                        ContractedEdgeDataSerializer.Deserialize(edgeEnumerator.Data0,
                            out neighbourWeight, out neighbourDirection);
                        var neighbourCanMoveForward = neighbourDirection == null || neighbourDirection.Value;
                        var neighbourCanMoveBackward = neighbourDirection == null || !neighbourDirection.Value;

                        if (neighbourCanMoveForward &&
                            neighbourWeight < weights[index])
                        {
                            forwardExists[index] = true;
                        }
                        if (neighbourCanMoveBackward &&
                            neighbourWeight < weights[index])
                        {
                            backwardExists[index] = true;
                        }

                        if (targetsToCalculate.Count == 0)
                        {
                            break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the hop limit.
        /// </summary>
        public int HopLimit
        {
            get
            {
                return _hopLimit;
            }
            set
            {
                _hopLimit = value;
            }
        }

        /// <summary>
        /// Gets or sets the max settles.
        /// </summary>
        public int MaxSettles
        {
            get
            {
                return _maxSettles;
            }
            set
            {
                _maxSettles = value;
            }
        }

        /// <summary>
        /// Represents a settled edge.
        /// </summary>
        private class SettledEdge
        {
            /// <summary>
            /// Creates a new settled edge.
            /// </summary>
            public SettledEdge(EdgePath edge, uint hops, bool forward, bool backward)
            {
                this.Edge = edge;
                this.Hops = hops;
                this.Forward = forward;
                this.Backward = backward;
            }

            /// <summary>
            /// The edge that was settled.
            /// </summary>
            public EdgePath Edge { get; set; }

            /// <summary>
            /// The hop-count of this vertex.
            /// </summary>
            public uint Hops { get; set; }

            /// <summary>
            /// Holds the forward flag.
            /// </summary>
            public bool Forward { get; set; }

            /// <summary>
            /// Holds the backward flag.
            /// </summary>
            public bool Backward { get; set; }
        }
    }
}