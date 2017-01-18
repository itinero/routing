// Itinero - Routing for .NET
// Copyright (C) 2017 Abelshausen Ben
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

using Itinero.Algorithms.Search;
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// Abstract representation of a directed weight matrix algorithm.
    /// </summary>
    public interface IDirectedWeightMatrixAlgorithm<T> : IAlgorithm
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
        /// Gets the router.
        /// </summary>
        RouterBase Router { get; }

        /// <summary>
        /// Gets the valid router points.
        /// </summary>
        List<RouterPoint> RouterPoints { get; }

        /// <summary>
        /// Returns the original index of the routerpoint, given the corrected index.
        /// </summary>
        int OriginalIndexOf(int correctedIdx);

        /// <summary>
        /// Returns the corrected index of the routerpoint, given the original index.
        /// </summary>
        int CorrectedIndexOf(int originalIdx);

        /// <summary>
        /// Gets the target paths.
        /// </summary>
        EdgePath<T>[] SourcePaths { get; }

        /// <summary>
        /// Gets the source paths.
        /// </summary>
        EdgePath<T>[] TargetPaths { get; }

        /// <summary>
        /// Returns the errors indexed per original routerpoint index.
        /// </summary>
        Dictionary<int, RouterPointError> Errors { get; }
    }
}