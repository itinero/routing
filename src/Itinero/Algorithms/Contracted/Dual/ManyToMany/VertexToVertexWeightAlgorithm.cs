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
        private readonly Dictionary<uint, Dictionary<int, T>> _buckets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public VertexToVertexWeightAlgorithm(DirectedMetaGraph graph, WeightHandler<T> weightHandler, DykstraSource<T>[] sources,
            DykstraSource<T>[] targets, T max)
        {
            _graph = graph;
            _sources = sources;
            _targets = targets;
            _weightHandler = weightHandler;
            _max = max;

            _buckets = new Dictionary<uint, Dictionary<int, T>>();
        }

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

    // TODO: implement a non-generic non-weighthandler version.
}