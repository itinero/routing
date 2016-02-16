// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Algorithms.Collections;

namespace OsmSharp.Routing.Algorithms.Contracted
{
    /// <summary>
    /// A vertex priority calculator.
    /// </summary>
    public interface IPriorityCalculator
    {
        /// <summary>
        /// Calculate the priority for the given vertex.
        /// </summary>
        float Calculate(BitArray32 contractedFlags, uint vertex);

        /// <summary>
        /// Notifies this calculator that the given vertex was contracted.
        /// </summary>
        void NotifyContracted(uint vertex);
    }
}