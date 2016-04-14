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

using Itinero.Data.Contracted;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy.
    /// </summary>
    public class ManyToManyBidirectionalDykstra : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly DirectedMetaGraph _graph;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly RouterPoint[] _sources;
        private readonly RouterPoint[] _targets;
        private readonly Dictionary<uint, Dictionary<int, float>> _buckets;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(RouterDb routerDb, Profile profile, RouterPoint[] sources,
            RouterPoint[] targets)
            : this(routerDb, profile, (p) => profile.Factor(routerDb.EdgeProfiles.Get(p)), sources, targets)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(RouterDb routerDb, Profile profile, Func<ushort, Factor> getFactor, RouterPoint[] sources,
            RouterPoint[] targets)
        {
            _routerDb = routerDb;
            _getFactor = getFactor;
            _sources = sources;
            _targets = targets;

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

            _buckets = new Dictionary<uint, Dictionary<int, float>>();
        }

        private float[][] _weights;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            // put in default weights and weights for one-edge-paths.
            _weights = new float[_sources.Length][];
            for (var i = 0; i < _sources.Length; i++)
            {
                var source = _sources[i];
                _weights[i] = new float[_targets.Length];
                for (var j = 0; j < _targets.Length; j++)
                {
                    var target = _targets[j];
                    _weights[i][j] = float.MaxValue;

                    if(target.EdgeId == source.EdgeId)
                    {
                        var path = source.PathTo(_routerDb, _getFactor, target);
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
                var forward = new Dykstra(_graph, _sources[i].ToPaths(_routerDb, _getFactor, true), false);
                forward.WasFound += (vertex, weight) =>
                    {
                        return this.ForwardVertexFound(i, vertex, weight);
                    };
                forward.Run();
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targets.Length; i++)
            {
                var backward = new Dykstra(_graph, _targets[i].ToPaths(_routerDb, _getFactor, false), true);
                backward.WasFound += (vertex, weight) =>
                    {
                        return this.BackwardVertexFound(i, vertex, weight);
                    };
                backward.Run();
            }
            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the weights.
        /// </summary>
        public float[][] Weights
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
        private bool ForwardVertexFound(int i, uint vertex, float weight)
        {
            Dictionary<int, float> bucket;
            if(!_buckets.TryGetValue(vertex, out bucket))
            {
                bucket = new Dictionary<int, float>();
                _buckets.Add(vertex, bucket);
                bucket[i] = weight;
            }
            else
            {
                float existing;
                if (bucket.TryGetValue(i, out existing))
                {
                    if(weight < existing)
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
        private bool BackwardVertexFound(int i, uint vertex, float weight)
        {
            Dictionary<int, float> bucket;
            if(_buckets.TryGetValue(vertex, out bucket))
            {
                foreach(var pair in bucket)
                {
                    var existing = _weights[pair.Key][i];
                    if (weight + pair.Value < existing)
                    {
                        _weights[pair.Key][i] = weight + pair.Value;
                    }
                }
            }
            return false;
        }
    }
}