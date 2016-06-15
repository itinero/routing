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
    /// An algorithm to calculate one-to-many weights/paths.
    /// </summary>
    public class OneToMany : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly RouterPoint _source;
        private readonly IList<RouterPoint> _targets;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly float _maxSearch;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, Profile profile,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
            : this(routerDb, (p) => profile.Factor(routerDb.EdgeProfiles.Get(p)), source, targets, maxSearch)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public OneToMany(RouterDb routerDb, Func<ushort, Factor> getFactor,
            RouterPoint source, IList<RouterPoint> targets, float maxSearch)
        {
            _routerDb = routerDb;
            _getFactor = getFactor;
            _source = source;
            _targets = targets;
            _maxSearch = float.MaxValue;
        }

        private EdgePath[] _best;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _best = new EdgePath[_targets.Count];

            // register the targets and determine one-edge-paths.
            var sourcePaths = _source.ToEdgePaths(_routerDb, _getFactor, true);
            var targetIndexesPerEdge = new Dictionary<uint, LinkedTarget>();
            var targetPaths = new IEnumerable<EdgePath>[_targets.Count];
            for (var i = 0; i < _targets.Count; i++)
            {
                var targets = _targets[i].ToEdgePaths(_routerDb, _getFactor, false);
                targetPaths[i] = targets;

                // determine one-edge-paths.
                if (_source.EdgeId == _targets[i].EdgeId)
                { // on same edge.
                    _best[i] = _source.EdgePathTo(_routerDb, _getFactor, _targets[i]);
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
            var max = 0f;
            for (var s = 0; s < _best.Length; s++)
            {
                if (_best[s] == null)
                {
                    max = _maxSearch;
                }
                else
                {
                    if (_best[s].Weight > max)
                    {
                        max = _best[s].Weight;
                    }
                }
            }

            // run the search.
            var dykstra = new Dykstra(_routerDb.Network.GeometricGraph.Graph, _getFactor, null,
                sourcePaths, max, false);
            dykstra.WasEdgeFound += (v1, w1, distance, path) =>
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
                                var fullPath = path.Append(targetPath);
                                if (best == null ||
                                   fullPath.Weight < best.Weight)
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
            dykstra.Run();

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the path to the given target.
        /// </summary>
        /// <returns></returns>
        public EdgePath GetPath(int target)
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
        public float[] Weights
        {
            get
            {
                var weights = new float[_best.Length];
                for (var i = 0; i < _best.Length; i++)
                {
                    weights[i] = float.MaxValue;
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
}
