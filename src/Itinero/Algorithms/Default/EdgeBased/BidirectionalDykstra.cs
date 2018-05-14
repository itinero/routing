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
                    localWeight = _weightHandler.Subtract(forwardVisit.Weight, forwardVisit.From.Weight);
                }
                var totalWeight = _weightHandler.Subtract(_weightHandler.Add(forwardVisit.Weight, backwardVisit.Weight), localWeight);
                if (_weightHandler.IsSmallerThan(totalWeight, _best.Item3))
                { // this is a better match.
                    if (forwardVisit.Vertex == backwardVisit.From.Vertex &&
                        (forwardVisit.From.From != null || backwardVisit.From.From != null))
                    { // paths match and are bigger than one edge.
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
                    localWeight = _weightHandler.Subtract(backwardVisit.Weight, backwardVisit.From.Weight);
                }
                var totalWeight = _weightHandler.Subtract(_weightHandler.Add(backwardVisit.Weight, forwardVisit.Weight), localWeight);
                if (_weightHandler.IsSmallerThan(totalWeight, _best.Item3))
                { // this is a better match.
                    if (forwardVisit.Vertex == backwardVisit.From.Vertex &&
                        (forwardVisit.From.From != null || backwardVisit.From.From != null))
                    { // paths match and are bigger than one edge.
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