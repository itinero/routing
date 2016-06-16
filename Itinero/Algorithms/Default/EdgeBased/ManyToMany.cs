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

using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights/paths.
    /// </summary>
    public class ManyToMany : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly RouterPoint[] _sources;
        private readonly RouterPoint[] _targets;
        private readonly float _maxSearch;
        private readonly Func<uint, IEnumerable<uint[]>> _getRestrictions;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, Profile profile, Func<uint, IEnumerable<uint[]>> getRestrictions,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
            : this(routerDb, (p) => profile.Factor(routerDb.EdgeProfiles.Get(p)), getRestrictions, sources, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(RouterDb routerDb, Func<ushort, Factor> getFactor, Func<uint, IEnumerable<uint[]>> getRestrictions,
            RouterPoint[] sources, RouterPoint[] targets,
            float maxSearch)
        {
            _routerDb = routerDb;
            _getFactor = getFactor;
            _sources = sources;
            _targets = targets;
            _maxSearch = maxSearch;
            _getRestrictions = getRestrictions;
        }

        private OneToMany[] _sourceSearches;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // search sources.
            _sourceSearches = new OneToMany[_sources.Length];
            for (var i = 0; i < _sources.Length; i++)
            {
                _sourceSearches[i] = new OneToMany(_routerDb, _getFactor, _getRestrictions, _sources[i], _targets, _maxSearch);
                _sourceSearches[i].Run();
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets all weights.
        /// </summary>
        public float[][] Weights
        {
            get
            {
                var weights = new float[_sources.Length][];
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
        public float GetBestWeight(int source, int target)
        {
            this.CheckHasRunAndHasSucceeded();

            var path = _sourceSearches[source].GetPath(target);
            if (path != null)
            {
                return path.Weight;
            }
            return float.MaxValue;
        }

        /// <summary>
        /// Gets the path from source to target.
        /// </summary>
        public EdgePath<float> GetPath(int source, int target)
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
}