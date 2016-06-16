// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Profiles;
using System;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// A default weight handler.
    /// </summary>
    public sealed class DefaultWeightHandler : WeightHandler<float>
    {
        private Func<ushort, Factor> _getFactor;

        /// <summary>
        /// Creates a new default weight handler.
        /// </summary>
        public DefaultWeightHandler(Func<ushort, Factor> getFactor)
        {
            _getFactor = getFactor;
        }
        
        /// <summary>
        /// Returns the weight that represents 'zero'.
        /// </summary>
        /// <returns></returns>
        public sealed override float Zero
        {
            get
            {
                return 0;
            }
        }
        
        /// <summary>
        /// Returns the weight that represents 'infinite'.
        /// </summary>
        /// <returns></returns>
        public sealed override float Infinite
        {
            get
            {
                return float.MaxValue;
            }
        }

        /// <summary>
        /// Adds the two weights.
        /// </summary>
        public sealed override float Add(float weight1, float weight2)
        {
            return weight1 + weight2;
        }

        /// <summary>
        /// Subtracts the two weights.
        /// </summary>
        public sealed override float Subtract(float weight1, float weight2)
        {
            return weight1 - weight2;
        }

        /// <summary>
        /// Calculates the weight for the given edge and distance.
        /// </summary>
        public sealed override float Calculate(ushort edgeProfile, float distance, out Factor factor)
        {
            factor = _getFactor(edgeProfile);
            return (distance * factor.Value);
        }

        /// <summary>
        /// Adds weight to given weight based on the given distance and profile.
        /// </summary>
        public sealed override float Add(float weight, ushort edgeProfile, float distance, out Factor factor)
        {
            factor = _getFactor(edgeProfile);
            return weight + (distance * factor.Value);
        }

        /// <summary>
        /// Gets the actual metric the algorithm should be using to determine shortest paths.
        /// </summary>
        public sealed override float GetMetric(float weight)
        {
            return weight;
        }
    }
}