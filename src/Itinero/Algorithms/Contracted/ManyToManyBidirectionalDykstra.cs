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
    public class ManyToManyBidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly DirectedMetaGraph _graph;
        private readonly RouterPoint[] _sources;
        private readonly RouterPoint[] _targets;
        private readonly Dictionary<uint, Dictionary<int, EdgePath<T>>> _buckets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(RouterDb routerDb, Profile profile, WeightHandler<T> weightHandler, RouterPoint[] sources,
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

            _buckets = new Dictionary<uint, Dictionary<int, EdgePath<T>>>();
        }

        private struct Solution
        {
            public EdgePath<T> Path1 { get; set; }

            public EdgePath<T> Path2 { get; set; }

            public EdgePath<T> Path { get; set; }
        }

        private Solution[][] _paths;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // put in default weights and weights for one-edge-paths.
            _paths = new Solution[_sources.Length][];
            for (var i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                _paths[i] = new Solution[_targets.Length];
                for (var j = 0; j < _targets.Length; j++)
                {
                    var target = _targets[j];

                    if (target.EdgeId == source.EdgeId)
                    {
                        var path = source.EdgePathTo(_routerDb, _weightHandler, target);
                        if (path != null)
                        {
                            _paths[i][j] = new Solution()
                            {
                                Path = path
                            };
                        }
                    }
                }
            }

            // do forward searches into buckets.
            for (var i = 0; i < _sources.Length; i++)
            {
                var forward = new Dykstra<T>(_graph, _weightHandler, _sources[i].ToEdgePaths(_routerDb, _weightHandler, true), false, _max);
                forward.WasFound += (path) =>
                {
                    return this.ForwardVertexFound(i, path);
                };
                forward.Run(cancellationToken);
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targets.Length; i++)
            {
                var backward = new Dykstra<T>(_graph, _weightHandler, _targets[i].ToEdgePaths(_routerDb, _weightHandler, false), true, _max);
                backward.WasFound += (path) =>
                {
                    return this.BackwardVertexFound(i, path);
                };
                backward.Run(cancellationToken);
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the paths.
        /// </summary>
        public EdgePath<T> GetPath(int source, int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var solution = _paths[source][target];
            if (solution.Path != null)
            {
                return solution.Path;
            }
            if (solution.Path1 == null ||
                solution.Path2 == null)
            {
                return null;
            }

            var vertices = new List<uint>();
            var fromSource = solution.Path1;
            var toTarget = solution.Path2;

            // add vertices from source.
            vertices.Add(fromSource.Vertex);
            while (fromSource.From != null)
            {
                if (fromSource.From.Vertex != Constants.NO_VERTEX)
                { // this should be the end of the path.
                    if (fromSource.Edge == Constants.NO_EDGE)
                    { // only expand when there is no edge id.
                        _graph.ExpandEdge(fromSource.From.Vertex, fromSource.Vertex, vertices, false, true);
                    }
                }
                vertices.Add(fromSource.From.Vertex);
                fromSource = fromSource.From;
            }
            vertices.Reverse();

            // and add vertices to target.
            while (toTarget.From != null)
            {
                if (toTarget.From.Vertex != Constants.NO_VERTEX)
                { // this should be the end of the path.
                    if (toTarget.Edge == Constants.NO_EDGE)
                    { // only expand when there is no edge id.
                        _graph.ExpandEdge(toTarget.From.Vertex, toTarget.Vertex, vertices, false, false);
                    }
                }
                vertices.Add(toTarget.From.Vertex);
                toTarget = toTarget.From;
            }

            return _routerDb.BuildEdgePath(_weightHandler, _sources[source], _targets[target], vertices);
        }

        /// <summary>
        /// Called when a forward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool ForwardVertexFound(int i, EdgePath<T> path)
        {
            Dictionary<int, EdgePath<T>> bucket;
            if (!_buckets.TryGetValue(path.Vertex, out bucket))
            {
                bucket = new Dictionary<int, EdgePath<T>>();
                _buckets.Add(path.Vertex, bucket);
                bucket[i] = path;
            }
            else
            {
                EdgePath<T> existing;
                if (bucket.TryGetValue(i, out existing))
                {
                    if (_weightHandler.IsSmallerThan(path.Weight, existing.Weight))
                    {
                        bucket[i] = path;
                    }
                }
                else
                {
                    bucket[i] = path;
                }
            }
            return false;
        }

        /// <summary>
        /// Called when a backward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool BackwardVertexFound(int i, EdgePath<T> path)
        {
            Dictionary<int, EdgePath<T>> bucket;
            if (_buckets.TryGetValue(path.Vertex, out bucket))
            {
                foreach (var pair in bucket)
                {
                    var existing = _paths[pair.Key][i];
                    var total = _weightHandler.Add(path.Weight, pair.Value.Weight);
                    var existingWeight = _weightHandler.Infinite;
                    if (existing.Path != null)
                    {
                        existingWeight = existing.Path.Weight;
                    }
                    else if (existing.Path1 != null &&
                        existing.Path2 != null)
                    {
                        existingWeight = _weightHandler.Add(existing.Path1.Weight,
                            existing.Path2.Weight);
                    }
                    if (_weightHandler.IsSmallerThan(total, existingWeight))
                    { // append the backward to the forward path.
                        _paths[pair.Key][i] = new Solution()
                        {
                            Path1 = pair.Value,
                            Path2 = path
                        };
                    }
                }
            }
            return false;
        }
    }

    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy.
    /// </summary>
    public sealed class ManyToManyBidirectionalDykstra : ManyToManyBidirectionalDykstra<float>
    {
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(Router router, Profile profile, RouterPoint[] sources,
            RouterPoint[] targets, float max = float.MaxValue)
            : base(router.Db, profile, profile.DefaultWeightHandler(router), sources, targets, max)
        {

        }
    }
}