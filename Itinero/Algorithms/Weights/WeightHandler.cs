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

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// An abstract weight handler class.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class WeightHandler<T>
        where T : struct
    {
        /// <summary>
        /// Adds the weight to the given weight based on the given distance and edge profile.
        /// </summary>
        public abstract T Add(T weight, ushort edgeProfile, float distance, out Factor factor);

        /// <summary>
        /// Calculates the weight for the given edge and returns the factor.
        /// </summary>
        public abstract T Calculate(ushort edgeProfile, float distance, out Factor factor);

        /// <summary>
        /// Adds the two weights.
        /// </summary>
        public abstract T Add(T weight1, T weight2);

        /// <summary>
        /// Subtracts the two weights.
        /// </summary>
        public abstract T Subtract(T weight1, T weight2);

        /// <summary>
        /// Gets the actual metric the algorithm should be using to determine shortest paths.
        /// </summary>
        public abstract float GetMetric(T weight);

        /// <summary>
        /// Returns the weight that represents 'zero'.
        /// </summary>
        /// <returns></returns>
        public abstract T Zero
        {
            get;
        }

        /// <summary>
        /// Returns the weight that represents 'infinite'.
        /// </summary>
        /// <returns></returns>
        public abstract T Infinite
        {
            get;
        }
    }
}