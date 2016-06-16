// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Weights;
using Itinero.Profiles;
using System;

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
        private readonly float _maxSearch;
        
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, WeightHandler<T> weightHandler,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
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
        protected override void DoRun()
        {
            // search sources.
            _sourceSearches = new OneToMany<T>[_sources.Length];
            for (var i = 0; i < _sources.Length; i++)
            {
                _sourceSearches[i] = new OneToMany<T>(_routerDb, _weightHandler, _sources[i], _targets, _maxSearch);
                _sourceSearches[i].Run();
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
    }

    /// <summary>
    /// An algorithm to calculate many-to-many weights/paths.
    /// </summary>
    public sealed class ManyToMany : ManyToMany<float>
    {
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, Profile profile,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
            : base(routerDb, profile.DefaultWeightHandler(routerDb), sources, targets, maxSearch)
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