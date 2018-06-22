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
        /// Returns the routerpoint index that represents the given weight in the weight matrix.
        /// </summary>
        /// <param name="weightIdx">The index in the weight matrix.</param>
        /// <returns>The routerpoint index, always exists and always returns a proper value.</returns>
        int OriginalIndexOf(int weightIdx);

        /// <summary>
        /// Returns the corrected index, or the index in the weight matrix for the given routerpoint index.
        /// </summary>
        /// <param name="resolvedIdx">The index of the resolved point.</param>
        /// <returns>The index in the weight matrix, -1 if this point is in error.</returns>
        int CorrectedIndexOf(int resolvedIdx);

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