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
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate one-to-many weights/paths.
    /// </summary>
    public class OneToMany<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly RouterPoint _source;
        private readonly IList<RouterPoint> _targets;
        private readonly WeightHandler<T> _weightHandler;
        private readonly T _maxSearch;
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, WeightHandler<T> weightHandler, Func<uint, IEnumerable<uint[]>> getRestrictions,
            RouterPoint source, IList<RouterPoint> targets, T maxSearch)
        {
            _routerDb = routerDb;
            _weightHandler = weightHandler;
            _source = source;
            _targets = targets;
            _maxSearch = maxSearch;
            _getRestrictions = getRestrictions;
        }

        private EdgePath<T>[] _best;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _best = new EdgePath<T>[_targets.Count];

            // register the targets and determine one-edge-paths.
            var sourcePaths = _source.ToEdgePaths(_routerDb, _weightHandler, true);
            var targetIndexesPerEdge = new Dictionary<uint, LinkedTarget>();
            var targetPaths = new IEnumerable<EdgePath<T>>[_targets.Count];
            for (var i = 0; i < _targets.Count; i++)
            {
                var targets = _targets[i].ToEdgePaths(_routerDb, _weightHandler, false);
                targetPaths[i] = targets;

                // determine one-edge-paths.
                if (_source.EdgeId == _targets[i].EdgeId)
                { // on same edge.
                    _best[i] = _source.EdgePathTo(_routerDb, _weightHandler, _targets[i]);
                }

                // register targets.
                for (var t = 0; t < targets.Length; t++)
                {
                    var target = targetIndexesPerEdge.TryGetValueOrDefault(targets[t].Vertex);
                    targetIndexesPerEdge[targets[t].Vertex] = new LinkedTarget()
                    {
                        Target = i,
                        Next = target
                    };
                }
            }

            // determine the best max search radius.
            var max = _weightHandler.Zero;
            for (var s = 0; s < _best.Length; s++)
            {
                if (_best[s] == null)
                {
                    max = _maxSearch;
                }
                else
                {
                    if (_weightHandler.IsLargerThan(_best[s].Weight, max))
                    {
                        max = _best[s].Weight;
                    }
                }
            }

            // run the search.
            var dykstra = new Dykstra<T>(_routerDb.Network.GeometricGraph.Graph, _weightHandler, _getRestrictions,
                sourcePaths, max, false);
            dykstra.Visit += (path) =>
            {
                LinkedTarget target;
                if (targetIndexesPerEdge.TryGetValue(path.Vertex, out target))
                { // there is a target for this vertex.
                    while (target != null)
                    {
                        var best = _best[target.Target];
                        foreach (var targetPath in targetPaths[target.Target])
                        {
                            if (targetPath.Vertex == path.Vertex)
                            { // there is a path here.
                                var fullPath = path.Append(targetPath, _weightHandler);
                                if (best == null ||
                                   _weightHandler.IsSmallerThan(fullPath.Weight, best.Weight))
                                { // not a best path yet, just add this one.
                                    best = fullPath;
                                }
                                break;
                            }
                        }

                        // set again.
                        _best[target.Target] = best;

                        // move to next target.
                        target = target.Next;
                    }
                }
                return false;
            };
            dykstra.Run(cancellationToken);

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the path to the given target.
        /// </summary>
        /// <returns></returns>
        public EdgePath<T> GetPath(int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var best = _best[target];
            if (best != null)
            {
                return best;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        public T[] Weights
        {
            get
            {
                var weights = new T[_best.Length];
                for (var i = 0; i < _best.Length; i++)
                {
                    weights[i] = _weightHandler.Infinite;
                    if (_best[i] != null)
                    {
                        weights[i] = _best[i].Weight;
                    }
                }
                return weights;
            }
        }

        private class LinkedTarget
        {
            public int Target { get; set; }

            public LinkedTarget Next { get; set; }
        }
    }

    /// <summary>
    /// An algorithm to calculate one-to-many weights/paths.
    /// </summary>
    public sealed class OneToMany : OneToMany<float>
    {
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(Router router, Profile profile, Func<uint, IEnumerable<uint[]>> getRestrictions,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : base(router.Db, profile.DefaultWeightHandler(router), getRestrictions, source, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, Func<ushort, Factor> getFactor, Func<uint, IEnumerable<uint[]>> getRestrictions,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : base(routerDb, new DefaultWeightHandler(getFactor), getRestrictions, source, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, DefaultWeightHandler weightHandler, Func<uint, IEnumerable<uint[]>> getRestrictions,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : base(routerDb, weightHandler, getRestrictions, source, targets, maxSearch)
        {

        }
    }
}