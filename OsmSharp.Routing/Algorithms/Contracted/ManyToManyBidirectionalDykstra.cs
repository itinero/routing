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

using OsmSharp.Routing.Graphs.Directed;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Contracted
{
    /// <summary>
    /// An algorithm to calculate many-to-many weights based on a contraction hierarchy.
    /// </summary>
    public class ManyToManyBidirectionalDykstra : AlgorithmBase
    {
        private readonly DirectedMetaGraph _graph;
        private readonly IList<IEnumerable<Path>> _sources;
        private readonly IList<IEnumerable<Path>> _targets;
        private readonly Dictionary<uint, Dictionary<int, float>> _buckets;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        public ManyToManyBidirectionalDykstra(DirectedMetaGraph graph, IList<IEnumerable<Path>> sources,
            IList<IEnumerable<Path>> targets)
        {
            _graph = graph;
            _sources = sources;
            _targets = targets;

            _buckets = new Dictionary<uint, Dictionary<int, float>>();
        }

        private float[][] _weights;

        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun()
        {
            // have default weights.
            _weights = new float[_sources.Count][];
            for (var i = 0; i < _sources.Count; i++)
            {
                _weights[i] = new float[_targets.Count];
                for(var j = 0; j < _targets.Count; j++)
                {
                    _weights[i][j] = float.MaxValue;
                }
            }

            // do forward searches into buckets.
            for(var i = 0; i < _sources.Count; i++)
            {
                var forward = new Dykstra(_graph, _sources[i], false);
                forward.WasFound += (vertex, weight) =>
                    {
                        return this.ForwardVertexFound(i, vertex, weight);
                    };
                forward.Run();
            }

            // do backward searches into buckets.
            for (var i = 0; i < _targets.Count; i++)
            {
                var backward = new Dykstra(_graph, _targets[i], true);
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