// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using Itinero.Profiles;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// Contains extension methods related to weights and weight handlers.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Calculates the weight for the given edge and distance.
        /// </summary>
        public static T Calculate<T>(this WeightHandler<T> handler, ushort edgeProfile, float distance)
            where T : struct
        {
            Factor factor;
            return handler.Calculate(edgeProfile, distance, out factor);
        }

        /// <summary>
        /// Returns true if weigh1 > weight2 according to the weight handler.
        /// </summary>
        public static bool IsLargerThan<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) > handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 > weight2 according to the weight handler.
        /// </summary>
        public static bool IsLargerThanOrEqual<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) >= handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 smaller than weight2 according to the weight handler.
        /// </summary>
        public static bool IsSmallerThan<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) < handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 smaller than weight2 according to the weight handler.
        /// </summary>
        public static bool IsSmallerThanOrEqual<T>(this WeightHandler<T> handler, T weight1, T weight2)
            where T : struct
        {
            return handler.GetMetric(weight1) <= handler.GetMetric(weight2);
        }

        /// <summary>
        /// Returns true if weigh1 smaller than metric according to the weight handler.
        /// </summary>
        public static bool IsSmallerThan<T>(this WeightHandler<T> handler, T weight1, float metric)
            where T : struct
        {
            return handler.GetMetric(weight1) < metric;
        }

        /// <summary>
        /// Returns the default weight handler.
        /// </summary>
        public static DefaultWeightHandler DefaultWeightHandler(this Profile profile, RouterDb db)
        {
            return new Weights.DefaultWeightHandler((e) => profile.Factor(db.EdgeProfiles.Get(e)));
        }
    }
}
