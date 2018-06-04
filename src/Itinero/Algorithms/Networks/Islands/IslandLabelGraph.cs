using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Security;
using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Networks.Analytics.Heatmaps;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Graphs;
using Itinero.Graphs.Directed;
using Itinero.Profiles.Lua.Tree;

namespace Itinero.Algorithms.Networks.Islands
{
    /// <summary>
    /// An island label graph.
    /// </summary>
    internal class IslandLabelGraph
    {
        private readonly DirectedGraph _graph;

        /// <summary>
        /// Creates a new island label graph.
        /// </summary>
        internal IslandLabelGraph()
        {
            _graph = new DirectedGraph(0, 0);
        }

        /// <summary>
        /// Connects the two given labels.
        /// </summary>
        /// <param name="label1">The first label.</param>
        /// <param name="label2">The second label.</param>
        internal void Connect(uint label1, uint label2)
        {
            if (label1 == label2)
            {
                return;
            }
            _graph.AddEdge(label1, label2);
        }

        /// <summary>
        /// Gets the edge enumerator.
        /// </summary>
        /// <returns></returns>
        internal DirectedGraph.EdgeEnumerator GetEdgeEnumerator()
        {
            return _graph.GetEdgeEnumerator();
        }

        /// <summary>
        /// Gets the label count.
        /// </summary>
        internal uint LabelCount => _graph.VertexCount;

        /// <summary>
        /// Finds loops and merges them together.
        /// </summary>
        /// <param name="maxSettles">The maximum labels to settle.</param>
        /// <param name="updateLabel">A callback to update label.</param>
        internal void FindLoops(uint maxSettles, IslandLabels islandLabels, Action<uint, uint> updateLabel)
        {
            // TODO: it's probably better to call reduce here when too much has changed.
            
            var pathTree = new PathTree();
            var enumerator = _graph.GetEdgeEnumerator();
            var settled = new HashSet<uint>();
            var queue = new Queue<uint>();
            var loop = new HashSet<uint>(); // keeps all with a path back to label, initially only label.
            uint label = 0;
            while (label < _graph.VertexCount)
            {
                if (!enumerator.MoveTo(label))
                {
                    label++;
                    continue;
                }

                if (islandLabels[label] != label)
                {
                    label++;
                    continue;
                }

                queue.Clear();
                pathTree.Clear();
                settled.Clear();

                loop.Add(label);
                queue.Enqueue(pathTree.Add(label, uint.MaxValue));

                while (queue.Count > 0 &&
                       settled.Count < maxSettles)
                {
                    var pointer = queue.Dequeue();
                    pathTree.Get(pointer, out var current, out var previous);

                    if (settled.Contains(current))
                    {
                        continue;
                    }

                    settled.Add(current);

                    if (!enumerator.MoveTo(current))
                    {
                        continue;
                    }

                    while (enumerator.MoveNext())
                    {
                        var n = enumerator.Neighbour;
                        
                        n = islandLabels[n];
                        
                        if (loop.Contains(n))
                        {
                            // yay, a loop!
                            loop.Add(current);
                            while (previous != uint.MaxValue)
                            {
                                pathTree.Get(previous, out current, out previous);
                                loop.Add(current);
                            }
                        }
                        if (settled.Contains(n))
                        {
                            continue;
                        }
                        
                        queue.Enqueue(pathTree.Add(n, pointer));
                    }
                }

                if (loop.Count > 0)
                {
                    this.Merge(loop, updateLabel);
                }
                loop.Clear();

                // move to the next label.
                label++;
            }
        }
        
        /// <summary>
        /// Merge all the given labels into one.
        /// </summary>
        /// <param name="labels">The labels to merge.</param>
        /// <param name="updateLabel">A callback to update label.</param>
        private void Merge(HashSet<uint> labels, Action<uint, uint> updateLabel)
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
                    var n = edgeEnumerator.Neighbour;
                    if (!labels.Contains(n))
                    {
                        neighbours.Add(n);
                    }
                }

                _graph.RemoveEdges(label);
            }

            if (bestLabel == uint.MaxValue)
            {
                return;
            }
            
            foreach (var neighbour in neighbours)
            {
                //_graph.RemoveEdge(bestLabel, neighbour);
                _graph.AddEdge(bestLabel, neighbour);
            }
            foreach (var label in labels)
            {
                if (label == bestLabel)
                {
                    continue;
                }
                
                updateLabel(label, bestLabel);
                //_graph.RemoveEdge(label, bestLabel);
                //_graph.AddEdge(label, bestLabel);
            }
        }

        /// <summary>
        /// Removes all islands that are not roots and updates all edges.
        /// </summary>
        /// <param name="islandLabels">The current labels.</param>
        internal long Reduce(IslandLabels islandLabels)
        {
            // remove vertices that aren't originals.
            var neighbours = new HashSet<uint>();
            var edgeEnumerator = _graph.GetEdgeEnumerator();
            var edgeEnumerator2 = _graph.GetEdgeEnumerator();
            var nonNullLabels = 0;
            for (uint label = 0; label < _graph.VertexCount; label++)
            {
                var minimal = islandLabels[label];
                if (!edgeEnumerator.MoveTo(label))
                {
                    _graph.RemoveEdges(label);
                    continue;
                }

                neighbours.Clear();
                while (edgeEnumerator.MoveNext())
                {
                    var n = edgeEnumerator.Neighbour;
                    n = islandLabels[n];
                    if (n == IslandLabels.NoAccess ||
                        n == IslandLabels.NotSet ||
                        n == minimal)
                    {
                        continue;
                    }
                    
                    // remove dead-ends.
                    if (!edgeEnumerator2.MoveTo(n))
                    { // this edge has no targets.
                        continue;
                    }

                    neighbours.Add(n);
                }

                _graph.RemoveEdges(label);
                if (neighbours.Count == 0)
                {
                    continue;
                }

                nonNullLabels++;
                foreach (var n in neighbours)
                {
                    _graph.RemoveEdge(minimal, n);
                    _graph.AddEdge(minimal, n);
                }
            }

            _graph.Compress();
            return nonNullLabels;
        }
    }
}