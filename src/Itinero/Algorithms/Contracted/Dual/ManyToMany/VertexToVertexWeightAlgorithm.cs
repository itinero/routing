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

using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;
using Itinero.Algorithms.Contracted.Dual.Cache;

namespace Itinero.Algorithms.Contracted.Dual.ManyToMany
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy between source and target vertices.
    /// </summary>
    public class VertexToVertexWeightAlgorithm<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly DykstraSource<T>[] _sources;
        private readonly DykstraSource<T>[] _targets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;
        private readonly SearchSpaceCache<T> _cache;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public VertexToVertexWeightAlgorithm(DirectedMetaGraph graph, WeightHandler<T> weightHandler, DykstraSource<T>[] sources,
            DykstraSource<T>[] targets, T max, SearchSpaceCache<T> cache = null)
        {
            _graph = graph;
            _sources = sources;
            _targets = targets;
            _weightHandler = weightHandler;
            _max = max;
            _cache = cache;
        }

        private Dictionary<uint, Dictionary<int, T>> _buckets;
        private T[][] _weights;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // put in default weights, all are infinite.
            // EXPLANATION: a path between two identical vertices has to contain at least one edge.
            _weights = new T[_sources.Length][];
            for (var i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                _weights[i] = new T[_targets.Length];
                for (var j = 0; j < _targets.Length; j++)
                {
                    var target = _targets[j];
                    _weights[i][j] = _weightHandler.Infinite;
                }
            }

            _buckets = new Dictionary<uint, Dictionary<int, T>>();
            if (_cache == null)
            { // when there is no cache, just calculate everything.
            
                // do forward searches into buckets.
                for (var i = 0; i < _sources.Length; i++)
                {
                    var forward = new Dykstra<T>(_graph, _weightHandler, _sources[i], false, _max);
                    forward.WasFound += (p, v, w) =>
                    {
                        return this.ForwardVertexFound(i, v, w);
                    };
                    forward.Run(cancellationToken);
                }

                // do backward searches into buckets.
                for (var i = 0; i < _targets.Length; i++)
                {
                    var backward = new Dykstra<T>(_graph, _weightHandler, _targets[i], true, _max);
                    backward.WasFound += (p, v, w) =>
                    {
                        return this.BackwardVertexFound(i, v, w);
                    };
                    backward.Run(cancellationToken);
                }
            }
            else
            { // when there is a cache, calculate everything that is not cached.
                // get forward search spaces.
                var forwardSpaces = new SearchSpace<T>[_sources.Length];
                for (var i = 0; i < _sources.Length; i++)
                {
                    var source = _sources[i];
                    if (!_cache.TryGet(source, false, out var space))
                    {
                        // not found, calculate search space.
                        var forward = new Dykstra<T>(_graph, _weightHandler, _sources[i], false, _max);                    
                        forward.WasFound += (p, v, w) =>
                        {
                            return this.ForwardVertexFound(i, v, w);
                        };
                        forward.Run(cancellationToken);
                        space = forward.GetSearchSpace();
                        
                        _cache.Add(source, false, space);
                    }
                    else
                    {
                        foreach (var visit in space.Visits)
                        {
                            this.ForwardVertexFound(i, visit.Key, visit.Value.Item2);
                        }
                    }
                }

                // get backward search spaces.
                var backwardSpaces = new SearchSpace<T>[_targets.Length];
                for (var i = 0; i < _targets.Length; i++)
                {
                    var target = _targets[i];
                    if (!_cache.TryGet(target, true, out var space))
                    {
                        var backward = new Dykstra<T>(_graph, _weightHandler, _targets[i], true, _max);
                        backward.WasFound += (p, v, w) =>
                        {
                            return this.BackwardVertexFound(i, v, w);
                        };
                        backward.Run(cancellationToken);
                        space = backward.GetSearchSpace();
                        
                        _cache.Add(target, true, space);
                    }
                    else
                    {
                        foreach (var visit in space.Visits)
                        {
                            this.BackwardVertexFound(i, visit.Key, visit.Value.Item2);
                        }
                    }
                }
                
//                // compose weights.
//                for (var s = 0; s < forwardSpaces.Length; s++)
//                {
//                    for (var t = 0; t < backwardSpaces.Length; t++)
//                    {
//                        var forwardSpace = forwardSpaces[s];
//                        var backwardSpace = backwardSpaces[t];
//
//                        _weights[s][t] = _weightHandler.Infinite;
//                        if (s == t)
//                        {
//                            _weights[s][t] = _weightHandler.Zero;
//                            continue;
//                        }
//
//                        foreach (var forwardVisit in forwardSpace.Visits)
//                        {
//                            //Console.WriteLine(forwardVisit.Value.Item2);
//
//                            if (!backwardSpace.Visits.TryGetValue(forwardVisit.Key, out var backwardVisit))
//                            {
//                                continue;
//                            }
//
//                            var sourceWeight = forwardVisit.Value.Item2;
//                            var targetWeight = backwardVisit.Item2;
//
//                            var total = _weightHandler.Add(sourceWeight, targetWeight);
//                            if (_weightHandler.IsSmallerThan(total, _weights[s][t]))
//                            {
//                                _weights[s][t] = total;
//                            }
//                        }
//                    }
//                }
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        public T[][] Weights
        {
            get
            {
                return _weights;
            }
        }

        /// <summary>
        /// Called when a forward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool ForwardVertexFound(int i, uint vertex, T weight)
        {
            Dictionary<int, T> bucket;
            if (!_buckets.TryGetValue(vertex, out bucket))
            {
                bucket = new Dictionary<int, T>();
                _buckets.Add(vertex, bucket);
                bucket[i] = weight;
            }
            else
            {
                T existing;
                if (bucket.TryGetValue(i, out existing))
                {
                    if (_weightHandler.IsSmallerThan(weight, existing))
                    {
                        bucket[i] = weight;
                    }
                }
                else
                {
                    bucket[i] = weight;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when a backward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool BackwardVertexFound(int i, uint vertex, T weight)
        {
            Dictionary<int, T> bucket;
            if (_buckets.TryGetValue(vertex, out bucket))
            {
                foreach (var pair in bucket)
                {
                    var existing = _weights[pair.Key][i];
                    var total = _weightHandler.Add(weight, pair.Value);
                    if (_weightHandler.IsSmallerThan(total, existing))
                    {
                        _weights[pair.Key][i] = total;
                    }
                }
            }
            return false;
        }
    }

    // TODO: implement a non-generic non-weight handler version.
}