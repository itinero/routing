// Itinero - Routing for .NET
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

namespace Itinero.Profiles
{
    /// <summary>
    /// Represents different profile metrics.
    /// </summary>
    public enum ProfileMetric
    {
        /// <summary>
        /// A profile that uses time in seconds.
        /// </summary>
        /// <remarks>Means that Factor() = 1/Speed().</remarks>
        TimeInSeconds,
        /// <summary>
        /// A profile that uses distance in meters.
        /// </summary>
        /// <remarks>Means that Factor() is constant, Speed() returns the actual speed.</remarks>
        DistanceInMeters,
        /// <summary>
        /// A profile that uses a custom metric.
        /// </summary>
        /// <remarks>Means that Factor() can be anything, Speed() returns the actual speed.</remarks>
        Custom
    }
}