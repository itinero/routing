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

using Itinero.Algorithms.Weights;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;

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
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(null, null, _weightHandler.Infinite);
            _maxForward = _weightHandler.Zero;
            _maxBackward = _weightHandler.Zero;
            _sourceSearch.Visit = (path) =>
            {
                if (path.From == null) return false;
                _maxForward = path.From.Weight;
                if (path.From.From == null)
                {
                    this.NeighbourReachedForward(path);
                }
                return false;
            };
            _sourceSearch.VisitNeighbour = (path) =>
            {
                return this.NeighbourReachedForward(path);
            };
            _targetSearch.Visit = (path) =>
            {
                if (path.From == null) return false;
                _maxBackward = path.From.Weight;
                if (path.From.From == null)
                {
                    this.NeighbourReachedBackward(path);
                }
                return false;
            };
            _targetSearch.VisitNeighbour = (path) =>
            {
                return this.NeighbourReachedBackward(path);
            };

            _sourceSearch.Initialize();
            _targetSearch.Initialize();
            while (true)
            {
                var source = false;
                if (_weightHandler.IsSmallerThan(_maxForward, _best.Item3))
                { // still a need to search, not best found or max < best.
                    source = _sourceSearch.Step();
                }
                
                var target = false;
                if (_weightHandler.IsSmallerThan(_maxBackward, _best.Item3))
                { // still a need to search, not best found or max < best.
                    target = _targetSearch.Step();
                }

                if (_weightHandler.IsLargerThanOrEqual(_weightHandler.Add(_maxBackward, _maxForward),
                    _best.Item3))
                { // a better path cannot be found.
                    break;
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
        private bool NeighbourReachedForward(EdgePath<T> forwardVisit)
        {
            // check backward search for the same vertex.
            if (!_targetSearch.TryGetVisit(-forwardVisit.Edge, out var backwardVisit)) return false; 
            
            // there is a status for this edge in the target search.
            var totalWeight = backwardVisit.Weight;
            if (forwardVisit.From != null)
            {
                totalWeight = _weightHandler.Add(forwardVisit.From.Weight, totalWeight);
            }
            
            // calculate the weight, count forward edge only once.
            if (!_weightHandler.IsSmallerThan(totalWeight, _best.Item3)) return false; 
            
            // this is a better match.
            if (forwardVisit.Vertex == backwardVisit.From.Vertex &&
                (forwardVisit.From.From != null || backwardVisit.From.From != null))
            { // paths match and are bigger than one edge.
                // verify if there are restrictions here.
                var targetVertexRestriction = _sourceSearch.GetRestriction(forwardVisit.Vertex);
                if (targetVertexRestriction != null)
                {
                    foreach (var restriction in targetVertexRestriction)
                    {
                        if (restriction != null &&
                            restriction.Length == 1)
                        {
                            // a simple restriction, restricted vertex, cannot join paths here.
                            return false;
                        }
                    }
                }
                
                _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(forwardVisit, backwardVisit, totalWeight);
                this.HasSucceeded = true;
            }
            return false;
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <returns></returns>
        private bool NeighbourReachedBackward(EdgePath<T> backwardVisit)
        {
            // check forward search for the same vertex.
            if (!_sourceSearch.TryGetVisit(-backwardVisit.Edge, out var forwardVisit)) return false;
            
            // there is a status for this edge in the source search.
            var totalWeight = forwardVisit.Weight;
            if (forwardVisit.From != null)
            {
                totalWeight = _weightHandler.Add(backwardVisit.From.Weight, totalWeight);
            }
            if (!_weightHandler.IsSmallerThan(totalWeight, _best.Item3)) return false; 
            
            // this is a better match.
            if (forwardVisit.Vertex == backwardVisit.From.Vertex &&
                (forwardVisit.From.From != null || backwardVisit.From.From != null))
            { // paths match and are bigger than one edge.
                // verify if there are restrictions here.
                var targetVertexRestriction = _sourceSearch.GetRestriction(forwardVisit.Vertex);
                if (targetVertexRestriction != null)
                {
                    foreach (var restriction in targetVertexRestriction)
                    {
                        if (restriction != null &&
                            restriction.Length == 1)
                        {
                            // a simple restriction, restricted vertex, cannot join paths here.
                            return false;
                        }
                    }
                }
                
                _best = new Tuple<EdgePath<T>, EdgePath<T>, T>(forwardVisit, backwardVisit, totalWeight);
                this.HasSucceeded = true;
            }
            return false;
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