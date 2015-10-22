// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Graphs;
using OsmSharp.Routing.Profiles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Default
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights/paths.
    /// </summary>
    public class ManyToMany : AlgorithmBase
    {
        private readonly Graph _graph;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly IList<IEnumerable<Path>> _sources;
        private readonly IList<IEnumerable<Path>> _targets;
        private readonly float _maxSearch;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToMany(Graph graph, Func<ushort, Factor> getFactor, IList<IEnumerable<Path>> sources, IList<IEnumerable<Path>> targets,
            float maxSearch)
        {
            _graph = graph;
            _getFactor = getFactor;
            _sources = sources;
            _targets = targets;
            _maxSearch = maxSearch;
        }

        private OneToMany[] _sourceSearches;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _sourceSearches = new OneToMany[_sources.Count];
            for(var i = 0; i < _sources.Count; i++)
            {
                _sourceSearches[i] = new OneToMany(_graph, _getFactor, _sources[i], _targets, _maxSearch);
                _sourceSearches[i].Run();
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the best weight for the source/target at the given index.
        /// </summary>
        /// <returns></returns>
        public float GetBestWeight(uint source, uint target)
        {
            this.CheckHasRunAndHasSucceeded();

            return _sourceSearches[source].GetBestWeight(target);
        }

        /// <summary>
        /// Gets the best vertex for the source/target at the given index.
        /// </summary>
        /// <returns></returns>
        public uint GetBestVertex(uint source, uint target)
        {
            this.CheckHasRunAndHasSucceeded();

            return _sourceSearches[source].GetBestVertex(target);
        }

        /// <summary>
        /// Gets the path from source to target.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath(uint source, uint target)
        {
            this.CheckHasRunAndHasSucceeded();

            return _sourceSearches[source].GetPath(target);
        }
    }
}