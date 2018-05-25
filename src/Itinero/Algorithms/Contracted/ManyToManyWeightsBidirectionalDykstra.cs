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

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy.
    /// </summary>
    public class ManyToManyWeightsBidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly DirectedMetaGraph _graph;
        private readonly RouterPoint[] _sources;
        private readonly RouterPoint[] _targets;
        private readonly Dictionary<uint, Dictionary<int, T>> _buckets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyWeightsBidirectionalDykstra(RouterDb routerDb, Profile profile, WeightHandler<T> weightHandler, RouterPoint[] sources,
            RouterPoint[] targets, T max)
        { 
            _routerDb = routerDb;
            _sources = sources;
            _targets = targets;
            _weightHandler = weightHandler;
            _max = max;

            ContractedDb contractedDb;
            if (!_routerDb.TryGetContracted(profile, out contractedDb))
            {
                throw new NotSupportedException(
                    "Contraction-based many-to-many calculates are not supported in the given router db for the given profile.");
            }
            if (!contractedDb.HasNodeBasedGraph)
            {
                throw new NotSupportedException(
                    "Contraction-based node-based many-to-many calculates are not supported in the given router db for the given profile.");
            }
            _graph = contractedDb.NodeBasedGraph;
            weightHandler.CheckCanUse(contractedDb);

            _buckets = new Dictionary<uint, Dictionary<int, T>>();
        }

        private T[][] _weights;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // put in default weights and weights for one-edge-paths.
            _weights = new T[_sources.Length][];
            for (var i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                _weights[i] = new T[_targets.Length];
                for (var j = 0; j < _targets.Length; j++)
                {
                    var target = _targets[j];
                    _weights[i][j] = _weightHandler.Infinite;

                    if(target.EdgeId == source.EdgeId)
                    {
                        var path = source.EdgePathTo(_routerDb, _weightHandler, target);
                        if (path != null)
                        {
                            _weights[i][j] = path.Weight;
                        }
                    }
                }
            }

            // do forward searches into buckets.
            for(var i = 0; i < _sources.Length; i++)
            {
                var forward = new Dykstra<T>(_graph, _weightHandler, _sources[i].ToEdgePaths(_routerDb, _weightHandler, true), false, _max);
                forward.WasFound += (path) =>
                    {
                        return this.ForwardVertexFound(i, path.Vertex, path.Weight);
                    };
                forward.Run(cancellationToken);
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targets.Length; i++)
            {
                var backward = new Dykstra<T>(_graph, _weightHandler, _targets[i].ToEdgePaths(_routerDb, _weightHandler, false), true, _max);
                backward.WasFound += (path) =>
                    {
                        return this.BackwardVertexFound(i, path.Vertex, path.Weight);
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
            if(!_buckets.TryGetValue(vertex, out bucket))
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
                    if(_weightHandler.IsSmallerThan(weight, existing))
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
            if(_buckets.TryGetValue(vertex, out bucket))
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

    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy.
    /// </summary>
    public sealed class ManyToManyWeightsBidirectionalDykstra : ManyToManyWeightsBidirectionalDykstra<float>
    {
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyWeightsBidirectionalDykstra(Router router, Profile profile, RouterPoint[] sources,
            RouterPoint[] targets, float max = float.MaxValue)
            : base(router.Db, profile, profile.DefaultWeightHandler(router), sources, targets, max)
        {

        }
    }
}