// Itinero - Routing for .NET
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

using Itinero.Algorithms.Weights;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Default
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

        private uint _bestVertex = uint.MaxValue;
        private T _bestWeight;
        private T _maxForward;
        private T _maxBackward;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _bestVertex = uint.MaxValue;
            _bestWeight = _weightHandler.Infinite;
            _maxForward = _weightHandler.Zero;
            _maxBackward = _weightHandler.Zero;
            _sourceSearch.WasFound = (vertex, weight) =>
            {
                _maxForward = weight;
                return false;
            };
            _targetSearch.WasFound = (vertex, weight) =>
            {
                _maxBackward = weight;
                return this.ReachedVertexBackward((uint)vertex, weight);
            };

            _sourceSearch.Initialize();
            _targetSearch.Initialize();
            var source = true;
            var target = true;
            while (source || target)
            {
                source = false;
                if (_weightHandler.IsSmallerThan(_maxForward, _bestWeight))
                { // still a need to search, not best found or max < best.
                    source = _sourceSearch.Step();
                }
                target = false;
                if (_weightHandler.IsSmallerThan(_maxBackward, _bestWeight))
                { // still a need to search, not best found or max < best.
                    target = _targetSearch.Step();
                }

                if(!source && !target)
                { // both source and target search failed or useless.
                    break;
                }
            }
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedVertexBackward(uint vertex, T weight)
        {
            // check forward search for the same vertex.
            EdgePath<T> forwardVisit;
            if (_sourceSearch.TryGetVisit(vertex, out forwardVisit))
            { // there is a status for this vertex in the source search.
                weight = _weightHandler.Add(weight, forwardVisit.Weight);
                if (_weightHandler.IsSmallerThan(weight, _bestWeight))
                { // this vertex is a better match.
                    _bestWeight = weight;
                    _bestVertex = vertex;
                    this.HasSucceeded = true;
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
        /// Gets the best vertex.
        /// </summary>
        public uint BestVertex
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _bestVertex;
            }
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public EdgePath<T> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();
            
            EdgePath<T> fromSource;
            EdgePath<T> toTarget;
            if(_sourceSearch.TryGetVisit(_bestVertex, out fromSource) &&
               _targetSearch.TryGetVisit(_bestVertex, out toTarget))
            {
                return fromSource.Append(toTarget, _weightHandler);
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }

    /// <summary>
    /// An implementation of the bi-directional dykstra algorithm.
    /// </summary>
    public sealed class BidirectionalDykstra : BidirectionalDykstra<float>
    {
        /// <summary>
        /// Creates a new instance of search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra sourceSearch, Dykstra targetSearch, Func<ushort, Factor> getFactor)
            : base(sourceSearch, targetSearch, new DefaultWeightHandler(getFactor))
        {

        }

        /// <summary>
        /// Creates a new instance of search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra sourceSearch, Dykstra targetSearch, DefaultWeightHandler weightHandler)
            : base(sourceSearch, targetSearch, weightHandler)
        {

        }
    }
}