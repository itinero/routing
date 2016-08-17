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

using Itinero.Algorithms.Collections;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Contracted.EdgeBased
{
    /// <summary>
    /// A vertex priority calculator.
    /// </summary>
    public interface IPriorityCalculator
    {
        /// <summary>
        /// Calculate the priority for the given vertex.
        /// </summary>
        float Calculate(BitArray32 contractedFlags, Func<uint, IEnumerable<uint[]>> getRestrictions, uint vertex);

        /// <summary>
        /// Notifies this calculator that the given vertex was contracted.
        /// </summary>
        void NotifyContracted(uint vertex);
    }
}