// Itinero - Routing for .NET
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
using System.Collections.Generic;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// An implementation of the bi-directional dykstra algorithm.
    /// </summary>
    public class BidirectionalDykstra<T> : AlgorithmBase
        where T : struct
    {
        private readonly Dykstra<T> _sourceSearch;
        private readonly Dykstra<T> _targetSearch;
        private readonly WeightHandler<T> _weightHandler;

        /// <summary>
        /// Creates a new instance of search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra<T> sourceSearch, Dykstra<T> targetSearch, WeightHandler<T> weightHandler)
        {
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
            _weightHandler = weightHandler;
        }

        private Tuple<EdgePath<T>, EdgePath<T>, T> _best = null;
        private T _maxForward;
        private T _maxBackward;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(null, null, _weightHandler.Infinite);
            _maxForward = _weightHandler.Zero;
            _maxBackward = _weightHandler.Zero;
            _sourceSearch.Visit = (path) =>
            {
                _maxForward = path.Weight;
                return this.ReachedForward(path);
            };
            _targetSearch.Visit = (path) =>
            {
                _maxBackward = path.Weight;
                return this.ReachedBackward(path);
            };

            _sourceSearch.Initialize();
            _targetSearch.Initialize();
            var source = true;
            var target = true;
            while (source || target)
            {
                source = false;
                if (_weightHandler.IsSmallerThan(_maxForward, _best.Item3))
                { // still a need to search, not best found or max < best.
                    source = _sourceSearch.Step();
                }
                target = false;
                if (_weightHandler.IsSmallerThan(_maxBackward, _best.Item3))
                { // still a need to search, not best found or max < best.
                    target = _targetSearch.Step();
                }
                
                if (!source && !target)
                { // both source and target search failed or useless.
                    break;
                }
            }
        }

        /// <summary>
        /// Called when a vertex was reached during a forward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedForward(EdgePath<T> forwardVisit)
        {
            // check backward search for the same vertex.
            EdgePath<T> backwardVisit;
            if (_targetSearch.TryGetVisit(-forwardVisit.Edge, out backwardVisit))
            { // there is a status for this edge in the target search.
                var localWeight = _weightHandler.Zero;
                if (forwardVisit.From != null)
                {
                    localWeight = forwardVisit.From.Weight;
                }
                var totalWeight = _weightHandler.Subtract(_weightHandler.Add(forwardVisit.Weight, backwardVisit.Weight), localWeight);
                if (_weightHandler.IsSmallerThan(totalWeight, _best.Item3))
                { // this is a better match.
                    if (forwardVisit.Vertex == backwardVisit.From.Vertex)
                    { // paths match.
                        _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(forwardVisit, backwardVisit, totalWeight);
                        this.HasSucceeded = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedBackward(EdgePath<T> backwardVisit)
        {
            // check forward search for the same vertex.
            EdgePath<T> forwardVisit;
            if (_sourceSearch.TryGetVisit(-backwardVisit.Edge, out forwardVisit))
            { // there is a status for this edge in the source search.
                var localWeight = _weightHandler.Zero;
                if (backwardVisit.From != null)
                {
                    localWeight = backwardVisit.From.Weight;
                }
                var totalWeight = _weightHandler.Subtract(_weightHandler.Add(backwardVisit.Weight, forwardVisit.Weight), localWeight);
                if (_weightHandler.IsSmallerThan(totalWeight, _best.Item3))
                { // this is a better match.
                    if (forwardVisit.Vertex == backwardVisit.From.Vertex)
                    { // paths match.
                        _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(forwardVisit, backwardVisit, totalWeight);
                        this.HasSucceeded = true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the source-search algorithm.
        /// </summary>
        public Dykstra<T> SourceSearch
        {
            get
            {
                return _sourceSearch;
            }
        }

        /// <summary>
        /// Returns the target-search algorithm.
        /// </summary>
        public Dykstra<T> TargetSearch
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
        public EdgePath<T> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();
            
            var fromSource = _best.Item1;
            var toTarget = _best.Item2;

            return fromSource.Append(toTarget.From, _weightHandler);
        }
    }

    /// <summary>
    /// An implementation of the bi-directional dykstra algorithm.
    /// </summary>
    public sealed class BidirectionalDykstra : BidirectionalDykstra<float>
    {
        /// <summary>
        /// Creates a new instance of the search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra sourceSearch, Dykstra targetSearch, Func<ushort, Factor> getFactor)
            : base(sourceSearch, targetSearch, new DefaultWeightHandler(getFactor))
        {

        }

        /// <summary>
        /// Creates a new instance of the search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra sourceSearch, Dykstra targetSearch, DefaultWeightHandler weightHandler)
            : base(sourceSearch, targetSearch, weightHandler)
        {

        }
    }
}