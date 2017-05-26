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
using Itinero.Data.Contracted;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;
using System.Collections.Generic;
using Itinero.Algorithms.Weights;
using Itinero.Algorithms.Collections;

namespace Itinero.Algorithms.Contracted.Witness
{
    /// <summary>
    /// A witness calculator based on dykstra's algorithm.
    /// </summary>
    public class DykstraWitnessCalculator<T>
        where T : struct
    {
        protected readonly BinaryHeap<uint> _pointerHeap;
        protected readonly WeightHandler<T> _weightHandler;
        protected readonly PathTree _pathTree;
        protected readonly DirectedGraph _graph;

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(WeightHandler<T> weightHandler, int hopLimit)
            : this(weightHandler, hopLimit, int.MaxValue)
        {

        }

        /// <summary>
        /// Creates a new witness calculator.
        /// </summary>
        public DykstraWitnessCalculator(WeightHandler<T> weightHandler, int hopLimit, int maxSettles)
        {
            _hopLimit = hopLimit;
            _weightHandler = weightHandler;

            _pointerHeap = new BinaryHeap<uint>();
            _maxSettles = maxSettles;
        }
        
        protected int _hopLimit;
        protected int _maxSettles;

        /// <summary>
        /// Calculates and updates the shortcuts by searching for witness paths.
        /// </summary>
        public void Calculate(Shortcuts<T> shortcuts)
        {
            var sources = new HashSet<uint>();
            var targets = new HashSet<uint>();

            while(true)
            {
                var source = Constants.NO_VERTEX;
                targets.Clear();

                foreach(var shortcut in shortcuts)
                {
                    if (source == Constants.NO_VERTEX &&
                        sources.Contains(shortcut.Key.Vertex1))
                        {
                            source = shortcut.Key.Vertex1;
                            sources.Add(source);
                        }
                    if (shortcut.Key.Vertex1 == source) 
                    {
                        targets.Add(shortcut.Key.Vertex2);
                    }
                }

                if (targets.Count == 0)
                { // no more searches needed.
                    break;
                }

                this.Calculate(shortcuts, source, targets);
            }
        }

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public virtual void Calculate(Shortcuts<T> shortcuts, uint source, HashSet<uint> targets)
        {
            throw new System.NotImplementedException();
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
        public DykstraWitnessCalculator(int hopLimit = int.MaxValue, int maxSettles = int.MaxValue)
            : base (new DefaultWeightHandler(null), hopLimit, maxSettles)
        {

        }

        /// <summary>
        /// Creates a new witnesscalculator.
        /// </summary>
        public DykstraWitnessCalculator(WeightHandler<float> weightHandler, int hopLimit, int maxSettles)
            : base (weightHandler, hopLimit, maxSettles)
        {

        }

        /// <summary>
        /// Calculates witness paths.
        /// </summary>
        public override void Calculate(Shortcuts<float> shortcuts, uint source, HashSet<uint> targets)
        {
            var forwardSettled = new HashSet<uint>();
            var backwardSettled = new HashSet<uint>();

            var forwardTargets = new HashSet<uint>();
            var backwardTargets = new HashSet<uint>();

            foreach(var target in targets)
            {
                var e = new OriginalEdge(source, target);
                var shortcut = shortcuts[e];

                if (shortcut.Forward > 0)
                {
                    forwardTargets.Add(e.Vertex2);
                }
                if (shortcut.Backward > 0)
                {
                    backwardTargets.Add(e.Vertex2);
                }
            }

            // queue the source.
            _pathTree.Clear();
            var p = _pathTree.AddSettledVertex(source, new WeightAndDir<float>(){
                Direction = new Dir(true, true),
                Weight = 0
            }, 0, uint.MaxValue);
            _pointerHeap.Push(p, 0);

            // dequeue vertices until stopping conditions are reached.
            var cVertex = Constants.NO_VERTEX;
            WeightAndDir<float> cWeight;
            var cHops = uint.MaxValue;
            var enumerator = _graph.GetEdgeEnumerator();
            while (_pointerHeap.Count > 0)
            {
                var cPointer = _pointerHeap.Pop();
                _pathTree.GetSettledVertex(cPointer, out cVertex, out cWeight, out cHops);

                if (forwardSettled.Contains(cVertex) ||
                    forwardTargets.Count == 0 ||
                    forwardSettled.Count > _maxSettles)
                {
                    cWeight.Direction = new Dir(false, cWeight.Direction.B);
                }
                if (backwardSettled.Contains(cVertex) ||
                    backwardTargets.Count == 0 ||
                    backwardSettled.Count > _maxSettles)
                {
                    cWeight.Direction = new Dir(cWeight.Direction.F, false);
                }

                if (cWeight.Direction.F)
                {
                    forwardSettled.Add(cVertex);
                    if (forwardTargets.Contains(cVertex))
                    { // target reached, evaluate it as a shortcut.
                        var e = new OriginalEdge(source, cVertex);
                        var shortcut = shortcuts[e];
                        if (shortcut.Forward < cWeight.Weight)
                        { // a witness path was found, don't add a shortcut.
                            shortcut.Forward = 0;
                            shortcuts[e] = shortcut;
                        }
                        forwardTargets.Remove(cVertex);
                        if(forwardTargets.Count == 0)
                        {
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
                        var e = new OriginalEdge(source, cVertex);
                        var shortcut = shortcuts[e];
                        if (shortcut.Backward < cWeight.Weight)
                        { // a witness path was found, don't add a shortcut.
                            shortcut.Backward = 0;
                            shortcuts[e] = shortcut;
                        }
                        backwardTargets.Remove(cVertex);
                        if(backwardTargets.Count == 0)
                        {
                            cWeight.Direction = new Dir(cWeight.Direction.F, false);
                            if (!cWeight.Direction.F && !cWeight.Direction.B)
                            {
                                continue;
                            }
                        }
                    }
                }

                if (cHops + 1 >= _hopLimit)
                {
                    continue;
                }

                if (forwardSettled.Count > _maxSettles &&
                    backwardSettled.Count > _maxSettles)
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
                    if (!nWeight.Direction.F && !nWeight.Direction.B)
                    {
                        continue;
                    }

                    var nPoiner = _pathTree.AddSettledVertex(nVertex, nWeight, cHops + 1, cPointer);
                    _pointerHeap.Push(nPoiner, nWeight.Weight);
                }
            }
        }
    }
}