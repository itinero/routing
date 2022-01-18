/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Weights;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;

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
        
        private T _bestWeight;
        private T _maxForward;
        private T _maxBackward;

        private EdgePath<T> _bestPathForward;
        private EdgePath<T> _bestPathBackward;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _bestWeight = _weightHandler.Infinite;
            _maxForward = _weightHandler.Zero;
            _maxBackward = _weightHandler.Zero;
            _sourceSearch.Visit = (visit) =>
            {
                if (visit.From != null && visit.From.From == null)
                {
                    this.ReachedNeighbourForward(visit);
                }

                return false;
            };
            _sourceSearch.NeighbourWasFound = this.ReachedNeighbourForward;
            _targetSearch.NeighbourWasFound = this.ReachedNeighbourBackward;
            _targetSearch.Visit = (visit) =>
            {
                if (visit.From != null && visit.From.From == null)
                {
                    this.ReachedNeighbourBackward(visit);
                }

                return false;
            };

            _sourceSearch.Initialize();
            _targetSearch.Initialize();
            while (true)
            {
                var source = false;
                if (_weightHandler.IsSmallerThan(_maxForward, _bestWeight))
                { // still a need to search, not best found or max < best.
                    source = _sourceSearch.Step();
                }
                
                var target = false;
                if (_weightHandler.IsSmallerThan(_maxBackward, _bestWeight))
                { // still a need to search, not best found or max < best.
                    target = _targetSearch.Step();
                }

                if(!source && !target)
                { // both source and target search failed or useless.
                    break;
                }
            
                if (_weightHandler.IsLargerThanOrEqual(_weightHandler.Add(_maxBackward, _maxForward),
                    _bestWeight))
                {
                    break;
                }
            }
        }

        private void ReachedNeighbourForward(EdgePath<T> edge)
        {
            if (edge.From == null) return;
            _maxForward = edge.From.Weight;

            // check backward search.
            if (!_targetSearch.TryGetVisit(edge.Vertex, out var backwardVisit)) return;
            
            // check for u-turns.
            if (edge.Edge == backwardVisit.Edge) return;
            
            // calculate total weight.
            var weight = _weightHandler.Add(edge.Weight, backwardVisit.Weight);
            if (!_weightHandler.IsSmallerThan(weight, _bestWeight)) return; 
            
            // weight is better, this path is better.
            _bestWeight = weight;
            _bestPathForward = edge;
            _bestPathBackward = backwardVisit;
            this.HasSucceeded = true;
        }

        private void ReachedNeighbourBackward(EdgePath<T> edge)
        {
            if (edge.From == null) return;
            _maxBackward = edge.From.Weight;

            // check forward search.
            if (!_sourceSearch.TryGetVisit(edge.Vertex, out var forwardVisit)) return;
            
            // check for u-turns.
            if (edge.Edge == forwardVisit.Edge) return;
            
            // calculate total weight.
            var weight = _weightHandler.Add(edge.Weight, forwardVisit.Weight);
            if (!_weightHandler.IsSmallerThan(weight, _bestWeight)) return; 
            
            // weight is better, this path is better.
            _bestWeight = weight;
            _bestPathForward = forwardVisit;
            _bestPathBackward = edge;
            this.HasSucceeded = true;
        }

        /// <summary>
        /// Returns the source-search algorithm.
        /// </summary>
        public Dykstra<T> SourceSearch => _sourceSearch;

        /// <summary>
        /// Returns the target-search algorithm.
        /// </summary>
        public Dykstra<T> TargetSearch => _targetSearch;

        /// <summary>
        /// Gets the best vertex.
        /// </summary>
        public uint BestVertex
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _bestPathBackward.Vertex;
            }
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public EdgePath<T> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();

            if(_bestPathBackward != null && _bestPathForward != null)
            {
                return _bestPathForward.Append(_bestPathBackward, _weightHandler);
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