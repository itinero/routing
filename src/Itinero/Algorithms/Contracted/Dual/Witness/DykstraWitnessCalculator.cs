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

using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Algorithms.Collections;
using Itinero.Data.Contracted;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System.Collections.Generic;
using System;
using System.Threading;

namespace Itinero.Algorithms.Contracted.Dual.Witness
{
    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public class DykstraWitnessCalculator<T>
        where T : struct
    {
        protected readonly WeightHandler<T> _weightHandler;
        protected readonly DirectedGraph _graph;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedGraph graph, WeightHandler<T> weightHandler, int hopLimit)
            : this(graph, weightHandler, hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedGraph graph, WeightHandler<T> weightHandler, int hopLimit, int maxSettles)
        {
            _hopLimit = hopLimit;
            _weightHandler = weightHandler;
            _graph = graph;
            
            _maxSettles = maxSettles;
        }

        protected int _hopLimit;
        protected int _maxSettles;

        /// <summary>
        /// Calculates and updates the shortcuts by searching for witness paths.
        /// </summary>
        public virtual void Calculate(uint vertex, Shortcuts<T> shortcuts)
        {
            var sources = new HashSet<OriginalEdge>();
            var waitHandlers = new List<ManualResetEvent>();

            while (true)
            {
                var source = Constants.NO_VERTEX;
                var targets = new Dictionary<uint, Shortcut<T>>();

                foreach (var shortcut in shortcuts)
                {
                    if (source == Constants.NO_VERTEX &&
                        !sources.Contains(shortcut.Key))
                    {
                        source = shortcut.Key.Vertex1;
                        sources.Add(shortcut.Key);
                    }
                    if (shortcut.Key.Vertex1 == source)
                    {
                        targets[shortcut.Key.Vertex2] = shortcut.Value;
                        sources.Add(shortcut.Key);
                    }
                    if (shortcut.Key.Vertex2 == source)
                    {
                        targets[shortcut.Key.Vertex1] = new Shortcut<T>()
                        {
                            Backward = shortcut.Value.Forward,
                            Forward = shortcut.Value.Backward
                        };
                        sources.Add(shortcut.Key);
                    }
                }

                if (targets.Count == 0)
                { // no more searches needed.
                    break;
                }

                Calculate(_graph, _weightHandler, vertex, source, targets, _maxSettles,
                    _hopLimit);

                foreach (var targetPair in targets)
                {
                    var e = new OriginalEdge(source, targetPair.Key);
                    Shortcut<T> s;
                    if (shortcuts.TryGetValue(e, out s))
                    {
                        shortcuts[e] = targetPair.Value;
                    }
                    else
                    {
                        e = e.Reverse();
                        shortcuts[e] = new Shortcut<T>()
                        {
                            Backward = targetPair.Value.Forward,
                            Forward = targetPair.Value.Backward
                        };
                    }
                }
            }
        }

