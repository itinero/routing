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

using Itinero.Algorithms.Search;
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// Abstract representation of a weight matrix algorithm.
    /// </summary>
    public interface IWeightMatrixAlgorithm<T> : IAlgorithm
    {
        /// <summary>
        /// Gets the mass resolver.
        /// </summary>
        IMassResolvingAlgorithm MassResolver { get; }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        IProfileInstance Profile { get; }

        /// <summary>
        /// Gets the weights between all valid router points.
        /// </summary>
        T[][] Weights { get; }

        /// <summary>
        /// Returns the original location index (after resolving) of the given weight matrix index.
        /// </summary>
        int OriginalIndexOf(int matrixIdx);

        /// <summary>
        /// Returns the corrected index, the index in the weight matrix, given the original location index (after resolving).
        /// </summary>
        int CorrectedIndexOf(int originalIdx);

        /// <summary>
        /// Gets the router.
        /// </summary>
        RouterBase Router { get; }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        List<RouterPoint> RouterPoints { get; }

        /// <summary>
        /// Returns the errors indexed per original routerpoint index.
        /// </summary>
        Dictionary<int, RouterPointError> Errors { get; }
    }
}