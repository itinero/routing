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

namespace Itinero.Algorithms.Matrices
{
    /// <summary>
    /// Contains extension methods related to the weight matrix algorithms.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Gets the index in the weight matrix, given the orginal location index.
        /// </summary>
        public static int WeightIndex(this IWeightMatrixAlgorithm<float> algorithm, int locationIdx)
        {
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.CorrectedIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Gets the orginal location index for the given corrected routerpoint index.
        /// </summary>
        public static int OriginalLocationIndex(this IWeightMatrixAlgorithm<float> algorithm, int correctedIdx)
        {
            var resolvedIndex = algorithm.OriginalIndexOf(correctedIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.MassResolver.LocationIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Gets the index in the weight matrix, given the orginal location index.
        /// </summary>
        public static int WeightIndex(this IDirectedWeightMatrixAlgorithm<float> algorithm, int locationIdx)
        {
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.CorrectedIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Gets the orginal location index for the given corrected routerpoint index.
        /// </summary>
        public static int OriginalLocationIndex(this IDirectedWeightMatrixAlgorithm<float> algorithm, int correctedIdx)
        {
            var resolvedIndex = algorithm.OriginalIndexOf(correctedIdx);
            if (resolvedIndex != -1)
            {
                return algorithm.MassResolver.LocationIndexOf(resolvedIndex);
            }
            return -1;
        }

        /// <summary>
        /// Returns true if the point at the given original location index is in error.
        /// </summary>
        public static bool IsInError(this IWeightMatrixAlgorithm<float> algorithm, int locationIdx)
        {
            LocationError le;
            RouterPointError rpe;
            return algorithm.TryGetError(locationIdx, out le, out rpe);
        }

        /// <summary>
        /// Tries to get an error for the given original location index.
        /// </summary>
        public static bool TryGetError(this IWeightMatrixAlgorithm<float> algorithm, int locationIdx, out LocationError locationError,
            out RouterPointError routerPointError)
        {
            locationError = null;
            routerPointError = null;
            if (algorithm.MassResolver.Errors.TryGetValue(locationIdx, out locationError))
            {
                return true;
            }
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (algorithm.Errors.TryGetValue(resolvedIndex, out routerPointError))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the point at the given original location index is in error.
        /// </summary>
        public static bool IsInError(this IDirectedWeightMatrixAlgorithm<float> algorithm, int locationIdx)
        {
            LocationError le;
            RouterPointError rpe;
            return algorithm.TryGetError(locationIdx, out le, out rpe);
        }

        /// <summary>
        /// Tries to get an error for the given original location index.
        /// </summary>
        public static bool TryGetError(this IDirectedWeightMatrixAlgorithm<float> algorithm, int locationIdx, out LocationError locationError,
            out RouterPointError routerPointError)
        {
            locationError = null;
            routerPointError = null;
            if (algorithm.MassResolver.Errors.TryGetValue(locationIdx, out locationError))
            {
                return true;
            }
            var resolvedIndex = algorithm.MassResolver.ResolvedIndexOf(locationIdx);
            if (algorithm.Errors.TryGetValue(resolvedIndex, out routerPointError))
            {
                return true;
            }
            return false;
        }
    }
}