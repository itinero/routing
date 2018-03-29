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
    public class VertexToVertexAlgorithm<T> : AlgorithmBase
        where T : struct
    {
        private readonly DirectedMetaGraph _graph;
        private readonly DykstraSource<T>[] _sources;
        private readonly DykstraSource<T>[] _targets;
        private readonly Dictionary<uint, Dictionary<int, EdgePath<T>>> _buckets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _max;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public VertexToVertexAlgorithm(DirectedMetaGraph graph, WeightHandler<T> weightHandler, DykstraSource<T>[] sources,
            DykstraSource<T>[] targets, T max)
        {
            _graph = graph;
            _sources = sources;
            _targets = targets;
            _weightHandler = weightHandler;
            _max = max;

            _buckets = new Dictionary<uint, Dictionary<int, EdgePath<T>>>();
        }

        private struct Solution
        {
            public EdgePath<T> Path1 { get; set; }

            public EdgePath<T> Path2 { get; set; }

            public EdgePath<T> Path { get; set; }
        }

        private Solution[][] _solutions;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _solutions = new Solution[_sources.Length][];
            for(var s = 0; s < _sources.Length; s++)
            {
                _solutions[s] = new Solution[_targets.Length];
            }

            // do forward searches into buckets.
            for (var i = 0; i < _sources.Length; i++)
            {
                var forward = new Dykstra<T>(_graph, _weightHandler, _sources[i], false, _max);
                forward.WasFound += (p, v, w) =>
                {
                    return this.ForwardVertexFound(forward, i, p, v, w);
                };
                forward.Run(cancellationToken);
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targets.Length; i++)
            {
                var backward = new Dykstra<T>(_graph, _weightHandler, _targets[i], true, _max);
                backward.WasFound += (p, v, w) =>
                {
                    return this.BackwardVertexFound(backward, i, p, v, w);
                };
                backward.Run(cancellationToken);
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        public EdgePath<T> GetPath(int source, int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var solution = _solutions[source][target];
            if (solution.Path == null)
            {
                if (solution.Path1 == null || solution.Path2 == null)
                {
                    return null;
                }
                if (solution.Path != null)
                {
                    return solution.Path;
                }

                var fromSource = solution.Path1;
                var toTarget = solution.Path2;
                var vertices = new List<uint>();

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

                var path = new EdgePath<T>(vertices[0]);
                for (var i = 1; i < vertices.Count; i++)
                {
                    path = new EdgePath<T>(vertices[i], default(T), path);
                }

                solution.Path = path;
            }
            return solution.Path;
        }

        /// <summary>
        /// Called when a forward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool ForwardVertexFound(Dykstra<T> dykstra, int i, uint pointer, uint vertex, T weight)
        {
            Dictionary<int, EdgePath<T>> bucket;
            if (!_buckets.TryGetValue(vertex, out bucket))
            {
                bucket = new Dictionary<int, EdgePath<T>>();
                _buckets.Add(vertex, bucket);
                bucket[i] = dykstra.GetPath(pointer);
            }
            else
            {
                EdgePath<T> existing;
                if (bucket.TryGetValue(i, out existing))
                {
                    if (_weightHandler.IsSmallerThan(weight, existing.Weight))
                    {
                        bucket[i] = dykstra.GetPath(pointer);
                    }
                }
                else
                {
                    bucket[i] = dykstra.GetPath(pointer);
                }
            }
            return false;
        }

        /// <summary>
        /// Called when a backward vertex was found.
        /// </summary>
        /// <returns></returns>
        private bool BackwardVertexFound(Dykstra<T> dykstra, int i, uint pointer, uint vertex, T weight)
        {
            Dictionary<int, EdgePath<T>> bucket;
            if (_buckets.TryGetValue(vertex, out bucket))
            {
                foreach (var pair in bucket)
                {
                    var existing = _solutions[pair.Key][i];
                    var total = _weightHandler.Add(weight, pair.Value.Weight);
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
                        _solutions[pair.Key][i] = new Solution()
                        {
                            Path1 = pair.Value,
                            Path2 = dykstra.GetPath(pointer)
                        };
                    }
                }
            }
            return false;
        }
    }

    // TODO: implement a non-generic non-weighthandler version.
}