        protected PathTree pathTree = new PathTree();
        protected BinaryHeap<uint> pointerHeap = new BinaryHeap<uint>();

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public virtual void Calculate(DirectedGraph graph, WeightHandler<T> weightHandler, uint vertex, 
            uint source, Dictionary<uint, Shortcut<T>> targets, int maxSettles, int hopLimit)
        {
            pathTree.Clear();
            pointerHeap.Clear();

            var forwardSettled = new HashSet<uint>();
            var backwardSettled = new HashSet<uint>();

            var forwardTargets = new HashSet<uint>();
            var backwardTargets = new HashSet<uint>();

            var maxWeight = 0f;

            foreach (var targetPair in targets)
            {
                var target = targetPair.Key;
                var shortcut = targetPair.Value;
                var e = new OriginalEdge(source, target);

                var shortcutForward = weightHandler.GetMetric(shortcut.Forward);
                if (shortcutForward > 0 && shortcutForward < float.MaxValue)
                {
                    forwardTargets.Add(e.Vertex2);
                    if (shortcutForward > maxWeight)
                    {
                        maxWeight = shortcutForward;
                    }
                }
                var shortcutBackward = weightHandler.GetMetric(shortcut.Backward);
                if (shortcutBackward > 0 && shortcutBackward < float.MaxValue)
                {
                    backwardTargets.Add(e.Vertex2);
                    if (shortcutBackward > maxWeight)
                    {
                        maxWeight = shortcutBackward;
                    }
                }
            }

            // queue the source.
            pathTree.Clear();
            pointerHeap.Clear();
            var p = pathTree.AddSettledVertex(source, new WeightAndDir<float>()
            {
                Direction = new Dir(true, true),
                Weight = 0
            }, 0);
            pointerHeap.Push(p, 0);

            // dequeue vertices until stopping conditions are reached.
            var cVertex = Constants.NO_VERTEX;
            WeightAndDir<float> cWeight;
            var cHops = uint.MaxValue;
            var enumerator = graph.GetEdgeEnumerator();
            while (pointerHeap.Count > 0)
            {
                var cPointer = pointerHeap.Pop();
                pathTree.GetSettledVertex(cPointer, out cVertex, out cWeight, out cHops);

                if (cVertex == vertex)
                {
                    continue;
                }

                if (cWeight.Weight >= maxWeight)
                {
                    break;
                }

                if (forwardSettled.Contains(cVertex) ||
                    forwardTargets.Count == 0 ||
                    forwardSettled.Count > maxSettles)
                {
                    cWeight.Direction = new Dir(false, cWeight.Direction.B);
                }
                if (backwardSettled.Contains(cVertex) ||
                    backwardTargets.Count == 0 ||
                    backwardSettled.Count > maxSettles)
                {
                    cWeight.Direction = new Dir(cWeight.Direction.F, false);
                }

                if (cWeight.Direction.F)
                {
                    forwardSettled.Add(cVertex);
                    if (forwardTargets.Contains(cVertex))
                    { // target reached, evaluate it as a shortcut.
                        Shortcut<T> shortcut;
                        if (targets.TryGetValue(cVertex, out shortcut))
                        {
                            var shortcutForward = weightHandler.GetMetric(shortcut.Forward);
                            if (shortcutForward > cWeight.Weight)
                            { // a witness path was found, don't add a shortcut.
                                shortcut.Forward = weightHandler.Zero;
                                targets[cVertex] = shortcut;
                            }
                        }
                        forwardTargets.Remove(cVertex);
                        if (forwardTargets.Count == 0)
                        {
                            if (backwardTargets.Count == 0)
                            {
                                break;
                            }
                            cWeight.Direction = new Dir(false, cWeight.Direction.B);
                            if (!cWeight.Direction.F && !cWeight.Direction.B)
                            {
                                continue;
                            }
                        }
                    }
                }
                if (cWeight.Direction.B)
                {
                    backwardSettled.Add(cVertex);
                    if (backwardTargets.Contains(cVertex))
                    { // target reached, evaluate it as a shortcut.
                        Shortcut<T> shortcut;
                        if (targets.TryGetValue(cVertex, out shortcut))
                        {
                            var shortcutBackward = weightHandler.GetMetric(shortcut.Backward);
                            if (shortcutBackward > cWeight.Weight)
                            { // a witness path was found, don't add a shortcut.
                                shortcut.Backward = weightHandler.Zero;
                                targets[cVertex] = shortcut;
                            }
                        }
                        backwardTargets.Remove(cVertex);
                        if (backwardTargets.Count == 0)
                        {
                            if (forwardTargets.Count == 0)
                            {
                                break;
                            }
                            cWeight.Direction = new Dir(cWeight.Direction.F, false);
                            if (!cWeight.Direction.F && !cWeight.Direction.B)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (cHops + 1 >= hopLimit)
                {
                    continue;
                }

                if (forwardSettled.Count > maxSettles &&
                    backwardSettled.Count > maxSettles)
                {
                    continue;
                }

                enumerator.MoveTo(cVertex);
                while (enumerator.MoveNext())
                {
                    var nVertex = enumerator.Neighbour;
                    var nWeight = ContractedEdgeDataSerializer.Deserialize(enumerator.Data0);

                    nWeight = new WeightAndDir<float>()
                    {
                        Direction = Dir.Combine(cWeight.Direction, nWeight.Direction),
                        Weight = cWeight.Weight + nWeight.Weight
                    };

                    if (nWeight.Direction.F &&
                        forwardSettled.Contains(nVertex))
                    {
                        nWeight.Direction = new Dir(false, nWeight.Direction.B);
                    }
                    if (nWeight.Direction.B &&
                        backwardSettled.Contains(nVertex))
                    {
                        nWeight.Direction = new Dir(nWeight.Direction.F, false);
                    }
                    if (!nWeight.Direction.F && !nWeight.Direction.B)
                    {
                        continue;
                    }

                    var nPoiner = pathTree.AddSettledVertex(nVertex, nWeight, cHops + 1);
                    pointerHeap.Push(nPoiner, nWeight.Weight);
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
    }

    /// <summary>
    /// A dykstra-based witness calculator for float weights.
    /// </summary>
    public sealed class DykstraWitnessCalculator : DykstraWitnessCalculator<float>
    {
        /// <summary>
        /// Creates a new witnesscalculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedGraph graph, int hopLimit = int.MaxValue, int maxSettles = int.MaxValue)
            : base(graph, new DefaultWeightHandler(null), hopLimit, maxSettles)
        {

        }

        /// <summary>
        /// Creates a new witnesscalculator.
        /// </summary>
        public DykstraWitnessCalculator(DirectedGraph graph, WeightHandler<float> weightHandler, int hopLimit, int maxSettles)
            : base(graph, weightHandler, hopLimit, maxSettles)
        {

        }
        
        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public override void Calculate(DirectedGraph graph, WeightHandler<float> weightHandler, uint vertex,
            uint source, Dictionary<uint, Shortcut<float>> targets, int maxSettles, int hopLimit)
        {
            pathTree.Clear();
            pointerHeap.Clear();

            var forwardSettled = new HashSet<uint>();
            var backwardSettled = new HashSet<uint>();

            var forwardTargets = new HashSet<uint>();
            var backwardTargets = new HashSet<uint>();

            var maxWeight = 0f;
            var maxForwardWeight = 0f;
            var maxBackwardWeight = 0f;

            foreach (var targetPair in targets)
            {
                var target = targetPair.Key;
                var shortcut = targetPair.Value;
                var e = new OriginalEdge(source, target);
                
                if (shortcut.Forward > 0 && shortcut.Forward < float.MaxValue)
                {
                    forwardTargets.Add(e.Vertex2);
                    if (shortcut.Forward > maxWeight)
                    {
                        maxWeight = shortcut.Forward;
                    }
                    if (shortcut.Forward > maxForwardWeight)
                    {
                        maxForwardWeight = shortcut.Forward;
                    }
                }
                if (shortcut.Backward > 0 && shortcut.Backward < float.MaxValue)
                {
                    backwardTargets.Add(e.Vertex2);
                    if (shortcut.Backward > maxWeight)
                    {
                        maxWeight = shortcut.Backward;
                    }
                    if (shortcut.Backward > maxBackwardWeight)
                    {
                        maxBackwardWeight = shortcut.Backward;
                    }
                }
            }

            // queue the source.
            pathTree.Clear();
            pointerHeap.Clear();
            var p = pathTree.AddSettledVertex(source, 0, new Dir(true, true), 0);
            pointerHeap.Push(p, 0);

            // dequeue vertices until stopping conditions are reached.
            var cVertex = Constants.NO_VERTEX;
            WeightAndDir<float> cWeight;
            var cHops = uint.MaxValue;
            var enumerator = graph.GetEdgeEnumerator();
            while (pointerHeap.Count > 0)
            {
                var cPointer = pointerHeap.Pop();
                pathTree.GetSettledVertex(cPointer, out cVertex, out cWeight, out cHops);

                if (cVertex == vertex)
                {
                    continue;
                }

                if (cWeight.Weight >= maxWeight)
                {
                    break;
                }

                if (forwardSettled.Contains(cVertex) ||
                    forwardTargets.Count == 0 ||
                    forwardSettled.Count > maxSettles ||
                    cWeight.Weight > maxForwardWeight)
                {
                    cWeight.Direction = new Dir(false, cWeight.Direction.B);
                }
                if (backwardSettled.Contains(cVertex) ||
                    backwardTargets.Count == 0 ||
                    backwardSettled.Count > maxSettles ||
                    cWeight.Weight > maxBackwardWeight)
                {
                    cWeight.Direction = new Dir(cWeight.Direction.F, false);
                }

                if (cWeight.Direction.F)
                {
                    forwardSettled.Add(cVertex);
                    if (forwardTargets.Contains(cVertex))
                    { // target reached, evaluate it as a shortcut.
                        Shortcut<float> shortcut;
                        if (targets.TryGetValue(cVertex, out shortcut))
                        {
                            var shortcutForward = shortcut.Forward;
                            if (shortcutForward > cWeight.Weight)
                            { // a witness path was found, don't add a shortcut.
                                shortcut.Forward = 0;
                                targets[cVertex] = shortcut;
                            }
                        }
                        forwardTargets.Remove(cVertex);
                        if (forwardTargets.Count == 0)
                        {
                            if (backwardTargets.Count == 0)
                            {
                                break;
                            }
                            cWeight.Direction = new Dir(false, cWeight.Direction.B);
                            if (!cWeight.Direction.F && !cWeight.Direction.B)
                            {
                                continue;
                            }
                        }
                    }
                }
                if (cWeight.Direction.B)
                {
                    backwardSettled.Add(cVertex);
                    if (backwardTargets.Contains(cVertex))
                    { // target reached, evaluate it as a shortcut.
                        Shortcut<float> shortcut;
                        if (targets.TryGetValue(cVertex, out shortcut))
                        {
                            var shortcutBackward = shortcut.Backward;
                            if (shortcutBackward > cWeight.Weight)
                            { // a witness path was found, don't add a shortcut.
                                shortcut.Backward = 0;
                                targets[cVertex] = shortcut;
                            }
                        }
                        backwardTargets.Remove(cVertex);
                        if (backwardTargets.Count == 0)
                        {
                            if (forwardTargets.Count == 0)
                            {
                                break;
                            }
                            cWeight.Direction = new Dir(cWeight.Direction.F, false);
                            if (!cWeight.Direction.F && !cWeight.Direction.B)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (cHops + 1 >= hopLimit)
                {
                    continue;
                }

                if (forwardSettled.Count > maxSettles &&
                    backwardSettled.Count > maxSettles)
                {
                    break;
                }

                enumerator.MoveTo(cVertex);
                while (enumerator.MoveNext())
                {
                    var nVertex = enumerator.Neighbour;
                    Dir nDir;
                    float nWeight;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0,
                        out nDir, out nWeight);

                    nDir._val = (byte)(cWeight.Direction._val & nDir._val);
                    //nWeight = new WeightAndDir<float>()
                    //{
                    //    Direction = Dir.Combine(cWeight.Direction, nWeight.Direction),
                    //    Weight = cWeight.Weight + nWeight.Weight
                    //};

                    if (nDir.F && forwardSettled.Contains(nVertex))
                    {
                        nDir._val = (byte)(nDir._val & 2);
                        //nDir = new Dir(false, nDir.B);
                    }
                    if (nDir.B && backwardSettled.Contains(nVertex))
                    {
                        nDir._val = (byte)(nDir._val & 1);
                        //nDir = new Dir(nDir.F, false);
                    }
                    if (nDir._val == 0)
                    {
                        continue;
                    }

                    nWeight = nWeight + cWeight.Weight;

                    var nPoiner = pathTree.AddSettledVertex(nVertex, nWeight, nDir, cHops + 1);
                    pointerHeap.Push(nPoiner, nWeight);
                }
            }
        }
    }
}