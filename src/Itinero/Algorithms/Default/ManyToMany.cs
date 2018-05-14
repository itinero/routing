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
using System.Threading;

namespace Itinero.Algorithms.Default
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights/paths.
    /// </summary>
    public class ManyToMany<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly WeightHandler<T> _weightHandler;
        private readonly RouterPoint[] _sources;
        private readonly RouterPoint[] _targets;
        private readonly T _maxSearch;
        
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, WeightHandler<T> weightHandler,
            RouterPoint[] sources, RouterPoint[] targets, T maxSearch)
        {
            _routerDb = routerDb;
            _weightHandler = weightHandler;
            _sources = sources;
            _targets = targets;
            _maxSearch = maxSearch;
        }

        private OneToMany<T>[] _sourceSearches;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            // search sources.
            _sourceSearches = new OneToMany<T>[_sources.Length];
            for (var i = 0; i < _sources.Length; i++)
            {
                _sourceSearches[i] = new OneToMany<T>(_routerDb, _weightHandler, _sources[i], _targets, _maxSearch);
                _sourceSearches[i].Run(cancellationToken);
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets all weights.
        /// </summary>
        public T[][] Weights
        {
            get
            {
                var weights = new T[_sources.Length][];
                for (var s = 0; s < _sources.Length; s++)
                {
                    weights[s] = _sourceSearches[s].Weights;
                }
                return weights;
            }
        }

        /// <summary>
        /// Gets the best weight for the source/target at the given index.
        /// </summary>
        /// <returns></returns>
        public T GetBestWeight(int source, int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var path = _sourceSearches[source].GetPath(target);
            if(path != null)
            {
                return path.Weight;
            }
            return _weightHandler.Infinite;
        }

        /// <summary>
        /// Gets the path from source to target.
        /// </summary>
        public EdgePath<T> GetPath(int source, int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var path = _sourceSearches[source].GetPath(target);
            if (path != null)
            {
                return path;
            }
            return null;
        }

        /// <summary>
        /// Tries to get the path from source to target.
        /// </summary>
        public bool TryGetPath(int source, int target, out EdgePath<T> path)
        {
            this.CheckHasRunAndHasSucceeded();

            return _sourceSearches[source].TryGetPath(target, out path);
        }
    }

    /// <summary>
    /// An algorithm to calculate many-to-many weights/paths.
    /// </summary>
    public sealed class ManyToMany : ManyToMany<float>
    {
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterBase router, Profile profile,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
            : base(router.Db, profile.DefaultWeightHandler(router), sources, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, Func<ushort, Factor> getFactor,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
            : base(routerDb, new DefaultWeightHandler(getFactor), sources, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, DefaultWeightHandler weightHandler,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
            : base(routerDb, weightHandler, sources, targets, maxSearch)
        {

        }
    }
}