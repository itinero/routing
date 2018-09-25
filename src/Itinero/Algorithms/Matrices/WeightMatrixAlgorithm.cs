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

using Itinero.Profiles;
using System.Collections.Generic;
using Itinero.Algorithms.Weights;
using Itinero.Algorithms.Search;
using Itinero.LocalGeo;
using System.Threading;

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// An algorithm to calculate a weight-matrix for a set of locations.
    /// </summary>
    public class WeightMatrixAlgorithm<T> : AlgorithmBase, IWeightMatrixAlgorithm<T>
        where T : struct
    {
        private readonly RouterBase _router;
        private readonly IProfileInstance _profile;
        private readonly WeightHandler<T> _weightHandler;
        private readonly IMassResolvingAlgorithm _massResolver;

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, Coordinate[] locations)
            : this(router, profile, weightHandler, new MassResolvingAlgorithm(
                router, new IProfileInstance[] { profile }, locations))
        {

        }

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler,
            List<RouterPoint> resolvedLocations)
            : this(router, profile, weightHandler, new PresolvedMassResolvingAlgorithm(
                router, new IProfileInstance[] { profile }, resolvedLocations))
        {

        }

        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, WeightHandler<T> weightHandler, IMassResolvingAlgorithm massResolver)
        {
            _router = router;
            _profile = profile;
            _weightHandler = weightHandler;
            _massResolver = massResolver;
        }

        private Dictionary<int, RouterPointError> _errors; // all errors per routerpoint idx.
        private List<int> _correctedIndices; // the original routerpoint per resolved point index.
        private List<RouterPoint> _correctedResolvedPoints; // only the valid resolved points.
        private T[][] _weights; // the weights between all valid resolved points.        
        
        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected sealed override void DoRun(CancellationToken cancellationToken)
        {
            // run mass resolver if needed.
            if (!_massResolver.HasRun)
            {
                _massResolver.Run(cancellationToken);
            }

            // create error and resolved point management data structures.
            _correctedResolvedPoints = _massResolver.RouterPoints;
            _errors = new Dictionary<int, RouterPointError>(_correctedResolvedPoints.Count);
            _correctedIndices = new List<int>(_correctedResolvedPoints.Count);
            for (var i = 0; i < _correctedResolvedPoints.Count; i++)
            {
                _correctedIndices.Add(i);
            }
            
            // calculate matrix.
            var nonNullInvalids = new HashSet<int>();
            _weights = _router.CalculateWeight(_profile, _weightHandler, _correctedResolvedPoints.ToArray(), nonNullInvalids);

            // take into account the non-null invalids now.
            if (nonNullInvalids.Count > 0)
            { // shrink lists and add errors.
                foreach (var invalid in nonNullInvalids)
                {
                    _errors[invalid] = new RouterPointError()
                    {
                        Code = RouterPointErrorCode.NotRoutable,
                        Message = "Location could not routed to or from."
                    };
                }

                _correctedResolvedPoints = _correctedResolvedPoints.ShrinkAndCopyList(nonNullInvalids);
                _correctedIndices = _correctedIndices.ShrinkAndCopyList(nonNullInvalids);
                _weights = _weights.SchrinkAndCopyMatrix(nonNullInvalids);
            }
            this.HasSucceeded = true;
        }
        
        /// <summary>
        /// Gets the router.
        /// </summary>
        public RouterBase Router
        {
            get
            {
                return _router;
            }
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        public IProfileInstance Profile
        {
            get
            {
                return _profile;
            }
        }

        /// <summary>
        /// Gets the mass resolver.
        /// </summary>
        public IMassResolvingAlgorithm MassResolver
        {
            get
            {
                return _massResolver;
            }
        }

        /// <summary>
        /// Gets the weights between all valid router points.
        /// </summary>
        public T[][] Weights
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _weights;
            }
        }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        public List<RouterPoint> RouterPoints
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _correctedResolvedPoints;
            }
        }
        
        /// <summary>
        /// Returns the corrected index, or the index in the weight matrix for the given routerpoint index.
        /// </summary>
        /// <param name="resolvedIdx">The index of the resolved point.</param>
        /// <returns>The index in the weight matrix, -1 if this point is in error.</returns>
        public int CorrectedIndexOf(int resolvedIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _correctedIndices.IndexOf(resolvedIdx);
        }
        
        /// <summary>
        /// Returns the routerpoint index that represents the given weight in the weight matrix.
        /// </summary>
        /// <param name="weightIdx">The index in the weight matrix.</param>
        /// <returns>The routerpoint index, always exists and always returns a proper value.</returns>
        public int OriginalIndexOf(int weightIdx)
        {
            this.CheckHasRunAndHasSucceeded();

            return _correctedIndices[weightIdx];
        }

        /// <summary>
        /// Returns the errors indexed per original routerpoint index.
        /// </summary>
        public Dictionary<int, RouterPointError> Errors
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _errors;
            }
        }
    }

    /// <summary>
    /// An algorithm to calculate a weight-matrix for a set of locations.
    /// </summary>
    public sealed class WeightMatrixAlgorithm : WeightMatrixAlgorithm<float>
    {
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, IMassResolvingAlgorithm massResolver)
            : base(router, profile, profile.DefaultWeightHandler(router), massResolver)
        {

        }
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, Coordinate[] locations)
            : base(router, profile, profile.DefaultWeightHandler(router), locations)
        {

        }
        /// <summary>
        /// Creates a new weight-matrix algorithm.
        /// </summary>
        public WeightMatrixAlgorithm(RouterBase router, IProfileInstance profile, List<RouterPoint> resolvedLocations)
            : base(router, profile, profile.DefaultWeightHandler(router), resolvedLocations)
        {

        }
    }
}