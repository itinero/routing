using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs;
using Itinero.Graphs.Directed;

namespace Itinero.Algorithms.Networks.Islands
{
    /// <summary>
    /// An island label graph.
    /// </summary>
    internal class IslandLabelGraph
    {
        private readonly Graph _graph;

        /// <summary>
        /// Creates a new island label graph.
        /// </summary>
        public IslandLabelGraph()
        {
            _graph = new Graph(1);
        }

        /// <summary>
        /// Connects the two given labels.
        /// </summary>
        /// <param name="label1">The first label.</param>
        /// <param name="label2">The second label.</param>
        public void Connect(uint label1, uint label2)
        {
            _graph.UpdateEdgeData();
            _graph.AddEdge(label1, label2);
        }

        /// <summary>
        /// Try to reduce the graph using the given updated labels.
        /// </summary>
        /// <param name="updated">The updated labels.</param>
        public void Reduce(HashSet<uint> updated)
        {
            var enumerator = _graph.GetEdgeEnumerator();
            while (updated.Count > 0)
            {
                // do a dykstra search starting at the first.
                var current = updated.First();
                var first = current;
                updated.Remove(current);
                
                var settled = new Dictionary<uint, uint>();
                var queue = new Queue<KeyValuePair<uint, uint>>();
                queue.Enqueue(new KeyValuePair<uint, uint>(current, Constants.NO_VERTEX));

                var triggeredMerge = false;
                while (queue.Count > 0)
                {
                    // dequeue next.
                    var dequeued = queue.Dequeue();
                    current = dequeued.Key;
                    
                    if (settled.ContainsKey(current))
                    { // already settled.
                        continue;
                    }

                    // mark as settled.
                    var previous = dequeued.Value;
                    settled[current] = previous;

                    if (!enumerator.MoveTo(current))
                    {
                        continue;
                    }

                    while (enumerator.MoveNext())
                    {
                        var neighbour = enumerator.Neighbour;
                        if (settled.ContainsKey(neighbour))
                        {
                            continue;
                        }

                        if (neighbour == first)
                        { // we found a loop!
                            var toMerge = new List<uint>();
                            toMerge.Add(neighbour);
                            toMerge.Add(current);
                            while (settled.TryGetValue(current, out previous))
                            {
                                if (previous == Constants.NO_VERTEX)
                                {
                                    break;;
                                }
                                toMerge.Add(previous);
                                current = previous;
                            }
                            
                            queue.Clear(); // stop the search.
                            this.Merge(toMerge);
                            break;
                        }
                        
                        queue.Enqueue(new KeyValuePair<uint, uint>(neighbour, current));
                    }
                }
            }
        }

        /// <summary>
        /// Merge all the given labels into one.
        /// </summary>
        /// <param name="labels">The labels to merge.</param>
        private void Merge(IEnumerable<uint> labels)
        {
            var edgeEnumerator = _graph.GetEdgeEnumerator();
            var bestLabel = uint.MaxValue;
            var neighbours = new HashSet<uint>();
            foreach (var label in labels)
            {
                if (label < bestLabel)
                {
                    bestLabel = label;
                }

                if (!edgeEnumerator.MoveTo(label))
                {
                    continue;
                }

                while (edgeEnumerator.MoveNext())
                {
                    neighbours.Add(edgeEnumerator.Neighbour);
                }

                _graph.RemoveEdges(label);
            }

            if (bestLabel == uint.MaxValue)
            {
                return;
            }
            
            foreach (var neighbour in neighbours)
            {
                _graph.AddEdge(bestLabel, neighbour);

                
            }
        }
    }
}