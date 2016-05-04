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
using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs.Directed;
using Itinero.Logging;
using System.Collections.Generic;
using Itinero.Data.Contracted.Edges;
using Itinero.Algorithms.Restrictions;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder : AlgorithmBase
    {
        private readonly DirectedDynamicGraph _graph;
        private readonly IPriorityCalculator _priorityCalculator;
        private readonly IWitnessCalculator _witnessCalculator;
        private readonly static Logger _logger = Logger.Create("HierarchyBuilder");
        private readonly Func<uint, IEnumerable<uint[]>> _getRestriction;

        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedDynamicGraph graph, IPriorityCalculator priorityCalculator, IWitnessCalculator witnessCalculator,
            Func<uint, IEnumerable<uint[]>> getRestriction)
        {
            _graph = graph;
            _priorityCalculator = priorityCalculator;
            _witnessCalculator = witnessCalculator;
            _getRestriction = getRestriction;
        }
        
        private BinaryHeap<uint> _queue; // the vertex-queue.
        private BitArray32 _contractedFlags; // contains flags for contracted vertices.

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            _queue = new BinaryHeap<uint>((uint)_graph.VertexCount);
            _contractedFlags = new BitArray32(_graph.VertexCount);
            _missesQueue = new Queue<bool>();

            // remove all edges that have witness paths, meaning longer than the shortest path
            // between the two ending vertices.
            this.RemoveWitnessedEdges();

            // build queue.
            this.CalculateQueue();

            var next = this.SelectNext();
            var latestProgress = 0f;
            var current = 0;
            var total = _graph.VertexCount;
            while (next != null)
            {
                // contract...
                this.Contract(next.Value);

                // ... and select next.
                next = this.SelectNext();

                // calculate and log progress.
                var progress = (float)(System.Math.Floor(((double)current / (double)total) * 10000) / 100.0);
                if (progress < 99)
                {
                    progress = (float)(System.Math.Floor(((double)current / (double)total) * 100) / 1.0);
                }
                if (progress != latestProgress)
                {
                    latestProgress = progress;

                    int totaEdges = 0;
                    int totalUncontracted = 0;
                    int maxCardinality = 0;
                    var neighbourCount = new Dictionary<uint, int>();
                    for (uint v = 0; v < _graph.VertexCount; v++)
                    {
                        if (!_contractedFlags[v])
                        {
                            neighbourCount.Clear();
                            var edges = _graph.GetEdgeEnumerator(v);
                            if (edges != null)
                            {
                                var edgesCount = edges.Count();
                                totaEdges = edgesCount + totaEdges;
                                if (maxCardinality < edgesCount)
                                {
                                    maxCardinality = edgesCount;
                                }
                            }
                            totalUncontracted++;
                        }
                    }

                    var density = (double)totaEdges / (double)totalUncontracted;
                    _logger.Log(TraceEventType.Information, "Preprocessing... {0}% [{1}/{2}] {3}q #{4} max {5}",
                        progress, current, total, _queue.Count, density, maxCardinality);
                }
                current++;
            }
        }

        /// <summary>
        /// Calculates the entire queue.
        /// </summary>
        private void CalculateQueue()
        {
            _logger.Log(TraceEventType.Information, "Calculating queue...");

            _queue.Clear();
            for (uint v = 0; v < _graph.VertexCount; v++)
            {
                if (!_contractedFlags[v])
                {
                    _queue.Push(v, _priorityCalculator.Calculate(
                        _contractedFlags, v));
                }
            }
        }

        /// <summary>
        /// Remove all witnessed edges.
        /// </summary>
        private void RemoveWitnessedEdges()
        {
            _logger.Log(TraceEventType.Information, "Removing witnessed edges...");
            

        }

        private int _k = 20; // The amount of queue 'misses' to recalculated.
        private int _misses; // Holds a counter of all misses.
        private Queue<bool> _missesQueue; // Holds the misses queue.

        /// <summary>
        /// Select the next vertex to contract.
        /// </summary>
        /// <returns></returns>
        private uint? SelectNext()
        {
            // first check the first of the current queue.
            while (_queue.Count > 0)
            { // get the first vertex and check.
                var first = _queue.Peek();
                if (_contractedFlags[first])
                { // already contracted, priority was updated.
                    _queue.Pop();
                    continue;
                }
                var queuedPriority = _queue.PeekWeight();

                // the lazy updating part!
                // calculate priority
                var priority = _priorityCalculator.Calculate(_contractedFlags, first);
                if (priority != queuedPriority)
                { // a succesfull update.
                    _missesQueue.Enqueue(true);
                    _misses++;
                }
                else
                { // an unsuccessfull update.
                    _missesQueue.Enqueue(false);
                }
                if (_missesQueue.Count > _k)
                { // dequeue and update the misses.
                    if (_missesQueue.Dequeue())
                    {
                        _misses--;
                    }
                }

                // if the misses are _k
                if (_misses == _k)
                { // recalculation.
                    this.CalculateQueue();

                    // clear misses.
                    _missesQueue.Clear();
                    _misses = 0;
                }
                else
                { // no recalculation.
                    if (priority != queuedPriority)
                    { // re-enqueue.
                        _queue.Pop();
                        _queue.Push(first, priority);
                    }
                    else
                    { // try to select another.
                        return _queue.Pop();
                    }
                }
            }
            return null; // all nodes have been contracted.
        }

        /// <summary>
        /// Contracts the given vertex.
        /// </summary>
        private void Contract(uint vertex)
        {
            // get and keep edges.
            var edges = new List<DynamicEdge>(_graph.GetEdgeEnumerator(vertex));

            // remove 'downward' edge to vertex.
            var i = 0;
            while (i < edges.Count)
            {
                _graph.RemoveEdge(edges[i].Neighbour, vertex);

                if (_contractedFlags[edges[i].Neighbour])
                { // neighbour was already contracted, remove 'downward' edge and exclude it.
                    _graph.RemoveEdge(vertex, edges[i].Neighbour);
                    edges.RemoveAt(i);
                }
                else
                { // move to next edge.
                    i++;
                }
            }
            
            // loop over all edge-pairs.
            for (var f = 0; f < edges.Count; f++)
            {
                var edge1 = edges[f];
                var restrictions1 = _getRestriction(edge1.Neighbour);

                float edge1Weight;
                bool? edge1Direction;
                ContractedEdgeDataSerializer.Deserialize(edge1.Data[0],
                    out edge1Weight, out edge1Direction);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                if (!edge1CanMoveForward)
                {
                    continue;
                }

                // add contracted edges if needed.
                for (var t = 0; t < edges.Count; t++)
                {
                    var edge2 = edges[t];

                    if (edge1.Neighbour == edge2.Neighbour)
                    { // do not try to add a shortcut between identical vertices.
                        continue;
                    }
                    var restrictions2 = _getRestriction(edge2.Neighbour);

                    float edge2Weight;
                    bool? edge2Direction;
                    ContractedEdgeDataSerializer.Deserialize(edge2.Data[0],
                        out edge2Weight, out edge2Direction);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    if (!edge2CanMoveBackward)
                    {
                        continue;
                    }
                    
                    // check if there are any restrictions restriction edge1->vertex->edge2
                    var vertexRestrictions = _getRestriction(vertex);
                    var sequence = new List<uint>();
                    if (vertexRestrictions != null)
                    {
                        // get sequence at vertex and check restrictions.
                        sequence.AddRange(edge1.GetSequence().Reverse());
                        sequence.Add(vertex);
                        sequence.AddRange(edge2.GetSequence());

                        if (vertexRestrictions.IsSequenceAllowed(sequence))
                        { // there is restriction the prohibits this move.
                            continue;
                        }
                    }

                    // figure out how much of the path needs to be saved at the source vertex.
                    uint[] sequence1 = null;
                    if (restrictions1 != null)
                    {
                        sequence = GetSequence(edge1, vertex, edge2);
                        var m = sequence.MatchAny(restrictions1);
                        if (m > 1)
                        { // there is a match that is non-trivial, make sure to add this.
                            sequence1 = new uint[m];
                            sequence.CopyTo(0, sequence1, 0, m);
                        }
                    }
                    uint[] sequence2 = null;
                    if (restrictions2 != null)
                    {
                        sequence = GetReverseSequence(edge1, vertex, edge2);
                        var m = sequence.MatchAnyReverse(restrictions1);
                        if (m > 1)
                        { // there is a match that is non-trivial, make sure to add this.
                            sequence2 = new uint[m];
                            sequence.CopyTo(0, sequence2, 0, m);
                        }
                    }

                    // calculate witness paths here, we need the sequences that are fixed to calculate a witness path.
                    EdgePath sourcePath = null;
                    var enumerator = _graph.GetEdgeEnumerator();
                    if (sequence1 != null &&
                        sequence1.Length > 0)
                    {
                        sourcePath = new EdgePath(enumerator.GetEdge(edge1.Neighbour, sequence1[0]).Id);
                        for(var s = 1; s < sequence1.Length; s++)
                        {
                            sourcePath = new EdgePath(enumerator.GetEdge(sequence1[s - 1], sequence1[s]).Id,
                                0, sourcePath);
                        }
                    }
                    EdgePath targetPath = null;
                    if (sequence2 != null &&
                        sequence2.Length > 0)
                    {
                        targetPath = new EdgePath(enumerator.GetEdge(edge2.Neighbour, sequence2[0]).Id);
                        for (var s = 1; s < sequence2.Length; s++)
                        {
                            targetPath = new EdgePath(enumerator.GetEdge(sequence2[s - 1], sequence2[s]).Id,
                                0, targetPath);
                        }
                    }
                    var forwardWitnessed = !(edge1CanMoveForward && edge2CanMoveBackward);
                    var backwardWitnessed = !(edge1CanMoveBackward && edge2CanMoveForward);
                    var maxWeight = edge1Weight + edge2Weight; // TODO: subtract weigths from source and target paths.
                    if (sourcePath != null && targetPath != null)
                    {
                        forwardWitnessed = forwardWitnessed || _witnessCalculator.Calculate(_graph, sourcePath, targetPath, vertex, maxWeight);
                        backwardWitnessed = backwardWitnessed || _witnessCalculator.Calculate(_graph, targetPath, sourcePath, vertex, maxWeight);
                    }
                    else if(sourcePath != null)
                    {
                        forwardWitnessed = forwardWitnessed || _witnessCalculator.Calculate(_graph, sourcePath, edge2.Neighbour, vertex, maxWeight);
                        backwardWitnessed = backwardWitnessed || _witnessCalculator.Calculate(_graph, edge2.Neighbour, sourcePath, vertex, maxWeight);
                    }
                    else if (targetPath != null)
                    {
                        forwardWitnessed = forwardWitnessed || _witnessCalculator.Calculate(_graph, edge1.Neighbour, targetPath, vertex, maxWeight);
                        backwardWitnessed = backwardWitnessed || _witnessCalculator.Calculate(_graph, targetPath, edge1.Neighbour, vertex, maxWeight);
                    }
                    else
                    {
                        forwardWitnessed = forwardWitnessed || _witnessCalculator.Calculate(_graph, edge1.Neighbour, edge2.Neighbour, vertex, maxWeight);
                        backwardWitnessed = backwardWitnessed || _witnessCalculator.Calculate(_graph, edge2.Neighbour, edge1.Neighbour, vertex, maxWeight);
                    }
                    bool? direction = null;
                    if (forwardWitnessed && backwardWitnessed)
                    { // witnessed paths are not shortest paths.
                        continue;
                    }
                    else if(backwardWitnessed)
                    { // only forward
                        direction = true;
                    }
                    else if (forwardWitnessed)
                    { // only backward
                        direction = false;
                    }
                    
                    // build data-array.
                    var dataSize = 2;
                    if (sequence1 != null)
                    {
                        dataSize += 1;
                        dataSize += sequence1.Length;
                    }
                    if (sequence2 != null)
                    {
                        if (sequence1 == null)
                        {
                            dataSize += 1;
                        }
                        dataSize += sequence2.Length;
                    }
                    var data = new uint[dataSize];
                    data[0] = ContractedEdgeDataSerializer.Serialize(edge1Weight + edge2Weight, direction);
                    data[1] = vertex;
                    if (sequence1 != null)
                    {
                        data[2] = (uint)sequence1.Length;
                        sequence1.CopyTo(data, 3);
                    }
                    if (sequence2 != null)
                    {
                        var sequence2Start = 3;
                        if (sequence1 == null)
                        {
                            data[2] = 0;
                        }
                        else
                        {
                            sequence2Start += sequence1.Length;
                        }
                        sequence2.CopyTo(data, sequence2Start);
                    }
                    _graph.AddEdge(edge1.Neighbour, edge2.Neighbour, data);
                }
            }

            _contractedFlags[vertex] = true;
            _priorityCalculator.NotifyContracted(vertex);
        }

        /// <summary>
        /// Gets the longest possible sequence along the path formed by edge1 -> edge2.
        /// </summary>
        private List<uint> GetSequence(DynamicEdge edge1, uint vertex, DynamicEdge edge2)
        {
            var sequence = new List<uint>();
            sequence.AddRange(edge1.GetReverseSequence(vertex));
            if (sequence.Count > 0 &&
                sequence[sequence.Count - 1] == vertex)
            {
                sequence.AddRange(edge2.GetSequence());
            }
            return sequence;
        }

        /// <summary>
        /// Gets the longest possible sequence along the path formed by edge2 -> edge1
        /// </summary>
        private List<uint> GetReverseSequence(DynamicEdge edge1, uint vertex, DynamicEdge edge2)
        {
            var sequence = new List<uint>();
            sequence.AddRange(edge2.GetReverseSequence(vertex));
            if (sequence.Count > 0 &&
                sequence[sequence.Count - 1] == vertex)
            {
                sequence.AddRange(edge1.GetSequence());
            }
            return sequence;
        }
    }
}