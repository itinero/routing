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
    /// Represents a factor and speed calculated by the dynamic profile.
    /// </summary>
    public struct DynamicFactorAndSpeed
    {
        /// <summary>
        /// Gets or sets the factor.
        /// </summary>
        public float Factor { get; set; }

        /// <summary>
        /// Gets or sets the speed factor.
        /// </summary>
        public float SpeedFactor { get; set; }

        /// <summary>
        /// Gets or sets the can stop bool.
        /// </summary>
        public bool CanStop { get; set; }

        /// <summary>
        /// Gets or sets the constraint values.
        /// </summary>
        public float[] Constraints { get; set; }

        /// <summary>
        /// Gets or sets the direction value.
        /// </summary>
        public ushort Direction { get; set; }
    }
}
