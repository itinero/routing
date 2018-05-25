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

        private uint _bestVertex = uint.MaxValue;
        private T _bestWeight;
        private T _maxForward;
        private T _maxBackward;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _bestVertex = uint.MaxValue;
            _bestWeight = _weightHandler.Infinite;
            _maxForward = _weightHandler.Zero;
            _maxBackward = _weightHandler.Zero;
            _sourceSearch.WasFound = (vertex, weight) =>
            {
                _maxForward = weight;
                return this.ReachedVertexForward(vertex, weight);
            };
            _targetSearch.WasFound = (vertex, weight) =>
            {
                _maxBackward = weight;
                return this.ReachedVertexBackward(vertex, weight);
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
        /// Called when a vertex was reached during a forward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedVertexForward(uint vertex, T weight)
        {
            // check backward search for the same vertex.
            EdgePath<T> backwardVisit;
            if (_targetSearch.TryGetVisit(vertex, out backwardVisit))
            { // there is a status for this vertex in the source search.
                weight = _weightHandler.Add(weight, backwardVisit.Weight);
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