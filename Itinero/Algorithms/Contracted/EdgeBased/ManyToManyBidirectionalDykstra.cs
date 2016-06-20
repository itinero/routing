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
using Itinero.Data.Contracted;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy.
    /// </summary>
    public class ManyToManyBidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly DirectedDynamicGraph _graph;
        private readonly RouterPoint[] _sources;
        private readonly RouterPoint[] _targets;
        private readonly Dictionary<uint, Dictionary<int, T>> _buckets;
        private readonly WeightHandler<T> _weightHandler;
        
        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(RouterDb routerDb, Profile profile, WeightHandler<T> weightHandler, RouterPoint[] sources,
            RouterPoint[] targets)
        {
            _routerDb = routerDb;
            _weightHandler = weightHandler;
            _sources = sources;
            _targets = targets;

            ContractedDb contractedDb;
            if (!_routerDb.TryGetContracted(profile, out contractedDb))
            {
                throw new NotSupportedException(
                    "Contraction-based many-to-many calculates are not supported in the given router db for the given profile.");
            }
            if (!contractedDb.HasEdgeBasedGraph)
            {
                throw new NotSupportedException(
                    "Contraction-based edge-based many-to-many calculates are not supported in the given router db for the given profile.");
            }
            _graph = contractedDb.EdgeBasedGraph;
            weightHandler.CheckCanUse(contractedDb);
            
            _buckets = new Dictionary<uint, Dictionary<int, T>>();
        }

        private T[][] _weights;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
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
                var forward = new Dykstra<T>(_graph, _weightHandler, _sources[i].ToEdgePaths(_routerDb, _weightHandler, true), false);
                forward.WasFound += (vertex, weight) =>
                    {
                        return this.ForwardVertexFound(i, vertex, weight);
                    };
                forward.Run();
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targets.Length; i++)
            {
                var backward = new Dykstra<T>(_graph, _weightHandler, _targets[i].ToEdgePaths(_routerDb, _weightHandler, false), true);
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
                foreach(var pair in bucket)
                {
                    var existing = _weights[pair.Key][i];
                    var totalWeight = _weightHandler.Add(weight, pair.Value);
                    if (_weightHandler.IsSmallerThan(totalWeight, existing))
                    {
                        _weights[pair.Key][i] = totalWeight;
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
            RouterPoint[] targets)
            : base(router.Db, profile, profile.DefaultWeightHandler(router), sources, targets)
        {

        }

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(RouterDb router, Profile profile, WeightHandler<float> weightHandler, RouterPoint[] sources,
            RouterPoint[] targets)
            : base(router, profile, weightHandler, sources, targets)
        {

        }
    }
}