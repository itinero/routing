// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.LongIndex;
using OsmSharp.Collections.PriorityQueues;
using OsmSharp.Logging;
using OsmSharp.Routing.Data.Contracted;
using OsmSharp.Routing.Graphs.Directed;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Contracted
{
    /// <summary>
    /// Builds a contraction hierarchy.
    /// </summary>
    public class HierarchyBuilder : AlgorithmBase
    {
        private readonly DirectedGraph _graph;
        private readonly IPriorityCalculator _priorityCalculator;
        private readonly IWitnessCalculator _witnessCalculator;

        /// <summary>
        /// Creates a new hierarchy builder.
        /// </summary>
        public HierarchyBuilder(DirectedGraph graph, IPriorityCalculator priorityCalculator, IWitnessCalculator witnessCalculator)
        {
            _graph = graph;
            _priorityCalculator = priorityCalculator;
            _witnessCalculator = witnessCalculator;
        }

        private BinaryHeap<uint> _queue; // the vertex-queue.
        private ILongIndex _contractedFlags; // contains flags for contracted vertices.

        /// <summary>
        /// Excutes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            _queue = new BinaryHeap<uint>();
            _contractedFlags = new OsmSharp.Collections.LongIndex.LongIndex.LongIndex();
            _missesQueue = new Queue<bool>();

            // build queue.
            this.CalculateQueue();

            var next = this.SelectNext();
            while(next != null)
            {
                // contract...
                this.Contract(next.Value);

                // ... and select next.
                next = this.SelectNext();
            }
        }

        /// <summary>
        /// Calculates the entire queue.
        /// </summary>
        private void CalculateQueue()
        {
            OsmSharp.Logging.Log.TraceEvent("HierarchyBuilder", TraceEventType.Information,
                "Calculating queue...");

            _queue.Clear();
            for (uint vertex = 0; vertex < _graph.VertexCount; vertex++)
            {
                if(!_contractedFlags.Contains(vertex))
                {
                    _queue.Push(vertex, _priorityCalculator.Calculate(_contractedFlags, vertex));
                }
            }
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
                if (_contractedFlags.Contains(first))
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

                    // TODO: investigate if compression of target graph is useful here.
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
        public void Contract(uint vertex)
        {
            // get and keep edges.
            var edges = new List<Edge>(_graph.GetEdgeEnumerator(vertex));

            // remove 'downward' edge to vertex.
            var i = 0;
            while(i < edges.Count)
            {
                _graph.RemoveEdge(edges[i].Neighbour, vertex);

                if(_contractedFlags.Contains(edges[i].Neighbour))
                { // neighbour was already contracted, remove 'downward' edge and exclude it.
                    _graph.RemoveEdge(vertex, edges[i].Neighbour);
                    edges.RemoveAt(i);
                }
                else
                { // move to next edge.
                    i++;
                }
            }

            // loop over all edge-pairs once.
            for(var j = 1; j < edges.Count; j++)
            {
                var edge1 = edges[j];

                float edge1Weight;
                bool? edge1Direction;
                uint edge1ContractedId;
                OsmSharp.Routing.Data.Contracted.ContractedEdgeDataSerializer.Deserialize(edge1.Data[0], edge1.Data[1],
                    out edge1Weight, out edge1Direction, out edge1ContractedId);
                var edge1CanMoveForward = edge1Direction == null || edge1Direction.Value;
                var edge1CanMoveBackward = edge1Direction == null || !edge1Direction.Value;

                // figure out what witness paths to calculate.
                var forwardWitnesses = new bool[j];
                var backwardWitnesses = new bool[j];
                var targets = new List<uint>(j);
                var targetWeights = new List<float>(j);
                for(var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    float edge2Weight;
                    bool? edge2Direction;
                    uint edge2ContractedId;
                    ContractedEdgeDataSerializer.Deserialize(edge2.Data[0], edge2.Data[1],
                        out edge2Weight, out edge2Direction, out edge2ContractedId);
                    var edge2CanMoveForward = edge2Direction == null || edge2Direction.Value;
                    var edge2CanMoveBackward = edge2Direction == null || !edge2Direction.Value;

                    forwardWitnesses[k] = !(edge1CanMoveForward && edge1CanMoveBackward);
                    backwardWitnesses[k] = !(edge1CanMoveBackward && edge1CanMoveForward);
                    targets.Add(edge2.Neighbour);
                    targetWeights.Add(edge1Weight + edge2Weight);
                }

                // calculate all witness paths.
                _witnessCalculator.Calculate(_graph, edge1.Neighbour, targets, targetWeights, ref forwardWitnesses, ref backwardWitnesses, vertex);

                // add contracted edges if needed.
                for (var k = 0; k < j; k++)
                {
                    var edge2 = edges[k];

                    if(!forwardWitnesses[k] && !backwardWitnesses[k])
                    { // add bidirectional edge.
                        _graph.AddEdge(edge1.Neighbour, edge2.Neighbour, ContractedEdgeDataSerializer.Serialize(
                            targetWeights[k], null, vertex));
                        _graph.AddEdge(edge2.Neighbour, edge1.Neighbour, ContractedEdgeDataSerializer.Serialize(
                            targetWeights[k], null, vertex));
                    }
                    else if(!forwardWitnesses[k])
                    { // add forward edge.
                        _graph.AddEdge(edge1.Neighbour, edge2.Neighbour, ContractedEdgeDataSerializer.Serialize(
                            targetWeights[k], true, vertex));
                        _graph.AddEdge(edge2.Neighbour, edge1.Neighbour, ContractedEdgeDataSerializer.Serialize(
                            targetWeights[k], false, vertex));
                    }
                    else if (!backwardWitnesses[k])
                    { // add forward edge.
                        _graph.AddEdge(edge1.Neighbour, edge2.Neighbour, ContractedEdgeDataSerializer.Serialize(
                            targetWeights[k], false, vertex));
                        _graph.AddEdge(edge2.Neighbour, edge1.Neighbour, ContractedEdgeDataSerializer.Serialize(
                            targetWeights[k], true, vertex));
                    }
                }
            }
        }
    }
}