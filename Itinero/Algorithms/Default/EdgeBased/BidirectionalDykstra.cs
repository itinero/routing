// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An implementation of the bi-directional dykstra algorithm.
    /// </summary>
    public class BidirectionalDykstra : AlgorithmBase
    {
        private readonly Dykstra _sourceSearch;
        private readonly Dykstra _targetSearch;

        /// <summary>
        /// Creates a new instance of search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra sourceSearch, Dykstra targetSearch)
        {
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
        }

        private Tuple<EdgePath<float>, EdgePath<float>, float> _best = null;
        private float _maxForward = float.MaxValue;
        private float _maxBackward = float.MaxValue;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _best = new Tuple<EdgePath<float>, EdgePath<float>, float>(null, null, float.MaxValue);
            _maxForward = float.MinValue;
            _maxBackward = float.MinValue;
            _sourceSearch.WasEdgeFound = (v1, w1, length, edge) =>
            {
                _maxForward = edge.Weight;
                return false;
            };
            _targetSearch.WasEdgeFound = (v1, w1, length, edge) =>
            {
                _maxBackward = edge.Weight;
                return this.ReachedBackward(v1, w1, length, edge);
            };

            _sourceSearch.Initialize();
            _targetSearch.Initialize();
            var source = true;
            var target = true;
            while (source || target)
            {
                source = false;
                if (_maxForward < _best.Item3)
                { // still a need to search, not best found or max < best.
                    source = _sourceSearch.Step();
                }
                target = false;
                if (_maxBackward < _best.Item3)
                { // still a need to search, not best found or max < best.
                    target = _targetSearch.Step();
                }

                if (this.HasSucceeded)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedBackward(uint vertex1, float weight1, float length, EdgePath<float> edge)
        {
            // check forward search for the same vertex.
            EdgePath<float> forwardVisit;
            if (_sourceSearch.TryGetVisit(-edge.Edge, out forwardVisit))
            { // there is a status for this vertex in the source search.
                var localWeight = edge.Weight - weight1;
                var totalWeight = edge.Weight + forwardVisit.Weight - localWeight;
                if (totalWeight < _best.Item3)
                { // this vertex is a better match.
                    _best = new Tuple<EdgePath<float>, EdgePath<float>, float>(forwardVisit, edge, totalWeight);
                    this.HasSucceeded = true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the source-search algorithm.
        /// </summary>
        public Dykstra SourceSearch
        {
            get
            {
                return _sourceSearch;
            }
        }

        /// <summary>
        /// Returns the target-search algorithm.
        /// </summary>
        public Dykstra TargetSearch
        {
            get
            {
                return _targetSearch;
            }
        }

        /// <summary>
        /// Gets the best edge.
        /// </summary>
        public long BestEdge
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _best.Item1.Edge;
            }
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath(out float weight)
        {
            this.CheckHasRunAndHasSucceeded();

            weight = 0;
            var fromSource = _best.Item1;
            var toTarget = _best.Item2;

            var path = new List<uint>();
            weight = fromSource.Weight + toTarget.Weight;
            fromSource.AddToList(path);
            path.RemoveAt(path.Count - 1);
            if (toTarget.From != null)
            {
                toTarget.From.AddToListReverse(path);
            }
            return path;
        }

        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath()
        {
            float weight;
            return this.GetPath(out weight);
        }
    }
}